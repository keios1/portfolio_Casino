using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageScreenEffect : MonoBehaviour
{
    public static DamageScreenEffect Instance;

    [Header("UI 연결")]
    [SerializeField] private Image damageImage;
    [SerializeField] private Image healImage;
    [SerializeField] private Image shieldImage;

    [Header("Damage 설정")]
    [SerializeField] private float damageMaxAlpha = 1f;
    [SerializeField] private float damageFadeSpeed = 3f;

    [Header("Heal 설정")]
    [SerializeField] private float healMaxAlpha = 1f;
    [SerializeField] private float healFadeSpeed = 3f;

    [Header("Shield 설정")]
    [SerializeField] private float shieldAlpha = 0.8f;

    [Tooltip("0번은 쉴드 유지 이미지, 1~5번은 깨지는 애니메이션 이미지")]
    [SerializeField] private Sprite[] shieldSprites;

    [SerializeField] private float shieldFrameTime = 0.1f;

    private Coroutine damageCoroutine;
    private Coroutine healCoroutine;
    private Coroutine shieldCoroutine;
    private bool isPlayingShieldBreak = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        SetAlpha(damageImage, 0f);
        SetAlpha(healImage, 0f);
        SetAlpha(shieldImage, 0f);
    }

    public void FlashDamage()
    {
        if (damageImage == null) return;

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        damageCoroutine = StartCoroutine(FlashRoutine(damageImage, damageMaxAlpha, damageFadeSpeed));
    }

    public void FlashHeal()
    {
        if (healImage == null) return;

        if (healCoroutine != null)
            StopCoroutine(healCoroutine);

        healCoroutine = StartCoroutine(FlashRoutine(healImage, healMaxAlpha, healFadeSpeed));
    }

    public void ShowShield()
    {
        if (shieldImage == null) return;

        if (shieldCoroutine != null)
            StopCoroutine(shieldCoroutine);

        if (shieldSprites != null && shieldSprites.Length > 0)
            shieldImage.sprite = shieldSprites[0];

        SetAlpha(shieldImage, shieldAlpha);

        if (TopBarUIManager.Instance != null)
            TopBarUIManager.Instance.SetShieldFillMode(true);
    }

    public void HideShield()
    {
        if (isPlayingShieldBreak)
            return;

        if (shieldCoroutine != null)
            StopCoroutine(shieldCoroutine);

        SetAlpha(shieldImage, 0f);

        if (TopBarUIManager.Instance != null)
            TopBarUIManager.Instance.SetShieldFillMode(false);
    }

    public void PlayShieldBreak()
    {
        if (shieldImage == null) return;
        PlayShieldBreakSound();

        if (shieldCoroutine != null)
            StopCoroutine(shieldCoroutine);

        shieldCoroutine = StartCoroutine(ShieldBreakRoutine());
    }

    private IEnumerator ShieldBreakRoutine()
    {
        isPlayingShieldBreak = true;

        SetAlpha(shieldImage, shieldAlpha);

        if (shieldSprites != null && shieldSprites.Length > 0)
        {
            for (int i = 0; i < shieldSprites.Length; i++)
            {
                shieldImage.sprite = shieldSprites[i];
                yield return new WaitForSeconds(shieldFrameTime);
            }
        }

        SetAlpha(shieldImage, 0f);

        if (TopBarUIManager.Instance != null)
            TopBarUIManager.Instance.SetShieldFillMode(false);

        shieldCoroutine = null;
        isPlayingShieldBreak = false;
    }
    private void PlayShieldBreakSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;
        if (AudioManager.Instance.skillSounds.shieldBreak == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.shieldBreak
        );
    }
    private IEnumerator FlashRoutine(Image image, float maxAlpha, float fadeSpeed)
    {
        Color c = image.color;

        while (c.a < maxAlpha)
        {
            c.a += Time.deltaTime * fadeSpeed * 5f;
            image.color = c;
            yield return null;
        }

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            image.color = c;
            yield return null;
        }

        SetAlpha(image, 0f);
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
