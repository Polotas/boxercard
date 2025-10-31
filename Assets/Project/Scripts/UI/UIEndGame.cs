using System.Collections;
using DG.Tweening;
using OneGear.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class UIEndGame : MonoBehaviour
{
    public GameObject endGameUI;
    public GameObject gamePlayUI;
    public Image visualPlayer;
    public TextMeshProUGUI textHeadLine;
    public TextMeshProUGUI textTotalDamage;
    public TextMeshProUGUI textTotalCards;
    public Button buttonNext;

    public void Start()
    {
        buttonNext.onClick.AddListener(Button_Next);
    }

    public void EndGame(bool isWin) => StartCoroutine(IE_EndGame(isWin));

    private IEnumerator IE_EndGame(bool isWin) 
    {
        yield return new WaitForSeconds(1);
        UITransition.Instance.CallTransition(TRANSITIONS.NULL_TO_FULL);   
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
        
        DOTween.To(() => 0, x => { startDamage = x; textTotalDamage.text = startDamage.ToString(); }, damage, 3f).SetEase(Ease.Linear);
        DOTween.To(() => 0, x => { startCardUse = x; textTotalCards.text = startCardUse.ToString(); }, cardUse, 3f).SetEase(Ease.Linear);
    }
    
    private void Button_Next() =>   UITransition.Instance.BackToLoading("03_Home",TRANSITIONS.NULL_TO_FULL,null,0);
}
