using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider))]
public class MouseHover : MonoBehaviour
{
    private Material mat;
    private float currentHover;
    private float targetHover;

    public bool IsHovering => targetHover > 0f;
    public bool IsHoverReady
    {
        set
        {
            if (value == false) // 이제 선택 안해요
                targetHover = 0f;

            _IsHoverReady = value;
        }
        get
        {
            return _IsHoverReady;
        }
    }
    private bool _IsHoverReady = false;

    public Action OnHoverClick = () => { };



    [SerializeField] private float fadeSpeed = 12f;

    private void Awake()
    {
        mat = GetComponent<SpriteRenderer>().material;
        mat.SetFloat("_Hover", 0f);
    }

    private void Update()
    {
        currentHover = Mathf.MoveTowards(
            currentHover,
            targetHover,
            Time.deltaTime * fadeSpeed
        );

        mat.SetFloat("_Hover", currentHover);
    }
    private void OnMouseOver()
    {
        if (IsHoverReady)
        {
            targetHover = 1f;
        }
    }

    private void OnMouseEnter()
    {
        if (IsHoverReady)
        {
            targetHover = 1f;
        }
    }

    private void OnMouseExit()
    {
        targetHover = 0f;
    }

    private void OnDisable()
    {
        if (mat != null)
            mat.SetFloat("_Hover", 0f);
    }

    private void OnMouseDown()
    {
        OnHoverClick();
    }
}
