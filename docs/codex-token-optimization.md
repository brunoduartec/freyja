# Otimização do Codex no Freyja

Auditoria e configuração de 25/11/2026 na branch `075-otimizacao-codex`, baseada no estado publicado `26ac056`. A [spec aprovada](../specs/075-otimizacao-codex/spec.md) define o escopo. Abra uma nova sessão Codex na raiz do Freyja com esta branch selecionada para usar o perfil.

## Inventário e decisões

| Componente | Versão e decisão |
| --- | --- |
| Codex CLI | `0.169.0`. O projeto é confiável. O app-server reconheceu a camada do Freyja. O modelo e o esforço padrão permanecem herdados do usuário. |
| Spec Kit | `1.0.1`, dez skills existentes. O fluxo de aprovação, branch, plan, tasks e implementação foi usado nesta entrega. O ponteiro local `.specify/feature.json` seleciona a spec 075. |
| Unity | Editor `6000.3.25f1`, Built-in, Pipeline `1.0.0` e Unity CLI `1.0.1`. O servidor global é o da Unity CLI. No Freyja, a mesma integração expõe comandos agrupados. |
| Plugin Unity | `unity@unity-agent-plugin` `0.1.10-beta`, instalado pela fonte oficial. A skill `unity:using-unity` aparece no contexto da CLI. O manifesto traz uma entrada de skills, sem um segundo MCP Unity. A instalação e o marketplace alteram o perfil de usuário do Codex. |
| Blender | `5.2.2 LTS`, `blender-mcp 1.0.2`, bridge local na porta `9876`. O MCP global continua ativo. Consulte [Conexões MCP](../Tools/MCP/README.md). |
| Serena | MCP global via `uvx`, pacote ativo `serena-agent 2.1.0` e servidor C# ativo neste projeto. Uma consulta de símbolos em `MooredBoat.cs` identificou o namespace `Freyja.Environment`. |
| RTK | `0.44.0`, uso explícito por comando e sem hook detectado. Na auditoria, o RTK reportou 1,8% de economia global estimada de saída. |
| Headroom | `0.36.4`, registrado via stdio. A auditoria não encontrou proxy nas portas padrão nem variáveis de proxy no processo. O perfil local desabilita o MCP. |
| Navegador e docs | Playwright MCP `0.0.86` e OpenAI Developer Docs ficam disponíveis por opt-in. `cua_repl` e `node_repl` continuam habilitados. Tripo CLI `0.7.2` funciona diretamente. Os registros já desabilitados `codex_app` e `computer-use` foram preservados. |

