using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IButtonAvailable : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
{
    public bool IsButtonBlocked { get; set; }
    //private bool isButtonBlocked = false;

    public void OnMouseClick(PointerEventData eventData) { }
    public void OnMouseDown(PointerEventData eventData) { }
    public void OnMouseUp(PointerEventData eventData) { }
    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsButtonBlocked)
            return;
        OnMouseClick(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsButtonBlocked)
            return;
        OnMouseDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsButtonBlocked)
            return;
        OnMouseUp(eventData);
    }
}
