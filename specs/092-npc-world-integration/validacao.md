# Validação — NPC World Integration

## Baseline anterior à implementação

- Branch: `092-npc-world-integration`, baseada em `main` (`fabd711`).
- Unity: 6000.3.25f1; Built-in; o Editor atual desta branch está `ready` em port 7800, PID 3373. O scene `Ilha_Principal.unity` abriu sem alterações pendentes. MCP direcionado e console verificado neste projeto.
- Unity Test Runner: baseline no outro worktree do mesmo commit-base: TestResultCollector registrou 40/40 PASS. A chamada assíncrona MCP excedeu o timeout ao retornar; resultado foi obtido do log original, não do payload final do comando.
- `rtk dotnet run --project Tools/NpcScheduleSimulator -- --validate`: PASS — 5 schedules NPC, 15 atividades, 3 overrides e 3 receitas executáveis.
- `rtk dotnet run --project Tools/NpcScheduleSimulator -- --test`: PASS — 160 assertions. Avisos prévios de lacuna `sleep_early` para fisherman.
- `rtk dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week`: PASS como simulação lógica — 7 dias, 3 NPCs, 1.047 eventos, 16 reservas/16 liberações, 0 claims residuais; duas falhas físicas simuladas pré-existentes (`demo_farmer/path_unreachable`, `demo_fisherman/animation_rejected`), `physicalValidation=false`.
- Este baseline não valida cenas, navegação real, pose ou comportamento da feature.

## Matriz W01–W19

| ID | Status | Método/evidência | Resultado observado |
|---|---|---|---|
| W01 | PASS | Unity 6000.3.25f1 recompile/status, batch EditMode XML e harness .NET final | Recompile `up_to_date`; `compilationFailed=false`, `compiling=false`; EditMode 40/40, 0 failed; .NET 174 assertions PASS e validação PASS. Console final: 0 erros; 2 warnings explicados no T024 (timeout de conta Unity AI e diagnóstico esperado do teste de ID duplicado do Smart Object). |
| W02 | PASS | Unity MCP `component.get_fields`, driver live e três controllers | `/Waypoints da ilha/NPC World Integration` referencia `Coastal Sun`, lista três hosts distintos; IDs `world_ivo`, `world_helena`, `world_luana`, ports/provedor/registry e animators presentes. |
| W03 | PARTIAL | Play Mode: provider null, `npcSchedules=[]`, duplicate host/ID, destino inativo e `TryBegin` de animação desconhecida | Diagnósticos finitos e sem owner falso; host sem JSON não inicia; duplicatas são ignoradas. JSON malformado/sintaxe inválida não foi testado. A rejeição de animação foi exercitada no adapter via Play Mode, não como uma rotina JSON completa. |
| W04 | PASS | Play Mode iniciou pela hora serializada intermediária | Driver projetou Day 1 10:30 (minuto 630), sem retroexecutar agendas anteriores. |
| W05 | PASS | Dia completo; runtime transform comparado com registry home points | Os três executaram `idle` até seus próprios `world_*_home`; distâncias ao chegar foram 0,20–0,21 m. |
| W06 | PASS | Play Mode, snapshots de movimento, eventos `move_to` e câmera runtime | NPCs caminharam por `village_square` e `beach`; chegadas de `IslandWaypointWalker` completadas. Captura panorâmica de Day 2 13:45 complementa os snapshots, mas não identifica individualmente cada personagem. |
| W07 | PASS | Play Mode em 11:00 e transição para 13:00; eventos e Animator observados | Reserva, approach, alinhamento, pose sentada, exit/stand-up, saída e `ResourceReleased` ocorreram na ordem; imagens de bancos no piso em `evidence/benches-grounded-detail.png`. |
| W08 | PASS | Três NPCs na janela compartilhada com dois bancos de dois slots | Dois ocuparam slots West e o terceiro um slot East; em teste West-only, os três competiram por dois lugares. |
| W09 | PASS | Desativei East apenas em Play Mode; observei owners/slots | West `sit_left/right` ocupados por dois NPCs; terceiro falhou finitamente `target_unavailable`; nenhum claim órfão/duplicado. |
| W10 | PASS | Relógio atravessou fronteiras enquanto atores estavam em movimento | Em fronteiras naturais, execução anterior foi cancelada/substituída uma vez; movimento/completion do waypoint não foi reaplicado por frame. |
| W11 | PASS | Avanço de 11:00 para 13:00 com NPCs sentados | Exit e movimento de aproximação ocorreram antes de `release_slot`; novo `wander` iniciou depois da liberação. |
| W12 | PASS (test-only) | `InterruptNpcForValidation("world_ivo")` e Play Mode | `ActivityCancelled` observado, owner retido enquanto `stand_up`/cleanup pendia, depois liberado por `cleanup_retry`. Não há evento real de início/fim de diálogo integrado nesta cena. |
| W13 | PASS com limite | Approach inacessível e `beach_east_path` inativo em Play Mode | `path_unavailable`/`target_unavailable` finitos e sem teleport ou claim residual. Limite reproduzido: fallback `idle` herda `target=beach` e pode falhar `destination_unavailable`. |
| W14 | PARTIAL | Desativei somente `NpcActivityController` enquanto Ivo estava sentado; Animator/owner consultados | `Sitting_Exit` tocou com owner ainda 1; após reativar chegou a `HumanM@Idle01` e owner foi 0. Retry tardio gerou `host_disabled` e `animation_timeout`. Disable do GameObject inteiro/destroy/unload não foi testado. |
| W15 | PASS | Gameplay Debug Panel via botão/slider; time scale mantido 1 | Pause estável, avanço para horário futuro resolve intent vigente, rollover natural avança o dia e retrocesso emite `clock_regressed` mantendo o snapshot anterior. |
| W16 | PASS | Edit de `world_ivo.json`, `AssetDatabase.Refresh`, restart Play Mode | Troca `beach` → `village_square` observada às 13:30 sem mudança de C#; asset restaurado depois. |
| W17 | PASS | Play Mode com relógio de 96 s por 24 h; runtimes e eventos reais | Passou mais de 24 h, incluindo homes, praça, bancos e praia; zero claims após release e no estado final em casa. Detalhes em T019 abaixo. |
| W18 | PASS | Stop/start Play Mode com reload atual do projeto | Cena limpa, sem camera runtime; reinício criou três hosts/execuções únicas e zero claims. Configuração atual não desabilita domain/scene reload. |
| W19 | NOT RUN | Inspeção de diff e Play Mode em `Ilha_Principal` | Nenhum arquivo de input/controle de protagonista foi alterado, mas controle de protagonista e regressão jogável não foram testados diretamente; requer regressão dedicada em `npc-integration-validation`. |