Os guias oficiais complementam as regras locais de cenas, GUIDs, escala e aceite. O Spec Kit mantém a orquestração. BMGD sobrepõe fluxos de design e produção, e GSD acrescenta orquestração e estado de planejamento. Codex GameDev Kit e Portable GameDev AI Agents acrescentariam outros papéis. Unity Expert Skills segue como referência para lacunas específicas. A Unity CLI atual oferece agrupamento e filtros nativos. Não houve evidência de compatibilidade do `unity-mcp-efficient` com este servidor. [Unity para Codex](https://docs.unity.com/en-us/ai/unity-plugin/codex) · [BMGD](https://github.com/bmad-code-org/bmad-module-game-dev-studio) · [GSD](https://github.com/gsd-build/get-shit-done)

A instalação intermediária do mesmo plugin pelo catálogo remoto aparecia habilitada na listagem, mas a skill não aparecia no contexto renderizado pela CLI. Essa instalação foi removida. Instalar pelo marketplace oficial da Unity resolveu a descoberta e evitou duas cópias.

## Operação

- **C# e cenas** — Use `freyja-unity-edit`. Consulte só os scripts, componentes e testes do fluxo afetado. Use Serena para navegação por símbolos e Unity MCP para operações do Editor. Para consultas determinísticas, use `unity status` ou `unity command --query <termo>`, com `--project-path "$PWD/Freyja"` na raiz. Consulte compilação e Console após mudanças de código. Agrupe mudanças antes de testar cenas e interações em Play Mode.
- **Blender e importação** — Use `freyja-blender-export`. Confirme o arquivo e a cena antes de alterar. Agrupe operações repetitivas, retorne só os dados relevantes e preserve a fonte editável. Valide o export no Unity e use a câmera real quando a aparência mudar.
- **Logs** — Use RTK para comandos suportados. Use `rtk proxy` quando o diagnóstico pedir conteúdo exato. Traga o resumo, os erros pertinentes e o caminho do log. Preserve o original para inspeção.
- **Delegação** — Use `implementer` em implementação delimitada, `validator` quando a revisão independente for útil e `game-designer` para decisões de gameplay, UX e balanceamento. O limite é um agente aberto e um nível de profundidade. Passe objetivo, arquivos e critérios em poucas linhas e encerre o agente ao receber a resposta. Inicie sem herdar histórico quando a ferramenta oferecer a opção. O Validador trabalha em modo somente leitura por instrução e pode executar testes focados. O sandbox técnico é herdado da sessão principal. Em tarefas pequenas, a sessão principal implementa e verifica.

### Opt-in para uma sessão de CLI

Execute na raiz do repositório. Para uma nova sessão no app, altere temporariamente a mesma chave em `.codex/config.toml` e restaure seu valor ao terminar.

```sh
codex -c 'mcp_servers.playwright.enabled=true'
codex -c 'mcp_servers.openaiDeveloperDocs.enabled=true'
codex -c 'mcp_servers.headroom.enabled=true'
codex -c 'mcp_servers.unity.args=["mcp"]'
```

O último comando usa o catálogo Unity completo. Use-o quando a descrição e o esquema individual de um comando forem mais úteis. Os MCPs opcionais podem ter seu catálogo consultado ou o processo adiado conforme o cache e a versão do Codex. A ativação de Headroom por esta chave apenas disponibiliza o servidor MCP. [Configuração por projeto](https://developers.openai.com/codex/config-basic) · [MCP](https://developers.openai.com/codex/mcp) · [Subagentes](https://developers.openai.com/codex/subagents)

## Validações e medidas

| Verificação | Resultado |
| --- | --- |
| Configuração efetiva | `codex mcp get --json` confirmou Unity agrupado, os três opcionais desabilitados com opt-in válido e os servidores preservados. `config/read` do app-server confirmou o limite de um agente e profundidade de um nível. |
| Skills e plugin | O validador local aprovou ambas as skills. `skills/list` descobriu as duas e as dez do Spec Kit. `plugin/read` confirmou a skill oficial e `codex debug prompt-input` mostrou as três skills Unity/Blender aplicáveis no contexto. |
| Agentes personalizados | A sessão efêmera em modo somente leitura acionou `implementer`, `validator` e `game-designer` pelos tipos exatos, com uma espera entre invocações. O rastreamento confirmou `fork_turns = "none"` para os três e os marcadores `I_OK`, `V_OK` e `D_OK`. O teste inicial do app-server com herança de contexto esbarrou na ausência de histórico persistido da sessão efêmera. O teste com instruções curtas resolveu o problema. |
| Unity | O editor reportou a raiz correta e a cena `Ilha_Tripo_Caminhada_011` ativa, carregada e sem alterações pendentes. Os grupos `scene` e `console` responderam. O Console indicou 0 erros, 1 aviso e compilação ociosa, sem falha. Foram verificados comandos de cenas, GameObjects, componentes, prefabs, assets, scripts, testes, Play Mode, Console, captura e pacotes. |
| Blender | O MCP global e o cliente stdio leram `Source/Blender/Cais_Iteracao01.blend`, cena `Cais_Iteracao01`, sem alterações pendentes. Catálogo com 26 ferramentas. |
| Playwright e Tripo | Playwright navegou a uma página de teste em um navegador headless isolado. O override de reativação foi aceito. `tripo --json --no-open whoami` confirmou a integração pela CLI. |
| Integridade | Git e hashes dos arquivos registraram a preservação da cena principal, fonte Blender, pacotes Unity, anexo e spec local 074. A entrega de configuração não alterou gameplay, assets ou dados de produção. |

A coleta do catálogo Unity usou o mesmo editor aberto e registrou o tamanho dos esquemas JSON serializados:

| Exposição | Ferramentas | Bytes |
| --- | ---: | ---: |
| Completa | 187 | 149.609 |
| Filtrada, retirando `package_*` | 181 | 144.785 |
| Agrupada, mantendo os domínios | 42 | 59.205 |

O agrupamento reduziu 60,4% do tamanho serializado do catálogo nessa coleta. A auditoria inicial encontrou respostas individuais de até 142 mil caracteres em três amostras parciais de sessões. O `AGENTS.md` caiu de 10.157 para 3.838 bytes. Em uma coleta de diagnóstico com a mesma frase de teste, o JSON renderizado de entrada caiu de 98,5 kB para cerca de 95,8 kB, com a skill oficial presente no segundo cenário. Tamanho serializado descreve o material disponível. O efeito sobre tokens faturados, cota e retrabalho permanece sem medição comparável.

Para medir na próxima entrega, use uma tarefa C# isolada, uma de gameplay e uma de Blender → Unity quando cada tipo voltar a aparecer. Registre modelo, esforço, estado dos editores, critérios de aceite, chamadas de ferramentas e agentes, tokens de entrada, entrada em cache, saída, duração e retrabalho. Use o mesmo estado do projeto e os mesmos critérios ao comparar com a base. Anote quando o produto só mostrar cota agregada. Registre compilação, Play Mode e evidência visual aplicáveis junto do custo. A baseline de consumo por tarefa e benchmarks de frameworks externos ficam pendentes.

## Backups e reversão

A cópia privada local está em `.git/freyja-codex-075.HoIKUY/`. Ela inclui o `AGENTS.md` original, o ponteiro 074, o TOML de usuário anterior, hashes e logs das sondagens. A pasta está fora dos arquivos versionados.

- **Branch e Spec Kit** — Confirme que as suas mudanças locais estão preservadas. Use `git switch 074-guia-unity` para voltar ao trabalho anterior. Na raiz, recupere o ponteiro 074 com `cp .git/freyja-codex-075.HoIKUY/feature.json .specify/feature.json`. Ao retornar à 075, o gerador da spec ou um ponteiro com `{"feature_directory":"specs/075-otimizacao-codex"}` recupera o contexto ativo.
- **Reverter a entrega versionada** — Identifique o commit desta configuração com `git log --oneline --grep="Configura o Codex para o Freyja" 075-otimizacao-codex` e use `git revert <commit>` na branch em que quiser retirar a mudança. Isso reverte perfil, agentes, skills, guia e documentos da entrega. O ponteiro local é independente do Git.
- **Instalação no perfil de usuário** — Remova apenas a instalação desta entrega com `codex plugin remove unity@unity-agent-plugin` e depois `codex plugin marketplace remove unity-agent-plugin`. Os dois comandos removem as entradas adicionadas pela instalação e preservam outras mudanças no perfil.
