# Feature Specification: Sistema de Animação de NPCs

**Feature Branch**: `089-npc-animation-system`

**Created**: 2026-11-27

**Status**: Draft para aprovação

**Input**: User description: `prompts/freyja-npc-animation-system.md` e pedido do usuário nesta conversa.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Locomoção legível (Priority: P1)

O jogador observa moradores compatíveis parados, caminhando e, quando houver animação adequada, correndo. A atuação acompanha o deslocamento real e volta a repouso quando o NPC para.

**Why this priority**: Caminhada e repouso são a base visual para qualquer atividade física dos NPCs.

**Independent Test**: Em uma demonstração isolada, observar três NPCs com movimento real, parada, espera e retomada; comparar estado visual e velocidade com o deslocamento observado.

**Acceptance Scenarios**:

1. **Given** um NPC compatível parado ou aguardando caminho, **When** permanece nessa condição, **Then** apresenta idle e não caminha no lugar indefinidamente.
2. **Given** um NPC com caminho viável, **When** se desloca e para, **Then** walk acompanha a velocidade efetiva e retorna a idle sem reinícios repetidos nem deslizamento evidente.
3. **Given** uma solicitação explícita de corrida, **When** há clip compatível disponível, **Then** run acompanha o movimento; sem clip adequado, a capacidade fica registrada como ausente e não é simulada.

### User Story 2 - Sentar e levantar com Smart Objects (Priority: P1)

Dois moradores chegam a vagas distintas do mesmo banco, sentam e permanecem sentados. Depois levantam e podem voltar a caminhar; um terceiro respeita as reservas existentes.

**Why this priority**: Demonstra a integração física reutilizável entre animação, pose, navegação e reserva de objetos.

**Independent Test**: Em cena de demonstração isolada, conduzir dois NPCs reais a slots independentes do mesmo banco, observar a pose sentada, levantar e liberar os recursos na ordem definida pelos contratos existentes.

**Acceptance Scenarios**:

1. **Given** uma chegada confirmada e uma reserva válida, **When** o NPC interage com um slot, **Then** alinha à pose configurada e executa sit_down até uma postura sentada real.
2. **Given** o NPC sentado enquanto a atividade aguarda, **When** o tempo de espera continua, **Then** permanece em sit_idle ou pose equivalente sem repetir sit_down.
3. **Given** dois slots livres no mesmo banco, **When** dois NPCs chegam, **Then** ocupam slots diferentes sem sobreposição evidente; outro NPC não executa pose em slot reservado.
4. **Given** um NPC sentado que conclui ou cancela a interação, **When** a recuperação permitida termina, **Then** executa stand_up, restaura o controle de locomoção e libera apenas recursos próprios conforme os contratos existentes.

### User Story 3 - Falhas e cancelamentos seguros (Priority: P2)

O sistema informa quando uma animação não pode começar ou terminar e protege uma operação nova contra callbacks, triggers ou cancelamentos antigos.

**Why this priority**: Sem identidade e limpeza seguras, a animação pode prender reservas ou interferir em movimento futuro.

**Independent Test**: Repetir e cancelar ações no deslocamento, no sentar e já sentado; remover/inutilizar uma configuração e verificar resultado explícito e limpeza possível.

**Acceptance Scenarios**:

1. **Given** uma animação/binding/Animator indisponível ou um slot invalidado, **When** a operação é solicitada ou interrompida, **Then** recebe falha explícita e não relata sucesso por timer.
2. **Given** uma ação cancelada e outra iniciada, **When** chega callback atrasado ou cancelamento repetido da ação anterior, **Then** a nova ação e seus recursos não são afetados.
3. **Given** um NPC desativado ou destruído durante interação, **When** recursos são limpos, **Then** a limpeza não depende de nova animação e não libera reserva pertencente a outra execução.

### Edge Cases

