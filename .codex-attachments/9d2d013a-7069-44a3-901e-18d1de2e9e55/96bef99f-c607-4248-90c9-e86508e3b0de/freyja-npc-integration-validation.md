# Freyja — NPC Integration Validation

**Feature:** `npc-integration-validation`  
**Natureza:** estabilização, regressão e correção de defeitos; não é uma expansão de funcionalidades.  
**Ambiente:** Unity 6, Unity MCP e ferramentas locais disponíveis, sujeitos à confirmação.  
**Objetivo:** comprovar que três NPCs reais executam suas agendas JSON autonomamente na ilha, de forma estável, previsível e reproduzível, antes de expandir a biblioteca de atividades ou a população.

## 1. Contexto e resultado esperado

O Freyja é um jogo de mistério em uma ilha. Os hábitos dos moradores deverão servir à investigação, de modo que o sistema de rotinas precisa ser confiável e observável. Foram relatadas como concluídas as seguintes features:

- `smart-objects-foundation`;
- `npc-schedule-core`;
- `npc-activity-execution-core`;
- `npc-navigation-system`;
- `npc-animation-system`;
- `npc-world-integration`.

Uma entrega anterior, `npc-unity-integration-code`, foi solicitada durante a indisponibilidade do Mac; verifique se existem artefatos/adaptadores dessa entrega. Não presuma que sua implementação seja completa nem que deva coexistir com uma segunda camada de integração.

O usuário informou a conclusão das seis features, mas seus relatórios, código e evidências reais precisam ser conferidos. Não reimplemente a arquitetura. A principal entrega desta sessão é uma matriz de testes verificável, correções mínimas com regressões e uma baseline estável na Unity. Caso tudo já funcione, nenhuma alteração de código de produção é obrigatória.

## 2. Fluxo de trabalho, agentes e segurança do Git

1. Leia AGENTS.md, constituição e instruções locais do Spec Kit. Use o fluxo existente de spec, plano, tasks, validação e correção, dimensionado para uma feature de estabilização. Não instale outro framework.
2. Registre branch, HEAD, `git status --short`, alterações staged/unstaged e não rastreadas relevantes, versão do Unity e disponibilidade das ferramentas. Identifique mudanças preexistentes, inclusive de outras sessões. Não faça reset, stash, clean, checkout destrutivo, merge, commit ou push sem autorização expressa. Não sobrescreva trabalho concorrente. Se houver conflito, restrinja alterações à parte segura e reporte.
3. Localize as specs e relatórios anteriores, especialmente os casos W01–W19 do World Integration. Identifique cenas, prefabs, três NPCs de demonstração, JSON reais, relógio, contratos, testes, harness, logs e debugger existentes. Inspecione apenas os arquivos necessários. Registre evidências anteriores válidas e reexecute os fluxos críticos na versão atual.
4. Use os agentes existentes, se disponíveis: **Validador** lidera baseline, matriz, reprodução e reteste; **Implementador** corrige apenas defeitos confirmados; **Game Designer** revisa a coerência visual e previsibilidade das rotinas, sem propor expansão. Se não houver agentes configurados, desempenhe esses papéis sequencialmente. Não permita que agentes concorrentes operem sobre a mesma cena/editor ou os mesmos arquivos.
5. Trabalhe em ordem: baseline → testes automatizados → smoke test físico → casos de falha → execução prolongada → desempenho → regressão final. Para cada defeito, reproduza antes de corrigir, aplique a menor correção e reteste. Priorize P0/P1. Reúna evidências durante a execução; não gere grandes dumps de logs, nem screenshots repetitivos, nem reenvie arquivos completos aos agentes.
6. Não termine apenas com specs/tasks quando houver meios de validar. Se o MCP falhar, limite tentativas e utilize as ferramentas de editor/Test Runner/CLI existentes quando possível; não dispare outra instância Unity sobre um projeto aberto. Se RTK falhar, utilize dotnet nativo. Identifique bloqueios reais em vez de inventar APIs ou afirmar execução que não ocorreu.

## 3. Baseline obrigatória