## Diário de evidências

Os registros abaixo incluem horário do jogo, NPC, executionId, destino, slot/owner, evento, localização física, imagem e resultado. Saídas do simulator e snapshots de intenção não são tratados como prova de execução física.

### Fase de fundação — 2026-11-27

- O primeiro teste batch identificou CS0103 (`operationalSeconds`) no driver recém-criado. A variável foi definida em `Update`; também foi corrigido o reinício do estado da projeção no `OnEnable` para evitar vazamento entre sessões sem domain reload.
- Após a correção: `unity test Freyja --mode EditMode` PASS (40/40); MCP do Editor alvo confirma compile limpo e Console 0/0/0. `dotnet run --project Tools/NpcScheduleSimulator -- --test` PASS (173 assertions, incluindo projeção e carregamento/overlap dos três JSONs); `--validate` PASS. `--simulate-week` PASS como teste lógico: 7 dias, 1.047 eventos, 16 reservas/liberações, 0 claims residuais, 2 falhas simuladas e `physicalValidation=false`.
- `Ilha_Principal` permanece sem configuração da feature até este ponto; a captura anterior à authoring mostra a praça central e o estado inicial da cena. Nenhum movimento/pose físico da feature foi validado ainda.

### Vertical slice e correções do cenário — 2026-11-27

