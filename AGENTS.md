# Orientações para agentes — Freyja

## Contexto e fontes

- Trabalhe em iterações curtas com artefatos visuais avaliáveis. Apresente cada versão e aguarde a avaliação do usuário antes de ampliar o escopo; não construir toda a sequência narrativa de uma vez.
- Para cada nova entrega, use primeiro speckit-specify e apresente uma spec curta para avaliação. Aguarde aprovação explícita dessa spec antes de executar speckit-plan, speckit-tasks ou implementar. Aprovar a entrega anterior ou dizer "pode seguir" autoriza preparar a próxima spec, não implementar um escopo ainda não avaliado. Depois da aprovação, siga o fluxo Spec Kit; ao concluir a entrega, pare para nova avaliação.
- Desenvolvemos um jogo 3D de investigação narrativa em Unity, com gráficos low poly e pixels visíveis nas silhuetas. O título definitivo é Freyja.
- Consulte `GDD.md` para decisões do jogo e `.specify/memory/constitution.md` para princípios do projeto. Leia apenas as partes relevantes para a tarefa.
- Antes de implementar uma funcionalidade, consulte sua pasta em `specs/`, incluindo `spec.md`, `plan.md` e `tasks.md`, quando existirem.
- Não transforme propostas em decisões silenciosamente. Registre dúvidas e divergências entre implementação e documentos; siga as instruções mais recentes do usuário.
- Preserve a lógica da investigação: diferencie fatos observados, depoimentos e hipóteses. Pistas e personagens devem respeitar o GDD.

## Ambiente Unity e ferramentas

- O projeto Unity está em `Freyja/`, usa o editor `6000.3.25f1` e o pipeline Built-in. A fonte Blender inicial está em `Source/Blender/Freyja.blend`. Consulte `Tools/MCP/README.md` para abrir os editores e verificar as conexões reais.
- Use o MCP global do Unity sempre que disponível para implementar, inspecionar cenas, configurar componentes e testar o jogo. Instale ou ative a integração no projeto quando necessário.
- Confirme o projeto e a cena ativos antes de modificar o editor. Salve cenas, prefabs e demais assets após alterações.
- Se o MCP estiver indisponível, informe a limitação e avance nas tarefas possíveis por arquivos ou CLI. Não declare validação no editor sem executá-la.
- Use o MCP global do Blender para criar ou editar modelos, cenários e animações. Preserve os arquivos-fonte editáveis e exporte os assets para o projeto Unity.
- Identifique a raiz Unity pela presença de `Assets/`, `Packages/` e `ProjectSettings/`; ela pode ser a raiz do repositório ou uma subpasta. Não crie um segundo projeto por engano.
- Ao inicializar o projeto, registre a versão exata do editor, o pipeline de renderização e a plataforma inicial. Enquanto não houver decisão, explicite a escolha antes de adotá-la; não presuma uma versão existente.
- Depois de inicializado, respeite `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` e `Packages/packages-lock.json`. Não atualize editor, pipeline ou dependências fora do escopo da tarefa.

## Organização dos artefatos

