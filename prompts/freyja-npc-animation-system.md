# Freyja — NPC Animation System

**Feature Spec Kit:** `npc-animation-system`  
**Objetivo:** implementar e validar uma camada reutilizável de animações físicas dos NPCs na Unity 6, integrando locomoção ao Navigation System e ações de interação ao Activity Execution Core e aos Smart Objects. Ao final, demonstrar NPCs que caminham, param, sentam, permanecem sentados, levantam e voltam a caminhar, sem depender da integração completa do scheduler.

## 1. Contexto e limites

O usuário informou que as quatro features abaixo terminaram. Confirme a implementação, os contratos, a compilação e as evidências disponíveis no repositório antes de avançar:

- `smart-objects-foundation`: descoberta, slots, poses e reservas.
- `npc-schedule-core`: resolução declarativa de intenções e horários.
- `npc-activity-execution-core`: execução, cancelamento, limpeza e operações de movimento/pose/animação.
- `npc-navigation-system`: movimentação física, chegada, cancelamento e dados de locomoção.

A Unity voltou a estar disponível. Use Unity MCP ou os mecanismos reais do editor para inspecionar e validar assets, prefabs e Play Mode. Pode existir código de `npc-unity-integration-code`; procure adaptadores de animação/pose já escritos e complete-os, sem duplicá-los.

A próxima feature, `npc-world-integration`, será responsável por conectar relógio, registro de destinos, scheduler e controller definitivo. Não implemente essa orquestração agora. Esta entrega termina na camada de animação e em uma demonstração isolada usando os componentes reais. Não altere as mecânicas ou o visual dos três protagonistas jogáveis.

## 2. Fluxo de trabalho, Git e economia de contexto

1. Leia AGENTS.md, constitution, instruções locais, specs relevantes e contratos reais das quatro features. Use o fluxo Spec Kit instalado (especificação, plano, tasks, implementação e validação). Utilize Game Designer, Implementador e Validador conforme a configuração real; se não estiverem disponíveis, documente e execute as revisões explicitamente. O Game Designer verifica legibilidade, escala e naturalidade; o Implementador integra os componentes; o Validador executa testes e verifica evidências, não apenas revisa código.
2. Inspecione `git status`, branch, diffs pertinentes, possíveis sessões concorrentes e baseline de compilação. Preserve alterações existentes. Não faça reset, stash, checkout destrutivo, merge, commit ou push automático. Não altere arquivos compartilhados simultaneamente com outra sessão. Se necessário, limite-se a arquivos isolados e documente o bloqueio real.
3. Antes de criar componentes, audite os Animators, controllers, rigs, avatars, clips, parâmetros, transições e adaptadores existentes. Verifique quais modelos são Humanoid, Generic ou usam outra solução, e quais podem compartilhar configuração. Não invente nomes de clips, estados ou GUIDs. Reutilize os sistemas que realmente existem.
4. Trabalhe com um plano pequeno. Leia apenas arquivos relevantes, limite buscas e chamadas MCP, reutilize testes/harness, e não execute uma auditoria geral do jogo. Não instale ou atualize pacotes sem necessidade. Se RTK falhar, use comandos nativos. Se Unity MCP falhar, use ferramentas reais alternativas disponíveis; não insista nem abra um segundo editor sobre o mesmo projeto em uso.
5. Implemente e teste de fato, não encerre somente com specs. Alterações em assets/cenas/prefabs devem usar Unity/editor tooling seguro, gerar `.meta` pela Unity e preservar as alterações existentes. Prefira cena ou prefab de demonstração isolado; não faça mudanças amplas na cena principal ou nos assets compartilhados.

## 3. Arquitetura e propriedade das responsabilidades

```text
Activity Execution Core (C# puro)
  |-- port de movimento ----------> NPC Navigation System
  |-- port de pose/animação -------> adaptador Unity de animação/pose
  |-- port de interação ----------> Smart Objects (tokens, slots)
                                      |
Navigation (estado/velocidade) ---> Animation System ---> Animator
Smart Object (pose do slot) ------> alinhamento controlado
```

