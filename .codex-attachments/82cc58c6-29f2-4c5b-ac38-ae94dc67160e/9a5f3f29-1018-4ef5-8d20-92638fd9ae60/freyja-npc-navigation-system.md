# Freyja — NPC Navigation System

**Feature Spec Kit:** `npc-navigation-system`  
**Objetivo:** implementar e validar a camada de navegação física dos NPCs na Unity 6, reutilizável pelas atividades `idle`, `wander` e `sit` e preparada para atividades futuras. Esta entrega deve comprovar deslocamento real, chegada, cancelamento e recuperação de falhas, sem depender de um scheduler completo ou de novas animações.

## 1. Contexto e fronteiras da entrega

- Smart Objects Foundation e NPC Schedule Core foram relatados como concluídos pelo usuário; Activity Execution Core está em finalização. Verifique o código, os contratos e os resultados existentes; não trate relatos como evidência de integração física.
- A Unity voltou a estar disponível. Use o editor e o Unity MCP quando úteis para inspeção, configuração e validação em Play Mode.
- Uma especificação anterior de `npc-unity-integration-code` pode ter produzido adaptadores. Localize-os antes de criar código. Se a navegação já existir, complete e valide essa implementação em vez de duplicá-la.
- Esta feature substitui apenas a parcela de navegação do plano anterior de integração. Não implemente a orquestração completa dos NPCs nesta entrega.
- A ilha tem população pequena, terreno com desníveis, escadas, praça, praia, cais e objetos interativos. Não presuma que todos esses destinos já estejam configurados ou navegáveis. O projeto evoluirá para interiores acessíveis, mas eles não são requisito desta entrega.

Fluxo arquitetural pretendido (nomes ilustrativos; use os contratos reais):

```text
Schedule Core -> Activity Execution Core -> port de navegação
                                             |
                                      adapter Unity
                                             |
                               NavMesh / movimento existente
                                             |
                              terreno e pontos de aproximação

Smart Object Registry -> slot reservado -> ponto de aproximação
Animation System (futuro) <- estado de locomoção
```

O Activity Core decide quando começar/cancelar uma operação. A navegação executa essa operação e retorna estado e resultado. O Smart Object controla reservas. O Animator futuro consome informações de movimento. Não transfira essas responsabilidades para a navegação.

## 2. Fluxo de trabalho e segurança do repositório

1. Leia AGENTS.md, instruções relevantes, constitution e specs existentes. Use Spec Kit (specify/plan/tasks/implement e revisão conforme o fluxo instalado). Use os agentes Game Designer, Implementador e Validador se realmente disponíveis, com contextos enxutos e sem leituras redundantes. Se não disponíveis, documente e faça as etapas de revisão explicitamente.
2. Inspecione `git status`, branch, diffs relevantes, versão Unity, manifest/lock de pacotes, cenas e componentes existentes. Confirme quais arquivos estão sendo editados pela sessão de Activity Core. Não sobrescreva mudanças preexistentes ou concorrentes. Não faça reset, stash, checkout destrutivo, merge, commit ou push automático.
3. Reaproveite o sistema de movimentação, AI Navigation, NavMeshSurface, agentes, obstáculos e ferramentas existentes. Não instale/atualize pacotes sem necessidade; caso uma dependência indispensável falte, documente o motivo e use somente a versão compatível confirmada no projeto, sem upgrades gerais. Se a mudança impactar sistemas compartilhados, pare nesse ponto e reporte o bloqueio.
4. Confirme a compilação inicial antes de adicionar código. Se houver erros preexistentes, registre e diferencie-os das regressões desta feature. Se houver mudanças concorrentes nos contratos do Activity Core, trabalhe em arquivos isolados e com harness local; não edite esses contratos nem invente uma API pública alternativa. Integração definitiva fica pendente se o contrato ainda não estiver estável.
5. Implemente, teste e valide na Unity nesta sessão quando possível. Não encerre apenas com specs ou tasks. Não insista em ferramenta indisponível: use alternativas reais do ambiente, como Unity Test Runner ou batchmode quando seguro, e dotnet nativo se RTK falhar. Não abra uma segunda instância do editor sobre o mesmo checkout em uso.