- T011: no Editor, os pés dos bancos estavam cerca de 1,15 m acima do piso. Os bounds mostraram o fundo das pernas em Y=7,495/7,475, enquanto raycasts no piso da praça variaram de Y=6,25 a 6,50. Rebaixei as raízes para West Y=5,20 e East Y=5,22, preservando os Approach no piso (Y=6,35/6,32) e colocando os Pose no assento (Y=6,85/6,87). A captura próxima `evidence/benches-grounded-detail.png` mostra os dois bancos apoiados na praça.
- T011: `NpcLocationRegistry` preserva `Largo`, `Rua leste — praia` e três pontos de casa. `Venda` e `L1` continuam como waypoints do cenário, mas não são destinos das rotinas porque raycasts/NavMesh não os validaram como caminhos de praça/praia.
- T012/T013: Play Mode com o relógio pausado em 10:30 mostrou os três personagens caminhando de seus pontos iniciais até `Largo`. Em 11:00, os três reservaram slots diferentes, alinharam no assento e ficaram em `Sitting_Idle_Loop`; dois usaram West e um East. Às 13:00 o fluxo tocou a saída, liberou os três slots e os NPCs voltaram a caminhar.
- T013/T017: interrupção de validação de Ivo no banco registrou `ActivityCancelled`, `residual=True`, tentativa de `stand_up`/limpeza, `ResourceReleased` em `cleanup_retry` e início da execução nova após limpeza. Foi observado um único erro transitório `residual_cleanup_pending` enquanto a animação terminava; o estado seguinte não reteve claim.
- T014: alterei somente `world_ivo.json` (`beach` → `village_square` em 13:00–18:00), atualizei o AssetDatabase e reiniciei o Play Mode. Às 13:30 o NPC carregou `wander` com `target=village_square`; depois restaurei o arquivo original. Nenhum C# foi alterado nesse teste.
- T016/T018: com East desativado somente durante o teste, os dois slots West ficaram `Occupied` por Ivo e Helena; Luana recebeu `target_unavailable`, não reservou slot, e os dois claims válidos permaneceram únicos. Num teste anterior, o East `Approach` inválido produziu `path_unavailable` e `ResourceReleased`; passei a excluir candidatos sem caminho NavMesh completo antes de reservar.
- T013: revisão do dia inteiro revelou que o core executava `idle` targetless como no-op antes de chamar o adapter. Adicionei um hook opcional `ITargetlessActivityPointProvider`; o adapter agora resolve o ponto de casa configurado sem mudar JSON ou a receita. A validação física de retorno a casa aguarda o reteste do dia completo.
- Depois dessas mudanças: Unity recompilou sem erros/avisos; Unity EditMode 40/40 passou; o harness .NET passou com 174 assertions. Avisos pré-existentes de lacuna `sleep_early` do `demo_fisherman` permanecem.

### T017 — interrupção, callbacks antigos e disable do host — 2026-11-27

- Relógio parado em 11:00 e Ivo executando `sit` em `npc_world_bench_west`. Ao desativar somente `NpcActivityController.enabled`, o log registrou `ActivityCancelled reason=host_disabled`; o banco permaneceu com `ReservationCount=1`. Três segundos depois, com o controller ainda desativado, o Animator filho estava em `Armature|Sitting_Exit` e a reserva seguia em 1. Ao reativar o controller, o Animator chegou a `HumanM@Idle01`, a reserva foi liberada (`ReservationCount=0`) e o snapshot cancelado foi limpo. A ocorrência `sit` vigente não reiniciou uma execução antiga.
- Logs Unity do probe: `host_disabled` às 21:57:25Z, `residual_cleanup_pending: animation_timeout` às 21:57:43Z; depois de 3 s reativado, snapshot `null`, intenção ainda `sit`, owner 0 e pose `HumanM@Idle01`. Portanto a reserva ficou retida até o fluxo de saída e limpeza, sem claim órfão.
- Limitação observada: desativar o componente pausa o polling da operação; OnDisable registra cleanup pendente e um retry tardio registrou `animation_timeout`, embora o Animator tenha completado a saída e a reserva tenha sido liberada. O comportamento foi seguro quanto à ocupação, mas não é silencioso; `npc-integration-validation` deve decidir se o diagnóstico transitório deve virar warning/retry contínuo.
- Os testes existentes incluídos no harness/EditMode cobrem token de operação antigo: o token antigo retorna falha/`operation_cancelled` e não modifica a operação substituta. Interrupção externa real já havia sido observada no cenário, com `ActivityCancelled`, retenção durante `stand_up` e `ResourceReleased` em `cleanup_retry`.

### T018 — configuração ausente e falhas finitas — 2026-11-27

