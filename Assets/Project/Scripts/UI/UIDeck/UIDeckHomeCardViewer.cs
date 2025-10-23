using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIDeckHomeCardViewer : MonoBehaviour
{
    public Button buttonAdd;
    public Button buttonRemove;
    public TextMeshProUGUI textQuantity;
    public TextMeshProUGUI textCardMax;
    public TextMeshProUGUI textCardDescription;
    public GameObject objectQuantity;
    public GameObject objectAddRemove;
    public GameObject objectInfo;
    public GameObject objectDescription;
    
    public UIMouseEvent _uiMouseEvent;
    public UIMouseEvent _uiMouseEventInfo;
    public CardView cardView;
    public CardData cardData;
    private int currentQuantity;
    private Vector3 originalScale;
    private Tween hoverScaleTween;

    private void Awake()
    {
        _uiMouseEvent.onMouseEnter += Enter;
        _uiMouseEvent.onMouseExit += Exit;
        
        _uiMouseEventInfo.onMouseEnter += EnterInfo;
        _uiMouseEventInfo.onMouseExit += ExitInfo;
        
        if (cardView != null)
        {
            originalScale = cardView.transform.localScale;
        }
        else
        {
            originalScale = Vector3.one;
        }
    }
    
    public void Setup(CardData data)
    {
        objectQuantity.SetActive(false);
        cardData = data;
        cardView.SetupCard(cardData);
        textCardDescription.text = data.description;
        // Capture a fresh baseline scale after setup in case the prefab sets a custom scale
        if (cardView != null)
        {
            originalScale = cardView.transform.localScale;
        }
        buttonAdd.onClick.AddListener(AddCard);
        buttonRemove.onClick.AddListener(RemoveCard);
    }

    public void UpdateQuantity(int quantity)
    {
        currentQuantity = quantity;
        textQuantity.text = quantity.ToString();
        textCardMax.text = quantity + "/" + cardData.maxCards;
        objectQuantity.SetActive(quantity > 0);
        buttonAdd.interactable = quantity < cardData.maxCards;
        buttonRemove.interactable = quantity > 0;
    }

    private void AddCard()
    {
        GameManager.Instance.AddCardToDeck(cardData.id);
    }
    
    private void RemoveCard()
    {
        GameManager.Instance.RemoveCardFromDeck(cardData.id);
    }

    public void Enter(PointerEventData eventData)
    {
        if (objectQuantity != null) objectQuantity.SetActive(false);
        if (objectAddRemove != null) objectAddRemove.SetActive(false);
        if (objectInfo != null) objectInfo.SetActive(true);
        
        if (cardView != null)
        {
            if (hoverScaleTween != null && hoverScaleTween.IsActive())
            {
                hoverScaleTween.Kill();
            }
            hoverScaleTween = cardView.transform.DOScale(originalScale * 1.2f, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    public void Exit(PointerEventData eventData)
    {
        if (objectQuantity != null) objectQuantity.SetActive(currentQuantity > 0);
        if (objectAddRemove != null) objectAddRemove.SetActive(true);
        if (objectInfo != null) objectInfo.SetActive(false);
        
        if (cardView != null)
        {
            if (hoverScaleTween != null && hoverScaleTween.IsActive())
            {
                hoverScaleTween.Kill();
            }
            hoverScaleTween = cardView.transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuad);
        }
    }
    
    public void EnterInfo(PointerEventData eventData)
    {
        if (objectDescription != null) objectDescription.SetActive(true);
    }

    public void ExitInfo(PointerEventData eventData)
    {
        if (objectDescription != null) objectDescription.SetActive(false);
    }

    private void OnDisable()
    {
        if (hoverScaleTween != null && hoverScaleTween.IsActive())
        {
            hoverScaleTween.Kill();
        }
        if (cardView != null)
        {
            cardView.transform.localScale = originalScale;
        }
    }


}
