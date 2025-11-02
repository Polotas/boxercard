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
    public Action<bool> OnPlayerStun;
    public Action<bool> OnAdversaryStun;
    public Action<GameTurn> OnTurnChanged;
    public Action<BattlePhase> OnPhaseChanged;
    public Action<string> OnBattleMessage;
    public Action OnBattleEnded;
}

public class BattleManager : MonoBehaviour
{
    public RectTransform rectGame;
    
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

    public bool playerWinner;
    private bool isBattleActive = false;
    
    [System.Serializable]
    public class SideState
    {
        public int attackPowerBonus; // Buff por turno (Adrenaline Rush, Overcharge)
        public bool focusDoubleNextCard; // Focus
        public bool precisionIgnoreFirstDefenseThisTurn; // Precision
        public int overchargeSelfDamagePending; // Dano ao fim do turno
        public bool stunnedNextTurn; // Stun
        public bool wasAttackedLastTurn; // Para Counter Punch
        public bool halveNextDefenseOnce; // Feint no alvo
        public CardData lastDefensePlayed; // Para Mirror Guard
    }

    [Header("Side States")] 
    public SideState playerState = new SideState();
    public SideState adversaryState = new SideState();
    
    public bool IsBattleActive() => isBattleActive;
    public GameTurn GetCurrentTurn() => gameTurn;
    public BattlePhase GetCurrentPhase() => currentPhase;

