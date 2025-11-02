using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public enum DropZoneType
{
    Defense,
    Corner,
    AttackTable
}

[RequireComponent(typeof(RectTransform))]
public class UIDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public FanLayout fanLayout;
    public Image backgroundImage;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 0.8f);
    [SerializeField] private Color acceptColor = new Color(0.5f, 1f, 0.5f, 0.8f);
    [SerializeField] private Color deniedColor = new Color(0.5f, 1f, 0.5f, 0.8f);
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Drop Zone Settings")] 
    public DropZoneType dropZoneType;
    [SerializeField] private string acceptedTag = "Card";
    [SerializeField] private bool destroyOnDrop = false;
    [SerializeField] private Vector2 snapOffset = Vector2.zero;
    [SerializeField] private bool resetRotationOnDrop = true; 
    [SerializeField] private bool centerCardOnDrop = true; 
    
    [Header("Effects")]
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulseSpeed = 2f;
    
    [Header("Attack Animation")]
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float attackScale = 1.2f;
    [SerializeField] private float attackSpeed = 0.3f;
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shakeIntensity = 2f;

    public bool isPlayer;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isHighlighted = false;
    private Tween pulseTween;

    public BattleManager battleManager;
    public UIDragHandler currentUIDragHandler;
    public CardController currentCardController;
    public System.Action<GameObject> OnCardDropped;
    
    public void SetResetRotationOnDrop(bool reset) => resetRotationOnDrop = reset;
    public void SetCenterCardOnDrop(bool center) => centerCardOnDrop = center;
    public void SetSnapOffset(Vector2 offset) => snapOffset = offset;
    public bool GetResetRotationOnDrop() => resetRotationOnDrop;
    public bool GetCenterCardOnDrop() => centerCardOnDrop;
    public Vector2 GetSnapOffset() => snapOffset;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        battleManager = FindFirstObjectByType<BattleManager>();
        
        if (backgroundImage != null) return;
        backgroundImage = gameObject.AddComponent<Image>();
        backgroundImage.color = normalColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject == null) return;
        
        if (CanAcceptDrop(droppedObject))
            HighlightDropZone(true);
        else
            ShowDeniedEffect(true);
    }
    
    public void OnPointerExit(PointerEventData eventData) => HighlightDropZone(false);

    public void OnDrop(PointerEventData eventData) { }

    private bool CanAcceptDrop(GameObject draggedObject)
    {
        if (draggedObject == null) return false;
        
        var carController = draggedObject.GetComponent<CardController>();
        if (carController.isPlayer != isPlayer && dropZoneType != DropZoneType.AttackTable) return false;
        
        switch (dropZoneType)
        {
            case DropZoneType.Defense:
                if (carController.data.type != CardType.Defense)
                    return false;
                break;
                
            case DropZoneType.AttackTable:
                if (carController.data.type == CardType.Defense)
                    return false;
                break;
                
            case DropZoneType.Corner:

                break;
        }
        
        if (!string.IsNullOrEmpty(acceptedTag))
        {
            return draggedObject.CompareTag(acceptedTag);
        }
        
        return draggedObject.GetComponent<UIDragHandler>() != null;
    }
    
    public void HandleDrop(GameObject droppedObject)
    {
        if (!CanAcceptDrop(droppedObject)) return;

        HighlightDropZone(false);
        droppedObject.transform.SetAsLastSibling();
        
        RectTransform droppedRect = droppedObject.GetComponent<RectTransform>();
        var carController = droppedObject.GetComponent<CardController>();
        var dragHandler = droppedObject.GetComponent<UIDragHandler>();
        
        if (carController == null) return;
        
        if (currentCardController != null && currentUIDragHandler != null)
            ReturnCardToDeck(currentUIDragHandler);
        
        ShowAcceptEffect();

        if (dragHandler)
        {
            dragHandler.onDeck = false;
            
            if (centerCardOnDrop)
                dragHandler.transform.position = transform.position;
                
            if (resetRotationOnDrop)
                dragHandler.transform.localRotation = Quaternion.identity;
        }
        
        droppedRect.DOKill();
        
        // Manter escala invertida para cartas do adversário
        Vector3 targetScale = (carController != null && !carController.isPlayer) 
            ? new Vector3(-1, -1, 1) 
            : Vector3.one;
        
        droppedRect.DOScale(targetScale, animationDuration).SetEase(Ease.OutQuad);
                
        Image cardImage = droppedObject.GetComponent<Image>();
        if (cardImage != null)
            cardImage.DOColor(Color.white, animationDuration);
        
        OnCardDropped?.Invoke(droppedObject);
        
        if (destroyOnDrop)
            Destroy(droppedObject, animationDuration + 0.1f);

        currentCardController = carController;
        currentUIDragHandler = dragHandler;
        if (dragHandler)
            dragHandler.currentUiDropZone = this;
        
        // Registrar última defesa jogada para Mirror Guard
        if (dropZoneType == DropZoneType.Defense && carController != null)
            battleManager.SetLastDefensePlayed(isPlayer, carController.data);

        // Se é uma carta dropada na mesa de ataque, executar ação baseada no tipo
        if (dropZoneType == DropZoneType.AttackTable)
        {
            Debug.Log($"=== CARTA DROPADA NA MESA DE ATAQUE ===");
            Debug.Log($"Carta: {carController.data.displayName}");
            Debug.Log($"Tipo: {carController.data.type}");
            Debug.Log($"É do jogador: {carController.isPlayer}");
            Debug.Log($"Zona é do jogador: {isPlayer}");

            switch (carController.data.type)
            {
                case CardType.Attack:
                    Debug.Log("Executando ataque imediato...");
                    StartCoroutine(ExecuteImmediateAttack(carController));
                    break;
                case CardType.Health:
                    Debug.Log("Executando cura imediata...");
                    StartCoroutine(ExecuteImmediateHealing(carController));
                    break;
                case CardType.Special:
                    Debug.Log("Executando Especial imediata...");
                    StartCoroutine(ExecuteImmediateSpecial(carController));
                    break;
                default:
                    Debug.LogWarning($"Tipo de carta não reconhecido para ação imediata: {carController.data.type}");
                    break;
            }
        }
        else
        {
            Debug.Log($"Carta dropada em zona tipo: {dropZoneType} (não é mesa de ataque)");
        }
        
        if(carController.isPlayer) GameManager.Instance.currentCardUse++;
        
        Debug.Log($"Card {droppedObject.name} foi solto na drop zone {gameObject.name} - Posição: {droppedRect?.anchoredPosition}, Rotação resetada: {resetRotationOnDrop}");
    }
    
    private void HighlightDropZone(bool highlight)
    {
        isHighlighted = highlight;
        pulseTween?.Kill();

        if (highlight)
        {
            backgroundImage.DOColor(highlightColor, animationDuration);
            
            pulseTween = rectTransform.DOScale(Vector3.one * pulseScale, 1f / pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            backgroundImage.DOColor(normalColor, animationDuration);
            rectTransform.DOScale(Vector3.one, animationDuration);
        }
    }
    
    private void ShowAcceptEffect()
    {
        backgroundImage.DOColor(acceptColor, animationDuration * 0.5f)
            .OnComplete(() => {
                backgroundImage.DOColor(normalColor, animationDuration * 0.5f);
            });
        
        rectTransform.DOPunchScale(Vector3.one * 0.1f, animationDuration, 5, 0.5f);
    }
    
    private void ReturnCardToDeck(UIDragHandler cardDragHandler)
    {
        if (cardDragHandler == null) return;
        
        cardDragHandler.onDeck = true;
        cardDragHandler.currentUiDropZone = null;
        
        var rectCard = cardDragHandler.GetComponent<RectTransform>();
        if (rectCard != null && cardDragHandler.fanLayout != null)
        {
            var cardController = cardDragHandler.GetComponent<CardController>();
            cardDragHandler.fanLayout.AddCard(rectCard,cardController, cardDragHandler.originalSiblingIndex);
        }
        
        Debug.Log($"Card {cardDragHandler.gameObject.name} retornou para o deck da dropzone {gameObject.name}");
    }
    
    private void ShowDeniedEffect(bool highlight)
    {
        isHighlighted = highlight;
        pulseTween?.Kill();

        if (highlight)
        {
            backgroundImage.DOColor(deniedColor, animationDuration);
            
            pulseTween = rectTransform.DOScale(Vector3.one * pulseScale, 1f / pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            backgroundImage.DOColor(normalColor, animationDuration);
            rectTransform.DOScale(Vector3.one, animationDuration);
        }
    }
    
    private void OnDestroy()
    {
        if (pulseTween != null)
            pulseTween.Kill();
    }
    
    private IEnumerator PlayAttackAnimation(GameObject attackCard, IEnumerator endAttack)
    {
        yield return new WaitForSeconds(attackDelay);
        
        var cardTransform = attackCard.GetComponent<RectTransform>();
        var originalPosition = cardTransform.anchoredPosition;
        var originalScale = cardTransform.localScale;
        
        var targets = FindAttackTargets();
        
        if (targets.Count > 0)
        {
            foreach (var target in targets)
            {
                yield return StartCoroutine(AnimateAttackToTarget(cardTransform, target, originalPosition, originalScale,endAttack));
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            yield return StartCoroutine(AnimateAttackToAdversary(cardTransform, originalPosition, originalScale,endAttack));
        }
    }
    
    private List<Transform> FindAttackTargets()
    {
        var targets = new System.Collections.Generic.List<Transform>();

        // Determinar quem é o adversário baseado no isPlayer da CARTA, não da zona
        var cardController = currentCardController;
        if (cardController == null) return targets;
        
        UIDropZone[] enemyDefenses = null;
        
        // Se a carta é do jogador, atacar defesas do adversário
        // Se a carta é do adversário, atacar defesas do jogador
        if (cardController.isPlayer)
        {
            enemyDefenses = battleManager.adversaryController.defenses;
            Debug.Log("Carta do jogador atacando defesas do adversário");
        }
        else
        {
            enemyDefenses = battleManager.playerController.defenses;
            Debug.Log("Carta do adversário atacando defesas do jogador");
        }
        
        // Adicionar APENAS A PRIMEIRA defesa ocupada como alvo (não todas)
        if (enemyDefenses != null)
        {
            foreach (var defenseZone in enemyDefenses)
            {
                if (defenseZone.currentCardController != null && 
                    defenseZone.currentCardController.data.type == CardType.Defense)
                {
                    targets.Add(defenseZone.currentCardController.transform);
                    Debug.Log($"Alvo de animação: {defenseZone.currentCardController.data.displayName}");
                    break; // APENAS o primeiro alvo para animação
                }
            }
        }
        
        Debug.Log($"Alvos de animação encontrados: {targets.Count}");
        return targets;
    }
    
    private IEnumerator AnimateAttackToTarget(RectTransform attacker, Transform target, Vector2 originalPos, Vector3 originalScale, IEnumerator endAttack)
    {
        // Calcular posição do alvo
        var targetPos = target.GetComponent<RectTransform>().anchoredPosition;
        
        // Sequência de animação de ataque
        var sequence = DOTween.Sequence();
        
        // 1. Preparar ataque (escalar e inclinar)
        sequence.Append(attacker.DOScale(originalScale * attackScale, 0.15f))
                .Join(attacker.DORotate(new Vector3(0, 0, -10f), 0.15f));
        
        // 2. Atacar (mover rapidamente para o alvo)
        sequence.Append(attacker.DOAnchorPos(targetPos, attackSpeed).SetEase(Ease.InQuad))
                .Join(attacker.DORotate(new Vector3(0, 0, 15f), attackSpeed));
        
        // 3. Impacto (shake e flash)
        sequence.AppendCallback(() => {
            // Shake no alvo
            target.GetComponent<RectTransform>().DOShakePosition(0.2f, 10f);
            
            // Flash de impacto
            var targetImage = target.GetComponent<Image>();
            if (targetImage != null)
            {
                targetImage.DOColor(Color.red, 0.1f).OnComplete(() => {
                    targetImage.DOColor(Color.white, 0.1f);
                });
            }
        });
        
        //yield return new WaitForSeconds(0.3f);
        StartCoroutine(endAttack);
        
        // 4. Retornar à posição original
        sequence.Append(attacker.DOAnchorPos(originalPos, 0.4f).SetEase(Ease.OutBack))
                .Join(attacker.DOScale(originalScale, 0.4f))
                .Join(attacker.DORotate(Vector3.zero, 0.4f));
        
        yield return sequence.WaitForCompletion();
    }
    
    private IEnumerator AnimateAttackToAdversary(RectTransform attacker, Vector2 originalPos, Vector3 originalScale, IEnumerator endAttack)
    {
        var targetPos = isPlayer ? new Vector2(300, 0) : new Vector2(-300, 0);
        
        var sequence = DOTween.Sequence();
        
        sequence.Append(attacker.DOScale(originalScale * 1.3f, 0.2f))
                .Join(attacker.DORotate(new Vector3(0, 0, -15f), 0.2f));
        
        sequence.Append(attacker.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.InQuad))
                .Join(attacker.DORotate(new Vector3(0, 0, 20f), 0.4f));
        
        sequence.AppendCallback(() => {
            if (enableScreenShake && Camera.main != null)
            {
                Camera.main.transform.DOShakePosition(0.3f, shakeIntensity);
            }
        });
        
       StartCoroutine(endAttack);
       
        sequence.Append(attacker.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutBack))
                .Join(attacker.DOScale(originalScale, 0.5f))
                .Join(attacker.DORotate(Vector3.zero, 0.5f));
        
        yield return sequence.WaitForCompletion();
    }
    
    private IEnumerator ExecuteImmediateAttack(CardController attackCard)
    {
        // Flurry: executa 3 animações de ataque separadas
        if (attackCard.data.id == "Flurry")
        {
            battleManager.battleEvents.OnBattleMessage?.Invoke("Flurry: 3 golpes rápidos!");
            yield return new WaitForSeconds(0.5f);
            
            for (int i = 0; i < 3; i++)
            {
                yield return StartCoroutine(PlayAttackAnimation(attackCard.gameObject, ProcessFlurryHit(attackCard, i + 1)));
                yield return new WaitForSeconds(0.2f); // Pequena pausa entre os hits
            }
            yield return StartCoroutine(RemoveCard(attackCard));
        }
        else
        {
            yield return StartCoroutine(PlayAttackAnimation(attackCard.gameObject,ProcessImmediateAttackDamage(attackCard)));
            yield return StartCoroutine(RemoveCard(attackCard));
        }
    }
    
    private IEnumerator ProcessImmediateAttackDamage(CardController attackCard)
    {
        DeckController target = null;
        string attackerName = "";
        
        if (attackCard.isPlayer)
        {
            target = battleManager.adversaryController;
            attackerName = "Jogador";
        }
        else
        {
            target = battleManager.playerController;
            attackerName = "Adversário";
        }

        yield return StartCoroutine(ProcessAttackLogic(attackCard, target, attackerName));
    }
    
    private IEnumerator ProcessFlurryHit(CardController attackCard, int hitNumber)
    {
        DeckController target = null;
        string attackerName = "";
        
        if (attackCard.isPlayer)
        {
            target = battleManager.adversaryController;
            attackerName = "Jogador";
        }
        else
        {
            target = battleManager.playerController;
            attackerName = "Adversário";
        }
        
        // Cada hit do Flurry causa 3 de dano
        int flurryDamage = 3;
        
        // Aplica buffs apenas no primeiro hit
        if (hitNumber == 1)
        {
            var attackerState = battleManager.GetState(attackCard.isPlayer);
            
            // Focus: dobra o poder da PRÓXIMA carta
            if (attackerState.focusDoubleNextCard)
            {
                flurryDamage *= 2;
                attackerState.focusDoubleNextCard = false;
            }
            
            // AdrenalineRush/Overcharge: bônus cumulativo no turno
            flurryDamage += attackerState.attackPowerBonus;
        }
        
        battleManager.battleEvents.OnBattleMessage?.Invoke($"{attackerName} - Flurry golpe {hitNumber}!");
        yield return new WaitForSeconds(0.3f);
        
        // Aplica o dano diretamente
        if (flurryDamage > 0)
        {
            if(!target.canDoDamage) yield break;
            target.health = Mathf.Max(0, target.health - flurryDamage);
            battleManager.battleEvents.OnBattleMessage?.Invoke($"{target.name} recebe {flurryDamage} de dano! Vida: {target.health}");
            
            if (target == battleManager.playerController)
            {
                battleManager.battleEvents.OnPlayerHealthChanged?.Invoke(target.health);
            }
            else
            {
                battleManager.battleEvents.OnAdversaryHealthChanged?.Invoke(target.health);
            }
            
            // Marca que o alvo recebeu dano (para Counter Punch no próximo turno dele)
            if (hitNumber == 1)
            {
                battleManager.MarkDamageReceived(target == battleManager.playerController);
            }
        }
    }
    
    private IEnumerator ProcessAttackLogic(CardController attackCard, DeckController target, string attackerName)
    {
        int attackPower = attackCard.power;
        int remainingDamage = attackPower;
        
        // Buffs/efeitos de lado
        var attackerState = battleManager.GetState(attackCard.isPlayer);
        var defenderState = battleManager.GetState(!attackCard.isPlayer);
        
        // Focus: dobra o poder da PRÓXIMA carta
        if (attackerState.focusDoubleNextCard)
        {
            attackPower *= 2;
            attackerState.focusDoubleNextCard = false;
        }
        
        // AdrenalineRush/Overcharge: bônus cumulativo no turno
        attackPower += attackerState.attackPowerBonus;
        
        // Notificar sobre o ataque
        battleManager.battleEvents.OnBattleMessage?.Invoke($"{attackerName} ataca com {attackCard.data.displayName} (Poder: {attackPower})!");
        yield return new WaitForSeconds(0.5f);

        // FEINT: não causa dano; aplica debuff na próxima defesa do oponente
        if (attackCard.data.id == "Feint")
        {
            battleManager.ApplyFeintToOpponent(attackCard.isPlayer);
            remainingDamage = 0;
        }
        
        // Obter zonas de defesa do alvo
        UIDropZone[] targetDefenses = null;
        if (target == battleManager.playerController)
            targetDefenses = battleManager.playerController.defenses;
        else if (target == battleManager.adversaryController)
            targetDefenses = battleManager.adversaryController.defenses;
        
        // Precision: ignorar primeira defesa
        bool ignoreFirstDefense = attackerState.precisionIgnoreFirstDefenseThisTurn;

        // Encontrar APENAS A PRIMEIRA carta de defesa para atacar
        UIDropZone firstDefenseZone = null;
        CardController firstDefenseCard = null;
        
        if (targetDefenses != null)
        {
            foreach (var defenseZone in targetDefenses)
            {
                if (defenseZone.currentCardController != null && 
                    defenseZone.currentCardController.data.type == CardType.Defense)
                {
                    if (ignoreFirstDefense)
                    {
                        ignoreFirstDefense = false; // Ignora esta e segue buscando próxima
                        continue;
                    }
                    firstDefenseZone = defenseZone;
                    firstDefenseCard = defenseZone.currentCardController;
                    Debug.Log($"🎯 Primeira defesa considerada: {firstDefenseCard.data.displayName}");
                    break; // PARAR na primeira defesa considerada
                }
            }
        }
        
        // Atacar APENAS a primeira defesa encontrada
        if (firstDefenseCard != null && firstDefenseZone != null)
        {
            int currentAttackPower = remainingDamage;
            int defensePower = firstDefenseCard.power;
            int defenseValue = firstDefenseCard.defense;
            
            // Feint: metade da próxima defesa do oponente
            if (defenderState.halveNextDefenseOnce)
            {
                defenseValue = Mathf.CeilToInt(defenseValue * 0.5f);
                defenderState.halveNextDefenseOnce = false;
                battleManager.battleEvents.OnBattleMessage?.Invoke("FEINT ativo: defesa reduzida pela metade");
            }
            
            // Body Blow: ignora 50% da defesa
            if (attackCard.data.id == "Body Blow")
            {
                int ignored = Mathf.FloorToInt(defenseValue * 0.5f);
                defenseValue -= ignored;
                battleManager.battleEvents.OnBattleMessage?.Invoke($"Body Blow ignora {ignored} de defesa");
            }
            
            Debug.Log($"=== COMBATE 1v1 ===");
            Debug.Log($"Atacante: {attackCard.data.displayName} (Power: {currentAttackPower})");
            Debug.Log($"Defensor: {firstDefenseCard.data.displayName} (Power: {defensePower}, Defesa: {defenseValue})");
            
            // REGRA PRINCIPAL: Se Power do Ataque > Defesa da Carta → Carta é DESTRUÍDA
            if (currentAttackPower > defenseValue)
            {
                Debug.Log($"💥 DESTRUIÇÃO: Ataque supera defesa da carta ({currentAttackPower} > {defenseValue})");
                
                // Carta de defesa é destruída, dano total do power do ataque passa
                remainingDamage = currentAttackPower;
                
                battleManager.battleEvents.OnBattleMessage?.Invoke($"{firstDefenseCard.data.displayName} foi destruída! Dano total: {currentAttackPower}");
                Debug.Log($"✗ {firstDefenseCard.data.displayName} DESTRUÍDA! Dano total: {currentAttackPower}");
                
                StartCoroutine(DestroyDefenseCard(firstDefenseCard, firstDefenseZone));
            }
            // Se Power do Ataque <= Defesa da Carta → Carta SOBREVIVE
            else
            {
                Debug.Log($"🛡️ SOBREVIVÊNCIA: Defesa da carta resiste ({defenseValue} >= {currentAttackPower})");
                
                // Agora comparar powers para determinar o resultado
                if (defensePower > currentAttackPower)
                {
                    Debug.Log($"CASO A: Defesa mais forte ({defensePower} > {currentAttackPower})");
                    
                    // A defesa da carta vira o power do ataque
                    firstDefenseCard.defense = currentAttackPower;
                    remainingDamage = 0; // Todo o dano foi absorvido
                    
                    battleManager.battleEvents.OnBattleMessage?.Invoke($"{firstDefenseCard.data.displayName} absorve o ataque! Nova defesa: {currentAttackPower}");
                    Debug.Log($"✓ {firstDefenseCard.data.displayName} absorveu ataque - Nova defesa: {currentAttackPower}");
                }
                else if (currentAttackPower > defensePower)
                {
                    Debug.Log($"CASO B: Ataque mais forte ({currentAttackPower} > {defensePower})");
                    
                    // Dano = power do ataque - power da defesa
                    int damageToPlayer = currentAttackPower - defensePower;
                    firstDefenseCard.defense = currentAttackPower;
                    
                    battleManager.battleEvents.OnBattleMessage?.Invoke($"{firstDefenseCard.data.displayName} resiste parcialmente! Dano: {damageToPlayer}");
                    Debug.Log($"✓ {firstDefenseCard.data.displayName} resiste - Nova defesa: {currentAttackPower}, Dano causado: {damageToPlayer}");
                    
                    remainingDamage = damageToPlayer;
                }
                else
                {
                    Debug.Log($"CASO C: Powers iguais ({currentAttackPower} = {defensePower})");
                    
                    firstDefenseCard.defense = currentAttackPower;
                    remainingDamage = 0;
                    
                    battleManager.battleEvents.OnBattleMessage?.Invoke($"{firstDefenseCard.data.displayName} absorve ataque igual! Nova defesa: {currentAttackPower}");
                    Debug.Log($"✓ {firstDefenseCard.data.displayName} absorveu ataque igual - Nova defesa: {currentAttackPower}");
                    StartCoroutine(DestroyDefenseCard(firstDefenseCard, firstDefenseZone));
                }
                
                // Atualizar visual da carta que sobreviveu
                yield return StartCoroutine(UpdateDefenseCardVisual(firstDefenseCard));
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.Log("🎯 Nenhuma carta de defesa encontrada para atacar");
        }
        
        // Efeitos pós-interação com defesa
        // Counter Punch: dobra dano se sofreu ataque no último turno
        if (attackCard.data.id == "Counter Punch" && attackerState.wasAttackedLastTurn)
        {
            remainingDamage *= 2;
            attackerState.wasAttackedLastTurn = false; // Consome o estado
            battleManager.battleEvents.OnBattleMessage?.Invoke("Counter Punch: dano dobrado!");
        }

        // Finisher: só causa dano total se alvo <= 30% HP
        if (attackCard.data.id == "Finisher")
        {
            float threshold = target.maxHealth * 0.3f;
            if (target.health > threshold)
            {
                remainingDamage = Mathf.CeilToInt(remainingDamage * 0.5f); // Penaliza quando fora da condição
                battleManager.battleEvents.OnBattleMessage?.Invoke("Finisher penalizado: alvo acima de 30% de HP");
            }
        }

        if (remainingDamage > 0)
        {
            if(!target.canDoDamage) yield break;
            target.health = Mathf.Max(0, target.health - remainingDamage);
            battleManager.battleEvents.OnBattleMessage?.Invoke($"{target.name} recebe {remainingDamage} de dano! Vida: {target.health}");
            
            if (target == battleManager.playerController)
            {
                battleManager.battleEvents.OnPlayerHealthChanged?.Invoke(target.health);
            }
            else
            {
                battleManager.battleEvents.OnAdversaryHealthChanged?.Invoke(target.health);
            }
            
            // Marca que o alvo recebeu dano (para Counter Punch no próximo turno dele)
            battleManager.MarkDamageReceived(target == battleManager.playerController);

            // Impacto forte quando o adversário recebe dano alto
            if (target == battleManager.adversaryController && remainingDamage > 10)
            {
                battleManager.PlayHeavyHitEffectsOnAdversary();
            }

            // Verificar e finalizar o jogo imediatamente se algum lado chegou a 0 de vida
            if (battleManager != null && battleManager.TryEndGameImmediate())
            {
                yield break;
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            battleManager.battleEvents.OnBattleMessage?.Invoke("Ataque foi completamente bloqueado pelas defesas!");
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator ExecuteImmediateHealing(CardController healingCard)
    {
        Debug.Log($"=== EXECUTANDO CURA IMEDIATA ===");
        Debug.Log($"Carta: {healingCard.data.displayName}");
        Debug.Log($"Poder de cura: {healingCard.power}");
        Debug.Log($"É do jogador: {healingCard.isPlayer}");
        Debug.Log($"Zona é do jogador: {isPlayer}");
        
        // 1. Pequena pausa após o drop
        yield return new WaitForSeconds(0.5f);
        
        // 2. Animação de cura
        Debug.Log("Iniciando animação de cura...");
        yield return StartCoroutine(PlayHealingAnimation(healingCard.gameObject));
        
        // 3. Aplicar cura imediatamente
        Debug.Log("Processando cura...");
        yield return StartCoroutine(ProcessImmediateHealing(healingCard));
        
        // 4. Remover carta após a cura
        Debug.Log("Removendo carta de cura...");
        yield return StartCoroutine(RemoveCard(healingCard));
        
        Debug.Log("=== CURA CONCLUÍDA ===");
    }

    private IEnumerator PlayHealingAnimation(GameObject healingCard)
    {
        var cardTransform = healingCard.GetComponent<RectTransform>();
        if (cardTransform == null) yield break;
        
        var originalScale = cardTransform.localScale;
        var originalPosition = cardTransform.anchoredPosition;
        
        // Sequência de animação de cura
        var sequence = DOTween.Sequence();
        
        // 1. Pulsar com brilho
        sequence.Append(cardTransform.DOScale(originalScale * 1.3f, 0.3f).SetEase(Ease.OutQuad));
        sequence.Join(cardTransform.DOPunchPosition(Vector3.up * 20f, 0.3f, 5, 0.5f));
        
        // 2. Efeito de brilho na carta
        var cardImage = healingCard.GetComponent<Image>();
        if (cardImage != null)
        {
            sequence.Join(cardImage.DOColor(Color.green, 0.2f).OnComplete(() => {
                cardImage.DOColor(Color.white, 0.2f);
            }));
        }
        
        // 3. Retornar ao tamanho original
        sequence.Append(cardTransform.DOScale(originalScale, 0.2f).SetEase(Ease.InQuad));
        
        yield return sequence.WaitForCompletion();
    }
    
    private IEnumerator ProcessImmediateHealing(CardController healingCard)
    {
        // Determinar quem será curado baseado no isPlayer
        DeckController target = null;
        string healerName = "";
        
        if (healingCard.isPlayer)
        {
            target = battleManager.playerController;
            healerName = "Jogador";
        }
        else
        {
            target = battleManager.adversaryController;
            healerName = "Adversário";
        }
        
        if (target == null) yield break;
        
        int healingAmount = healingCard.power;
        int oldHealth = target.health;
        target.health = Mathf.Min(target.maxHealth, target.health + healingAmount);
        int actualHealing = target.health - oldHealth;
        
        // Notificar sobre a cura
        battleManager.battleEvents.OnBattleMessage?.Invoke($"{healerName} se curou {actualHealing} pontos com {healingCard.data.displayName}!");
        
        // Atualizar interface
        if (target == battleManager.playerController)
        {
            battleManager.battleEvents.OnPlayerHealthChanged?.Invoke(target.health);
        }
        else
        {
            battleManager.battleEvents.OnAdversaryHealthChanged?.Invoke(target.health);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"{healerName} se curou {actualHealing} pontos. Vida: {oldHealth} → {target.health}");
    }

    private IEnumerator ExecuteImmediateSpecial(CardController specialCard)
    {
        Debug.Log($"Carta: {specialCard.data.displayName}");
        Debug.Log($"Poder de cura: {specialCard.power}");
        Debug.Log($"É do jogador: {specialCard.isPlayer}");
        Debug.Log($"Zona é do jogador: {isPlayer}");
        
        // 1. Pequena pausa após o drop
        yield return new WaitForSeconds(0.5f);
        
        // 2. Animação de cura
        Debug.Log("Iniciando animação de cura...");
        yield return StartCoroutine(PlaySpecialAnimation(specialCard));
        
        Debug.Log("Removendo carta de cura...");
        yield return StartCoroutine(RemoveCard(specialCard));
        
        Debug.Log("=== Uso do especial concluido ===");
    }
    
    private IEnumerator PlaySpecialAnimation(CardController specialCard)
    {
        var cardTransform = specialCard.GetComponent<RectTransform>();
        if (cardTransform == null) yield break;
        
        var originalScale = cardTransform.localScale;

        var sequence = DOTween.Sequence();
        
        sequence.Append(cardTransform.DOScale(originalScale * 1.3f, 0.3f).SetEase(Ease.OutQuad));
        sequence.Join(cardTransform.DOPunchPosition(Vector3.up * 20f, 0.3f, 5, 0.5f));
        
        sequence.Append(cardTransform.DOScale(originalScale, 0.2f).SetEase(Ease.InQuad));
        
        yield return sequence.WaitForCompletion();

        if (specialCard.cardSpecials != null)
        {
            specialCard.cardSpecials.UseSpecial();
        }
        else
        {
            Debug.LogWarning($"CardSpecials ausente em {specialCard.data.displayName}. Executando fallback do especial.");
            // Fallback mínimo para especiais críticos
            if (specialCard.data.special == SpecialType.SecondWind)
            {
                battleManager.ApplySecondWind(specialCard.isPlayer);
            }
        }
        yield return new WaitForSeconds(1);
    }
    
    private IEnumerator RemoveCard(CardController card)
    {
        if (card == null) yield break;
        
        currentCardController = null;
        currentUIDragHandler = null;
        
        var cardTransform = card.GetComponent<RectTransform>();
        var cardImage = card.GetComponent<Image>();
        
        if (cardTransform != null)
        {
            cardTransform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack);
            
            if (cardImage != null)
            {
                cardImage.DOFade(0f, 0.4f);
            }
            
            yield return new WaitForSeconds(0.4f);
        }
        
        RemoveCardFromOwnerDeck(card);
        
        if (card != null)
        {
            Destroy(card.gameObject);
        }
        
        Debug.Log($"Carta de cura {card.data.displayName} foi consumida e removida");
    }
    
    private IEnumerator DestroyDefenseCard(CardController defenseCard, UIDropZone defenseZone)
    {
        if (defenseCard == null || defenseZone == null) yield break;
        
        // Limpar referências da zona imediatamente
        defenseZone.currentCardController = null;
        defenseZone.currentUIDragHandler = null;
        
        // Animação de destruição
        var cardTransform = defenseCard.GetComponent<RectTransform>();
        var cardImage = defenseCard.GetComponent<Image>();
        
        if (cardTransform != null)
        {
            // Efeito de explosão/destruição
            cardTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            cardTransform.DOShakeRotation(0.3f, 30f);
            
            // Fade out vermelho (indicando destruição)
            if (cardImage != null)
            {
                cardImage.DOColor(Color.red, 0.2f).OnComplete(() => {
                    cardImage.DOFade(0f, 0.3f);
                });
            }
            
            // Escalar para zero após o efeito
            yield return new WaitForSeconds(0.2f);
            cardTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // Remover carta do deck antes de destruir
        RemoveCardFromOwnerDeck(defenseCard);
        
        // Destruir GameObject
        if (defenseCard != null)
        {
            Destroy(defenseCard.gameObject);
        }
        
        Debug.Log($"Carta de defesa {defenseCard.data.displayName} foi destruída pelo ataque");
    }
    
    private IEnumerator UpdateDefenseCardVisual(CardController defenseCard)
    {
        if (defenseCard == null) yield break;
        
        // Efeito visual de dano recebido
        var cardTransform = defenseCard.GetComponent<RectTransform>();
        var cardImage = defenseCard.GetComponent<Image>();
        
        if (cardTransform != null)
        {
            // Animação de "hit" - tremor e mudança de cor
            cardTransform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.3f);
            
            if (cardImage != null)
            {
                // Flash vermelho indicando dano
                var originalColor = cardImage.color;
                cardImage.DOColor(Color.red, 0.15f).OnComplete(() => {
                    cardImage.DOColor(originalColor, 0.15f);
                });
            }
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // Atualizar texto da carta se houver (defesa modificada)
        var cardView = defenseCard.cardView;
        if (cardView != null)
        {
            // Forçar atualização visual da carta com nova defesa
            cardView.UpdateCardVisuals();
        }
        
        Debug.Log($"Visual da carta {defenseCard.data.displayName} atualizado - Nova defesa: {defenseCard.defense}");
    }

    private void RemoveCardFromOwnerDeck(CardController card)
    {
        if (card.isPlayer)
        {
            battleManager.playerController.RemoveCardFromDeck(card.data);
        }
        else
        {
            battleManager.adversaryController.RemoveCardFromDeck(card.data);
        }
    }
}