- Na raiz Unity, concentre conteúdo próprio em `Assets/_Project/`, com pastas como `Scenes`, `Scripts`, `Prefabs`, `Art`, `Audio`, `Materials`, `Data` e `Tests`, criadas conforme a necessidade. Preserve convenções existentes.
- Mantenha arquivos de produção, como `.blend`, em uma pasta de fontes fora de `Assets/`, e exports consumidos pelo jogo dentro de `Assets/_Project/`. Documente os caminhos adotados.
- Mantenha conteúdo de terceiros separado do código e dos assets próprios. Evite modificar diretamente pacotes ou assets externos.
- Versione `Assets/` com seus arquivos `.meta`, `Packages/` e `ProjectSettings/`. Configure serialização de assets como texto e metadados visíveis quando o projeto for criado.
- Nunca apague ou regenere `.meta` de assets existentes para resolver problemas de importação. Prefira mover e renomear assets pelo editor; ao operar por arquivos, preserve o par asset/`.meta` e seus GUIDs.
- Não versione `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, builds ou arquivos de IDE gerados. Não apague arquivos existentes do usuário ao ajustar o `.gitignore`.

## Código e estado do jogo

- Use C# compatível com a versão do Unity adotada. Siga as convenções existentes; para código novo, use nomes em inglês e namespaces coerentes.
- Mantenha classes pequenas e responsabilidades claras. Separe regras de investigação e progressão dos componentes de apresentação quando isso facilitar manutenção e testes.
- Prefira referências explícitas pelo Inspector, com campos privados e `[SerializeField]`. Valide dependências obrigatórias e evite buscas globais repetidas.
- Evite alocações e buscas de componentes em `Update`; use eventos ou chamadas explícitas quando adequados. Meça antes de introduzir otimizações complexas.
- Use IDs estáveis para pistas, diálogos e estados de progressão. Não use texto exibido ao jogador como identificador.
- Use ScriptableObjects para definições compartilhadas quando fizer sentido; mantenha o estado mutável da sessão separado dos assets de definição.
- Respeite o sistema de input, a câmera e a UI adotados pelo projeto. Não introduza sistemas concorrentes sem necessidade.
- Não amplie o escopo com frameworks, persistência ou abstrações sem uma necessidade da entrega atual. Para a primeira entrega, siga o estado de sessão previsto no plano.

## Cenas, prefabs e gráficos

- Use `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` como cena principal para as próximas alterações no Unity. Trabalhe nela e crie outra cena somente quando o usuário solicitar ou uma necessidade técnica concreta exigir.

- Ao reutilizar os modelos TripoAI da spec 011, consulte `Source/Blender/Ilha_Tripo_011/README.md` para a escala calibrada pelos degraus originais e pelo protagonista de 1,80 m.

- Reutilize prefabs para elementos recorrentes. Evite alterações manuais em YAML de cenas e prefabs; prefira o editor ou suas APIs e valide referências após alterações.
- Verifique escala de importação, orientação, pivôs, materiais e colisões ao trazer assets do Blender. Use uma convenção consistente de unidades.
- Mantenha rostos, pistas e pontos de interação legíveis na câmera real do jogo. Valide a direção low poly e os pixels visíveis nessa câmera.
- Use placeholders identificados quando previstos na etapa. Não substitua assets finais existentes por placeholders sem justificativa da tarefa.

## Validação e entrega

- Após mudanças de código, verifique a compilação no Unity e os erros relevantes do Console. Após mudanças de cena ou interação, valide o fluxo afetado em Play Mode.
- Use testes Edit Mode para regras determinísticas e Play Mode para comportamentos que dependam do ciclo do Unity, quando o risco e a complexidade justificarem. Não crie testes que apenas repitam a implementação.
- Em investigação, verifique ordens diferentes de descoberta, repetição de diálogos e transições de estado pertinentes à mudança. Na entrega inicial, teste ambas as ordens previstas para liberar a pergunta de Ivo.
- Faça build quando a tarefa envolver empacotamento, plataforma ou mudança que exija essa validação. Registre a plataforma e o resultado; não trate compilação no editor como prova de build.
- Marque tarefas concluídas somente com evidência: cena, arquivo, teste ou captura correspondente. Atualize `tasks.md` quando a tarefa fizer parte de uma spec.
- Ao finalizar, informe o que mudou, onde encontrar os artefatos, o que foi verificado e qualquer limitação real. Diferencie verificações executadas de verificações pendentes.

## Comunicação e comandos

- Salve todas as entregas do Freyja em `/Users/bruno.duartec/Documents/Projects/Freyja`.

- Responda em português, de forma direta e concisa. Preserve detalhes necessários para retomar o trabalho.
- Leia `/Users/bruno.duartec/.codex/RTK.md` uma vez por sessão antes de usar comandos shell. Use `rtk` em comandos suportados com saída volumosa e `rtk proxy` para conteúdo exato ou comandos sem suporte; recorra ao comando nativo se necessário.
- Restrinja buscas aos arquivos e intervalos relevantes e amplie apenas quando necessário. Não repita verificações aprovadas sem mudança ou dúvida que justifique.
- Preserve alterações do usuário e mantenha as mudanças restritas à tarefa solicitada.


## Git e branches do Spec Kit

- A partir desta orientação, cada nova spec deve ser criada em uma branch própria, nunca diretamente em `main`. O nome deve seguir o padrão sequencial do Spec Kit: `<NNN>-<nome-curto>`, correspondente à pasta `specs/<NNN>-<nome-curto>/`.
- Antes de escrever a spec, verifique o estado do Git e as branches locais/remotas. Use `main` atualizada como base quando isso puder ser feito sem misturar ou descartar alterações existentes. Preserve trabalhos em andamento.
- Use `.specify/scripts/bash/create-new-feature.sh --dry-run --json --short-name <nome-curto> "<descrição>"` para resolver o próximo número e nome sem criar artefatos. Crie e selecione a branch resolvida com Git antes de gravar a nova spec.
- Execute o mesmo script sem `--dry-run`, com os mesmos argumentos, para criar a pasta e os metadados. Confirme que o nome retornado corresponde à branch selecionada; mantenha `.specify/feature.json` apontando para essa pasta. Se houver divergência, corrija-a antes de prosseguir.
- Na versão local atual, esse script cria a pasta e os metadados, mas não cria a branch Git. A existência da pasta não substitui a criação da branch. Se um hook oficial de Git estiver ativo e já tiver criado a branch correta, reutilize-a em vez de criar outra.
- Ao retomar uma spec existente, trabalhe na branch correspondente. Planejamento, tarefas e implementação dessa feature permanecem nela. Essa regra de branches não elimina a aprovação da spec antes de planejar ou implementar.
- Não reescreva o histórico nem mova alterações de outras entregas para satisfazer o fluxo. Esta orientação vale para novas specs após o commit que a registra; specs anteriores permanecem com seu histórico atual.
