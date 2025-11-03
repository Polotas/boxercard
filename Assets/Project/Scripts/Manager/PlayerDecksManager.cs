using System.Collections.Generic;
using UnityEngine;

public class PlayerDecksManager : MonoBehaviour
{
    public List<BoxerInfo> playerBoxers;

    public Sprite GetVictoryVisual(string boxer)
    {
        foreach (var t in playerBoxers)
        {
            if (t.id == boxer)
                return t.visualVictory;
        }

        return null;
    }
    
    public Sprite GetLoseVisual(string boxer)
    {
        foreach (var t in playerBoxers)
        {
            if (t.id == boxer)
                return t.visualLose;
        }

        return null;
    }
}
