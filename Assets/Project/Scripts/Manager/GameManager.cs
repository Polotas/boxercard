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

        AudioManager.SetMute(AudioManager.AudioType.BG,playerData.optionsData.soundBGFX);
        AudioManager.SetMute(AudioManager.AudioType.FX,playerData.optionsData.soundFX);
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

    public bool CanAddOrRemoveCard()
    {
        foreach (var t in playerData.listBoxerDeckData)
        {
            if (t.boxerID != playerData.currentBoxer) continue;
            return t.listOFCards.Count > 25 && t.listOFCards.Count < 30;
        }
        
        return false;
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

[Serializable]
public class OptionData
{
    public bool soundBGFX = true;
    public bool soundFX = true;
}