Nomes são ilustrativos. O Schedule continua decidindo o que fazer e quando. O Activity Core controla o ciclo de vida, as operações, os timeouts de atividade, a identidade da execução e a liberação de reservas. O Navigation controla deslocamento físico. O Animation System traduz solicitações e estado de movimento em animações, acompanha sua conclusão e permite cancelar com segurança. Não crie um segundo executor, scheduler ou registry.

O domínio puro não deve depender de UnityEngine, Animator, Transform ou AnimationClip. Use os ports existentes e um adaptador fino; caso a interface real seja insuficiente, apresente a menor extensão compatível, com testes de regressão. Não crie um contrato concorrente só para o harness. Fakes só em testes; não os selecione silenciosamente em produção.

### 3.1 Entregáveis mínimos de código

- Um componente reutilizável de animação de NPC, ou evolução do componente existente, com bindings configuráveis e acesso às informações reais de locomoção.
- Adaptador do port de animação/pose do Activity Core, respeitando identidades e resultados reais. Evite uma classe por operação ou uma hierarquia genérica excessiva.
- Configuração compartilhável por rig/avatar (ScriptableObject ou mecanismo já existente) mapeando IDs semânticos para parâmetros/estados/clips reais. Overrides pontuais por personagem somente quando necessários.
- Autoridade de movimento e alinhamento definida, com transição segura entre locomoção e interação.
- Diagnóstico mínimo no Inspector e/ou ferramentas existentes, Gizmos de pose quando úteis, documentação de configuração, testes e harness de demonstração.

## 4. Locomoção: idle, walk e run

- Use velocidade efetiva e estado fornecidos pelo Navigation System, não a velocidade máxima configurada nem a mera existência de um destino. Se direção for necessária, use o referencial do personagem. Não mantenha uma segunda lógica de movimentação.
- Configure transições de idle para walk e de walk para run se houver suporte e clips compatíveis. Use Blend Tree simples ou o mecanismo existente; valores de velocidade, thresholds, damping e histerese devem corresponder aos assets e agentes reais. Não imponha números universais sem testar a escala. Evite oscilações em baixa velocidade e disparos repetidos de triggers.
- NPC parado, aguardando caminho, bloqueado ou recém-chegado não deve aparentar caminhar indefinidamente. Ao voltar a mover-se, transição natural, sem reiniciar a animação a cada tick.
- Valide coerência entre velocidade visual e deslocamento (sem deslizamento evidente, passos excessivamente rápidos ou caminhada no lugar). Se houver diferença, ajuste a configuração de forma localizada, preservando controles e velocidade de gameplay existentes. Não implemente foot IK nesta fase.
- Corrida é uma capacidade suportada, não uma nova regra de comportamento. Teste-a com uma solicitação explícita no harness se houver clip apropriado. Não faça os NPCs correrem na rotina por padrão.
- Em locomoção, mantenha uma única autoridade sobre posição/rotação. Prefira animação in-place conduzida pela navegação, se compatível com o projeto. Caso o projeto use root motion deliberadamente, integre ao contrato existente e demonstre que não há dupla movimentação. Não mude root motion de controllers dos protagonistas ou assets compartilhados sem necessidade.

## 5. Catálogo e bindings de animação

IDs mínimos propostos; use os IDs efetivamente presentes no catálogo/receitas do Activity Core. Se existir diferença, documente o mapeamento, sem criar uma segunda fonte de verdade para as atividades.

