# Validação — NPC Navigation System

**Data:** 2026-11-27
**Branch/worktree:** `089-npc-navigation-system` — `/Users/bruno.duartec/Documents/Projects/Freyja-089-npc-navigation-system`
**Estado:** implementação executada e validada parcialmente; os limites não exercitados permanecem listados. A integração 090 foi commitada em `9c82ab1`; somente o `IslandWaypointWalker` foi estendido nesta feature. Contratos do Activity Core e o adapter não foram alterados.

## Ambiente e preflight

- Unity `6000.3.25f1`; projeto conectado: `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`; cena `Assets/_Project/Scenes/CadeiraSmartObject_Demo.unity`.
- Cena estava limpa em Edit Mode antes e depois (`dirty=False`). A cena não foi salva. Fixtures, alvos temporários, cápsulas e câmera existiram somente durante Play Mode e foram destruídos ao sair (`NAVTEST_* = 0`).
- `IslandWaypointNetwork` registrou `IlhaWaypointNavigation` em Play Mode. Triangulação observada: 8.145 índices.
- Agente existente: raio 0,25; altura 1,65; climb 0,4; inclinação máxima 46°; voxel 0,09; velocidade 1,6; aceleração 8; rotação 540°/s; stopping distance 0,12. Nenhum asset de NavMesh, terreno, cena, controle jogável ou pacote foi alterado.
- A cena oferece uma cadeira Smart Object com um slot `sit` e Transform `Approach`; não há Smart Object real de dois slots. Existem 15 `IslandWaypointWalker`, 14 desativados.

## Código e compilação

- `IslandWaypointWalker` preserva o `NavMeshAgent` existente e agora valida caminho completo, tolerâncias horizontal/vertical, invalidação, cancelamento por operação, progresso, watchdog, lifecycle e telemetria. Watchdog usa `Time.unscaledDeltaTime` enquanto o jogo está ativo e pausa quando `Time.timeScale == 0`.
- `NpcNavigationDemoHarness` oferece três fixtures, alvos, sequência finita de região, aproximação externa, comandos para cancelar/reemitir ticket e diagnósticos sob demanda. Não escolhe nem reserva Smart Objects, não cria pathfinder/agent paralelo, e registra transições sem log por frame.
- Fontes da branch foram copiadas temporariamente ao único projeto Unity conectado para importação e compilação. Após a alteração final do watchdog, Unity MCP recompile retornou `failed=false`, `errors=[]`, `compilationFailed=false`.
- Estado Unity observado ao encerrar os testes anteriores: `compilationFailed=false`, 0 erros atuais e 1 aviso atual no ground truth do Console. O buffer contém mensagens históricas de outras execuções; não são atribuídas à feature sem evidência temporal.

## Play Mode — três NPCs simultâneos

As três operações começaram no mesmo Play Mode como `Running`, com `PathComplete`, antes do deslocamento. Cada uma terminou `Succeeded` dentro das tolerâncias configuradas (0,20 m horizontal e 0,25 m vertical):

| Fixture / operação | Rota | Resultado observado |
|---|---|---|
| A-pier / op 2 (repetida op 3) | Píer — convés `(-36.73, 1.81, -55.80)` → Píer — saída `(-32.50, 1.93, -41.50)` | `Succeeded`, posição `(-32.65, 1.93, -41.63)`, erro horizontal ≈ 0,20 m e vertical ≈ 0,00 m, `PathComplete`, motivo `arrived`. |
| B-approach / op 1 (repetida op 2) | Venda `(-6.32, 7.05, -2.01)` → `Approach` real da cadeira `(-2.00, 6.99, -2.10)` | `Succeeded`, posição `(-2.20, 7.03, -2.10)`, erro horizontal ≈ 0,20 m e vertical ≈ 0,04 m, `PathComplete`, motivo `arrived`. Não foi movido à pose sentada. |
| C-ridge / op 2 (repetida op 3) | Crista `(5.00, 23.01, 50.00)` → patamar da igreja `(-1.66, 15.03, 25.00)` | `Succeeded`, posição `(-1.47, 15.03, 25.06)`, erro horizontal ≈ 0,20 m e vertical ≈ 0,00 m, `PathComplete`, motivo `arrived`. Rota existente percorreu trecho com mudança de elevação. |

Durante a rota C, telemetria MCP mostrou velocidade `(-0.06, 2.40, 3.35)`, direção `(-0.02, 0.58, 0.81)`, `PathComplete`, `elapsed≈6.94 s`, `noProgress≈0.004 s`; chegada em `≈8.48 s`. São leituras temporais de runtime; as capturas não são usadas isoladamente como prova de movimento.

### Recuperação e casos de falha executados

- **Ticket obsoleto/idempotência:** operação 5 foi substituída pela 6; `CancelMove(5)` retornou `false`, sem parar a op 6 (`Running`). Cancelar op 6 duas vezes retornou `true/true`; estado final `Cancelled`, motivo `cancelled`.
- **Destino fora do NavMesh:** rejeitado, `Failed/path_invalid`, posição preservada. Agente iniciado off-surface também falhou sem teleporte.
- **Destino destruído em movimento:** início aceito e `Running`; após destruir o Transform em Play Mode terminou `Failed/target_removed`.
- **Desativação:** desativar o walker durante rota terminou `Cancelled/walker_disabled`.
- **Pausa/watchdog:** após ajuste final, rota completa imobilizada (`NavMeshAgent.isStopped`) com limite `0,7 s`. Com `timeScale=0` por 1,2 s reais, permaneceu `Running`, `elapsed=0`, `noProgress=0`; após resume terminou `Failed/no_progress_timeout` em `≈0,700 s`. Em `timeScale=2`, também expirou em `≈0,704 s` não escalados, demonstrando tempo de runtime independente da aceleração do mundo.
- **Região finita:** B chegou a `Venda — banca` e em seguida a `Largo`; ambas `Succeeded`. Fixtures sem candidatos produziram `region_no_candidates`, sem iniciar loop.
- Nenhum `NavMeshObstacle` ou collider de bloqueio foi colocado no caminho. O watchdog foi testado com agente parado de forma controlada; avoidance de obstáculo dinâmico não está validado.

