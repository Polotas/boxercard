using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using OneGear.UI.Utility;

public enum GameTurn
{
    Player,
    Adversary
}

public enum BattlePhase
{
    Setup,      // Início do turno - spawn de cartas
    Action,     // Jogadores colocam cartas nas zonas
    Combat,     // Resolução do combate
    Cleanup     // Limpeza e preparação para próximo turno
}

[System.Serializable]
public class BattleEvents
{
    public Action<int> OnPlayerHealthChanged;
    public Action<int> OnAdversaryHealthChanged;
    public Action<GameTurn> OnTurnChanged;
    public Action<BattlePhase> OnPhaseChanged;
    public Action<string> OnBattleMessage;
    public Action OnBattleEnded;
}

public class BattleManager : MonoBehaviour
{
    [Header("Controllers")]
    public PlayerController playerController;
    public AdversaryController adversaryController;
    
    [Header("Battle State")]
    public GameTurn gameTurn = GameTurn.Player;
    public BattlePhase currentPhase = BattlePhase.Setup;
    
    [Header("Battle Settings")]
    [SerializeField] private float phaseTransitionDelay = 1f;
    [SerializeField] private float combatAnimationTime = 2f;
    
    [Header("Events")]
    public BattleEvents battleEvents = new BattleEvents();
    private bool isBattleActive = false;
    
    public bool IsBattleActive() => isBattleActive;
    public GameTurn GetCurrentTurn() => gameTurn;
    public BattlePhase GetCurrentPhase() => currentPhase;

    private void Awake()
    {
        UITransition.Instance.CallTransition(TRANSITIONS.FULL_TO_NULL);
    }
    
    public void StartBattle()
    {
        isBattleActive = true;
        gameTurn = GameTurn.Player;
        currentPhase = BattlePhase.Action;
        
        battleEvents.OnTurnChanged?.Invoke(gameTurn);
        battleEvents.OnPhaseChanged?.Invoke(currentPhase);
        battleEvents.OnBattleMessage?.Invoke("Batalha iniciada! Turno do Jogador");
        Debug.Log("Batalha iniciada!");
    }
    
    public void EndPlayerTurn()
    {
        if (!isBattleActive || gameTurn != GameTurn.Player) return;
        DragStatus.canDrag = false;
        Debug.Log("Fim do turno do jogador");
        battleEvents.OnBattleMessage?.Invoke("Fim do turno do jogador");
        
        StartCoroutine(ProcessTurnTransition());
    }
    
    private IEnumerator ProcessTurnTransition()
    {
        // Fase de Setup do Adversário
        currentPhase = BattlePhase.Setup;
        gameTurn = GameTurn.Adversary;
        battleEvents.OnPhaseChanged?.Invoke(currentPhase);
        battleEvents.OnTurnChanged?.Invoke(gameTurn);
        
        yield return new WaitForSeconds(phaseTransitionDelay);
        
        // Adversário joga
        yield return StartCoroutine(NextTurn(false));
        
        // Fase de Combate
        currentPhase = BattlePhase.Combat;
        battleEvents.OnPhaseChanged?.Invoke(currentPhase);
        battleEvents.OnBattleMessage?.Invoke("Resolvendo combate...");
        
        yield return StartCoroutine(ResolveCombat());
        
        // Cleanup e próximo turno
        currentPhase = BattlePhase.Cleanup;
        battleEvents.OnPhaseChanged?.Invoke(currentPhase);
        
        yield return StartCoroutine(CleanupPhase());
        
        // Verificar fim de jogo
        if (CheckGameEnd()) yield break;
        
        // Próximo turno do jogador
        StartNextPlayerTurn();
    }
    
