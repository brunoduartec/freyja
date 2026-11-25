# Especificação do estudo de iluminação

**Branch** — `033-estudo-iluminacao`  
**Data** — 2026-11-18  
**Estado** — Aprovada para implementação  
**Pedido** — Criar um ciclo de dia e noite e um céu bonito de observar, com nuvens e sol.

## Cenários e testes

### P1 — Olhar para o céu durante a caminhada

Quem joga pode levantar a câmera para acompanhar o sol e as nuvens. O céu segue a direção low poly pixelada de Freyja, com nuvens de contornos bem definidos e movimento lento. A proposta vai do dourado suave do amanhecer ao azul claro do dia, passa por um pôr do sol quente e chega a uma noite azul profunda.

1. No cais e na praça, ao olhar para cima durante o dia, é possível enquadrar o disco solar e reconhecer as formas de nuvens em movimento.
2. Ao voltar a câmera para o vilarejo, a direção da luz e das sombras é coerente com a posição do sol.

### P2 — Ver a ilha mudar com a hora

Em uma caminhada na cena principal, o jogador percebe a passagem do tempo pela cor do céu, pela direção e intensidade da luz e pelo comprimento das sombras.

1. Ao acompanhar uma volta completa, são visíveis amanhecer, dia, pôr do sol e noite, nessa ordem, com retorno gradual ao amanhecer.
2. À noite, o caminho e os pontos de interação do trecho avaliado continuam reconhecíveis na câmera de jogo.

### Casos de borda

- A passagem da meia-noite preserva cor, brilho e movimento das nuvens sem salto perceptível.
- Próximo ao horizonte, o sol some gradualmente enquanto céu, cenário e sombras mantêm uma transição coerente.

## Requisitos funcionais

- **FR-001** — Executar um ciclo visual contínuo e repetível na cena principal `Ilha_Tripo_Caminhada_011`, perceptível durante a caminhada.
- **FR-002** — Exibir sol visível durante o dia e nuvens estilizadas que cruzam o céu suavemente e continuam discerníveis à noite.
- **FR-003** — Sincronizar céu, iluminação e sombras com a hora, preservando a leitura do caminho e das interações.
- **FR-004** — Permitir avaliar horários específicos e assistir ao ciclo em ritmo acelerado durante a revisão do estudo.

## Critérios de sucesso

- **CS-001** — Um vídeo na câmera de jogo mostra uma volta completa em até 2 minutos, com as quatro fases e a passagem para o ciclo seguinte.
- **CS-002** — Quatro capturas do mesmo enquadramento, uma por fase, permitem comparar céu e iluminação no terreno. O sol aparece em pelo menos uma captura diurna e as nuvens são discerníveis nas quatro.
- **CS-003** — Uma caminhada entre o cais e a praça em pleno dia e à noite permite reconhecer o caminho e os pontos de interação existentes ao longo do trecho.

## Premissas

- A paleta e o ritmo acelerado são propostas para avaliação visual. A duração do dia na campanha fica para uma decisão posterior.
- O escopo desta entrega é o estudo visual do ciclo em uma cena. O vínculo entre amanhecer, pôr do sol e as regras de possessão do GDD fica para uma etapa futura.
