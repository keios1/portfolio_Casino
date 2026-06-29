using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private void Apply()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        Rect rect = new Rect(0f, 0f, 1f, 1f);

        if (currentAspect > targetAspect)
        {
            float width = targetAspect / currentAspect;
            rect.width = width;
            rect.x = (1f - width) * 0.5f;
        }
        else
        {
            float height = currentAspect / targetAspect;
            rect.height = height;
            rect.y = (1f - height) * 0.5f;
        }

        cam.rect = rect;
    }
}
