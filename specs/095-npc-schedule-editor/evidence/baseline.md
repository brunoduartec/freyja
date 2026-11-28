# Baseline da feature 095

Captura inicial feita em 2026-11-27, antes das alterações de código da feature.

## Hashes das agendas de integração

| Arquivo | SHA-256 inicial | Estado verificado após implementação inicial |
|---|---|---|
| `world_helena.json` | `f9c14459f384498f6e91c17718bb9b7ec764587e3e8a612caca3232f7fb66e2d` | igual |
| `world_ivo.json` | `02c0c468475c2d05e96c36d5b024fa07067c04c301d69bdb1814ee37a613051a` | igual |
| `world_luana.json` | `847b3eb268300aab96cf26f235a8e65d05daca281a63a36a6eb8424b79d44e03` | igual |

Comando de captura: `shasum -a 256 Freyja/Assets/_Project/NpcScheduling/Data/npcs/world_integration/*.json`.

## Unity Editor e Console

- Projeto descoberto pela Unity CLI: `Freyja/`, versão `6000.3.25f1`, PID 25199.
- Estado da conexão na captura: `unreachable`; não houve leitura real do Console nem confirmação de cena ativa por MCP neste subcontexto.
- A API MCP Unity retornou “tool ... is not available”. A sessão principal depois também observou `127.0.0.1:7800 unreachable` e suspendeu novas tentativas. Compilação/Console/Editor ficam pendentes de reconexão.

## Harness antes da implementação

- `dotnet run --project Tools/NpcScheduleSimulator -- --validate`: `OK: 5 NPCs, 15 atividades, 3 overrides, 3 receitas executáveis`.
- `dotnet run --project Tools/NpcScheduleSimulator -- --test`: `PASS: 174 assertions`.
- Avisos presentes no baseline: lacunas após `sleep_early` em `fisherman.json`, `demo_fisherman.json` e agenda efetiva de `demo_fisherman`.

## Unity MCP após reconexão (2026-11-28)

- Editor respondeu como `ready`, Unity `6000.3.25f1`, projeto `Freyja/`, cena `Ilha_Principal` aberta/ativa, Edit Mode.
- A primeira recompilação do código da feature encontrou dois CS0029 na assinatura de `FilteredDocuments()`; o retorno foi corrigido de `string[]` para `NpcScheduleDocument[]` e a recompilação seguinte passou sem erros.
- Ground truth do Console após recompilar: `compilationFailed=false`, `consoleErrors=0`, `consoleWarnings=0`. O buffer cumulativo ainda conserva mensagens antigas, inclusive erros anteriores à correção e falhas de conexão do próprio pipeline; não são erros ativos atuais.
- Menu `Freyja/NPC/Schedule Editor` abriu e foi capturado em `editor-window.png`. A captura apresenta `world_helena`, o caminho do JSON runtime, cinco blocos efetivos e a timeline.
- Unity Test Runner: 40/40 testes Edit Mode e 9/9 testes Play Mode passaram. Eles não cobrem save/UI do Schedule Editor nem mudança de comportamento dos NPCs ativos.
- CUA continuou bloqueado pela sessão macOS. Nenhum JSON foi editado e a prova física em Play Mode permanece pendente.
- SHA-256 dos três arquivos no estado atual ainda corresponde ao baseline acima.
- Um probe efêmero `script_run` criou um JSON de teste temporário no diretório canônico para round-trip/conflito/falha de escrita; o arquivo e `.meta` foram removidos no `finally`, e os hashes `world_*` foram conferidos novamente.
- Ground truth do Console depois dos probes/testes registra zero erros de compilação e dois warnings: timeout de acesso à Account API e warning esperado do teste Edit Mode que verifica IDs de Smart Object duplicados.
