# Research: NPC Animation System

## Contratos existentes verificados

- `specs/085-smart-objects-foundation/`: descoberta de candidatos, slots independentes e reservation handles exatos.
- `specs/086-npc-schedule-core/`: resolve agenda em `ActivityIntent`; animação não decide calendário.
- `specs/087-npc-activity-execution-core/contracts/execution-contract.md`: estados terminais determinísticos, propriedade por identidade e cleanup antes de substituição; o Core chama `stand_up` antes de liberar claim ocupada.
- `specs/089-npc-navigation-system/contracts/navigation-operation.md`: ticket de navegação exato, rejeição de resultado obsoleto e Navigation como autoridade de movimento. `IslandWaypointWalker` expõe `Velocity`, `Direction`, `MoveStatus`, `TryBeginMove`, `GetMoveStatus` e `CancelMove`.
- `NpcAnimatorBindings` e `NpcUnityActivityPorts` adaptam `play_animation`, `move_to`, `align_to`, reservations e polling por `ActivityOperation`.

## Catálogo usado

- Residentes: 11 prefabs avaliados; Avatars Humanoid válidos. Controllers antigos variavam por personagem e tinham apenas estados básicos sem parâmetro de blend.
- Kevin Iglesias Human Basic Motions FREE: `HumanM@Idle01` (2,7 s, loop), `HumanM@Walk01_Forward` (0,8 s, loop) e `HumanM@Run01_Forward` (0,6 s, loop), clips in-place Humanoid. Variantes femininas continuam disponíveis.
- Quaternius Universal Animation Library, `Assets/ThirdParty/Quaternius/Universal Animation Library/UAL1_Standard.fbx`: `Armature|Sitting_Enter` (1,300 s), `Armature|Sitting_Idle_Loop` (1,667 s) e `Armature|Sitting_Exit` (1,033 s), importados como Humanoid. O import reporta `loop=False` para os três, incluindo `Sitting_Idle_Loop`; o perfil usa os dois primeiros como transição e hold contínuo, e o último como saída.
- `Player004_Male_Casual.fbx` contém `Man_Sitting`/`Man_Standing` como Generic. Não foram retargetados nem alterados; protagonista e controles não fazem parte desta feature.
- O usuário localizou a Universal Animation Library gratuita já baixada. Assim, os três clips necessários existem; não foi comprado nem importado conteúdo Asset Store e nenhum crédito Tripo foi usado.

## Decisões de implementação

1. Um `SharedResident.controller` e `SharedResidentProfile.asset` para NPCs Humanoid compatíveis.
2. `IslandWaypointWalker.Velocity` dirige `Speed`; walk inicia a partir de `1,35 m/s`, run a partir de `2,5 m/s`; zero efetivo leva ao idle.
3. Locomoção in-place e `Animator.applyRootMotion=false` nos NPCs do demo.
4. IDs semânticos de sentar/levantar ficam no perfil. Activity Core/Unity ports conservam identidade de execução e propriedade do banco.
5. O ciclo está funcional no demo, mas a pose de sentar parece alta e como se o NPC estivesse caindo. A qualidade deve ser revista em outra entrega; não inverter a orientação que está correta agora.
6. `Sitting_Idle_Loop` não repete segundo o import auditado (`loop=False`). Foi observado o NPC continuar sentado; repetir o movimento ciclicamente segue sem prova e precisa de ajuste/validação futura.
7. Busca de Asset Store foi feita após inventário inicial; com os clips locais Quaternius confirmados, não houve motivo para baixar ou comprar outro pacote.

## Pontos verificados manualmente no Editor

- Dois residentes ocuparam simultaneamente vagas distintas do banco; terceiro recebeu `target_unavailable`.
- O cancelamento durante `wait` acionou `stand_up`/cleanup e liberou o handle próprio. O encerramento normal do outro residente também tocou `stand_up`, moveu até aproximação e liberou o recurso.
- Walk foi visto no overlay a `1,36 m/s`; run foi observado em movimento a `2,85–2,92 m/s` (o agente recebeu configuração de `3,00 m/s`). A captura de locomoção incluída mostra walk, não run.
- Cancelamento durante navegação, pausa/retomada do Animator e retorno a uma nova intenção após cancelar não foram exercitados pelo harness nesta entrega.
