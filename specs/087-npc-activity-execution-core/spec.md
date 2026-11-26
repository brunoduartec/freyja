# Especificação: NPC Activity Execution Core

**Feature Branch**: `087-npc-activity-execution-core-latest`  
**Criado**: 2026-11-26  
**Status**: Draft — aguardando aprovação para planejamento  
**Entrada**: `freyja-npc-activity-execution-core.md` anexado pelo usuário

## Cenários e testes

### US1 — Executar uma intenção como atividade observável (P1)

O simulador transforma a `ActivityIntent` atual do Schedule Core em uma execução determinística. Um operador consegue acompanhar estados, passos, início, término e motivos sem confundir intenção com ação física ou evento narrativo.

**Teste independente**: alimentar intents e snapshots de tempo ao coordenador, avançar ticks monotônicos e verificar estados e eventos registrados.

**Aceitação**

1. Intenção semanticamente idêntica em ticks sucessivos mantém a mesma execução; mudança efetiva substitui a execução após cancelamento e limpeza.
2. Uma execução por NPC progride sem bloquear outros NPCs; execução e callbacks antigos não afetam tentativas novas.
3. Saltos de relógio avaliam somente a intenção vigente. Retrocesso é rejeitado ou explicitamente tratado e não há recuperação retroativa de ações.

### US2 — Compor atividades primitivas com falha e limpeza seguras (P1)

Receitas versionadas definem sequências lineares de operações reutilizáveis. Nesta etapa, `idle`, `wander` e `sit` são executáveis; outras atividades reconhecidas pelo catálogo permanecem sem executor.

**Teste independente**: carregar receitas JSON e executá-las contra adaptadores simulados determinísticos, inclusive com indisponibilidade, timeout, interrupção e falha de limpeza.

**Aceitação**

1. `idle` pode aguardar no local atual ou mover-se a um destino lógico; `wander` percorre pontos simulados de uma região sem reservar objeto; `sit` encontra, reserva, aproxima, alinha, ocupa, anima, aguarda, restaura a pose e libera o slot.
2. Reserva só é ocupada após movimento e alinhamento bem-sucedidos. Interrupção, falha, timeout, remoção do NPC e conclusão liberam recursos adquiridos em ordem inversa, sem alegar liberação não confirmada.
3. Receitas inválidas, sem executor e falhas transitórias têm diagnóstico estável; fallback para idle e tentativas são limitados, não recursivos e respeitam a janela atualizada.

### US3 — Demonstrar rotinas e interrupções em sete dias (P2)

O harness existente combina Schedule Core e execução, exibindo as rotinas de sete dias e cenários sintéticos de disputa por slots, mudanças de agenda, cancelamentos e falhas.

**Teste independente**: executar o comando de simulação semanal mais os testes automatizados, e verificar a saída determinística e o estado final dos recursos.

**Aceitação**

1. As mesmas entradas produzem a mesma sequência ordenada de eventos e resultados, com tempo simulado e correlação por NPC/execução.
2. Os fixtures usam três NPCs fictícios, duas fontes com dois slots cada e regiões `village_square`, `beach` e `home`; disputas nunca concedem o mesmo slot a dois donos.
3. A simulação identifica claramente atividade pretendida, executada ou fallback e não declara presença, testemunho, pista, animação real ou deslocamento físico.

## Casos de borda

- Expiração da intenção durante preparo ou deslocamento cancela operações; durante `wait` de uma atividade já estabelecida, encerra normalmente e permite a sequência de saída.
- Atualização apenas de validade/metadados não reinicia uma execução equivalente. Ocorrências em dias sucessivos ou que cruzem meia-noite não são confundidas.
- Alvo/região inexistente, nenhum slot livre, caminho impossível, timeout, animação rejeitada, reserva invalidada, definição sem executor e falha residual de limpeza têm resultados explícitos.
- Ticks repetidos, operações instantâneas e retornos tardios não causam loop ilimitado, efeitos duplicados ou liberação de reserva pertencente a outro NPC/execução.
- `wander` sem região falha ou usa fallback documentado; `idle` sem alvo permanece no local atual.

## Requisitos

### Funcionais

