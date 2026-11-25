# Prompt para o Codex — Auditoria e otimização de tokens do Freya

> **Como usar:** abra o repositório Freya no Codex quando sua cota estiver disponível. Primeiro utilize o modo de planejamento e cole o conteúdo da seção **“Prompt para executar”**. Revise o plano antes de permitir alterações. A auditoria e a execução pelo agente **também consomem tokens**; o objetivo é reduzir o custo das sessões futuras.
>
> **Prioridade:** eficiência real e mensurável, sem quebrar o ambiente de desenvolvimento.

---

## Contexto conhecido antes da execução

Conforme a saída de `codex mcp list` já inspecionada:

| Servidor | Configuração observada | Situação observada |
|---|---|---|
| `unity` | `~/.unity/bin/unity mcp` | Habilitado |
| `blender` | `blender-mcp` | Habilitado |
| `headroom` | `~/.local/bin/headroom mcp serve` | Habilitado |
| `serena` | `uvx --from serena-agent serena start-mcp-server --project-from-cwd --context codex --open-web-dashboard False` | Habilitado |
| `playwright` | `npx @playwright/mcp@latest` | Habilitado |
| `node_repl` e `cua_repl` | Ferramentas de automação/REPL | Habilitadas |
| `codex_app` e `computer-use` | Integrações do aplicativo | Desabilitadas |
| `openaiDeveloperDocs` | `https://developers.openai.com/mcp` | Habilitado |

- O servidor Unity usa **Unity CLI** (`unity mcp`), não há confirmação de que seja o MCP da CoplayDev. **Não presumir compatibilidade com `unity-mcp-efficient`.**
- O Blender e o Unity às vezes abrem e fecham durante o trabalho do Codex; isso, por si só, não prova uso ou ausência de MCP, nem consumo excessivo.
- Headroom está registrado como um servidor MCP. Isso **não significa compressão automática** das chamadas ao modelo.
- O RTK (Rust Token Killer) já foi testado, mas o mecanismo de integração e os ganhos efetivos precisam ser verificados.
- MCP habilitado **não significa necessariamente** que todas as definições de suas ferramentas são incluídas em todas as requisições; verificar se há carregamento sob demanda.
- `Unsupported` na coluna `Auth` da listagem **não** significa necessariamente falha do MCP.

---

# Prompt para executar

Você é um engenheiro sênior especializado em Codex, Unity, Blender, Model Context Protocol (MCP) e engenharia de agentes. Sua tarefa é **auditar, otimizar, testar e documentar** o ambiente de desenvolvimento do projeto **Freya**, visando **reduzir o consumo total de tokens e de cota do Codex em tarefas futuras**, sem degradar capacidade funcional, qualidade do jogo, confiabilidade ou qualidade das verificações visuais.

Trabalhe de forma deliberadamente econômica: em cada etapa, faça somente as consultas e testes necessários; não carregue o repositório inteiro em contexto; não crie subagentes para tarefas que podem ser resolvidas diretamente; e não repita varreduras sem motivo.

## Procedimento inicial obrigatório

1. **Comece em modo de planejamento**: entregue um plano resumido com diagnóstico pretendido, arquivos que pretende inspecionar, mudanças candidatas, riscos e critérios de sucesso. **Não altere arquivos nesta fase.**
2. Na fase de implementação, **faça inventário e backups antes de modificar qualquer coisa**. Identifique a raiz do projeto e se há alterações não commitadas; não sobrescreva trabalho existente.
3. Faça mudanças **pequenas, reversíveis e preferencialmente específicas do Freya**. Não altere configurações globais caso exista uma alternativa local comprovadamente válida.
4. Confirme na instalação local os comandos, opções e esquemas de configuração, especialmente porque as versões mudam. **Não assuma que exemplos de documentação de outras versões são compatíveis.**
5. Não instale, desinstale nem atualize ferramentas automaticamente se isso não for necessário. Não habilite proxies que interceptem tráfego ou credenciais sem aprovação explícita.
6. Preserve os mecanismos de desenvolvimento, exportação, teste e validação visual existentes. Não modifique gameplay, cenas, meshes, assets ou materiais para testar configuração, a menos que isso seja estritamente necessário e possa ser feito em uma cópia temporária.
7. Não exponha segredos, tokens, variáveis de autenticação ou dados de usuário nos relatórios. Mascare valores sensíveis.
8. Registre **o que foi confirmado, o que é hipótese e o que não pôde ser medido**. Não prometa percentuais de economia sem dados comparáveis.

