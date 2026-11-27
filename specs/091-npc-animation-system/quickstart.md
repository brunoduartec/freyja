# Quickstart: NPC Animation System

## Configuração reutilizável

1. Confirme que os clips Kevin Iglesias Human Basic Motions FREE e `Assets/ThirdParty/Quaternius/Universal Animation Library/UAL1_Standard.fbx` estão importados.
2. No Unity, execute `Freyja > NPC Animation 091 > Create Shared Locomotion Assets`. O comando cria `Assets/_Project/Art/NpcAnimation091/SharedResident.controller` e `Assets/_Project/Resources/NpcAnimation091/SharedResidentProfile.asset`.
3. Em cada NPC compatível, use Avatar Humanoid válido, adicione `NpcAnimatorBindings` e `NpcAnimationDriver`, atribua o perfil compartilhado e o `IslandWaypointWalker`. Mantenha `Animator.applyRootMotion` desativado; o walker/NavMeshAgent é o único dono do deslocamento.
4. O driver lê `IslandWaypointWalker.Velocity` real. O perfil usa `Speed`, parada até `0,05 m/s`, caminhada a partir de `1,35 m/s`, corrida a partir de `2,5 m/s` e damping de `0,12 s`.

## Animações ligadas ao perfil

- Locomoção in-place de `Human Basic Motions FREE`: `HumanM@Idle01`, `HumanM@Walk01_Forward` e `HumanM@Run01_Forward`.
- Sentar/levantar de Quaternius Universal Animation Library: `Armature|Sitting_Enter` → `Armature|Sitting_Idle_Loop` → `Armature|Sitting_Exit`, em rig Humanoid.
- IDs semânticos usados pelo Activity Core: `sit_down`, `sit_idle`, `stand_up`. O controller/perfil é compartilhado; não crie um controller por morador.

**Limitação visual registrada:** o ciclo de sentar funciona, mas a pose ainda parece alta e dá impressão de queda. A animação de sentar precisa ser revista. A próxima revisão deve preservar o sentido correto do NPC e os lados do banco; esse ajuste visual foi adiado. O import do clip `Sitting_Idle_Loop` está com `loop=False`, apesar do nome; o hold observado permanece sentado, mas repetição cíclica ainda não foi comprovada.

## Demo de três residentes

1. Execute `Freyja > NPC Animation 091 > Create Three Resident Demo Scene` e abra `Assets/_Project/Scenes/NpcAnimation_Demo.unity`.
2. Entre em Play Mode. A cena contém três moradores reais, NavMesh, walker, o controller/perfil compartilhado e o Smart Object de banco com duas vagas.
3. Use os botões na Game View: `Walk`, `Run`, `Start sit lifecycle`, `Schedule end`, `Cancel all`, `Retry cleanup`, `Capture locomotion evidence` e `Capture activity evidence`. A terceira tentativa no banco deve receber `target_unavailable` enquanto as duas vagas estão ocupadas.
4. Para capturar corrida, inicie `Run`, espere o overlay mostrar velocidade acima de `2,5 m/s` e então clique em `Capture locomotion evidence`. A captura de locomoção atual incluída no repositório mostra caminhada a `1,36 m/s`; o Run foi observado a `2,85–2,92 m/s` em Play Mode.

## Ordem e recuperação do banco

O Activity Core reserva um handle exato, navega até `Approach_Sit_Left/Right`, alinha, ocupa a vaga, toca `sit_down`, mantém o estado de sentado durante o `wait`, toca `stand_up`, navega de volta à aproximação e libera o mesmo handle. Em cancelamento durante o hold, o Core conserva a claim até concluir a recuperação e libera apenas a reserva daquela execução. O demo é temporário; não adiciona controller definitivo, integração global de rotinas ou atividades profissionais.

## Evidência e testes

- Game View com os dois residentes sentados e o terceiro rejeitado: [`evidence/playmode-activity.png`](evidence/playmode-activity.png).
- Game View de locomoção com caminhada efetiva e terceira tentativa rejeitada: [`evidence/playmode-locomotion.png`](evidence/playmode-locomotion.png).
- Resultados Unity Test Runner: [`evidence/editmode-results.xml`](evidence/editmode-results.xml) e [`evidence/playmode-results.xml`](evidence/playmode-results.xml).
- Matriz de cobertura e limitações: [`validacao.md`](validacao.md).