- **FR-001**: O núcleo consome `ActivityIntent` e os modelos existentes sem redefinir agenda, catálogo, relógio ou reservas de produção.
- **FR-002**: Cada NPC possui no máximo uma execução ativa; intents equivalentes preservam a execução e substituições aguardam limpeza.
- **FR-003**: Cada execução expõe snapshot com identidade, intent vigente, destino lógico e efetivo quando disponível, estado, passo, tempos simulados, falha/cancelamento e recurso diagnóstico.
- **FR-004**: Eventos de ciclo de vida registram início e término de atividade e passos, cancelamento, falha, reserva e liberação, com códigos de motivo estáveis e correlação.
- **FR-005**: Operações primitivas têm início único, progresso por tick explícito e resultados de sucesso, falha ou cancelamento; ticks recebem tempo monotônico operacional e calendário simulado.
- **FR-006**: Receitas são dados JSON versionados, lineares, validados antes da execução e limitados a operações tipadas permitidas; catálogo continua sendo a fonte dos IDs de atividade.
- **FR-007**: Apenas `idle`, `wander` e `sit` têm receita nesta entrega. A ausência de receita é detectável e tratada sem afirmar que a atividade do catálogo é executável.
- **FR-008**: Recursos adquiridos são registrados por execução e liberados independentemente do passo normal, de forma idempotente e com confirmação do adaptador; falhas de limpeza não impedem tentativas de liberar outros recursos.
- **FR-009**: Retentativas, tentativas de slots e transições por tick são limitadas e determinísticas. Fallback para idle não altera a intenção do Schedule Core nem recursa caso idle falhe.
- **FR-010**: O simulador existente é ampliado para sete dias completos, três NPCs de demonstração e cenários de disputa, atualização de agenda, cancelamento e falha.
- **FR-011**: Núcleo e harness não dependem da Unity. Adaptadores de produção, navegação, animação, presença física e integração de cenas ficam fora do escopo.
- **FR-012**: Os dados e mensagens distinguem intenção, atividade executada e fallback e identificam expressamente resultados como simulação.

### Entidades

- **Compromisso semântico**: NPC, ocorrência temporal, atividade, alvo, parâmetros e origem relevantes à decisão de manter ou trocar uma execução.
- **Execução**: tentativa única de realizar um compromisso, com ciclo de vida, tempo, resultado, falhas e ownership de recursos.
- **Receita e passo**: sequência validada de operações primitivas associada a um ID já existente no catálogo.
- **Handle de recurso**: token opaco e vinculado a dono/execução, cuja validade e liberação são confirmadas por um port.
- **Snapshot e evento de execução**: diagnóstico imutável do estado corrente e registro ordenado das transições simuladas.

## Critérios de sucesso

- **SC-001**: Os testes anteriores do Schedule Core e os novos testes passam no harness .NET, e o build termina sem erros.
- **SC-002**: Uma simulação de sete dias termina sem deadlock, execução retroativa ou slot simultaneamente pertencente a dois NPCs.
- **SC-003**: Para entradas iguais, a sequência de eventos e o estado final do harness são idênticos entre execuções.
- **SC-004**: Todas as interrupções e falhas injetadas deixam os recursos liberados ou registram explicitamente o recurso residual e a falha de limpeza.
- **SC-005**: Nenhum resultado simulado é descrito como comportamento físico validado na Unity.

## Restrições e pressupostos

- A branch `086-npc-schedule-core` contém alterações locais preexistentes e os artefatos de Schedule Core e Smart Objects estão presentes no checkout atual. Essas alterações devem ser preservadas.
- O contrato atual de `ActivityIntent` fornece NPC, atividade, alvo, dia, validade, prioridade, origem, fonte e bloco. Sua identidade atual não inclui ocorrência diária ou parâmetros; o executor deverá resolver semanticamente a ocorrência sem modificar o resolver sem necessidade comprovada.
- O checkout contém `ReservationHandle` com proprietário/token, validade, ocupação e liberação no Smart Objects. Ele é referência para o mapeamento futuro; esta feature usa ports e fakes, não integra nem valida o sistema físico.
- O tempo operacional e o calendário do jogo são injetados separadamente; não se usa relógio de parede, espera real ou aleatoriedade não controlada.
- A Unity está indisponível. Não editar cenas, prefabs, animações, assets ou `.meta`, nem alterar pacotes. Não criar commit, merge ou push nesta etapa.
- A especificação detalhada anexada governa os contratos completos de estados, operações, limpeza, fixtures, CLI e entrega. Este documento resume seus critérios para o Spec Kit.

## Fora de escopo

Executor Unity, NavMesh e movimento real, Animator e animação real, criação de GameObjects, integração de produção com Smart Objects, pistas/testemunhos/eventos narrativos, atividades além de `idle`, `wander` e `sit`, motor genérico de workflows, scripting, behavior trees, GOAP, ECS/DOTS e alterações ao relógio ou resolver do Schedule Core.
