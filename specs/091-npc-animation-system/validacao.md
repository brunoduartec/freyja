# Validação — NPC Animation System

**Branch:** `091-npc-animation-system`  
**Base:** `55f81b0` (`Clarify final Unity validation status`)  
**Unity:** 6000.3.25f1, Built-in Render Pipeline  
**Data:** 2026-11-27  
**Escopo:** shared controller/profile, locomotion, clips Quaternius de sentar/levantar, adapter Activity Core e demo isolado.

O trabalho está na worktree isolada `/Users/bruno.duartec/Documents/Projects/Freyja-091-npc-animation-system`; o checkout original com mudanças preexistentes foi preservado. Não foi feito commit nem merge. O Unity MCP estava sem conexão com o Editor; usei a instância real do Unity Editor e seu Test Runner. `Packages/manifest.json` não mudou e nenhum pacote foi instalado/atualizado.

## Inventário observado

- Inventário de Animator/rig/avatar/clip/controller exportado pelo Unity Editor em [`asset-inventory.txt`](asset-inventory.txt). Foram encontrados 11 prefabs residentes com Avatar Humanoid válido. Controllers antigos eram individuais e tinham estados básicos; esta feature adiciona um controller compartilhado para os NPCs compatíveis, usado pelos três residentes da cena de demo.
- Kevin Iglesias Human Basic Motions FREE, clips in-place Humanoid: `HumanM@Idle01` (2,7 s, loop), `HumanM@Walk01_Forward` (0,8 s, loop), `HumanM@Run01_Forward` (0,6 s, loop). Há variantes femininas no pack.
- Quaternius Universal Animation Library, `Assets/ThirdParty/Quaternius/Universal Animation Library/UAL1_Standard.fbx`, rig importado como Humanoid: `Armature|Sitting_Enter` (1,300 s), `Armature|Sitting_Idle_Loop` (1,667 s), `Armature|Sitting_Exit` (1,033 s). Os três foram ligados às ações semânticas `sit_down`, `sit_idle` e `stand_up`.
- O import audit reporta `loop=False` nos três clips Quaternius, inclusive `Sitting_Idle_Loop`. O ator permanece no estado/pose sentada durante o hold observado, mas repetição cíclica do movimento não foi validada; revisar a configuração/animação na próxima passada.
- `Assets/_Project/Art/Characters/Player004/Player004_Male_Casual.fbx` oferece `Man_Sitting` e `Man_Standing` Generic. Não foram usados ou editados, e não houve alteração de animação/controle dos protagonistas.
- Os clips necessários existem no pack gratuito local; não houve compra nem import de Asset Store, e nenhum crédito Tripo foi usado. A busca Asset Store previamente feita ficou apenas como pesquisa; a UAL local atendeu a necessidade do ciclo solicitado.
- Pesquisa anterior: [MC Sample](https://assetstore.unity.com/packages/3d/animations/mc-sample-believable-3d-animations-by-mocap-central-291284) anuncia animação sentada ao piano, sem confirmar adequação ao banco; [Everyday Motion Pack Free](https://assetstore.unity.com/packages/3d/animations/everyday-motion-pack-free-115067) não confirmou publicamente as transições necessárias. Nenhum foi importado.

## Alteração da receita existente

A receita `sit` recebeu `move_to approach` entre `stand_up` e `release_slot`. Isso altera sua sequência anterior: a vaga permanece reservada durante o retorno à aproximação e a conclusão depende desse movimento. Consumidores de `recipes.json` que usam `sit` herdam essa etapa; falha no retorno segue o cleanup do Activity Core. A mudança diverge da promessa original de não alterar a semântica da receita e deve ser considerada na integração seguinte. O fluxo normal foi observado no demo; não implica validação de todos os consumidores existentes.

## Configuração efetiva

- Controller compartilhado: `Assets/_Project/Art/NpcAnimation091/SharedResident.controller`.
- Perfil compartilhado: `Assets/_Project/Resources/NpcAnimation091/SharedResidentProfile.asset`.
- Parâmetro `Speed`; parada `0,05 m/s`, walk `1,35 m/s`, run `2,5 m/s`, damping `0,12 s`.
- Os NPCs do demo usam clips in-place e `Animator.applyRootMotion=false`. `IslandWaypointWalker`/NavMeshAgent mantém a autoridade de posição; `NpcAnimationDriver` consulta velocidade real. O walker não compete escrevendo o parâmetro de animação.
- Os dois slots do banco mantêm pontos de aproximação independentes. O cancelamento/saída conserva a orientação atual, sem inverter frente e lados.

## Evidência real de Play Mode

- [`evidence/playmode-activity.png`](evidence/playmode-activity.png): Game View com `nuno-pires` e `amelia-faria` ocupando vagas distintas e simultâneas; `mauro-neves` retorna `target_unavailable`. A qualidade visual de sentar ainda parece alta/como queda; está anotada como pendência para rever. Não ajustei novamente o facing nessa validação.
- [`evidence/playmode-locomotion.png`](evidence/playmode-locomotion.png): Game View da locomoção, overlay a `1,36 m/s` durante caminhada e tentativa do terceiro residente sem vaga. O estado idle foi visto em Game View com `v=0,00`; run foi visto ao vivo com `v=2,85–2,92 m/s` depois de solicitar `3,00 m/s`. A PNG atual captura walk, não run.
- O teste visual de sentar também percorreu `wait`; clicar `Cancel all` enquanto os dois estavam sentados tocou a recuperação e liberou a claim própria de ambos. Em execução anterior do mesmo harness, schedule end levou o segundo residente por `stand_up`, retorno à aproximação e `release_slot`/`Completed`.
- Não foi executado cancelamento durante navegação, pausa/retomada do Animator, invalidação do Smart Object pela cena Activity, nem retorno automático a uma nova intenção após cancelamento. Esses fluxos não são considerados validados por esta entrega.

## Resultados de teste

- Unity Test Runner Edit Mode: **PASS — 40/40**, 0 falhas, 0 ignorados (inclui 12 testes de animação, 12 de Smart Objects e 16 de WorldBuilder). XML salvo em [`evidence/editmode-results.xml`](evidence/editmode-results.xml).
- Unity Test Runner Play Mode: **PASS — 8/8**, 0 falhas, 0 ignorados (inclui 1 teste de animação/Smart Object, 6 testes de Smart Objects e 1 smoke test de WorldBuilder). XML em [`evidence/playmode-results.xml`](evidence/playmode-results.xml). O teste de reserva que percorre NavMesh levou ~15,3 s.
- `dotnet run --project Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj -- --test`: última execução documentada **PASS — 160 assertions**; dois avisos de lacuna em fixture de fisherman são preexistentes.
- Os testes do Editor passaram na instância Unity 6000.3.25f1. Os XMLs indicam todos os casos executados aprovados. Não afirmo que pause/time-scale ou cancelamento em movimento foram cobertos por eles.
- O resultado Edit/Play Mode pode ser repetido em Unity Test Runner, escolhendo EditMode/PlayMode e executando `Run` no nó `Freyja`.

## Matriz AN-01–AN-17

| ID | Estado | Evidência / limite |
|---|---|---|
| AN-01 | Pass | Test Runner compilou e executou as suites; 40 Edit + 8 Play aprovados, sem erro de compilação reportado. |
| AN-02 | Pass | Idle real visto com `v=0,00`; controller compartilhado ativo no demo. |
| AN-03 | Pass | Walk visto no Game View a `1,36 m/s`; retorno a idle ao parar observado. |
| AN-04 | Pass parcial | Run real observado a `2,85–2,92 m/s` depois de pedido de `3,00`; captura de tela salva representa walk. |
| AN-05 | Pass | Dois caminhos chegam, concluem `align_to` e ocupam slots diferentes; log de Activity Core e screenshot. |
| AN-06 | Pass | `Sitting_Enter` real encerra e inicia o hold, visível na captura de dois residentes. |
| AN-07 | Parcial | Os atores permanecem sentados em `wait`, mas `Sitting_Idle_Loop` importou sem loop; repetição cíclica não provada. |
| AN-08 | Pass | `Sitting_Exit` foi observado em cancelamento durante `wait` e em schedule end do fluxo normal. |
| AN-09 | Pass | Dois residentes sentados simultaneamente em dois slots: screenshot e Play Mode. |
| AN-10 | Pass | Terceiro recebe `target_unavailable` quando as duas vagas estão reservadas/ocupadas. |
| AN-11 | Parcial | Cancelamento durante pose/`wait` testado e limpo. Cancelamento no meio da navegação não foi executado manualmente. |
| AN-12 | Parcial | Schedule end conclui stand-up, move ao ponto de aproximação e libera. Retomar uma nova intenção após cancelamento não foi exercitado. |
| AN-13 | Parcial | Testes Edit cobrem validação/identidade de binding; o cenário Unity demo usa bindings presentes, então falha de binding/timeout não foi exercitada no Editor. |
| AN-14 | Parcial | Suite Play cobre ciclo de vida/desativação de Smart Objects, mas não o invalida durante este fluxo completo de Activity/Animation. |
| AN-15 | Pass | Testes Edit de operation identity, polling/cancel stale e idempotência aprovados. |
| AN-16 | Pendente | Pausar/retomar Animator e relógio operacional não foi testado. |
| AN-17 | Pass parcial | Suites do projeto e simulator passaram; protagonistas não foram alterados. Integração com rotinas globais e controller definitivo estão fora desta feature. |

## Qualidade de sentar e ajustes posteriores

O ciclo funcional usa os clips reais da UAL, mas a imagem ainda mostra uma pose elevada que parece uma queda. A pose precisa ser revista. O usuário aprovou deixar o ajuste fino para depois e pediu apenas que o NPC não volte a ficar invertido; preserve a orientação e o lado correto do banco na próxima passada. Também conferir `loopTime` de `Sitting_Idle_Loop` no import e validar por tempo prolongado.

## Contratos a usar em `npc-world-integration`

- Fornecer `ActivityIntent`, `GameTimeSnapshot` e segundos operacionais monotônicos ao `NpcActivityController`/`ActivityCoordinator`.
- Manter por ator `NpcLocationRegistry`, `NpcScheduleTextAssetProvider`, `NpcUnityActivityPorts`, `NpcAnimatorBindings`, `NpcAnimationDriver`, `NpcAnimationProfile`, `IslandWaypointWalker` e `SmartObjectReservationOwner`.
- Usar IDs de NPC/execução/operação e handles exatos do Activity Core; não duplicar reservas, navegação, scheduler ou tickets de animação.
- `IslandWaypointWalker.Velocity` é fonte da locomotion; root motion continua desligado nos NPCs deste perfil.
- A sequência de banco é reserva → chegada/navegação → alinhamento → ocupação → `sit_down` → hold `sit_idle` → `stand_up` → retorno à aproximação → release do handle exato. Cleanup mantém o recurso até a saída ser confirmada.
- World integration controla relógio e registro de destinos; o Core controla ciclo da atividade/claims; Navigation controla movimento; Animation controla somente pose e operação visual.
