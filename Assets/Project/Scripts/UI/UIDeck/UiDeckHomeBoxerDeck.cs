using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UiDeckHomeBoxerDeck : MonoBehaviour
{
    public Transform grid;
    public UiDeckHomeCardView prefab;
    public TextMeshProUGUI textQuantity;
    public int maxCards;
    
    private CardsManager _cardsManager;
    private List<UiDeckHomeCardView> _listDeck = new List<UiDeckHomeCardView>();

    private void Awake()
    {
        _cardsManager = FindFirstObjectByType<CardsManager>();
    }

    public void PopulateDecks(List<string> cardsStrings)
    {
        foreach (var t in _listDeck)
            Destroy(t.gameObject);

        _listDeck = new List<UiDeckHomeCardView>();
        
       // cardsStrings.Sort((a, b) => string.Compare(a, b, System.StringComparison.OrdinalIgnoreCase));
        var cards = _cardsManager.GetCards(cardsStrings,false);
        cards = cards.OrderBy(c => c.displayName, StringComparer.OrdinalIgnoreCase).ToList();
        
        Debug.Log("CARDS STRING: " + cardsStrings.Count + " CARDS: " + cards.Count);
        
        foreach (var t in cards)
        {
            UiDeckHomeCardView card = Instantiate(prefab, grid, true);
            card.transform.localScale = Vector3.one;
            card.Setup(t);
            _listDeck.Add(card);
        }

        textQuantity.text = cards.Count + "/" + maxCards;
    }
}