| ID semântico | Função | Semântica de execução |
|---|---|---|
| `idle` | Repouso em pé | Estado contínuo, controlado pela locomoção |
| `walk` | Caminhada | Loop controlado pela velocidade |
| `run` | Corrida | Loop controlado pela velocidade |
| `sit_down` | Transição em pé → sentado | Ação finita, sucesso após pose sentada real |
| `sit_idle` | Permanecer sentado | Loop/pose persistente, não uma ação que bloqueia indefinidamente a receita |
| `stand_up` | Transição sentado → em pé | Ação finita, sucesso após recuperação da postura |

Use um mapeamento validável: ID semântico, estado/parametrização real, layer aplicável, tipo finito/loop, condição de conclusão e timeout apropriado. Não serialize nomes de states ou hashes Unity nos JSON de rotinas. JSON continua fonte de verdade para agenda e receitas; bindings são configuração de apresentação física. Não duplique horários ou metadados de atividades no Animator.

Um Animator Controller compartilhado por rig compatível é preferível a um por morador. Use overrides somente se necessário e tecnicamente compatíveis. Não reestruture todos os controllers nem crie uma biblioteca de animação genérica para o jogo inteiro.

### 5.1 Auditoria de assets e retargeting

- Localize clips nos pacotes existentes, inclusive os assets dos personagens jogáveis se forem reutilizáveis sem alteração. Verifique rig, avatar, loop, importação, root motion e escala. Faça retargeting somente quando houver suporte real e valide visualmente o personagem. Um clip Humanoid não é automaticamente compatível com todo rig Generic.
- Priorize assets licenciados já no projeto. Não use Tripo AI, não compre assets, não baixe pacotes externos nem crie dependência paga sem autorização. Não instale frameworks avançados de animação.
- Se não houver um clip adequado de sentar/levantar, registre o que foi encontrado e o que falta. Uma pose provisória tecnicamente válida pode ser usada apenas como demonstração identificada, se viável, sem declarar a animação final validada. Nunca use personagem em pé para aprovar `sit`, nem simule sucesso por timer. Não fabrique referência a clip inexistente. Prossiga com locomoção e testes do contrato que forem possíveis e apresente o bloqueio específico.

## 6. Contrato de execução, loops e interrupções

Adapte às assinaturas reais. Não redesenhe o Activity Core para encaixar uma API imaginada.

- Iniciar ação: recebe identidade de execução/operação, ID semântico e contexto necessário; valida Animator ativo, binding, layer, parâmetros, compatibilidade e estado físico. Dispara a solicitação uma única vez.
- Consultar progresso: retorna Running/Succeeded/Failed/Cancelled (ou equivalentes reais), motivo de falha e informações de diagnóstico. Um trigger aceito não é conclusão. Verifique entrada no estado correspondente à operação atual, transições e critério de saída/conclusão; use eventos ou observação de estado quando apropriado. Não confunda estado anterior, normalized time de outro loop ou simples duração estimada com sucesso.
- Cancelar: atua apenas sobre a execução atual e é idempotente. Eventos atrasados, callbacks repetidos e cancelamentos antigos não podem concluir ou interromper outra ação. Não deixe triggers antigos pendentes. Limpe assinaturas e recursos no disable/destroy.
- Diferencie ações finitas de estados contínuos. O `sit_down` deve concluir quando o NPC estiver sentado e estabilizado. A pose/loop sentado persiste enquanto o Activity Core aguarda o fim da agenda. Não reinicie o clip de sentar durante a espera. `stand_up` conclui ao recuperar postura e condições de movimento. Se a receita real não usa `sit_idle` explícito, implemente a permanência na pose dentro da camada visual, sem alterar a semântica da receita.
- Timeout de operação usa o tempo operacional e as convenções existentes; não dependa de hora do calendário do jogo, DateTime.Now, Task.Delay ou Thread.Sleep. Respeite pausa legítima, Animator speed, desativação e transições. Falha de configuração ou ação rejeitada deve retornar motivo explícito, sem tentativas infinitas. Não multiplique a política de retries do Activity Core.
- Se o Animator estiver fora de cena/culling, desativado ou sem controller, defina política explícita que não produza espera infinita ou falso sucesso. Nesta fase, teste com NPCs ativos e configuração que permita progresso observável; não implemente LOD ou simulação offscreen.
- Em caso de mudança de agenda, interrupção externa ou erro durante `sit_down`/`sit_idle`/`stand_up`, faça recuperação de postura e liberação do controle físico coerente com o contrato de limpeza. Cancelamento não precisa significar retorno instantâneo em pé; use saída segura e limitada quando o objeto e o NPC ainda estão ativos. Em disable/destroy, não dependa de nova animação para limpeza. Se não for possível terminar uma saída, exponha falha, pare o movimento e preserve estado consistente. Não inicie nova locomoção enquanto o personagem ainda estiver fisicamente sentado no slot.

