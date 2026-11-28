# Matriz de validação — feature 095

Estado registrado em 2026-11-28. A matriz separa implementação em fonte, compilação, inspeção visual e comportamento testado.

| Verificação | Estado | Evidência | Limite |
|---|---|---|---|
| Descoberta, snapshot de destinos, estados da lista e invalidação de cache | Compilou; fonte `world_helena` foi validada contra as regiões ativas no snapshot da cena | `evidence/editor-window.png` | Interação com o dropdown de destinos e com outros documentos pendente |
| Agenda efetiva baseada no loader, incluindo herança e origem das entradas | PASS visual parcial | `evidence/editor-window.png` | Captura mostra cinco blocos efetivos e origem local para `world_helena`; herança ainda não exercitada |
| Flags de contexto no preview | Campo `nome=true/false` presente; comportamento não confirmado | `NpcScheduleEditorWindow.cs` | Sem teste de override no Editor |
| Lacunas e sobreposições na timeline | PASS visual estrutural; marcadores específicos não exercitados | `evidence/editor-window.png` | Sem rascunho temporário para gerar lacuna/sobreposição |
| Validação de candidate: sobreposição e alvo obrigatório | PASS em `ScheduleDataLoader`/repositório | Unity MCP probe: `overlapRejected=true`, `targetlessRejected=true` | Exercitado por método; controles/diagnóstico visual não testados |
| Atividade adicionada dinamicamente ao catálogo | PASS no catálogo/loader | Unity MCP probe: `dynamicActivityDiscovered=true`, `dynamicActivityWithoutRecipeDetected=true` com JSON em memória | Não houve seleção no dropdown nem alteração persistida |
| IDs de Smart Object repetidos | Opções preservam linhas duplicadas, mostram hierarquia e status `ambiguous` | `NpcScheduleEditorDestinations.cs`, `NpcScheduleEditorWindow.cs` | Sem cena conectada para observar os dados concretos |
| Harness `--validate` | PASS em 2026-11-28 | comando `dotnet run --project Tools/NpcScheduleSimulator -- --validate` | Não cobre scripts de Editor |
| Harness `--test` | PASS, 174 assertions, em 2026-11-28 | comando `dotnet run --project Tools/NpcScheduleSimulator -- --test` | 3 avisos de lacuna conhecidos; não cobre scripts de Editor |
| Unity compile, Console e abertura visual da janela | PASS | Unity MCP: recompile sem erros; Console `compilationFailed=false`, `consoleErrors=0`; menu aberto e captura em `evidence/editor-window.png` | Ground truth às 04:12 UTC registrou um warning ativo, sem mensagem retornada para classificá-lo |
| Unity Edit Mode / Play Mode suites existentes | PASS: 40 Edit Mode, 9 Play Mode | Resultados do Unity MCP em 2026-11-28 | Suites existentes não cobrem o Schedule Editor |
| Três agendas ativas e alvos da cena | PASS: `world_helena`, `world_ivo`, `world_luana` são runtime-loaded e válidas | Probe `script_run` em `NpcScheduleEditorRepository.ValidateAll`, com `Ilha_Principal` ativa | Não exercitou seleção/edição visual de cada documento |
| Fluxo Play Mode e prova física | BLOCKED | CUA informou que o Mac continua bloqueado; nenhum save visual ou comportamento alterado foi observado | Requer desbloqueio da sessão para operar a interface |
| SE01–SE34 | Classificados abaixo | Matriz detalhada incorporada a `spec.md` | Status reflete as verificações realmente executadas nesta sessão |

As agendas `world_helena`, `world_ivo` e `world_luana` mantiveram os hashes documentados em `baseline.md` na última leitura. Não houve alteração de JSON ou cena durante esta revisão.

## Matriz SE01–SE34

`BLOCKED` significa que a verificação depende de interação UI/Play Mode não realizada porque o Mac informou a tela bloqueada. `NOT RUN` significa que o caso não foi executado nem tentado de forma equivalente.

