# Especificação do lagarto rosa

**Branch** `026-lagarta-rosa`

**Data** 17/11/2026

**Status** Aprovada em 17/11/2026, com correção do autor para lagarto rosa

**Pedido** Gerar a ilustração e depois o modelo no Tripo. Correção do autor: “Eu quero um lagarto só que cor de rosa”.

## Cenários e verificação

### P1 — Avaliar a ilustração

O autor abre uma imagem em PNG e vê um único lagarto rosa de corpo inteiro, em vista de três quartos. A ilustração deve deixar visíveis a cabeça, as quatro patas e a cauda, com fundo neutro e sem elementos que escondam a silhueta. Ao reduzir a imagem a uma miniatura, ainda é possível reconhecer o lagarto e sua cor dominante.

### P2 — Inspecionar o modelo 3D

O autor abre a versão texturizada em GLB, gerada pelo Tripo a partir do PNG, e gira o modelo para inspecionar frente, laterais, costas e parte inferior. O modelo deve preservar a cor rosa dominante e a anatomia de lagarto da referência. A entrega inclui uma prévia do modelo para avaliação.

### Casos de revisão

- Se o fundo ou a pose ocultar partes do corpo na ilustração, corrigir a imagem antes de usá-la como referência.
- Se o modelo apresentar geometria de fundo, textura ausente ou perda das patas ou da cauda, corrigir o resultado e reinspecioná-lo antes da entrega.

## Requisitos

- **FR-001** — Entregar um PNG que sirva como ilustração e como referência de geração do modelo.
- **FR-002** — Gerar o modelo no Tripo usando o PNG entregue e fornecer um GLB texturizado.
- **FR-003** — Inspecionar o modelo no Blender e manter uma fonte editável em `.blend`. Salvar os arquivos sob `TripoAi/lagarto-rosa/` e `Source/Blender/Lagarto_Rosa/` na cópia de trabalho da feature.
- **FR-004** — Limitar esta entrega aos artefatos visuais. Uso em cena, escala definitiva, animação e regras de interação ficam para outra entrega.

## Critérios de sucesso

- **SC-001** — Um PNG abre e mostra o lagarto inteiro com rosa dominante, cabeça, quatro patas e cauda identificáveis.
- **SC-002** — Um GLB e um arquivo editável abrem com a textura visível. Na inspeção em quatro ângulos, o modelo mantém a silhueta de lagarto e apresenta somente a criatura.
- **SC-003** — A entrega inclui uma prévia do modelo e os caminhos dos três arquivos.

## Premissas

- A correção do autor pede um lagarto rosa como artefato visual. A função narrativa dessa criatura fica para decisão no GDD.
- A proposta usa anatomia de lagarto de corpo alongado, quatro patas baixas, cauda afilada e volumes facetados, em consonância com o estilo low poly de Freyja. Rosa é a cor dominante.
