# Relatório — `npc-integration-validation`

**Status da feature: PARCIAL.** A integração operacional observada é suficientemente estável para avançarmos para `npc-debug-tools`, mantendo as lacunas visuais e de falhas isoladas abertas. Isto não significa que todos os critérios V01–V35 foram aprovados, nem dá aceite visual à pose sentada.

## Decisão e resumo

Três moradores executaram rotinas JSON autonomamente na cena real `Ilha_Principal`: deslocaram-se entre praça, praia e casas, reservaram assentos, sentaram, receberam mudanças de intenção e liberaram recursos. Uma execução de Play Mode passou de Day 1 10:30 para além de Day 12 14:20 (mais de 11 dias de jogo) com o relógio real da cena em ciclo de 96 s por dia; houve também um smoke com ciclo de 600 s/dia para observar uma ocupação simultânea mais longa. Isso é movimento/animação físicos, não o harness lógico.

No smoke focado, Ivo e Helena reservaram slots diferentes do banco oeste e Luana usou um slot do banco leste. Os três alcançaram `wait` depois de `move_to`, `align_to`, `occupy_slot` e `play_animation`. Na janela longa, 27 eventos de reserva tiveram seus 27 eventos de liberação correspondentes; não se observou claim residual no estado final capturado. A cobertura de saturação dos slots disponíveis veio do teste anterior W09, que desativou o banco leste durante o teste: os dois slots oeste foram ocupados e o terceiro NPC falhou finitamente sem reserva.

No limite de uma atividade sentada, Ivo emitiu `residual_cleanup_pending`; cerca de 1,03 s depois, o retry de cleanup confirmou a saída, liberou o recurso e iniciou a intenção vigente. O Validador conferiu o fluxo no código e classificou como espera transitória segura, sem P2 funcional confirmado.

O Game Designer recomenda avançar para `npc-debug-tools`, mantendo pendente a revisão próxima da pose, orientação e repetição da animação sentada. A captura Scene View produzida nesta sessão é ilegível para avaliar os NPCs e foi descartada. O relatório W091 registra uma pose elevada parecida com queda, facing pendente e `Sitting_Idle_Loop` com `loop=false`; esses pontos não receberam aceite visual aqui.

## Baseline

- Branch: `093-npc-integration-validation`, criada sobre a branch local não integrada `092-npc-world-integration` para preservar o trabalho existente.
- HEAD inicial: `90c6b2744100bc7e293650465128cebd49122103`.
- Unity: `6000.3.25f1`, Built-in. Editor principal único; estado final `ready`, Play Mode parado, sem compilação em curso.
- Cena ativa: `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`.
- Inicialmente já existiam itens não rastreados `.codex-attachments/...` (anexo fornecido) e `prompts/`; foram preservados. O diff final contém apenas dois scripts de lifecycle e um teste de regressão; nenhuma cena, JSON ou configuração de projeto foi alterada. Os artefatos desta validação estão em `specs/093-npc-integration-validation/` e este relatório na raiz.
- O relógio temporário foi restaurado e salvo com `hour=10.5`, `cycleSeconds=96`, `advanceClock=false`; `Time.timeScale=1`. Não houve mudança persistente na cena. Nenhum commit, push ou merge foi feito.
- Driver de produção `/Waypoints da ilha/NPC World Integration`: clock `/Coastal Sun`, três hosts únicos `world_ivo`, `world_helena`, `world_luana`, JSONs individuais e registry/ports/bindings reais. Smart Objects observados: `npc_world_bench_west` e `npc_world_bench_east`, dois slots `sit` cada. As posições e referências foram lidas da cena aberta; não foram usados NPCs fakes para a prova física.
- Unity MCP inicialmente teve falhas de transporte; o Editor existente foi recuperado pela Unity CLI/MCP, sem iniciar outra instância. Versão CLI observada: 1.0.1; atualização 1.1.0 disponível e não instalada.
- Console após a correção e as regressões: `compilationFailed=false`, `compiling=false`, 0 erros e 2 warnings atuais (timeout da Account API do Unity AI Toolkit e diagnóstico esperado de ID SmartObject duplicado em teste). Antes do fix, parar Play Mode com três NPCs sentados produziu `animator_unavailable`; após a correção, teardown e regressões terminaram sem erros atuais. O buffer histórico reteve erros temporários de transporte MCP e falhas de compilação do novo teste durante sua autoria; foram resolvidos antes da execução final.

