# Tasks: NPC Activity Execution Core

**Input**: `specs/087-npc-activity-execution-core/`  
**Pré-requisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/execution-contract.md`  
**Testes**: obrigatórios por solicitação explícita e spec; estender o harness existente.

## Phase 1 — Base compartilhada

**Objetivo**: criar contratos da máquina de execução sem duplicar o Schedule Core.

- [x] T001 Mapear novas fontes via inclusões `Core/*.cs` e `Execution/*.cs` do `Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj`, sem cópias.
- [x] T002 Definir modelos de execução, estados, eventos, compromisso, relógios e motivos em `Freyja/Assets/_Project/Scripts/NpcScheduling/Execution/ActivityExecutionModels.cs`.
- [x] T003 Definir ports e operações síncronas tickáveis com tokens e cancelamento em `Freyja/Assets/_Project/Scripts/NpcScheduling/Execution/ActivityExecutionModels.cs`.
- [x] T004 Criar schema v1 e receitas `idle`, `wander`, `sit` em `Freyja/Assets/_Project/NpcScheduling/Data/execution/`.
- [x] T005 Implementar loader/validator por `StrictJson`, catálogo e passos em `Tools/NpcScheduleSimulator/ExecutionHarness.cs`.
- [x] T006 Criar cobertura nova integrada em `Tools/NpcScheduleSimulator/ExecutionTests.cs`, invocada pela suíte anterior em `Tests.cs`.

## Phase 2 — US1: Ciclo de vida de execução (P1)

**Objetivo**: executar uma intent vigente com estado/eventos determinísticos e substituição segura.

**Teste independente**: resolver intents existentes, avançar ticks e verificar snapshots, correlação, equivalência, atualização de validade, terminalidade, salto de tempo e cancelamento antigo.

- [x] T007 Implementar state machine, snapshots e eventos em `Freyja/Assets/_Project/Scripts/NpcScheduling/Execution/ActivityExecution.cs`.
- [x] T008 Implementar coordenador, identidade sem `ValidFrom` corrente, dia/ocorrência, terminalidade e substituição com cleanup em `ActivityExecution.cs`.
- [x] T009 Cobrir intent repetida minuto a minuto, deadline atualizado, callbacks/tokens, cancelamento, fallback e retrocesso de relógio em `Tools/NpcScheduleSimulator/ExecutionTests.cs`.
- [x] T010 Cobrir expiração durante deslocamento e saída normal/`stand_up` ao expirar espera em `ExecutionTests.cs`.

## Phase 3 — US2: Operações, recursos e receitas (P1)

**Objetivo**: executar as três atividades por receitas e garantir ownership/cleanup em sucesso, interrupção e falha.

**Teste independente**: executar cada receita com fakes e injetar disputa, falha de navegação/animação, reserva inválida e cleanup negado; verificar tokens e resultados.

- [x] T011 Implementar runtime whitelist, busca, reservas, movimento, alinhamento, ocupação, animação, wait e release em `ActivityExecution.cs`.
- [x] T012 Rastrear ownership e cleanup inverso com confirmação, retry limitado e bloqueio por recurso residual em `ActivityExecution.cs`.
- [x] T013 Validar receitas e ordem; fallback para idle ocorre uma vez, sem recursão, após falha/ausência de receita em `ExecutionHarness.cs` e `ActivityExecution.cs`.
- [x] T014 Criar fakes de 2 bancos × 2 slots, regiões, deslocamento/pose e falhas injetáveis em `Tools/NpcScheduleSimulator/ExecutionHarness.cs`.
- [x] T015 Implementar as três receitas em `Freyja/Assets/_Project/NpcScheduling/Data/execution/recipes.json` e suas operações no núcleo compartilhado.
- [x] T016 Cobrir exclusividade, bank B, caminho/animação/timeout, cancelamento nas fases, falha/retry de cancelamento de operação, expiração, fallback, Resume/Remove com resíduos e tokens em `ExecutionTests.cs`.

## Phase 4 — US3: Simulação integrada de sete dias (P2)

**Objetivo**: ampliar o simulador atual e demonstrar transições reais do resolver mais a execução simulada para três NPCs fictícios.

**Teste independente**: executar `--simulate-week` duas vezes, comparar trilhas e verificar sete dias, disputa, atualização de agenda, cancelamento, falha e invariantes finais de slots.

- [x] T017 Integrar execução ao comando `--simulate-week`, mantendo o método antigo de resolução para regressão, em `Program.cs` e `ExecutionWeekSimulator.cs`.
- [x] T018 Carregar `Fixtures/execution-week.json` por `StrictJson`, validar e passá-lo ao `ScheduleResolver` junto aos overrides existentes em `ExecutionWeekSimulator.cs`.
- [x] T019 Exibir transições de intenção e eventos separados, com tempo, dono, execução, intended/executed, motivo e marca simulada.
- [x] T020 Testar sete dias determinísticos, transições de blocos e overrides reais, disputa/falhas/cancelamento e zero claims após shutdown em `ExecutionTests.cs`.

## Phase 5 — Fechamento

- [x] T021 [P] Atualizar guia de receitas, operações e limites em `Freyja/Assets/_Project/NpcScheduling/README.md`.
- [x] T022 Executar build, suíte anterior+nova, `--validate`, duas simulações semanais e `git diff --check`; registrar comandos e resultados reais em `specs/087-npc-activity-execution-core/evidencias.md`.
- [x] T023 Revisar diff e referências proibidas no núcleo (Unity, relógio real, tarefas/esperas reais, aleatoriedade), verificar arquivos/metadados e documentar checklist de integração Unity em `specs/087-npc-activity-execution-core/evidencias.md`.

## Dependências e ordem

- Base T001–T006 estabelece loader e contratos.
- US1 (T007–T010) depende da Base.
- US2 (T011–T016) depende de US1 e pode desenvolver fakes em paralelo após T003.
- US3 (T017–T020) depende de US1 e US2.
- Fechamento depende de todas as histórias. Validação independente ocorre após T021.
- O trabalho será sequencial por haver um único Implementador e arquivos compartilhados entre histórias.

## Estratégia de implementação

Começar pelos contratos e testes de identidade/estados, depois coordenador, operações e limpeza; carregar receitas e exercitar os fakes; integrar o simulador somente depois que as atividades funcionarem isoladamente; terminar com a regressão anterior e a evidência semanal. Toda ação permanece simulada. Não criar commits, merge, push, cenas ou `.meta` nesta entrega.
