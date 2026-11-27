# Freyja — NPC Activity Execution Core

Feature: `npc-activity-execution-core`  
Objetivo: transformar as intenções do Schedule Core em atividades com ciclo de vida verificável, usando C# puro e adaptadores simulados. Esta entrega não implementa a execução física na Unity.

## 1. Contexto e limites

Freyja é um jogo de mistério em uma ilha pequena, com três protagonistas jogáveis e rotinas de moradores que futuramente servirão de base a pistas, testemunhos e eventos narrativos. As duas entregas anteriores são `smart-objects-foundation` e `npc-schedule-core`.

O relatório da última entrega informa: pendem importação e geração de `.meta`, compilação no Unity 6, mapeamento dos destinos para locais reais e qualquer executor físico. Nenhuma integração física ou de Smart Objects foi validada. Não houve merge nem push; existem alterações locais preexistentes; RTK falhou e os comandos foram executados via `dotnet` nativo.

- Não dependa da Unity aberta, Unity MCP, Blender, Tripo AI ou Mac do usuário. Não edite cenas, prefabs, animações, assets, NavMesh ou `.meta` gerados. Não invente GUIDs. Não altere os NPCs oficiais.
- Preserve o trabalho anterior. Não recrie Schedule Core, catálogo, relógio, simulador ou Smart Objects. Não crie um segundo sistema de reservas de produção; um fake em testes é permitido.
- O núcleo deve compilar sem referências a UnityEngine/UnityEditor. O executor de produção na Unity fica para uma próxima entrega.
- Não introduza GOAP, behavior trees, ECS/DOTS, linguagem de scripting, editor visual, framework genérico de workflows, IA via LLM em runtime ou dependências comerciais.
- A agenda representa intenção; uma atividade simulada não prova presença física, testemunho ou acontecimento narrativo real.

## 2. Fluxo de trabalho e preservação do checkout

1. Leia AGENTS.md, instruções aplicáveis, constituição e specs anteriores do Spec Kit. Inspecione apenas os pontos relevantes do código real: ActivityIntent, catálogo, resolver, simulador, testes, contratos de Smart Objects se acessíveis, versão de Unity, SDK e limites de assemblies. Registre `git status --short`, branch e um resumo das alterações preexistentes (não despeje diffs completos). Não faça reset, stash, clean, checkout destrutivo, merge ou push sem autorização. Não inclua alterações alheias em commits; não faça commit automaticamente nesta entrega.
2. Confirme que os arquivos do Schedule Core estão de fato presentes no checkout atual. Se não estiverem, procure apenas referências locais acessíveis de forma não destrutiva; não reconstrua o núcleo a partir de suposições. Se não puder acessar o contrato real, entregue o plano e os testes/contratos que não dependam dele, identifique o bloqueio e não declare a integração concluída. Se houver colisão com mudanças locais, preserve-as e trabalhe nas partes independentes.
3. Use Spec Kit conforme a configuração existente (specify/plan/tasks/implement e checks aplicáveis). Reutilize documentos existentes. Game Designer: comportamento, coerência de interrupções e escopo. Implementador: código, fixtures e testes. Validador: revisão independente de invariantes e execução dos testes. Use subagentes apenas se disponíveis; caso contrário, execute esses papéis sequencialmente e informe. Não instale novos frameworks de agentes.
4. Trabalhe em fatias: contratos e estados; operações e limpeza; integração com agenda; dados e harness; testes e documentação. Compartilhe paths, contratos, diffs pequenos e resultados. Evite leituras duplicadas, logs longos e chamadas a ferramentas indisponíveis. Use `dotnet` nativo se RTK estiver indisponível; não tente repará-lo nesta feature.
5. Implemente efetivamente se houver checkout e recursos suficientes. Nunca invente execução, aprovação ou resultados de testes. Ao final, informe arquivos alterados, comandos e status das pendências.

## 3. Arquitetura e fronteiras

```text
ScheduleResolver existente -> ActivityIntent
                                |
                   ActivityCoordinator (por NPC)
                                |
                   ActivityExecutionCore (state machine)
                       /                     \
           ActivityRecipe / Steps       Execution ports
                                             |
                              Fakes determinísticos nesta etapa
                                             |
                 [futuro: Smart Objects, NavMesh, Animator, props]
```