## 7. Alinhamento e interação com Smart Objects

### 7.1 Sequência de referência

```text
Activity Core: localizar e reservar slot (token válido)
  -> Navigation: alcançar ponto de aproximação
  -> Pose: validar slot e alinhar posição/orientação local
  -> Activity Core: confirmar ocupação conforme contrato existente
  -> Animation: sit_down -> pose/loop sentado
  -> Activity Core: aguardar prazo do compromisso
  -> Animation: stand_up -> recuperar postura e controle de locomoção
  -> Activity Core: liberar reserva
  -> Navigation: próximo destino
```

A sequência deve seguir o contrato real de Smart Objects/Activity Core. O sistema de animação não deve reservar, ocupar ou liberar slots autonomamente. Se uma operação de recuperação exigir manter uma reserva até sair do assento, respeite a ordem de limpeza e a propriedade do token; não permita outro NPC ocupar o mesmo espaço antes da saída segura. Se houver incompatibilidade na ordem existente, reporte e corrija de forma mínima e testada.

### 7.2 Regras físicas

- Utilize pose e orientação reais do slot, incluindo ponto de aproximação e eventual ponto de saída já existente. Não hardcode coordenadas de bancos. O alvo final de interação pode não estar sobre o NavMesh; isso não é um motivo para navegar até o centro do assento.
- Alinhe somente após chegada confirmada e reserva válida. Alinhamento local com tolerâncias configuráveis de distância, ângulo e altura; não teletransporte o NPC a longa distância. Rejeite pose inacessível, destruída ou incompatível. A orientação do rig, posição da pelve, altura do assento e posição dos pés devem ser verificadas no modelo real.
- Estabeleça handoff explícito entre navegação e pose: parar movimento, garantir que somente um componente controla Transform/rotação, sincronizar o estado interno do agente e restaurar os parâmetros que a interação alterou quando voltar à locomoção. Não permita NavMeshAgent e root motion escreverem simultaneamente. Não desabilite permanentemente componentes nem use `Warp` para mascarar erro de caminho ou alinhamento. Se sincronização local for indispensável, siga o contrato do Navigation e valide tolerâncias e superfície.
- Ajuste pontos/offsets nos prefabs de teste com ferramentas Unity, sem mover construções ou modificar toda a ilha. Não altere clips originais de terceiros de forma destrutiva. Um banco de dois lugares deve permitir poses independentes sem sobreposição evidente.
- Se o slot ficar inválido durante uma interação, informe ao executor e inicie a recuperação prevista; não mantenha a pose em objeto inexistente. Não libere token pertencente a outra execução. O Activity Core é autoridade para cleanup e resultados terminais.

## 8. Preparação para atividades futuras (sem implementá-las)

A mesma camada deve permitir registrar futuramente `talk`, `fish`, `sweep`, `sleep`, `eat`, `water_crops` e outras ações finitas ou em loop, por meio dos bindings existentes. Documente como adicionar um novo ID e associá-lo a um estado real. Não implemente essas atividades, props/equipamentos, IK, animações faciais, sincronização social, root motion avançado ou uma DSL. Não adicione um framework de animações apenas para prever requisitos futuros.

