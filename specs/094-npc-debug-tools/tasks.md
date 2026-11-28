# Tasks: NPC Debug Tools

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md), [contracts](contracts/editor-diagnostics-api.md), [quickstart](quickstart.md).  
**Branch**: `094-npc-debug-tools`

## Phase 1 — Baseline e estrutura

**Purpose**: preservar o trabalho anterior e preparar uma implementação isolada do Editor.

- [x] T001 [US1] Reconfirmar branch, diff/untracked, scene, Console e Editor state; salvar baseline em `specs/094-npc-debug-tools/evidence/baseline.md` sem limpar o Console. Estado e limites MCP registrados.
- [x] T002 [P] [US1] Criar estrutura `Freyja/Assets/_Project/Editor/NpcDebug/` e `.meta` via Unity; manter todas as APIs de UI fora de player.
- [x] T003 [P] [US3] Confirmar formato/caminho de exportação fora de `Assets` e acrescentar ignore local se necessário sem alterar regras amplas do repo.

## Phase 2 — Fundação: contratos observáveis

**Purpose**: expor apenas leituras/comandos mínimos, sob `UNITY_EDITOR`, antes da UI.

- [x] T004 [US1] Guardar `ResolutionTrace` atual e expor pedido de reavaliação consumido por `NpcActivityController.TickFromWorld`; preservar execução ativa idêntica em `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcActivityController.cs`.
- [x] T005 [US1] Publicar `ActivityExecutionEvent` por evento estático `UNITY_EDITOR` no limite `Emit`, sem alocação/log extra quando não há assinantes, em `NpcActivityController.cs`.
- [x] T006 [US1] Expor cópia de leitura dos hosts validados do driver, compilada só no Editor, em `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcWorldRuntimeDriver.cs`.
- [x] T007 [US3] Expor cópia de leitura dos objetos do registry e limpar estado ao reset de subsistema em `Freyja/Assets/_Project/SmartObjects085/Runtime/SmartObjectRegistry.cs`.
- [x] T008 [US2] Adicionar taxa de calendário transitória Editor-only no mesmo `CoastalDayNight`, sem `Time.timeScale` ou persistência, em `Freyja/Assets/_Project/Scripts/Environment/CoastalDayNight.cs`.
- [x] T009 [US1] Criar DTOs de valor, filtros, status de disponibilidade, comandos e resultado serializáveis em `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugModels.cs`.
- [x] T010 [US1] Criar serviço de histórico Editor-only, ring buffer 512, descarte contado e reinscrição segura em reload/Play Mode em `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugEventBuffer.cs`. Overflow validado: 514 eventos, capacidade 512, dois descartes contados (D19).

**Checkpoint**: snapshots podem ser formados sem acessar caminho ou mudar estado; eventos só acumulam enquanto o Editor está ativo.

## Phase 3 — US1: Monitor passivo (P1)

**Goal**: comparar intenção/execução dos três NPCs reais na cena, com estados desconhecidos explicitados.

**Independent test**: Edit Mode mostra vazio/commands off; Play Mode captura IDs reais e distingue agenda, execução, navegação, Animator, reservas e status.

- [x] T011 [P] [US1] Criar capturas globais, individuais, Smart Objects e filtros determinísticos em `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugDiagnostics.cs`.
- [x] T012 [US1] Coletar walker/path atual sem calcular caminhos e Animator atual sem atribuir causalidade em `NpcDebugDiagnostics.cs`.
- [x] T013 [US1] Construir janela IMGUI com busca, filtros, lista ordenada, seleção, detalhe, update manual/2Hz e status de disponibilidade em `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugMonitorWindow.cs`. Compilação e estado Play Mode capturados; busca/atualização reais exercitadas (D01/D04).
- [x] T014 [US1] Adicionar busca e localizar alvos no Hierarchy/Scene View preservando seleção e descartando referências obsoletas no monitor. Handler Locate selecionou Ivo e seleção de Helena persistiu após atualização (D04).
- [x] T015 [US1] Mostrar próxima fronteira conhecida e última conclusão a partir do trace/event buffer; rotular previsão futura e eventos anteriores à assinatura como indisponíveis.

