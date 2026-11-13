# Tarefas — 002 · Caminhar pelo cais

**Estado:** implementação e validação técnica concluídas; aguardando avaliação do usuário. Evidência: [validacao-us1.md](evidence/validacao-us1.md).
**Entrada:** [spec](spec.md), [plano](plan.md), [pesquisa](research.md), [estados](data-model.md), [controles](contracts/player-controls.md) e [validação](quickstart.md).

Cada caixa registra trabalho efetivamente concluído, com evidência. Todos os caminhos abaixo são relativos à raiz do projeto. [P] indica preparação independente em arquivos distintos; [US1] identifica a única história da spec.

**Testes:** a validação funcional da spec é obrigatória. Como a spec não solicita suíte automatizada ou TDD, não há tarefas obrigatórias de criação de testes automatizados; a automação mencionada no plano permanece opcional. Não repetir verificações anteriores sem mudança ou dúvida relevante.

## Fase 1 — Preparação

- [X] T001 Conferir versões fixadas, conexão MCP ao projeto correto e alterações não salvas antes de operar; reutilizar evidências válidas e registrar somente verificações necessárias em `specs/002-caminhada-no-cais/evidence/ambiente.md`, tomando `Tools/MCP/README.md` como referência. Não descartar nem salvar silenciosamente alterações do usuário.
- [X] T002 Preparar no editor as pastas `Freyja/Assets/_Project/Scripts/Prototype/`, `Freyja/Assets/_Project/Art/Characters/Prototype/` e `Freyja/Assets/_Project/Prefabs/`, preservando GUIDs existentes e sem atualizar `Freyja/Packages/manifest.json` ou o editor. Depende de T001.

## Fase 2 — Base da cena

Nenhuma tarefa da história começa antes de concluir esta fase.

- [X] T003 Criar `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity` a partir de `Freyja/Assets/_Project/Scenes/Cais_Iteracao02.unity` via Unity MCP, conservando o cenário e sua iluminação; registrar o estado da fonte em `specs/002-caminhada-no-cais/evidence/ambiente.md` para conferir sua preservação. Depende de T002.
- [X] T004 Completar proxies sólidos de caixas, pedras, costão e barco alcançáveis pelo jogador/câmera em `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity`; revisar tábuas/degraus e configurar masks sem colisão sobre o mar. Manter alterações como overrides locais, sem aplicar ao prefab `Freyja/Assets/_Project/Prefabs/Cais_VisualReview.prefab`; se necessário criar layers em `Freyja/ProjectSettings/TagManager.asset` preservando as existentes. Depende de T003.

**Checkpoint:** nova cena salva; fonte preservada e superfícies relevantes preparadas. Isso ainda não comprova navegação.

## Fase 3 — US1 · Percorrer o cenário em terceira pessoa (P1 / MVP)

**Objetivo:** controlar uma figura provisória do desembarque até o início do caminho e voltar, avaliando movimento, câmera, escala e passagem.
**Teste independente:** abrir a nova cena e completar duas idas e voltas; conferir câmera junto aos obstáculos, queda, reinício e liberação/retomada do cursor.

- [X] T005 [P] [US1] Produzir uma figura estática de aproximadamente 1,8 m no Blender MCP, salvar `Source/Blender/Player_Provisorio.blend` e exportar `Freyja/Assets/_Project/Art/Characters/Prototype/Player_Provisorio.fbx`; validar escala, orientação e materiais na importação. Sem personagem ou animação final. Depende de T004.
- [X] T006 [P] [US1] Criar `Freyja/Assets/_Project/Scripts/Prototype/CaisPlayerController.cs` com CharacterController, entrada planar fornecida pela sessão, direção relativa ao Transform da câmera, magnitude limitada a 1, gravidade, rotação e operação de reset. Expor referências e parâmetros ajustáveis sem depender de classes ainda não criadas. Depende de T004.
- [X] T007 [P] [US1] Criar `Freyja/Assets/_Project/Scripts/Prototype/CaisCameraRig.cs` com alvo Transform, órbita em perspectiva, pitch limitado, consumo de delta sem segunda multiplicação por tempo, atualização após movimento e operação de reset; proteger SphereCast/near clip contra sólidos, inclusive perto do pivô, com recolhimento rápido e retorno suave. Depende de T004.
- [X] T008 [US1] Criar `Freyja/Assets/_Project/Scripts/Prototype/CaisSessionController.cs` como dono do input e estados de controle: referenciar Player/Move, Player/Look, UI/Cancel e UI/Click em `Freyja/Assets/InputSystem_Actions.inputactions`, habilitando somente o necessário. Encaminhar amostras aos controladores; clique captura, Esc/perda de foco ou lock interrompem; foco sozinho não recaptura. Limpar entradas e primeiro delta, desabilitar ações/assinaturas ao sair e evitar estado estático. Depende de T006 e T007.
- [X] T009 [US1] Completar recuperação em `Freyja/Assets/_Project/Scripts/Prototype/CaisSessionController.cs`: validar SpawnPoint com piso e espaço livre; detectar queda/bounds; reposicionar com CharacterController temporariamente desabilitado; limpar velocidades e câmera; preservar corretamente estado de foco/captura. Spawn inválido deve produzir diagnóstico e bloquear controle sem loop. Depende de T008.
- [X] T010 [US1] Montar `Freyja/Assets/_Project/Prefabs/PlayerPrototype.prefab` e vinculá-lo em `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity`; configurar figura, cápsula, rig, sessão, SpawnPoint, bounds e masks. Substituir a câmera de revisão nesta cena por uma única câmera de gameplay e um AudioListener, sem alterar a cena-fonte. Depende de T005, T009 e T007.
- [X] T011 [US1] Adicionar Canvas de instruções legíveis em `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity` e vincular sua apresentação em `Freyja/Assets/_Project/Scripts/Prototype/CaisSessionController.cs`: WASD, mouse, Esc e clique para controlar/retomar. Não habilitar comandos de salto/corrida nem um segundo sistema de input. Depende de T010.
- [X] T012 [US1] Calibrar parâmetros de controlador, câmera e overrides de passagem em `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity`, partindo dos valores do plano; conferir encaixe da cápsula, junções, degraus, oclusão e retorno da câmera, sem piso sobre a água nem ampliação do cenário. Depende de T011.
- [X] T013 [US1] Executar o roteiro completo de `specs/002-caminhada-no-cais/quickstart.md` em Play Mode e registrar `specs/002-caminhada-no-cais/evidence/validacao-us1.md`: duas idas e voltas, axial/diagonal, obstáculos, órbita/near clip, queda/bounds, reinício, Esc, perda de foco com W pressionado, recaptura sem salto e nova execução sem Domain Reload. Corrigir falhas nos arquivos responsáveis antes de marcar concluído; registrar também compilação/Console e preservação da fonte. Depende de T012.