Nomes sugeridos, adaptar aos contratos reais. O Schedule Core continua responsável por decidir *o que* fazer e quando. O executor controla *como* a atividade progride, sem reimplementar regras de prioridade ou resolução de agenda. O coordenador compara intenções, inicia, mantém ou interrompe uma execução. Um host de teste ou o futuro adaptador Unity fornece o tempo e chama Tick. Nenhum loop por NPC com threads ou tarefas em background. Não use relógio de parede, Thread.Sleep, Task.Delay, I/O no tick ou aleatoriedade não controlada.

Use a mesma fonte C# no harness e no futuro runtime Unity; não copie classes para um projeto .NET paralelo. Siga o padrão de compilação já usado no Schedule Core, com APIs e linguagem compatíveis com o Unity 6 do projeto (verifique a configuração disponível). Separe testes e artefatos de build dos assets importados. Aproveite asmdefs e serializador existentes; não faça reorganizações amplas.

### 3.1 Contrato de entrada e identidade

Inspecione `ActivityIntent` real: ID de NPC, atividade, destino lógico, origem/bloco/regra, parâmetros, prioridade e validade temporal (nomes e formatos reais podem divergir). Use esse tipo diretamente ou um adaptador fino, não uma segunda definição concorrente. Preserve trace e informações relevantes para diagnósticos.

Defina a identidade semântica de um compromisso: NPC, ocorrência do bloco (incluindo dia/intervalo absoluto, quando necessário), atividade, alvo, parâmetros e origem pertinente. Diferencie essa identidade da identidade única de cada tentativa de execução. Atualização de metadados/validade por nova resolução não deve reiniciar uma atividade equivalente; atualização de prazo deve ser respeitada. Mudança de destino, parâmetros semânticos ou ocorrência efetiva deve provocar transição conforme política explícita. Não use somente activityId, referência de objeto ou instante do tick como identidade. Trate a ocorrência que atravessa meia-noite sem reinício indevido.

### 3.2 Contratos de saída

Exponha snapshot de execução: NPC, intenção vigente, executionId, atividade, destino lógico, estado, operação atual, início e término, motivo de falha/cancelamento e eventual recurso reservado (diagnóstico). Distinga destino pretendido de destino efetivo fornecido pelo adaptador. Eventos de execução são distintos de eventos de resolução do Schedule Core.

Eventos mínimos: `activity_started`, `step_started`, `step_completed`, `activity_completed`, `activity_cancelled`, `activity_failed`, `resource_reserved`, `resource_released`. Nomes adaptáveis. Inclua tempo simulado, IDs de correlação, origem simulada quando aplicável e reason codes estáveis. Não exponha automaticamente informações ao jogador nem alimente sistema de pistas nesta feature.

## 4. Ciclo de vida e execução

Estados sugeridos: Pending, Preparing, Moving, Executing, CleaningUp, Completed, Cancelled, Failed. Um estado de limpeza pode ser interno se a semântica for equivalente. Estados terminais só devem ser publicados depois de finalizar o procedimento de limpeza ou registrar explicitamente uma falha de limpeza.

- Uma única execução ativa por NPC; operações concorrentes não podem provocar efeitos de duas atividades diferentes. NPCs distintos podem progredir independentemente e disputar recursos simulados.
- Operações têm início uma única vez, polling/tick de progresso, término explícito (sucesso/falha/cancelamento) e cancelamento/limpeza idempotente. Use operações com contrato simples e estados `Running`, `Succeeded`, `Failed` (ou equivalentes). Não crie uma classe/interface nova para cada detalhe sem necessidade.
- Tick recebe tempo monotônico explícito e snapshot de tempo do jogo. Separe, se necessário, tempo de duração operacional e calendário simulado: timeout de navegação não deve depender de DateTime.Now nem confundir minutos do jogo com segundos de execução. Documente como o simulador avança ambos.
- `wait` até o fim da agenda respeita a validade atualizada da intenção e não utiliza espera real. Se a janela vencer durante preparo, movimento ou animação, cancele operações pendentes e inicie limpeza. A agenda deve ser reavaliada no horário atual, não retomada cegamente.
- Intenção idêntica repetida não reinicia execução. Atividade terminal não é iniciada novamente em todo tick para a mesma ocorrência; use política explícita para atividade concluída antecipadamente, falha ou fallback. Um novo compromisso válido pode iniciar normalmente.
- Cancelamento externo (simulação de diálogo, remoção de NPC, evento) deve parar operações e limpar recursos. Defina política simples de interrupção: solicitação externa ou nova intenção efetiva substitui a anterior, e a próxima inicia após limpeza. Não implemente diálogos ou eventos físicos. Defina como remover o bloqueio e retomar a intenção corrente. Chamadas antigas/retornos tardios não afetam a nova execução; use executionId/tokens de operação.
- Avanço de relógio, inclusive saltos de vários horários ou dias: considere somente a intenção atual, não execute retroativamente ações que nunca aconteceram. Rejeite ou trate explicitamente retrocesso/reset de tempo; nenhum loop infinito para recuperar ticks perdidos.
- Limite transições por tick para impedir receitas de operações instantâneas de travar o host. Defina resultados explícitos para alvo inexistente, indisponibilidade, caminho impossível, timeout, animação rejeitada, reserva invalidada e definição sem executor. Retentativas finitas com cooldown determinístico; não repita a mesma falha a cada frame. Não faça teletransporte ou simule sucesso falso.

