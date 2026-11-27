# Quickstart — Code Validation

Run from repository root:

```sh
dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj
dotnet run --project Tools/NpcScheduleSimulator -- --test
dotnet run --project Tools/NpcScheduleSimulator -- --validate
dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week > /tmp/freyja-npc-week.log
tail -n 1 /tmp/freyja-npc-week.log
```

Expected: the existing assertion suite passes; `--validate` accepts the existing schedule and three recipes; the week simulation has deterministic output and zero residual simulated claims. The harness is not Unity compilation, NavMesh, Smart Object, or Animator validation.

## Unity integration later

Use the checklist in `integration-checklist.md`. Do not create or attach a host to an official NPC. First configure a disposable test NPC only after the project clock/context provider, data TextAssets, location/region points, seat slots and Animator binding are confirmed. Run Edit Mode tests and then the full Play Mode failure matrix; capture Console/test evidence and update `validation.md`.
