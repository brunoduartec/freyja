# Validação — NPC Schedule Core

Branch `086-npc-schedule-core`; checkout disponível em macOS arm64 com .NET SDK 11.0.100. Esta atualização acrescenta uma checagem no Unity Editor/Play Mode ao registro anterior. Não foram usados MCP ou Blender, e nenhum Smart Object foi criado, alterado ou integrado.

## Evidências executadas

- `dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj` — exit 0; 0 warnings, 0 errors.
- `dotnet run --project Tools/NpcScheduleSimulator -- --test` — exit 0; `PASS: 68 assertions`. Exercita fronteiras e horários inválidos, dia absoluto/semana, meia-noite e exceções em dias consecutivos, expansão isolada de templates, operações de edição, parsing/validação negativa, contexto, prioridades 50/70/90, empate ordinal, conflitos estáticos, fallback, retorno ao bloco atual, target lógico `object` e duas semanas idênticas (296 transições cada). Os três avisos no teste vêm do fixture temporário alterado para comprovar edição data-driven; a configuração original não tem lacunas.
- `dotnet run --project Tools/NpcScheduleSimulator -- --validate` — exit 0; `OK: 5 NPCs, 15 atividades, 3 overrides`, sem warnings na configuração original.
- `dotnet run --project Tools/NpcScheduleSimulator -- --query demo_fisherman 4 23:00 clear demo_secret_meeting_unlocked=true` — exit 0; retorna `secret_meeting`, prioridade 90, validade 1380–1425, destino lógico `hill_ruins`.
- A mesma consulta sem a flag — exit 0; retorna `sleep` até 1440. Isso demonstra ausência de correspondência para flag ausente.
- `dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week` — exit 0; 296 transições de cinco personagens em sete dias. Observadas chuva Day 2 07:00, encontro técnico Day 4 23:00 e retorno à rotina às 23:45, domingo fixture Day 7 08:00.
- `unity status --project-path "$PWD/Freyja" --format json` — Editor 6000.3.25f1 ativo e pronto no projeto `Freyja/`.
- `unity command console status` — `compiling=false`, `compilationFailed=false`. O Editor concluiu a compilação/importação sem falha reportada para o estado atual do projeto; não é um build do jogo.
- Play Mode foi iniciado e encerrado pelo Unity CLI; captura `Game View` em 1280×720 inspecionada. Ela mostra a cena/HUD de demonstração Smart Objects que já estava carregada, com texto sobreposto. Não representa nem valida visualmente as agendas: esta feature é lógica e não cria renderers, NPCs ou UI.
- A checagem do Console após o cursor inicial não apontou falha de compilação/runtime atribuível ao Core. Um erro registrado durante a inspeção veio do comando inválido `editor.playmode status` (o CLI não oferece esse subcomando); o Play Mode foi encerrado normalmente. Contagens históricas do Console não foram tratadas como erros novos desta feature.
- `python3 -m json.tool Freyja/Assets/_Project/NpcScheduling/Data/schema/v1.json` — exit 0, JSON Schema sintaticamente válido.
- `git diff --check` — exit 0, sem diagnóstico. Busca direcionada no Core e harness por `UnityEngine`, `UnityEditor`, `MonoBehaviour`, `SmartObjects`, `DateTime.Now` e `Random` não encontrou referências.

## Limites e pendências

Os 68 assertions exercitam o parser e loader reais, não mocks. O loader usa `StrictJson`, parser pequeno em C# puro compartilhado no Core e sem I/O; JSON Schema 2020-12 descreve os quatro documentos, e o loader faz validação semântica cruzada. A importação e ausência de falha de compilação foram observadas no Editor, mas não foi executado um build standalone do jogo. A validação física de destinos continua pendente.

Unity gerou arquivos `.meta` para os novos assets ao importar. Ainda é necessário validar destinos lógicos contra o conteúdo real e qualquer integração física em uma entrega separada. A inspeção visual desta tarefa limita-se à cena que já estava aberta; não existe visualização de agenda a validar nesta feature. O commit desta entrega foi sincronizado à `main` local por cherry-pick; não houve push.
