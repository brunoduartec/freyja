# Contrato interno do núcleo de execução

Este contrato descreve a fronteira C# pura entre coordenador/receitas e host/adaptadores. Nomes reais podem acompanhar o namespace existente sem mudar as semânticas.

## Entrada e host

- O host entrega `ActivityIntent` resolvida no tempo de jogo atual, `GameTimeSnapshot` e tempo operacional monotônico em segundos.
- O host chama `Tick` de forma síncrona; não há callback em background.
- O host pode pedir cancelamento externo/remoção de NPC. A execução invalida imediatamente seu token e limpa antes de iniciar substituição.
- Resolver continua sendo fonte única da intenção; coordenador não calcula agenda nem prioridade.

## Adapter result

Toda operação assíncrona simulada retorna status determinístico `Running | Succeeded | Failed | Cancelled` e código de razão estável. `Begin` é chamado uma vez por operação. Poll/cancel idempotentes. Respostas incluem dados simbólicos apenas (handle, ponto lógico, destino efetivo); nenhuma estrutura Unity.

## Ownership da interação

1. Consultar candidatos compatíveis ordenados deterministicamente.
2. Tentar reservar candidato com dono e execução; se indisponível, tentar próximo até limite.
3. Guardar token opaco somente após reserva confirmada e emitir `resource_reserved`.
4. Movimento e alinhamento devem concluir; só então solicitar ocupação e validar handle.
5. Liberar explicitamente no sucesso; qualquer saída não terminal também percorre cleanup em ordem inversa.
6. Emitir `resource_released` apenas após confirmação. Reserva inválida ou liberação rejeitada permanece diagnóstico residual.

Repetir ou atrasar liberação do token antigo não pode liberar recurso de outra tentativa.

## Identidade e transição

Comparar compromisso semântico e ocorrência efetiva, não apenas activityId, `ActivityIntent.Identity`, objeto ou instante de tick. Mudança só de validade atualiza deadline corrente. Mudança do compromisso causa cancelamento, cleanup e então início da nova intent. A comparação de ocorrência deve tratar corretamente dia/intervalo em rotinas que atravessam meia-noite.

Uma intent terminal não reinicia a cada tick na mesma ocorrência. Retry usa orçamento/cooldown finito; compromisso novo reinicia orçamento. Saltos de calendário resolvem somente intent corrente, sem replay retroativo. Regressão temporal gera erro de contrato explícito.

## Eventos mínimos

`activity_started`, `step_started`, `step_completed`, `activity_completed`, `activity_cancelled`, `activity_failed`, `resource_reserved`, `resource_released`. Ordem monotônica; origem e correlação disponíveis. Reason codes são enumerados/estáveis; eventos simulados nunca representam fatos narrativos ou confirmação física.