## Testes automatizados

| Ambiente | Comando/execução | Resultado |
|---|---|---|
| .NET | `rtk dotnet run --project Tools/NpcScheduleSimulator -- --test` | PASS: 174 assertions. |
| .NET | `rtk dotnet run --project Tools/NpcScheduleSimulator -- --validate` | PASS: 5 schedules, 15 atividades, 3 overrides, 3 receitas. |
| .NET lógico | `--simulate-week`, duas execuções iguais | 7 dias, 3 NPCs fake, 1.047 eventos, 2 falhas simuladas, 19 cancelamentos, 16 reservas/16 liberações, 0 claims residuais, `physicalValidation=false`. SHA-256 das duas saídas: `3d87465176c8e8d9d7c5a1d2d2031fc80822abe80e163203ddf5f7049c9b9257`. Não prova execução física. |
| Unity Edit Mode | `Freyja.SmartObjects.EditModeTests` | 12 passaram, 0 falharam, 0 ignorados. |
| Unity Edit Mode | `Freyja.NpcAnimation.EditModeTests` | 12 passaram, 0 falharam, 0 ignorados. |
| Unity Play Mode | Todas as suítes descobertas | 8 passaram, 0 falharam, 0 ignorados: 6 Smart Objects, 1 Animation, 1 WorldBuilder. Inclui ocupação de slots distintos, rejeição do terceiro pedido, fallback para outro banco e cleanup de caminho inacessível. |

Total na regressão final: 49 testes Unity (40 Edit Mode + 9 Play Mode), todos passaram. O harness .NET passou com 174 assertions antes da correção de adapter/controller; essa mudança Unity não altera o core/harness. Não foram encontrados assemblies Unity de teste para Schedule, Activity Execution, Navigation ou World Integration; portanto os 49 testes não são apresentados como cobertura completa dos seis sistemas.

## Execução física e desempenho

### Play Mode

- Longa duração: três NPCs na cena principal, início em Day 1 10:30, relógio de cena em avanço automático, `cycleSeconds=96`, `Time.timeScale=1`. O runtime passou de Day 12 14:20 (mais de 11 dias completos de calendário de jogo) e ciclos de idle, wander e sit foram registrados. A duração exata de parede não foi instrumentada; consulte [evidências resumidas](specs/093-npc-integration-validation/evidence/physical-playmode.md).
- Concorrência física: no smoke focado com relógio a 600 s/dia, as três rotinas agendadas entraram em `sit`; Ivo e Helena usaram slots separados de West, Luana usou East. Os logs correlacionados mostram reservas, chegada, alinhamento, ocupação, animação e espera. O snapshot anterior registrou três claims distintos e posições nos assentos.
- Mudança de horário: na janela de 13:00, cancelamentos substituíram a execução de sit/wander e a intenção atual foi iniciada; o recurso permaneceu reservado enquanto a saída segura terminava e foi liberado pelo cleanup retry. Um hook de validação interrompeu Ivo e retomou o compromisso vigente.
- Tempo: em `Time.timeScale=0`, hora de jogo e `Time.timeAsDouble` ficaram parados durante espera não escalada; em `Time.timeScale=2`, ambos avançaram em proporção à escala. Não foi feita revisão visual isolada de Animator e timeouts em todas as escalas.
- Não foi testado nesta sessão um caminho físico bloqueado por obstáculo nem destruição/disable de GameObject durante ação. Esses casos seguem parciais, reaproveitando apenas os testes isolados indicados na matriz e no W01–W19.
- A imagem [playmode-day2-morning-travel.png](specs/093-npc-integration-validation/evidence/playmode-day2-morning-travel.png) é uma captura do Game View do cenário/personagem e HUD; não prova pose ou locomoção dos NPCs. A captura Scene View de assento não estava enquadrada de modo legível e não foi mantida como prova.

### Unity Profiler

Capturas no Editor, três NPCs, `Time.timeScale=1`. Amostras: movimento 100 frames, idle 20, três NPCs em assentos simultâneos 20. O ciclo de 600 s/dia foi usado na captura de assentos. Median CPU: 4,627 ms (movimento), 4,448 ms (idle), 4,438 ms (assentos). GC Alloc mediano: 12.345, 12.305 e 12.305 bytes/frame, respectivamente. Nenhuma amostra ultrapassou a referência de 16,667 ms nesses recortes; não há budget oficial aprovado. Resumo por amostra em [profiler-summary.md](specs/093-npc-integration-validation/evidence/profiler-summary.md).

