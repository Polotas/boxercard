using UnityEngine;

public class CardBoxerController : MonoBehaviour
{
    public bool isPlayer;
    public BoxerData boxerData;
    public CardBoxerView cardBoxerView;
    private BattleManager _battleManager;
    
    private void Awake()
    {
        _battleManager = FindFirstObjectByType<BattleManager>();
    }
    
    public void SetupBoxer(BoxerData data, bool subscribeToBattleEvents)
    {
        boxerData = data;
        cardBoxerView.Setup(data,isPlayer);

        if (subscribeToBattleEvents) SubscribeToBattleEvents();
    }

    private void UpdateHealth(int valor) => cardBoxerView.UpdateHealthValor(valor);
    
    private void UpdateStun(bool active) => cardBoxerView.UpdateStun(active);
    
    private void SubscribeToBattleEvents()
    {
        switch (isPlayer)
        {
            case true:
                _battleManager.battleEvents.OnPlayerHealthChanged += UpdateHealth;
                _battleManager.battleEvents.OnPlayerStun += UpdateStun;
                break;
            case false:
                _battleManager.battleEvents.OnAdversaryHealthChanged += UpdateHealth;
                _battleManager.battleEvents.OnAdversaryStun += UpdateStun;
                break;
        }
    }
}
