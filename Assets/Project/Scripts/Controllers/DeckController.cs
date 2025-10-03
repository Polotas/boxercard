using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckController : MonoBehaviour
{
    public BoxerData boxerData;
    public bool player = false;
    public int maxHealth = 30;
    public int health = 100;

    public CardBoxerController cardBoxerController;
    public DeckCards deckCards;
    public List<string> cards;
    public List<CardData> currentCards;
    
    private CardsManager _cardsManager;

    private void Awake()
    {
        _cardsManager = FindFirstObjectByType<CardsManager>();
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
        // Remove a primeira ocorrência da carta do deck
        for (int i = 0; i < currentCards.Count; i++)
        {
            if (currentCards[i].name == cardToRemove.name)
            {
                currentCards.RemoveAt(i);
                Debug.Log($"Carta {cardToRemove.displayName} removida do deck de {name}. Cartas restantes: {currentCards.Count}");
                return;
            }
        }
        Debug.LogWarning($"Carta {cardToRemove.displayName} não encontrada no deck de {name}");
    }
    
    public int GetRemainingCardsCount()
    {
        return currentCards.Count;
    }
}
