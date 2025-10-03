using UnityEngine;

public enum CardType
{
    Attack,
    Defense,
    Health
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Boxe/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Base Info")]
    public string name;
    public string displayName;
    public CardType type;

    [Header("View")] 
    public Sprite visual;
    
    [Header("Stats")]
    public int power;
    public int defense;
}