## 3. Escopo funcional

### 3.1 Auditoria e configuração do NavMesh

- Identifique se o projeto utiliza `NavMeshAgent`, outro controlador ou integração própria. Adote o mecanismo existente se atende aos requisitos; a preferência para NPCs é NavMesh da Unity, não criar um algoritmo de pathfinding.
- Verifique a superfície navegável, agent type, radius, height, step/climb, slope, speed, acceleration, stopping distance e área permitida, considerando as dimensões reais dos personagens e a escala do cenário. Não imponha valores genéricos sem verificar a geometria.
- Verifique caminhos entre pontos de teste: praça, praia, residência (ponto externo), banco e, se viável, cais e uma escada existente. Use as superfícies atuais antes de considerar novo bake. Mantenha água, paredes, telhados, penhascos e áreas proibidas fora do espaço navegável conforme a geometria e regras do projeto.
- Corrija somente problemas locais indispensáveis ao cenário de teste. Não rebake ou substitua o NavMesh de toda a ilha sem necessidade demonstrada; prefira uma área/cena de validação isolada para alterações experimentais. Registre qualquer alteração persistente feita em cena ou asset, e seu efeito sobre o World Builder.
- Escadas e rampas: reaproveite a solução de colisão e navegação existente; valide um percurso com desnível se disponível. Não introduza IK de pés, física avançada ou travessia especial por links nesta fase. Se uma escada exigir trabalho específico, documente separadamente o motivo e a rota afetada; não declare a rota validada.
- Não crie NavMeshSurface em cada NPC nem faça rebake contínuo em runtime. Utilize obstáculos/carving existentes de forma criteriosa; não adicione carving móvel em todos os personagens.

### 3.2 Componente e adaptador de movimento

Implemente ou complete um componente pequeno (ex.: `NpcNavigationController`) e o adaptador do port existente no Activity Core. Evite criar ambos se um único componente cumprir o contrato sem prejudicar separação de responsabilidades. O domínio puro não deve passar a depender da Unity.

Operações necessárias (nomes e assinaturas ilustrativos):

| Operação | Semântica |
|---|---|
| Iniciar movimento | Aceita destino e identidade da operação; verifica agente, destino e disponibilidade |
| Consultar progresso | Retorna estado atual, distância restante quando válida e motivo de falha |
| Cancelar | Para apenas a operação vigente do dono correto, limpa o caminho e finaliza com segurança |
| Consultar locomoção | Exibe velocidade real, direção, estado de deslocamento e chegada para diagnóstico e futura animação |

Estados mínimos equivalentes a `Idle`, `PathPending`, `Moving`, `Arrived`, `Failed`, `Cancelled`. Reuse enum/resultados existentes; não crie outra máquina de estados de atividades.

Requisitos:

