# Especificação 056 — Passada de subida nas escadas

**Branch** `056-passada-subida-escadas`  
**Data** 2026-11-19  
**Status** Implementada, com captura para avaliação

## Cenários de uso e testes

### Subir com apoio visível (P1)

Na cena principal, o jogador sobe as escadas da L3 e da B3 usando os controles normais. O pé supera a quina antes de pousar, a sola encosta na superfície e o corpo avança sobre esse apoio enquanto o outro pé inicia a passada.

### Parar, retomar e chegar ao topo (P2)

O jogador pode parar no meio do lance, retomar e seguir pelo patamar. A passada retoma a partir do apoio existente. A transição para o piso plano e o retorno pela escada continuam transitáveis.

## Requisitos

- **FR-001** — Dar ao protagonista uma animação específica de subida que mostre elevação do pé, contato da sola e transferência de peso.
- **FR-002** — Adaptar o ponto e o ângulo de contato às superfícies reais da L3 e da B3. Durante o apoio, o pé permanece no ponto de contato até iniciar a próxima passada.
- **FR-003** — Adaptar a trajetória do sapato à quina de cada degrau e manter a passada sincronizada em duas velocidades de aproximação.
- **FR-004** — Preservar a resposta aos controles, a parada no lance e a transição para o patamar.

## Critérios de sucesso

- **SC-001** — Completar a subida da L3 e da B3 em duas velocidades de aproximação. Em cada percurso, revisar de lado e em câmera lenta todos os contatos até o patamar.
- **SC-002** — Nas capturas, conferir que a ponta do sapato libera cada quina, que a sola encontra a superfície no início de cada apoio e que o corpo avança sobre o apoio antes da próxima passada. Conferir sola, quina e pelve no mesmo enquadramento.
- **SC-003** — Em cada casa, parar no meio do lance, retomar, chegar ao patamar e voltar à base sem saltos ou reposicionamentos.

## Premissas

- A entrega visual abrange o protagonista e os dois lances jogáveis da cena principal. A descida integra o teste de regressão.
- A proposta usa a animação de subida como base da pose. A técnica de adaptação aos degraus será detalhada no planejamento.
