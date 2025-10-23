using UnityEngine;

public class UIDecksHome : MonoBehaviour
{
    public UIDeckHomeSelector uIDeckSelector;

    private void Awake()
    {
        GameManager.Instance.Load();
    }

    private void Start()
    {
        uIDeckSelector.PopulateDecks();
    }
}
