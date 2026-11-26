# Feature Specification: Smart Objects Foundation

- **Feature Branch**: `085-smart-objects-foundation`
- **Created**: 2026-11-26
- **Status**: Approved — aprovação explícita recebida em 2026-11-26
- **Input**: `.codex-attachments/.../freyja-smart-objects-foundation.md` (escopo integral e critérios detalhados)

## User Scenarios & Testing

### User Story 1 — Configurar objetos interativos pelo Inspector (P1)

Como designer de cenário, quero transformar um prefab em Smart Object e configurar slots, atividades, aproximação e pose final no Inspector, para disponibilizar capacidades de interação sem scripts específicos por atividade.

**Teste independente**: configurar e duplicar um prefab de banco de dois slots; editar referências e atividades pelo Inspector; confirmar IDs de objeto únicos e persistentes e IDs locais de slot estáveis.

**Aceite**: visual, colliders e pontos de interação ficam separados; gizmos distinguem aproximação, pose e orientação; validações apontam referências/IDs/configurações inválidas sem sobrescrever IDs silenciosamente.

### User Story 2 — Descobrir e reservar capacidades sem conflito (P1)

Como consumidor futuro de atividades, quero consultar objetos ativos por ID, região e atividade, reservar um slot compatível e liberar apenas minha reserva vigente, para que vários NPCs possam buscar capacidades sem uso duplo.

**Teste independente**: testes Edit Mode cobrem registro, descoberta, ordenação determinística, disputa, ocupação, invalidação e limpeza de reservas.

**Aceite**: um slot admite um proprietário; handle identifica objeto, slot, proprietário e token vigente; handles antigos/de terceiros não ocupam nem liberam reservas atuais; estados runtime não persistem em prefab; desativação, destruição, cancelamento e reinício invalidam reservas conforme lifecycle documentado.

### User Story 3 — Verificar uma fatia vertical com bancos e capacidades futuras (P2)

Como designer/implementador, quero observar três agentes pedindo `sit`, reservando dois slots do banco A e um do banco B, para verificar concorrência, fallback, aproximação navegável e liberação.

**Teste independente**: iniciar cena de demonstração reproduzível e observar os agentes caminharem antes de ocupar/alinha-los à pose final; cancelar uma solicitação e provocar caminho inválido para confirmar liberação. Poço (`collect_water`) e cais (`fish`/`watch_sea`) demonstram somente a configuração de capacidades se os assets e superfícies forem adequados.

**Aceite**: dois handles distintos em A, terceiro em B, sem atribuição dupla; deslocamento não teletransporta; timeout/cancelamento libera somente o handle próprio. Na evidência, os três agentes e bancos são localizáveis e os gizmos mostram slots, estados e orientação legíveis. Pose de referência não representa animação de sentar completa.

## Edge Cases

- Objeto/slot/owner é desativado ou destruído durante uma reserva ou deslocamento.
- IDs vazios/duplicados, slot incompatível/desabilitado, referências ausentes ou ponto fora do NavMesh.
- Reserva disputada, handle de outro dono, handle expirado, liberação repetida ou Play Mode reiniciado.
- Primeiro candidato torna-se indisponível entre descoberta e reserva; consumidor tenta o próximo candidato.
- Ausência de NPC, superfície de navegação, acesso ao Editor/MCP ou asset adequado: registrar o bloqueio real, sem criar sistema paralelo de movimento nem alegar validação visual.

## Requirements

### Functional Requirements

- **FR-001**: Smart Objects oferecem ID estável por instância, agrupamento/região opcional e slots serializados configuráveis pelo Inspector.
- **FR-002**: Cada slot possui ID local estável, atividades suportadas, disponibilidade e Transforms distintos para aproximação e pose/orientação final.
- **FR-003**: Registro acompanha lifecycle de cena/componente e permite descoberta por ID, região e atividade, excluindo objetos inativos; candidatos ordenam por distância e desempate determinístico por ID.
- **FR-004**: Reserva valida atividade, slot, dono e disponibilidade de modo indivisível no contexto Unity; ocupação exige handle vigente do dono; liberação repetida é segura e não afeta terceiros.
- **FR-005**: Desativação/destruição e cancelamento invalidam/liberam recursos; reativação e novo Play Mode não restauram handles anteriores. Configuração e estado runtime permanecem separados.
- **FR-006**: Inspector/gizmos expõem IDs, aproximação, pose, orientação, disponibilidade e estados; validação distingue erro de configuração de checagem não executável.
- **FR-007**: Criar prefab reutilizável de banco com dois slots `sit`, colliders separados dos pontos e visual provisório facilmente substituível somente se a busca de assets locais não encontrar banco adequado; sem Tripo/serviços pagos.
- **FR-008**: Configurar dois bancos próximos numa cena/área de demonstração não invasiva, com três solicitantes, fallback, chegada antes da ocupação e cancelamento/caminho inválido com liberação.
- **FR-009**: Quando adequados e acessíveis, configurar poço (`collect_water`) e slots de cais (`fish` e/ou `watch_sea`) sem implementar mecânicas.
- **FR-010**: Testes Edit Mode cobrem registro, IDs, descoberta, concorrência, compatibilidade, estados, handles e lifecycle; testes Play Mode cobrem integração e navegação quando pré-requisitos existirem. Executar testes e reportar resultados/evidências reais.
- **FR-011**: Não implementar scheduler, rotinas, executor de atividades, animação completa/IK, pesca/coleta, persistência de reservas ou novo sistema de movimento.

### Key Entities

- **Smart Object**: instância configurável e descoberta no mundo, com identidade estável e slots.
- **Interaction Slot**: capacidade identificada localmente, com atividades, estado, aproximação e pose final.
- **Reservation Handle**: referência opaca à instância/slot, proprietário e geração/token, válida apenas para a claim vigente.
- **Registry**: índice de objetos ativos e candidatos determinísticos por atividade/região/origem.

## Success Criteria

- **SC-001**: Três solicitantes obtêm, sem duplicação, dois slots no banco A e fallback para um slot no banco B; a disputa por um slot único tem um vencedor.
- **SC-002**: Testes executados comprovam consultas e limpeza corretas para disable/destroy/cancelamento, handles antigos ou alheios e reinício de Play Mode.
- **SC-003**: Um prefab duplicado mantém IDs únicos/estáveis após salvar/reabrir e permite adicionar/editar slots e atividades sem código C#.
- **SC-004**: Agentes alcançam aproximações configuradas antes do alinhamento final; caminho inválido/cancelamento não deixa slot preso. Se pré-requisitos faltarem, a limitação é registrada e a navegação fica explicitamente bloqueada.
- **SC-005**: Evidência visual mostra a configuração e estados com leitura espacial clara; evidência de logs/testes não é descrita como substituta de validação visual indisponível.
- **SC-006**: Compilação e testes relevantes não introduzem erros, warnings recorrentes ou regressões atribuíveis à feature.

## Assumptions

- A cena principal é `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`; prefere-se cena de validação isolada/área pequena e não invasiva.
- Reutilizam-se navegação, moradores e convenções existentes, sem atualizar dependências fora do escopo aprovado.
- Tags base: `sit`, `collect_water`, `fish`, `watch_sea`; capacidade deriva dos slots habilitados compatíveis.
- As atividades indicam capacidade futura. Uma pose final ou agente em pé não prova animação completa da atividade.
- A spec anexada é a fonte integral dos casos de teste, limites de escopo e instruções de entrega; esta spec resume seus critérios para aprovação.