- Um único componente tem autoridade sobre a posição/rotação de locomoção do NPC. Não misture `NavMeshAgent`, CharacterController, Rigidbody, root motion e escrita de Transform de forma concorrente. Preserve o comportamento dos protagonistas; não modifique seus controllers para adequar os NPCs.
- O início da operação deve acontecer uma vez; não chame `SetDestination` a cada frame. Use polling/tick no fluxo existente, sem corrotinas ou threads duplicadas que disputem o agente. Não dependa de callbacks tardios sem identificar a execução.
- Trate agente desativado/fora do NavMesh, destino inválido, solicitação rejeitada, caminho pendente, caminho parcial/inválido, distância não finita e destino destruído. Utilize projeção no NavMesh somente dentro de tolerância pequena configurável, preservando a intenção do alvo e evitando selecionar outro piso ou lado de uma parede. Não aceite caminho parcial como chegada.
- Confirme chegada apenas após resolver o caminho da solicitação atual, com caminho válido (ou caso comprovado de já estar no destino), distância/tolerância apropriada e condição de parada coerente. Considere diferença vertical e posição efetiva para evitar falso sucesso. Não use apenas `remainingDistance == 0`, nem apenas `velocity == 0`.
- Diferencie ausência de progresso, espera por cálculo de caminho e obstrução temporária. Timeout e detecção de travamento devem ser configuráveis, com tentativas limitadas e cooldown quando apropriado. Se o Activity Core já controla retentativas/timeout, respeite essa autoridade; use watchdog local apenas para garantir término de operações físicas e sem multiplicar retentativas.
- Use tempo de execução físico conforme as convenções do projeto, separado de minutos/horas do mundo. Defina comportamento sob pausa e aceleração do relógio: não cause timeout artificial durante pausa legítima; salto de agenda é tratado por cancelamento do Activity Core, não por teletransporte.
- Cancelamento idempotente e associado à execução/operação vigente: chamadas antigas não podem parar movimento novo. Ao desabilitar/destruir NPC, limpar operação e callbacks sem depender de novos ticks. Não altere configurações que pertencem a outro sistema.
- Não use `Warp`, teletransporte ou movimento direto pelo Transform para mascarar caminho inválido. Não declare sucesso quando o agente não se deslocou e não estava no destino. Não invente progresso de animação.

### 3.3 Destinos e deslocamento regional (`wander`)

- Aceite destinos resolvidos pelo contrato existente: ponto no mundo, Transform ou referência equivalente. Use IDs simbólicos existentes para configurar exemplos; não crie o Location Registry definitivo nesta entrega.
- Prepare uma região de demonstração com pontos configuráveis, preferencialmente reaproveitando regiões e waypoints existentes. Gizmos devem mostrar os pontos.
- Para `wander`, use a seleção de pontos e repetição do Activity Core se já implementadas. O adaptador realiza cada deslocamento; não implemente um segundo scheduler ou loop concorrente. Se necessário, ofereça uma função pequena de seleção/validação de candidatos para o contrato existente. Um harness pode exercitar sequência de pontos até a atividade terminar.
- Valide candidatos navegáveis e alcançáveis, com número finito de tentativas. Não selecione pontos na água, fora da região ou em outro nível por acidente. Se a região for definida apenas pelos endpoints, documente que o trajeto pode sair dela; confinamento integral do caminho só deve ser exigido onde o cenário já o permitir ou houver requisito real.
- Se não houver ponto válido, retorne falha explícita; o executor decide fallback. Evite movimentação nervosa, destinos a cada frame e loops sem progresso. Sem patrulha genérica, perseguição ou planejamento procedural complexo.

### 3.4 Aproximação de Smart Objects

- Reaproveite Smart Objects existentes e seus slots/Transforms de aproximação. A navegação recebe o ponto de aproximação do slot escolhido pelo consumidor, não procura nem reserva bancos por conta própria.
- Valide pelo menos um banco existente com dois pontos de aproximação, desde que esteja configurado, e dois NPCs chegando a pontos distintos. Utilize as reservas reais no harness quando o contrato permitir. Se o banco/slots ainda não estiverem disponíveis, use marcadores provisórios na cena de teste para validar movimento e registre que a integração com objeto real continua pendente.
- O consumidor de teste ou Activity Core mantém propriedade do handle e faz liberação após conclusão/cancelamento/falha. A navegação deve reportar falha de alvo invalidado; não ocupar, liberar ou alterar reservas por conta própria. Teste que o consumidor libera corretamente em cancelamento, quando a integração estiver disponível.
- A chegada é no ponto de aproximação, não na pose sentada. O alinhamento fino da pose e transição de autoridade de movimento para animação pertencem à futura feature de animação/integração. Não force posição sobre o assento.

### 3.5 Movimento de múltiplos NPCs

