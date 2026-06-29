using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBlocker : MonoBehaviour
{
    public List<Button> buttonComponents;
    public List<IButtonAvailable> buttonAvailableComponents;
    public List<CanvasGroup> canvasGroups;

    public void BlockButton()
    {
        if (buttonComponents != null)
        {
            foreach (var button in buttonComponents)
            {
                button.interactable = false;
            }
        }
        if (buttonAvailableComponents != null)
        {
            foreach (var buttonAvailable in buttonAvailableComponents)
            {
                buttonAvailable.IsButtonBlocked = true;
            }
        }
        if (canvasGroups != null)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void ReleaseButton()
    {
        if (buttonComponents != null)
        {
            foreach (var button in buttonComponents)
            {
                button.interactable = true;
            }
        }
        if (buttonAvailableComponents != null)
        {
            foreach (var buttonAvailable in buttonAvailableComponents)
            {
                buttonAvailable.IsButtonBlocked = false;
            }
        }
        if (canvasGroups != null)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    // Start is called before the first frame update
    void Start() // 아마 여기서 스테틱에 컴포넌트 넣습니다.
    {
        
    }
}
