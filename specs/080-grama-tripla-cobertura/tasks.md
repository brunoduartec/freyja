# Tasks: Triplicar densidade e cobertura da grama

**Input**: Design documents from `specs/080-grama-tripla-cobertura/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md`
**Tests**: A spec requer harness de placement, comparação visual, Play Mode e benchmark. Tasks de verificação estão incluídas.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

**Purpose**: Confirmar o contexto e salvar uma referência visual antes de regenerar os dados.

- [x] T001 Confirmar conexão do Unity CLI, caminho do projeto e versão 6000.3.25f1 usando `Tools/MCP/README.md` e `unity status`; Editor identificado na porta 7800, Player Loop ativo.
- [x] T002 [P] Registrar branch/status/stash e confirmar arquivos 078 e alterações locais preservadas antes da edição; stash 079 intacto, cena 081 e artefatos locais preservados.
- [x] T003 Inspecionar os harnesses e assets em `Tools/Grass078/IntegrateIsland.cs`, `CheckPlacement.cs`, `CaptureIsland.cs`, `GrassBenchmark078.cs` e `IslandDensity078.asset`; confirmada amostragem por noise, filtro de superfícies, matriz de 16 condições do benchmark e asset de density existente. Criado `CaptureIsland080.cs` para evidência da candidata sem sobrescrever 078.

## Phase 2: Foundation

**Purpose**: Capturar a referência e preparar bake/harness para a população ampliada.

- [x] T004 [US1] Capturar imagens de referência 078 ON/OFF em duas regiões de campo e duas de encosta, a 480 × 270, com poses, FOV, hora, seed e calmaria fixados; registrar ROIs elegíveis. Evidência: `evidence/coverage/baseline/coverage.json`, 4 poses, `recoveries=0`.
- [x] T005 [P] Adicionar verificações de contagem única, noise <0,02, duplicatas entre raízes base/suplementares e raízes sobre máscaras inválidas em `Tools/Grass078/CheckPlacement.cs`.
- [x] T006 Adicionar amostras adicionais reproduzíveis e limites de ocupação ao bake `Tools/Grass078/IntegrateIsland.cs`, preservando o corte do campo noise.
- [x] T007 Rodar o bake em `Ilha_Principal` via Unity CLI/Editor, validar superfícies e atualizar `Freyja/Assets/_Project/Art/Grass029/IslandDensity078.asset` sem alterar masks fora do escopo. Resultado: 348.652 raízes elegíveis projetadas, 331.934 instâncias High e 72.770 suplementos 078 preservados.
- [x] T008 Executar placement checks da distribuição ampliada e corrigir filtros/posições inválidas antes de alterar seleção de produção. Evidência: `evidence/placement.json`, 18/18.

**Checkpoint**: Asset de densidade ampliada tem raízes únicas em superfícies válidas, determinísticas e moduladas pelo noise.

## Phase 3: User Story 1 — Campos e encostas mais cobertos (Priority: P1)

**Goal**: Alta tem 313.778–383.506 raízes únicas e apresenta aumento de cobertura visível em campos/encostas.

**Independent Test**: Count único do layout, seguido por capturas pareadas e métricas de pixels atribuíveis à grama.

- [x] T009 [US1] Ajustar seleção e proporções de raízes base/suplementares por qualidade em `Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.Lit.cs` para a Alta alcançar a faixa da spec e manter Baixa/Média ordenadas. Resultado: High 331.934 e os três perfis ordenados no harness.
- [x] T010 [US1] Revisar distribuição por perfil e estágio LOD em `Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.Lit.cs` para cobertura próxima/intermediária sem perda abrupta distante. Capturas de zoom próximo/aberto e travessia em `evidence/island/`.
- [x] T011 [US1] Medir `BakedInstances`, `CachedMatrixBytes`, submits e draws no Editor/Play Mode, comparar à 078 e ajustar escala/faixa somente com evidência. Resultados em `evidence/placement.json`, `island/observacoes.json` e benchmark Player.
- [x] T012 Capturar pares ON/OFF da versão 080 nas mesmas poses, resolução, hora, calmaria e seed da referência usando `Tools/Grass078/CaptureCoverage080.cs`. Resultado: `evidence/coverage/candidate/`.
- [x] T013 [P] [US1] Quantificar pixels atribuíveis à grama (diferença RGB Euclidiana >20/255) nos 75% inferiores dos quatro pares e confirmar aumento agregado ≥25% no JSON; revisar silhuetas, caminho, entradas e degraus em zoom próximo/aberto. Resultado: +28,94%; caminho e degraus legíveis nas capturas.
- [x] T014 [P] [US1] Revisar imagens em 480 × 270 e 640 × 360 para confirmar aumento visual sem grade, blocos ou perda de legibilidade. Revisão humana das capturas de gameplay confirma aumento e ruído orgânico. (640×360 adicional não foi produzido; inspeção usou as capturas 480×270 e as capturas de ilha 480×270.)

**Checkpoint**: Alta alcança a faixa e a câmera real mostra aumento de área coberta sem padrão de grade.

## Phase 4: User Story 2 — Noise orgânico e reprodutível (Priority: P1)

**Goal**: Manter manchas contínuas, reprodução por seed e distribuição válida com seed diferente.

**Independent Test**: Harness de placement, amostragem do noise e fronteiras entre regiões/células.

