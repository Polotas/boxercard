using UnityEngine;

public class DeckCards : MonoBehaviour
{
    [SerializeField] private FanLayout fanLayout;
    [SerializeField] private GameObject cardPrefab;
    
    public void SpawnCard(CardData data, bool flipCard = true, bool player = true)
    {
        RectTransform card = Instantiate(cardPrefab).GetComponent<RectTransform>();
        card.transform.position = transform.position;
        var dragHandler = card.GetComponent<UIDragHandler>();
        var carController = card.GetComponent<CardController>();
        
        if(!player) Destroy(dragHandler);
        else dragHandler.fanLayout = fanLayout;

        carController.SetupCard(data,flipCard,player);
        fanLayout.AddCard(card);
    }
}
