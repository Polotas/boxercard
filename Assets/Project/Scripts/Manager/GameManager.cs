using System;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    public bool initGame;
    public bool hasLoad = false;
    public string sceneToLoad;

    public PlayerData playerData;

    public Action<string> onUpdateDeck;

    public void Load()
    {
        if (!ES3.KeyExists("PlayerSave"))
        {
            playerData = new PlayerData().InitPlayerData();
            ES3.Save("PlayerSave", playerData);
        }
        else
        {
            playerData = ES3.Load<PlayerData>("PlayerSave");
        }
        
        hasLoad = true;
    }

    public void Save()
    {
        ES3.Save("PlayerSave", playerData);
    }
    
    public List<string> GetListOfCardsDeck(string boxer)
    {
        var cards = new List<string>();
        if (!hasLoad) Load();
        
        boxer ??= "boxer01";
        playerData.currentBoxer = boxer;
        foreach (var t in playerData.listBoxerDeckData)
        {
            if (t.boxerID == boxer)
            {
                return t.listOFCards;
            }
        }
        
        return cards;
    }
    
    public List<string> AddCardToDeck(string card)
    {
        var cards = new List<string>();

        foreach (var t in playerData.listBoxerDeckData)
        {
            if (t.boxerID != playerData.currentBoxer) continue;
            t.listOFCards.Add(card);
            onUpdateDeck?.Invoke(playerData.currentBoxer);
            ES3.Save("PlayerSave", playerData);
            return t.listOFCards;
        }

        return cards;
    }
    
    public List<string> RemoveCardFromDeck(string card)
    {
        var cards = new List<string>();

        foreach (var t in playerData.listBoxerDeckData)
        {
            if (t.boxerID != playerData.currentBoxer) continue;
            for (int i = 0; i < t.listOFCards.Count; i++)
            {
                if (t.listOFCards[i] != card) continue;
                t.listOFCards.RemoveAt(i);
                onUpdateDeck?.Invoke(playerData.currentBoxer);
                ES3.Save("PlayerSave", playerData);
                return t.listOFCards;
            }
        }
        
        return cards;
    }
}
