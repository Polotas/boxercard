using System;
using System.Collections;
using OneGear.UI.Utility;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIHome : MonoBehaviour
{
    public Image playerImage;
    public Sprite[] playersVisual;
    public Image adversaryImage;
    public Sprite[] adversaryVisual;

    public Animator animator;
    public Button buttonStart;
    
    private void Awake()
    {
        playerImage.sprite = playersVisual[Random.Range(0, playersVisual.Length)];
        adversaryImage.sprite = adversaryVisual[Random.Range(0, adversaryVisual.Length)];
        buttonStart.onClick.AddListener(Button_Start);
    }

    private void Start()
    {
        UITransition.Instance.CallTransition(TRANSITIONS.FULL_TO_MIDDLE);
    }

    private void Button_Start() =>   UITransition.Instance.BackToLoading("03_Game",TRANSITIONS.MIDDLE_TO_FULL,HideUI,0);

    private void HideUI() => animator.Play("UI_Home_Exit");

}