## Fase 1 — Inventário e custo do ambiente atual

- Verifique versões e instalação de Codex, Unity CLI, Blender MCP, Serena, Headroom e RTK.
- Inspecione, conforme necessário: `~/.codex/config.toml`, `.codex/config.toml` no Freya, arquivos `AGENTS.md`, configurações herdadas de plugins, registros MCP e hooks de shell/CLI.
- Utilize comandos locais que não invoquem o modelo quando possível, como `codex mcp list`, `codex mcp get <servidor>`, `unity mcp --help`, `rtk --version` e `rtk gain` **apenas quando disponíveis**.
- Valide a precedência das configurações, o mecanismo de projetos confiáveis e se configurações locais de MCP são aceitas pela versão instalada.
- Liste os servidores ativos e quais são efetivamente úteis no desenvolvimento do Freya. Distinga servidores globais, do projeto e adicionados por plugins.
- Avalie se o Codex usa descoberta/carregamento sob demanda das ferramentas; não contabilize como custo fixo ferramentas que não são incluídas no contexto.
- Procure evidências disponíveis, sem leituras massivas, de chamadas repetidas, logs extensos, retornos verbosos do MCP e múltiplas inicializações do Editor.
- Identifique se o problema principal é contexto de entrada, saídas de ferramentas, raciocínio, validações repetidas ou chamadas de agentes.

**Entrega parcial:** inventário conciso, pontos de desperdício confirmados e prioridades classificadas por impacto esperado, risco e esforço.

## Fase 2 — Otimização do Unity CLI e do Unity MCP

- Identifique o servidor `unity` real e use `~/.unity/bin/unity mcp --help` (ou caminho efetivamente instalado) para conferir suporte a opções como `--tool-groups`, `--tools`, `--exclude-tools` e `--exclude-tool-groups`. **Não invente nomes de grupos ou ferramentas.**
- Se essas opções existirem, teste e proponha um subconjunto de ferramentas/grupos adequado às tarefas do Freya, preservando edição de cenas, GameObjects, prefabs, scripts, assets e validação quando necessário.
- Compare manter o MCP completo, limitar ferramentas e utilizar ferramentas agrupadas. Não presuma que menos ferramentas visíveis sempre significam menos tokens de ponta a ponta.
- Avalie comandos diretos da Unity CLI (`unity command` e outros documentados pela versão instalada) para operações determinísticas em que eles sejam mais econômicos que interações múltiplas via MCP.
- Prefira interfaces estruturadas e comandos específicos a execução arbitrária de código; não usar `eval` sem necessidade clara.
- Identifique se abrir/fechar o Unity é exigido pela integração ou se há inicializações redundantes que podem ser reduzidas.
- Não substitua verificações indispensáveis do Editor por simples confirmação de compilação.
- **Não instale `unity-mcp-efficient` apenas porque ele existe**; primeiro comprove compatibilidade com o servidor atual. Se não houver compatibilidade, não o utilize.

## Fase 3 — Otimização do Blender

- Identifique a implementação de `blender-mcp` e as operações suportadas.
- Distinga necessidades de GUI, chamadas MCP, execução Python em modo batch/headless e CLI.
- Avalie agrupar operações repetitivas em scripts ou lotes quando isso reduzir chamadas e não prejudicar a inspeção do resultado.
- Evite retornar geometria, hierarquias inteiras, listas enormes de objetos, materiais ou transforms quando bastarem resumos e propriedades pontuais.
- Verifique se abertura/fechamento frequentes do Blender têm justificativa técnica; não altere o comportamento sem garantir funcionamento equivalente.
- Mantenha validação visual proporcional às mudanças efetivamente visuais.