### 4.1 Limpeza e ownership

Registre cada recurso efetivamente adquirido no contexto da execução (reserva, prop, operação de movimento/animação). Libere em ordem inversa quando pertinente, em conclusão, cancelamento, falha, timeout ou remoção do NPC. Uma falha em uma etapa de limpeza não impede as outras. Exponha diagnóstico de falha residual; não reporte recurso liberado sem confirmação do adaptador. Use uma política limitada e explícita para falhas de limpeza e não inicie nova atividade que possa colidir com recurso ainda possuído sem uma resolução segura.

O token de reserva deve ser opaco para o núcleo e vinculado ao dono/execução. Liberação repetida ou atrasada não pode liberar reserva de outro NPC ou de uma nova execução. Não substitua esses tokens por mero ID do banco. A operação de ocupação só ocorre após aproximação e alinhamento bem-sucedidos, segundo o contrato real de Smart Objects. Se o contrato real estiver acessível, documente o mapeamento; se não, mantenha o port abstrato e indique as suposições. Nenhum adaptador de produção de Smart Objects nesta etapa.

## 5. Operações primitivas e ports

Implemente apenas o necessário para `idle`, `wander` e `sit`. O catálogo pode reconhecer outras atividades, mas isso não significa que sejam executáveis.

| Operação | Contrato |
|---|---|
| `find_target` | Localizar candidatos para destino simbólico e capacidade exigida; ordenar deterministicamente (distância simulada e desempate por ID, se aplicável). |
| `reserve_slot` | Tentar reservar um candidato compatível, tentar alternativas limitadas quando ocupado. |
| `move_to` | Iniciar movimento simulado; acompanhar sucesso, falha, cancelamento e timeout. |
| `align_to` | Alinhamento simulado na pose do slot, sem depender de Transform. |
| `occupy_slot` | Confirmar ocupação após chegada/alinhamento, validar handle. |
| `play_animation` | Acionar uma animação simbólica e aguardar resposta simulada. |
| `wait` | Aguardar prazo de atividade ou duração configurada com tempo injetado. |
| `release_slot` | Liberar handle vigente; a limpeza é garantida mesmo se esta operação não for alcançada. |

Ports sugeridos: descoberta/reserva de interação; movimento (inclui seleção de pontos de caminhada por região, quando útil); pose/animação. Não crie uma interface para cada método se agrupar responsabilidades coesas for mais simples. `EquipProp`, pesca e mecânicas de uso de itens são extensões futuras, não implementar agora. Use identificadores/dados portáveis e não tipos Unity; evite implementar cálculo espacial real.

### 5.1 Fakes e fixtures

Fakes ficam na infraestrutura de teste/harness, nunca são selecionados silenciosamente em produção. Simule dois bancos com dois slots cada, regiões `village_square`, `beach` e `home` e três NPCs de demonstração. Use IDs estáveis fictícios, tempo de deslocamento configurável, falha de caminho e animação injetáveis, reserva invalidável e logs. O fake de reserva deve modelar exclusividade, ocupação e tokens com propriedade, sem se tornar uma segunda implementação de produção. Nenhum GameObject é criado.

## 6. Receitas de atividades e dados

Reutilize catálogo e IDs reais do Schedule Core. JSON continua fonte de verdade da configuração de agendas; receitas definem passos executáveis, não horários. Estenda o catálogo de forma compatível ou use um arquivo de receitas referenciado por activityId; não duplique metadados de atividades nem altere arquivos existentes sem necessidade. Use versionamento compatível e parser existente. Se uma atividade declarada não tiver receita, isso deve ser detectável e explicitamente tratado, não uma definição inválida por si só. Receitas são sequências lineares de operações tipadas/whitelist, sem scripts, expressões livres, condicionais arbitrárias ou loops genéricos.