| ID | Estado | Tipo/evidência | Limite |
|---|---|---|---|
| SE01 | PASS | Unity MCP abriu `Freyja/NPC/Schedule Editor`; captura em `evidence/editor-window.png` | Menu/abertura confirmados; controles não exercitados |
| SE02 | BLOCKED | UI/Editor; busca/seleção não exercitadas | Interação CUA bloqueada pelo macOS |
| SE03 | BLOCKED | Captura parcial mostra a timeline de 24h e cinco blocos efetivos de `world_helena` em `evidence/editor-window.png` | Não pude exercitar a seleção de um bloco e conferir o item selecionado |
| SE04 | BLOCKED | UI/Editor; CRUD não exercitado | Interação CUA bloqueada pelo macOS |
| SE05 | BLOCKED | UI/Editor; limites não exercitados | Interação CUA bloqueada pelo macOS |
| SE06 | BLOCKED | UI/Editor; intervalos inválidos/noturnos não exercitados | Interação CUA bloqueada pelo macOS |
| SE07 | BLOCKED | UI/Editor; sobreposição não testada | Interação CUA bloqueada pelo macOS |
| SE08 | BLOCKED | Preview; lacuna/fallback não testados pelo editor | Harness existente não cobre a janela |
| SE09 | BLOCKED | UI/Editor; status de receita não inspecionado na janela | Interação CUA bloqueada pelo macOS |
| SE10 | BLOCKED | UI/Editor; troca de alvo não exercitada | Interação CUA bloqueada pelo macOS |
| SE11 | BLOCKED | UI/Editor; destinos inválidos não exercitados | Interação CUA bloqueada pelo macOS |
| SE12 | BLOCKED | UI/Editor/cena; destinos simbólicos das outras agendas e escopo fora da cena ativa não exercitados | Interação CUA bloqueada pelo macOS |
| SE13 | BLOCKED | Play Mode; ocupação transitória não testada via editor | Não foram feitas reservas ou mutações para teste |
| SE14 | BLOCKED | UI/Editor; herança não exercitada visualmente | Interação CUA bloqueada pelo macOS |
| SE15 | BLOCKED | UI/Editor; remoção/restauração herdada não exercitada | Interação CUA bloqueada pelo macOS |
| SE16 | BLOCKED | UI/Editor; impacto de template não exercitado | Interação CUA bloqueada pelo macOS |
| SE17 | BLOCKED | UI/Editor; duplicação não exercitada | Interação CUA bloqueada pelo macOS |
| SE18 | BLOCKED | UI/Editor; associação/criação não exercitada | Interação CUA bloqueada pelo macOS |
| SE19 | BLOCKED | Preview/UI; condição não exercitada pelo editor | Harness existente não valida a UI |
| SE20 | BLOCKED | Preview/UI; tie-break não exercitado pelo editor | Harness existente não valida a UI |
| SE21 | BLOCKED | Preview/UI; paridade do editor não exercitada | Harness core passou, mas não constitui preview do editor |
| SE22 | BLOCKED | UI/Editor; undo/redo/revert não exercitados | Interação CUA bloqueada pelo macOS |
| SE23 | BLOCKED | UI/Editor; troca com draft dirty não exercitada | Interação CUA bloqueada pelo macOS |
| SE24 | BLOCKED | Persistência/UI; round-trip sem edição não exercitado | Interação CUA bloqueada pelo macOS |
| SE25 | BLOCKED | Persistência/Editor; save/reimport/reopen não exercitados | Interação CUA bloqueada pelo macOS |
| SE26 | BLOCKED | Editor; safe mode não exercitado | Interação CUA bloqueada pelo macOS |
| SE27 | PASS | Unity MCP `script_run`: alterou o JSON de um documento temporário após capturar fingerprint; `TrySave` recusou e preservou o texto externo | Probe usou método/repositório diretamente, sem UI; asset temporário foi removido |
| SE28 | PASS | Unity MCP `script_run` invocou `Save()` com falha de escrita forçada; resultado `originalRestored=true`, `draftRetained=true`, `failureReported=true` | Método Save foi chamado via probe, sem clique visual; falha de importação não injetada |
| SE29 | BLOCKED | Unity MCP `script_run` em Play Mode: `editorIsPlaying=true`, `writeBlocked=true` para `ReplaceDraft` | A sessão recusou a mutação; a janela e seus controles não foram observados visualmente porque o Mac está bloqueado |
| SE30 | BLOCKED | Play Mode físico; nenhuma edição via UI nem captura real | Requisito central ainda não provado |
| SE31 | BLOCKED | Unity UI; foco/diagnóstico integrado não exercitados | Interação CUA bloqueada pelo macOS |
| SE32 | BLOCKED | Editor; reinício/reload não testados | Interação CUA bloqueada pelo macOS |
| SE33 | BLOCKED | Performance Editor; escala de 30 documentos não medida | Cache inspecionado em código, responsividade não medida |
| SE34 | BLOCKED | Harness, 40 testes Edit Mode, 9 Play Mode e `git diff --check` passaram; | Player build não rodado; suites Unity não cobrem os fluxos do editor |

