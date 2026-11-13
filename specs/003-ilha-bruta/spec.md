# Spec 003 — Ilha bruta caminhável

**Criada:** 12/11/2026 · **Estado:** implementação e validação técnica realizadas; aguardando avaliação do usuário.

Entregas: [cena Unity](../../Freyja/Assets/_Project/Scenes/Ilha_Bruta_003.unity), [fonte Blender](../../Source/Blender/Ilha_Bruta_003.blend), [guia de avaliação](quickstart.md) e [evidências do percurso](evidence/percurso.md). O teste dos controles foi sintético; a avaliação visual e dos controles físicos permanece pendente.

## Cenário e teste

### Explorar a ilha em construção (P1)

Quero caminhar pela ilha inteira em uma versão bruta para avaliar seu formato, relevo, distâncias e disposição do vilarejo antes da construção das casas.

**Teste independente:** começar no cais e percorrer os principais pontos da ilha, passando pelos blocos que representam as casas.

**Aceitação:**
1. A ilha tem costa, relevo e caminhos; o cais existente se conecta ao terreno.
2. É possível caminhar do cais ao largo e chegar às casas baixas, casas altas, casas do leste e ao mirante, retornando ao cais.
3. As 12 casas previstas no GDD são representadas por blocos grandes, distribuídos nas três áreas, sem casas detalhadas ou interiores.
4. O jogador consegue avaliar os caminhos e a escala com o personagem e a câmera já existentes.

## Requisitos

- **FR-001:** criar o modelo bruto da ilha inteira, editável no Blender e jogável na Unity.
- **FR-002:** usar como referência aproximada os 260 × 180 m de extensão e os 24 m de altura máxima previstos no GDD; ajustar conforme o teste de navegação.
- **FR-003:** representar a enseada sul, cais, caminho principal, largo, encosta, rota pelos fundos, hortas, ruína e mirante norte com formas ou marcações simples.
- **FR-004:** posicionar 12 blocos para as casas: quatro nas casas baixas, quatro nas casas altas e quatro no leste. Venda, sala comunitária e galpão podem ser indicados por volumes simples para avaliar sua localização.
- **FR-005:** permitir caminhar pelas áreas acessíveis e entre os pontos principais, com colisões e sem atravessar o terreno ou ficar preso no percurso.
- **FR-006:** manter os controles e a recuperação após queda da entrega anterior; a nova cena deve preservar a cena jogável já existente.
- **FR-007:** entregar o arquivo editável da ilha e uma cena pronta para avaliação do percurso.

**Casos de borda:** terrenos íngremes e limites da ilha não podem prender o jogador; uma queda no mar deve permitir voltar ao ponto inicial. Não é necessário escalar todos os costões.

## Critérios de sucesso

- **SC-001:** completar, sem travamento, um percurso do cais ao largo, às três áreas de casas e ao mirante, retornando ao cais.
- **SC-002:** contar os 12 blocos de casas e reconhecer sua distribuição nas três áreas.
- **SC-003:** abrir a fonte no Blender e jogar a nova cena na Unity para avaliar a escala e os caminhos.

## Premissas e limites

Esta é uma maquete de navegação: medidas e formato serão avaliados na prática. Terreno, blocos e marcadores são provisórios e devem poder ser ajustados depois.

**Fora do escopo:** casas detalhadas, interiores, moradores, diálogos, pistas, animações finais e acabamento visual definitivo. O encontro com Artur fica no backlog.

**Planejamento:** [plan.md](plan.md) e [tarefas](tasks.md), gerados por solicitações explícitas do usuário. A ilha ainda não foi implementada.
