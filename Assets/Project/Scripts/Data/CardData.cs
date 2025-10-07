using UnityEngine;

public enum CardType
{
    Attack,
    Defense,
    Health,
    Special,
    Corner
}

public enum SpecialType
{
    ExtraCards,
    ExtraDamage,
    SuperDefense,
    DestroyDefense
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Boxe/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Base Info")]
    public string id;
    [TextArea] public string displayName;
    public CardType type;
    public SpecialType special;
    
    [Header("View")] 
    public Sprite visual;
    
    [Header("Stats")]
    public int power;
    public int defense;
}