### Capturas de Play Mode

- `evidence/playmode-agent-a-in-progress.png`
- `evidence/playmode-agent-b-approach.png`
- `evidence/playmode-agent-c-arrival.png`
- `evidence/playmode-three-agent-arrival.png` (câmera de jogo original, sem os fixtures no enquadramento)
- `evidence/playmode-three-agent-scene-view.png` (enquadramento ruim; não usar como evidência de resultado)

As capturas individuais mostram as cápsulas de teste com o cenário e o destino real; as transições/estados são comprovados pela telemetria Unity MCP e tabelas acima.

## Testes automatizados

- Unity Smart Objects Edit Mode: **12 aprovados, 0 falhos, 0 ignorados**.
- Unity Smart Objects Play Mode: **3 aprovados, 0 falhos, 0 ignorados**.
- A integração 090/.NET passou build com 0 avisos/erros, `--test`: **153 assertions**, e `--validate`: 5 NPCs, 15 atividades, 3 overrides e 3 receitas. Simulação semanal reportou 1.021 eventos, 19 cancelamentos, 16 reservas/releases, 0 claims residuais e 2 falhas de receita; é simulação, não teste físico.
- Não há assembly de teste de navegação. Os assemblies Smart Objects existentes referenciam `Freyja.SmartObjects.Runtime`, não a classe Prototype em `Assembly-CSharp`; testes automatizados diretos exigiriam mudança arquitetural fora do escopo. Não foram criados testes de navegação em outro framework.
- Uma execução Unity completa anterior excedeu 300 s e foi cancelada; não é contabilizada como aprovada. Schedule/Activity Core não têm suites Unity disponíveis; Activity Core foi coberto pelo harness .NET 090.

## Matriz de aceite

| ID | Resultado | Evidência / limite |
|---|---|---|
| NAV-01 | **PASS** | Três rotas físicas chegaram com estado terminal e dentro das tolerâncias. |
| NAV-02 | **NOT RUN** | Não isolado agente já no destino nem diferença vertical no mesmo X/Z. |
| NAV-03 | **PARTIAL** | Ticket substituído/cancelado validado; `pathPending` assíncrono não foi induzido pois os paths atuais calculam imediatamente. |
| NAV-04 | **PASS** | Destino inválido e origem off-surface falharam sem teleporte; path parcial não foi isolado. |
| NAV-05 | **PASS** | Agente off-surface falhou sem teleporte; walker desativado encerrou operação com razão explícita. |
| NAV-06 | **PASS** | Cancelamento obsoleto não afetou nova operação; cancelamento repetido foi idempotente. |
| NAV-07 | **PARTIAL** | Stall/watchdog verificado após ajuste para tempo não escalado; avoidance de obstáculo dinâmico permanece sem teste. |
| NAV-08 | **PASS** | Duas paradas de região terminaram e lista vazia gerou falha finita. |
| NAV-09 | **PARTIAL** | Três agentes concluíram rotas simultâneas; cruzamento estreito/deadlock não foi isolado. |
| NAV-10 | **PARTIAL** | Approach de cadeira real alcançado; reserva e duas aproximações não testadas porque a cena oferece um slot. |
| NAV-11 | **PARTIAL** | Desativação do walker e destruição do alvo testadas; unload de cena e destruição do GameObject do NPC em rota não isolados. |
| NAV-12 | **PASS** | Pause/resume repetido após ajuste final e recompile; 1,2 s pausado não acumularam watchdog e `timeScale=2` manteve limite em segundos reais. |
| NAV-13 | **PARTIAL** | Rota Crista → patamar da igreja chegou com mudança de elevação no NavMesh existente; escada/cais específico não foi isolado. |
| NAV-14 | **PARTIAL** | Smart Objects Edit 12/12 e Play 3/3; Activity .NET 153 assertions. Suite Unity completa expirou e não há suites Unity para Schedule/Activity. |

## Configuração do harness

1. Adicione `NpcNavigationDemoHarness` temporariamente à cena que registra `IslandWaypointNetwork`.
2. Configure até três fixtures com `npcId`, `IslandWaypointWalker`, destino, pontos finitos de região e `selectedApproachPoint`. O componente usa o walker/`NavMeshAgent` existente; seleção e reserva permanecem do consumidor.
3. Em Play Mode, use `Navigation Demo/Move to configured destinations`, `Move to next region point`, `Move to selected approaches`, `Cancel all movement` ou `Reissue then cancel stale operation`.
4. Consulte `GetFixtureDiagnostics()` para operation ID, estado, requested/effective, path, velocity/direction, progresso/tempo e razão. Logs ocorrem em transição; gizmos mostram tolerâncias, pontos e cantos do path.

## Pendências e handoff para `npc-animation-system`

- Repetir pause/watchdog depois do ajuste final; testar avoidance com obstáculo dinâmico, cruzamento de agentes e `pathPending`/já-no-destino.
- Criar testes automatizados de navegação após haver assembly de runtime acessível à suíte, ou aprovar uma extração de assembly separada.
- Validar dois slots/reservas quando houver Smart Object adequado; cobrir unload de cena e destruição do NPC durante movimento.
- Na entrega de animação, usar velocidade/direção reais como entradas de locomoção e definir autoridade de rotação/interrupção com Activity Core. Blend tree, root motion, IK e pose sentada permanecem fora desta feature.
