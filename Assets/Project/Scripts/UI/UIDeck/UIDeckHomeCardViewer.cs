using System;
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

    private GameManager _gameManager;
    private UIPopup _uiPopup;
    
    private void Awake()
    {
        _uiMouseEvent.onMouseEnter += Enter;
        _uiMouseEvent.onMouseExit += Exit;
        
        _uiMouseEventInfo.onMouseEnter += EnterInfo;
        _uiMouseEventInfo.onMouseExit += ExitInfo;

        _uiPopup = FindFirstObjectByType<UIPopup>();
        
        if (cardView != null)
        {
            originalScale = cardView.transform.localScale;
        }
        else
        {
            originalScale = Vector3.one;
        }
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
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
        if(_gameManager.CanAddOrRemoveCard())
            _gameManager.AddCardToDeck(cardData.id);
        else
            _uiPopup.Open_PopupWarning("You’ve reached the card limit.");
    }
    
    private void RemoveCard()
    {
        if(_gameManager.CanAddOrRemoveCard())
            _gameManager.RemoveCardFromDeck(cardData.id);
        else
            _uiPopup.Open_PopupWarning("You’re at the minimum card limit");
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
