# Validação baseline e estado de sessão

- Projeto Unity: `Freyja/`, Unity `6000.3.25f1`, cena `Assets/_Project/Scenes/Ilha_Principal.unity`.
- Branch: `094-npc-debug-tools`, HEAD `90c6b27` no início da validação.
- Git já continha alterações/untracked da entrega `093-npc-integration-validation` e trabalho anterior. Foram preservados; a entrega 094 não foi comitada nem integrada isoladamente.
- A cena permaneceu `isDirty:false` durante os testes Play Mode. Alterações temporárias de disponibilidade do banco foram feitas somente durante Play Mode e revertidas ao sair.
- Console não foi limpo. Após compilar, o Console reportou zero erros/avisos novos de compilação; avisos do Unity AI Assistant/Account API apareceram durante a sessão. Na checagem final, o Editor respondeu `ready`, Edit Mode, sem compilação e `compilationFailed:false`; o Console MCP reportou 132 erros contabilizados e 25 avisos atuais, mas não foi possível ler suas mensagens porque o servidor Pipeline em `127.0.0.1:7800` estava indisponível. Não foi possível determinar se os erros são relacionados à sondagem sintética de overflow ou preexistentes/externos. Nenhuma afirmação de Console sem erros finais.
- `git diff --check` passou no encerramento da revisão local.
- Reteste após retomada: recompilação pós-correção passou. Build StandaloneOSX final passou com 0 erros e 0 avisos. Após os testes finais, Editor está `ready`, Play Mode parado, `Ilha_Principal` carregada e `isDirty:false`. Console MCP reportou zero erros/avisos correntes; seu buffer histórico mantém 288 erros/374 avisos contados, incluindo `SocketException: Invalid arguments` no `Unity.Pipeline.BasePipelineServer`. O Console não foi limpo manualmente.

Evidências Play Mode e JSON reais estão nesta pasta; ver `validation-matrix.md` e `../npc-debug-tools-report.md`.
