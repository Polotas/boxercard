using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

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
                if (t1 == t.id)
                {
                    currentCards.Add(t);
                }
            }
        }
        
        currentCards = currentCards.OrderBy(x => Random.value).ToList();
        
        return currentCards;
    }
    
    [ContextMenu("FIND CARDS")]
    public void FindAllCardData()
    {
        string folderPath = "Assets/Project/ScriptableObjects/Cards";
        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { folderPath });

        cards = new List<CardData>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

            if (card != null)
            {
                cards.Add(card);
                Debug.Log($"Encontrado: {card.name} em {assetPath}");
            }
        }

        Debug.Log($"✅ Total de CardData encontrados: {cards.Count}");
    }
}
