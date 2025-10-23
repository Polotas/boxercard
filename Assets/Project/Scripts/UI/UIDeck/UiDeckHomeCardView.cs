using UnityEngine;
using UnityEngine.EventSystems;

public class UiDeckHomeCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardView miniCardView;
    
    public void Setup(CardData data)
    {
        miniCardView.SetupCard(data,false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    private void SetHeight(float height)
    {

    }
}