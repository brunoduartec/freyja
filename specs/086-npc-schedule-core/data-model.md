# Modelo de dados

- `ActivityDefinition`: `id`, tipo de alvo aceito (`none`, `location`, `object`) e rótulo opcional. Não representa executor.
- `ScheduleEntry`: `id`, minutos inicial/final, `activityId`, alvo lógico opcional.
- `ScheduleTemplate`: `schemaVersion`, `id`, entries base.
- `NpcScheduleDefinition`: `schemaVersion`, `npcId`, template opcional, entries próprios, substituições, remoções e adições.
- `ScheduleOverride`: `schemaVersion`, `id`, `npcId`, prioridade 50/70/90, condições e entries.
- `ScheduleConditions`: dia absoluto, dia da semana, clima e flags booleanas; todos os campos presentes devem corresponder.
- `GameTimeSnapshot`: dia absoluto positivo e minuto [0,1439]; weekday é derivado do calendário do conjunto carregado.
- `WorldContextSnapshot`: clima e mapa de flags explicitamente presentes; ausência de flag não equivale a false.
- `ActivityIntent`: NPC, atividade, alvo, janela de validade, prioridade e origem/bloco; dado de intenção sem efeitos colaterais.
- `ResolutionTrace`: razão compacta, candidatos/empates relevantes e avisos de fallback.
- `ResolvedScheduleData`: catálogo, agendas expandidas e regras validadas, somente leitura para o resolver.
- `ScheduleTransition`: transição planejada emitida pelo simulador apenas quando atividade/alvo/origem/bloco/prioridade mudam.

Referências entre JSON são resolvidas/validadas ao carregar. Templates nunca são alterados por expansão. Entries têm IDs únicos no escopo da agenda/regra. Destinos são identificadores simbólicos fictícios.
