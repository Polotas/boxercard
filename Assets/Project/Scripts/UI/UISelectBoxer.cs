using System;
using System.Collections.Generic;
using OneGear.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBoxer : MonoBehaviour
{
    public UIHome uiHome;
    public List<UIBoxerSelector> boxerSelectors;
    public Button buttonStart;
    public Animator animator;
    
    public Color colorSelect;
    public Color colorDeselect;
    
    private void Awake()
    {
        for (int i = 0; i < boxerSelectors.Count; i++)
        {
            boxerSelectors[i].Setup();
            int index = i; // importante para capturar o valor corretamente
            boxerSelectors[i].button.onClick.AddListener(() => ButtonSelect(index));
        }
        
        buttonStart.onClick.AddListener(Button_Start);
    }
    
    private void ButtonSelect(int valor)
    {
        var boxerId = boxerSelectors[valor].boxerId;
        GameManager.Instance.playerData.currentBoxer = boxerId;
        UpdateSelect(boxerId);
    }

    public void UpdateSelect(string boxerId)
    {
        foreach (var t in boxerSelectors)
        {
            t.UpdateSelect(boxerId,colorSelect,colorDeselect);
        }
    }

    private void Button_Start() 
    {
        animator.Play("UI_Deck_Exit");
        UITransition.Instance.BackToLoading("03_Game",TRANSITIONS.MIDDLE_TO_FULL,null,0);
    }
}

[Serializable]
public class UIBoxerSelector
{
    public Button button;
    public Image[] outline;
    public Image bgBoxer;
    public Animator animator;
    public TextMeshProUGUI textName;
    public string boxerName;
    public string boxerId;

    public void Setup()
    {
        textName.text = boxerName;
    }
    
    public void UpdateSelect(string id, Color colorSelect, Color colorDeselect)
    {
        if (id == boxerId)
        {
            foreach (var t in outline)
            {
                t.color = Color.yellow;
            }

            bgBoxer.color = colorSelect;
            animator.Play("Select");
        }
        else
        {
            foreach (var t in outline)
            {
                t.color = Color.black;
            }

            bgBoxer.color = colorDeselect;
            animator.Play("Idle");
        }
    }
}
