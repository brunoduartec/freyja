# Feature Specification: Edição manual da grama baked

**Feature Branch**: `084-editor-grama`
**Created**: 2026-11-26
**Status**: Approved for implementation
**Input**: Permitir que o usuário edite manualmente as posições de grama já baked, avaliando uma ferramenta própria ou integração com o editor de terreno.

## User Scenarios & Testing

### User Story 1 — Editar tufos pela Scene View (Priority: P1)

Como artista de ambiente, quero selecionar, adicionar, mover, girar e remover tufos baked diretamente sobre o terreno na Scene View, para corrigir a composição local da ilha sem regenerar toda a distribuição.

**Independent Test**: Em `Ilha_Principal`, selecionar um tufo, movê-lo para uma superfície válida, adicionar e remover outro tufo, salvar, fechar e reabrir a cena; as mudanças devem persistir e aparecer na câmera de jogo.

**Acceptance Scenarios**:

1. **Given** o layout baked 080 ativo, **When** o artista seleciona e move um tufo na Scene View, **Then** a posição nova acompanha a superfície válida e a mudança pode ser desfeita com Undo.
2. **Given** um ponto válido do terreno, **When** o artista pinta/adiciona um tufo, **Then** o tufo usa mesh/perfil existente, rotação e escala configuráveis e fica imediatamente visível no renderer instanciado.
3. **Given** um tufo selecionado, **When** o artista o remove, **Then** apenas essa colocação é removida e Undo a restaura.
4. **Given** alterações manuais salvas, **When** a ferramenta procedural de bake é executada, **Then** as edições autorais são preservadas ou o usuário recebe uma escolha explícita antes de substituí-las.
5. **Given** o Unity Editor aberto, **When** o usuário procura uma ferramenta para editar grama, **Then** encontra uma entrada de menu principal com nome direto, uma ação no componente/objeto `Coastal Grass — Ilha` e ajuda que indica como abrir e usar o painel.

### User Story 2 — Encontrar a ferramenta junto ao fluxo de terreno (Priority: P2)

Como artista, quero encontrar a ferramenta contextual na toolbar da Scene View quando seleciono o Terrain, para editar grama junto ao fluxo de composição do terreno.

**Independent Test**: Selecionar o Terrain, ativar `Editar Grama` na toolbar da Scene View, ajustar raio e densidade, pintar/apagar sobre terreno elegível e salvar o resultado persistente.

**Acceptance Scenarios**:

1. **Given** o Terrain selecionado na Scene View, **When** o usuário ativa `Editar Grama` na toolbar contextual, **Then** o raio e a densidade são ajustáveis e o pincel mostra sua área.
2. **Given** áreas inválidas como caminho, areia mascarada ou construção, **When** o usuário tenta pintar, **Then** a ferramenta bloqueia a colocação ou mostra claramente que o ponto não é elegível.

## Edge Cases

- O brush intersecta mais de uma célula baked ou a borda da ilha.
- A superfície clicada não é o Terrain elegível, é inclinada demais ou está coberta por máscara/exclusão.
- O bake procedural é solicitado após edição manual e há risco de substituir posições autorais.
- Undo/Redo atravessa adições, remoções e movimentações em mais de uma célula.
- O asset está ausente, incompatível com o layout fonte ou somente leitura.
- Muitas operações de pincel são feitas em sequência; a ferramenta precisa agrupar Undo sem reconstruir toda a ilha por evento.

## Requirements

### Functional Requirements

