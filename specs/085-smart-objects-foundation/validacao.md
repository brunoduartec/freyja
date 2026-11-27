# Validação — Smart Objects e cadeira na praça

## Evidência do Editor

- Unity `6000.3.25f1`, projeto `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`, Editor Pipeline 7800.
- Cena isolada `Freyja/Assets/_Project/Scenes/CadeiraSmartObject_Demo.unity`; a cena principal não foi alterada por esta demonstração.
- Cadeira `Freyja/Assets/_Project/Prefabs/SmartObjects085/CadeiraProvisoria.prefab`, colocada no waypoint `Largo`, aproximadamente `(-2.00, 6.99, -3.00)`. Possui collider separado, visual de madeira provisório e slot `sit` habilitado.
- `Elisa Neves` inicia em `(-6.40, 6.94, -2.90)`. O caminho existente `IslandWaypointWalker` conduziu a moradora até `(-2.00, 6.99, -3.00)` antes da ocupação. Estado observado: harness `OccupiedAtPoseReference`, `SmartObjectSeatedPose.IsSeated = true`, slot `Occupied`.
- Após `ReleaseAll`, a pose voltou a `IsSeated = false`, slot `Available` e `ReservationCount = 0`.
- Captura Scene View: `specs/085-smart-objects-foundation/evidence/CadeiraSmartObject_Sentada.png`.
- Vídeo Play Mode: `Artifacts/videos/CadeiraSmartObject_Demo.mp4` — 6 s, 24 fps, 1280×720. O arquivo foi reaberto e decodificado; o frame aos 5,5 s mostra Elisa sentada na cadeira.
- Na cópia demo, o único componente `CoastalGrass` que produzia `NullReferenceException` recorrente foi desativado; a cena principal preserva sua configuração.

## Testes e compilação

- `unity command script recompile`: concluído; Editor ficou `compiling=false`, `domainReloadInProgress=false`, sem falha de compilação reportada.
- `unity command test run --mode editor --filter Freyja.SmartObjects --timeout 600`: **12/12 passaram**.
- `unity command test run --mode playmode --filter Freyja.SmartObjects --async-tests true --timeout 600`: **3/3 passaram** — disable/reactivation/destroy, limpeza por owner e reset do subsistema.
- Console após a execução dos testes: `compilationFailed=false`; execução Play Mode da demonstração também chegou ao estado ocupado e liberou a reserva conforme acima.

## Limites observados

- A pose sentada é aplicada por músculos Humanoid e aparece depois da chegada; não existe clip de transição de sentar nos assets de movimento residentes inspecionados.
- O material e a geometria da cadeira são provisórios e substituíveis. Poço/cais, scheduler e mecânicas completas de atividade não fazem parte deste objetivo.
