using UnityEngine;

public class CardController : MonoBehaviour
{
    public bool isPlayer;
    public CardData data;
    public CardView cardView;
    public CardSpecials cardSpecials;
    
    public int power;
    public int defense;
    
    private int originalPower;
    private int originalDefense;

    public void SetupCard(CardData _data, bool flipCard = true, bool player = true)
    {
        data = _data;
        power = data.power;
        defense = data.defense;
        originalPower = data.power;
        originalDefense = data.defense;
        cardView.SetupCard(_data,flipCard);
        cardSpecials.Setup(this);
        isPlayer = player;
    }
    
    public void ApplyPowerBonus(int bonus)
    {
        power = originalPower + bonus;
        cardView.UpdateCardVisuals();
    }
    
    public void ResetToOriginalValues()
    {
        power = originalPower;
        defense = originalDefense;
        cardView.UpdateCardVisuals();
    }
}
