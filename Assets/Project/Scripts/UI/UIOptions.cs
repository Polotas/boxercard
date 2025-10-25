using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIOptions : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup bgCanvasGroup;
    public Transform objectOptions;
    
    public Button buttonOpenOptions;
    public Button buttonCloseOptions;

    public Button buttonMuteBGFX;
    public Button buttonMuteFX;
    
    public GameObject visualBGFX;
    public GameObject visualFX;

    [Header("Config")]
    public float fadeDuration = 0.3f;
    public float scaleDuration = 0.3f;
    public Ease scaleEase = Ease.OutBack;
    public Ease fadeEase = Ease.Linear;
    
    private GameManager _gameManager;
    
    private void Awake()
    {
        buttonOpenOptions.onClick.AddListener(OpenOptions);
        buttonCloseOptions.onClick.AddListener(CloseOptions);
        buttonMuteBGFX.onClick.AddListener(MuteBGFX);
        buttonMuteFX.onClick.AddListener(MuteFX);
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        var optionsData = _gameManager.playerData.optionsData;
        visualBGFX.SetActive(optionsData.soundBGFX);
        visualFX.SetActive(optionsData.soundFX);
    }

    private void OpenOptions()
    {
        bgCanvasGroup.alpha = 0f;
        bgCanvasGroup.blocksRaycasts = true;
        bgCanvasGroup.interactable = true;
        objectOptions.localScale = Vector3.zero;
        
        bgCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase);
        objectOptions.DOScale(Vector3.one, scaleDuration).SetEase(scaleEase);
    }

    private void CloseOptions()
    {
        bgCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase);
        
        objectOptions.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                bgCanvasGroup.blocksRaycasts = false;
                bgCanvasGroup.interactable = false;
            });
    }

    private void MuteBGFX()
    {
        var optionsData = _gameManager.playerData.optionsData;
        optionsData.soundBGFX = !optionsData.soundBGFX;
        
        _gameManager.Save();
        AudioManager.SetMute(AudioManager.AudioType.BG,optionsData.soundBGFX);
        visualBGFX.SetActive(optionsData.soundBGFX);
    }

    private void MuteFX()
    {
        var optionsData = _gameManager.playerData.optionsData;
        optionsData.soundFX = !optionsData.soundFX;
        
        _gameManager.Save();
        AudioManager.SetMute(AudioManager.AudioType.BG,optionsData.soundFX);
        visualFX.SetActive(optionsData.soundFX);
    }
}
