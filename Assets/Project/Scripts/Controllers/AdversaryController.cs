using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AdversaryController : DeckController
{
    public UIDropZone[] defenses;
    public UIDropZone corner;
    public UIDropZone attackTable; // Mesa onde cartas de ataque e cura são colocadas
    public FanLayout fanLayout;
    public Canvas canvas;
    
    [Header("AI Settings")]
    [SerializeField] private float decisionDelay = 1f;
    [SerializeField] private float actionDelay = 0.5f;
    [SerializeField] private int maxCardsPerTurn = 3;
    
    // Estrutura para análise do estado do jogo
    private struct GameState
    {
        public bool needsHealing;
        public bool playerHasAttacks;
        public bool playerHasDefenses;
        public int emptyDefenseSlots;
        public int emptyAttackSlots;
        public bool attackTableEmpty;
        public float healthPercentage;
        public int playerAttackThreat;
    }
    
    public void StartGame(BoxerData data) => SetupBoxer(data);
    
    public IEnumerator PlayTurn()
    {
        Debug.Log("=== TURNO DO ADVERSÁRIO ===");
        
        yield return new WaitForSeconds(decisionDelay);
        
        // IA simples: priorizar defesa se jogador tem carta de ataque, senão atacar
        yield return StartCoroutine(MakeAIDecision());
    }
    
    private IEnumerator MakeAIDecision()
    {
        if (currentCards.Count == 0)
        {
            Debug.Log("Adversário não tem cartas para jogar - passando turno");
            yield break;
        }
        
        Debug.Log("=== PLANEJAMENTO DE TURNO DO ADVERSÁRIO ===");
        
        // Analisar estado atual do jogo
        var gameState = AnalyzeGameState();
        
        // Criar lista de cartas para usar neste turno
        var plannedCards = PlanTurnCards(gameState);
        
        if (plannedCards.Count == 0)
        {
            Debug.Log("Nenhuma carta útil encontrada - adversário passa o turno");
            yield break;
        }
        
        Debug.Log($"Adversário planeja usar {plannedCards.Count} cartas neste turno:");
        foreach (var card in plannedCards)
        {
            Debug.Log($"- {card.displayName} ({card.type})");
        }
        
        // Executar o plano de turno
        yield return StartCoroutine(ExecuteTurnPlan(plannedCards));
    }

    private IEnumerator PlaceCardStrategically(CardController cardToPlace)
    {
        bool playerHasAttack = HasPlayerAttackInCorner();
        var rectTransformCard = cardToPlace.GetComponent<RectTransform>();
        fanLayout.RemoveCard(rectTransformCard);
        
        switch (cardToPlace.data.type)
        {
            case CardType.Defense when playerHasAttack:
                yield return StartCoroutine(PlaceDefenseCard(cardToPlace));
                break;
            case CardType.Attack:
                yield return StartCoroutine(PlaceAttackCard(cardToPlace));
                break;
            case CardType.Health:
                yield return StartCoroutine(PlaceHealthCard(cardToPlace));
                break;
            default:
                yield return StartCoroutine(PlaceCardInAvailableSlot(cardToPlace));
                break;
        }
    }
    
    private bool ShouldUseHealing()
    {
        // Usar cura apenas se vida estiver abaixo de 20%
        float healthPercentage = (float)health / 100f;
        bool shouldHeal = healthPercentage < 0.2f;
        
        Debug.Log($"Adversário vida: {health}/100 ({healthPercentage:P}) - Deve usar cura: {shouldHeal}");
        return shouldHeal;
    }
    
    private bool HasPlayerAttackInCorner()
    {
        var battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager?.playerController?.corner?.currentCardController != null)
        {
            return battleManager.playerController.corner.currentCardController.data.type == CardType.Attack;
        }
        return false;
    }
    
    private IEnumerator PlaceDefenseCard(CardController card)
    {
        // Encontrar slot de defesa vazio ou substituir o mais fraco
        UIDropZone bestDefenseSlot = FindBestDefenseSlot();
        
        if (bestDefenseSlot != null)
        {
            yield return StartCoroutine(PlaceCardInZone(card, bestDefenseSlot));
            Debug.Log($"Adversário colocou {card.data.displayName} na defesa");
        }
    }
    
    private IEnumerator PlaceAttackCard(CardController card)
    {
        // Priorizar mesa de ataque para cartas de ataque
        if (attackTable != null && attackTable.currentCardController == null)
        {
            yield return StartCoroutine(PlaceCardInZone(card, attackTable));
            Debug.Log($"Adversário colocou {card.data.displayName} na mesa para atacar");
        }
        else if (attackTable != null && attackTable.currentCardController != null && 
                 attackTable.currentCardController.power < card.power)
        {
            // Substituir ataque mais fraco na mesa
            yield return StartCoroutine(PlaceCardInZone(card, attackTable));
            Debug.Log($"Adversário substituiu ataque na mesa por {card.data.displayName} (mais forte)");
        }
        else if (corner.currentCardController == null)
        {
            yield return StartCoroutine(PlaceCardInZone(card, corner));
            Debug.Log($"Adversário colocou {card.data.displayName} no corner para atacar");
        }
        else if (corner.currentCardController != null && corner.currentCardController.power < card.power)
        {
            // Substituir ataque mais fraco no corner
            yield return StartCoroutine(PlaceCardInZone(card, corner));
            Debug.Log($"Adversário substituiu ataque no corner por {card.data.displayName} (mais forte)");
        }
        else
        {
            // Se não há slots de ataque disponíveis, descartar a carta
            Debug.Log($"Adversário não conseguiu colocar {card.data.displayName} - slots de ataque ocupados com cartas mais fortes");
            yield return StartCoroutine(DiscardCard(card));
        }
    }
    
    private IEnumerator PlaceHealthCard(CardController card)
    {
        Debug.Log($"=== ADVERSÁRIO TENTANDO USAR CURA ===");
        Debug.Log($"Carta: {card.data.displayName} (Poder: {card.power})");
        Debug.Log($"Mesa de ataque disponível: {attackTable != null}");
        
        if (attackTable != null)
        {
            Debug.Log($"Mesa ocupada: {attackTable.currentCardController != null}");
            if (attackTable.currentCardController != null)
            {
                Debug.Log($"Carta atual na mesa: {attackTable.currentCardController.data.displayName} ({attackTable.currentCardController.data.type})");
            }
        }
        
        // Usar mesa de ataque para cartas de cura também
        if (attackTable != null && attackTable.currentCardController == null)
        {
            Debug.Log($"Colocando {card.data.displayName} na mesa vazia para cura");
            yield return StartCoroutine(PlaceCardInZone(card, attackTable));
            Debug.Log($"Adversário colocou {card.data.displayName} na mesa para cura - DEVE CURAR AGORA");
        }
        else if (attackTable != null && attackTable.currentCardController != null)
        {
            // Decidir se vale a pena substituir baseado no tipo e poder
            var currentCard = attackTable.currentCardController;
            bool shouldReplace = false;
            
            if (currentCard.data.type == CardType.Health && currentCard.power < card.power)
            {
                // Substituir cura mais fraca
                shouldReplace = true;
                Debug.Log($"Substituindo cura mais fraca ({currentCard.power} < {card.power})");
            }
            else if (currentCard.data.type == CardType.Attack && card.power > currentCard.power)
            {
                // Substituir ataque mais fraco por cura mais forte (estratégia defensiva)
                shouldReplace = true;
                Debug.Log($"Substituindo ataque por cura ({currentCard.power} < {card.power})");
            }
            
            if (shouldReplace)
            {
                yield return StartCoroutine(PlaceCardInZone(card, attackTable));
                Debug.Log($"Adversário substituiu {currentCard.data.displayName} na mesa por {card.data.displayName} para cura - DEVE CURAR AGORA");
            }
            else
            {
                Debug.Log($"Adversário não conseguiu colocar {card.data.displayName} - mesa ocupada com carta mais vantajosa");
                yield return StartCoroutine(DiscardCard(card));
            }
        }
        else
        {
            Debug.Log($"Adversário não conseguiu colocar {card.data.displayName} - mesa de ataque não disponível");
            yield return StartCoroutine(DiscardCard(card));
        }
    }
    
    private IEnumerator PlaceCardInAvailableSlot(CardController card)
    {
        if (card.data.type == CardType.Attack)
        {
            yield return StartCoroutine(PlaceAttackCard(card));
            yield break;
        }
        
        if (card.data.type == CardType.Defense)
        {
            yield return StartCoroutine(PlaceDefenseCard(card));
            yield break;
        }
        
        if (card.data.type == CardType.Health)
        {
            yield return StartCoroutine(PlaceHealthCard(card));
            yield break;
        }
        
        if (corner.currentCardController == null)
        {
            yield return StartCoroutine(PlaceCardInZone(card, corner));
            yield break;
        }
        
        Debug.Log($"Adversário não encontrou slot apropriado para {card.data.displayName} ({card.data.type})");
        yield return StartCoroutine(DiscardCard(card));
    }
    
    private UIDropZone FindBestDefenseSlot()
    {
        UIDropZone bestSlot = null;
        int lowestDefense = int.MaxValue;
        
        foreach (var defenseSlot in defenses)
        {
            if (defenseSlot.currentCardController == null)
            {
                return defenseSlot; // Slot vazio é sempre melhor
            }
            
            if (defenseSlot.currentCardController.defense < lowestDefense)
            {
                lowestDefense = defenseSlot.currentCardController.defense;
                bestSlot = defenseSlot;
            }
        }
        
        return bestSlot;
    }
    
    private UIDropZone FindAvailableDefenseSlot()
    {
        foreach (var defenseSlot in defenses)
        {
            if (defenseSlot.currentCardController == null)
            {
                return defenseSlot;
            }
        }
        return null;
    }
    
    private IEnumerator PlaceCardInZone(CardController card, UIDropZone targetZone)
    {
        yield return StartCoroutine(FlipAndPositionCard(card, targetZone));
        targetZone.HandleDrop(card.gameObject);
        yield return new WaitForSeconds(actionDelay);
    }
    
    private IEnumerator FlipAndPositionCard(CardController card, UIDropZone targetZone)
    {
        var cardTransform = card.GetComponent<RectTransform>();
        var cardView = card.cardView;
        
        if (cardTransform == null || cardView == null) yield break;

        Vector3 worldPosition = cardTransform.position;
        
        cardTransform.SetParent(canvas.transform, true);
        cardTransform.SetAsLastSibling(); // Ficar por cima de outras cartas
        
        cardTransform.position = worldPosition;
        
        cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cardTransform.anchorMin = new Vector2(0.5f, 0.5f);
        
        var targetRectTransform = targetZone.GetComponent<RectTransform>();
        Vector2 targetPosition;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetRectTransform.position),
            canvas.worldCamera,
            out targetPosition
        );
        
        var moveAnimation = cardTransform.DOAnchorPos(targetPosition, 0.5f).SetEase(Ease.OutQuad);
        
        StartCoroutine(cardView.IE_FlipCard());
        yield return moveAnimation.WaitForCompletion();
        
        cardTransform.localRotation = Quaternion.identity; // Rotação 0
        cardTransform.SetAsLastSibling(); // Por cima de outras cartas
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"Carta do adversário {card.data.displayName} movida para {targetZone.name} via Canvas - Rotação resetada e posicionada como LastSibling");
    }
    
    private IEnumerator DiscardCard(CardController card)
    {
        var cardTransform = card.GetComponent<RectTransform>();
        var cardImage = card.GetComponent<Image>();
        
        if (cardTransform != null)
        {
            cardTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            
            if (cardImage != null)
            {
                cardImage.DOFade(0f, 0.3f);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            Destroy(card.gameObject);
        }
        
        Debug.Log($"Adversário descartou {card.data.displayName} - não havia slot apropriado");
    }
    
    private CardData FindHealingCardInDeck()
    {
        // Procurar carta de cura no deck atual
        foreach (var cardData in currentCards)
        {
            if (cardData.type == CardType.Health)
            {
                Debug.Log($"Carta de cura encontrada no deck: {cardData.displayName} (Power: {cardData.power})");
                return cardData;
            }
        }
        
        Debug.Log("Nenhuma carta de cura encontrada no deck");
        return null;
    }
    
    private void SpawnSpecificCard(CardData specificCard)
    {
        // Encontrar a carta específica no deck e spawnná-la
        for (int i = 0; i < currentCards.Count; i++)
        {
            if (currentCards[i] == specificCard)
            {
                Debug.Log($"Spawnando carta específica: {specificCard.displayName}");
                
                // Spawnar a carta específica
                deckCards.SpawnCard(specificCard, player, player);
                
                // Remover do deck
                currentCards.RemoveAt(i);
                
                Debug.Log($"Carta {specificCard.displayName} spawnada e removida do deck. Cartas restantes: {currentCards.Count}");
                return;
            }
        }
        
        Debug.LogWarning($"Carta específica {specificCard.displayName} não encontrada no deck para spawn");
    }
    
    private GameState AnalyzeGameState()
    {
        var battleManager = FindFirstObjectByType<BattleManager>();
        var gameState = new GameState();
        
        // Análise da própria saúde
        gameState.healthPercentage = (float)health / 100f;
        gameState.needsHealing = gameState.healthPercentage < 0.2f;
        
        // Análise dos slots próprios
        gameState.emptyDefenseSlots = CountEmptyDefenseSlots();
        gameState.attackTableEmpty = attackTable.currentCardController == null;
        
        // Análise das ameaças do jogador
        if (battleManager?.playerController != null)
        {
            gameState.playerHasAttacks = HasPlayerAttacks(battleManager.playerController);
            gameState.playerHasDefenses = HasPlayerDefenses(battleManager.playerController);
            gameState.playerAttackThreat = CalculatePlayerAttackThreat(battleManager.playerController);
        }
        
        Debug.Log($"Estado do jogo analisado:");
        Debug.Log($"- Vida: {gameState.healthPercentage:P} (Precisa cura: {gameState.needsHealing})");
        Debug.Log($"- Slots defesa vazios: {gameState.emptyDefenseSlots}");
        Debug.Log($"- Mesa de ataque vazia: {gameState.attackTableEmpty}");
        Debug.Log($"- Jogador tem ataques: {gameState.playerHasAttacks}");
        Debug.Log($"- Ameaça do jogador: {gameState.playerAttackThreat}");
        
        return gameState;
    }
    
    private List<CardData> PlanTurnCards(GameState gameState)
    {
        var plannedCards = new List<CardData>();
        
        // Obter cartas disponíveis na mão (FanLayout) em vez do deck
        var availableCards = GetCardsInHand();
        
        Debug.Log("=== PLANEJAMENTO DE CARTAS ===");
        Debug.Log($"🃏 Cartas disponíveis na mão: {availableCards.Count}");
        Debug.Log($"🎯 Máximo de cartas por turno: {maxCardsPerTurn}");
        
        // Mostrar todas as cartas disponíveis na mão
        Debug.Log("📋 Cartas na mão:");
        for (int i = 0; i < availableCards.Count; i++)
        {
            Debug.Log($"  {i + 1}. {availableCards[i].displayName} ({availableCards[i].type}) - Power: {availableCards[i].power}, Defense: {availableCards[i].defense}");
        }
        
        // PRIORIDADE 1: Cura crítica
        if (gameState.needsHealing && ShouldUseHealing())
        {
            Debug.Log("🩺 VERIFICANDO PRIORIDADE 1: Cura crítica necessária");
            var healingCard = FindCardOfType(availableCards, CardType.Health);
            if (healingCard != null)
            {
                plannedCards.Add(healingCard);
                availableCards.Remove(healingCard);
                Debug.Log($"✅ PRIORIDADE 1: Cura crítica - {healingCard.displayName} ADICIONADA");
            }
            else
            {
                Debug.Log("❌ PRIORIDADE 1: Nenhuma carta de cura disponível");
            }
        }
        else
        {
            Debug.Log("ℹ️ PRIORIDADE 1: Cura não necessária (vida > 20%)");
        }
        
        // PRIORIDADE 2: Defesa contra ameaças OU defesa preventiva
        if (gameState.emptyDefenseSlots > 0 && plannedCards.Count < maxCardsPerTurn)
        {
            if (gameState.playerAttackThreat > 0)
            {
                Debug.Log($"🛡️ VERIFICANDO PRIORIDADE 2: Defesa contra ameaça de {gameState.playerAttackThreat}");
            }
            else
            {
                Debug.Log($"🛡️ VERIFICANDO PRIORIDADE 2: Defesa preventiva (sem ameaça atual)");
            }
            
            var defenseCards = FindCardsOfType(availableCards, CardType.Defense);
            Debug.Log($"🛡️ Cartas de defesa disponíveis: {defenseCards.Count}");
            
            // Se há ameaça, colocar múltiplas defesas. Se não há, colocar pelo menos 1
            int defensesNeeded;
            if (gameState.playerAttackThreat > 0)
            {
                defensesNeeded = Mathf.Min(gameState.emptyDefenseSlots, defenseCards.Count, maxCardsPerTurn - plannedCards.Count);
            }
            else
            {
                // Defesa preventiva: pelo menos 1 carta se houver slot vazio
                defensesNeeded = Mathf.Min(1, gameState.emptyDefenseSlots, defenseCards.Count, maxCardsPerTurn - plannedCards.Count);
            }
            
            Debug.Log($"🛡️ Defesas necessárias: {defensesNeeded} (slots vazios: {gameState.emptyDefenseSlots}, disponíveis: {defenseCards.Count}, espaço restante: {maxCardsPerTurn - plannedCards.Count})");
            
            for (int i = 0; i < defensesNeeded; i++)
            {
                var bestDefense = FindBestDefenseCard(defenseCards, gameState.playerAttackThreat);
                if (bestDefense != null)
                {
                    plannedCards.Add(bestDefense);
                    availableCards.Remove(bestDefense);
                    defenseCards.Remove(bestDefense);
                    Debug.Log($"✅ PRIORIDADE 2: Defesa - {bestDefense.displayName} ADICIONADA");
                }
            }
        }
        else
        {
            Debug.Log($"ℹ️ PRIORIDADE 2: Defesa não possível (slots vazios: {gameState.emptyDefenseSlots}, espaço restante: {maxCardsPerTurn - plannedCards.Count})");
        }
        
        // PRIORIDADE 3: Ataque
        if (gameState.attackTableEmpty && plannedCards.Count < maxCardsPerTurn)
        {
            Debug.Log("⚔️ VERIFICANDO PRIORIDADE 3: Ataque");
            var attackCard = FindBestAttackCard(availableCards);
            if (attackCard != null)
            {
                plannedCards.Add(attackCard);
                availableCards.Remove(attackCard);
                Debug.Log($"✅ PRIORIDADE 3: Ataque - {attackCard.displayName} ADICIONADA");
            }
            else
            {
                Debug.Log("❌ PRIORIDADE 3: Nenhuma carta de ataque disponível");
            }
        }
        else
        {
            Debug.Log($"ℹ️ PRIORIDADE 3: Ataque não necessário (mesa vazia: {gameState.attackTableEmpty}, espaço restante: {maxCardsPerTurn - plannedCards.Count})");
        }
        
        // PRIORIDADE 4: Jogar qualquer carta disponível (garantir que sempre jogue algo)
        if (plannedCards.Count == 0 && availableCards.Count > 0)
        {
            Debug.Log("🎲 PRIORIDADE 4: Nenhuma carta estratégica selecionada - jogando qualquer carta disponível");
            
            // Tentar jogar qualquer carta que possa ser colocada
            var anyUsableCard = FindAnyUsableCard(availableCards, gameState);
            if (anyUsableCard != null)
            {
                plannedCards.Add(anyUsableCard);
                availableCards.Remove(anyUsableCard);
                Debug.Log($"✅ PRIORIDADE 4: Carta genérica - {anyUsableCard.displayName} ({anyUsableCard.type}) ADICIONADA");
            }
            else
            {
                Debug.LogWarning("❌ PRIORIDADE 4: Nenhuma carta pode ser jogada no estado atual");
            }
        }
        
        // PRIORIDADE 5: Preencher turno com cartas adicionais se houver espaço
        // while (plannedCards.Count < maxCardsPerTurn && availableCards.Count > 0)
        // {
        //     var additionalCard = FindAnyUsableCard(availableCards, gameState);
        //     if (additionalCard != null)
        //     {
        //         plannedCards.Add(additionalCard);
        //         availableCards.Remove(additionalCard);
        //         Debug.Log($"✅ PRIORIDADE 5: Carta adicional - {additionalCard.displayName} ({additionalCard.type}) ADICIONADA");
        //     }
        //     else
        //     {
        //         Debug.Log("ℹ️ PRIORIDADE 5: Não há mais cartas utilizáveis");
        //         break;
        //     }
        // }
        
        Debug.Log($"🎯 PLANO FINALIZADO: {plannedCards.Count} cartas selecionadas de {availableCards.Count} disponíveis na mão");
        
        if (plannedCards.Count == 0)
        {
            Debug.LogWarning("⚠️ NENHUMA carta foi selecionada para o plano!");
        }
        else
        {
            Debug.Log($"🎉 Plano criado com sucesso: {plannedCards.Count} cartas para jogar");
        }
        
        return plannedCards;
    }
    
    private IEnumerator ExecuteTurnPlan(List<CardData> plannedCards)
    {
        Debug.Log($"=== EXECUTANDO PLANO DE TURNO ({plannedCards.Count} CARTAS) ===");
        
        int successfulPlays = 0;
        
        for (int i = 0; i < plannedCards.Count; i++)
        {
            var cardToPlay = plannedCards[i];
            Debug.Log($"🎴 Executando carta {i + 1}/{plannedCards.Count}: {cardToPlay.displayName} ({cardToPlay.type})");
            
            // Encontrar a carta na mão (FanLayout) em vez de spawnar nova
            var cardInHand = FindCardInHand(cardToPlay);
            if (cardInHand != null)
            {
                Debug.Log($"✅ Carta {cardToPlay.displayName} encontrada na mão - colocando estrategicamente");
                yield return StartCoroutine(PlaceCardStrategically(cardInHand));
                successfulPlays++;
                
                Debug.Log($"✅ Carta {cardToPlay.displayName} colocada com sucesso!");
                yield return new WaitForSeconds(actionDelay);
            }
            else
            {
                Debug.LogError($"❌ FALHA ao encontrar carta {cardToPlay.displayName} na mão");
            }
            
            // Log de progresso
            Debug.Log($"📊 Progresso: {i + 1}/{plannedCards.Count} cartas processadas");
        }
        
        Debug.Log($"=== PLANO DE TURNO CONCLUÍDO ===");
        Debug.Log($"📈 Resultado: {successfulPlays}/{plannedCards.Count} cartas jogadas com sucesso");
        
        if (successfulPlays == 0)
        {
            Debug.LogWarning("⚠️ NENHUMA carta foi jogada com sucesso neste turno!");
        }
        else if (successfulPlays < plannedCards.Count)
        {
            Debug.LogWarning($"⚠️ Apenas {successfulPlays} de {plannedCards.Count} cartas foram jogadas");
        }
        else
        {
            Debug.Log("🎉 TODAS as cartas planejadas foram jogadas com sucesso!");
        }
    }
    
    // Métodos auxiliares de análise
    private int CountEmptyDefenseSlots()
    {
        int empty = 0;
        foreach (var slot in defenses)
        {
            if (slot.currentCardController == null)
                empty++;
        }
        return empty;
    }
    
    private bool HasPlayerAttacks(PlayerController player)
    {
        if (player.corner.currentCardController?.data.type == CardType.Attack) return true;
        if (player.attackTable.currentCardController?.data.type == CardType.Attack) return true;
        return false;
    }
    
    private bool HasPlayerDefenses(PlayerController player)
    {
        foreach (var defense in player.defenses)
        {
            if (defense.currentCardController?.data.type == CardType.Defense)
                return true;
        }
        return false;
    }
    
    private int CalculatePlayerAttackThreat(PlayerController player)
    {
        int threat = 0;
        if (player.corner.currentCardController?.data.type == CardType.Attack)
            threat += player.corner.currentCardController.power;
        if (player.attackTable.currentCardController?.data.type == CardType.Attack)
            threat += player.attackTable.currentCardController.power;
        return threat;
    }
    
    private CardData FindCardOfType(List<CardData> cards, CardType type)
    {
        return cards.FirstOrDefault(c => c.type == type);
    }
    
    private List<CardData> FindCardsOfType(List<CardData> cards, CardType type)
    {
        return cards.Where(c => c.type == type).ToList();
    }
    
    private CardData FindBestDefenseCard(List<CardData> defenseCards, int threatLevel)
    {
        // Encontrar defesa com poder adequado para a ameaça
        return defenseCards.OrderByDescending(c => c.defense).FirstOrDefault();
    }
    
    private CardData FindBestAttackCard(List<CardData> availableCards)
    {
        var attackCards = FindCardsOfType(availableCards, CardType.Attack);
        return attackCards.OrderByDescending(c => c.power).FirstOrDefault();
    }
    
    private CardData FindAnyUsableCard(List<CardData> availableCards, GameState gameState)
    {
        Debug.Log($"🔍 Procurando qualquer carta utilizável entre {availableCards.Count} disponíveis");
        
        // Prioridade 1: Cartas de defesa se há slots vazios
        if (gameState.emptyDefenseSlots > 0)
        {
            var defenseCards = FindCardsOfType(availableCards, CardType.Defense);
            if (defenseCards.Count > 0)
            {
                var bestDefense = defenseCards.OrderByDescending(c => c.defense).First();
                Debug.Log($"🛡️ Carta de defesa encontrada: {bestDefense.displayName}");
                return bestDefense;
            }
        }
        
        // Prioridade 2: Cartas de ataque se mesa está vazia
        if (gameState.attackTableEmpty)
        {
            var attackCards = FindCardsOfType(availableCards, CardType.Attack);
            if (attackCards.Count > 0)
            {
                var bestAttack = attackCards.OrderByDescending(c => c.power).First();
                Debug.Log($"⚔️ Carta de ataque encontrada: {bestAttack.displayName}");
                return bestAttack;
            }
        }
        
        // Prioridade 3: Cartas de cura se mesa está vazia (mesmo sem necessidade crítica)
        if (gameState.attackTableEmpty)
        {
            var healingCards = FindCardsOfType(availableCards, CardType.Health);
            if (healingCards.Count > 0)
            {
                var bestHealing = healingCards.OrderByDescending(c => c.power).First();
                Debug.Log($"🩺 Carta de cura encontrada: {bestHealing.displayName}");
                return bestHealing;
            }
        }
        
        // Prioridade 4: Qualquer carta que possa substituir uma mais fraca
        // Verificar se pode substituir carta de ataque na mesa
        if (!gameState.attackTableEmpty && attackTable.currentCardController != null)
        {
            var currentAttackPower = attackTable.currentCardController.power;
            var betterAttacks = FindCardsOfType(availableCards, CardType.Attack)
                .Where(c => c.power > currentAttackPower).ToList();
            
            if (betterAttacks.Count > 0)
            {
                var bestReplacement = betterAttacks.OrderByDescending(c => c.power).First();
                Debug.Log($"🔄 Carta de ataque melhor encontrada para substituição: {bestReplacement.displayName} (atual: {currentAttackPower}, nova: {bestReplacement.power})");
                return bestReplacement;
            }
        }
        
        Debug.Log("❌ Nenhuma carta utilizável encontrada");
        return null;
    }
    
    private List<CardData> GetCardsInHand()
    {
        var cardsInHand = new List<CardData>();
        
        if (fanLayout == null)
        {
            Debug.LogError("FanLayout não está configurado no AdversaryController");
            return cardsInHand;
        }
        
        // Obter todas as cartas no fanLayout
        var cardTransforms = fanLayout.GetCardsInLayout();
        
        Debug.Log($"🃏 Verificando {cardTransforms.Count} cartas no FanLayout");
        
        foreach (var cardTransform in cardTransforms)
        {
            var cardController = cardTransform.GetComponent<CardController>();
            if (cardController != null && cardController.data != null)
            {
                cardsInHand.Add(cardController.data);
                Debug.Log($"  ✅ Carta na mão: {cardController.data.displayName} ({cardController.data.type})");
            }
            else
            {
                Debug.LogWarning($"  ❌ CardController ou data não encontrado em carta do FanLayout");
            }
        }
        
        Debug.Log($"🃏 Total de cartas válidas na mão: {cardsInHand.Count}");
        return cardsInHand;
    }
    
    private CardController FindCardInHand(CardData targetCard)
    {
        if (fanLayout == null)
        {
            Debug.LogError("FanLayout não está configurado no AdversaryController");
            return null;
        }
        
        // Obter todas as cartas no fanLayout
        var cardTransforms = fanLayout.GetCardsInLayout();
        
        Debug.Log($"🔍 Procurando carta {targetCard.displayName} entre {cardTransforms.Count} cartas na mão");
        
        foreach (var cardTransform in cardTransforms)
        {
            var cardController = cardTransform.GetComponent<CardController>();
            if (cardController != null && cardController.data != null)
            {
                if (cardController.data.id == targetCard.id || 
                    cardController.data.displayName == targetCard.displayName)
                {
                    Debug.Log($"✅ Carta {targetCard.displayName} encontrada na mão!");
                    
                    // Remover carta do FanLayout antes de usar
                    fanLayout.RemoveCard(cardTransform);
                    
                    return cardController;
                }
            }
        }
        
        Debug.LogError($"❌ Carta {targetCard.displayName} NÃO encontrada na mão!");
        return null;
    }
}