    public Action onGameEnd;
    
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
        ResetAttackCardsVisuals(true);
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
        if (CheckGameEnd())
        {
            onGameEnd?.Invoke();
            yield break;
        }
        ResetAttackCardsVisuals(false);
        // Próximo turno do jogador
        StartCoroutine(NextTurn(true));
    }
    
    private IEnumerator NextTurn(bool isPlayer)
    {
        gameTurn = isPlayer ? GameTurn.Player : GameTurn.Adversary;
        currentPhase = BattlePhase.Action;
        
        battleEvents.OnTurnChanged?.Invoke(gameTurn);
        battleEvents.OnPhaseChanged?.Invoke(currentPhase);

        yield return new WaitForSeconds(1f);
      
        // Reset de buffs por turno
        var state = GetState(isPlayer);
        
        // Se tinha bônus de ataque, resetar valores visuais das cartas
        if (state.attackPowerBonus > 0)
        {
            ResetAttackCardsVisuals(isPlayer);
        }
        
        state.attackPowerBonus = 0;
        state.precisionIgnoreFirstDefenseThisTurn = false;
        state.overchargeSelfDamagePending = Mathf.Max(0, state.overchargeSelfDamagePending); // mantém até cleanup

        // Stun: perder o turno
        if (state.stunnedNextTurn)
        {
            state.stunnedNextTurn = false;
            battleEvents.OnBattleMessage?.Invoke(isPlayer ? "Jogador está atordoado e perde o turno!" : "Adversário está atordoado e perde o turno!");
            // Tremor no início do turno em que o lado está atordoado
            DoStun(isPlayer,false);
            yield return new WaitForSeconds(phaseTransitionDelay);
            yield break;
        }

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
                GameManager.Instance.currentDamageAdversary += remainingDamage;
                battleEvents.OnAdversaryHealthChanged?.Invoke(target.health);
            }
            
            // Impacto forte quando o adversário recebe dano alto
            if (target == adversaryController && remainingDamage > 10)
            {
                PlayHeavyHitEffectsOnAdversary();
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
            var cardController = rectCard.GetComponent<CardController>();
            cardDragHandler.fanLayout.AddCard(rectCard,cardController ,cardDragHandler.originalSiblingIndex);
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
        
        // Aplicar penalidades/efeitos de fim de turno
        ApplyEndOfTurnEffects(playerController, playerState);
        ApplyEndOfTurnEffects(adversaryController, adversaryState);
        
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
            playerWinner = false;
            return true;
        }
        
        if (adversaryController.health <= 0)
        {
            battleEvents.OnBattleMessage?.Invoke("Jogador venceu!");
            battleEvents.OnBattleEnded?.Invoke();
            isBattleActive = false;
            Debug.Log("Fim de jogo - Jogador venceu!");
            playerWinner = true;
            return true;
        }
        
        return false;
    }

    // Permite finalizar imediatamente quando a vida de algum lado chega a 0 fora do fluxo normal de turno
    public bool TryEndGameImmediate()
    {
        if (CheckGameEnd())
        {
            onGameEnd?.Invoke();
            return true;
        }
        return false;
    }

    private void DoStun(bool isPlayer, bool stun)
    {
        DeckController controller = isPlayer ? (DeckController)playerController : (DeckController)adversaryController;
        if(isPlayer) battleEvents.OnPlayerStun?.Invoke(stun);
        if(!isPlayer) battleEvents.OnAdversaryStun?.Invoke(stun);
        
        // if (controller == null) return;
        //
        // var rect = controller.GetComponent<RectTransform>();
        //
        // if (rect != null)
        //     rect.DOShakePosition(0.35f, 15f);
    }

    public void PlayHeavyHitEffectsOnAdversary()
    {
        rectGame.DOShakePosition(0.4f, 25f);
    }

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
    
    public void ExtraDamage(bool isPlayer)
    {
        // Placeholder: desativa dano do ALVO até o fim do turno
        DeckController target = isPlayer ? (DeckController)adversaryController : (DeckController)playerController;
        target.canDoDamage = false;
    }
    
    public void SuperDefense(bool isPlayer)
    {
        // Placeholder: desativa dano RECEBIDO do alvo (mantém sem alteração de cartas)
        DeckController target = isPlayer ? (DeckController)adversaryController : (DeckController)playerController;
        target.canDoDamage = false;
    }
    
    public void DestroyDefenses(bool isPlayer)
    {
        // Implementação: destrói TODAS as defesas do oponente
        DeckController target = isPlayer ? (DeckController)adversaryController : (DeckController)playerController;
        DestroyAllDefenses(target);
    }

    // ===== Novos utilitários/estados =====
    public SideState GetState(bool isPlayer) => isPlayer ? playerState : adversaryState;

    public void MarkDamageReceived(bool isPlayer)
    {
        var state = GetState(isPlayer);
        state.wasAttackedLastTurn = true;
    }

    public void SetLastDefensePlayed(bool isPlayer, CardData data)
    {
        var state = GetState(isPlayer);
        state.lastDefensePlayed = data;
    }

    public void ApplyAdrenalineRush(bool isPlayer)
    {
        GetState(isPlayer).attackPowerBonus += 4;
        battleEvents.OnBattleMessage?.Invoke(isPlayer ? "+4 Poder de Ataque neste turno (Jogador)" : "+4 Poder de Ataque neste turno (Adversário)");
        UpdateAttackCardsVisuals(isPlayer, 4);
    }

    public void ApplyFocus(bool isPlayer)
    {
        GetState(isPlayer).focusDoubleNextCard = true;
        battleEvents.OnBattleMessage?.Invoke("FOCUS: próxima carta terá o dobro de poder");
    }

    public void ApplyStunToOpponent(bool casterIsPlayer)
    {
        GetState(!casterIsPlayer).stunnedNextTurn = true;
        battleEvents.OnBattleMessage?.Invoke("STUN: oponente perderá o próximo turno");
        // Tremor imediato na carta do oponente ao aplicar Stun
        
        DoStun(!casterIsPlayer,true);
    }

    public void ApplyBreakGuardToOpponent(bool casterIsPlayer)
    {
        DeckController target = casterIsPlayer ? (DeckController)adversaryController : (DeckController)playerController;
        DestroyAllDefenses(target);
    }

    public void ApplyOvercharge(bool isPlayer)
    {
        var state = GetState(isPlayer);
        state.attackPowerBonus += 8;
        state.overchargeSelfDamagePending += 5;
        battleEvents.OnBattleMessage?.Invoke("OVERCHARGE: +8 poder neste turno (leva 5 de dano no fim)");
        
        // Atualizar visualmente todas as cartas de ataque na mão
        UpdateAttackCardsVisuals(isPlayer, 8);
    }

    public void ApplyPrecision(bool isPlayer)
    {
        GetState(isPlayer).precisionIgnoreFirstDefenseThisTurn = true;
        battleEvents.OnBattleMessage?.Invoke("PRECISION: ignorará a primeira defesa neste turno");
    }

    public void ApplyFeintToOpponent(bool casterIsPlayer)
    {
        GetState(!casterIsPlayer).halveNextDefenseOnce = true;
        battleEvents.OnBattleMessage?.Invoke("FEINT: próxima defesa do oponente terá 50% de defesa");
    }

    public void ApplySecondWind(bool isPlayer)
    {
        DeckController ctrl = isPlayer ? (DeckController)playerController : (DeckController)adversaryController;
        int healAmount = Mathf.CeilToInt(ctrl.maxHealth * 0.25f);
        int old = ctrl.health;
        ctrl.health = Mathf.Min(ctrl.maxHealth, ctrl.health + healAmount);
        if (ctrl == playerController) battleEvents.OnPlayerHealthChanged?.Invoke(ctrl.health); else battleEvents.OnAdversaryHealthChanged?.Invoke(ctrl.health);
        battleEvents.OnBattleMessage?.Invoke($"Second Wind: curou {ctrl.health - old} HP e comprou 1 carta");
        GetExtrasCards(isPlayer, 1);
    }

    public void ApplyMirrorGuard(bool casterIsPlayer)
    {
        var opponentState = GetState(!casterIsPlayer);
        if (opponentState.lastDefensePlayed == null)
        {
            battleEvents.OnBattleMessage?.Invoke("Mirror Guard: sem defesa do oponente para copiar");
            return;
        }
        var zone = FindFirstEmptyDefenseZone(casterIsPlayer);
        if (zone == null)
        {
            battleEvents.OnBattleMessage?.Invoke("Mirror Guard: sem slot de defesa disponível");
            return;
        }
        var spawner = casterIsPlayer ? playerController.deckCards : adversaryController.deckCards;
        var go = spawner.SpawnCardObject(opponentState.lastDefensePlayed, false, casterIsPlayer);
        if (go != null)
        {
            zone.HandleDrop(go);
            battleEvents.OnBattleMessage?.Invoke($"Mirror Guard: defesa '{opponentState.lastDefensePlayed.displayName}' copiada");
        }
    }

    private UIDropZone FindFirstEmptyDefenseZone(bool isPlayer)
    {
        var zones = isPlayer ? playerController.defenses : adversaryController.defenses;
        if (zones == null) return null;
        foreach (var z in zones)
        {
            if (z != null && z.currentCardController == null) return z;
        }
        return null;
    }

    private void DestroyAllDefenses(DeckController target)
    {
        UIDropZone[] targetDefenses = null;
        if (target == playerController) targetDefenses = playerController.defenses; else if (target == adversaryController) targetDefenses = adversaryController.defenses;
        if (targetDefenses == null) return;
        foreach (var dz in targetDefenses)
        {
            if (dz != null && dz.currentCardController != null && dz.currentCardController.data.type == CardType.Defense)
            {
                // reutiliza rotina de destruição da UIDropZone (se existir)
                var card = dz.currentCardController;
                dz.currentCardController = null;
                dz.currentUIDragHandler = null;
                if (card != null) Destroy(card.gameObject);
            }
        }
        battleEvents.OnBattleMessage?.Invoke($"Todas as defesas de {target.name} foram destruídas!");
    }

    private void ApplyEndOfTurnEffects(DeckController controller, SideState state)
    {
        if (state.overchargeSelfDamagePending > 0)
        {
            controller.health = Mathf.Max(0, controller.health - state.overchargeSelfDamagePending);
            if (controller == playerController) battleEvents.OnPlayerHealthChanged?.Invoke(controller.health); else battleEvents.OnAdversaryHealthChanged?.Invoke(controller.health);
            battleEvents.OnBattleMessage?.Invoke($"Overcharge: {controller.name} sofreu {state.overchargeSelfDamagePending} de dano no fim do turno");
            state.overchargeSelfDamagePending = 0;
        }
    }
    
    private void UpdateAttackCardsVisuals(bool isPlayer, int bonus)
    {
        DeckController controller = isPlayer ? (DeckController)playerController : (DeckController)adversaryController;
        var cardsInHand = controller.GetCardsInHand();
        
        foreach (var card in cardsInHand)
        {
            if (card.data.type == CardType.Attack)
            { 
                if(card != null)
                    card.ApplyPowerBonus(bonus);
            }
        }
        
        Debug.Log($"Bônus de +{bonus} aplicado visualmente em {cardsInHand.Count} cartas de ataque na mão");
    }
    
    public void ResetAttackCardsVisuals(bool isPlayer)
    {
        DeckController controller = isPlayer ? (DeckController)playerController : (DeckController)adversaryController;
        var cardsInHand = controller.GetCardsInHand();
        
        foreach (var card in cardsInHand)
        {
            if(card != null)
                card.ResetToOriginalValues();
        }
        
        Debug.Log($"Valores originais restaurados em {cardsInHand.Count} cartas na mão");
    }
}