## 9. Demonstração e ferramentas de autoria

Use três NPCs de teste (reutilize os prefabs de Navigation se disponíveis), dois bancos de teste configurados como Smart Objects ou os bancos existentes adequados. Prefira harness/área de teste isolada; não dependa do scheduler/relógio real nem do controller definitivo. Acione as operações reais do Activity Core se seu contrato permitir; caso contrário, use harness mínimo que chama os adapters e componentes reais, e deixe explícito que a orquestração completa está pendente. Não use fakes para representar a animação ou movimento no teste físico.

Roteiro: NPC A e B caminham até dois slots do mesmo banco, alinham, sentam, mantêm a pose e saem com segurança. NPC C tenta o banco ocupado e deve respeitar a reserva; usa alternativa disponível ou recebe indisponibilidade conforme contrato existente. Em outro cenário, cancele a ação de A durante o deslocamento ou a transição de sentar e valide que nenhuma ação tardia interfere no próximo movimento. Repita com o NPC já sentado. Faça uma solicitação de corrida isolada se suportada pelos clips. Após levantar, valide caminho real de retorno.

Inspector/diagnóstico mínimo, reutilizando o que existir:

- ID de NPC/execução/operação, ID de animação solicitado, estado do Animator, layer, parâmetros/velocidade efetiva, modo de locomoção/interação.
- Resultado da operação, tempo/timeout e reason code, estado de recuperação da postura.
- Slot/pose de interação, erro de distância/ângulo quando aplicável, proprietário do controle de movimento e estado do agente.
- Gizmos de aproximação/pose já fornecidos pelos Smart Objects, complementados somente quando necessário; sem construir janela completa de edição de rotinas.

## 10. Testes e critérios de aceite

Execute a baseline de testes de Schedule, Activity, Smart Objects e Navigation; registre os comandos reais, resultados e eventuais falhas preexistentes. Escreva testes unitários/Edit Mode para mapeamentos, validação, identidade/cancelamento e estados quando possível. Testes Play Mode devem executar componentes reais e verificar o estado físico em sequência. Asserções devem aguardar condições com limites, não depender de esperas arbitrárias e frágeis. Use o Unity Test Runner e Unity MCP conforme disponíveis. Simulação não comprova movimento/animação reais.

| ID | Cenário | Aceite |
|---|---|---|
| AN-01 | Compilar e importar na Unity 6 | Nenhum erro novo, referências válidas, sem Missing Script nos prefabs de teste. |
| AN-02 | NPC parado e em espera de caminho | Idle real; sem caminhada no lugar indefinida. |
| AN-03 | Deslocamento e parada | Walk sincronizado à velocidade real, retorno a idle, sem deslizamento evidente. |
| AN-04 | Corrida solicitada | Run sincronizado, se clip compatível; ausência de clip é pendência, não passe fictício. |
| AN-05 | Navegação → pose | Chegada real antes de alinhar; tolerâncias respeitadas, sem teletransporte visível. |
| AN-06 | Sentar | Transição termina em pose sentada real, compatível com o banco. |
| AN-07 | Permanecer sentado | Pose/loop persistente, sem reiniciar a transição nem bloquear o executor indefinidamente. |
| AN-08 | Levantar e caminhar | Postura recuperada, autoridade do movimento restaurada, reserva liberada na ordem segura. |
| AN-09 | Dois NPCs no mesmo banco | Slots independentes, sem interpenetração significativa ou reserva duplicada. |
| AN-10 | Terceiro NPC sem slot livre | Respeita reserva e alternativa/falha existente; não executa pose no slot ocupado. |
| AN-11 | Cancelamento em trânsito ou durante sit_down | Recuperação segura, movimento futuro não afetado por callbacks antigos. |
| AN-12 | Interrupção quando sentado | Saída segura, limpeza de recursos, retorno a locomoção; nenhuma segunda execução concorrente. |
| AN-13 | Binding inválido / Animator indisponível / timeout | Falha explícita e limitada, sem falso sucesso ou loop infinito. |
| AN-14 | Disable/destroy ou slot invalidado | Limpeza e ausência de reservas/controle físico presos; nenhuma liberação de token alheio. |
| AN-15 | Solicitação repetida e evento atrasado | Não reinicia animação nem altera outra operação; cancelamento idempotente. |
| AN-16 | Pausa e retomada | Sem timeout indevido; execução retoma conforme contrato temporal real. |
| AN-17 | Regressão | Testes anteriores continuam passando; controles dos protagonistas preservados. |

