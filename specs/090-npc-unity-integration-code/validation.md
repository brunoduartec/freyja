# Validation — NPC Unity Integration Code

## Repositório e escopo

- Branch: `090-npc-unity-integration-code`; base `main` em `73803dd` (Activity Execution Core sincronizado).
- Unity: `Freyja/ProjectSettings/ProjectVersion.txt` informa `6000.3.25f1`.
- Alterações locais desta entrega: adapters em `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/`, loaders compartilhados em `Core/` e `Execution/`, extensão additive de `Prototype/IslandWaypointWalker.cs`, harness e documentação da feature.
- Estado revisado para o commit: oito arquivos tracked modificados (`ActivityExecution.cs`, `ActivityExecutionModels.cs`, `IslandWaypointWalker.cs`, `ExecutionHarness.cs`, `ExecutionTests.cs`, `ExecutionWeekSimulator.cs`, `Program.cs`, `Tests.cs`); dois loaders, cinco scripts Unity, fixture JSON, documentos da feature e metadados Unity para os assets novos. Nenhuma cena, prefab, animação/controller, NavMesh, `ProjectSettings` ou `Packages` foi modificada.
- Não houve commit/merge desta feature.

## Verificações executadas

- `dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj` — sucesso, 0 warnings, 0 errors; compilou Core/Execution e harness compartilhados.
- `dotnet run --project Tools/NpcScheduleSimulator --no-build -- --test` — `PASS: 153 assertions`, incluindo loader/resolver da fixture runtime, conclusão/rejeição/timeout de `stand_up`, timeout do ticket de animação interrompida, retry, ordem de liberação e substituição bloqueada.
- `dotnet run --project Tools/NpcScheduleSimulator --no-build -- --validate` — `OK: 5 NPCs, 15 atividades, 3 overrides, 3 receitas executáveis`.
- `dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week` — `SIM_WEEK days=7 npc=3 events=1021 failures=2 cancellations=19 reservations=16 releases=16 residualClaims=0 physicalValidation=false`; cenário de banco cheio reivindicou 2/2 slots e encerrou com 0 claims. Falhas e cancelamentos são simulados.
- `git diff --check` — sucesso, sem whitespace errors.
- Reexecução após a revisão: `--test` retornou `PASS: 153 assertions`; `--validate` retornou `OK: 5 NPCs, 15 atividades, 3 overrides, 3 receitas executáveis`; `--simulate-week` retornou 7 dias, 1021 eventos, 2 falhas e 19 cancelamentos simulados, 16 reservas/liberações e 0 claims residuais (`physicalValidation=false`).
- Unity MCP na Unity `6000.3.25f1`: os fontes exatos do branch foram importados temporariamente no checkout aberto `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`; a primeira compilação detectou `ScheduleDataLoader` usando `AsReadOnly()` em `IList<string>`. A chamada foi corrigida para copiar para `List<string>`; recompilação subsequente retornou `failed=false`, `errors=[]`, `compilationFailed=false`. A cena permaneceu não dirty. A compilação foi feita fora deste worktree, sem validação de Play Mode da rotina integrada.
- Revisão estática desta etapa confirmou a retenção do claim pelo `stand_up` pollable, a ordem de liberação e a ausência de alteração em cena/prefab/.meta. Não houve revisão física no Unity Editor.
- Revisão estática de direção das dependências: Core/Execution permanecem Unity-free; o projeto .NET compila por seus `Compile Include` existentes. Não foi criado asmdef; os novos scripts Unity permanecem no Assembly-CSharp para conservar acesso aos componentes existentes.

## Estado da evidência

- **Implementado:** loader compartilhado para schedule e receita, provider explícito de TextAssets, host por NPC com snapshots de mundo fornecidos pelo chamador, registro simbólico de locais que valida pertença ao `IslandWaypointNetwork` vinculado ao walker, adaptação a Smart Objects e bindings explícitos de Animator. Cancelamento inicia `stand_up` pelo port de animação já mapeado; o core faz polling limitado pelo timeout do adaptador e só libera a reserva exata após sucesso. Falha/timeout mantém o token e permite retry. Ao desabilitar o host, a referência do coordenador é mantida se houver residual, para retomar cleanup quando receber snapshots após reativação.
- **Revisado estaticamente:** propriedade dos tickets de movimento e tokens de operação; cada claim guarda o `ReservationHandle` exato associado a NPC, execução e owner; resolução de pontos sem fallback por nome/coordenadas; falhas de inicialização e operação têm códigos estáveis.
- **Ajuste pós-revisão independente:** `NpcUnityActivityPorts.Poll` agora detecta destino destruído/desativado enquanto a operação ainda está em andamento; invalidação, perda de claim e timeout cancelam apenas o ticket de movimento correspondente antes de reportar falha. O adapter Unity ainda precisa ser compilado e exercitado no Editor.
- **Compilado:** Core, Execution, harness .NET e fontes Unity via Unity MCP com cópias temporárias no Editor aberto. O Unity MCP compilou o conjunto, mas o Play Mode da rotina integrada permanece não executado.
- **Play Mode:** não executado. Nenhum comportamento físico foi validado.

## Limitações que bloqueiam validação/comportamento físico

- Não foi localizado provider real para relógio/contexto de mundo. `NpcActivityController.TickFromWorld(...)` exige snapshots atuais fornecidos externamente; não fabrica relógio, política de pausa ou contexto. Um cleanup pausado por disable só volta a avançar quando o host recebe outro snapshot válido.
- Não há state IDs/paths/clips reais verificados no Animator. Bindings são explícitos no Inspector e `TryBegin` exige que o Animator contenha o state.
- O binding real do Animator para `stand_up` ainda precisa ser configurado e verificado no Editor. O código agora faz polling desse state e retém ownership em caso de falha/timeout, mas nenhum clip, state, postura física ou recuperação real foi validado.
- Alignment usa tolerância configurável e reposiciona o Transform após validar distância; colisões, orientação, raiz animada e interação com NavMesh precisam ser conferidas no Editor.
- Destinos, região, pontos, Smart Objects, NavMesh e Animator não estão configurados em cena/prefab, conforme escopo code-only.
- A fixture JSON de NPC usa o schema runtime e o loader/resolver compartilhados. A integração com Unity ainda requer importá-la como TextAsset e arrastá-la para `NpcScheduleTextAssetProvider.npcSchedules[]`; a lista de checagem registra esse passo.

## Pendências para Unity

Usar o checklist completo em `integration-checklist.md`. Em resumo: criar `.meta` via Unity, verificar compilação/Console e referências de assemblies, ligar TextAssets existentes, provider de clock/context, host e owner no NPC descartável, registrar pontos/regiões, configurar walker/NavMesh e slots Smart Object, mapear states reais, executar `idle → wander → sit → wander → idle`, testar falhas/cancelamento/destruição e salvar evidência Edit/Play Mode. Todas as etapas físicas permanecem pendentes.
