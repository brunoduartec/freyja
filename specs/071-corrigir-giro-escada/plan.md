# Plano técnico — giro no meio da escada

Ajustar a transição entre a passada guiada da subida e a locomoção normal. O comando do jogador determina se é pausa ou mudança de direção. A velocidade após colisões continua servindo à animação de locomoção.

## Sequência da primeira entrega

1. Reproduzir na cena `Ilha_Tripo_Caminhada_011`, em Play Mode. Confirmar o lance e registrar vídeo, direção solicitada, deslocamento real, estado do apoio e atividade da câmera no momento da falha.
2. Em `CaisPlayerController`, disponibilizar a intenção planar calculada antes da colisão, inclusive a ausência de comando. Em `StairFootPlacement`, usar a mesma regra de pausa para a amostragem da animação de subida e para o IK. Um comando lateral ou de descida encerra a retenção da passada mesmo com velocidade real zero.
3. Usar a redução gradual do peso da passada para passar aos apoios sondados sob os pés. Manter os apoios antigos apenas durante a transição e recalcular as âncoras a partir da posição atual ao retomar, inclusive em reversões rápidas e no mesmo lance. Conferir que a animação base, a pelve e as rotações das botas acompanham essa transição.
4. Verificar compilação e Console. Executar o [roteiro de Play Mode](quickstart.md). Se o aviso de câmera reaparecer, registrar câmera ativa, display de saída e mensagens do Console junto à captura. Uma causa independente volta para avaliação de escopo.
5. Registrar as evidências na spec e conferir os critérios de aceitação antes da entrega.

## Ambiente e dependência

Unity 6000.3.25f1, Built-in, cena principal do projeto e MCP Unity. A implementação usa o commit 9ff36a5, base da cena em que a falha foi observada. A branch 071 foi posicionada nessa base antes de receber o código. As alterações locais do painel de debug seguem no workspace.

Durante o salto, limpar a classificação de subida quando o ajuste de velocidade da escada for ignorado. Incluir uma tentativa de salto no lance no roteiro de regressão.

A mudança se concentra no estado de locomoção existente. O plano respeita o GDD e a constituição ao preservar a cena de referência, usar o Unity para validar e exigir evidência visual.

## Revisão aprovada — controle longitudinal

Filtrar a velocidade solicitada no eixo do lance quando a sonda sob a posição do protagonista encontrar um degrau dentro da tolerância de degrau configurada no controlador, cobrindo a transição do corpo sobre o espelho. Usar a velocidade filtrada tanto para orientar o corpo quanto para decidir se existe intenção de movimento. Componente longitudinal abaixo de 15% da velocidade solicitada conta como lateral. Preservar a limitação de velocidade na subida, o controle livre no patamar e a regra existente de salto. O collider Patamar da B3 participa do apoio visual da passada e permite movimento livre. Verificar os dois lances em Play Mode.
