# Relatório de implementação — NPC Schedule Editor

**Estado:** Código compilado e janela inspecionada pelo Unity MCP; suites Unity existentes passaram. Prova de gravação através dos controles, Debug Monitor correlacionado e mudança física em Play Mode pendentes porque o Mac segue bloqueado para interação CUA.

## Baseline e escopo

- Unity `6000.3.25f1`; projeto `Freyja/`; cena principal `Ilha_Principal`.
- Branch de entrega: `095-npc-schedule-editor`, derivada do baseline local `094-npc-debug-tools`. A pasta não rastreada de anexos e o histórico local foram preservados.
- Os três NPCs ativos são `world_ivo`, `world_helena` e `world_luana`. A cena referencia os três JSONs; `templates` e `overrides` estão vazios no provider dessa cena. Dados `demo_*` são biblioteca/testes, não prova de comportamento dos três NPCs.
- Catálogo: 15 atividades declaradas; receitas disponíveis atualmente: `idle`, `wander`, `sit`.
- Sem novos behaviors, receitas, schema, cenas, prefabs, pacotes, narrativas ou sistemas de população.

## Implementação

- Janela IMGUI em `Freyja > NPC > Schedule Editor` com busca de documentos, timeline de 24 horas, lista/form de blocos, horário exato, CRUD/duplicação, opções do catálogo, herança/template, condições suportadas, validação, preview e diagnóstico runtime.
- JSON v1 existente continua sendo fonte persistida. Draft, cache e undo/redo são estados de sessão. Save é explícito, Edit Mode only, revalida o candidato, compara SHA-256, substitui o arquivo e reimporta somente o asset selecionado.
- Preview usa `ScheduleDataLoader`, `ActivityRecipeLoader` e `ScheduleResolver`. Atividades sem receita são identificadas; o preview não executa ação ou reserva.
- Destinos continuam simbólicos. O snapshot da cena diferencia `resolved`, `ambiguous`, `unavailable`, `invalid_target` e `unverified`; slots são dinâmicos e não são persistidos.
- A integração usa `NpcDebugDiagnostics` e abre o monitor existente focado no NPC. Não cria buffer de eventos/runtime paralelo.
- Cache de validação por revisão, snapshot de destino fora de `OnGUI`, invalidação por mudança/import de projeto e captura de monitor a cada 0,5 s foram revisados em fonte. Responsividade real permanece não medida.

## Guia de utilização

1. No Edit Mode, abra `Freyja > NPC > Schedule Editor` e filtre pelo ID do NPC.
2. Confira a origem e a indicação `RUNTIME`, `DEMO` ou `BIBLIOTECA`; selecione um bloco na timeline/lista para ver e editar seus campos.
3. Use `Adicionar`, `Duplicar bloco` ou `Remover`; intervalos seguem `[início,fim)`, com `24:00` apenas como fim. Lacunas são permitidas; sobreposições ou erros do loader bloqueiam save.
4. Escolha atividade no catálogo e um destino simbólico compatível/observável. Atividade catalogada sem receita não está fisicamente implementada. Um destino `unverified` ainda não tem prova no snapshot da cena.
5. Templates/overrides usam operações/campos aceitos pelo core. O preview permite hora, dia, clima e flags e mostra intenção prevista, sem alterar o mundo.
6. Undo/redo/reverter afetam o draft. Salvar exige Edit Mode, não haver conflito externo e validação válida. Em Play Mode a janela é somente leitura; a agenda salva entra pelo próximo carregamento normal.
7. Para comparar execução real, use a seção de runtime e `Abrir Debug Monitor neste NPC` depois de entrar em Play Mode.

**Limite atual:** a janela foi aberta e capturada pelo Unity MCP, mas não consegui interagir com seus campos porque o Mac continua bloqueado. Os fluxos de edição/salvamento descritos aqui estão implementados em fonte; a gravação via interface ainda precisa ser exercitada.

## JSON real antes/depois e evidência física

Antes da edição, `world_ivo.json` contém este bloco real:

```json
{ "id": "square_sit", "start": "11:00", "end": "13:00", "activityId": "sit", "targetId": "village_square" }
```

**Depois da edição pela UI:** não existe ainda. Nenhum save visual foi feito; por isso não há diff após, snapshot runtime correlacionado ou captura Play Mode a reportar. A feature não está validada ponta a ponta.

Captura feita pelo Unity MCP: [editor-window.png](evidence/editor-window.png). Mostra a janela no documento runtime `world_helena`, caminho `Assets/_Project/NpcScheduling/Data/npcs/world_integration/world_helena.json`, agenda efetiva de cinco blocos e timeline de 24 horas. O menu `Freyja/NPC/Schedule Editor` abriu normalmente.

Hashes baseline das três agendas e detalhes de conexão estão em [evidence/baseline.md](evidence/baseline.md). Classificação individual SE01–SE34, com limites, está em [evidence/validation-matrix.md](evidence/validation-matrix.md). Ainda não há captura do Debug Monitor/runtime nem diff JSON posterior a uma edição visual.

## Testes e validações