## Phase 4 — US2: Controles seguros e histórico (P1)

**Goal**: atuar pelo clock/host/coordinator proprietários e comprovar resultado após cleanup.

**Independent test**: pausar, avançar, alterar taxa do calendário, reavaliar e cancelar em Play Mode; snapshot posterior confirma efeito ou rejeição/pending.

- [x] T016 [US2] Implementar lease de pausa/taxa que restaura ao fechar/sair de Play Mode apenas se o valor ainda for o último aplicado pelo monitor em `NpcDebugDiagnostics.cs`.
- [x] T017 [US2] Implementar avanço positivo de 15/60 minutos e destino futuro pelo `CoastalDayNight.SetHour`; limitar saltos, cruzar rollover real e bloquear recuo.
- [x] T018 [US2] Implementar comando de reavaliação no tick existente e cancelamento com status pending quando ownership/cleanup permanecer em `NpcDebugDiagnostics.cs`.
- [x] T019 [US2] Mostrar comandos, estados, motivos, histórico filtrável e aviso `unsupported` de override no `NpcDebugMonitorWindow.cs`.
- [x] T020 [US2] Testar idempotência de reavaliação, duplicata/conflito, cancelamento e lifecycle do ring/lease via consultas Edit/Play Mode e MCP. Reavaliação repetida, cancelamentos, cancelamento duplicado e overflow passaram (D11/D12/D14/D19).

## Phase 5 — US3: JSON e Scene View (P1)

**Goal**: oferecer evidência estruturada sem UI e overlays passivos.

**Independent test**: snapshot global/individual via Unity MCP sem janela produz JSON válido/estável; Scene View usa dados capturados e não altera o estado.

- [x] T021 [P] [US3] Implementar serializer JSON com ordem fixa, cultura invariável, escaping, null/status de valores inválidos em `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugDiagnostics.cs` (serializer integrado à API, sem arquivo separado).
- [x] T022 [US3] Expor API pública `CaptureSnapshot`, `CaptureNpc`, `CaptureSmartObjects`, `GetRecentEvents` e `CaptureJson` sem dependência da janela em `NpcDebugDiagnostics.cs`.
- [x] T023 [US3] Exportar snapshot global/individual sob `Temp/NpcDebugTools/`, verificar escrita antes de sucesso, e copiar resumo para clipboard via menu/janela. Export Play Mode global/individual retornou `file_written`; JSONs parseados e arquivados na pasta de evidências (D20).
- [x] T024 [US3] Implementar overlay da Scene View para seleção (padrão) ou todos, destino/path existente, regiões e pontos/slots reais em `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugSceneOverlay.cs`. Labels posicionados em tela com fundo/colisão e evidência da Scene View; bounds de região não existem no contrato observado, então exibe-se somente o ID (D17).
- [x] T025 [US3] Remover callbacks do `SceneView`, listeners e caches no fechar/desabilitar, reload, scene change e saída de Play Mode.
- [x] T026 [US3] Validar API/JSON com `script_eval` via Unity MCP com janela fechada, filtros e export real no mesmo estado de Play Mode.

## Phase 6 — Regressão, custo e entrega