- **FR-001**: A ferramenta MUST expor colocações baked individuais como alvos selecionáveis/editáveis na Scene View sem criar um GameObject por tufo.
- **FR-002**: O usuário MUST poder adicionar, selecionar, mover, girar, alterar escala e remover tufos persistidos no layout renderizado.
- **FR-003**: A posição editada MUST ser projetada e validada contra terreno, máscaras de superfície, caminhos/acessos, construções e exclusões existentes.
- **FR-004**: Undo/Redo MUST restaurar operações de edição sem perder alterações não relacionadas da cena.
- **FR-005**: O bake procedural MUST preservar o conjunto manual ou apresentar uma confirmação/estratégia de merge antes de sobrescrevê-lo.
- **FR-006**: O layout editado MUST manter instancing, perfis/meshes LOD, lighting, vento e bounds corretos do renderer existente.
- **FR-007**: A ferramenta MUST apresentar controles mínimos de pincel: raio, densidade/espaçamento, mesh/perfil e escala/rotação aleatória; modo adicionar e remover.
- **FR-008**: O fluxo MUST indicar a persistência e permitir salvar/reabrir a cena sem deriva de posição ou duplicação de tufo.
- **FR-009**: A ferramenta MUST ser encontrável por uma entrada de menu principal do Unity com nome simples e buscável, uma ação contextual no objeto/componente de grama e uma ferramenta contextual na toolbar da Scene View quando o Terrain está selecionado.
- **FR-010**: O usuário MUST conseguir abrir o painel de edição em no máximo duas ações a partir do Unity Editor e ver uma instrução curta de uso/atalhos sem consultar documentação externa.

### Key Entities

- **Colocação manual**: raiz, yaw, escala e mesh/perfil de um tufo incorporado ao layout baked.
- **Pincel**: raio, espaçamento/densidade, perfil e modo de edição usados por uma passada.
- **Região baked**: célula espacial que agrega lotes GPU-instanced e seus bounds.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Um artista adiciona, move, remove e desfaz/refaz uma colocação pela Scene View; cada operação afeta somente a colocação/célula pertinente.
- **SC-002**: Todas as operações aplicadas em pontos elegíveis permanecem após salvar e reabrir; pontos inválidos exercitados (caminho, areia, encosta acima do limite e exclusão) resultam em zero novos tufos.
- **SC-003**: Executar o bake procedural após edição preserva os tufos manuais ou mostra aviso de confirmação com opção de cancelar; não há perda silenciosa.
- **SC-004**: A cena continua usando GPU instancing e LOD existente; o número de GameObjects não cresce com o número de tufos e não há GC por quadro atribuível à ferramenta em runtime.
- **SC-005**: A ferramenta permite editar tufos da densidade 080 e tufos authoring-base, mantendo dados e IDs coerentes em ambas as camadas.
- **SC-006**: Um usuário que não conhece a localização da ferramenta consegue encontrá-la pela busca de comandos do Unity ou pelo objeto `Coastal Grass — Ilha`, abri-la em até duas ações e iniciar uma edição após ler a instrução exibida no painel.

## Assumptions

- A edição é uma ferramenta de Unity Editor, usada em `Ilha_Principal` com Play Mode parado.
- Deve-se reutilizar o `CoastalGrassLayout` atribuído ao campo `layout` (atualmente `VegetacaoReassentada.asset`), consultar `AuthoredCoastalTufts078.asset` e `IslandDensity078` para editar raízes existentes, e reaproveitar o filtro espacial de `CoastalGrassAuthoring`, em vez de criar prefabs individuais.
- O projeto não tem um painel de pintura baked de grama e não tem o pacote opcional Terrain Tools instalado; a integração inicial é uma ferramenta contextual da toolbar Scene View, sem substituir o Inspector nativo ou acoplar dados ao TerrainData.
- As edições manuais serão separáveis das colocações procedurais para permitir regeneração segura do bake.
- A resolução do pincel e o feedback visual serão escolhidos no plano após inspeção do fluxo real de Scene View e da representação serializada atual.
- A entrada principal terá nome de produto simples (por exemplo, **Freyja > Vegetação > Editar Grama**); o nome final e os atalhos serão decididos no plano de implementação.
- A ferramenta mostrará no próprio painel onde alternar adicionar/remover, como selecionar tufos e como salvar; a ação contextual abre o mesmo painel, evitando superfícies diferentes de edição.