    private IEnumerator NextTurn(bool isPlayer)
    {
        gameTurn = isPlayer ? GameTurn.Player : GameTurn.Adversary;
        currentPhase = BattlePhase.Action;
        
        battleEvents.OnTurnChanged?.Invoke(gameTurn);
        battleEvents.OnPhaseChanged?.Invoke(currentPhase);

        yield return new WaitForSeconds(1f);
      
        if (isPlayer)
        {
            if (playerController.currentCards.Count > 0)
            {
                playerController.SpawnCard();
                battleEvents.OnBattleMessage?.Invoke("Você recebeu uma nova carta!");
            }
            DragStatus.canDrag = true;
        }
        else
        {
                
            if (adversaryController.currentCards.Count > 0)
            {
                adversaryController.SpawnCard();
                battleEvents.OnBattleMessage?.Invoke("Adversário recebeu uma nova carta!");
                Debug.Log("Adversário recebeu uma carta do deck no início do turno");
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.LogWarning("Adversário não tem mais cartas no deck");
            } 
        }
        
        if(!isPlayer) yield return StartCoroutine(adversaryController.PlayTurn());
        yield return new WaitForSeconds(phaseTransitionDelay);
    }

    private IEnumerator ResolveCombat()
    {
        Debug.Log("=== RESOLUÇÃO DE COMBATE ===");
        
        // Resolver ataques do jogador vs defesas do adversário
       // yield return StartCoroutine(ResolvePlayerAttacks());
        
        yield return new WaitForSeconds(0.5f);
        
        // Resolver ataques do adversário vs defesas do jogador  
       // yield return StartCoroutine(ResolveAdversaryAttacks());
        
        yield return new WaitForSeconds(combatAnimationTime);
    }
    
    private IEnumerator ResolvePlayerAttacks()
    {
        // Ataque da mesa do jogador
        if (playerController.attackTable != null && playerController.attackTable.currentCardController != null)
        {
            var attackCard = playerController.attackTable.currentCardController;
            if (attackCard.data.type == CardType.Attack)
            {
                yield return StartCoroutine(ProcessAttack(attackCard, adversaryController, "Jogador"));
            }
        }
        
        yield return null;
    }
    
    private IEnumerator ResolveAdversaryAttacks()
    {
        // Ataque da mesa do adversário
        if (adversaryController.attackTable != null && adversaryController.attackTable.currentCardController != null)
        {
            var attackCard = adversaryController.attackTable.currentCardController;
            if (attackCard.data.type == CardType.Attack)
            {
                yield return StartCoroutine(ProcessAttack(attackCard, playerController, "Adversário"));
            }
        }
        
        yield return null;
    }
    
