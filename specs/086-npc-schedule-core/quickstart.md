# Quickstart

Requisitos: .NET SDK 11 (target `net11.0`), sem Unity.

Na raiz do checkout:

```sh
dotnet run --project Tools/NpcScheduleSimulator -- --validate
dotnet run --project Tools/NpcScheduleSimulator -- --test
dotnet run --project Tools/NpcScheduleSimulator -- --query demo_fisherman 1 07:00 --weather clear
dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week
```

A semana percorre sete dias absolutos de fixture. As transições são horários planejados pelo resolver, não registro de presença ou evidência narrativa. JSON e regras de autoria estão em `Freyja/Assets/_Project/NpcScheduling/Data/`; veja o README junto aos arquivos para cadastrar IDs, editar horários e regras. Configuração inválida retorna exit code diferente de zero.