### Verificações realmente executadas

- Schedule Simulator `--validate`: PASS, 5 NPCs, 15 atividades, 3 overrides, 3 receitas executáveis.
- Schedule Simulator `--test`: PASS, 174 assertions. Três avisos de lacuna do `demo_fisherman` permanecem iguais ao baseline.
- Unity MCP `script.recompile`: PASS sem erros depois de corrigir o tipo de retorno de `FilteredDocuments()`.
- Console Unity no último snapshot, às 04:12 UTC: `compilationFailed=false`, zero erros ativos e um warning ativo; a coleta não retornou sua mensagem, então a origem fica sem classificação.
- Unity Edit Mode: PASS, 40/40; Play Mode suite existente: PASS, 9/9. Esses testes não cobrem a interface do Schedule Editor.
- Unity MCP `script_run` repository probes: `malformedRejected=true`, `unknownSchemaRejected=true` (schema 99 com diagnóstico do arquivo), `invalidTimeRejected=true`, `noOpRoundTripPassed=true`, `conflictPreserved=true`, `failedWriteRestored=true`, sem exceção. Os JSONs de prova e `.meta` foram removidos; os três hashes de produção continuam iguais.
- Unity MCP `script_run` candidate/catalog probe: `overlapRejected=true`, `targetlessRejected=true`, `dynamicActivityDiscovered=true`, `dynamicActivityWithoutRecipeDetected=true`, `dynamicCandidateValid=true`; JSON do catálogo foi alterado só em memória.
- Unity MCP `script_run` session probe: `undoRedoRevertPassed=true`, `malformedReadOnlyPassed=true`, sem exceção. Isso testa a sessão de edição em isolamento e não substitui a interação UI.
- Unity MCP `script_run` Save probe: `saveReturnedWithoutException=true`, `originalRestored=true`, `draftRetained=true`, `failureReported=true`, sem exceção; invocou o método do EditorWindow, não clicou no botão.
- Unity MCP `script_run` delete-conflict probe: `deleteConflictDetected=true`; removeram um documento temporário depois do fingerprint e o `TrySave` não recriou o arquivo.
- Unity MCP `script_run` durante Play Mode: `editorIsPlaying=true`, `writeBlocked=true`; confirmou que a sessão recusa `ReplaceDraft`. A janela e os controles de somente leitura não foram operados visualmente.
- Unity MCP `script_run` em cada agenda ativa: `world_helena`, `world_ivo` e `world_luana` reportaram `runtime=true`, `valid=true`, `error=null` com a cena principal carregada.
- Menu `Freyja/NPC/Schedule Editor` abriu a janela; `evidence/editor-window.png` mostra `world_helena`, origem runtime, agenda efetiva e timeline.
- CUA continua sem acesso interativo devido à tela bloqueada. Nenhum save via UI, preview de flags, foco visual do Debug Monitor ou mudança física foi executado.
- `git diff --check`: PASS.
- Hashes dos três JSONs de integração permanecem iguais aos valores em `baseline.md` na última leitura.
- Revisão estática independente (segunda passada): achados de repaint, herança, classificação e IDs ambíguos resolvidos em fonte.
- Hardening posterior pelo agente principal: estados de destino `unavailable`/`invalid_target`, refresh em `EditorApplication.projectChanged` e casamento de draft pelo AssetPath; recompilação Unity passou.