## Fase 4 — Configuração MCP por projeto

- Verifique a forma correta de declarar e **desabilitar por projeto** servidores já existentes na versão instalada do Codex, incluindo o suporte a `enabled = false` e, se for o caso, listas de ferramentas habilitadas/desabilitadas.
- Caso seguro e suportado, crie ou ajuste `.codex/config.toml` no Freya, preservando outras configurações existentes.
- Considere a política inicial abaixo como **hipótese**, não como alteração cega:
  - `unity`: disponível e limitado às operações relevantes, conforme suporte real.
  - `blender`: disponível quando o trabalho envolver assets/visual.
  - `serena`: disponível se melhorar navegação e edição de código sem produzir retornos redundantes.
  - `headroom`: manter ativo apenas se houver benefício prático demonstrável na forma como está integrado.
  - `playwright`: desabilitado por padrão no Freya se não houver testes web/browser relevantes.
  - `openaiDeveloperDocs`: desabilitado por padrão se a documentação não for necessária nas tarefas típicas do jogo.
  - `node_repl` e `cua_repl`: reavaliar necessidade; **não desligar automaticamente** recursos que sejam empregados por UI testing, automações ou plugins.
  - Outros: avaliar isoladamente e preservar funções necessárias.
- Não exponha duas integrações equivalentes para o mesmo propósito sem justificativa; evite duplicidade entre MCP, CLI e automação visual.
- Forneça um modo simples e documentado de reabilitar temporariamente ferramentas desligadas.
- Confirme que as alterações são realmente aplicadas pelo Codex e não apenas sintaticamente válidas.

## Fase 5 — Headroom e RTK

**Headroom**
- Verifique se está funcionando somente como MCP, se há proxy adicional e como a compressão é acionada.
- Não considere `headroom mcp serve` sinônimo de compressão automática.
- Avalie custo/benefício de compressões intencionais e se há risco de perder logs/erros úteis.
- Não implemente proxy de tráfego ou modifique autenticação sem aprovação específica.
- Se não trouxer ganho mensurável no Freya, proponha desabilitá-lo para este projeto, sem desinstalar.

**RTK**
- Confirme que é o RTK (Rust Token Killer) correto, a versão instalada, seus hooks e sua integração com o Codex.
- Execute `rtk gain` apenas se disponível, interprete a estatística de economia de saída com cautela e diferencie-a de economia real de cota.
- Avalie se os filtros estão cobrindo comandos frequentes do fluxo Unity/C# e Blender sem ocultar exceções importantes.
- Evite duas camadas de compressão redundantes ou que causem retrabalho do agente.

## Fase 5A — Pesquisa aplicada de frameworks, skills e múltiplos agentes para Unity

**Objetivo desta fase:** escolher uma arquitetura de trabalho para o Freya baseada em ferramentas existentes e padrões publicados, **com a menor sobrecarga possível**. Não instalar frameworks apenas por terem muitos agentes ou muitas skills.

### Candidatos identificados (pesquisa de 23/11/2026)

