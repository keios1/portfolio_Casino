using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public AudioClip clickClip;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager.Instance가 없습니다.");
            return;
        }

        if (clickClip == null)
        {
            Debug.LogWarning($"{gameObject.name} 버튼의 clickClip이 비어 있습니다.");
            return;
        }

        AudioManager.Instance.PlayUI(clickClip);
    }
}
