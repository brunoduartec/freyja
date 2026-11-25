# Orientações para agentes — Freyja

Freyja é um jogo 3D de investigação narrativa, low poly com pixels visíveis nas silhuetas. Consulte os trechos pertinentes de `GDD.md`, `.specify/memory/constitution.md` e da spec ativa. Distinga fatos observados, depoimentos, hipóteses e propostas. Registre divergências e preserve as decisões aprovadas.

## Entregas e Spec Kit

- Trabalhe em iterações curtas e avaliáveis. Use `speckit-specify` para cada entrega, apresente uma spec curta e aguarde aprovação explícita antes de planejar, gerar tarefas ou implementar. “Pode seguir”, após avaliar a entrega anterior, autoriza preparar a próxima spec. Execute `speckit-plan`, `speckit-tasks` e `speckit-implement` após a aprovação. Ao concluir a entrega, aguarde nova avaliação.
- Verifique Git e branches locais/remotas antes de uma nova spec. Prefira `main` atualizada quando for possível preservar o trabalho em andamento. Rode `.specify/scripts/bash/create-new-feature.sh --dry-run --json --short-name <nome-curto> "<descrição>"`, crie ou selecione a branch sequencial retornada e repita o comando sem `--dry-run`. Confirme a correspondência da branch, da pasta em `specs/` e de `.specify/feature.json`. Retome specs existentes na branch correspondente. Preserve o histórico e as mudanças de outras entregas.
- Leia `spec.md`, `plan.md` e `tasks.md` aplicáveis. Atualize tarefas concluídas com evidência e indique arquivos, verificações e limitações ao entregar. Salve as entregas na raiz deste repositório.

## Unity e Blender

- A raiz Unity é `Freyja/` e a cena principal é `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. Crie outra cena somente por pedido ou necessidade técnica concreta. Respeite `ProjectVersion.txt` (6000.3.25f1), Built-in, manifesto e lock de pacotes. Atualizações de dependências exigem escopo aprovado.
- Use os MCPs globais do Unity e Blender quando disponíveis. Para ativar a integração ou confirmar projeto e cena, consulte `Tools/MCP/README.md`. Se o MCP falhar, indique a limitação e prossiga por CLI ou arquivos no escopo possível. Reserve a afirmação de validação no Editor para checagens executadas nele.
- Mantenha conteúdo próprio em `Assets/_Project/`, conteúdo de terceiros separado e fontes editáveis em `Source/Blender/`. Versione assets com seus `.meta`, `Packages/` e `ProjectSettings/`, preservando GUIDs, serialização de texto e metadados visíveis. Mantenha caches, logs, builds e arquivos gerados de IDE fora do Git.
- Use `freyja-unity-edit` ao editar C#, cenas ou prefabs e `freyja-blender-export` ao preparar exports. Mudanças de código exigem checar compilação e Console. Mudanças de cena ou interação exigem Play Mode. Use a câmera real quando a mudança afetar a aparência. Planeje verificações adicionais conforme o risco.

## Contexto e delegação

- Responda em português. Use buscas e intervalos focados, o resultado necessário de cada ferramenta e o log original quando for preciso diagnosticar. Para comandos shell, leia `~/.codex/RTK.md` uma vez por sessão. Use RTK para saídas volumosas e `rtk proxy` ou o comando nativo quando precisar do conteúdo exato.
- Resolva tarefas pequenas na sessão principal. Use `implementer` para implementação delimitada, `validator` para verificação independente e `game-designer` para regras, progressão, UX ou balanceamento. Delegue um papel por vez com objetivo, arquivos relevantes, aceite e pendências. Use a opção de iniciar sem herdar o histórico quando disponível. Feche a delegação ao receber a resposta.
- Agrupe consultas ou mudanças independentes, verifique apenas o fluxo afetado e investigue a causa antes de tentar novamente. Consulte `docs/codex-token-optimization.md` para opções de ferramentas, verificação e reversão.
