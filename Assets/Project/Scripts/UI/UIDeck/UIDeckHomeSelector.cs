using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDeckHomeSelector : MonoBehaviour
{
    public UiDeckHomeBoxerDeck uiDeckHomeBoxerDeck;
    public UIDeckHomeCards uiDeckHomeCards;
    public Transform grid;
    public UIDeckHomeBoxer prefab;
    
    private PlayerDecksManager _playerDecks;
    private List<UIDeckHomeBoxer> _listDeck;

    private void Awake()
    {
        _listDeck = new List<UIDeckHomeBoxer>();
        _playerDecks = FindFirstObjectByType<PlayerDecksManager>();
    }

    private void Start()
    {
        GameManager.Instance.onUpdateDeck += OnSelect;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onUpdateDeck -= OnSelect;
    }

    public void PopulateDecks()
    {
        _listDeck = new List<UIDeckHomeBoxer>();

        foreach (var t in _playerDecks.playerBoxers)
        {
            UIDeckHomeBoxer boxer = Instantiate(prefab, grid, true);
            boxer.Setup(t,this);
            _listDeck.Add(boxer);
        }
    }

    public void OnSelect(string boxerName)
    {
        var cardsStrings = GameManager.Instance.GetListOfCardsDeck(boxerName);
        uiDeckHomeBoxerDeck.PopulateDecks(cardsStrings);
        uiDeckHomeCards.UpdateCards(cardsStrings);

        Debug.Log("BOXER ID: " + boxerName);
        
        foreach (var t in _listDeck)
        {
            t.OnUpdate(boxerName);
        }
    }
}