As capturas são do Editor e não estimam desempenho final de build. Não há contador de requests NavMesh/path exposto pela ferramenta nesta configuração. Memória do Editor variou aproximadamente entre 4,34 e 4,35 GB residente, sem atribuição específica aos NPCs; heap Mono ficou perto de 1,3–1,4 GB. Esses valores não justificam otimização nesta etapa. Dumps binários de Profiler foram removidos após sumarização.

## Matriz V01–V35

| ID | Ambiente | Status | Observado e evidência |
|---|---|---|---|
| V01 | Unity Editor/Play Mode | PASS | Unity 6000.3.25f1, cena principal aberta, referências reais dos três hosts; após correção, recompile limpa, `compilationFailed=false` e 0 erros atuais no Console. Os erros `animator_unavailable` anteriores ao fix estão documentados em V27. |
| V02 | .NET, Edit Mode, Play Mode | PARTIAL | 174 assertions, 24 Edit Mode e 8 Play Mode passaram. Não existem suítes Unity descobertas para Schedule, Activity, Navigation e World Integration. [Resumo de testes](specs/093-npc-integration-validation/evidence/test-summary.md). |
| V03 | Play Mode | PASS | Iniciou em 10:30 com a intenção válida corrente, sem reproduzir atividades anteriores; W04 e run físico da cena. |
| V04 | Play Mode | PASS | Três IDs/hosts únicos seguiram seus JSONs e destinos próprios durante mais de 11 dias físicos; logs/snapshots do runtime e W02/W17. |
| V05 | Play Mode/JSON | PASS | W16 mudou `world_ivo.json` de `beach` para `village_square`, recarregou pelo mecanismo existente, observou o destino e restaurou o fixture sem edição C#. |
| V06 | .NET/Play Mode | PARTIAL | Testes cobrem IDs duplicados e binding ausente; W03 verificou referências/host duplicado. Sintaxe JSON malformada e isolamento de JSON inválido por NPC não foram testados. |
| V07 | Play Mode | PASS | Relógio físico cruzou meia-noite, incrementou dia e seguiu a intenção vigente; W17/T019 e run prolongado. |
| V08 | Play Mode/.NET | PASS | Várias fronteiras de agenda substituíram execuções sem tempestade por frame; semana lógica repetível sem claims residuais. |
| V09 | Play Mode | PARTIAL | Pause e escalas 0/2 verificadas para relógio e tempo escalado; não isolamos animação/timeout durante pause em todas as etapas. |
| V10 | Play Mode | NOT RUN | Não há condição/prioridade existente ativa nas rotinas da cena que permitisse validar alternativa condicional sem inventar fixture. |
| V11 | Play Mode | PARTIAL | Idle em ponto de casa e posição próxima foram aprovados em W05/T019; nesta sessão a rotina longa incluiu idle, mas não foi feito snapshot próximo de cada chegada para revalidar a tolerância. |
| V12 | Play Mode | PASS | Caminhos físicos para praça e praia observados nos três residentes; chegadas concluídas. W06/T019 e run prolongado. |
| V13 | Play Mode | PARTIAL | Rotas existentes da praça e praia funcionaram; escadas/rampas específicas não foram mapeadas nem validadas nesta rodada. |
| V14 | Play Mode | PARTIAL | Três NPCs se movimentaram simultaneamente durante vários ciclos; não foi isolado cruzamento estreito com colisão/deadlock instrumentados. |
| V15 | Play Mode/harness | PARTIAL | Approach NavMesh inválido/destino inativo falhou finitamente em W13; não foi produzido nesta rodada um caminho parcial invalidado em marcha. |
| V16 | Play Mode | PARTIAL | Watchdog de agente parado já coberto no teste isolado de navegação; obstáculo físico controlado não foi colocado na ilha. |
| V17 | Play Mode/Animation | PARTIAL | Movimento e animações foram executados; suite de animação passou. Sem captura próxima legível no Game View para avaliar foot sliding, transição ou facing no mundo real. |
| V18 | Play Mode | PASS | W07/T019 registraram reserva, aproximação, alinhamento, sentar, pose, levantar, saída e liberação; nesta sessão três execuções reais chegaram a `wait` sentadas. Revisão visual próxima continua fora do aceite. |
| V19 | Play Mode | PARTIAL | Três NPCs tiveram claims/slots distintos, dois em West e um em East; pose/clipping visual dos dois no mesmo banco não foi avaliado em captura legível nesta sessão. |
| V20 | Play Mode | PASS | Terceiro residente usou o banco East enquanto os dois slots West estavam reservados; suite Play Mode também aprovou fallback de banco alternativo. |
| V21 | Play Mode | PARTIAL | W09 desativou East, ocupou os dois slots West e o terceiro falhou finitamente sem claim. Disponibilidade posterior após reativação não foi verificada neste cenário de saturação. |
| V22 | Play Mode | PARTIAL | W09 desativou East durante o teste, mas não durante aproximação/uso. Invalidação em trânsito/ocupação continua sem teste. |
| V23 | Edit/Play Mode | PARTIAL | Binding de animação ausente rejeitado em teste de adapter/W03; falta fixture de receita JSON real que exercite falha de animação/alinhamento e cleanup completo. |
| V24 | Play Mode | PASS | Mudança natural de agenda durante movimento/sit cancelou a execução substituída; o NPC iniciou a intenção vigente e a reserva foi liberada após saída segura. |
| V25 | Play Mode | PARTIAL | Cancelamento de sit com cleanup e liberação após ~1,03 s foi observado uma vez. `sit_down`, `sit_idle` e `stand_up` não foram forçados e testados separadamente nesta rodada. |
| V26 | Play Mode | PARTIAL | Hook `InterruptNpcForValidation` produziu cancelamento/retomada física da intenção vigente; início/fim de diálogo real não está integrado. |
| V27 | Play Mode | PARTIAL | Reproduzido: desativar o GameObject de Ivo sentado liberava Owner/slot, mas ao reativar o controller ficava bloqueado em `Cancelled residual=true`. Fix aplicado e smoke físico passou: o slot ficou Available, dois claims alheios permaneceram ocupados, e após reativar Ivo resolveu `sit`, entrou em `Executing` e reservou novamente. O teste isolado `NpcGameObjectDisableClearsAdapterStateForReactivation` também passou. Destroy físico do NPC em movimento não foi testado; lifecycle de owner/objeto é coberto separadamente. |
| V28 | Play Mode | PASS | W18 reiniciou Play Mode e recompôs três hosts únicos, sem claims residuais/duplicados. Não refeito após o run desta sessão, sem alteração de código. |
| V29 | Harness/Play Mode | PARTIAL | Tokens atrasados e substituição normal cobertos por testes anteriores; sequência rápida de alterações JSON/agenda em produção não foi reproduzida. |
| V30 | Play Mode, ilha real | PASS | Mais de um dia completo foi observado fisicamente, incluindo idle, wander, sit e retornos; run alcançou mais de 11 dias de jogo com ciclo 96 s/dia. |
| V31 | Harness .NET lógico | PASS | Semana determinística executada duas vezes com hash idêntico, 0 claims residuais; falhas `path_unreachable` e `animation_rejected` são simuladas, não físicas. |
| V32 | Play Mode físico | PARTIAL | Mais de 7 dias físicos completos foram observados com deslocamento/animações reais e relógio contínuo. A duração exata de parede não foi anotada e o registro de endpoints da sessão é agregado; não usar a semana lógica como substituto. |
| V33 | Unity Profiler/Diagnostics | PARTIAL | Medidas idle, movimento e contention (três assentos) registradas; sem requests NavMesh expostos nem perfil de build/atribuição de memória dos NPCs. |
| V34 | Game View/Play Mode | PARTIAL | Cena e HUD foram capturados e clock avançou; não houve teste direto de input jogável, câmera com interação, controles ou interação do protagonista nesta validação. |
| V35 | Edit/Play Mode/.NET | PASS | Após a correção Unity: 40 Edit Mode + 9 Play Mode passaram; recompile e Console finais limpos. Harness .NET 174 assertions passou antes do fix e não foi afetado pela alteração Unity. Smoke físico do caso afetado passou. |

