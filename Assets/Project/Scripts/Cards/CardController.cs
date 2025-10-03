using UnityEngine;

public class CardController : MonoBehaviour
{
    public bool isPlayer;
    public CardData data;
    public CardView cardView;

    public int power;
    public int defense;

    public void SetupCard(CardData _data, bool flipCard = true, bool player = true)
    {
        data = _data;
        power = data.power;
        defense = data.defense;
        cardView.SetupCard(_data,flipCard);
        isPlayer = player;
    }
}