- Contratos declarados como concluídos não estão disponíveis ou diferem da descrição: documentar assinaturas observadas e adaptar somente à superfície real; não inventar um contrato equivalente.
- Faltam clips de sentar/levantar, repouso ou locomoção para determinado rig: indicar cada capacidade e rig afetados; não substituir uma ação por uma pose enganosa.
- Agente bloqueado, sem caminho, recém-chegado ou aguardando: permanecer visualmente parado.
- Slot ou objeto é destruído/inválido durante a execução: falhar e recuperar/limpar conforme contrato, sem manter posse ou controle presos.
- Animator fica desativado, sem controller ou sem progresso observável: falhar dentro de limite explícito, sem espera infinita ou sucesso falso.
- Cancelamento ocorre durante transição ou enquanto sentado: não iniciar nova locomoção antes de recuperar postura e autoridade física seguras.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Antes de alterar a implementação, levantar os contratos existentes de Smart Objects, Activity Execution e Navigation, distinguindo artefatos encontrados de capacidades apenas declaradas.
- **FR-002**: Inventariar controllers, rigs, avatars e clips candidatos, registrar compatibilidade real e priorizar assets já presentes no projeto.
- **FR-003**: Disponibilizar uma camada reutilizável para NPCs compatíveis, com bindings configuráveis para `idle`, `walk`, `run`, `sit_down`, `sit_idle` e `stand_up`; nenhum controller exclusivo por morador será necessário quando os rigs forem compatíveis.
- **FR-004**: Sincronizar idle/walk/run com estado e velocidade efetivos fornecidos pelo sistema de navegação disponível, preservando uma única autoridade sobre o deslocamento físico.
- **FR-005**: Integrar pose e animação às operações e ao ciclo de vida reais do Activity Execution Core, preservando identidade, resultado, cancelamento, timeout e limpeza existentes.
- **FR-006**: Só alinhar ao Smart Object após chegada confirmada e reserva válida; usar poses reais dos slots e não permitir que a camada de animação reserve, ocupe ou libere slots por conta própria.
- **FR-007**: Manter a pose sentada durante a espera sem reiniciar a transição de sentar; levantar e devolver autoridade de locomoção antes de liberar recursos na ordem segura do contrato real.
- **FR-008**: Falhas de configuração, interrupções e cancelamentos devem ser explícitos, limitados e idempotentes; callbacks antigos não podem concluir ou interromper outra operação.
- **FR-009**: Entregar demonstração isolada, com três NPCs reais, dois sentados simultaneamente no mesmo banco, terceiro respeitando indisponibilidade/reserva, e casos de cancelamento.
- **FR-010**: Registrar testes automatizados e visuais separadamente, evidências sequenciais de Play Mode e o resultado individual de cada capacidade solicitada; não declarar validação sem execução real.
- **FR-011**: Preservar controles e animações dos protagonistas jogáveis, agenda JSON, decisões narrativas e semântica do Activity Core; não implementar controller definitivo, integração geral de rotinas, profissões ou novo scheduler.
- **FR-012**: Não adquirir ou baixar assets, não usar créditos Tripo e não apresentar animação ausente como funcional.

### Key Entities *(include if feature involves data)*

- **Binding de animação**: associação configurável entre um identificador semântico, um recurso compatível e a forma observável de conclusão.
- **Operação de animação**: solicitação vinculada à identidade real de uma execução/operação, com estado, resultado e cancelamento conforme o contrato existente.
- **Pose de slot**: posição e orientação fornecidas por um Smart Object reservado, usadas para alinhamento físico sem transferir propriedade do token à camada visual.
- **Perfil de rig compatível**: configuração compartilhada por modelos que podem usar os mesmos bindings e recursos de animação.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Os seis IDs solicitados têm binding verificado ou aparecem individualmente como capacidade indisponível, com os assets faltantes nomeados por caminho e rig.
- **SC-002**: Em Play Mode, três NPCs executam deslocamento real; dois sentam simultaneamente em slots distintos do mesmo banco e o terceiro respeita a indisponibilidade do slot.
- **SC-003**: A sequência caminhar → sentar → permanecer sentado → levantar → caminhar é observada sem falso sucesso, sobreposição de autoridade de movimento ou reserva duplicada.
- **SC-004**: Cada cancelamento/falha exercitado retorna resultado finito e não afeta a operação seguinte; limpeza não deixa reserva ou controle físico indevido.
- **SC-005**: Compilação e testes de regressão relevantes são executados no projeto Unity registrado; evidências e cenários não executados são identificados separadamente.

## Assumptions

- O escopo inicial usa uma demonstração isolada e componentes reais; não depende do relógio ou da integração completa do scheduler.
- A frase do pedido que declara as quatro features anteriores concluídas é uma premissa a verificar no checkout. A busca inicial encontrou specs 085 e 086; os contratos/código de Activity Execution e Navigation ainda não foram localizados nessa inspeção focada.
- A ferramenta Unity MCP responde para `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`, Unity `6000.3.25f1`; a consulta inicial de assets retornou apenas um `AnimationClip` separado, o que não comprova inventário de clips embutidos em FBX.
- A capacidade final de cada movimento depende de clip, avatar e rig realmente compatíveis; ausência será documentada em vez de substituída por uma demonstração fictícia.
- A branch candidata retornada pelo dry-run é `089-npc-animation-system`. A branch atual e as alterações locais serão preservadas durante a etapa de aprovação da spec.
