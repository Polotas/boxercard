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
    Shield,
    DestroyDefense,
    AdrenalineRush,
    Focus,
    Stun,
    BreakGuard,
    Overcharge,
    MirrorGuard,
    Precision,
    SecondWind
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Boxe/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Base Info")]
    public string id;
    public int maxCards;
    [TextArea] public string displayName;
    [TextArea] public string description;
    public CardType type;
    public SpecialType special;
    
    [Header("View")] 
    public Sprite visual;
    
    [Header("Stats")]
    public int power;
    public int defense;
}