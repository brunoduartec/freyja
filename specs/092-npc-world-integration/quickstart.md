# Quickstart: NPC World Integration

## Cena e objetos confirmados

Abra `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` no Unity 6000.3.25f1. A integração usa a cena existente e estes objetos:

| Hierarchy path | Componente(s) e campos relevantes |
|---|---|
| `/Coastal Sun` | `CoastalDayNight`: `hour`, `cycleSeconds` (96 s por 24 h), `advanceClock`. A cena está serializada em 10:30, com avanço pausado. |
| `/Waypoints da ilha/NPC World Integration` | `NpcWorldRuntimeDriver`: `clock`, `hosts`, `weather`, `runtimeStatus`; `NpcLocationRegistry`: `waypointNetwork`, `points`; `NpcScheduleTextAssetProvider`: `activityCatalog`, `templates`, `npcSchedules`, `overrides`, `activityRecipes`. |
| `/Island023/Moradores060/Ivo Batista` | `NpcActivityController.npcId=world_ivo`; `dataProvider`, `locations`, `activityPorts`, `animatorBindings`; `NpcUnityActivityPorts.idleDestinationPointId=world_ivo_home`. |
| `/Island023/Moradores060/Helena Batista` | `NpcActivityController.npcId=world_helena`; ponto de casa `world_helena_home`. |
| `/Island023/Moradores060/Luana Batista` | `NpcActivityController.npcId=world_luana`; ponto de casa `world_luana_home`. |
| `/NPC Integration Bench — West` e `/NPC Integration Bench — East` | `SmartObject` com `objectId=npc_world_bench_west/east`, `regionId=village_square`, slots `sit_left` e `sit_right`. Os anchors são filhos `Approach_Sit_*` e `Pose_Sit_*`. |
| `/Cais Session` | `GameplayDebugPanel`, conectado ao relógio e aos botões/slider existentes. |

O driver e o registry compartilham o objeto `/Waypoints da ilha/NPC World Integration`. O provider aponta para `Assets/_Project/NpcScheduling/Data/activities.json`, `Assets/_Project/NpcScheduling/Data/execution/recipes.json` e os três JSONs abaixo. Cada morador mantém os componentes existentes `IslandWaypointWalker`, `SmartObjectReservationOwner`, `NpcUnityActivityPorts` e `NpcAnimatorBindings`; não adicione outro controlador de navegação.

Pontos de rotina confirmados: `square_largo` (`regionId=village_square`), `beach_east_path` (`regionId=beach`) e os três pontos `world_*_home` (`regionId=home`). `square_venda` e `beach_east_access` estão marcados `unused_access`; não os use como destinos antes de validar o caminho NavMesh.

## Play Mode

1. Aguarde compilação sem erros e inicie Play Mode. O driver projeta o valor atual de `CoastalDayNight` e resolve somente a atividade vigente; a hora serializada inicial é 10:30.
2. **A cena inicia com o relógio pausado (`advanceClock=false`).** Para ver os horários avançarem, abra o painel de debug e pressione `Ciclo automático pausado` para ligá-lo. O ciclo existente leva 96 segundos para 24 horas de jogo, aproximadamente 4 segundos por hora, a `Time.timeScale=1`.
3. O código de `/Cais Session` alterna o painel com F3. Os botões `Meio-dia`, `Meia-noite`, `Ciclo automático pausado/ligado` e o slider são os controles existentes. Os eventos dos botões/slider foram validados no Editor; a abertura por F3 não foi confirmada nesta sessão de MCP.
4. Veja o personagem no Game View e consulte `NPC_ACTIVITY` no Console. O log informa NPC, execution ID, destino, step e resultado; confirme também a posição, animação e `ReservationCount`. Um intent ou log de início sozinho não prova chegada.
5. Para validar um dia, deixe o ciclo automático ligado por pelo menos 96 s e observe praça, bancos, praia e retorno aos pontos pessoais. As três rotinas se sobrepõem em `11:00–13:00` e competem por quatro slots.

## Editar uma rotina sem C#

Rotinas atuais:

- `Assets/_Project/NpcScheduling/Data/npcs/world_integration/world_ivo.json`
- `Assets/_Project/NpcScheduling/Data/npcs/world_integration/world_helena.json`
- `Assets/_Project/NpcScheduling/Data/npcs/world_integration/world_luana.json`

Edite `schemaVersion`, `npcId` e `entries` conforme `Assets/_Project/NpcScheduling/Data/schema/v1.json`. Cada entrada tem `id`, `start`, `end`, `activityId` e, quando a atividade tem destino, `targetId`. IDs seguem `^[a-z][a-z0-9_]*$`; horários usam `HH:mm`, com `24:00` permitido somente como fim. Entradas de um mesmo NPC não devem se sobrepor. Nesta entrega use somente `idle`, `wander` e `sit`:

- `idle`: sem `targetId`; o controller usa o ponto configurado em `NpcUnityActivityPorts.idleDestinationPointId`.
- `wander`: `targetId` é o `regionId` de waypoints acessíveis, atualmente `village_square` ou `beach`.
- `sit`: `targetId` é a região dos Smart Objects, atualmente `village_square`.

Exemplo de rotina individual:

```json
{
  "schemaVersion": 1,
  "npcId": "world_ivo",
  "entries": [
    { "id": "home_morning", "start": "00:00", "end": "07:00", "activityId": "idle" },
    { "id": "square_morning", "start": "07:00", "end": "11:00", "activityId": "wander", "targetId": "village_square" },
    { "id": "square_sit", "start": "11:00", "end": "13:00", "activityId": "sit", "targetId": "village_square" },
    { "id": "beach_afternoon", "start": "13:00", "end": "18:00", "activityId": "wander", "targetId": "beach" },
    { "id": "home_evening", "start": "18:00", "end": "24:00", "activityId": "idle" }
  ]
}
```

Salve o JSON, aguarde reimport e reinicie Play Mode. `NpcScheduleTextAssetProvider.npcSchedules` precisa referenciar o TextAsset. Hot reload durante uma execução não foi implementado. A edição `beach` → `village_square` foi validada sem alteração de C#.

## Cadastrar outro NPC

1. Use um personagem existente e acessível na ilha; garanta `IslandWaypointWalker`, `SmartObjectReservationOwner` e Animator/`NpcAnimatorBindings` configurados.
2. No objeto do personagem, configure um `NpcActivityController` com ID único em `npcId` e referências `dataProvider`, `locations`, `activityPorts` e `animatorBindings`.
3. Em seu `NpcUnityActivityPorts`, defina `idleDestinationPointId` para seu ponto pessoal.
4. Crie um Transform filho de `/Waypoints da ilha` em lugar caminhável e registre em `NpcLocationRegistry.points` com `pointId` exclusivo, `regionId=home` e `point` apontando ao Transform. Não desative o destino.
5. Crie `world_<id>.json` com o mesmo `npcId` e entradas válidas; adicione o TextAsset em `NpcScheduleTextAssetProvider.npcSchedules`.
6. Adicione o `NpcActivityController` ao array `NpcWorldRuntimeDriver.hosts`. Confirme ID único, dependências válidas, `ready=true` e um único host por NPC no Play Mode.

## Limites verificados

- O dia/weekday é contado apenas na sessão; não há calendário civil nem persistência. Um retrocesso fora da meia-noite gera `clock_regressed` e pausa a projeção até o relógio avançar além do último valor válido ou Play Mode reiniciar.
- Se um waypoint de destino fica inativo, `wander` falha finitamente. O fallback atual `idle` herda o destino inválido e também pode falhar `destination_unavailable`; nenhum deles teleporta ou deixa reserva de banco.
- Desativar somente `NpcActivityController` durante o assento mantém o owner até o caminho de saída/cleanup; retry tardio pode emitir `animation_timeout` antes de liberar o claim.
- Use `dotnet run --project Tools/NpcScheduleSimulator -- --validate` para validar referências/esquema e `--test` para regressões lógicas; esses comandos não validam a navegação física. Veja os métodos e resultados observados em `validacao.md`.