## Defeitos, correções e pendências

**P1 corrigido: NPC não retomava após desativação do GameObject sentado.** Reprodução: no Play Mode com Ivo sentado, desative o GameObject inteiro e reative-o. O `SmartObjectReservationOwner` liberava a reserva, mas `NpcActivityController` preservava coordinator residual enquanto `NpcUnityActivityPorts` retinha operações efêmeras; após reativação, a execução ficava cancelada com `animation_operation_in_progress`, sem retomar a agenda. Correção: o caminho de `OnDisable` quando o GameObject inteiro sai da hierarquia limpa o estado efêmero do adapter e descarta coordinator/resolver/intenção; desativar só o componente continua usando safe exit normal. Após fix, o smoke real preservou os dois claims dos outros NPCs, liberou o slot do Ivo e retomou a agenda com nova reserva. `NpcGameObjectDisableClearsAdapterStateForReactivation` passou 1/1.

O stop de Play Mode com os três NPCs sentados, antes do fix, também emitiu `animator_unavailable`; após a correção, o teardown terminou com 0 erros atuais. No limite normal de agenda, o retry `intent_replaced` permaneceu seguro: conservou slot durante a saída, liberou-o e iniciou a intenção vigente.

Arquivos alterados:

- `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcActivityController.cs`
- `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcUnityActivityPorts.cs`
- `Freyja/Assets/_Project/SmartObjects085/Tests/PlayMode/SmartObjectLifecycleTests.cs`

