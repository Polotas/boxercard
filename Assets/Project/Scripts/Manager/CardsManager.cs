using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardsManager : MonoBehaviour
{
    public List<CardData> cards;

    public List<CardData> GetCards(List<string> cardsName)
    {
        var currentCards = new List<CardData>();

        foreach (var t1 in cardsName)
        {
            foreach (var t in cards)
            {
                if (t1 == t.name)
                {
                    currentCards.Add(t);
                }
            }
        }
        
        currentCards = currentCards.OrderBy(x => Random.value).ToList();
        
        return currentCards;
    }
}
