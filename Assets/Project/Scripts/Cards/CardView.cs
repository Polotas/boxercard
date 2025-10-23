using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI defenseText;
    public GameObject powerObj;
    public GameObject defenseObj;
    public Image visual;
    public Image bg;
    public Transform cardFlip;
    public GameObject backCard;
    public Shadow[] shadows;

    public GameObject attackBG;
    public GameObject defenseBG;
    public GameObject healthBG;
    public GameObject specialBG;
    public Color colorNormal;
    public Color colorDefense;
    public Color colorCorner;
    
    public void SetupCard(CardData data, bool flipCard = true)
    {
        if(backCard != null) backCard.SetActive(true);
        if(visual != null) visual.sprite = data.visual;
        cardName.text = data.displayName.ToUpper();
        powerText.text = data.power.ToString();
        defenseText.text = data.defense.ToString();
        powerObj.SetActive(false);
        defenseObj.SetActive(data.type == CardType.Defense);
        powerObj.SetActive(data.type != CardType.Special);
        SetShadow(new Vector2(-5,-5));
        RectTransform rt = cardName.GetComponent<RectTransform>();
        Vector2 offsetMin = rt.offsetMin; 
        Vector2 offsetMax = rt.offsetMax; 

        switch (data.type)
        {
            case CardType.Health:
                bg.color = colorNormal;
                healthBG.SetActive(true);
                break;
            case CardType.Attack:
                bg.color = colorNormal;
                attackBG.SetActive(true);
                break;
            case CardType.Defense:
                bg.color = colorDefense;
                defenseBG.SetActive(true);
                break;
            case CardType.Special:
                offsetMin.x = 10;
                bg.color = colorNormal;
                specialBG.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        
        if(flipCard) FlipCard();
    }

    public void SetShadow(Vector2 distance)
    {
        foreach (var t in shadows)
            t.effectDistance = distance;
    }
    
    public void FlipCard() => StartCoroutine(IE_FlipCard());
    
    public IEnumerator IE_FlipCard()
    {
        yield return new WaitForSeconds(0.3f);
        cardFlip.DOLocalRotate(new Vector3(0, 90, 0), 0.1f);
        yield return new WaitForSeconds(0.1f);
        backCard.SetActive(false);
        cardFlip.DOLocalRotate(Vector3.zero, 0.1f);
    }
    
    public void UpdateCardVisuals()
    {
        var cardController = GetComponent<CardController>();
        if (cardController != null)
        {
            powerText.text = cardController.power.ToString();
            defenseText.text = cardController.defense.ToString();
            
            powerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            defenseText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }
    }
}