Exemplo conceitual (adaptar ao schema real):

```json
{
  "schemaVersion": 1,
  "activityId": "sit",
  "steps": [
    {"action": "find_target", "capability": "sit", "target": "intent_target"},
    {"action": "reserve_slot"},
    {"action": "move_to", "point": "approach"},
    {"action": "align_to", "point": "pose"},
    {"action": "occupy_slot"},
    {"action": "play_animation", "animation": "sit_down"},
    {"action": "wait", "until": "schedule_end"},
    {"action": "play_animation", "animation": "stand_up"},
    {"action": "release_slot"}
  ]
}
```

A expiração durante `wait` representa término normal e autoriza a sequência de saída, enquanto expiração antes de chegar/sentar representa cancelamento. Distinguir saída normal de preempção: a nova intenção aguarda limpeza; a saída pode ter prazo limitado independente da janela expirada. Em interrupção durante a pose sentada, solicite restauração de pose/animação por port quando aplicável e libere a reserva; não dependa de alcançar o último passo. O fake só comprova chamadas e estados, não comprova animação real de levantar.

- `idle`: com alvo lógico, deslocar até local válido, depois aguardar; sem alvo, aguardar no local atual. Não exigir Smart Object.
- `wander`: localizar região; realizar um percurso simples simulado entre pontos autorizados até fim da janela, com intervalos explícitos e quantidade de transições limitada por tick. Implementar repetição apenas como comportamento desta operação/atividade, não como motor de grafos genérico. Sem reserva de banco. Se a região não existir, falha/fallback explícito.
- `sit`: sequência acima, com dois lugares independentes por banco e busca de alternativa. Falha em todos os candidatos -> fallback/retry limitado e diagnosticado.

Valide antes de executar: IDs de receita únicos e existentes no catálogo, operações reconhecidas, argumentos obrigatórios, prazos válidos, ordem coerente de reserva/movimento/ocupação/liberação e ausência de consumo de recurso não adquirido. Erros devem identificar arquivo/atividade/passo. Não considere `fish`, `sleep`, `eat`, `secret_meeting` ou outras atividades implementadas apenas porque existem no catálogo.

### 6.1 Política de fallback

Se não houver receita ou não houver destino disponível, registre a intenção original, motivo e execução alternativa `idle` quando seguro (sem forjar mudança de agenda). Evite recursão de fallback se `idle` também falhar; não reinicie a cada tick. Uma falha transitória pode ter novas tentativas limitadas dentro da janela vigente, com cooldown e configuração conservadora; novas intenções resetam o orçamento. Diferencie atividade pretendida, executada e fallback em todos os logs.

## 7. Harness integrado ao Schedule Core

Amplie o simulador existente; não crie outro simulador de agendas. Preserve o modo anterior de somente resolver agenda e seus testes. Adicione um modo de execução simulada com clock/ticks determinísticos, portas fake, receitas e resumo dos resultados. Se a estrutura real exigir outra organização, documente a decisão e reutilize as mesmas fontes.

Cenário obrigatório: três NPCs com agendas de teste que usam `idle`, `wander` e `sit`. Os dois primeiros devem reservar slots distintos do banco A e o terceiro um slot do banco B. A ordem de arbitragem entre NPCs no mesmo tick deve ser estável (por ID ou regra explícita). Exercite passagem para próxima atividade, cancelamento durante movimento, interrupção enquanto sentado, banco lotado, alvo removido, caminho inalcançável, timeout e retorno tardio. Não sobrescreva rotinas de demonstração anteriores: acrescente fixtures específicas quando necessário.

Simule sete dias com o catálogo e templates existentes quando disponíveis; atividades sem receita devem aparecer como não suportadas/fallback, não como sucessos de execução. Compare duas execuções com as mesmas entradas e seed, se usada: mesma sequência de eventos sem depender de GUIDs aleatórios ou relógio de parede nos resultados relevantes. Gere logs estruturados (JSONL ou formato existente) e resumo: intenções, atividades iniciadas/concluídas/canceladas/falhas, fallback, reservas adquiridas/liberadas e pendências. No término da simulação, encerre as execuções ativas e verifique ausência de vazamentos; diferencie esse shutdown de término normal. Documente comandos reais e como selecionar cenários. Não faça alegações de performance física da Unity a partir do harness.

## 8. Testes e critérios de aceite