    private IEnumerator ProcessAttack(CardController attackCard, DeckController target, string attackerName)
    {
        int attackPower = attackCard.power;
        int remainingDamage = attackPower;
        
        battleEvents.OnBattleMessage?.Invoke($"{attackerName} ataca com {attackCard.data.displayName} (Poder: {attackPower})!");
        yield return new WaitForSeconds(1f);
        
        // Obter zonas de defesa do alvo
        UIDropZone[] targetDefenses = null;
        if (target == playerController)
            targetDefenses = playerController.defenses;
        else if (target == adversaryController)
            targetDefenses = adversaryController.defenses;
        
        if (targetDefenses != null)
        {
            // Atacar cartas de defesa primeiro
            foreach (var defenseZone in targetDefenses)
            {
                if (defenseZone.currentCardController != null && 
                    defenseZone.currentCardController.data.type == CardType.Defense && 
                    remainingDamage > 0)
                {
                    var defenseCard = defenseZone.currentCardController;
                    int defenseValue = defenseCard.defense;
                    
                    if (remainingDamage >= defenseValue)
                    {
                        // Carta de defesa é destruída
                        remainingDamage -= defenseValue;
                        battleEvents.OnBattleMessage?.Invoke($"{defenseCard.data.displayName} foi destruída! (Defesa: {defenseValue})");
                        
                        // Remover carta da zona (retornar para o deck)
                        ReturnCardToDeck(defenseCard.GetComponent<UIDragHandler>());
                        defenseZone.currentCardController = null;
                        defenseZone.currentUIDragHandler = null;
                    }
                    else
                    {
                        // Carta de defesa sobrevive com defesa reduzida
                        defenseCard.defense -= remainingDamage;
                        battleEvents.OnBattleMessage?.Invoke($"{defenseCard.data.displayName} resiste! (Defesa restante: {defenseCard.defense})");
                        remainingDamage = 0;
                    }
                    
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        
        // Se ainda há dano restante, aplicar à vida do jogador
        if (remainingDamage > 0)
        {
            target.health = Mathf.Max(0, target.health - remainingDamage);
            battleEvents.OnBattleMessage?.Invoke($"{target.name} recebe {remainingDamage} de dano! Vida: {target.health}");
            
            if (target == playerController)
            {
                battleEvents.OnPlayerHealthChanged?.Invoke(target.health);
            }
            else
            {
                battleEvents.OnAdversaryHealthChanged?.Invoke(target.health);
            }
            
            yield return new WaitForSeconds(1f);
        }
        else
        {
            battleEvents.OnBattleMessage?.Invoke("Ataque foi completamente bloqueado pelas defesas!");
            yield return new WaitForSeconds(1f);
        }
        
        // Remover carta de ataque após o uso
        RemoveUsedAttackCard(attackCard);
    }
    
    private void RemoveUsedAttackCard(CardController attackCard)
    {
        if (attackCard == null) return;
        
        // Encontrar a zona que contém esta carta
        UIDropZone attackZone = null;
        
        if (playerController.attackTable != null && playerController.attackTable.currentCardController == attackCard)
        {
            attackZone = playerController.attackTable;
        }
        else if (adversaryController.attackTable != null && adversaryController.attackTable.currentCardController == attackCard)
        {
            attackZone = adversaryController.attackTable;
        }
        
        if (attackZone != null)
        {
            // Limpar referências da zona
            attackZone.currentCardController = null;
            attackZone.currentUIDragHandler = null;
            
            // Animação de desaparecimento e destruição
            var cardTransform = attackCard.GetComponent<RectTransform>();
            var cardImage = attackCard.GetComponent<Image>();
            
            if (cardTransform != null)
            {
                cardTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                
                if (cardImage != null)
                {
                    cardImage.DOFade(0f, 0.3f).OnComplete(() => {
                        if (attackCard != null)
                        {
                            Destroy(attackCard.gameObject);
                        }
                    });
                }
                else
                {
                    // Fallback se não tiver Image
                    DOVirtual.DelayedCall(0.3f, () => {
                        if (attackCard != null)
                        {
                            Destroy(attackCard.gameObject);
                        }
                    });
                }
            }
            
            battleEvents.OnBattleMessage?.Invoke($"{attackCard.data.displayName} foi consumida no ataque!");
            Debug.Log($"Carta de ataque {attackCard.data.displayName} foi removida após o uso");
        }
    }
    
    private void ReturnCardToDeck(UIDragHandler cardDragHandler)
    {
        if (cardDragHandler == null) return;
        
        // Marca o card como estando no deck novamente
        cardDragHandler.onDeck = true;
        
        // Remove a referência à dropzone atual
        cardDragHandler.currentUiDropZone = null;
        
        // Adiciona o card de volta ao fan layout na posição original
        var rectCard = cardDragHandler.GetComponent<RectTransform>();
        if (rectCard != null && cardDragHandler.fanLayout != null)
        {
            cardDragHandler.fanLayout.AddCard(rectCard, cardDragHandler.originalSiblingIndex);
        }
    }
    
    private int CalculateAttackDamage(CardController attackCard, UIDropZone[] defenseZones)
    {
        int totalAttack = attackCard.power;
        int totalDefense = 0;
        
        // Somar defesa de todas as zonas de defesa
        foreach (var defenseZone in defenseZones)
        {
            if (defenseZone.currentCardController != null && 
                defenseZone.currentCardController.data.type == CardType.Defense)
            {
                totalDefense += defenseZone.currentCardController.defense;
            }
        }
        
        int finalDamage = Mathf.Max(0, totalAttack - totalDefense);
        Debug.Log($"Ataque: {totalAttack}, Defesa: {totalDefense}, Dano Final: {finalDamage}");
        
        return finalDamage;
    }
    
    private void ApplyDamage(DeckController target, int damage)
    {
        target.health = Mathf.Max(0, target.health - damage);
        
        if (target == playerController)
        {
            battleEvents.OnPlayerHealthChanged?.Invoke(target.health);
        }
        else
        {
            battleEvents.OnAdversaryHealthChanged?.Invoke(target.health);
        }
        
        Debug.Log($"{target.name} recebeu {damage} de dano. Vida restante: {target.health}");
    }
    
    private IEnumerator CleanupPhase()
    {
        battleEvents.OnBattleMessage?.Invoke("Limpando campo de batalha...");
        
        // Aplicar efeitos de cura das cartas de Health
        ApplyHealthEffects(playerController);
        ApplyHealthEffects(adversaryController);
        
        // Remover cartas usadas (opcional - dependendo das regras do jogo)
        // ClearUsedCards();
        
        yield return new WaitForSeconds(phaseTransitionDelay);
    }
    
    private void ApplyHealthEffects(DeckController controller)
    {
        // Cast para o tipo correto para acessar as propriedades específicas
        UIDropZone[] defenses = null;
        UIDropZone corner = null;
        
        if (controller == playerController)
        {
            defenses = playerController.defenses;
            corner = playerController.corner;
        }
        else if (controller == adversaryController)
        {
            defenses = adversaryController.defenses;
            corner = adversaryController.corner;
        }
        
        if (defenses != null)
        {
            foreach (var defenseZone in defenses)
            {
                if (defenseZone.currentCardController != null && 
                    defenseZone.currentCardController.data.type == CardType.Health)
                {
                    int healing = defenseZone.currentCardController.power;
                    controller.health = Mathf.Min(100, controller.health + healing); // Assumindo vida máxima de 100
                    
                    battleEvents.OnBattleMessage?.Invoke($"{controller.name} se curou {healing} pontos de vida!");
                    
                    if (controller == playerController)
                    {
                        battleEvents.OnPlayerHealthChanged?.Invoke(controller.health);
                    }
                    else
                    {
                        battleEvents.OnAdversaryHealthChanged?.Invoke(controller.health);
                    }
                }
            }
        }
        
        // Verificar carta de cura no corner também
        if (corner != null && corner.currentCardController != null && 
            corner.currentCardController.data.type == CardType.Health)
        {
            int healing = corner.currentCardController.power;
            controller.health = Mathf.Min(100, controller.health + healing);
            
            battleEvents.OnBattleMessage?.Invoke($"{controller.name} se curou {healing} pontos de vida!");
            
            if (controller == playerController)
            {
                battleEvents.OnPlayerHealthChanged?.Invoke(controller.health);
            }
            else
            {
                battleEvents.OnAdversaryHealthChanged?.Invoke(controller.health);
            }
        }
    }
    
    private bool CheckGameEnd()
    {
        if (playerController.health <= 0)
        {
            battleEvents.OnBattleMessage?.Invoke("Adversário venceu!");
            battleEvents.OnBattleEnded?.Invoke();
            isBattleActive = false;
            Debug.Log("Fim de jogo - Adversário venceu!");
            return true;
        }
        
        if (adversaryController.health <= 0)
        {
            battleEvents.OnBattleMessage?.Invoke("Jogador venceu!");
            battleEvents.OnBattleEnded?.Invoke();
            isBattleActive = false;
            Debug.Log("Fim de jogo - Jogador venceu!");
            return true;
        }
        
        return false;
    }

    private void StartNextPlayerTurn() => StartCoroutine(NextTurn(true));

    public void GetExtrasCards(bool isPlayer, int quantity)
    {
        if (isPlayer)
        {
            playerController.GetExtrasCards(quantity);
        }
        else
        {
            adversaryController.GetExtrasCards(quantity);
        }
    }
    
    public void SuperDefense(bool isPlayer)
    {
        if (isPlayer)
        {
            playerController.canDoDamage = false;
        }
        else
        {
            adversaryController.canDoDamage = false;
        }
    }

}