- Confirme a presença e a integração dos seis sistemas no checkout atual. Compare contratos reais, assembly definitions, dependências e versões. Preserve a separação de C# puro e runtime Unity; não duplique código para compilar.
- Confirme importação de scripts e JSON e compilação Unity 6. Arquivos `.meta` devem ser gerados normalmente pelo editor. Não fabrique GUIDs nem edite YAML de cenas/prefabs à mão.
- Execute os testes existentes do Schedule, Activity, Smart Objects, Navigation, Animation e World Integration. Execute harness dotnet, EditMode e PlayMode conforme suportado. Descubra comandos/paths reais; não invente nomes de suítes. Registre testes existentes com falhas antes das correções, diferenciando falha preexistente de regressão introduzida.
- Abra o cenário real da ilha e confirme que os três NPCs de demonstração possuem IDs únicos, rotinas individuais, um host/executor ativo por personagem, referências válidas a clock, locations, navigation, animation e Smart Objects reais.
- Confirme pelo Inspector/logs que o caminho de produção não está usando mocks, fakes ou comandos manuais que substituam a agenda. Reuse a cena isolada existente para casos destrutivos e saturação; não modifique permanentemente o cenário principal para simular falhas.
- Verifique jogo base: protagonista jogável, câmera, input, interação e ciclo de dia/noite continuam operando. Registre o ponto inicial do profiler quando possível.

Se faltar um componente crítico, não reinvente o sistema; trate a configuração/correção mínima viável ou reporte o bloqueio. Não execute um teste físico dependente de uma compilação falha e declare sucesso apenas com os testes dotnet.

## 4. Contratos e invariantes a verificar

- Scheduler é a autoridade sobre intenção, horários e prioridades; Activity Core é a autoridade sobre a execução e seu ciclo de vida; o host conecta componentes; Smart Objects são a autoridade sobre reservas; Navigation e Animation possuem responsabilidades físicas já definidas.
- Cada NPC possui apenas uma execução ativa; uma intenção inalterada não gera reinício por frame. Compromissos com mesmo activityId mas destinos/intervalos diferentes devem ser distinguidos pelo contrato existente.
- Mudanças de agenda durante cleanup não podem iniciar uma atividade antiga. Ao terminar a interrupção, deve ser retomado o compromisso vigente, sem replay de compromissos saltados.
- Um slot tem no máximo um proprietário. A reserva não implica chegada nem ocupação física. Quando um NPC está sentado, o lugar permanece protegido até saída segura; quando o NPC é removido, a reserva deve ser liberada de acordo com seu ciclo de vida.
- Cancelar/falhar/desativar deve liberar recursos e remover callbacks/inscrições, mesmo em inicialização parcial ou falhas de adaptadores. Callbacks de execuções antigas não podem alterar a execução atual.
- Tempo do mundo e tempo operacional têm semânticas distintas. Pausa, aceleração e timeouts precisam respeitar o contrato documentado. Não use hora de parede ou espera real arbitrária para mascarar erros de simulação.
- O NavMeshAgent e o Animator não podem disputar autoridade sobre o transform. Handoffs de posição/rotação, alinhamento, retomada da navegação e root motion devem respeitar os contratos reais.
- Nenhuma chegada, pose, atividade concluída ou informação de investigação pode ser considerada real se apenas simulada. Rotina prevista, intenção, atividade física efetiva e resultado são informações distintas.
- JSON permanece fonte de verdade; não crie uma configuração duplicada em C#, ScriptableObjects ou cenas. Não introduza hot reload se não existir; valide edição pelo procedimento já documentado.

## 5. Matriz de testes integrada

Para cada teste registre: ID, ambiente (dotnet/EditMode/PlayMode/cenário real), precondições, passos, comportamento esperado, observado, status e evidência. Os casos que dependem de integração inexistente devem ser marcados BLOCKED; casos não executados, NOT RUN. Não confunda aprovação unitária com aprovação visual.

### A. Rotinas, dados e relógio

