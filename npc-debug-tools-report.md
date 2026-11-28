# Relatório — NPC Debug Tools

## Resultado

**PARCIAL.** Implementação disponível no Editor e exercitada com os três NPCs reais em Play Mode. A matriz D01–D26 e as limitações estão em [validation-matrix.md](specs/094-npc-debug-tools/evidence/validation-matrix.md). D24 e D25 permanecem parciais; D13 não é suportado por falta de contrato de override. Cancelamento sentado/em deslocamento, desativação de NPC reservado, reload de cena, export JSON, overflow, falha injetada no cleanup, busca/seleção/Locate pelos handlers da janela, overlay atualizado e build foram verificados. A microamostra API aberta/fechada foi medida, mas não equivale a perfil completo do Editor.

## Uso

Abra **Freyja > NPC > Debug Monitor**. Em Edit Mode a janela identifica que dados ao vivo/comandos estão indisponíveis. Em Play Mode mostra rotina prevista e efetiva, navegação, animação, Smart Objects, eventos e razões observadas. Os controles usam o relógio, host e executor existentes: pausa afeta somente o calendário; taxa também altera somente o calendário; avançar encaminha para o relógio real; cancelar pode ficar pendente até cleanup; reavaliação sinaliza o tick real. Override informa `no_temporary_override_contract`.

Para harness/editor, chame `Freyja.NpcDebug.Editor.NpcDebugDiagnostics.CaptureJson(null, true)`, `CaptureNpcJson("world_ivo", true)` ou `ExportJson(null, true)` pelo Unity MCP `script_eval`; os arquivos ficam sob `Temp/NpcDebugTools/`. A API não exige janela aberta. Ver [contrato](specs/094-npc-debug-tools/contracts/editor-diagnostics-api.md).

## Evidência real

- `diagnostics-three-seated-playmode.json`: às 11:00, Helena, Ivo e Luana executando `sit`, animação `Armature|Sitting_Idle_Loop`, 3 slots ocupados e 1 disponível.
- `diagnostics-two-slot-dispute-playmode.json`: Helena e Ivo ocupam duas vagas; Luana registra `target_unavailable`, inicia fallback `idle` e recebe `destination_unavailable` do executor existente.
- `diagnostics-sit-cancel-cleanup-playmode.json`: cancelamento de Ivo por `debug_cancel`; cleanup encerra a execução, libera o recurso via sistema proprietário e snapshot indica zero falhas.
- `diagnostics-midnight-playmode.json`: relógio real chegou ao dia 3, 00:21.
- Exportações recentes: [diagnostics-export-global-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-export-global-playmode.json) (3 NPCs) e [diagnostics-export-world-ivo-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-export-world-ivo-playmode.json) (Ivo individual). A captura da janela atualizada está em [monitor-live-after-recompile.png](specs/094-npc-debug-tools/evidence/monitor-live-after-recompile.png).
- Cancelamento em deslocamento: [diagnostics-cancel-moving-world-helena.json](specs/094-npc-debug-tools/evidence/diagnostics-cancel-moving-world-helena.json). Desativação com slot ocupado: [diagnostics-npc-deactivation-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-npc-deactivation-playmode.json). Reload de cena: [diagnostics-scene-reload-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-scene-reload-playmode.json).
- Ring overflow: [diagnostics-ring-overflow-editmode.json](specs/094-npc-debug-tools/evidence/diagnostics-ring-overflow-editmode.json). Falha de cleanup injetada: [diagnostics-cleanup-failure-injected.json](specs/094-npc-debug-tools/evidence/diagnostics-cleanup-failure-injected.json).
- Busca `world_ivo`, persistência de seleção e Locate por handlers da janela: [diagnostics-ui-interactions-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-ui-interactions-playmode.json), com [captura do monitor](specs/094-npc-debug-tools/evidence/monitor-ui-interaction-playmode.png). Cancelamento duplicado: [diagnostics-duplicate-cancel-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-duplicate-cancel-playmode.json). Serialização de NaN/Infinity: [diagnostics-nonfinite-json-editmode.json](specs/094-npc-debug-tools/evidence/diagnostics-nonfinite-json-editmode.json).
- Microbenchmark de 100 capturas com janela aberta/fechada: [diagnostics-capture-microbenchmark-playmode.json](specs/094-npc-debug-tools/evidence/diagnostics-capture-microbenchmark-playmode.json). `CaptureSnapshot` reportou média 0,030 ms / 0 B aberta e 0,029 ms / 0 B fechada. A chamada síncrona bloqueia o loop durante a amostra; repaint, CPU por frame, Editor update completo e Player não foram medidos.
- Imagens: `monitor-editmode.png`, `monitor-sit-playmode.png`, `monitor-fallback-playmode.png`, `monitor-ui-interaction-playmode.png`, `scene-overlay-editor-window-playmode.png`. A captura final da janela Scene mostra as três labels separadas e caminhos visíveis. Bounds de região continuam indisponíveis pelo contrato de Smart Object existente; o overlay exibe o ID da região.

