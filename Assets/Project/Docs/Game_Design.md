## BoxerCard — Documento de Mecânicas e Cartas

### Visão geral
BoxerCard é um card game de boxe em turnos. Cada lado (Jogador e Adversário) pilota um `Boxer` com vida inicial definida em `BoxerData.health`. O objetivo é reduzir a vida do oponente a 0.

- **Vida inicial**: definida por `BoxerData` do boxer selecionado.
- **Condição de vitória**: um lado atinge vida ≤ 0.
- **Cartas**: quatro tipos funcionais — Ataque, Defesa, Cura (Health) e Especial — além da zona utilitária Corner.

### Estrutura de turno e fases
O fluxo de batalha é controlado por `BattleManager`.

- **Turno atual**: `GameTurn` (Player | Adversary).
- **Fases** (`BattlePhase`):
  - Setup: preparação do turno (spawn de carta do topo do deck, se houver).
  - Action: o lado ativo joga cartas (arrastar/soltar).
  - Combat: fase prevista para resolução em lote; hoje a resolução principal ocorre no momento do drop na Mesa de Ataque (ver abaixo).
  - Cleanup: limpeza e efeitos residuais (p.ex., cura de cartas no Corner).

Observação: atualmente, ataques e curas acontecem imediatamente ao dropar na Mesa de Ataque (animações + aplicação de efeitos), e a fase Combat é utilizada apenas como etapa de transição/feedback.

### Zonas do tabuleiro (UIDropZone)
Cada zona valida o tipo de carta aceito e possui comportamento próprio:

- **Defenses (DropZoneType.Defense)**
  - Aceita: somente cartas de Defesa.
  - Persistem no campo e interagem com o primeiro ataque recebido.

- **Attack Table (DropZoneType.AttackTable)**
  - Aceita: Ataque, Cura, Especial. (Não aceita Defesa.)
  - Efeitos executados imediatamente ao drop (ver Regras de Resolução).
  - Após resolver, a carta é consumida e removida do deck/campo.

- **Corner (DropZoneType.Corner)**
  - Aceita: qualquer tipo (sem restrição). Na prática, usada para manter carta de Cura para aplicar efeito no Cleanup.
  - Cartas colocadas aqui permanecem até removidas/substituídas.

### Fluxo de compra e mão
- No início da partida, cada lado gera 5 cartas da lista inicial (`DeckController.IE_StarchMatch`).
- No começo do turno, se houver cartas no deck atual, 1 carta é comprada automaticamente.
- O Jogador arrasta cartas (drag-and-drop) enquanto `DragStatus.canDrag` estiver habilitado (habilita no turno do jogador e desabilita ao finalizar o turno).

### Regras de resolução (quando e como os efeitos acontecem)
Resoluções principais ocorrem no drop na Mesa de Ataque, com animações e efeitos imediatos. Além disso, há resolução de cura em Cleanup para cartas no Corner.

- **Ataque (Attack)** — Drop na Mesa de Ataque executa ataque imediato:
  1. O ataque mira somente a **primeira** carta de Defesa do oponente, se existir; caso contrário, acerta vida diretamente.
  2. Resolução contra a primeira Defesa encontrada:
     - Se `attackPower > defenseValue` da carta de Defesa: a Defesa é destruída e o dano que passa é igual ao `attackPower` (dano total).
     - Se `defenseValue >= attackPower`: compara-se `defensePower` vs `attackPower`:
       - `defensePower > attackPower`: a Defesa absorve todo o dano; a defesa da carta é setada para `attackPower` (para feedback visual) e não passa dano.
       - `attackPower > defensePower`: passa dano residual `attackPower - defensePower`; a defesa da carta é setada para `attackPower`.
       - `attackPower == defensePower`: a Defesa absorve o ataque e é destruída; não passa dano.
  3. Se sobrar dano após a interação com a Defesa, aplica-se ao `health` do oponente (respeitando `canDoDamage`).
  4. A carta de Ataque usada é consumida e removida.

  Modificadores e variações de ataque (novos):
  - Body Blow: ignora 50% da defesa do primeiro alvo.
  - Feint: não causa dano; aplica debuff (−50% de defesa) na próxima Defesa inimiga.
  - Counter Punch: dano dobrado se o atacante foi atingido no último turno.
  - Flurry: executa 3 mini-ataques de 3 (aplica 9 no total).
  - Finisher: causa dano total apenas se o alvo estiver com ≤ 30% do HP máx; caso contrário, aplica dano reduzido (50%).
  - Precision (buff): ignora a primeira Defesa deste turno (ver Especiais).

- **Cura (Health)**
  - Drop na Mesa de Ataque: cura imediata o dono em `power`, até `maxHealth` do boxer; a carta é consumida e removida.
  - Drop no Corner: a cura é aplicada no **Cleanup**; a carta permanece no Corner e pode curar novamente a cada Cleanup enquanto estiver lá.

- **Defesa (Defense)**
  - Só pode ser colocada em `Defenses`.
  - Fica persistente no campo e interage com o próximo ataque recebido conforme as regras acima.