Execute primeiro os testes anteriores disponíveis para estabelecer baseline e, ao final, a suíte relevante completa. Use `dotnet test`/comandos reais da infraestrutura existente. Se SDK, dependências ou rede estiverem indisponíveis, informe o comando tentado e a falha; não declare aprovação. Não desabilite testes para obter verde.

Testes obrigatórios:

1. Intenção real do Schedule Core consumida pelo executor; catálogo e resolução anteriores sem regressão.
2. Intenção equivalente em múltiplos ticks não reinicia atividade; atualização de validade é respeitada; mudança de alvo ou ocorrência inicia nova execução após limpeza.
3. `idle` sem alvo aguarda sem reservar; `idle` com alvo e `wander` usam movimento simulado; `sit` realiza sequência esperada, confirma ocupação após chegada e libera ao sair.
4. Dois NPCs reservam slots distintos do banco A; terceiro usa banco B; todos ocupados -> alternativa segura; não há dupla ocupação. Ordem determinística.
5. Cancelamento durante cada fase relevante (reserva, deslocamento, alinhamento, animação, espera) e mudança de agenda liberam recursos, inclusive quando `release_slot` ainda não foi alcançado.
6. Falhas de caminho, animação, timeout, alvo inexistente, reserva invalidada e exceções de adaptadores geram estados e diagnósticos corretos; cleanup tenta todos os recursos e expõe falhas residuais.
7. Handle inválido/antigo, callbacks tardios, cancelamento repetido e liberação repetida não alteram a execução nova nem os recursos de terceiros.
8. Expiração durante movimento cancela; expiração durante espera sentada finaliza com saída normal; atividade de maior prioridade preempta sem deixar reservas penduradas.
9. Atividade sem receita não reporta sucesso falso; fallback não altera intenção original; retry limitado; conclusão antecipada não gera reinícios incessantes; fallback não entra em recursão.
10. Meia-noite, mudança de dia, saltos de relógio e reset seguem política documentada; ações omitidas não viram eventos retroativos.
11. Validação de receitas rejeita operações desconhecidas, argumentos inválidos e sequências incoerentes; mensagens são úteis para autoria.
12. Sete dias simulados; reprodução determinística; nenhum recurso ainda reservado após shutdown normal; relatórios distinguem simulação, falha, cancelamento e fallback.
13. Limite de transições por tick e tentativas demonstrado; nenhuma dependência Unity no núcleo nem falso adaptador de produção.

## 9. Documentação e entrega

Entregue README atualizado com arquitetura, diagrama de fluxo, contrato real de ActivityIntent, decisões de identidade e tempo, estados, política de interrupção, cleanup, fallback e exemplos de autoria de receita. Explique como cadastrar uma atividade executável sem duplicar seu ID de catálogo e quando uma nova operação exige C#.

Crie um checklist objetivo da próxima etapa de integração Unity, sem implementá-la: importar arquivos e gerar `.meta`; compilar no Unity 6; confirmar Smart Objects existentes (IDs, reservas, ocupação, invalidação, lifecycle); mapear destinos lógicos para locais reais; adaptar clock, navegação, pose/animação e Smart Objects; verificar animações de sentar/levantar; validar com três NPCs e dois bancos em Play Mode; capturar evidências visuais; garantir que saves/troca de protagonista e diálogos não prejudiquem recursos (integrações futuras conforme escopo). Não marcar itens como aprovados nesta entrega.

Relatório final obrigatório:
- Resumo da implementação, decisões tomadas, contratos efetivamente encontrados e arquivos criados/alterados.
- Status Git antes/depois, alterações preexistentes preservadas, sem merge/push/commit automático.
- Comandos executados, número de testes aprovados/reprovados/não executados, caminhos de logs e resultado dos cenários de simulação.
- Distinção explícita entre testes C# reais, execução em fakes e validações Unity pendentes.
- Limitações do ambiente e riscos de integração; nenhuma alegação de que NPCs já se movimentam fisicamente ou sentam no jogo.

## 10. Definition of Done

A entrega está concluída no escopo lógico quando o executor usa o contrato real do Schedule Core, as três receitas funcionam no harness, as interrupções/falhas limpam recursos corretamente, a suíte relevante executa com resultados reportados, a simulação é reproduzível, e a documentação permite editar receitas e rodar testes. Se requisitos não puderem ser verificados, marque-os como pendentes e apresente evidências do que foi efetivamente realizado. Unity e Smart Objects físicos permanecem não validados. Não iniciar novos sistemas narrativos, profissões ou outras features nesta entrega.
