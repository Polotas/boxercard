using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movimento")]
    [SerializeField] private float returnDuration = 0.3f; // Tempo para voltar ao original
    [SerializeField] private float followSpeed = 12f; // Velocidade de seguimento do cursor (mais suave)
    [SerializeField] private bool useSmoothMovement = true; // Usar movimento suave
    [SerializeField] private float movementThreshold = 2f; // Threshold para detectar parada de movimento
    [SerializeField] private float rotationReturnSpeed = 3f; // Velocidade de retorno da rotação

    [Header("Elevação Visual (Estilo Balatro)")]
    [SerializeField] private float scaleAnimationDuration = 0.15f;
    [SerializeField] private Vector2 shadowOffset = new Vector2(3f, -3f);
    
    [Header("Configurações do Shadow")]
    [SerializeField] private float shadowAlpha = 0.3f; // Transparência do shadow
    [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.3f); // Cor do shadow
    [SerializeField] private float shadowFadeInDuration = 0.1f; // Tempo para aparecer
    [SerializeField] private float shadowFadeOutDuration = 0.15f; // Tempo para desaparecer

    [Header("Rotação Dinâmica")]
    [SerializeField] private float rotationIntensity = 12f; // Reduzido para mais suavidade
    [SerializeField] private float maxRotationAngle = 20f; // Reduzido para movimentos mais sutis
    [SerializeField] private bool useInstantRotation = true; // Nova opção para rotação imediata
    [SerializeField] private float mousePositionRotationMultiplier = 0.015f; // Reduzido para mais suavidade

    private CardController _cardController;
    private RectTransform rectTransform;
    public Canvas canvas;
    private CanvasGroup canvasGroup;
    private GraphicRaycaster graphicRaycaster;
    public FanLayout fanLayout;

    // Estados originais
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color originalColor;
    private int originalSortingOrder;

    // Estados de drag
    private bool isDragging;
    private float currentRotation;
    private Vector2 targetPosition;
    private Vector2 currentVelocity;
    private Vector2 mouseOffset; // Offset do mouse em relação ao centro da carta
    private Vector2 lastMousePosition; // Para detectar movimento (em coordenadas de tela)
    private Vector2 currentMousePosition; // Posição atual do mouse
    private float timeSinceLastMovement; // Tempo desde o último movimento
    private bool isMoving; // Se está se movendo atualmente

    public UIDropZone currentUiDropZone;
    public GameObject shadowClone;
    public CanvasGroup shadowCanvasGroup;
    public Transform originalParent;
    public int originalSiblingIndex;

    public bool onDeck;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        var gos = GameObject.FindGameObjectsWithTag("CanvasGameplay"); 
        if(gos.Length < 1) return;
        
        canvas = gos[0].GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        _cardController = GetComponent<CardController>();
        
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        
        currentRotation = 0f;
        targetPosition = originalPosition;
    }

    private void Update()
    {
        if (!isDragging) return;
        
        if (useSmoothMovement)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            Vector2 newPos = Vector2.SmoothDamp(currentPos, targetPosition, ref currentVelocity, 1f / followSpeed);
            rectTransform.anchoredPosition = newPos;
        }
            
        float mouseDistance = Vector2.Distance(currentMousePosition, lastMousePosition);
            
        if (mouseDistance > movementThreshold)
        {
            isMoving = true;
            timeSinceLastMovement = 0f;
            lastMousePosition = currentMousePosition;
        }
        else
        {
            timeSinceLastMovement += Time.deltaTime;
                
            if (timeSinceLastMovement > 0.1f)
                isMoving = false;
        }
        
        if (isMoving) return;
        float targetRotationReturn = 0f;
        currentRotation = Mathf.Lerp(currentRotation, targetRotationReturn, rotationReturnSpeed * Time.deltaTime);
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || currentUiDropZone != null) return; 
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        if(onDeck) rectTransform.SetAsLastSibling();
        rectTransform.DOScale(originalScale * 1.15f, 0.15f).SetEase(Ease.OutQuad);
        _cardController.cardView.SetShadow(new Vector2(-15,-15));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        rectTransform.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuad);
        if(onDeck) rectTransform.SetSiblingIndex(originalSiblingIndex);
        _cardController.cardView.SetShadow(Vector2.zero);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!DragStatus.canDrag) return;
        isDragging = true;
        currentRotation = 0f; 
        isMoving = true; 
        timeSinceLastMovement = 0f;

        rectTransform.transform.localScale = originalScale;
        
        originalParent = rectTransform.parent;

        if(onDeck) fanLayout.RemoveCard(rectTransform);

        CreateShadow();
        
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        
        rectTransform.SetParent(canvas.transform, true);
        rectTransform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        
        currentMousePosition = eventData.position;
        lastMousePosition = eventData.position;
        
        Vector2 mouseLocalPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out mouseLocalPosition);
        
        _cardController.cardView.SetShadow(new Vector2(-15,-15));
        mouseOffset = mouseLocalPosition;
        rectTransform.DOKill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!DragStatus.canDrag) return;
        currentMousePosition = eventData.position;
        
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out mousePosition);
        
        Vector2 cardTargetPosition = mousePosition - mouseOffset;
        
        if (useSmoothMovement)
        {
            targetPosition = cardTargetPosition;
        }
        else
        {
            Vector2 delta = eventData.delta / canvas.scaleFactor;
            rectTransform.anchoredPosition += delta;
        }
        
        if (isMoving)
        {
            Vector2 cardCenter = rectTransform.anchoredPosition;
            Vector2 mouseRelativeToCenter = mousePosition - cardCenter;
            
            if (mouseRelativeToCenter.magnitude > 5f) 
            {
                float targetRotation = Mathf.Clamp(-mouseRelativeToCenter.x * mousePositionRotationMultiplier * rotationIntensity, -maxRotationAngle, maxRotationAngle);
                currentRotation = Mathf.Lerp(currentRotation, targetRotation, useInstantRotation ? 0.6f : 0.2f);
                rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!DragStatus.canDrag) return;
        DestroyShadow();
        isDragging = false;
        bool droppedInZone = CheckDropZones(eventData.position);
        
        if (!droppedInZone)
        {
            ReturnToOriginalPosition();
        }
        else
        {
            _cardController.cardView.SetShadow(Vector2.zero);
            originalPosition = rectTransform.anchoredPosition;
        }
        
        canvasGroup.blocksRaycasts = true;
        RemoveDragVisualEffects();
    }
    
    private void RemoveDragVisualEffects() => rectTransform.DOScale(originalScale, scaleAnimationDuration).SetEase(Ease.OutQuad);

    private void CreateShadow()
    {
        shadowClone = Instantiate(gameObject,originalParent);
        var rectTransformShadowClone = shadowClone.GetComponent<RectTransform>();
        shadowClone.name = gameObject.name + "_Shadow";
        shadowClone.transform.localScale = Vector3.one;
        
        if(onDeck) fanLayout.AddCard(rectTransformShadowClone,originalSiblingIndex);
        
        Debug.Log("CREATE SHADOW: " + shadowClone.name);
        
        UIDragHandler shadowDrag = shadowClone.GetComponent<UIDragHandler>();
        if (shadowDrag != null) DestroyImmediate(shadowDrag);
        
        shadowCanvasGroup = shadowClone.GetComponent<CanvasGroup>();
        
        if (shadowCanvasGroup == null)
            shadowCanvasGroup = shadowClone.AddComponent<CanvasGroup>();
        
        shadowCanvasGroup.alpha = 0f; // Começar invisível
        shadowCanvasGroup.blocksRaycasts = false; // Não bloquear cliques
        shadowCanvasGroup.interactable = false; // Não interativo

        RectTransform shadowRect = shadowClone.GetComponent<RectTransform>();
        shadowRect.anchoredPosition = originalPosition + shadowOffset;
        shadowRect.localRotation = originalRotation; // Manter rotação original
        
        shadowCanvasGroup.DOFade(shadowAlpha, shadowFadeInDuration).SetEase(Ease.OutQuad);
    }

    private void DestroyShadow()
    {
        if (shadowClone != null && shadowCanvasGroup != null)
        {
            shadowCanvasGroup.DOFade(0f, shadowFadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    if (shadowClone != null)
                    {
                        DestroyImmediate(shadowClone);
                        shadowClone = null;
                        shadowCanvasGroup = null;
                    }
                });
            
            var rectTransformShadowClone = shadowClone.GetComponent<RectTransform>();
            fanLayout.RemoveCard(rectTransformShadowClone);
        }
        else if (shadowClone != null)
        {
            DestroyImmediate(shadowClone);
            shadowClone = null;
            shadowCanvasGroup = null;
        }
    }

    private bool CheckDropZones(Vector2 screenPosition)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        return DragStatus.CheckDropZones(screenPosition, gameObject, _cardController, results,pointerEventData);
    }

    private void ReturnToOriginalPosition()
    {
        if (onDeck)
        {
            fanLayout.AddCard(rectTransform,originalSiblingIndex);
            _cardController.cardView.SetShadow(new Vector2(-5,-5));
        }
        else
        {
            rectTransform.DOAnchorPos(originalPosition, returnDuration).SetEase(Ease.OutBack);
            rectTransform.DOLocalRotateQuaternion(originalRotation, returnDuration).SetEase(Ease.OutQuad);
        }
        
        currentRotation = 0f;
    }
    
    public void SetShadowAlpha(float alpha)
    {
        shadowAlpha = Mathf.Clamp01(alpha);
        
        if (shadowCanvasGroup != null)
                shadowCanvasGroup.alpha = shadowAlpha;
    }
    
    public void SetShadowColor(Color color)
    {
        shadowColor = color;
        if (shadowClone == null) return;
        
        Image shadowImage = shadowClone.GetComponent<Image>();
        
        if (shadowImage != null)
            shadowImage.color = shadowColor;
    }
    
    public void SetShadowOffset(Vector2 offset)
    {
        shadowOffset = offset;
        if (shadowClone == null) return;
        
        RectTransform shadowRect = shadowClone.GetComponent<RectTransform>();
        
        if (shadowRect != null)
            shadowRect.anchoredPosition = originalPosition + shadowOffset;
    }
}