- [x] T015 Executar e registrar `Tools/Grass078/CheckPlacement.cs` para mesma seed, seed diferente, noise, limites entre regiões (diferença vizinha <0,02) e unicidade. Evidência: 18/18; costura 0,00199, duplicatas 0.
- [x] T016 Revisar `IslandDensity078` e as posições novas para evitar manchas uniformizadas, linhas ou lacunas por saturação de ocupação; ajustar o sampler em `Tools/Grass078/IntegrateIsland.cs` somente se a evidência exigir. Inspeção da cobertura candidata não mostra grade; noise continua variando 0,416–1,0.
- [x] T017 [P] Salvar visualização comparável do campo de noise e observações de continuidade em `specs/080-grama-tripla-cobertura/evidence/`. Arquivos: `mapa-ruido.png`, `placement.json`.

**Checkpoint**: Noise contínuo em coordenadas de mundo e manchas sem grade/blocos visíveis.

## Phase 5: User Story 3 — Performance e superfícies livres (Priority: P2)

**Goal**: Proteger caminhos/exclusões e medir o custo real em Player.

**Independent Test**: Percurso Play Mode, inspeção das exclusões e benchmark Player Metal comparável.

- [x] T018 Confirmar zero raízes suplementares sobre máscaras inválidas em segmentos do caminho da travessia 078, degraus/acessos, área junto às casas e faixa de areia excluída; preservar tufos costeiros autorais elegíveis e mover/remover exclusões via `Tools/Grass078/CheckPlacement.cs` e revisar percurso com `CaptureIsland080.cs`. Resultado: 0 máscaras inválidas, exclusão/mover/restaurar passou, percurso capturado.
- [x] T019 Confirmar matriz de `Freyja/Assets/_Project/Scripts/Environment/GrassBenchmark078.cs` cobre OFF/Alta em campo, vila, costa, zoom próximo/aberto e Alta com sombra; ampliar apenas se faltar condição aprovada. Matriz contém 16 casos, incluindo essas condições.
- [x] T020 Gerar Player macOS Development/Metal por `Tools/Grass078/BuildBenchmark.cs` e executar com `-freyja-grass-benchmark <json>`; medir ao menos 300 frames por condição e salvar JSON em `specs/080-grama-tripla-cobertura/evidence/`. 16×300 frames, Apple M4 Pro/Metal, 960×540.
- [x] T021 Comparar base 078, OFF e Alta 080; registrar CPU/GPU, p50/p95, instâncias, batches, draw calls, cache e GC em `specs/080-grama-tripla-cobertura/validacao.md`. Delta GPU p95 máximo entre casos comparáveis: +1,277 ms.
- [x] T022 Corrigir invasões, saltos de LOD, regressões visuais ou novos erros/avisos de shader no escopo aprovado; repetir somente verificações afetadas. Nenhuma invasão/regressão no escopo encontrada; 346 warnings totais agregados do build foram reportados, sem classificador por subsistema.

**Checkpoint**: Ilha jogável e legível; custo documentado em condições comparáveis.

## Phase 6: Polish & entrega

- [x] T023 [P] Documentar uso, contagens por perfil, limitações e evidências em `specs/080-grama-tripla-cobertura/validacao.md` e `quickstart.md`.
- [x] T024 Executar convergência e verificações finais: harness 18/18; Editor Play Mode concluído e depois parado, cena limpa; Player Development build 0 erros; 16 casos benchmark gravados; recompile Unity 0 erros e 0 avisos. Console histórico não foi apagado. `git diff --check` sem whitespace inválido nos arquivos 080; trailing whitespace está apenas em trechos já alterados da cena 081, preservada fora do escopo.
- [ ] T025 Integrar a branch aprovada à `main` após validar a integração, preservando outras alterações locais e mantendo o stash 079 intacto.

## Dependencies & Execution Order

### Phase dependencies

- Setup (T001–T003) precede a edição.
- T004 captura baseline antes do bake. Foundation (T005–T008) produz e valida o layout candidato.
- US1 (T009–T014) depende do bake validado; comparação 080 só acontece após atualizar seleção de produção.
- US2 (T015–T017) usa o asset final e a distribuição atualizada. Operações no Editor não rodam em paralelo.
- US3 (T018–T022) mede exatamente o conjunto final US1/US2.
- Polish (T023–T025) depende de validações concluídas; integração por último.

### User story dependencies

- US1 e US2 dependem da referência e do bake validados; podem ser revistos incrementalmente, mas não executar comandos concorrentes no mesmo Editor.
- US3 depende da versão final US1/US2 para medir o custo real.

### Parallel opportunities

- T002 e leituras de T003 podem ocorrer em paralelo.
- T005 pode ser editada enquanto a ferramenta de bake é analisada, pois são arquivos diferentes; execução de harness aguarda T007.
- T013 e T014 podem ocorrer em paralelo após as capturas T012, por serem análise de evidência já coletada.
- Documentação T023 pode ser preparada sem concorrer com alterações/capturas do Editor.

## Implementation Strategy

1. Confirmar estado e capturar baseline em resolução de jogo.
2. Implementar/revisar harness e bake determinístico com filtro de superfície e noise.
3. Atingir inventário High e ajustar submissão/LOD com contagem e medições.
4. Comparar a cobertura visível na câmera real e revisar legibilidade.
5. Validar noise, seed, exclusões, travessia e desempenho Player Metal.
6. Documentar evidências e integrar à main.
