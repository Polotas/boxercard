using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGameStart : MonoBehaviour
{
    public GameObject objCanvasGame;
    public Button buttonStart;
    public Animator animationUI;
    public Animator animationGamePlayUI;
    
    public BoxerData playerData;
    
    [Header("Boxers")] 
    public PlayerController playerController;
    public AdversaryController adversaryController;
    public CardBoxerController boxerPlayerAnimationUI;
    public CardBoxerController boxerAdversaryAnimationUI;
    private BattleManager _battleManager;
    private BoxerManager _boxerManager;
    private BoxerData _boxerDataAdversary;
    
    private void Awake()
    {
        buttonStart.onClick.AddListener(StartGame);
        _battleManager = FindFirstObjectByType<BattleManager>();
        _boxerManager = FindFirstObjectByType<BoxerManager>(); 
        _battleManager.battleEvents.OnTurnChanged += CallNextTurn;
        
        _boxerDataAdversary = _boxerManager.GetAdversary();
        boxerPlayerAnimationUI.SetupBoxer(playerData,false);
        boxerAdversaryAnimationUI.SetupBoxer(_boxerDataAdversary,false);
    }

    private void StartGame() => StartCoroutine(IE_StartGame());

    private IEnumerator IE_StartGame()
    {
        animationUI.Play("Game_Start_Exit");
        
        yield return new WaitForSeconds(0.6f);
        animationGamePlayUI.Play("GamePlay");
        objCanvasGame.SetActive(true);
        playerData.cards = GameManager.Instance.GetListOfCardsDeck(null);
        playerController.StartGame(playerData);
        adversaryController.StartGame(_boxerDataAdversary);
        
        yield return new WaitForSeconds(2f);

        var uiGamePlay = FindFirstObjectByType<UIGamePlay>();
        if (uiGamePlay != null)
        {
            uiGamePlay.StartBattle();
        }
    }

    private void CallNextTurn(GameTurn gameturn)
    {
        animationUI.Play("NextTurn");
    }
}
