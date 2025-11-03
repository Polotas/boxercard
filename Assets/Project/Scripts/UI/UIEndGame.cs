using System.Collections;
using DG.Tweening;
using OneGear.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class UIEndGame : MonoBehaviour
{
    public Canvas canvas;
    public GameObject endGameUI;
    public GameObject gamePlayUI;
    public GameObject objVictory;
    public GameObject objKO;
    public Image visualPlayer;
    public TextMeshProUGUI textHeadLine;
    public TextMeshProUGUI textTotalDamage;
    public TextMeshProUGUI textTotalCards;
    public Button buttonNext;

    private PlayerDecksManager _playerDecksManager;

    public void Start()
    {
        _playerDecksManager = FindFirstObjectByType<PlayerDecksManager>();
        buttonNext.onClick.AddListener(Button_Next);
    }

    public void EndGame(bool isWin) => StartCoroutine(IE_EndGame(isWin));

    private IEnumerator IE_EndGame(bool isWin)
    {
        var boxerId = GameManager.Instance.playerData.currentBoxer;
        visualPlayer.sprite = isWin ? _playerDecksManager.GetVictoryVisual(boxerId) : _playerDecksManager.GetLoseVisual(boxerId);
        canvas.sortingOrder = 1;
        yield return new WaitForSeconds(1);
        UITransition.Instance.CallTransition(TRANSITIONS.NULL_TO_MIDLE);
        yield return new WaitForSeconds(.3f);
        objVictory.SetActive(isWin);
        objKO.SetActive(!isWin);
        yield return new WaitForSeconds(3);
        objVictory.SetActive(false);
        objKO.SetActive(false);
        canvas.sortingOrder = 0;
        UITransition.Instance.CallTransition(TRANSITIONS.MIDDLE_TO_FULL);   
        yield return new WaitForSeconds(1f);
        UITransition.Instance.CallTransition(TRANSITIONS.FULL_TO_NULL);  
        endGameUI.SetActive(true);
        gamePlayUI.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        textHeadLine.text = isWin ? "VICTORY BY KNOCKOUT" : "LOST BY KNOCKOUT";
        var damage = GameManager.Instance.currentDamageAdversary;
        var cardUse = GameManager.Instance.currentCardUse;
        
        
        var startDamage = 0;
        var startCardUse = 0;
        
        DOTween.To(() => 0, x => { startDamage = x; textTotalDamage.text = startDamage.ToString(); }, damage, 1f).SetEase(Ease.Linear);
        DOTween.To(() => 0, x => { startCardUse = x; textTotalCards.text = startCardUse.ToString(); }, cardUse, 1f).SetEase(Ease.Linear);
    }
    
    private void Button_Next() =>   UITransition.Instance.BackToLoading("03_Home",TRANSITIONS.NULL_TO_FULL,null,0);
}
