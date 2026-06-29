using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardShowManager : MonoBehaviour
{
    public static CardShowManager instance;
    public GameObject cardBackObject;

    public List<Button> CardButton = new List<Button>();
    public Func<int, IEnumerator> onCardClicked;
    [SerializeField] private GameObject takeCardBackObject;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator CardWaitShow(
    Func<IEnumerator> innerPrevFunction,
    Func<List<GameObject>, IEnumerator> innerAfterFunction,
    List<GameObject> cardList,
    float term)
    {
        yield return innerPrevFunction();

        if (takeCardBackObject != null)
            takeCardBackObject.SetActive(true);

        List<GameObject> spawnedList = new List<GameObject>();

        for (int i = 0; i < cardList.Count; ++i)
        {
            GameObject go = Instantiate(cardList[i], Vector3.zero, Quaternion.identity);
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            RectTransform srcRt = cardList[i].GetComponent<RectTransform>();

            if (rt != null && srcRt != null)
            {
                go.transform.localPosition = new Vector3(
                    (rt.sizeDelta.x + term) * i
                    - ((srcRt.sizeDelta.x + term) / 2.0f) * ((float)cardList.Count - 1f),
                    0,
                    0
                );
            }

            spawnedList.Add(go);
        }

        yield return innerAfterFunction(spawnedList);

        for (int i = 0; i < spawnedList.Count; i++)
        {
            if (spawnedList[i] != null)
                Destroy(spawnedList[i]);
        }

        spawnedList.Clear();

        if (takeCardBackObject != null)
            takeCardBackObject.SetActive(false);
    }

    public IEnumerator CardClickShow(
        int count,
        Func<IEnumerator> prevAction,
        Func<int, IEnumerator> afterAction)
    {
        yield return CardClickShow(count, prevAction, afterAction, cardBackObject, 0f);
    }

    public IEnumerator CardClickShow(
        int count,
        Func<IEnumerator> prevAction,
        Func<int, IEnumerator> afterAction,
        GameObject defaultCard,
        float term)
    {
        CardButton.Clear();

        for (int i = 0; i < count; ++i)
        {
            int index = i;

            GameObject go = Instantiate(defaultCard, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            RectTransform srcRt = defaultCard.GetComponent<RectTransform>();

            if (rt != null && srcRt != null)
            {
                go.transform.localPosition = new Vector3(
                    (rt.sizeDelta.x + term) * index
                    - ((srcRt.sizeDelta.x + term) / 2.0f) * ((float)count - 1f),
                    0,
                    0
                );
            }
            CardShowHasClicked clicked = go.GetComponent<CardShowHasClicked>();
            clicked.hasClicked = false;
            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                CardButton.Add(btn);
                btn.onClick.AddListener(() =>
                {
                    if (clicked.hasClicked)
                    {
                        return;
                    }

                    clicked.hasClicked = true;
                    StartCoroutine(HandleCardClick(index, afterAction));
                });
            }
        }

        yield return prevAction();
        onCardClicked = afterAction;
    }

    private IEnumerator HandleCardClick(int index, Func<int, IEnumerator> afterAction)
    {
        yield return StartCoroutine(afterAction(index));

        foreach (Button b in CardButton)
        {
            if (b != null)
                Destroy(b.gameObject);
        }

        CardButton.Clear();
    }
    public void SetTakeCardBack(bool active)
    {
        if (takeCardBackObject != null)
            takeCardBackObject.SetActive(active);
    }
}
