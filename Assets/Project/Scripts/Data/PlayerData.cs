using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string currentBoxer = "boxer01";
    public List<BoxerDeckData> listBoxerDeckData;
    public OptionData optionsData = new OptionData();
    
    public PlayerData InitPlayerData()
    {
        this.listBoxerDeckData = new List<BoxerDeckData>();

        var boxer1 = new BoxerDeckData {boxerID = "boxer01", listOFCards = GetFirstDeck()};
        var boxer2 = new BoxerDeckData {boxerID = "boxer02", listOFCards = GetFirstDeck()};

        this.listBoxerDeckData.Add(boxer1);
        this.listBoxerDeckData.Add(boxer2);
        optionsData = new OptionData();
        return this;
    }

    private List<string> GetFirstDeck()
    {
        var firstDeck = new List<string>
        {
            // Ataque base
            "Jab","Jab","Jab","Hoock","Upper",
            // Novos ataques
            "Cross","Body Blow","Counter Punch","Flurry","Feint","Finisher",
            // Defesas
            "Block","Block","S.Block","Dodge",
            // Cura
            "Small Health","Health","Big Health",
            // Especiais
            "Extra Cards","Focus","Adrenaline Rush","Stun","Break Guard","Overcharge","Mirror Guard","Precision","Second Wind"
        };

        return firstDeck;
    }
}

[Serializable]
public class BoxerDeckData
{
    public string boxerID;
    public List<string> listOFCards;
}
