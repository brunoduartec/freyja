# Tasks: NPC Schedule Core

**Input**: `specs/086-npc-schedule-core/spec.md`, plan, research and data model.
**Tests**: Obrigatórios conforme seção 9 da spec.

## Phase 1 — Base

- [x] T001 Criar CLI .NET sem pacotes em `Tools/NpcScheduleSimulator/` incluindo fontes compartilhadas em `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/`.
- [x] T002 [P] Criar schema v1 e catálogo de atividades em `Freyja/Assets/_Project/NpcScheduling/Data/schema/` e `activities.json`.
- [x] T003 Implementar parser JSON estrito e diagnósticos localizados em `Tools/NpcScheduleSimulator/`.

## Phase 2 — US1: Consultar intenção determinística

**Independent test**: mesmo snapshot retorna intent equivalente; blocos, limites, meia-noite, validade e fallback são cobertos.

- [x] T004 Implementar modelos puros, horário, snapshots e parser de relógio em `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/`.
- [x] T005 Implementar `ScheduleResolver`, trace, janela de validade e identidade sem timestamp de consulta em `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/`.
- [x] T006 [P] Criar testes do resolver para limites, calendário, fallback, saltos e determinismo em `Tools/NpcScheduleSimulator/Tests/` (assertions executadas).

## Phase 3 — US2: Autorar profissões e personalizar NPCs

**Independent test**: cinco templates e cinco NPCs carregam; operações de herança são válidas e não mutam template; configuração inválida falha com diagnóstico.

- [x] T007 Implementar loader, expansão de template, substituição, remoção, adição e validação semântica em `Tools/NpcScheduleSimulator/`.
- [x] T008 [P] Adicionar cinco templates, NPCs fictícios, aliases lógicos e schema documentado em `Freyja/Assets/_Project/NpcScheduling/Data/`.
- [x] T009 Criar testes de parsing, referências, conflitos, operações de agenda e edição data-driven em `Tools/NpcScheduleSimulator/Tests/`.

## Phase 4 — US3: Exceções e simulação

**Independent test**: sete dias com clima, calendário e flag narrativa produzem transições estáveis, prioridades corretas e saída compacta.

- [x] T010 Implementar matching de condições e prioridade/desempate estável em `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/`.
- [x] T011 [P] Criar regras de chuva, domingo e evento demo, marcados sintéticos, em `Freyja/Assets/_Project/NpcScheduling/Data/overrides/`.
- [x] T012 Implementar simulador de sete dias, sink de transições em texto compacto e CLI query/validate/simulate em `Tools/NpcScheduleSimulator/`.
- [x] T013 Criar testes de prioridade, flags ausentes, empate, ordem estável, retorno pós-override, transições equivalentes e semana completa em `Tools/NpcScheduleSimulator/Tests/`.

## Phase 5 — Finalização

- [x] T014 [P] Documentar formato, edição de rotinas, comandos reais e integração futura em `Freyja/Assets/_Project/NpcScheduling/README.md`.
- [x] T015 Executar autotestes e CLI após a ampliação de cobertura e registrar resultados observados em `specs/086-npc-schedule-core/validacao.md` (build 0 warning/0 erro, validação, consultas e simulação executadas; contagem final no relatório de validação).
- [x] T016 Revisar os caminhos da entrega: sem referências Unity/Smart Objects no domínio e sem alterações intencionais a arquivos de outras entregas.

## Dependências

T001–T003 fundamentam US1/US2; US2 requer US1 para resolver a agenda expandida; US3 requer o modelo e loader. T014 pode ocorrer em paralelo após contratos fixados. T015–T016 encerram.

## Estratégia

Concluir contratos e domínio antes de avançar ao carregamento; depois dados, overrides e CLI; por fim executar testes independentes. Sem paralelismo de escrita em arquivos compartilhados. A revisão Game Designer foi feita antes desta etapa; a revisão Validador será independente após a implementação.
