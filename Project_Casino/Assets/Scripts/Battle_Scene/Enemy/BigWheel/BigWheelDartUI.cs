using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWheelDartUI : MonoBehaviour
{
    public GameObject[] dartImages;

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        int index = UnityEngine.Random.Range(0, 3);
        dartImages[index].SetActive(true);
    }

    public void Hide()
    {
        foreach (GameObject dart in dartImages)
        {
            dart.SetActive(false);
        }
    }
}