| ID | Cenário | Aceite |
|---|---|---|
| V01 | Importação e compilação Unity 6 | Sem erros de compilação; referências reais válidas; falhas anteriores registradas. |
| V02 | Regressão dos testes dos seis sistemas | Resultados e contagens reais registrados; nenhuma nova falha. |
| V03 | Iniciar em horário intermediário | Cada NPC resolve o compromisso atual, sem reproduzir horários passados. |
| V04 | Três rotinas diferentes | NPCs seguem seus respectivos horários e destinos; sem duplicação de host/execução. |
| V05 | Alterar horário ou destino em JSON | Mudança observada após o mecanismo de recarga já suportado; nenhuma edição C#. Restaurar o fixture original. |
| V06 | Dados inválidos, IDs duplicados, destino ausente | Diagnóstico acionável, sem fallback silencioso para sucesso, sem interromper NPCs saudáveis. |
| V07 | Passagem da meia-noite | Dia/calendário e compromissos sincronizados, sem duplicação do ciclo. |
| V08 | Avançar horário por várias horas e cruzar fronteiras consecutivas | Resolução do compromisso atual, cleanup da execução anterior, sem tempestade de reinícios. |
| V09 | Pausar e retomar; mudar escala de tempo | Comportamento de agenda, animação, movimento e timeout consistente com a política do jogo; sem corrida temporal. |
| V10 | Condição/prioridade de agenda existente | Se suportada, ativa alternativa e volta à agenda vigente; testar sem criar sistema climático ou narrativo novo. |

### B. Navegação, animação e Smart Objects

| ID | Cenário | Aceite |
|---|---|---|
| V11 | Idle em destino externo | Chega à residência/área prevista e aguarda; não caminha parado nem teletransporta. |
| V12 | Wander em praça e praia | Chega à região, percorre pontos válidos e respeita limites e política da navegação. |
| V13 | Rampas, inclinações e escadas existentes | Deslocamento pelos trechos realmente suportados do cenário, sem atravessar chão/obstáculos; mapear trechos não navegáveis como limitações. |
| V14 | Três NPCs cruzando caminhos | Sem colisão persistente ou deadlock; pequenas aproximações transitórias compatíveis com o projeto. |
| V15 | Destino inacessível/caminho parcial ou invalidado | Falha ou fallback explícito no limite definido, sem espera infinita ou falso Arrived. |
| V16 | NPC preso por obstáculo controlado | Recuperação/timeout dentro da política existente; nenhuma tempestade de tentativas. |
| V17 | Locomoção idle/walk e run se suportado | Transições coerentes com velocidade real, sem deslizamento de pés persistente, vibração ou root motion concorrente. |
| V18 | Sit completo, por agenda real | Reserva → aproximação → alinhamento → sentar → pose mantida → levantar → saída segura → liberação; comprovar cada marco. |
| V19 | Dois NPCs sentados no mesmo banco | Slots e poses distintos; nenhum clipping grave, sobreposição ou dupla reserva. |
| V20 | Terceiro NPC disputa banco de dois slots | Outro banco compatível ou fallback conforme regra existente; nunca ocupa lugar já reservado. |
| V21 | Todos os slots compatíveis saturados | Fallback/timeout limitado; disponibilidade posterior respeitada pela política atual; não criar fila global nova. |
| V22 | Smart Object ou slot desativado durante aproximação/uso | Cancelamento e cleanup seguros, sem referência destruída persistente ou ocupação fictícia. |
| V23 | Animação indisponível ou falha de alinhamento em fixture | Resultado de falha explícito, cleanup e retomada segura; nunca declarar que sentou se faltou clip ou pose. |

### C. Interrupções e ciclo de vida

| ID | Cenário | Aceite |
|---|---|---|
| V24 | Mudança de horário durante movimento | Cancela movimento anterior, libera recurso quando devido e segue compromisso atual. |
| V25 | Mudança de horário durante sit_down, sit_idle e stand_up | Testar as três fases separadamente. Saída segura conforme contrato; sem novo movimento antes da devolução da autoridade e sem liberar slot enquanto ocupado fisicamente. |
| V26 | Interrupção externa e retomada | Iniciar/encerrar diálogo real se hook existir. Caso contrário, testar interrupção pelo harness e registrar diálogo real como pendente; retomada do horário atual. |
| V27 | Desativar/destruir NPC durante movimento ou sentado | Recursos liberados, callbacks antigos ignorados e outros NPCs continuam saudáveis. |
| V28 | Reativar NPC e reinicializar cena/Play Mode | Sem hosts duplicados, inscrições residuais ou reservas órfãs; testar opções de domain reload existentes sem alterar configuração global desnecessariamente. |
| V29 | Repetir mudanças rápidas de agenda e cancelamentos | Apenas uma execução ativa por NPC; nenhuma conclusão atrasada modifica execução nova. |