- [x] T027 [US1] Verificar menu, Edit Mode, seleção/filtros, idle/wander/sit, dois NPCs em disputa e terceiro em fallback com três controllers reais. Menu, estados reais e handlers de busca/seleção/Locate foram exercitados; evidência D04, com automação por reflection.
- [x] T028 [US2] Validar pausa/retoma, avanços 15min/1h/meia-noite, taxa de calendário, cancelamento sentado/em movimento e reavaliação repetida; declarar não suportado onde não houver contrato. Cancelamentos sentado e em deslocamento passaram; override documentado como não suportado (D12/D13).
- [x] T029 [US3] Validar consulta sem janela, JSON global/individual/events, serialização de valores não finitos, ordenação, Scene View passiva e objeto/slot lifecycle. API, exports, lifecycle, valores não finitos e overlay compilado/capturado em Play Mode passaram (D17); bounds de região permanecem indisponíveis pelo contrato.
- [x] T030 [P] [US1] Repetir abrir/fechar monitor e start/stop Play Mode três vezes; verificar domain reload e Console sem erros novos.
- [x] T031 [US3] Medir amostras comparáveis com UI fechada/aberta, três NPCs; documentar custos e limites em `specs/094-npc-debug-tools/evidence/performance.md`. Microbenchmark 100 capturas em ambos os estados, com limites explicitados (D24 parcial).
- [x] T032 [US3] Capturar JSON real e imagens úteis de janela/Scene View em `specs/094-npc-debug-tools/evidence/`.
- [x] T033 [US3] Preencher D01–D26, atualizar tasks com evidência e escrever relatório final em `npc-debug-tools-report.md`, incluindo limitações e gate para `npc-schedule-editor`.
- [x] T034 [US1] Executar regressões relevantes existentes de Smart Objects e Animation; compilar/importar e revisar Console sem apagar erros de baseline. Suítes de agenda/execução/navegação não existem nesta bateria (D25 parcial).
- [x] T035 [US3] Revisão independente Validador dos contratos/diff/evidências; corrigir defeitos reproduzidos e atualizar status sem alegar cobertura ausente. Risco de cleanup corrigido e recompilado; teste de falha injetada permanece pendente em T042.

## Dependências e ordem

- T001–T010 bloqueiam a implementação das histórias.
- US1 depende de T004–T010; US2 depende de T004, T006 e T008; US3 depende de T007, T009 e T010.
- T011–T015 podem começar após a fundação; T016–T020 após as portas reais; T021–T026 após DTOs e captura.
- Validação física T027–T030 depende de importação/compilação aprovada e montagem funcional da UI/API.
- Relatório D01–D26 T033 depende das evidências T026–T032 e revisão T035.

## Oportunidades paralelas

- Após T009, serializer/API model (T021) pode avançar sem tocar UI.
- Após T010 e portas disponíveis, janela IMGUI e serializer ficam em arquivos independentes.
- Não paralelizar alterações no mesmo controller/clock/registry, cena ou sessão Unity.

## Estratégia de implementação

1. Fundação de baixo risco e captura passiva.
2. Janela + JSON consumíveis com Play Mode real.
3. Comandos condicionais e Scene View.
4. Matriz D01–D26, custos e regressões; concluir somente conforme evidência.

## Phase 7: Convergence

- [x] T036 Completar interação de busca, filtros, seleção persistente e Locate pela UI com os três NPCs reais e registrar evidência; handlers reais validados por reflection (D04).
- [x] T037 Validar rejeição de comandos duplicados/conflitantes, registrando resultado real; segundo cancelamento rejeitado com `no_active_execution` (D14).
- [x] T038 Exercitar desativação de NPC reservado e reload de cena; confirmar cleanup de slot, sessão reaberta e consulta sem dados obsoletos (D16, D23).
- [x] T039 Validar capacidade 512 do histórico com teste finito e seguro, confirmando descarte e contador (D19).
- [x] T040 Coletar comparação de captura/tempo/GC com monitor aberto e fechado em condições iguais; limitações de escopo registradas e D24 mantido parcial.
- [x] T041 Obter confirmação de Console/estado do Editor Unity após reconexão MCP e completar revisão independente final (D01, T035). Console não foi limpo; erros repetidos do Pipeline estão identificados e compilação sem erros.

## Phase 8: Convergence

- [x] T042 Compilar e testar o caminho de falha em `ActivityExecution.RetryCleanup`, garantindo que release malsucedido reporte `cleanup_failed` mesmo após `debug_cancel` e não esconda ownership residual (SC-004). Probe com fake `IActivityPorts` e evidência anexada.
- [x] T043 Executar `ExportJson` global e individual, confirmar sucesso de escrita somente após o arquivo aparecer sob `Temp/NpcDebugTools/` e anexar evidência real (D20).
- [x] T044 Adicionar ou executar cobertura automatizada relevante de Agenda, Execução e Navegação, ou manter D25 parcial com lacunas aceitas explicitamente (D25). Mantida a classificação PARTIAL por ausência das suítes.
