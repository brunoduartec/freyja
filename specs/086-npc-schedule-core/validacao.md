# Validação — NPC Schedule Core

Branch `086-npc-schedule-core`; checkout disponível em macOS arm64 com .NET SDK 11.0.100. Não foram usados Unity/Editor, MCP, Blender, Play Mode ou Smart Objects. O Core compilou no harness .NET, mas sua compilação no Unity ainda não foi testada.

## Evidências executadas

- `dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj` — exit 0; 0 warnings, 0 errors.
- `dotnet run --project Tools/NpcScheduleSimulator -- --test` — exit 0; `PASS: 68 assertions`. Exercita fronteiras e horários inválidos, dia absoluto/semana, meia-noite e exceções em dias consecutivos, expansão isolada de templates, operações de edição, parsing/validação negativa, contexto, prioridades 50/70/90, empate ordinal, conflitos estáticos, fallback, retorno ao bloco atual, target lógico `object` e duas semanas idênticas (296 transições cada). Os três avisos no teste vêm do fixture temporário alterado para comprovar edição data-driven; a configuração original não tem lacunas.
- `dotnet run --project Tools/NpcScheduleSimulator -- --validate` — exit 0; `OK: 5 NPCs, 15 atividades, 3 overrides`, sem warnings na configuração original.
- `dotnet run --project Tools/NpcScheduleSimulator -- --query demo_fisherman 4 23:00 clear demo_secret_meeting_unlocked=true` — exit 0; retorna `secret_meeting`, prioridade 90, validade 1380–1425, destino lógico `hill_ruins`.
- A mesma consulta sem a flag — exit 0; retorna `sleep` até 1440. Isso demonstra ausência de correspondência para flag ausente.
- `dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week` — exit 0; 296 transições de cinco personagens em sete dias. Observadas chuva Day 2 07:00, encontro técnico Day 4 23:00 e retorno à rotina às 23:45, domingo fixture Day 7 08:00.
- `python3 -m json.tool Freyja/Assets/_Project/NpcScheduling/Data/schema/v1.json` — exit 0, JSON Schema sintaticamente válido.
- `git diff --check` — exit 0, sem diagnóstico. Busca direcionada no Core e harness por `UnityEngine`, `UnityEditor`, `MonoBehaviour`, `SmartObjects`, `DateTime.Now` e `Random` não encontrou referências.

## Limites e pendências

Os 65 assertions exercitam o parser e loader reais, não mocks. O loader usa `StrictJson`, parser pequeno em C# puro compartilhado no Core e sem I/O; JSON Schema 2020-12 descreve os quatro documentos, e o loader faz validação semântica cruzada. A validação física de destinos e a compilação no perfil Unity continuam pendentes.

As pastas sob `Assets` ainda não têm `.meta`: não inventei GUIDs. Ao retornar ao Mac, importar no Editor para gerar os metadados e confirmar compilação. Também conferir as alterações/branch pré-existentes de Smart Objects, mapear destinos lógicos e só planejar executor físico em uma entrega separada. Não há integração ao `main` ou push nesta branch.