### D. Estabilidade, cenário e desempenho

| ID | Cenário | Aceite |
|---|---|---|
| V30 | Um dia completo em Play Mode, na ilha | Os três NPCs completam as atividades suportadas da agenda: idle, wander e sit, com deslocamento/poses reais; retorno externo às residências. |
| V31 | Sete dias consecutivos em simulação lógica | Harness integrado registra compromissos, execuções, falhas e recursos; sem inconsistências, reservas órfãs ou deadlocks. |
| V32 | Sete dias em Play Mode acelerado | Ciclos físicos reais com tempo suficiente para terminar ações; se ambiente impedir duração adequada, executar período possível, registrar dias completos e marcar restante como parcial/BLOCKED. Não substituir pelo V31. |
| V33 | Profiler e diagnóstico | Capturar baseline com três NPCs e amostra durante movimento/contenda; CPU, GC, memória e frequência de resolução/path requests quando disponíveis; sem vazamento ou crescimento inexplicado. |
| V34 | Regressão dos protagonistas e cenário | Input, câmera (inclusive renderização existente), relógio e interações previamente funcionais permanecem funcionando. Sem alterações indevidas no mapa, materiais ou controles. |
| V35 | Regressão final após correções | Reexecutar suites relevantes, smoke físico e casos afetados; evidências correspondem ao código final. |

Os cenários W01–W19 anteriores devem ser mapeados aos V01–V35, reaproveitando testes existentes sem duplicá-los. Adicione apenas testes úteis que cubram lacunas ou protejam correções.

## 6. Como executar os testes de longa duração

Use o relógio e os controles de simulação já existentes. Não crie um relógio de produção paralelo. Registre: escala do relógio, timeScale quando aplicável, duração de cada execução real, dias/minutos simulados, número de NPCs e configuração de fixtures.

- **Smoke físico:** execute pelo menos uma sequência real de ida ao banco, sentar, levantar e saída usando relógio/agenda, com tempo para todas as animações e deslocamentos. Não pule a fase difícil por avanço brusco do relógio.
- **Um dia físico:** percorra a sequência completa dos três NPCs no cenário real, incluindo uma janela em que disputam slots. Se for necessário reduzir a escala do relógio em certos intervalos de teste, documente como. A agenda deve permanecer a origem das decisões; não dispare cada atividade manualmente.
- **Sete dias lógicos:** use o harness dotnet/ambiente já implementado e seed registrada quando houver aleatoriedade. Simule ticks, mudanças e falhas conforme capacidades existentes. Execute duas vezes o mesmo cenário/seed se o core promete determinismo; compare decisões e transições determinísticas, não coordenadas de pathfinding físicas ou timestamps de parede.
- **Sete dias físicos:** execute ciclos acelerados com tempo suficiente para completar deslocamentos e animações. Se um teste de sete dias não couber no ambiente, não deixe processos rodando indefinidamente nem declare sete dias validados por saltos de calendário. Registre quantos dias físicos completos foram comprovados, por que o restante não ocorreu e como reproduzi-lo. A aprovação integral permanece pendente até a cobertura exigida.
- **Avanço brusco:** teste separadamente, para verificar cancelamento e sincronização; saltos não contabilizam atividades físicas concluídas.

Colete, pelos mecanismos existentes ou um harness mínimo de teste: dia/hora, npcId, intenção/compromisso, execução, transições, destino, slot/owner, falha/fallback, tempo de espera e contagem de reservas. Registre acontecimentos significativos em vez de logs por frame. Métricas úteis: tentativas/conclusões/cancelamentos/falhas por atividade; duração de espera; NPCs presos; ocupação máxima por slot; reservas órfãs; reinícios da mesma intenção; exceções. Compare contagem e propriedade de recursos antes/depois de cada fixture. Não exija zero falhas intencionalmente induzidas; exija tratamento correto e ausência de falha não explicada.