| Caso | Método/observação | Resultado |
|---|---|---|
| Provider ausente | Em Play Mode, removi temporariamente a referência `dataProvider` de Ivo, forcei reinitialização e consultei o host/owner/transform antes e depois de 1 s. | `missing_schedule_data_provider` foi emitido uma vez; o driver também registrou `controller_not_initialized` uma vez. `ready=false`, snapshot null, owner 0 e posição permaneceu `(-4.02, 6.49, -8.57)`. Referência original restaurada e host voltou a `ready=true`. |
| Arquivos JSON NPC ausentes | Em Play Mode, esvaziei temporariamente o array `npcSchedules` do provider compartilhado e reinicializei Luana. | Diagnóstico `missing_npc_schedule_assets`; host não ficou pronto, não criou snapshot/execução nem claim, e posição ficou em `(-5.00, 6.85, -9.43)`. Recarreguei os três TextAssets originais por AssetDatabase e Luana voltou a `ready=true`. |
| Host/controller duplicado | Probe do driver já executado em Play Mode às 21:47Z, com referência duplicada e depois ID duplicado. | Uma mensagem por problema; nenhuma duplicata aceita. Driver voltou a três hosts únicos após restaurar a configuração runtime. |
| Destino inativo | Desativei somente `beach_east_path` em Play Mode e avancei o relógio de 11:00 para 13:00. | Ivo e Luana registraram uma execução `wander` com `target_unavailable`; Helena apresentou `operation_cancel_failed` ao fechar uma ação anterior. Ivo/Luana tentaram o fallback `idle` uma vez, mas ele herdou `target=beach` e falhou `destination_unavailable`. Nenhum NPC teleportou para o waypoint inativo, nenhum slot ficou reservado e não houve repetição por frame. Ao reativar o ponto, Ivo iniciou uma única execução `wander` com caminho real. **Limite:** o fallback de `idle` não substitui um destino inválido pelo home point porque o intent preserva o target não vazio. |
| Rejeição de animação | Em Play Mode, passei `npc_world_missing_test` ao `NpcAnimatorBindings.TryBegin` com um `ActivityOperation` válido e identidade de teste. | Retornou `accepted=false`, `reason=animation_binding_missing`, sem ticket, reserva ou alteração de posição. O adapter rejeitou a animação antes de acionar o Animator. |

Os probes que alteraram provider, ponto ativo e hosts ocorreram só em runtime; referência/ponto foram restaurados e nenhuma dessas mutações foi salva na cena.

### T019 — dia físico completo no relógio original — 2026-11-27

- Play Mode abriu com o valor serializado da cena `10.5` (Day 1 `10:30`), três hosts prontos e projeção `Day 1 / 630`. Para começar a janela no meio do dia, avancei o relógio existente a `13:00` e habilitei `CoastalDayNight.AdvancesAutomatically`; `cycleSeconds=96`, `Time.timeScale=1` e nenhum relógio paralelo.
- O ciclo real cruzou meia-noite e completou Day 2 `13:00`; a observação continuou até Day 4 `01:38`, excedendo 24 horas de jogo. Durante a janela: os três começaram em `wander:village_square`; os três foram a seus `world_*_home` durante a noite (move concluído e execução `idle`); na manhã seguinte chegaram a Largo; às 11:00 cada um reservou um slot distinto, passou por aproximação/alinhamento/animação sentada; às 13:00 a saída terminou, os três emitiram `ResourceReleased` e começaram `wander:beach`.
- Snapshots ao vivo: Day 2 `10:04` — os três em `wander:village_square`, caminhando na praça; Day 2 `13:45` — os três em `wander:beach`, `Moving`, `ReservationCount=0`; Day 4 `01:38` — os três em `idle`, cada transform a 0.20–0.21 m de seu ponto pessoal (`world_ivo_home`, `world_helena_home`, `world_luana_home`), todos com owner 0. A câmera runtime do Game View foi registrada em `evidence/day2-13-45-npc-camera.png`; os logs da execução incluem os IDs de execução e eventos de release.
- Nas transições observadas houve uma execução por NPC/ocorrência, substituída na fronteira de agenda; não houve replay de janela passada nem início de uma atividade em cada frame. Não ficaram claims ativos após o assento ou no retorno para casa.

### T020 — controles do relógio em Play Mode — 2026-11-27

