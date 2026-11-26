# Modelo de dados — execução de atividades

## Execução e compromisso

`ActivityIntent` é o contrato de entrada existente e não será redefinido.

| Campo conceitual | Regra |
|---|---|
| Compromisso | NPC, atividade, alvo, parâmetros semânticos, origem/fonte/bloco e ocorrência temporal |
| `executionId` | Identidade única da tentativa, nunca derivada apenas de intent ou tick |
| Janela vigente | Lida da intenção re-resolvida; atualizada sem reiniciar compromisso equivalente |
| Atividade pretendida | ID original do intent |
| Atividade executada | Receita realmente selecionada; distingue fallback |
| Destino lógico/efetivo | Alvo simbólico do intent e resultado relatado pelo adaptador quando houver |

Ocorrência identifica bloco e dia absoluto aplicável. Uma rotina dividida à meia-noite preserva a continuidade quando representar o mesmo compromisso; o início no dia seguinte representa nova ocorrência quando o resolver emitir outro compromisso. Intents antigos nunca são retomados sem nova resolução.

## Máquina de estados

`Pending → Preparing → (Moving) → Executing → CleaningUp → Completed | Cancelled | Failed`.

- Apenas um estado terminal é publicado após tentativa de cleanup.
- Um recurso residual é representado com falha de limpeza e impede início inseguro.
- Interrupção pode entrar em `CleaningUp` de qualquer estado não terminal.
- `executionId` invalida operações/retornos atrasados.
- Transições por tick têm limite fixo configurado.

## Operação primitiva

Uma operação tipada fornece início único, progresso por tick e resultado `Running`, `Succeeded`, `Failed` ou `Cancelled`; conclusão e cancelamento são idempotentes. Configurações incluem apenas campos permitidos à operação (capability, região, alvo, animação, ponto, prazo/duração e limites). Uma receita possui schema version 1, `activityId` único e lista ordenada de passos.

Whitelist: `find_target`, `reserve_slot`, `move_to`, `align_to`, `occupy_slot`, `play_animation`, `wait`, `release_slot`. Validação cruza existência do activityId no catálogo, args obrigatórios, prazos positivos, ordem coerente e ownership do recurso.

## Recursos e portas

- `ReservationToken`: opaco, por execução/dono/slot e geração; só o adaptador decide validade e confirma liberação.
- `ExecutionContext`: operações atuais, token e handle, posses em ordem de aquisição, tentativa, cooldown e falhas de limpeza.
- `InteractionPort`: candidatos determinísticos por região/capacidade, reservar, validar, ocupar e liberar.
- `MovementPort`: iniciar, consultar progresso, cancelar e informar destino efetivo simulado.
- `PosePort`: alinhar, iniciar/restaurar animação simbólica, consultar e cancelar.

Fakes são implementações apenas no harness. Slots exclusivos distinguem reservado de ocupado; liberação exige token proprietário correto. Falhas configuráveis: sem alvo, indisponibilidade, invalidar reserva, caminho impossível/timeout, animação rejeitada e cleanup rejeitado.

## Tempo e eventos

Cada tick recebe `operationalSeconds` finito e monotônico e snapshot atual de calendário do jogo. Timeout de operações usa segundos; janela/`wait` usa dia absoluto e minuto de jogo. Nenhuma operação lê relógio real.

Evento leva sequência determinística, tempo simulado, NPC, `executionId`, atividade pretendida/executada, passo, reason code e origem simulada. Tipos: atividade iniciada/concluída/cancelada/falhou; passo iniciado/concluído; recurso reservado/liberado. Eventos não são eventos de resolução e não alimentam narrativa.

## Atividades v1

- `idle`: com alvo, mover até destino e aguardar fim; sem alvo, aguardar no local.
- `wander`: encontrar região, percorrer ponto autorizado determinístico, esperar intervalo e repetir dentro da janela; sem recurso reservado.
- `sit`: encontrar e reservar slot compatível; mover, alinhar, ocupar, tocar animação de entrada, aguardar prazo, tocar saída e liberar. Interrupção tenta restaurar pose e liberar.

Falha/ausência de receita pode usar fallback `idle` uma vez por compromisso se não houver recurso residual inseguro. Retry de busca é finito, tem cooldown simulado e reseta numa intenção nova.
