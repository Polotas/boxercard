using System.Collections.Generic;
using UnityEngine;

public class UIDeckHomeCards : MonoBehaviour
{
    public Transform grid;
    public UIDeckHomeCardViewer prefab;
    public List<UIDeckHomeCardViewer> cards;
    
    private CardsManager _cardsManager;

    private void Awake()
    {
        _cardsManager = FindFirstObjectByType<CardsManager>();
        PopulateDecks();
    }
    
    public void PopulateDecks()
    {
        cards = new List<UIDeckHomeCardViewer>();

        foreach (var t in _cardsManager.cards)
        {
            UIDeckHomeCardViewer card = Instantiate(prefab, grid, true);
            card.transform.localScale = Vector3.one;
            card.Setup(t);
            cards.Add(card);
        }
    }

    public void UpdateCards(List<string> listCards)
    {
        // 1️⃣ Conta quantas vezes cada carta aparece
        Dictionary<string, int> cardCounts = new Dictionary<string, int>();
        foreach (var cardName in listCards)
        {
            if (cardCounts.ContainsKey(cardName))
                cardCounts[cardName]++;
            else
                cardCounts[cardName] = 1;
        }

        // 2️⃣ Atualiza cada card na UI
        foreach (var cardViewer in cards)
        {
            string id = cardViewer.cardData.id; // ou o campo equivalente
            if (cardCounts.TryGetValue(id, out int quantity))
            {
                cardViewer.UpdateQuantity(quantity);
            }
            else
            {
                cardViewer.UpdateQuantity(0);
            }
        }
    }
}
