# NPC Animation Contract

## Locomoção

- Um `NpcAnimationProfile` e `SharedResident.controller` podem ser compartilhados por NPCs com Avatars Humanoid compatíveis.
- `NpcAnimationDriver` lê `IslandWaypointWalker.Velocity` e `MoveStatus`, escreve somente o parâmetro `Speed` e desativa root motion no Animator configurado. Não define Transform, destino, velocidade do NavMeshAgent ou estado da atividade.
- O `IslandWaypointWalker`/NavMeshAgent continua como única autoridade de movimento. Os clips idle/walk/run são in-place. Enquanto uma operação de ação ou pose ocupada possui o Animator, atualizações de locomoção ficam suspensas.
- Limiares do perfil: parar `0,05 m/s`, walk `1,35 m/s`, run `2,5 m/s`; damping `0,12 s`.

## Operações de animação

- `NpcUnityActivityPorts.Begin("play_animation", npcId, executionId, targetId, null, animationId, operationalSeconds)` conserva a identidade de `ActivityOperation` e retorna falha com reason code quando não puder iniciar.
- `NpcAnimatorBindings` aceita semantic IDs do perfil, não caminhos de Animator em dados de rotina. Uma ação finita só conclui depois de entrar no estado certo e concluir o clip não-looping; um hold contínuo não conclui por normalized-time timeout.
- Poll/cancel recebem exatamente `ActivityOperation`, NPC e execução. Chamadas obsoletas não alteram o Animator; cancelamento repetido é idempotente e limitado.
- Clips sentados usados: `sit_down` → `Base Layer.Sitting_Enter` (1,300 s), `sit_idle` → `Base Layer.Sitting_Idle_Loop` (1,667 s), `stand_up` → `Base Layer.Sitting_Exit` (1,033 s). Os três foram importados como Humanoid de `UAL1_Standard.fbx`. O clip idle foi importado com `loop=False`; o controller sustenta o hold, mas ainda é preciso revisar/validar uma repetição cíclica.
- A pose de sentar está visualmente ruim: fica alta e parece que o personagem está caindo. Revisar em uma entrega visual posterior. Preservar a orientação atual e não inverter a direção do NPC no banco.

## Smart Objects e Activity Execution Core

- O Activity Core é o único dono da reserva, ocupação, espera, ordem de levantar e liberação. Nenhum componente de animação reserva ou libera vagas.
- Fluxo normal observado: reservar handle exato → mover até aproximação → confirmar movimento → alinhar/transferir controle de movimento → ocupar slot → `sit_down` → pose sentada durante `wait` → `stand_up` → mover até aproximação → liberar o mesmo handle.
- Cancelamento durante o hold observado: o Core manteve o recurso durante `stand_up`, concluiu a limpeza e liberou somente o handle da execução cancelada. O demo não confirmou retorno do NPC ao destino de uma nova intenção após cancelamento.
- Se a ação não puder iniciar ou concluir, retornar failure/reason explícito; não reportar sucesso falso. A claim permanece sob o retry/cleanup do Activity Core até o recurso ser restaurado e liberado com segurança.

## Próxima feature: `npc-world-integration`

- A integração entrega à execução o `ActivityIntent`, `GameTimeSnapshot` e tempo operacional monotônico por meio do `NpcActivityController`/`ActivityCoordinator` existente; a resolução de agenda fica fora da animação.
- Inicializar e manter, por ator, `NpcLocationRegistry`, `NpcScheduleTextAssetProvider`, `NpcUnityActivityPorts`, `NpcAnimatorBindings`, `NpcAnimationDriver`, perfil compartilhado, `IslandWaypointWalker` e `SmartObjectReservationOwner`.
- Encaminhar `npcId`, `executionId`, `ActivityOperation`/operation identity, cancelamento e retry sem substituí-los nem gerar scheduler paralelo.
- World integration será dona do relógio global e do registro de destinos. Activity Core conserva autoridade sobre ciclo da atividade, claims e limpeza; Navigation conserva movimento; Animation conserva apenas pose/estado visual.
- Não adicionar nesta integração o NPC Controller definitivo, rotinas completas, atividades profissionais ou regras específicas de postura/altura do banco. A revisão de qualidade de `sit_down` permanece uma pendência visual explícita.