| Opção | Para que serve | Evidência pública | Decisão inicial para Freya |
|---|---|---|---|
| **Unity Agent Plugin oficial** — https://github.com/Unity-Technologies/unity-agent-plugin | Skills de Unity, integração com Unity CLI/Editor, orientações mantidas pela Unity, Unity 6+ | Oficial; o repositório `Unity-Technologies/skills` ultrapassava 1 mil estrelas na consulta | **Prioridade 1**: verificar se já está instalado, habilitado e sem skills duplicadas; adotar se compatível |
| **BMad Game Dev Studio (BMGD)** — https://github.com/bmad-code-org/bmad-module-game-dev-studio | Game Designer, desenvolvimento, arquitetura, GDD, prototipação e ciclo de produção para Unity/Unreal/Godot | Projeto mais estabelecido que os kits pequenos examinados; centenas de estrelas na consulta; documentação de módulos e agentes | **Piloto opcional** para design e planejamento, sem instalar toda a cadeia automaticamente |
| **GSD / Get Shit Done** — https://github.com/gsd-build/get-shit-done | Planejamento por fases, estado persistente, orquestração de subagentes e contexto isolado | Documenta roteadores de skills e redução estimada de metadados (~120 vs ~2.150 tokens para listagens comparadas pelo projeto), **não** prova redução do consumo total no Freya | **Piloto alternativo** se o gargalo forem sessões longas/contexto inflado; não combinar por padrão com BMGD e Spec Kit |
| **Codex GameDev Kit** — https://github.com/IlyasGameDev/codex-gamedev-kit | 49 papéis, 74 skills, templates e checkpoints | Repositório pequeno (poucas estrelas na consulta), sem benchmark independente de economia total | **Não instalar por padrão**; alto risco de excesso de roteamento e manutenção |
| **Portable GameDev AI Agents** — https://github.com/ilezhnin/gamedev-ai-agents | Fluxo de estúdio com designer, planner, implementadores, QA e gates no Unity | Código e documentação disponíveis; comunidade ainda pequena | Apenas referência para fluxos e contratos, sem instalação inicial |
| **Unity Expert Skills** — https://github.com/scenario-labs/skills/tree/main/skills/game-engines/unity | Skills específicas para gameplay, UI, shaders, otimização e pipeline em Unity 6.3 | Autor relata testes práticos; não há validação independente de economia | Consultar somente se a skill oficial não cobrir necessidade especializada e versão for compatível |

**Observação de evidência:** número de estrelas é indicador de interesse público, não de eficácia, adoção em produção ou economia de tokens. Relatos de Reddit são experiências individuais, não benchmarks controlados. Os projetos listados acima são **candidatos**, não dependências obrigatórias.

### O que a comunidade tem feito — padrões a avaliar

- **Skills específicas do projeto:** regras de edição de cenas, `.meta`/GUID, testes, importação de assets, critérios de aceitação e limites para screenshots. Preferir uma skill curta que impeça erros recorrentes a um catálogo genérico de dezenas de habilidades.
- **Subagentes de leitura/validação:** rodar testes de EditMode/PlayMode, filtrar logs, avaliar diffs, retornar somente falhas, evidências e arquivos afetados. Este padrão consta de recomendações de praticantes na comunidade da Unity.
- **Game designer sob demanda:** revisar regras, dificuldade, experiência do jogador e critérios de aceite *antes* da implementação quando a tarefa altera gameplay; não invocá-lo para correções técnicas triviais.
- **Handoffs compactos entre sessões:** objetivo, arquivos alterados, contratos, pendências, testes e próximos passos em pequenos arquivos versionados, em vez de reapresentar todo o projeto a cada conversa.
- **Trabalho paralelo só em áreas independentes:** cenas e assets compartilhados, ProjectSettings, prefabs e `.meta` criam risco de conflito. Se necessário, usar worktrees ou branches isolados, contrato comum de integração e um único ponto de merge.
- **Validação progressiva:** verificações locais baratas primeiro, testes focados depois, PlayMode/screenshots somente conforme risco de mudança. Encerrar depois de atingir critérios de aceite e limitar tentativas de correção sem nova evidência.

Fontes de experiência comunitária: https://discussions.unity.com/t/tips-and-tricks-for-using-claude-code-codex-with-the-unity-cli/1740182/1 ; https://www.reddit.com/r/ClaudeAI/comments/1xou972/which_claude_code_skills_are_actually_useful_for/ ; https://www.reddit.com/r/codex/comments/1xl8syv/how_are_you_coordinating_codex_worktrees_when/ .

### Arquitetura-alvo mínima — ponto de partida recomendado