- Valide três NPCs utilizando o mesmo código e configurações reutilizáveis. Use avoidance e parâmetros do NavMeshAgent existente para evitar sobreposição grosseira em caminhos compartilhados.
- Teste cruzamento de trajetórias e disputa por espaço próximo aos bancos. Não implemente sistema próprio de multidões, filas, tráfego ou colisão avançada. Se gargalos ocorrerem, registre a causa e trate com ajuste local ou falha controlada.
- Mantenha baixo custo de execução: sem buscas globais ou recálculos de caminho indiscriminados por frame. Não é necessário ECS/DOTS, jobs, GOAP ou LOD de simulação agora.

## 4. Ferramentas de diagnóstico e autoria

Use Inspector e Gizmos simples, evitando criar uma EditorWindow completa. Exponha, em Play Mode, NPC/operation ID, estado, destino solicitado e efetivo, caminho/cantos quando disponíveis, status do caminho, velocidade, tempo sem progresso, último motivo de falha e cancelamento. Diferencie dados configuráveis de dados runtime; não grave estado temporário no prefab.

Mostre os pontos de aproximação, raios/tolerâncias e caminhos quando selecionado. Use logs estruturados apenas em transições e falhas relevantes, com IDs de correlação. Evite log por frame. Inclua instruções para configurar um NPC existente e iniciar/cancelar um percurso de teste sem editar C#.

Exponha os dados necessários à futura animação de locomoção (velocidade, direção/estado) sem implementar ou substituir Animator Controllers. Animação ausente não impede testar navegação, mas deve ficar explicitamente registrada; NPC deslizando não significa animação validada.

## 5. Cena ou harness de validação

Crie uma demonstração pequena e removível, de preferência em cena/área de teste existente, sem povoar a ilha definitiva. Reaproveite prefabs de NPCs disponíveis; se indisponíveis, use agentes/cápsulas provisórios identificados apenas como fixtures. Não gere assets pelo Tripo AI, não use Blender para esta entrega e não consuma créditos externos.

Roteiro mínimo:

1. NPC A sai de um ponto de residência externo e chega à praça.
2. NPC A percorre uma sequência de pontos de uma região de demonstração.
3. NPCs A e B se deslocam para aproximações distintas de um banco real, quando disponível; NPC C cruza suas trajetórias ou segue outro ponto.
4. Cancele um movimento em andamento e inicie um novo destino; comprove que o cancelamento anterior não interfere na nova operação.
5. Envie um NPC a um ponto inalcançável, bloqueie uma rota de teste e verifique falha/cancelamento sem travamento infinito.
6. Se houver rampa/escada/cais utilizável, valide a travessia e registre quais locais foram realmente testados.
7. Desative e reative um NPC; confirme que ele não mantém uma operação inválida e que o harness/consumidor limpa reservas próprias.

Use relógio/agenda real apenas se a integração já estiver disponível e estável. O objetivo desta feature é demonstrar navegação, não uma rotina diária completa. Não reimplemente o harness de agenda; teste a camada separadamente e execute a regressão dos núcleos anteriores.

## 6. Testes e critérios de aceite

Crie testes focados conforme a estrutura existente (Edit Mode para lógica e validação; Play Mode para NavMesh, agente e movimento físico). Não use fakes como evidência de navegação real. Se necessário, use uma cena de teste determinística com NavMesh pequeno para os testes automatizados. Não introduza um framework de testes novo sem justificativa.

