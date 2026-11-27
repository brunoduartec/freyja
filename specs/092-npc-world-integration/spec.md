# Feature Specification: NPC World Integration

**Feature Branch**: `092-npc-world-integration`  
**Created**: 2026-11-27  
**Status**: Draft for approval  
**Input**: `prompts/freyja-npc-world-integration.md` and the user's implementation request.

## User Scenarios & Testing

### User Story 1 — Moradores seguem suas rotinas (P1)

Como jogador, quero observar três moradores caminhando pela ilha e executando `idle`, `wander` e `sit` no horário previsto, para que suas rotinas sejam legíveis e consistentes.

**Why this priority**: É a primeira fatia vertical que demonstra a integração entre agenda, relógio, execução e mundo físico.

**Independent Test**: Iniciar Play Mode na cena da ilha, observar os três NPCs com IDs e agendas próprias e confirmar chegada, movimento regional, sentar e levantar por estados e eventos reais.

**Acceptance Scenarios**:

1. **Given** três NPCs válidos e rotinas JSON distintas, **When** Play Mode inicia, **Then** cada um resolve apenas o compromisso vigente e percorre o destino registrado usando navegação e animação existentes.
2. **Given** a janela `sit` vigente, **When** há assento disponível, **Then** o NPC reserva um slot, navega à aproximação, alinha, ocupa, senta, levanta e libera o mesmo recurso após sair.
3. **Given** uma agenda alterada em JSON, **When** os dados são reimportados e Play Mode reiniciado, **Then** o NPC executa o novo horário/destino sem mudança de código C#.

### User Story 2 — Contenção, mudança de agenda e falhas são seguras (P1)

Como responsável pelo projeto, quero que reservas, interrupções e falhas de caminho tenham cleanup e diagnóstico claros, sem duplicar controladores nem simular sucesso.

**Independent Test**: Disputar slots com três NPCs, ocupar todos os slots, alterar o relógio durante movimento e assento, interromper e desativar NPCs, e invalidar destino/caminho; conferir snapshots, logs e recursos.

**Acceptance Scenarios**:

1. **Given** três NPCs e um banco com dois slots, **When** os três tentam sentar, **Then** nenhum slot tem dois owners e o terceiro tenta alternativa compatível ou falha de forma finita e diagnosticável.
2. **Given** uma ação em andamento, **When** a intenção muda ou o NPC é desativado, **Then** só a execução proprietária é cancelada, o assento permanece reservado até saída segura e callbacks antigos não iniciam outra ação.
3. **Given** destino, caminho, JSON ou dependência inválidos, **When** a integração inicializa ou opera, **Then** registra falha explícita sem teletransporte, ocupação ou animação fictícios.

### User Story 3 — Rotinas usam o mundo existente (P2)

Como jogador, quero que compromissos acompanhem o relógio/calendário e destinos reais, inclusive ao iniciar em horário intermediário, pausar, avançar rapidamente ou atravessar meia-noite.

**Independent Test**: Controlar o relógio real do jogo, iniciar em horário intermediário, seguir um dia completo com tempo operacional para deslocamentos e testar saltos, pausa e mudança de dia sem replay.

**Acceptance Scenarios**:

1. **Given** o jogo começa em um horário intermediário, **When** os NPCs inicializam, **Then** cada um executa a intenção atual sem reproduzir compromissos anteriores.
2. **Given** o relógio avança, pausa ou salta para outro período/dia, **When** a agenda muda, **Then** o executor converge para a intenção vigente sem fila de horários obsoletos nem reinícios contínuos da mesma intenção.
3. **Given** pontos de retorno individuais e regiões reais registrados, **When** as rotinas referenciam esses IDs, **Then** cada destino resolve de forma explícita e ausência/desativação/duplicidade é diagnosticada.

### Edge Cases

- IDs duplicados, host/executor duplicado, JSON inválido, receita conhecida mas não suportada fisicamente, provider ou referência obrigatória ausente.
- Destino simbólico sem registro, região sem ponto navegável, objeto desativado/destruído, agente fora do NavMesh, caminho parcial e animação/pose indisponível.
- Banco sem slots livres; slot invalidado durante aproximação, ocupação ou saída; cleanup que falha temporariamente.
- Mudanças repetidas no relógio durante cleanup, salto de horário, meia-noite, pausa/time scale, retrocesso/reset e reinício do Play Mode sem domain reload.
- Interrupção real por diálogo somente se houver contrato/evento existente; caso contrário, exercitar comando explícito de teste e registrar diálogo como pendência.

## Requirements

### Functional Requirements

