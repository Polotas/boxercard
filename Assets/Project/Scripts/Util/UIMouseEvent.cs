using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action<PointerEventData> onMouseEnter;
    public Action<PointerEventData> onMouseExit;

    public void OnPointerEnter(PointerEventData eventData) => onMouseEnter?.Invoke(eventData);
    public void OnPointerExit(PointerEventData eventData) => onMouseExit?.Invoke(eventData);
}