| ID | Cenário | Resultado esperado |
|---|---|---|
| NAV-01 | Destino alcançável | Agente percorre caminho completo e chega dentro da tolerância configurada |
| NAV-02 | Agente já no destino | Sucesso somente se a posição atual realmente satisfizer os critérios |
| NAV-03 | Caminho pendente / solicitação nova | Nenhum sucesso prematuro nem uso de estado residual do caminho anterior |
| NAV-04 | Destino inalcançável, parcial ou inválido | Falha explícita; nunca chegada falsa ou teletransporte |
| NAV-05 | Agente desativado ou fora do NavMesh | Falha segura com diagnóstico, sem exceções recorrentes |
| NAV-06 | Cancelamento em movimento e cancelamento repetido | Agente para; operação antiga não afeta nova operação |
| NAV-07 | Obstrução / ausência de progresso | Timeout/falha em prazo limitado, sem loop infinito |
| NAV-08 | Wander com pontos válidos e sem pontos válidos | Deslocamento entre pontos autorizados ou falha explícita |
| NAV-09 | Três NPCs simultâneos | Movimento independente; sem travamento permanente ou disputa por controle |
| NAV-10 | Aproximação de Smart Objects | Chegada em slots diferentes; reservas preservadas e limpas pelo consumidor de teste |
| NAV-11 | Desativação, destruição ou troca de cena | Operações e callbacks encerrados; sem estado obsoleto |
| NAV-12 | Pausa e retomada | Sem timeout artificial quando pausado segundo a política definida |
| NAV-13 | Escada/rampa/cais, se disponíveis | Travessia real validada ou bloqueio documentado por local |
| NAV-14 | Regressão | Testes de Smart Objects, Schedule e Activity Core não apresentam novas falhas atribuíveis à feature |

Critérios obrigatórios para declarar a navegação validada: compilação sem novos erros na Unity 6, testes relevantes executados, movimento real em Play Mode, chegada e cancelamento comprovados, casos de falha exercitados, três NPCs de demonstração e nenhuma regressão observada nos controles dos protagonistas. A validação de aproximação de Smart Objects reais é um marco separado caso não esteja disponível; reporte explicitamente. Não declare integração completa se houver qualquer cenário essencial bloqueado.

## 7. Fora do escopo

- Implementar ou refatorar Schedule Core, Activity Core ou Smart Objects.
- Criar NPC Controller definitivo, World Clock Adapter ou Location Registry geral.
- Implementar Animator, blend trees, IK, root motion, sentar/levantar ou equipar ferramentas.
- Criar interiores, navegação entre cenas/portais, natação, escalada, saltos ou travessias especiais.
- Distribuir moradores e mobiliário por toda a ilha; refazer terreno, casas ou escala do cenário.
- Criar sistema próprio de pathfinding, ECS/DOTS, comportamento de multidão, GOAP ou Behavior Trees.
- Refatoração ampla, alterações estéticas, geração de assets ou introdução de dependências comerciais.

## 8. Entrega e evidências

Ao finalizar, apresente relatório compacto com:

- Estado real das dependências, arquivos criados/alterados, cenas/prefabs/asset NavMesh modificados e motivo de cada alteração importante.
- Contrato de navegação efetivamente implementado e como o Activity Core poderá consumi-lo; indique se o adaptador foi conectado ou apenas preparado e por quê.
- Instruções concretas para adicionar o componente a um NPC, configurar destino/região, iniciar o teste e interpretar o diagnóstico.
- Comandos e resultados reais dos testes, contagem de aprovados/falhos/ignorados, resultado da compilação Unity e testes de regressão; se RTK falhar, registre comando nativo equivalente.
- Evidências do Play Mode: logs/transições com IDs e resultado de chegada/cancelamento; capturas ou gravações quando viáveis, com caminhos dos artefatos. Uma captura estática sozinha não comprova movimento.
- Matriz NAV-01 a NAV-14 com `PASS`, `FAIL`, `BLOCKED` ou `NOT RUN` e evidência curta. Diferencie implementação, compilação, teste simulado, teste físico e validação integrada. Não invente evidências.
- Pendências e contratos para a próxima feature `npc-animation-system`: dados de velocidade/direção, autoridade sobre movimento/rotação, aproximação versus pose final e política de interrupção.

**Condição de término:** entregar uma camada de navegação reutilizável, configurável, diagnosticável e demonstrada na Unity quando o ambiente permitir. Não antecipar as features seguintes para encobrir dependências ausentes. Se a Unity ou os contratos necessários estiverem bloqueados, conclua o que puder sem duplicar sistemas e reporte exatamente o que ainda não foi validado.
