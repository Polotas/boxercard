using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    public BoxerData boxerData;
    public bool player = false;
    public int maxHealth = 30;
    public int health = 100;

    public BattleManager battleManager;
    public CardBoxerController cardBoxerController;
    public DeckCards deckCards;
    public List<string> cards;
    public List<CardData> currentCards;
    public List<GameObject> cardsObj;

    public bool canDoDamage = true;
    
    private CardsManager _cardsManager;

    private void Awake()
    {
        _cardsManager = FindFirstObjectByType<CardsManager>();
        battleManager = FindFirstObjectByType<BattleManager>();
        battleManager.battleEvents.OnTurnChanged += ChangeTurn;
    }

    public void SetupBoxer(BoxerData data)
    {
        boxerData = data;
        maxHealth = data.health;
        health = data.health;
        cardBoxerController.SetupBoxer(boxerData,true);
        StartCoroutine(IE_StarchMatch());
    }

    private IEnumerator IE_StarchMatch() 
    {
        currentCards = _cardsManager.GetCards(cards);

        for (int i = 0; i < 5; i++)
        {
            SpawnCard();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void GetExtrasCards(int quantity) => StartCoroutine(IE_GetExtraCards(quantity));

    private IEnumerator IE_GetExtraCards(int quantity) 
    {
        for (int i = 0; i < quantity; i++)
        {
            yield return new WaitForSeconds(0.2f);
            SpawnCard();
        }
    }

    public void SpawnCard()
    {
        if (currentCards.Count > 0)
        {
            deckCards.SpawnCard(currentCards[0],player,player);
            currentCards.RemoveAt(0);
        }
        else
        {
            Debug.LogWarning($"{name} não tem mais cartas no deck para spawnar");
        }
    }
    
    public void RemoveCardFromDeck(CardData cardToRemove)
    {
        for (int i = 0; i < currentCards.Count; i++)
        {
            if (currentCards[i].id == cardToRemove.id)
            {
                currentCards.RemoveAt(i);
                cardsObj[2].SetActive(currentCards.Count >= 15);
                cardsObj[1].SetActive(currentCards.Count >= 10);
                cardsObj[0].SetActive(currentCards.Count > 0);
                return;
            }
        }
        
        Debug.LogWarning($"Carta {cardToRemove.displayName} não encontrada no deck de {name}");
    }

    private void ChangeTurn(GameTurn turn)
    {
        canDoDamage = true;
    }
}
