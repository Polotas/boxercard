using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour
{
    [Header("UI Popup Warning")]
    public CanvasGroup bgCanvasGroup;
    public Transform objectOptions;
    public TextMeshProUGUI textWarning;
    public Button buttonClose;
    
    [Header("Config")]
    public float fadeDuration = 0.3f;
    public float scaleDuration = 0.3f;
    public Ease scaleEase = Ease.OutBack;
    public Ease fadeEase = Ease.Linear;
    
    private void Awake()
    {
        buttonClose.onClick.AddListener(Close_Popup);
    }
    
    public void Open_PopupWarning(string text)
    {
        bgCanvasGroup.alpha = 0f;
        bgCanvasGroup.blocksRaycasts = true;
        bgCanvasGroup.interactable = true;
        objectOptions.localScale = Vector3.zero;
        textWarning.text = text;
        bgCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase);
        objectOptions.DOScale(Vector3.one, scaleDuration).SetEase(scaleEase);
    }
    
    private void Close_Popup()
    {
        bgCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase);
        
        objectOptions.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                bgCanvasGroup.blocksRaycasts = false;
                bgCanvasGroup.interactable = false;
            });
    }
}