| Checagem | Resultado observado | Limite |
|---|---|---|
| Schedule Simulator `--validate` | PASS: 5 NPCs, 15 atividades, 3 overrides, 3 receitas | Harness/core; não compila ou testa o Editor |
| Schedule Simulator `--test` | PASS: 174 assertions | Avisos de lacuna do `demo_fisherman` mantêm baseline; sem cobertura da janela |
| `git diff --check` e scan de whitespace em fontes/docs novos | PASS após o último ajuste de fonte e das evidências | Sem espaços/tabs finais encontrados |
| Unity compile/Console/menu/GUI | PASS | MCP recompilou sem erros, menu abriu e janela capturada; último ground truth (04:12 UTC): `compilationFailed=false`, `consoleErrors=0`, um warning ativo sem mensagem disponível para classificação |
| Unity Edit Mode tests | PASS: 40/40 | Unity MCP, 2026-11-28; testes existentes não cobrem o editor de rotinas |
| Unity Play Mode suite | PASS: 9/9 | Unity MCP, 2026-11-28; testes existentes cobrem Smart Objects/animação/world smoke, não a alteração física do cronograma |
| Documentos dos três NPCs ativos | PASS: runtime-loaded e válidos para `world_helena`, `world_ivo`, `world_luana` | Probe `NpcScheduleEditorRepository.ValidateAll` via Unity MCP com `Ilha_Principal` ativa; não exercitou a seleção visual de cada NPC |
| Probes via Unity MCP `script_run` (temporários, removidos após execução) | PASS: JSON inválido/schema 99/horário inválido rejeitados; no-op, conflito externo e rollback de escrita confirmados | `malformedRejected=true`, `unknownSchemaRejected=true`, `invalidTimeRejected=true`, `noOpRoundTripPassed=true`, `conflictPreserved=true`, `failedWriteRestored=true`; import-failure recovery ainda não foi testado |
| Probe da sessão via Unity MCP `script_run` | PASS: undo/redo/revert e documento malformado somente leitura | Resultado: `undoRedoRevertPassed=true`, `malformedReadOnlyPassed=true`; não testa os controles UI |
| Probe de candidatos/catálogo via Unity MCP | PASS: sobreposição e alvo ausente rejeitados; nova atividade vista no catálogo e receita ausente distinguida | `overlapRejected=true`, `targetlessRejected=true`, `dynamicActivityDiscovered=true`, `dynamicActivityWithoutRecipeDetected=true`; a atividade foi adicionada apenas em memória |
| Probe `Save()` via Unity MCP `script_run` | PASS: em falha forçada de escrita o arquivo original foi restaurado, o rascunho permaneceu dirty e o status reportou falha | `originalRestored=true`, `draftRetained=true`, `failureReported=true`; método invocado por reflexão, sem clique visual |
| Probe de conflito por remoção via Unity MCP | PASS: após remoção externa depois do fingerprint, o save foi recusado e o arquivo permaneceu ausente | `deleteConflictDetected=true`; documento de prova removido do projeto |
| Guarda da sessão em Play Mode | PASS no nível da sessão | Probe em Play Mode: `editorIsPlaying=true`, `writeBlocked=true` ao tentar `ReplaceDraft`; a janela não foi interagida visualmente |
| Play Mode e mudança física | BLOCKED | Mac segue bloqueado para CUA. Não foi salva alteração via UI nem observada execução correspondente |
| Player build/editor isolation | NOT RUN | Nenhum build foi executado nesta entrega |

## Arquivos de código e documentos

- Novos: `Freyja/Assets/_Project/Editor/NpcScheduleEditor/` (`NpcScheduleEditorWindow.cs`, `NpcScheduleEditorSession.cs`, `NpcScheduleEditorRepository.cs`, `NpcScheduleEditorPreview.cs`, `NpcScheduleEditorDestinations.cs` e `.meta`).
- Alterado: `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugMonitorWindow.cs` para abrir/focar o NPC existente.
- Spec Kit: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/editor-data-contract.md`, `quickstart.md`, `tasks.md`, `checklists/requirements.md`, este relatório e `evidence/{baseline,validation-matrix}.md`.
- Nenhum JSON de rotina ou cena alterado. Os três hashes `world_*` continuam iguais ao baseline na última leitura.

## Handoff para `npc-activity-library`

Uma atividade nova deve ser adicionada ao `activities.json` e à fonte existente de receitas em `execution/recipes.json`; o editor consulta esses documentos e não exige uma lista de atividades no código. O contrato da atividade determina `targetKind`; a receita define as operações existentes, alvos/capabilities e animações. A biblioteca seguinte deve fornecer behavior/recipe e evidência de destino real. Em seguida, validar automaticamente a aparição no dropdown, o status de executabilidade e a compatibilidade simbólica sem alterar o editor.

## Pendências antes de concluir

1. Desbloquear o Mac para operar os controles da janela Unity.
2. Testar round-trip, herança, flags/preview, conflitos, falha de IO e os fluxos de menu/debug.
3. Salvar uma mudança visual em um dos três NPCs, capturar diff + Debug Monitor + comportamento físico em Play Mode e restaurar o JSON baseline.
4. Atualizar resultados finais, repetir checks após alterações e marcar as tarefas concluídas/bloqueadas com evidência.

**Limitação residual de persistência:** o writer confere o fingerprint mais de uma vez antes de `File.Replace`, mas a comparação e a substituição não formam uma operação atômica do sistema de arquivos. Uma gravação externa simultânea exatamente nessa pequena janela ainda não foi excluída por teste nem por mecanismo de compare-and-swap.