- **FR-001**: A integração MUST compor os contratos existentes de Schedule, Activity Execution, Smart Objects, Navigation e Animation/Pose; nenhum subsistema pode ser reimplementado ou duplicado.
- **FR-002**: Cada NPC MUST ter um ID estável, um host e um executor proprietários, receber dados/configuração válidos e produzir diagnóstico para identidade ou dependências inválidas.
- **FR-003**: A agenda MUST ser carregada do JSON pelo loader/schema existente e permanecer como fonte de verdade editável sem alteração de C#.
- **FR-004**: A intenção MUST ser resolvida com o relógio, calendário e contexto reais existentes, incluindo dia, minuto, dia da semana e condições já suportadas.
- **FR-005**: O host MUST resolver na inicialização, fronteira de agenda e mudança de contexto, atualizando os ports físicos na cadência necessária sem trabalho global por frame.
- **FR-006**: A integração MUST preservar identidade da intenção/execução, prioridades, cancelamento, retomada e cleanup definidos pelo Schedule e Activity Core; intenção planejada não comprova presença nem ação realizada.
- **FR-007**: Destinos simbólicos MUST resolver a registros explícitos de pontos/regiões reais; cada NPC MUST poder usar ponto externo de retorno individual.
- **FR-008**: `idle`, `wander` e `sit` MUST usar receitas e adapters reais disponíveis. Atividades fora desse recorte permanecem no catálogo sem sucesso físico simulado.
- **FR-009**: A atividade `sit` MUST preservar a mesma reserva/token desde a descoberta até saída segura e liberação; competição MUST impedir dupla ocupação e usar alternativa/falha finita existente.
- **FR-010**: Navegação MUST usar o sistema existente e reportar falhas de rota/chegada/destino sem teletransporte ou controle paralelo do agente.
- **FR-011**: Animação MUST refletir movimento real e usar ações finitas de sentar/levantar e pose sustentada sem disparos repetidos por frame.
- **FR-012**: Disable/destroy/unload e interrupções MUST remover inscrições e cancelar operações com cleanup idempotente, mantendo ownership até saída segura quando necessário.
- **FR-013**: O diagnóstico por NPC MUST distinguir compromisso planejado, execução, chegada, animação/pose, destino, slot/owner, resultado e motivo de falha/mudança, sem spam por frame.
- **FR-014**: Três NPCs MUST ter rotinas JSON distintas contendo somente atividades físicas suportadas neste recorte, regiões de praça/praia, pontos individuais de retorno e uma disputa sobreposta por assentos.
- **FR-015**: A demonstração MUST iniciar automaticamente em Play Mode e ser verificada em ambiente de teste seguro e na cena real da ilha, sem alterações aos controles/câmeras de protagonistas.
- **FR-016**: O teste MUST cobrir um ciclo de um dia com tempo operacional para ações físicas, além de casos de slots saturados, mudanças de relógio, interrupção, caminho/destino inacessível, cleanup, edição de JSON e reinício de Play Mode.
- **FR-017**: Conteúdo e configuração MUST usar ativos existentes e o Editor Unity para salvar cenas/prefabs/metadados; não serão adicionados assets comprados, créditos Tripo, novas atividades profissionais ou sistemas narrativos/editor visual.

### Key Entities

- **NPC Host**: composição por morador que liga identidade, JSON, relógio/contexto, executor e ports físicos.
- **Rotina JSON**: horários, atividades, destinos simbólicos e condições resolvidos pelo contrato existente.
- **Intenção/Execução**: compromisso planejado e ciclo físico associado, com identidade, estado e resultado distintos.
- **Destino**: ID simbólico que aponta para ponto de retorno individual, região navegável ou Smart Object compatível.
- **Reserva**: ownership tokenizado de um slot real, retido durante aproximação, ocupação e saída.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Três hosts únicos carregam três rotinas JSON válidas e distintas ao iniciar Play Mode, sem erros de compilação ou dependências obrigatórias ausentes.
- **SC-002**: Cada NPC completa ao menos um ciclo físico observado de chegada, `wander` e `sit`/levantar usando destino, navegação, animação e reserva reais.
- **SC-003**: Em disputa por dois slots, no máximo dois NPCs ocupam simultaneamente os slots e o terceiro obtém alternativa válida ou falha finita registrada.
- **SC-004**: Ao fim do dia simulado, os três NPCs convergem para as rotinas vigentes sem replay retroativo, execução duplicada, reserva órfã ou controle de protagonista alterado.
- **SC-005**: Alterar um horário ou destino no JSON e reiniciar Play Mode muda o comportamento observado sem recompilar ou alterar C#.
- **SC-006**: Toda condição W01–W19 do prompt recebe status, método e evidência; comportamentos sem execução real permanecem explicitamente como pendentes.

## Assumptions

- Os contratos e ports existentes nas cinco fundações e adaptadores Unity encontrados na branch-base são a autoridade; divergências devem ser verificadas antes do plano.
- Os exemplos de horário, IDs, moradores e destinos no prompt são dados de demonstração, não fatos canônicos do GDD.
- Rotinas demonstrativas usarão somente `idle`, `wander` e `sit`; outras atividades continuam no catálogo sem execução física nesta feature.
- Mudança de JSON entra em vigor após reimportação e reinício do Play Mode; hot reload não é requisito.
- Unity MCP só pode validar/editar o checkout que efetivamente reporta como projeto ativo; validação em outro checkout não será atribuída a esta branch sem sincronização rastreável.
- Se não houver evento real de interrupção por diálogo, o caso será testado por comando explícito e a integração com diálogo ficará pendente.