Trechos abaixo foram extraídos dos JSONs reais citados (campos selecionados):

```json
{
  "absoluteDay": 1,
  "minuteOfDay": 660,
  "slotAvailableCount": 1,
  "slotOccupiedCount": 3,
  "npcId": "world_helena",
  "intendedActivity": "sit",
  "executedActivity": "sit",
  "executionState": "Executing",
  "executionId": "world_helena-exec-000002",
  "effectiveTarget": "npc_world_bench_west",
  "animationState": "Armature|Sitting_Idle_Loop"
}
```

Na disputa com um banco desativado apenas durante Play Mode, o histórico real de Luana contém `ActivityFailed(sit, target_unavailable)`, `ActivityStarted(intended=sit, executed=idle)` e `ActivityFailed(idle, destination_unavailable)`. No cancelamento de Ivo, o histórico contém `ActivityCancelled(reason=debug_cancel)` seguido por `ResourceReleased`; o snapshot posterior mostra `executionStatus=unavailable`, duas vagas disponíveis, duas ocupadas e `failedNpcCount=0`.
- Regressões executadas após a correção: SmartObjects EditMode 12/12 e PlayMode 7/7; Animation EditMode 12/12 e PlayMode 1/1. Não há suites para Agenda/Execução/Navegação nessa bateria, então D25 permanece parcial.
- StandaloneOSX build pós-correção: sucesso, 0 erros e 0 avisos; tipos de diagnóstico Editor não encontrados no Player inspecionado.
- Após testes, Editor voltou a Edit Mode. A recompilação final passou (`failed=false`, `errors=[]`) e não há erro corrente; Console reportou 1 aviso corrente de conectividade do Unity AI Toolkit (“Account API did not become accessible within 30 seconds”) e o buffer histórico contém erros do `Unity.Pipeline.BasePipelineServer` e o erro de compilação anterior já corrigido. O Console não foi limpo manualmente.

## Arquivos da feature

Novos: `Freyja/Assets/_Project/Editor/NpcDebug/` (models, event buffer, diagnostics API, monitor e Scene overlay, mais `.meta`); `specs/094-npc-debug-tools/` (spec, plano, contratos, tarefas, checklist e evidências); este relatório.

Alterados para portas de diagnóstico: `CoastalDayNight.cs`, `NpcActivityController.cs`, `NpcWorldRuntimeDriver.cs`, `SmartObjectRegistry.cs`, `ActivityExecution.cs` (falha do release agora troca a razão terminal para `cleanup_failed`; recompilada, buildada e exercitada com falha injetada). O controller também contém alterações pré-existentes da integração 093; há mudanças em `NpcUnityActivityPorts.cs`/testes e um diff em `ProjectSettings.asset` que adiciona `InputSystem_Actions.inputactions` a `preloadedAssets`. Essas mudanças do working tree foram preservadas e não são atribuídas à 094. Ver `git status` antes de separar/commitar.

## Próxima feature

Antes do `npc-schedule-editor`, a medição de custo geral do Editor (repaint/update e CPU por frame) pode complementar o microbenchmark restrito D24. D25 fica parcial pela ausência de suites para Agenda/Execução/Navegação. Override continua sem suporte até existir contrato no executor/agenda. O fallback ocioso que terminou em `destination_unavailable` segue como pendência upstream.
