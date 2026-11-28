# Modelo de dados: NPC Debug Tools

Snapshots são cópias Editor-only com valores simples, sem `GameObject`, `Transform`, `Animator`, agente ou serviço vivo. Campos com fonte ausente carregam status `unknown`, `unavailable`, `not_applicable`, `stale` ou `available`; não há preenchimento por inferência.

## Snapshot global

- schema version, sessão de Play Mode, path relativo da cena, modo, timestamp UTC, hora de jogo e calendário observado;
- relógio: disponível/indisponível, avanço automático, taxa própria do calendário e escopo da pausa;
- contagens de controllers registrados/ativos/prontos/com falha;
- contagem de slots disponíveis, reservados, ocupados e desativados;
- filtro aplicado, quantidade retornada, limite de resultados e indicadores de truncamento/descarte.

## Snapshot de NPC

- identidade: ID configurado, nome de exibição e status ativo/pronto;
- rotina: atividade prevista atual, alvo, origem, prioridade e intervalo válido; motivo/trace realmente devolvido pelo resolver quando disponível;
- execução: estado, etapa/operação, ID, atividade efetiva, alvo pedido/resolvido, fallback, motivo, duração/idade operacional e ownership residual;
- navegação: estado atual, alvo simbólico/posição efetiva se já expostos, distância/velocidade e cantos do caminho já calculado; valores geométricos ausentes ou inválidos com status;
- animação: hash/nome do estado e clips atuais lidos do Animator; sem presumir causalidade com a operação;
- reserva: IDs de objeto/slot, estado, atividades compatíveis e owner; recurso permanece diferente de presença/ocupação física;
- conclusão: última transição `ActivityCompleted` vista no buffer, incluindo execução/hora/atividade; pode estar indisponível se ocorreu antes da inscrição.

## Snapshot de Smart Object

- ID, região, nome, posição e disponibilidade;
- slots ordenados por ID, estado real, atividades configuradas, posição de aproximação/pose se válida, owner NPC/execução quando puder ser associado;
- status de configuração ambígua/ID duplicado sem escolher arbitrariamente um dono.

## Eventos

- sequência e ordem de captura, tipo, NPC, execução, atividade pretendida/efetiva, alvo, etapa, razão, dia/minuto e tempo operacional;
- capacidade padrão de 512; descartes acumulados; filtro e limite de consulta aplicados;
- o buffer mantém apenas dados de transição pequenos e não guarda objetos Unity.

## Comando

- ID correlacionável, comando, alvo, origem `debug`, instante, estado `accepted`/`pending`/`completed`/`rejected`/`failed`, motivo e resultado observado;
- comando aceito representa uma solicitação encaminhada e só vira concluído após confirmação no snapshot/evento.

## Enumerações/status

- `DataStatus`: available, unknown, unavailable, not_applicable, stale.
- `NpcDebugFilter`: busca textual e estados all, active, failed, fallback, reserving/occupied.
- `DebugCommandStatus`: accepted, pending, completed, rejected, failed, unsupported.
- `ActivityState` e `InteractionSlotState` continuam sendo enums dos domínios existentes; não duplicar sua máquina de estado.