- O painel existente foi aberto e fotografado em `evidence/debug-panel-open.png`. Seus eventos reais de botão e slider foram acionados pelo Unity MCP: `autoButton` pausou em 17:53 e o valor permaneceu idêntico após 2 s; `timeSlider` avançou para 19:00 e selecionou somente `idle` nos três hosts; depois avançou para 23:58, e `autoButton` religou o relógio. O rollover natural atualizou a projeção para Day 5 sem trocar a execução `idle`.
- O slider então retrocedeu o relógio para 01:00. O driver registrou uma única vez `clock_regressed`; manteve o último snapshot monotônico (Day 5 05:30) e a mesma intenção/execução dos três hosts, sem resolver agenda retroativamente. Um avanço posterior para 15:30 resolveu somente a intenção atual `wander:beach` para todos, sem replay das janelas anteriores.
- Limitações verificadas/documentadas: o dia e weekday da projeção são de sessão e não persistem; não há data/calendário civil; retroceder o relógio bloqueia a projeção até avançar além da última hora válida ou reiniciar a sessão. O ciclo existente é 96 s por 24 h e foi observado em `timeScale=1`. O atalho F3, simulado via MCP, não abriu visualmente o painel nesta execução; o screenshot confirma o painel e os eventos de seus botões/slider, mas o atalho de teclado continua pendente de validação manual.

### T021 — reinício de Play Mode — 2026-11-27

- Antes de parar: driver pronto, três hosts IDs únicos `world_helena/world_ivo/world_luana`, zero claims. Parei o Play Mode sem salvar estado runtime. Em Edit Mode, a cena permaneceu `dirty=false`; hora voltou ao serializado `10.5`, auto-clock `false`, provider tinha os três JSONs, o waypoint `Rua leste — praia` estava ativo e a câmera runtime temporária não existia.
- `ProjectSettings/EditorSettings.asset`: `m_EnterPlayModeOptionsEnabled=1`, `m_EnterPlayModeOptions=0` (domain reload e scene reload não estão desabilitados). Ao iniciar Play Mode novamente, Unity `codeLoadCount` passou de 19 para 20. O driver recompôs três hosts únicos e prontos; snapshots novos `world_*‑exec‑000001`, intenção `wander:village_square` em Day 1 10:30; `ReservationCount=0` nos três. Não foram observadas cópias extras, owner residual ou câmera temporária após o restart.

## Pendências para `npc-integration-validation`

### Entrega T023 — inventário e pendências

**Arquivos da feature**

- Cena: `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`.
- Dados: `Freyja/Assets/_Project/NpcScheduling/Data/npcs/world_integration.meta` e três pares JSON/`.meta`: `world_ivo`, `world_helena`, `world_luana`.
- Código novo: `WorldClockProjection.cs`/`.meta`; `NpcWorldRuntimeDriver.cs`/`.meta`.
- Código alterado: `ActivityExecution.cs`, `ActivityExecutionModels.cs`, `NpcActivityController.cs`, `NpcUnityActivityPorts.cs`; harness `ExecutionHarness.cs` e testes `ExecutionTests.cs`.
- Spec Kit: `spec.md`, `plan.md`, `tasks.md`, `research.md`, `data-model.md`, `quickstart.md`, `validacao.md`, `contracts/runtime-composition.md` e artefatos auxiliares sob `specs/092-npc-world-integration/`.
- O Unity reordenou temporariamente scripting defines em `Freyja/ProjectSettings/ProjectSettings.asset`; T024 removeu essa alteração de serialização, e o arquivo não consta no diff final.
- O diretório local não rastreado `prompts/` foi preservado e não faz parte desta feature.

**Evidências visuais mantidas**

- `evidence/benches-grounded-detail.png` e `evidence/benches-grounded-sceneview.png`: bancos apoiados no piso da praça.
- `evidence/day2-13-45-npc-camera.png`: captura de câmera runtime da ilha durante o ciclo.
- `evidence/playmode-day2-beach-camera.png`: captura de câmera runtime anterior durante retorno à praia.
- `evidence/debug-panel-open.png`: painel existente com o relógio pausado.
- Capturas em Scene View sob o cenário ou Game View mostrando apenas o protagonista foram removidas; não são evidência dos NPCs.

**Falhas/limites observados**