Pendências abertas: pose/facing/loop de assento sem aceite visual próximo; JSON malformado; destino inacessível em marcha e bloqueio por obstáculo físico; troca de objeto/slot durante ação; falha de animação/alinhamento via receita real; saturação com posterior recuperação; destruição/unload de NPC durante movimento e desativação enquanto caminhando; interrupção de diálogo; mudanças rápidas de agenda; input e interação do protagonista; medidas em build e contador de path requests.

## Reprodução

1. Abrir `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` e entrar em Play Mode. A cena salva está restaurada em 10:30, auto clock desligado, ciclo de 96 s.
2. O driver está em `/Waypoints da ilha/NPC World Integration`; selecione Ivo Batista, Helena Batista e Luana Batista em `/Island023/Moradores060/`. Cada controller referencia o provider e registry reais.
3. Os TextAssets JSON individuais ficam em `Freyja/Assets/_Project/NpcScheduling/Data/npcs/world_integration/`: `world_ivo.json`, `world_helena.json`, `world_luana.json`. A agenda seleciona praça, janela de assento nos bancos e praia em horários diferentes. Para recarregar JSON, use o fluxo documentado de AssetDatabase/Play Mode existente; não edite C#.
4. Os dois Smart Objects e slots estão na praça; acompanhe `NPC_ACTIVITY`, `ResourceReserved`, `ResourceReleased` e o campo `runtimeStatus` do driver. O smoke simultâneo anterior usou `cycleSeconds=600` somente em execução temporária e restaurou o projeto ao final.
5. Reexecutar harness: `rtk dotnet run --project Tools/NpcScheduleSimulator -- --test`, `--validate` e `--simulate-week`. Unity: Test Runner > EditMode/PlayMode, Assemblies `Freyja.SmartObjects.*Tests` e `Freyja.NpcAnimation.*Tests`.

## Artefatos e tarefas Spec Kit

- [Spec](specs/093-npc-integration-validation/spec.md), [plano](specs/093-npc-integration-validation/plan.md), [tarefas](specs/093-npc-integration-validation/tasks.md) e requisitos fonte do anexo.
- [Resumo de testes](specs/093-npc-integration-validation/evidence/test-summary.md), [execução física](specs/093-npc-integration-validation/evidence/physical-playmode.md) e [métricas do Profiler](specs/093-npc-integration-validation/evidence/profiler-summary.md).
- [Captura Game View de viagem](specs/093-npc-integration-validation/evidence/playmode-day2-morning-travel.png) é contexto do cenário/HUD, não prova de pose de NPC.
- A branch continua local e separada. Como a matriz mantém itens PARTIAL/NOT RUN, a feature não foi marcada como aceite integral nem integrada à `main`.

**Gate seguinte:** avançar para `npc-debug-tools` é aceitável para a estabilidade operacional observada, mantendo revisão visual do assento e falhas de lifecycle como pendências explícitas. Não avançar para novas atividades ou expansão populacional com base apenas nesta validação.