## 7. Validação visual e evidências reais

Use Unity MCP e/ou ferramentas do editor disponíveis. Verifique Game View para aparência e Scene View/Gizmos para posições e paths. Não confunda editor fora de Play Mode com execução física. Valide com a câmera real do Freyja.

- Capture início, aproximação, pose sentada, levantamento e saída para pelo menos uma sequência real, com logs correlacionados por ID e horário. Imagens isoladas não provam continuidade; use observação de execução ou gravação curta quando disponível.
- Capture dois NPCs no mesmo banco e a alternativa do terceiro, incluindo dados de reserva; verifique clipping, orientação e ausência de sobreposição grave.
- Examine a locomoção em terreno plano e trechos inclinados realmente presentes, pés, velocidade, transições e altura relativa ao solo. Não implemente IK nesta entrega para corrigir problemas estéticos menores.
- Registre paths relativos dos screenshots/logs/test results e o passo para reproduzir. Prefira poucos artefatos úteis, mantenha-os no local de evidências já adotado pelo projeto, fora de assets de produção quando possível. Não versionar dumps enormes, caches ou gravações sem necessidade.

## 8. Performance: medir antes de otimizar

Use Unity Profiler e ferramentas existentes para amostras comparáveis, registrando hardware/ambiente de execução, editor/build, escala de tempo e número de NPCs. Compare períodos semelhantes: idle, deslocamento simultâneo e interação. Inspecione CPU main thread, GC Alloc, heap/memória, atualizações de agenda, busca de destinos e requests de navegação se expostos. Não transforme custo do Editor/MCP em métrica de desempenho do build final.

Não há budget de FPS/CPU formal confirmado para Freyja. Não invente um limite nem declare performance aprovada com base em um único frame. Registre medições e limitações. Corrija somente gargalos evidentes e diretamente atribuíveis à integração (ex.: parsing JSON todo frame, reemitir path request incessantemente, inscrições não removidas). Não implementar ECS/DOTS, LOD, jobs, pooling genérico ou sistema de simulação offscreen. Teste de 30 habitantes e metas de desempenho do build ficam para a expansão populacional; três NPCs não comprovam escalabilidade de toda a ilha.

## 9. Correções de defeitos e prioridade

Classifique os problemas encontrados:

| Severidade | Exemplos | Ação |
|---|---|---|
| P0 — bloqueante | Compilação falha, crash, corrupção de dados, regressão que torna o jogo inutilizável | Corrigir antes de prosseguir nos testes dependentes; se impossível, reportar bloqueio. |
| P1 — grave | NPC permanentemente travado, slot órfão, duas execuções/ocupações simultâneas, agenda incoerente, atividade física sempre impossível | Corrigir e criar teste de regressão; impede aprovação integral. |
| P2 — moderado | Pequeno desalinhamento recorrente, transição visual ruim, diagnóstico insuficiente | Corrigir se pequeno e diretamente relacionado; registrar o restante com reprodução. |
| P3 — melhoria | Polimento de animação, conveniência de autoria, novas capacidades | Registrar backlog; não implementar nesta sessão. |

Para cada defeito: ID, caso Vxx, passos mínimos, esperado/observado, evidência, causa confirmada ou hipótese claramente identificada, mudança efetuada, teste de regressão e resultado do reteste. Não esconder erro por catch vazio, desabilitar asserts, relaxar tolerâncias sem justificativa, aumentar timeouts arbitrariamente, desligar NPCs, teletransportar para o destino ou alterar a rotina para evitar uma falha. Não tratar ausência de animação como sucesso. Não refatorar subsistemas inteiros para corrigir um bug localizado.

## 10. Fora do escopo

- Novas atividades (`fish`, `collect_water`, `sweep`, `eat`, `sleep`, `chat`, agricultura e comércio).
- Editor visual de rotinas, timeline, debugger completo, ferramentas novas de autoria. Diagnóstico pontual necessário para reproduzir/validar defeitos é permitido; use as ferramentas existentes.
- Novas funcionalidades de diálogos, missões, possessão, confiança, pistas, testemunhos, save/load, interior de casas ou população definitiva da ilha.
- Novos frameworks de IA, reorganização arquitetural ampla, substituição do NavMesh, Animator ou Smart Objects, IK e polimento artístico geral.
- Comprar assets, baixar pacotes sem autorização, utilizar Tripo AI, modificar controles dos protagonistas ou reconstruir mapa/terreno.