Registre quais testes são automatizados, quais exigem inspeção visual e quais não puderam ser realizados. Em Play Mode, produza evidências sequenciais (logs correlacionados com IDs, estados e resultados; capturas dos marcos ou vídeo curto se suportado), não apenas uma captura de NPC junto de um banco. Verifique console, ausência de exceções, agentes retomando caminhos e slots efetivamente liberados. Use os assets e NPCs reais; não considere teste em C# puro como validação física.

### Gate de conclusão

Para declarar a feature concluída e pronta para `npc-world-integration`: compilação Unity confirmada; locomoção idle/walk e teste de run conforme disponibilidade documentada; sit_down/pose sentada/stand_up reais e retorno seguro ao movimento; reservas e cancelamentos demonstrados com três NPCs; testes de regressão sem falhas novas; instruções reproduzíveis. Se clips ou integração física indispensáveis faltarem, marque a capacidade afetada como parcial/bloqueada e explique o que falta. Não maquie o aceite nem expanda o escopo para contornar ausência de assets.

## 11. Fora do escopo

- Novo scheduler, NPC Controller definitivo, clock adapter, registro geral de destinos e integração global de rotinas.
- Editor visual de agendas, população de toda a ilha, profissões, diálogo, missões, possessão, interiores ou narrativa.
- Biblioteca completa de atividades/animações, ferramentas/props, IK, motion matching, ragdoll, sistemas de multidões, ECS/DOTS, comportamento avançado e LOD.
- Alteração geral de terreno, NavMesh, World Builder, controles ou Animator dos protagonistas. Corrigir apenas bloqueios locais comprovados dentro da fronteira da feature; reportar outras pendências.
- Compras, créditos Tripo, downloads externos sem autorização, atualização geral de dependências, operações destrutivas de Git.

## 12. Relatório final exigido

Entregue um relatório curto, rastreável e reproduzível contendo:

1. Status da feature: implementado, compilado, testes automatizados, validação física e bloqueios (separadamente).
2. Infraestrutura encontrada/reutilizada, contratos reais utilizados, autoridade de movimento e decisões de alinhamento/limpeza; diferenças em relação aos nomes sugeridos.
3. Catálogo de IDs → bindings/estados reais → clips existentes, rigs compatíveis e origem dos assets; capacidades não disponíveis. Não invente referências.
4. Arquivos/scripts/asmdefs/JSON/assets/prefabs/cenas/.meta alterados e justificativa; `git status` final; alterações locais anteriores preservadas.
5. Comandos executados, resultados por teste AN-01 a AN-17 (passou/falhou/não executado), motivo e caminhos das evidências. Não invente números ou evidências.
6. Guia rápido de autoria: como configurar um NPC, adicionar bindings, configurar pose de assento, executar harness e reproduzir testes na Unity.
7. Contratos e pendências para `npc-world-integration`: como consumir os ports reais de Navigation e Animation, dependências, inicialização, ciclo de vida, resultados, política de cancelamento e como integrar ao relógio e scheduler sem duplicação.

**Resultado final:** uma camada física de animação reutilizável e validada, que deixa a próxima entrega concentrar-se em conectar agenda, executor, destinos, relógio e NPCs ao mundo real do Freyja.
