# Matriz de validação D01–D26

Ambiente: Unity 6000.3.25f1, `Ilha_Principal`, NPCs reais `world_helena`, `world_ivo`, `world_luana`. PASS significa o escopo observado descrito, não cobertura de subcasos não exercitados.

| ID | Status | Evidência / limite |
|---|---|---|
| D01 | PASS | Recompilação Unity MCP após a correção de cleanup e do Scene View overlay: `failed=false`, `errors=[]`, `compilationFailed=false`; Editor voltou a Edit Mode. |
| D02 | PASS | `monitor-editmode.png`: Edit Mode informa indisponibilidade de dados ao vivo. |
| D03 | PASS | `monitor-playmode.png` e `diagnostics-global-playmode.json`: três IDs reais sem duplicatas. |
| D04 | PASS (handlers) | `diagnostics-ui-interactions-playmode.json`: query `world_ivo` retorna só Ivo; seleção `world_helena` persiste ao atualizar; Locate seleciona `Ivo Batista`. Métodos reais da janela foram invocados via reflection; não houve automação de mouse. Janela capturada em `monitor-ui-interaction-playmode.png`. |
| D05 | PASS | `diagnostics-global-playmode.json`, `diagnostics-three-seated-playmode.json`; captura de wander, idle e sit reais. |
| D06 | PASS (com limite upstream) | `diagnostics-two-slot-dispute-playmode.json`, `monitor-fallback-playmode.png`: duas ocupações, `target_unavailable`, fallback idle e falha subsequente `destination_unavailable`. A atividade fallback idle do sistema anterior não conclui. |
| D07 | PASS | Mudança real de wander para sit ao atravessar 11:00, sem restart na reavaliação repetida; capturas sit. |
| D08 | PASS | Pause/resume altera avanço do calendário; movimento e execução física não são pausados por essa API. |
| D09 | PASS | Avanço de 15/60 min e cruzamento de meia-noite observados; `diagnostics-midnight-playmode.json` mostra dia 3, 00:21. Atividades puladas não aparecem concluídas. |
| D10 | PASS | Taxa 5x observada no relógio/calendário; `Time.timeScale` não é usado. Sem benchmark de locomoção. |
| D11 | PASS | Reavaliação repetida aceita sem alterar ID da execução ativa de Helena. |
| D12 | PASS | Cancelamento sentado de Ivo e em deslocamento de Helena foram aceitos e limparam a execução sem falha: `diagnostics-sit-cancel-cleanup-playmode.json`, `diagnostics-cancel-moving-world-helena.json`. |
| D13 | BLOCKED | Override retorna `unsupported`, `no_temporary_override_contract`; não existe mecanismo proprietário compatível. |
| D14 | PASS | Avanço `-1` rejeitado sem mutação (`diagnostics-invalid-clock-command.json`). Cancelamento ativo aceito e cancelamento duplicado imediatamente rejeitado com `no_active_execution`: `diagnostics-duplicate-cancel-playmode.json`. |
| D15 | PASS | Falhas/fallback reais com razões em `diagnostics-two-slot-dispute-playmode.json`. |
| D16 | PASS | Helena foi desativada enquanto ocupava slot em Play Mode: estado inativo, duas vagas livres e dois slots ainda ocupados. `diagnostics-npc-deactivation-playmode.json`. |
| D17 | PASS (com limite de regiões) | `scene-overlay-editor-window-playmode.png` após o ajuste mostra as três etiquetas separadas e os caminhos na Scene View real. Bounds de região não existem no contrato observado; só o ID da região pode ser exibido. |
| D18 | PASS | Histórico ordenado mostra início/falha/fallback/cancelamento em capturas JSON e `monitor-fallback-playmode.png`. Eventos anteriores à inscrição não são reconstruídos. |
| D19 | PASS | Probe Edit Mode finito emitiu 514 eventos pelo subscriber real; buffer retornou 512, `droppedCount=2`, ordens 3–514. Histórico limpo após o probe. `diagnostics-ring-overflow-editmode.json`. |
| D20 | PASS | Exportações global e individual em Play Mode retornaram `file_written` e os arquivos foram lidos/parseados: `diagnostics-export-global-playmode.json` (3 NPCs) e `diagnostics-export-world-ivo-playmode.json` (1 NPC). |
| D21 | PASS | `diagnostics-closed-window-global-playmode.json` e `diagnostics-closed-window-world-ivo-playmode.json` obtidos sem janela visível. |
| D22 | PASS | Três ciclos de abrir/fechar monitor e repetidos ciclos Play Mode executados; sem erro de compilação observado. |
| D23 | PASS | Ilha_Principal foi recarregada em Play Mode depois de desativação temporária; nova captura mostrou 3 NPCs ativos/prontos, zero falhas e 4 slots livres. A cena foi restaurada limpa após sair do Play Mode; `diagnostics-scene-reload-playmode.json`. |
| D24 | PARTIAL | `diagnostics-capture-microbenchmark-playmode.json`: 100 capturas API, 3 NPCs, 0,030 ms/0 B com janela residente e 0,029 ms/0 B fechada. Não mede repaint/update completo nem custo de Player; limites em `performance.md`. |
| D25 | PARTIAL | Reexecutadas após a correção: EditMode SmartObjects 12/12, Animation 12/12; PlayMode SmartObjects 7/7, Animation 1/1. Fluxos NPC reais foram observados, mas não há suites próprias para Agenda/Execução/Navegação. |
| D26 | PASS | Build StandaloneOSX concluída após a correção final: 0 erros e 0 avisos. Tipos de diagnóstico Editor não aparecem em `ScriptingAssemblies.json` nem nas DLLs Managed inspecionadas. |

Teste complementar para **T042**: `ActivityExecution.RetryCleanup` foi exercitado com falha injetada. Após `debug_cancel`, restauração de pose assíncrona e release recusado, a razão final foi `cleanup_failed`; cleanup permaneceu pendente e ownership foi preservado. Evidência: `diagnostics-cleanup-failure-injected.json`.

Teste complementar para serialização numérica: NaN/Infinity passaram por `ToVector` e pelo conversor JSON de produção, gerando `null` e `unavailable_non_finite` em `diagnostics-nonfinite-json-editmode.json`.

Resumo: cobertura funcional principal exercitada em Play Mode real, mas validação global PARCIAL pela medição de escopo restrito D24, ausência do contrato de override D13 e ausência de suites do domínio em D25.