1. **Base:** Unity Agent Plugin oficial (verificar `codex plugin list` e versão do Unity 6+), preservando a Unity CLI já instalada. A Unity recomenda o plugin em vez de instalar separadamente o pacote `Unity-Technologies/skills`, porque duplicaria as skills. Os comandos documentados atualmente são `codex plugin marketplace add Unity-Technologies/unity-agent-plugin` e `codex plugin add unity@unity-agent-plugin`; **confirmar compatibilidade local, pré-visualizar e obter autorização antes de instalar**.
2. **Orquestração existente:** reutilizar Spec Kit/OpenSpec ou o processo de especificação já presente no Freya, caso esteja funcionando. Não sobrepor BMGD, GSD e outro framework completo para a mesma tarefa.
3. **Agentes enxutos — criação obrigatória na implementação:** deixar **três papéis configurados e disponíveis permanentemente no projeto** — `implementer` (altera código/Unity conforme escopo), `validator` (somente leitura/testes/logs e relatório objetivo), `game-designer` (somente regras, balanceamento e aceitação). **Criar ou atualizar os três arquivos de definição**, sem duplicar agentes preexistentes; confirmar a sintaxe suportada pela versão do Codex instalada. O acionamento continua **sob demanda**: isso **não** significa executar três subagentes em toda tarefa.
4. **Tarefas pequenas:** por padrão, **um agente principal** implementa e executa verificações locais. Invocar `validator` separadamente só em mudanças relevantes ou quando precisa de revisão independente; invocar `game-designer` quando a mecânica, progressão, UX ou balanceamento muda.
5. **Skills específicas e progressivas:** preferir até **duas ou três skills próprias por projeto** *se houver demanda recorrente*, por exemplo: `freya-unity-safe-edit`, `freya-playtest-verify`, `freya-blender-import`. `SKILL.md` deve ter `name`/`description` curtos, com referências e scripts carregados só quando necessários. Evitar cópias dos mesmos guias de Unity cobertos pelo plugin oficial.
6. **Contrato curto:** guardar somente visão geral do Freya, convenções e caminhos para documentos especializados no `AGENTS.md`. Critérios específicos vivem em GDD, ADRs e arquivos de subsistema sob demanda.
7. **Sem paralelismo padrão:** limitar delegação concorrente e profundidade de agentes se a instalação oferecer esses controles. Não introduzir um 'supervisor de supervisores' nem vários revisores automáticos sem ganho demonstrado.

### Fase 5B — Instalar/configurar os três agentes do Freya (obrigatório na fase de implementação)

**Requisito explícito do usuário:** não basta recomendar os papéis. **Crie ou atualize os três agentes personalizados `implementer`, `validator` e `game-designer` para o Freya**, em arquivos versionáveis de configuração específicos do projeto, após confirmar a versão do Codex, a localização e o esquema de agentes customizados aceitos. A documentação de referência é https://developers.openai.com/codex/subagents ; em versões que a suportam, a localização pode ser `.codex/agents/*.toml`. Não inventar chaves de TOML nem assumir que a sintaxe continua idêntica; verificar localmente.

**Contratos mínimos exigidos:**

- **`implementer` — implementação**: C#, gameplay, integração Unity e Blender conforme escopo; ler apenas arquivos afetados; seguir regras de cenas, prefabs, `.meta` e GUID; produzir diff pequeno; executar testes focados necessários; não tocar em mecânicas fora dos critérios de aceite.
- **`validator` — validação independente**: começar em modo somente leitura, salvo autorização explícita para correções; revisar diffs, compilação, EditMode/PlayMode pertinentes e logs resumidos; reportar somente falhas, evidências e riscos; inspeção visual quando o resultado exigir; não reler o repositório inteiro.
- **`game-designer` — design e balanceamento**: produzir ou revisar regras, loop de jogo, dificuldade, progressão, UX e critérios de aceite; entregar especificações sucintas para o implementador; não editar código ou assets por padrão e não ser chamado para manutenção técnica simples.

**Orquestração e economia:** manter os três agentes **instalados e disponíveis** no projeto, mas **não os invocar automaticamente juntos**. O coordenador escolhe `implementer` para mudanças de código, `game-designer` apenas quando a tarefa envolver decisões de gameplay e `validator` apenas quando uma revisão separada trouxer benefício. Para tarefas pequenas, realizar a implementação e a validação objetiva na sessão principal, sem spawn extra. Não habilitar paralelismo padrão, nem agentes adicionais para supervisionar os três. Evitar cópia de contexto entre agentes; usar handoff conciso com objetivo, arquivos relevantes, critérios de aceite e pendências.

