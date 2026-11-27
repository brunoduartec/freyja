# Tasks: Editor manual da grama baked

**Input**: Design documents em `specs/084-editor-grama/`
**Prerequisites**: spec, plan, research, data-model, quickstart
**Tests**: Harness de authoring e validação Unity pedidos pelos critérios de aceite.

## Phase 1: Setup

- [ ] T001 Confirmar branch `084-editor-grama`, Unity project/cena corretos, estado do Console e preservar conflitos/alterações locais em `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`.
- [ ] T002 [P] Capturar baseline do layout/raízes visíveis e referência de Editor em `specs/084-editor-grama/evidence/baseline/`.

## Phase 2: Foundation

- [ ] T003 Definir IDs estáveis de raiz e modelo de operação manual em `Freyja/Assets/_Project/Scripts/Environment/CoastalGrassLayout.cs`.
- [ ] T004 [P] Implementar consulta enumerável de raízes base/suplementares e filtro de operações em `Freyja/Assets/_Project/Scripts/Environment/GrassDensityLayout.cs` e APIs relacionadas.
- [ ] T005 Integrar tombstones e destinos manuais à reconstrução regional, bounds e LOD em `Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.Lit.cs`.
- [ ] T006 Preservar e transferir overlay se o bake substituir o asset `layout` em `Freyja/Assets/_Project/Editor/CoastalGrassAuthoring.cs`.
- [ ] T007 Recompilar com Unity e validar o runtime sem edições (contagem, LOD, máscaras e instancing) antes da UI.

**Checkpoint**: camada manual separada da geração e executável pelo renderer existente.

## Phase 3: User Story 1 — Editar tufos pela Scene View (P1)

**Goal**: adicionar, selecionar, mover, girar, escalar e apagar tufos baked com Undo.
**Independent Test**: executar os modos na Scene View, desfazer/refazer, salvar/reabrir e verificar resultado renderizado.

- [ ] T008 [US1] Implementar janela UI Toolkit, ações/modes, parâmetros do pincel, ajuda e estado de persistência em `Freyja/Assets/_Project/Editor/GrassPlacementEditorWindow.cs`.
- [ ] T009 [US1] Implementar `EditorTool` contextual para Terrain, preview do raio, raycast, desenho e interação Scene View em `Freyja/Assets/_Project/Editor/GrassPlacementTool.cs`.
- [ ] T010 [US1] Implementar adicionar/apagar com amostragem determinística, filtros de superfície, exclusões e Undo agrupado em `GrassPlacementEditorWindow.cs`.
- [ ] T011 [US1] Implementar seleção, handle de posição e controles de yaw/escala para base, density e tufos manuais sem GameObjects por tufo em `GrassPlacementEditorWindow.cs`.
- [ ] T012 [US1] Criar menu `Freyja/Vegetação/Editar Grama` e ação no Inspector de `Coastal Grass` que abram o mesmo painel em `Freyja/Assets/_Project/Editor/CoastalGrassEditor.cs`.
- [ ] T013 [P] [US1] Implementar harness de edição/Undo, filtros, IDs e persistência em `Tools/Grass078/CheckManualGrassEditing.cs`.
- [ ] T014 [US1] Recompilar, executar harness e provar adicionar/mover/remover/Undo/Redo em Scene View; salvar/reabrir o layout e conferir câmera de jogo em `specs/084-editor-grama/evidence/`.

**Checkpoint**: fluxo manual P1 disponível e encontrável, sem reconstruir bake.

## Phase 4: User Story 2 — Acesso fácil pelo Unity e fluxo de terreno (P2)

**Goal**: abrir a ferramenta em até duas ações e orientar o artista no painel.
**Independent Test**: usar menu/busca e botão de Inspector, ler a ajuda, pintar/apagar, e confirmar bloqueio de terreno inválido.

- [ ] T015 [US2] Verificar comando via Unity Search, item de menu, botão do Inspector e toolbar contextual do Terrain abrindo/ativando o mesmo fluxo em `GrassPlacementEditorWindow.cs`, `CoastalGrassEditor.cs` e `GrassPlacementTool.cs`.
- [ ] T016 [US2] Revisar mensagens de validação/atalhos para caminho, areia, construção, inclinação e exclusões em `GrassPlacementEditorWindow.cs`.
- [ ] T017 [US2] Capturar janela, pincel/preview e botão contextual no Editor em `specs/084-editor-grama/evidence/editor/`.

## Phase 5: Polish e entrega

- [ ] T018 [P] Documentar como abrir e usar ferramenta, persistência/Undo e bake seguro em `specs/084-editor-grama/quickstart.md` e `validacao.md`.
- [ ] T019 Verificar compilação/Console sem limpar histórico, controles e IDs, estado da cena, harness, persistência, Game/Scene View e convergência da spec.
- [ ] T020 Integrar a branch aprovada à `main` somente após avaliação, preservando mudanças locais e o conflito prévio da cena até resolução explícita/segura.

## Dependencies

- T003–T006 são a base do renderer e protegem o bake.
- US1 depende de Foundation; US2 depende do menu/Inspector entregue em US1.
- Captura do Editor exige Unity da raiz correto e uma cena carregável.

## Estratégia

1. Primeiro validar overlay/runtime sem UI.
2. Entregar janela e pincel P1 com Undo e salvar/reabrir.
3. Validar os três acessos de descoberta e as mensagens do pincel.
4. Revisar captura visual e Console; só depois pedir avaliação da entrega.
