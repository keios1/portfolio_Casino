using UnityEngine;

public class ChipHoverOutline : MonoBehaviour
{
    private Renderer[] renderers;
    private float current;
    private float target;

    [SerializeField] private float fadeSpeed = 12f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            r.material.SetFloat("_Hover", 0f);
        }
    }

    void Update()
    {
        current = Mathf.MoveTowards(current, target, Time.deltaTime * fadeSpeed);

        foreach (var r in renderers)
        {
            r.material.SetFloat("_Hover", current);
        }
    }

    void OnMouseEnter()
    {
        target = 1f;
    }

    void OnMouseExit()
    {
        target = 0f;
    }
}