**Validação obrigatória:** conferir que os três arquivos existem, são reconhecidos pela versão instalada e podem ser selecionados/invocados em uma sessão de teste de baixo custo quando houver cota. Não confundir arquivo criado com agente funcional. Registrar em relatório o caminho dos arquivos, papéis, regras de acionamento e evidência de reconhecimento. Se a versão instalada não suportar agentes customizados, **não declarar sucesso**: preparar arquivos compatíveis quando possível, explicar o impedimento e fornecer o procedimento exato para ativar após atualizar, sem atualizar o Codex automaticamente.

### Comparação obrigatória antes de adotar um framework externo

**Não instale dois frameworks completos para comparar sem critérios.** Faça primeiro um diagnóstico de documentação e compatibilidade. Se os ganhos parecem plausíveis, teste somente **um candidato por vez**, com aprovação do usuário e em branch/worktree separada ou snapshot reversível.

Benchmark orientativo para quando houver cota, com um número pequeno de tarefas reais e equivalentes (não inventar métricas):

- Tarefa A: correção de script C# isolado.
- Tarefa B: mudança de gameplay com testes e critérios de aceite claros.
- Tarefa C: mudança visual envolvendo Blender/Unity, com comparação por captura quando necessária.

Para cada tarefa, registrar cenário base e cenário otimizado: tokens de entrada/saída (quando disponíveis), chamadas de ferramentas, chamadas de modelo/agentes, custo/uso de cota reportado pelo produto quando disponível, duração, resultado funcional, regressões, tempo de revisão humana e necessidade de retrabalho. Não misturar *cota do plano* com preço por token da API; os mecanismos podem ser distintos. Se não puder repetir tarefas comparáveis, marcar avaliação como qualitativa.

**Critério de decisão:** só adotar um framework completo se ele reduzir o **custo por tarefa concluída corretamente** ou o retrabalho de forma consistente, sem aumentar conflitos, passos operacionais ou manutenção. Se um framework economizar contexto por mensagem mas acionar quatro subagentes e gastar mais cota total, rejeitá-lo para o Freya.

**Pesquisa adicional a executar localmente:** verificar a versão do Unity no `ProjectSettings/ProjectVersion.txt`; listar plugins/skills já instalados antes de adicionar outros; avaliar eventuais skills duplicadas; examinar se o Freya já possui uma estrutura Spec Kit, OpenSpec ou BMAD que possa ser reaproveitada. Nenhuma dessas informações foi confirmada pela simples lista de MCPs.

**Entrega parcial adicional:** recomendar explicitamente uma opção entre (A) configuração mínima com Unity oficial + papéis sob demanda, (B) piloto BMGD, (C) piloto GSD, ou (D) manter o workflow atual. Justificar por custo total esperado, compatibilidade, maturidade, esforço e risco. Solicitar aprovação antes de instalar frameworks externos que alterem o fluxo de trabalho do projeto.

## Fase 6 — Regras operacionais curtas para o Freya

Revise os arquivos `AGENTS.md` existentes e evite duplicar ou contradizer instruções. Se apropriado, acrescente uma seção **curta e acionável** com políticas como:

- Antes de ler arquivos, identificar exatamente o conjunto necessário; usar busca direcionada e trechos em vez de arquivos inteiros.
- Em mudanças de C#, limitar o escopo aos scripts e testes afetados.
- Para operações Unity determinísticas, preferir a via mais econômica já validada (CLI ou MCP), sem trocar qualidade por economia aparente.
- Para operações Blender repetitivas, preferir lotes quando suportados e seguros.
- Não abrir cenas, executar o jogo ou renderizar screenshots em cada microalteração; agrupar mudanças e validar nos pontos relevantes.
- Não fazer nova tentativa idêntica após falha sem investigar a causa.
- Evitar logs de build/teste completos; trazer resumo e extratos úteis, preservando detalhes de erro para diagnóstico.
- Não iniciar múltiplos agentes que carregam novamente todo o projeto se uma etapa sequencial resolver o problema.
- Separar implementação, validação e game design quando trouxer independência real, mas evitar replicar contexto e trabalho.
- Manter critérios de aceite objetivos; compilar, testar e fazer inspeção visual quando o tipo de mudança exigir.

