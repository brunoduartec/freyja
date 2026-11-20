# Especificação — Relevo de toda a ilha

**Branch** `037-relevo-ilha`  
**Criada em** 18/11/2026  
**Estado** Implementado e aguardando avaliação visual

## Cenário de uso e teste

### P1 — Ler o relevo ao percorrer a vila

Estender ao mapa inteiro a direção do [piloto da encosta oeste](../036-relevo-encosta/spec.md). O jogador deve conseguir ler como a costa, a subida e os lotes se encaixam a partir do chão. A [vista atual](vista-atual.png) registra onde ainda há fileiras de pedras e onde a costa se aproxima dos caminhos. A [referência da vila](../017-referencia-terreno-tripo/vila-casa-isolada.png) orienta as relações entre praia, encostas, lotes e crista.

| Setor | Resultado esperado |
| --- | --- |
| Costa oeste | Continuar o talude do piloto até a casa isolada, ajustando a transição à medida que o terreno sobe. Manter o acesso e o apoio da casa no alto. |
| Costa leste | Dar continuidade à descida até a água, com trechos mais baixos perto da enseada e mais altos junto aos lotes elevados. Reservar a faixa útil do caminho onde ele se aproxima da borda. |
| Norte e crista | Manter a leitura da parte alta. Definir um ombro junto à trilha superior e uma descida mais marcada para o mar. Preservar a chegada ao mirante e a leitura da igreja como marco. |
| Enseada sul e cais | Manter a areia livre. Ajustar as pontas da praia e as junções com os taludes laterais, preservando o encaixe entre píer, galpão e subida. |
| Taludes internos | Integrar ao chão os cortes entre caminhos e lotes. Identificar patamares onde sustentam entradas, pátios ou hortas existentes. |

### Aceite em jogo

1. Ao percorrer os eixos do cais à praça e da praça à igreja, os caminhos e os patamares continuam legíveis. A trilha alta, a casa isolada e os acessos leste permanecem alcançáveis.
2. Nas vistas da costa e da crista, o relevo é legível com a fileira repetida de pedras oculta. A linha da água, o espaço de areia e o apoio dos lotes aparecem claramente.

## Requisitos

- **FR-001** — Revisar o conjunto da ilha setor a setor, conectando as transições na costa e nas encostas internas. Variar a inclinação conforme as cotas e o espaço até os caminhos e lotes.
- **FR-002** — Ocultar de modo reversível as fileiras repetidas de pedras costeiras e da crista no estudo. Avaliar a forma com grama e terra exposta. A eventual volta de pedras em pontos específicos depende da revisão visual.
- **FR-003** — Proteger o traçado, a largura útil e as cotas de chegada dos caminhos, as entradas e os apoios das construções. Preservar o perímetro útil de areia e a ligação entre o cais e a terra.

## Critérios de sucesso

- **SC-001** — Entregar um par de vistas aéreas no mesmo enquadramento e pares ao nível do jogador para a subida do cais, a costa leste e a trilha alta. Comparar perfis representativos oeste, leste e norte e conferir a transição para a praia.
- **SC-002** — Completar em jogo os percursos cais–praça, praça–igreja e as ligações à trilha alta, à casa isolada e à costa leste. Conferir as entradas existentes junto aos trechos ajustados, sem bloqueios novos.

## Premissas

- Toda a ilha entra na revisão. As mudanças de forma se concentram nas transições que precisam acompanhar a costa, os caminhos e os lotes.
- O piloto oeste é o ponto de partida. A extensão fica pronta para uma avaliação conjunta da ilha antes de decidir pedras pontuais.