- **Especiais (Special)** — Drop na Mesa de Ataque executa imediatamente e consome a carta:
  - Extra Cards: compra imediatamente `power` cartas (ex.: 2).
  - Focus: a próxima carta jogada tem o dobro de poder.
  - Adrenaline Rush: todos os Ataques ganham +4 de poder neste turno.
  - Stun: o oponente perde o próximo turno.
  - Break Guard: destrói todas as cartas de Defesa do oponente.
  - Overcharge: +8 de poder neste turno; sofre 5 de dano no fim do turno.
  - Mirror Guard: copia a última Defesa usada pelo inimigo para um slot de Defesa livre.
  - Precision: ignora completamente a primeira Defesa do oponente neste turno.
  - Second Wind: restaura 25% do HP máx e compra 1 carta.
  - Extra Damage (placeholder): desativa dano do alvo até trocar o turno.
  - Shield (placeholder): desativa dano recebido até trocar o turno.

### IA do Adversário
O Adversário planeja o turno com base no estado do jogo:

- Analisa: vida própria (< 20% ativa priorização de cura), slots de Defesa vazios, mesa de ataque livre, presença/ameaça de ataques do jogador.
- Prioridades típicas:
  1) Cura crítica (se < 20% de vida e houver carta de Cura);
  2) Preencher Defesas (mais de uma se houver ameaça);
  3) Colocar um Ataque na mesa se estiver livre;
  4) Fallback: jogar qualquer carta utilizável (ou descartar se não couber/for útil);
- Posicionamento: cartas são retiradas da mão (FanLayout) e movidas para as zonas via animação. Substituições podem ocorrer se a nova carta for mais forte.

### Decks, dados e carregamento de cartas
- `CardsManager` carrega todos os `CardData` em `Assets/Project/ScriptableObjects/Cards` e resolve cartas por `id`.
- `BoxerData` define `health` e a lista inicial de cartas (`List<string> cards` com ids).
- `PlayerData` atualizado para testes: inclui ataques base e novos ("Cross", "Body Blow", "Counter Punch", "Flurry", "Feint", "Finisher"), além de especiais ("Focus", "Adrenaline Rush", "Stun", "Break Guard", "Overcharge", "Mirror Guard", "Precision", "Second Wind") e curas/defesas padrão.

### Catálogo de cartas (estado atual dos assets)

#### Ataque
- Jab — Poder: 4
- Hoock — Poder: 6
- Upper — Poder: 15
- Cross — Poder: 10 (golpe direto)
- Body Blow — Poder: 8 (ignora 50% da defesa do primeiro alvo)
- Counter Punch — Poder: 6 (dano dobrado se foi atacado no último turno)
- Flurry — Poder base: 3 (3×3 no total)
- Feint — Poder: 0 (reduz a próxima Defesa inimiga em 50%)
- Finisher — Poder: 18 (dano total somente se alvo ≤ 30% HP)

#### Defesa (Power/Defense)
- Block — 8 / 8
- S.Block — 4 / 8
- Dodge — 15 / 12

#### Cura
- Small Health — Cura: 5
- Health — Cura: 9
- Big Health — Cura: 14

#### Especiais
- Extra Cards — Compra imediata: `power` (atualmente 2)
- Focus — Próxima carta tem o dobro de poder
- Adrenaline Rush — +4 poder em todos os Ataques neste turno
- Stun — Oponente perde o próximo turno
- Break Guard — Destrói todas as Defesas do oponente
- Overcharge — +8 poder neste turno; sofre 5 de dano no fim
- Mirror Guard — Copia a última Defesa do inimigo para seu campo
- Precision — Ignora a primeira Defesa neste turno
- Second Wind — Cura 25% do HP máx e compra 1 carta
- Extra Damage — Placeholder (bloqueia dano do alvo até trocar turno)
- Shield — Placeholder (bloqueia dano recebido até trocar turno)

Notas:
- Alguns nomes de display podem divergir dos `id` (p.ex., Small/Health/Big Health). O que importa para o deck é o `id`.
- O asset "Extra Cards" tem `displayName` com quebras de linha; visual pode precisar revisão na UI.

### Condições de fim de jogo
- Se a vida do Jogador chega a 0 → "Adversário venceu!" e a batalha encerra.
- Se a vida do Adversário chega a 0 → "Jogador venceu!" e a batalha encerra.

### Observações e oportunidades de melhoria
- Fase Combat pode futuramente consolidar a resolução (em vez de resolver no drop).
- Especiais (Extra Damage, Shield, Destroy Defense) estão como placeholders desativando dano; alinhar com design desejado.
- Regra de alvo: ataques consideram apenas a primeira carta de Defesa disponível; pode-se expandir para múltiplas defesas ou priorização.
- Corner aceita qualquer tipo de carta; hoje é usado como slot utilitário (cura persistente). Se quiser limitar, ajustar `UIDropZone.CanAcceptDrop`.

---
Este documento reflete o comportamento atual do código e dos assets em `Assets/Project/ScriptableObjects/Cards`. Qualquer alteração nos scripts/ScriptableObjects deve ser refletida aqui.


