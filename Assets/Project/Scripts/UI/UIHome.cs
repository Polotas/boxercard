using OneGear.UI.Utility;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIHome : MonoBehaviour
{
    [Header("Scripts")] 
    public UIDecksHome uiDecksHome;
    public UISelectBoxer uiSelectBoxer;
    
    [Header("View")]
    public Image playerImage;
    public Sprite[] playersVisual;
    public Image adversaryImage;
    public Sprite[] adversaryVisual;

    [Header("Canvas")]
    public Canvas canvasHome;
    public Canvas canvasDeck;
    public Canvas canvasBoxers;
    
    [Header("Animator")]
    public Animator animator;
    public Animator animatorDeck;
    public Animator animatorBoxers;
    
    [Header("Buttons")]
    public Button buttonArcade;
    public Button buttonCampaing;
    public Button buttonDeck;
    public Button buttonBackDeck;
    public Button buttonBackBoxer;
    
    private void Awake()
    {
        playerImage.sprite = playersVisual[Random.Range(0, playersVisual.Length)];
        adversaryImage.sprite = adversaryVisual[Random.Range(0, adversaryVisual.Length)];
        buttonArcade.onClick.AddListener(Button_Arcade);
        buttonCampaing.onClick.AddListener(Button_Campaing);
        buttonDeck.onClick.AddListener(Button_Deck);
        buttonBackDeck.onClick.AddListener(Button_BackDeck);
        buttonBackBoxer.onClick.AddListener(Button_BackBoxer);
    }

    private void Start()
    {
        UITransition.Instance.CallTransition(TRANSITIONS.FULL_TO_MIDDLE);
    }

    public void Button_Start() =>   UITransition.Instance.BackToLoading("03_Game",TRANSITIONS.MIDDLE_TO_FULL,HideUI,0);

    private void Button_Deck()
    {
        animator.Play("UI_Home_Exit");
        animatorDeck.Play("UI_Deck_Enter");
        canvasHome.sortingOrder = 3;
        canvasDeck.sortingOrder = 2;
        uiDecksHome.uIDeckSelector.OnSelect("boxer01");
        UITransition.Instance.CallTransition(TRANSITIONS.MIDDLE_TO_NULL);
    }
    
    private void Button_BackDeck()
    {
        animator.Play("UI_Home_Enter");
        animatorDeck.Play("UI_Deck_Exit");
        canvasHome.sortingOrder = 2;
        canvasDeck.sortingOrder = 3;
        UITransition.Instance.CallTransition(TRANSITIONS.NULL_TO_MIDLE);
    }
    
    private void Button_Arcade()
    {
        UI_Boxer();
    }
    
    private void Button_Campaing()
    {
        UI_Boxer();
    }
    
    private void UI_Boxer()
    {
        animator.Play("UI_Home_Exit");
        animatorBoxers.Play("UI_Deck_Enter");
        canvasHome.sortingOrder = 3;
        canvasBoxers.sortingOrder = 2;
        //uiDecksHome.uIDeckSelector.OnSelect("boxer01");
        
        var boxerId = GameManager.Instance.playerData.currentBoxer;
        uiSelectBoxer.UpdateSelect(boxerId);
        UITransition.Instance.CallTransition(TRANSITIONS.MIDDLE_TO_NULL);
    }
    
    private void Button_BackBoxer()
    {
        animator.Play("UI_Home_Enter");
        animatorBoxers.Play("UI_Deck_Exit");
        canvasHome.sortingOrder = 2;
        canvasBoxers.sortingOrder = 3;
        UITransition.Instance.CallTransition(TRANSITIONS.NULL_TO_MIDLE);
    }
    
    private void HideUI() => animator.Play("UI_Home_Exit");

}
