using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Boxe/Boxer Data")]
public class BoxerData : ScriptableObject
{
    [Header("Base Info")]
    public BoxerInfo boxerInfo;

    [Header("Stats")]
    public int health;

    [Header("Cards")] 
    public List<string> cards;
}

[Serializable]
public class BoxerInfo
{   
    public string id;
    public string displayName;
    public Sprite visual;
    public Sprite visualVictory;
    public Sprite visualLose;
}