using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGamePlay : MonoBehaviour
{
    [Header("UI Elements")]
    public Button buttonEndTurn;
    public TextMeshProUGUI textCurrentTurn;
    public TextMeshProUGUI textBattleMessage;
    public TextMeshProUGUI textCurrentPhase;

    private BattleManager _battleManager;
    
    private void Awake()
    {
        _battleManager = FindFirstObjectByType<BattleManager>();
        buttonEndTurn.onClick.AddListener(Button_EndTurn);
        
        SubscribeToBattleEvents();
    }
    
    private void Start()
    {
        UpdateCurrentTurn(_battleManager.GetCurrentTurn());
        UpdateCurrentPhase(_battleManager.GetCurrentPhase());
    }

    private void SubscribeToBattleEvents()
    {
        if (_battleManager?.battleEvents == null) return;
        _battleManager.battleEvents.OnTurnChanged += UpdateCurrentTurn;
        _battleManager.battleEvents.OnPhaseChanged += UpdateCurrentPhase;
        _battleManager.battleEvents.OnBattleMessage += UpdateBattleMessage;
        _battleManager.battleEvents.OnBattleEnded += OnBattleEnded;
    }
    
    private void OnDestroy()
    {
        if (_battleManager?.battleEvents == null) return;
        _battleManager.battleEvents.OnTurnChanged -= UpdateCurrentTurn;
        _battleManager.battleEvents.OnPhaseChanged -= UpdateCurrentPhase;
        _battleManager.battleEvents.OnBattleMessage -= UpdateBattleMessage;
        _battleManager.battleEvents.OnBattleEnded -= OnBattleEnded;
    }
    
    private void Button_EndTurn()
    {
        if (_battleManager.GetCurrentTurn() == GameTurn.Player && 
            _battleManager.GetCurrentPhase() == BattlePhase.Action)
        {
            _battleManager.EndPlayerTurn();
        }
    }

    private void UpdateCurrentTurn(GameTurn turn)
    {
        if (textCurrentTurn != null)
        {
            string turnText = turn == GameTurn.Player ? "Seu Turno" : "Turno do Adversário";
            textCurrentTurn.text = turnText;
        }
        
        if (buttonEndTurn != null)
        {
            buttonEndTurn.interactable = (turn == GameTurn.Player && 
                                         _battleManager.GetCurrentPhase() == BattlePhase.Action);
        }
    }
    
    private void UpdateCurrentPhase(BattlePhase phase)
    {
        if (textCurrentPhase != null)
        {
            string phaseText = phase switch
            {
                BattlePhase.Setup => "Preparação",
                BattlePhase.Action => "Ação",
                BattlePhase.Combat => "Combate",
                BattlePhase.Cleanup => "Limpeza",
                _ => "Desconhecida"
            };
            textCurrentPhase.text = $"Fase: {phaseText}";
        }
        
        if (buttonEndTurn != null)
        {
            buttonEndTurn.interactable = (phase == BattlePhase.Action && 
                                         _battleManager.GetCurrentTurn() == GameTurn.Player);
        }
    }
    
    private void UpdateBattleMessage(string message)
    {
        if (textBattleMessage != null)
            textBattleMessage.text = message;
        
        Debug.Log($"[BATALHA] {message}");
    }
    
    private void OnBattleEnded()
    {
        if (buttonEndTurn != null)
        {
            buttonEndTurn.interactable = false;
        }
        
        Debug.Log("Batalha finalizada!");
    }
    
    public void StartBattle() => _battleManager.StartBattle();
}
