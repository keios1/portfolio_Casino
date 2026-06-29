using JetBrains.Annotations;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public abstract class EnemyBase : MonoBehaviour, IBattleUnit
{
    public enum EAnimationRequestArgument
    {
        none,
        idle,
        ready,
        attack,
        hit,
        hitParry,
        died,
        heal,
        convert,
        MAX,
    }

    public class AddHPEventArgs : EventArgs
    {
        private bool _isInitialized = false; public bool IsInitialized => _isInitialized;
        public bool isUsed = false;
        public int amount; // 회복할때 양은 그때그때 변함.
        public Vector3 enemyWorldPosition;

        public void Initialize(Vector3 enemyWorldPosition)
        {
            this._isInitialized = true;
            this.enemyWorldPosition = enemyWorldPosition;
            this.isUsed = false;
        }

    }



    [Header("데이터")]
    public EnemyData data;

    [Header("공통 체력 UI 연결")]
    public Image hpFillImage;
    public TextMeshProUGUI hpText;

    [Header("애니메이션 스프라이트")]
    public SpriteRenderer spriteRenderer; 
    public Sprite idleSprite;      // Idle
    public Sprite readySprite;     // 공격 대기 자세
    public Sprite attackSprite;    // 데미지를 줄 때
    public Sprite hitSprite;       // 공격받았을 때
    public Sprite deadSprite;      // 죽었을 때
    protected Sprite currentBaseSprite; // 그로기 상태 유지
    public int CurrentHP => currentHP; //카오스 카드 케이스4번유무

    [Header("애니메이션 이펙트")]
    public GameObject hitEffectPrefab; // 피격 이펙트 프리팹
    public GameObject parryEffectPrefab; // 패링 이펙트 프리팹

    [Header("연관 게임오브젝트-UI")]
    public MouseHover mouseTarget;
    public bool isTargetable { get; protected set; } // 타겟 가능 여부


    protected int currentHP;
    protected int maxHP;
    protected Player targetPlayer;
    private Coroutine actionAnimCoroutine; // 잠깐 포즈를 취하고 돌아오기 위한 코루틴

    public Animator animatorOrNull; // 에니메이션을 위한 것 , 없으면 null로 둬도 됨

    public event Action hitEvent = () => { }; // 해당 에너미가 피격되었을때, 다른 객체가 실행해야 하는 코루틴(메서드 내에서 StartCoroutine을 호출하시오!)
    public event Action attackEvent = () => { }; // 해당 에너미가 공격을 실행했을때, 다른 객체가 실행해야 하는 코루틴(메서드 내에서 StartCoroutine을 호출하시오!)
    public event Action dieEvent = () => { }; // 해당 에너미가 사망했을 때, 다른 객체가 실행해야 하는 코루틴(메서드 내에서 StartCoroutine을 호출하시오!)

    public event Action onAttackedEvent;

    protected virtual bool UseSlotDeadSound => false;

    [SerializeField] protected int currentShield = 0;
    public int CurrentShield => currentShield;

    protected virtual void Awake()
    {
        if (data != null)
        {
            currentHP = data.maxHP;
            maxHP = data.maxHP;

            UpdateHPUI();

   
        }

        currentBaseSprite = idleSprite;
        ChangeSprite(idleSprite);

        if (mouseTarget == null)
        {
            isTargetable = false;
        }
        else
        {
            mouseTarget.GetComponent<MouseHover>().OnHoverClick +=
                               () =>
                               {
                                   CardManager.instance.selectedEnemyDynamic = this;
                                   CardManager.instance.selectedEnemyButtonClicked = true;
                               };

            mouseTarget.GetComponent<MouseHover>().IsHoverReady = false;// 초기에는 선택 버튼 비활성화
            isTargetable = true;
        }

        
    }

    public void SetPlayerTarget(Player player) { targetPlayer = player; }

    // Q:이 함수의 존재 이유 => 같은 스테이지에 다수의 적 에너미가 존재할 수 있습니다. 이때, 사망한 에너미가 무엇인지를 판별할 필요가 있습니다. 에너미 선택 혹은 AI 호출에서 사용됩니다.
    // Q:virtual이 존재하는 이유 => 보스 몬스터는 체력이 0 이하 일때 발악 패턴을 사용합니다. 그럼에도 무력화가 성공했는지 여부를 판정해야 할때, 오버라이드를 해서 판정할 것입니다.
    public virtual bool IsAlive()
    {
        return currentHP > 0;
    }

    // 공격 모션
    protected void ApplyDamageToPlayer(int damage, bool useBaseAnim = true)
    {
        if (damage <= 0) return;

        Debug.Log($"[Enemy -> Player] {damage} 데미지 공격");

        if (useBaseAnim)
        {
            if (animatorOrNull != null)
            {
                animatorOrNull.Play("Attack");
            }
            else
            {
                Debug.Log($"<?> {gameObject.name}_EnemyBase : ApplyDamageToPlayer(int damage, bool useBaseAnim = true)에서 에니메이터를 실행하지 않았습니다.");

                //PlayActionAnimation(attackSprite, 0.5f);
            }
        }

        if (targetPlayer != null) targetPlayer.TakeDamage(damage);
        else Debug.LogError($">> {gameObject.name} 에서 : 타겟 플레이어가 설정되지 않았습니다");
    }

    public abstract IEnumerator ExecuteEnemyTurn();

    // TakeDamage(int damage)와 로직은 동일
    // 변화된 점은 실행 이펙트(패링)
    // - 인터페이스가 해당 매개변수 변경을 금지하여, 함수 재사용이 어렵게 된 이유로 새로 추가됨.
    public virtual void TakeParryDamage(int damage)
    {
        if (hitEffectPrefab == null)
        {
            Debug.Log($">> {gameObject.name}_EnemyBase : TakeParryDamage(int damage)에서 parryEffectPrefab이 연결되지 않았습니다.");
        }
        TakeDamageCommon(damage, parryEffectPrefab);
    }

    public virtual void TakeDamage(int damage)
    {
        if (hitEffectPrefab == null)
        {
            Debug.Log($">> {gameObject.name}_EnemyBase : TakeDamage(int damage)에서 hitEffectPrefab이 연결되지 않았습니다.");
        }
        TakeDamageCommon(damage, hitEffectPrefab);
    }

    public virtual void AddShield(int amount)
    {
        if (amount <= 0) return;

        currentShield += amount;
        Debug.Log($"[Enemy] {data.enemyName} Shield +{amount}, Current Shield: {currentShield}");

        if (DamageTextManager.Instance != null)
        {
            Vector3 basePos = transform.position;

            if (spriteRenderer != null)
                basePos = spriteRenderer.bounds.center;

            DamageTextManager.Instance.ShowDamage(amount, basePos + Vector3.up * 1.5f, Color.cyan);
        }
    }

    public virtual void ClearShield()
    {
        if (currentShield <= 0) return;

        Debug.Log($"[Enemy] {data.enemyName} Shield Clear: {currentShield} -> 0");
        currentShield = 0;
    }


    public virtual void AddHP(AddHPEventArgs args)
    {
        currentHP += args.amount;

        if (currentHP > data.maxHP) currentHP = data.maxHP;

        UpdateHPUI();

        Debug.Assert(DamageTextManager.Instance != null, "<!> : 이 씬에 DamageTextManager 클래스를 담은 게임오브젝트가 없어요");
        // Debug.Assert(spriteRenderer != null, $"<!> : {gameObject.name}에서 EnemyBase : 이 게임오브젝트에서 spriteRenderer가 없어요");
        DamageTextManager.Instance.ShowDamage(args.amount, args.enemyWorldPosition + Vector3.up * 1.5f, Color.green);

        args.isUsed = true;
    }
    protected void UpdateHPUI()
    {
        Debug.Log($">> {gameObject.name}의 체력 변경 UpdateHPUI()");


        if (data == null) return;

        if (hpFillImage != null)
        {
            Debug.Log($">> {gameObject.name}의 체력 변경 UpdateHPUI() -> 안에 바꿈");

            float ratio = data.maxHP > 0 ? (float)currentHP / data.maxHP : 0f;
            hpFillImage.fillAmount = ratio;
        }
        if (hpText != null && data != null) hpText.text = $"{currentHP} / {data.maxHP}";
    }

    // 죽었을 때 사망 모션
    protected virtual void Die()
    {
        Debug.Log($"{data.enemyName} 처치됨");

        if (UseSlotDeadSound &&
        AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null &&
        AudioManager.Instance.enemySounds.slotDead != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.slotDead);
        }

        dieEvent();

        if (animatorOrNull != null)
        {
            animatorOrNull.SetBool("IsAlive", false);
            animatorOrNull.Play("Dead");
        }
        else
        {
            ChangeSprite(deadSprite);
        }

        //IEnumerator _dieRoutine()
        //{
        //    yield return new WaitForSeconds(1.5f);
        //    gameObject.SetActive(false);
        //}
        //StartCoroutine(_dieRoutine());

        Destroy(gameObject,1.5f);
    }

    // 단순히 스프라이트를 교체하는 함수
    protected void ChangeSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
            spriteRenderer.sprite = newSprite;
    }

    public virtual void ChangeAnimation(EAnimationRequestArgument animArg)
    {
        if (animatorOrNull == null)
        {
            Debug.LogWarning($"<?> {gameObject.name}_EnemyBase : ChangeAnimation(EAnimationRequestArgument animArg)에서 에니메이터가 없습니다.");
            return;
        }

        switch (animArg)
        {
            case EAnimationRequestArgument.idle:
                animatorOrNull.Play("Idle");
                break;
            case EAnimationRequestArgument.ready:
                animatorOrNull.Play("Ready");
                break;
            case EAnimationRequestArgument.attack:
                animatorOrNull.Play("Attack");
                break;
            case EAnimationRequestArgument.hit:
                animatorOrNull.Play("Hit");
                break;
            case EAnimationRequestArgument.hitParry:
                animatorOrNull.Play("HitParry");
                break;
            case EAnimationRequestArgument.died:
                animatorOrNull.Play("Dead");
                break;
            default:
                Debug.LogWarning($"<?> {gameObject.name}_EnemyBase : ChangeAnimation(EAnimationRequestArgument animArg)에서 정의되지 않은 애니메이션 요청 {animArg}");
                break;
        }
    }

    protected void ForceChangeSprite(Sprite newSprite)
    {
        if (actionAnimCoroutine != null)
        {
            StopCoroutine(actionAnimCoroutine);
            actionAnimCoroutine = null;
        }

        currentBaseSprite = newSprite;
        ChangeSprite(newSprite);
    }

    private IEnumerator ActionAnimationRoutine(Sprite actionSprite, float duration)
    {
        ChangeSprite(actionSprite);
        yield return new WaitForSeconds(duration);

        // 무조건 idleSprite로 가는 게 아니라, 기억해 둔 currentBaseSprite로 돌아감
        if (currentHP > 0) ChangeSprite(currentBaseSprite);
    }

    // 공격, 피격 처럼 잠깐 포즈를 취했다가 다시 Idle 돌아가는 함수
    protected void PlayActionAnimation(Sprite actionSprite, float duration)
    {
        if (actionAnimCoroutine != null) StopCoroutine(actionAnimCoroutine);
        actionAnimCoroutine = StartCoroutine(ActionAnimationRoutine(actionSprite, duration));
    }

    private void TakeDamageCommon(int damageValue, GameObject effectPrefab)
    {
        if (currentHP <= 0) return;
        if (damageValue <= 0) return;
        hitEvent(); // 피격 이벤트 실행(ex) 피격시 다른 게임오브젝트 애니메이션)


        if (effectPrefab == null)
        {
            Debug.Log($">> {gameObject.name}_EnemyBase : TakeDamageCommon(int damageValue, GameObject effectPrefab)에서 이펙트 프리펩을 찾을 수 없습니다..");
        }
        else
        {
            Instantiate(effectPrefab, transform.position + new Vector3(0, 0, -1), Quaternion.identity);
        }

        int blockedDamage = Mathf.Min(currentShield, damageValue);
        currentShield -= blockedDamage;

        int hpDamage = damageValue - blockedDamage;

        if (hpDamage <= 0)
        {
            Debug.Log($">> EnemyBase : {data.enemyName} 쉴드로 모든 피해 방어. Remain Shield: {currentShield}");

            if (DamageTextManager.Instance != null)
            {
                Vector3 basePos = transform.position;

                if (spriteRenderer != null)
                    basePos = spriteRenderer.bounds.center;

                DamageTextManager.Instance.ShowDamage(0, basePos + Vector3.up * 1f, Color.cyan);
            }

            return;
        }


        currentHP -= damageValue;
        if (currentHP < 0) currentHP = 0;

        onAttackedEvent?.Invoke();

        Debug.Log($">> EnemyBase : {data.enemyName} HP: {currentHP}/{data.maxHP}");
        UpdateHPUI();

        if (AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null &&
        AudioManager.Instance.enemySounds.enemyHit != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.enemyHit);
        }

        if (DamageTextManager.Instance != null)
        {
            Vector3 basePos = transform.position;

            if (spriteRenderer != null)
                basePos = spriteRenderer.bounds.center;

            DamageTextManager.Instance.ShowDamage(damageValue, basePos + Vector3.up * 1f, Color.gray);
        }
        if (currentHP > 0)
        {
            if (animatorOrNull != null)
            {
                Debug.Assert(gameObject.activeInHierarchy == true);
                Debug.Assert(animatorOrNull.gameObject.activeInHierarchy == true, $"<!> : {animatorOrNull.gameObject.name} 게임오브젝트가 죽어있네요.");

                animatorOrNull.Play("Hit");
            }
            else
            {
                Debug.Log($"<?> {gameObject.name}_EnemyBase : TakeDamage(int damage)에서 에니메이터를 실행하지 않았습니다.");

                PlayActionAnimation(hitSprite, 0.4f);
            }
        }
        else
        {
            Die();
        }
    }
}