- Probe ao desativar `NpcActivityController` em assento deixa cleanup pendente enquanto não há polling e pode emitir `animation_timeout`; a animação de saída concluiu, a reserva permaneceu retida e depois foi liberada.
- Com waypoint inativo, a rotina `wander` falha corretamente; o fallback `idle` herda o destino inválido e falha uma vez em `destination_unavailable`, sem claim ou teleport.
- Não testei sintaxe JSON malformada, `GameObject` desativado/destroy/unload durante ação, interrupção por diálogo real, atalho F3 por teclado nem controles do protagonista. W03/W14/W19 refletem isso; não são PASS completos.
- `--simulate-week` é simulação lógica e mantém duas falhas físicas simuladas pré-existentes (`demo_farmer/path_unreachable`, `demo_fisherman/animation_rejected`), `physicalValidation=false`.

**Próxima feature `npc-integration-validation`**

- Regressão jogável dos controles do protagonista na ilha e validação manual de F3 abrindo o painel.
- Disable de GameObject inteiro e destroy/unload durante ação, incluindo cleanup sem retomar uma intenção antiga.
- Teste de JSON malformado/ID duplicado e rotina com referência inválida no provider, conferindo exatamente uma mensagem por host.
- Matriz repetível de destinos/NavMesh inacessíveis e rejeições reais de animação dentro de uma receita, verificando cleanup/diagnóstico.
- Avaliar fallback do destino inválido para o ponto pessoal de casa e o ruído de `animation_timeout` no disable tardio.

### T024 — verificações finais — 2026-11-27

- Unity MCP no Editor alvo (Unity 6000.3.25f1): `script.recompile` retornou `up_to_date`. Console ground truth final reportou `compilationFailed=false`, `compiling=false`, 0 erros, 2 warnings e 6 logs. Os warnings são: timeout de rede/acesso à conta Unity AI e o aviso esperado `Duplicate SmartObject ID 'same'` disparado pelo teste `SmartObjectRegistryTests.DuplicateIdsAreDiagnosedAndNeverReplaceLookup`; não há warning de compilação.
- Unity Test Runner batch: XML `/tmp/npc-world-integration-editmode.xml`, `result=Passed`, 40 testes, 40 passados, 0 falhos, 0 inconclusivos ou ignorados. A consulta do MCP após o encerramento retornou `no_tests` (sem execução assíncrona em andamento); por isso o resultado é atribuído ao XML/log batch, não à consulta MCP.
- `.NET --validate`: PASS, 5 schedules, 15 atividades, 3 overrides e 3 receitas executáveis. `.NET --test`: PASS, 174 assertions (o warning pré-existente de lacuna `sleep_early` para `demo_fisherman` continua). `.NET --simulate-week`: 7 dias, 3 NPCs, 1.047 eventos, 2 falhas simuladas (`demo_farmer/path_unreachable`, `demo_fisherman/animation_rejected`), 19 cancelamentos, 16 reservas, 16 liberações e 0 claims residuais; `physicalValidation=false`, portanto não é evidência de movimento físico.
- `git diff --check` foi revisado: sinaliza espaços finais somente nas linhas YAML Unity de strings vazias (`m_Name: ` e `runtimeStatus: `) da cena. Esses espaços são a representação serializada pelo Unity para escalares vazios e foram mantidos para evitar edição manual da cena exportada. A alteração transitória de ordem em `scriptingDefineSymbols` de `Freyja/ProjectSettings/ProjectSettings.asset` foi removida; o arquivo não está no inventário final.
- Inventário final da feature: cena `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`; dados `Freyja/Assets/_Project/NpcScheduling/Data/npcs/world_integration/` (três JSON e respectivos `.meta`, mais `.meta` do diretório); código novo `WorldClockProjection.cs`/`.meta` e `NpcWorldRuntimeDriver.cs`/`.meta`; código alterado `ActivityExecution.cs`, `ActivityExecutionModels.cs`, `NpcActivityController.cs`, `NpcUnityActivityPorts.cs`; harness/testes `ExecutionHarness.cs` e `ExecutionTests.cs`; Spec Kit `spec.md`, `plan.md`, `tasks.md`, `research.md`, `data-model.md`, `quickstart.md`, `validacao.md`, `contracts/runtime-composition.md` e `evidence/` listados acima. O diretório preexistente e não rastreado `prompts/` foi preservado sem alteração e excluído da feature.
- O fechamento final mantém W03 e W14 como PARTIAL e W19 como NOT RUN. Não foi observado teste de sintaxe JSON malformada, GameObject inteiro desativado/destroy/unload, início de diálogo real, atalho F3 pelo teclado ou controle jogável do protagonista; nenhuma dessas condições é apresentada como validada.