## 11. Definition of Done e gate para a próxima etapa

A feature pode ser considerada integralmente concluída quando:

- [ ] Projeto compila na Unity 6 e os testes obrigatórios anteriores e novos possuem resultados registrados, sem regressões não resolvidas.
- [ ] Três NPCs executam agendas distintas em Play Mode, na ilha, com `idle`, `wander` e `sit` fisicamente reais e edição de JSON comprovada.
- [ ] Um dia completo físico e sete dias lógicos e físicos são comprovados (distinguindo cada ambiente e método).
- [ ] Contenda, saturação, interrupções, destinos inválidos e desativação são tratados conforme contratos; nenhuma reserva órfã, dupla execução ou ocupação indevida permanece.
- [ ] Não há P0/P1 abertos na fatia de rotinas, nem erros/exceções não explicados no fluxo normal. Falhas deliberadamente induzidas são distinguidas de bugs.
- [ ] Protagonistas e cenário não sofrem regressões; desempenho foi medido e limitações registradas.
- [ ] Evidências, comandos, configurações, resultados e mudanças finais estão documentados; fixtures temporários foram removidos/restaurados sem afetar mudanças locais preexistentes.

Se houver bloqueio ou cobertura incompleta, não declare conclusão integral. Informe status parcial, casos aprovados e pendentes, impacto e menor ação necessária para completar. Para próximos passos, não recomende começar expansão de atividades/população se os invariantes fundamentais ainda falharem. Eventuais P2/P3 não impeditivos podem ser documentados para revisão do usuário. Não altere critérios de aceite para marcar tasks feitas.

## 12. Relatório final obrigatório

Entregue um resumo objetivo no final da sessão e um relatório reproduzível em Markdown na pasta de specs/relatórios já usada pelo projeto (informe path real). Sugestão de estrutura:

1. **Status geral:** APROVADO / PARCIAL / REPROVADO, com justificativa e gate para próxima etapa.
2. **Baseline:** branch, HEAD, Unity, ferramentas, cenas, NPCs, dados e estado inicial do Git; testes anteriores e problemas preexistentes.
3. **Matriz V01–V35:** para cada caso, PASS / FAIL / BLOCKED / NOT RUN, ambiente, resultado observado, evidência, e indicação de reteste. Para casos com subcenários parcialmente cobertos, explicite cada um e não marque PASS global sem cobertura integral.
4. **Resumo de testes:** contagens separadas de dotnet, EditMode e PlayMode; duração, comandos reais e caminhos dos arquivos de resultado. Não somar execuções repetidas como novos testes independentes.
5. **Execução prolongada:** dias lógicos completos, dias físicos completos, escala, seed se aplicável, transições por NPC, falhas esperadas/inesperadas, contagem máxima/final de reservas e resultados de cleanup.
6. **Defeitos e correções:** severidade, causa, arquivos, regressão e status final. Liste pendências e seus impactos.
7. **Evidências visuais e desempenho:** paths e correlação com os testes; medições, ambiente e limitações.
8. **Mudanças:** arquivos de código, testes, dados, prefabs/cenas e `.meta` alterados, distinguindo alterações próprias de preexistentes. Estado final do Git. Confirme ausência de commit/push automático.
9. **Reprodução pelo usuário:** cena real, como iniciar Play Mode, quais NPCs selecionar, onde observar atividade/reserva, como avançar relógio, como editar JSON e como executar testes. Use nomes reais do projeto.
10. **Próximo passo:** se gate aprovado, `npc-debug-tools` (reaproveitando diagnósticos existentes), depois `npc-schedule-editor`. Se bloqueado, liste a correção residual prioritária. Atualize o status das tasks do Spec Kit conforme evidências.

**Resultado esperado:** uma baseline estável do sistema de rotinas, comprovada por testes e comportamento real de três moradores no Freyja, sem acrescentar novas mecânicas antes de resolver os problemas de integração.
