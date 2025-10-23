## BoxerCard — Direção de Arte e Guias Visuais

### Identidade e Tema
- **Tema**: Boxe arcade, leve e vibrante. Sensação de impacto e agilidade.
- **Times/Cores**: Jogador em azul, Adversário em vermelho (ver `CardBoxerView`: `blueBG`/`redBG`).
- **Leitmotif visual**: Ícones simples e legíveis, cores sólidas e feedbacks com brilho/flash para hits e cura.

### Paleta de Cores (funcional)
Observação: As cores exatas de cartas são definidas via Inspector (`CardView`: `colorNormal`, `colorDefense`, `colorCorner`), mas o uso é padronizado:
- **Ataque (Attack)**: Fundo normal; realce com ícone de espada.
- **Defesa (Defense)**: Fundo em tom de defesa (ex.: verde/azulado), com ícone de escudo.
- **Cura (Health)**: Fundo normal com detalhes em verde; ícone de coração/kit médico.
- **Especial (Special)**: Fundo normal com destaque visual próprio.
- **DropZones**: Highlight amarelo suave no hover; verde no “aceito”; vermelho no “negado”. Efeito de pulsar sutil.

### Tipografia
Assets disponíveis em `Assets/Project/Art/Fonts`:
- **Lilita One**: títulos/logotipo, telas de abertura.
- **Orbitron**: títulos de UI/menus, botões principais.
- **Roboto Mono**: números e valores (vida, poder, defesa) para legibilidade consistente.

Regras:
- Nomes de carta em CAIXA ALTA (ver `CardView` `cardName.text = data.displayName.ToUpper();`).
- Manter contraste alto texto/fundo, strokes ou sombras leves para leitura sobre imagens.

### Iconografia e Mapeamento de Assets
Pasta base: `Assets/Project/Art`.

Cartas (ícones): `Art/Cards`
- Ataques: `jab_icon.png`, `hook_punch_icon.png`, `uppercut_icon.png`.
- Cura: `heal_light_kit_icon.png`, `heal_medium_kit_icon.png`, `heal_strong_kit_icon.png`.
- Comprar cartas: `draw_card_icon.png` (para “Extra Cards”).
- Defesa: `boxcard_defense_icon_7_barrier.png`, `boxcard_defense_icon_9_aura.png`, variações `block_variation_*.png`.

UI Genérica: `Art/UI`
- Ícones simples: `icon_sword_simple.png` (ataque), `icon_shield_simple.png` (defesa), `icon_heart_simple.png` (vida), `heart.png`.
- Backs/Views de carta: `UI/CardsBack`, `UI/CardsView`.

Logos/Branding: `Art/Logo`
- `game_logo_3.png`, `logo_boxdeck_v4_simple_transparent.png`, `LogoOneGear.png`.

Backgrounds:
- `BG.png`, `BG1.png`, `cartoon_background.png`, `simple_ring_audience_background.png`.

### Layout de Cartas
Componente: `CardView` (TextMeshPro + Images)
- Áreas: Nome (topo), Visual central (sprite quadrado ou 4:5), Stats (power/defense).
- Exibição por tipo:
  - Attack: `attackBG` ativo, `powerObj` visível.
  - Defense: `defenseBG` ativo, `powerObj` e `defenseObj` visíveis.
  - Health: `healthBG` ativo, foco em `power` (cura), `defenseObj` oculto.
  - Special: `specialBG` ativo; texto/ícone contextual.
- Diretrizes de tamanho (recomendação):
  - Ícone/visual da carta: 256–512 px (quadrado) para UI HD; PNG com fundo transparente.
  - Margens internas: manter safe area para textos (não tocar bordas); evitar overflow do `displayName`.

### HUD de Boxers
Componente: `CardBoxerView`
- Visual do boxer (sprite), nome em caixa alta, vida com `textHealth`.
- Feedback de dano: flash/vermelho e shake (`CardImpactAnimation.PlayDamageImpact`).
- Feedback de cura: flash/verde (`CardImpactAnimation.PlayHealingImpact`).
- Identidade de time: BG azul (player), vermelho (adversário).

### Animações e Feedback (VFX/UI)
Padrões implementados (DOTween):
- **Hover/DropZone**: highlight de cor + leve pulsar.
- **Drop Aceito**: flash verde e “punch scale”.
- **Drop Negado**: flash vermelho.
- **Ataque Imediato** (na Mesa de Ataque):
  - Escala, inclinação, deslocamento até o alvo.
  - Shake no alvo e flash vermelho.
  - Retorno à posição e consumo da carta.
- **Cura Imediata**: “punch scale”, tom esverdeado e retorno.
- **Defesa Atingida**: “punch scale”, flash vermelho, atualização de números.

Diretrizes visuais:
- Evitar exageros: durações curtas (0.1–0.5s) para manter responsividade.
- Usar easing suave (Ease.OutQuad/Ease.InOutSine) como padrão.

### Backgrounds e Composição
- Telas: usar `BG.png/BG1.png` ou `cartoon_background.png` com desfoque/overlay sutil.
- Batalha: `simple_ring_audience_background.png` para atmosfera de ringue.
- Overlays: gradientes leves em topo/rodapé para legibilidade do HUD.

### Pipeline de Arte (Unity Import)
- Tipo: Sprite (2D and UI).
- Pixels per Unit: 100 (padrão, ajustar se necessário para escala 1:1 na UI).
- Mesh Type: Tight.
- Filter Mode: Bilinear para UI geral, Point para pixel art.
- Compression: None (UI nítida); considerar Low qualidade em dispositivos alvo para otimização.
- Slices/Atlas (opcional): agrupar sprites de UI frequentes para reduzir draw calls.

### Convenções e Organização
- Pastas por categoria: `Art/Cards`, `Art/UI`, `Art/Logo`, `Art/Backgrounds`, `Art/Fonts`.
- Nome de arquivo descritivo do propósito (ex.: `jab_icon.png`, `cards_back_blue.png`).
- Exportar em PNG (transparência), evitar JPEG para UI.

### Checklist de Entrega de Assets
- Dimensões adequadas e múltiplos de 2 (256/512/1024) para texturas.
- Fundo transparente quando apropriado.
- Teste de leitura: textos e ícones legíveis em 1080p e 720p.
- Verificar contraste WCAG básico (botões e textos principais).
- Conferir pivôs/anchors de sprites que animam.

### Roadmap Visual (Sugerido)
- Definir paleta numérica (hex) para `Attack/Defense/Health/Special` e aplicar nos prefabs.
- Refinar ícones de especiais (Extra Damage, Shield, Destroy Defense) com linguagem única.
- Criar atlas para `UI/CardsView` e `UI/Base`.
- Criar variações de backs de carta por raridade/tipo.

—
Este guia reflete os assets atuais em `Assets/Project/Art` e os comportamentos visuais dos scripts (`CardView`, `CardBoxerView`, `UIDropZone`). Atualize conforme novos assets e requisitos surgirem.


