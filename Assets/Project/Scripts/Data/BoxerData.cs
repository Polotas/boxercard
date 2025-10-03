using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Boxe/Boxer Data")]
public class BoxerData : ScriptableObject
{
    [Header("Base Info")]
    public string id;
    public string displayName;
    
    [Header("View")] 
    public Sprite visual;
    
    [Header("Stats")]
    public int health;

    [Header("Cards")] 
    public List<string> cards;
}