Não crie um `AGENTS.md` enorme. Busque o menor conjunto de instruções eficazes.

## Fase 7 — Verificação e mensuração

- Valide sintaxe e precedência de configuração; confira saída de comandos locais e se cada MCP essencial pode ser iniciado/listado conforme suporte instalado.
- Faça testes **não destrutivos** e de baixo custo. Não gaste tokens repetindo tarefas grandes apenas para gerar métricas.
- Se houver telemetria confiável, compare cenários similares em número de chamadas, tokens de entrada, tokens de saída, uso de ferramentas, duração e qualidade da conclusão.
- Se não houver métrica confiável, reporte os indicadores observáveis, identifique lacunas e prepare um pequeno protocolo de benchmark para usar quando houver cota disponível.
- Separe ganho potencial, ganho observado e regressões. Não misture estimativas de descrições MCP com economia total da tarefa.
- Confira que C# scripts, cenas, prefabs, assets e fluxo Blender/Unity permanecem utilizáveis.
- Não altere o código do jogo como parte do benchmark sem autorização.

## Fase 8 — Entregáveis obrigatórios

Ao final, entregue:

1. **Inventário** de ferramentas/versões/MCPs e resumo do funcionamento atual.
2. **Diagnóstico priorizado**: desperdícios comprovados, hipóteses e riscos.
3. **Mudanças aplicadas**: arquivos tocados, justificativas e diff enxuto.
4. **Backups e rollback**: localização dos backups e comandos precisos para desfazer cada alteração.
5. **Validações**: o que foi testado, resultado de cada teste e limitações.
6. **Economia**: dados reais quando disponíveis; caso contrário, expectativas qualitativas e como medir na próxima sessão.
7. **Guia operacional**: qual MCP/CLI usar por tipo de tarefa, quando desabilitar/reativar recursos e como evitar retrabalho.
8. **Pendências**: otimizações promissoras que não foram aplicadas e o motivo.
9. **Framework e skills**: inventário de plugins/skills, comparação de Unity oficial × BMGD × GSD × workflow atual, decisão de adoção/descartes com evidência e protocolo de benchmark.
10. **Três agentes exigidos**: arquivos de `implementer`, `validator` e `game-designer` criados/atualizados, configuração e método de acionamento documentados; teste de reconhecimento/invocação quando suportado. Se houver bloqueio de compatibilidade, detalhar exatamente qual é e como resolver sem alegar sucesso.

Salve um relatório de auditoria em `docs/codex-token-optimization.md` (ou no local de documentação já adotado pelo projeto), sem sobrescrever documento existente sem avaliação. Se a criação de arquivos não estiver autorizada na fase atual, apresente o relatório no chat e peça aprovação para a fase de implementação.

**Critério final de sucesso:** Freya continua funcional; as configurações são reversíveis; o Codex carrega somente o que realmente precisa sempre que a plataforma permitir; o fluxo reduz leituras, chamadas e validações redundantes; e as alegações de economia são rastreáveis a medições ou claramente rotuladas como hipóteses.

---

## Checklist de uso depois que a cota voltar

- [ ] Abrir o repositório Freya no Codex.
- [ ] Entrar no modo de planejamento e colar a seção **Prompt para executar**.
- [ ] Revisar o plano e os riscos antes de autorizar mudanças.
- [ ] Conferir backups e diffs antes de aceitar alterações globais.
- [ ] Confirmar Unity 6+ e verificar `codex plugin list` antes de instalar o plugin oficial da Unity.
- [ ] Confirmar que `implementer`, `validator` e `game-designer` foram configurados no Freya e reconhecidos pelo Codex (ou que o bloqueio foi documentado).
- [ ] Não instalar GSD, BMGD e kits de agentes simultaneamente; escolher no máximo um piloto adicional.
- [ ] Verificar o relatório final e registrar métricas nas tarefas seguintes.