**Checkpoint:** US1 funcional e verificável isoladamente. Todos os requisitos FR-001 a FR-007 e critérios SC-001 a SC-004 devem ter resultado explícito; não apresentar pendências como aprovadas.

## Fase 4 — Entrega para avaliação

- [X] T014 [P] Ajustar `specs/002-caminhada-no-cais/quickstart.md` com a cena, controles e passos realmente disponíveis, incluindo instrução de clique inicial e limitações reais; manter links para `specs/002-caminhada-no-cais/evidence/validacao-us1.md`. Depende de T013.
- [X] T015 [P] Capturar no Unity MCP uma prévia e um vídeo curto ou sequência de capturas do percurso em `specs/002-caminhada-no-cais/evidence/`; identificar em `specs/002-caminhada-no-cais/evidence/validacao-us1.md` o que foi verificado em movimento e o que é apenas imagem estática. Depende de T013.
- [X] T016 Conferir arquivos/cenas salvos, referências e ausência de erros relevantes; atualizar `specs/002-caminhada-no-cais/spec.md` e `specs/README.md` para entrega pronta para avaliação, apontando a cena e evidências. Preparar apresentação ao usuário e parar antes de ampliar o escopo; aprovação visual/jogável só será registrada quando recebida. Depende de T014 e T015.

## Dependências e ordem

~~~text
T001 → T002 → T003 → T004
                         ├→ T005 ────────────────────┐
                         ├→ T006 ─┐                 │
                         └→ T007 ─┴→ T008 → T009 ───┴→ T010
                                                       ↓
                                                     T011 → T012 → T013
                                                                       ├→ T014 ─┐
                                                                       └→ T015 ─┴→ T016
~~~

Existe somente US1 (P1); não há histórias adicionais ou entregas narrativas nesta lista. Setup e base bloqueiam a história; documentação e apresentação vêm após sua validação.

## Paralelismo possível

- Após T004, T005 (Blender), T006 e T007 (arquivos C# distintos) podem ser preparados independentemente. T008 só começa após os dois controladores.
- Após T013, T014 (guia) e T015 (captura) podem avançar sem editar os mesmos arquivos simultaneamente; a atualização do registro de evidências deve ser coordenada.
- Não operar duas alterações de cena simultaneamente no mesmo editor. [P] não autoriza criar subagentes ou ampliar a entrega.

## Cobertura

| Requisito | Tarefas principais |
| --- | --- |
| FR-001 — cena separada | T003, T010, T013 |
| FR-002 — figura provisória | T005, T010, T012 |
| FR-003 — caminhada | T006, T008, T012, T013 |
| FR-004 — câmera | T007, T010, T012, T013 |
| FR-005 — sólidos e passagem | T004, T006, T012, T013 |
| FR-006 — recuperação e reinício | T009, T010, T013 |
| FR-007 — instruções e foco | T008, T011, T013, T014 |

## Estratégia de implementação

Executar uma única fatia: preparação → cena-base → US1 → validação → apresentação. O MVP é a história inteira, não apenas um personagem que anda sem câmera/foco/recuperação. Salvar e verificar cada etapa; diante de alteração de escopo, voltar à spec antes de implementar. Ao concluir, aguardar avaliação do usuário.
