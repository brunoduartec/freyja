# UV e textura da ilha do TripoAI

**Criada em**: 2026-11-13  
**Estado**: Primeira versão entregue em 2026-11-13; aguardando avaliação visual  
**Entrada**: Abrir `TripoAi/island.glb` no Blender, fazer UV mapping e criar a textura.

## User Scenarios & Testing

### User Story 1 — Avaliar a ilha texturizada (Priority: P1)

Como responsável pelo jogo, quero ver a ilha com superfícies identificáveis e editáveis, seguindo a [referência usada na geração](../../Referencias/ilha-relevo-tripoai.png).

**Why priority**: O modelo veio sem UVs nem imagens de textura.

**Independent Test**: Comparar a entrega com a referência em vista aérea e em detalhes do chão e das encostas.

**Acceptance Scenarios**:

1. **Given** o modelo original, **When** abrir a entrega, **Then** reconhecer o mesmo contorno e relevo, com grama verde amarelada, rocha cinza terrosa, terra nos caminhos e areia na enseada.
2. **Given** a ilha texturizada, **When** inspecionar chão, costa e encostas, **Then** encontrar textura sem lacunas, distorções ou emendas visivelmente quebradas.
3. **Given** os arquivos entregues, **When** reabrir a fonte, **Then** carregar a textura sem arquivos ausentes e poder editar imagem e UVs.

### Edge Cases

- Verificar distorções na malha densa; redução de polígonos fica fora do escopo.
- Partes ocultas na referência recebem acabamento coerente com as áreas adjacentes.
- Caminhos pouco definidos seguem a geometria existente; divergências serão registradas.

## Requirements

### Functional Requirements

- **FR-001**: Preservar o GLB original e sua geometria, trabalhando em uma nova fonte editável.
- **FR-002**: Criar UVs para toda a malha, sem sobreposição involuntária de superfícies distintas.
- **FR-003**: Criar e aplicar textura de cor com grama, rocha, caminhos e areia distribuídos conforme a referência, em acabamento de baixa resolução compatível com o projeto.
- **FR-004**: Entregar fonte `.blend`, textura PNG, layout UV e prévias aéreas e de detalhe.
- **FR-005**: Reabrir a fonte e verificar carregamento da textura, cobertura, emendas e silhueta.

## Success Criteria

### Measurable Outcomes

- **SC-001**: 100% das faces possuem coordenadas de textura válidas.
- **SC-002**: Contorno e relevo permanecem iguais ao modelo importado, sem alteração da geometria.
- **SC-003**: Grama, rocha, terra e areia são distinguíveis nas prévias, sem lacunas ou emendas visivelmente quebradas.
- **SC-004**: A fonte reabre com textura carregada; fonte, imagem, layout e prévias estão em caminhos documentados.

## Assumptions

- A referência proposta é `Referencias/ilha-relevo-tripoai.png`.
- Inspeção: 290.322 vértices e 580.632 triângulos, sem UVs nem imagens. Modelo aberto no Blender para inspeção, sem salvar sobre o original.
- Escopo restrito ao acabamento do terreno. Não inclui retopologia, construções, vegetação adicional, colisões ou integração no Unity.
- Fonte e evidências ficarão em pasta própria em `Source/Blender/`; resolução será definida no plano.
- Após aprovação explícita desta spec, seguir planejamento e tarefas. A entrega termina na primeira versão texturizada para avaliação.
