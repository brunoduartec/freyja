# Freyja — NPC Debug Tools

**Feature:** `npc-debug-tools`  
**Etapa:** 8 de 12 do roadmap de rotinas.  
**Ambiente:** Unity 6, Unity MCP disponível conforme relato do usuário.  
**Objetivo:** permitir que o desenvolvedor inspecione, controle e diagnostique os NPCs reais em Play Mode, pela Unity e por uma interface de diagnóstico estruturada que possa ser reutilizada pelo agente Validador, sem introduzir novas mecânicas ou duplicar a infraestrutura existente.

## 1. Contexto e pré-condições

As seguintes entregas foram relatadas como concluídas:

- `smart-objects-foundation`
- `npc-schedule-core`
- `npc-activity-execution-core`
- `npc-navigation-system`
- `npc-animation-system`
- `npc-world-integration`
- `npc-integration-validation`

As atividades físicas inicialmente suportadas são `idle`, `wander` e `sit`. O cenário de referência contém três NPCs com agendas JSON e Smart Objects reais. Confirme esse estado no checkout, nos relatórios e no editor. A conclusão relatada não substitui as evidências. Leia especialmente o relatório final de integração, seus P0/P1 remanescentes e os diagnósticos já produzidos. Se houver bloqueio crítico, registre-o; corrija apenas o necessário para esta feature, sem reimplementar os sistemas anteriores.

O Freyja é um jogo de mistério com rotinas previsíveis e três protagonistas jogáveis. O monitor é uma ferramenta de desenvolvimento, não uma interface do jogador; informações internas, flags secretas e atividades não descobertas não devem vazar para a interface de gameplay.

## 2. Processo de trabalho e economia de contexto

1. Leia `AGENTS.md`, instruções locais, constituição e workflow Spec Kit existentes. Gere/atualize spec, plan e tasks usando o fluxo do projeto; não reinstale frameworks nem crie infraestrutura agêntica nova.
2. Registre branch, HEAD, `git status --short`, arquivos não rastreados relevantes, versão Unity, cenas/harness e baseline de compilação/testes. Identifique o que já existe de debugger, logs, snapshots, comandos de relógio e ferramentas de autoria. Reutilize os componentes úteis.
3. Game Designer: delimite UX, clareza dos estados e critérios de aceite. Implementador: altere somente os pontos necessários. Validador: revise contratos, execute testes e confirme que a ferramenta reflete o mundo real. Se agentes não estiverem disponíveis, desempenhe os papéis sequencialmente. Uma única responsabilidade de escrita por arquivo.
4. Preserve alterações preexistentes. Não faça reset, stash, clean, checkout destrutivo, commit, merge ou push automático. Não sobrescreva mudanças de outras sessões; reporte conflitos e avance no que for seguro.
5. Leia apenas arquivos relevantes; compartilhe um resumo curto de contratos reais; agrupe consultas MCP; não repita screenshots ou despeje logs extensos no contexto. Use filtros, resumos e artefatos locais. Caso RTK falhe, utilize os comandos nativos do dotnet e registre o fato.
6. Execute em incrementos: auditoria/baseline; snapshots; monitor somente leitura; controles seguros; Scene View; exportação e testes. Não pare apenas em specs e tasks se houver condições de implementar. Se Unity/MCP falhar, use alternativas disponíveis e marque testes físicos pendentes; não simule sucesso.

## 3. Arquitetura: observabilidade e comandos, não uma nova IA

```text
Relógio / Schedule / Host / Activity / Navigation / Animator / Smart Objects
                                |
                    fontes reais de diagnóstico
                                |
                 snapshots + buffer limitado de eventos
                          /                  \
              Unity EditorWindow          exportador JSON
                   /      \
              lista      detalhe / Scene View
                   |
           comandos de debug explícitos
                   |
       APIs reais de relógio / host / executor
```

Os nomes desta especificação são conceituais. Use os tipos, portas e métodos reais encontrados. Preserve os núcleos C# independentes de UnityEngine/UnityEditor conforme organização existente. Use uma camada pequena de snapshots/consultas; a UI não deve recalcular agendas, caminhos ou simular estados. Não crie um segundo relógio, controlador, scheduler, executor, catálogo, registro de locais ou registro de reservas. Não use reflexão genérica para manipular campos privados ou acessar todos os objetos da cena a cada atualização.

Separe consultas somente leitura de comandos que alteram a simulação. Consultar um NPC não pode disparar reavaliação, pathfinding, reserva, animação ou alteração de gameplay. A ferramenta deve funcionar com três NPCs e crescer para a população prevista da ilha, sem pressupor que o desempenho com 30 NPCs esteja validado.

## 4. Contrato de diagnóstico estruturado

Reutilize a API de diagnóstico já implementada, se houver. Complete somente informações ausentes que sejam úteis e acessíveis. Crie DTOs/snapshots imutáveis ou cópias defensivas, serializáveis, sem referências a GameObject, Transform, Animator ou objetos vivos. Não exponha referências mutáveis de serviços nem reintroduza dependências Unity nos núcleos puros.

### 4.1 Snapshot global

- `schemaVersion`, identificação da sessão/cena, modo (Edit/Play), instante do relógio do jogo (dia/minuto, conforme calendário real), timestamp de captura e escala/estado do relógio se disponível.
- Quantidade de NPCs registrados/ativos/desativados e com falhas conhecidas; contagem de slots disponíveis/reservados/ocupados segundo o modelo real.
- Coleção filtrável de NPCs e Smart Objects; configuração de filtros, limite de eventos e indicadores de dados truncados.

### 4.2 Snapshot individual

| Grupo | Dados mínimos ou desejáveis quando disponíveis |
|---|---|
| Identidade | ID estável, nome de exibição, configuração/rotina, template, status do host |
| Agenda | Compromisso vigente, ID da atividade prevista, destino simbólico, início/fim e próxima fronteira conhecida |
| Decisão | Regra selecionada, origem (template/individual/override), prioridade real, condições relevantes e motivo da resolução se expostos |
| Execução | ID de execução/intenção, atividade efetiva, estado/etapa atual, duração, cancelamento, fallback e motivo da última falha |
| Navegação | Destino solicitado/resolvido, estado, caminho completo/parcial/inválido, distância restante/velocidade quando válidas, timeout ou stuck segundo contrato existente |
| Animação | Ação solicitada, estado/clip atual identificável, estado de transição e alinhamento/controle de movimento quando disponíveis |
| Smart Objects | ID de objeto/slot, estado de reserva, proprietário e condição de ocupação conforme fonte real |
| Histórico | Transições recentes, falhas e comandos correlacionados por NPC e execução |

Diferencie claramente atividade programada, atividade efetivamente executada e última atividade concluída. Um pedido de reserva não significa slot reservado; um slot reservado não significa NPC sentado; uma intenção não prova presença física. A UI deve diferenciar `unknown`, `not_applicable`, `unavailable` e valor real, inclusive dados desatualizados ou não acessíveis. Não invente estados ou razões para preencher campos. Se explicar a decisão exigir instrumentação, exponha a decisão efetivamente tomada pelo resolver, sem calcular uma segunda decisão em paralelo.

Exemplo ilustrativo (adapte ao contrato existente; não transforme os nomes deste exemplo em requisitos de renomeação):

```json
{
  "schemaVersion": 1,
  "mode": "play",
  "gameTime": {"day": 1, "minuteOfDay": 750},
  "npcId": "npc_03",
  "scheduledActivity": "sit",
  "currentActivity": "wander",
  "executionState": "executing",
  "fallbackReason": "no_available_slot",
  "destinationId": "village_square",
  "navigationStatus": "valid",
  "reservation": null
}
```

O exemplo é fictício, não é evidência de execução no Freyja.

## 5. Unity EditorWindow — NPC Debug Monitor

Disponibilize uma janela pelo menu `Freyja > NPC > Debug Monitor` ou pelo padrão já adotado no projeto, documentando o caminho efetivo. Utilize UI Toolkit ou IMGUI conforme convenção existente. Não instale pacotes nem gaste tempo com polimento visual. A janela deve ser funcional, redimensionável e legível, com áreas roláveis.

### 5.1 Cabeçalho

- Cena e modo atual, horário/dia, estado do relógio, velocidade de simulação quando suportada.
- Contadores de NPCs ativos, falhas e slots reservados/ocupados (separados quando houver essa distinção).
- Botão de atualização manual, autoatualização opcional, indicação de última captura e exportação.
- Estado vazio explícito fora de Play Mode ou quando não existirem fontes de diagnóstico.

### 5.2 Lista de NPCs

Cada linha mostra ID/nome, atividade prevista e efetiva, estado, destino, falha/fallback e reserva resumida. Inclua busca por ID/nome e filtros mínimos: todos, ativos, com falha, em fallback, reservando/ocupando. Não trate fallback legítimo como erro grave. Permita selecionar e localizar o GameObject correspondente no Hierarchy/Scene View quando ele existir. Preserve seleção durante atualização; remova seleção obsoleta com segurança. Ordenação estável e sem reconstrução excessiva de UI.

### 5.3 Detalhe do NPC selecionado

Mostre todas as categorias do snapshot com rótulos claros, incluindo motivo de escolha quando disponível, estado de execução, etapa atual, próxima mudança de rotina e falhas. Mostre a rotina resolvida ou o compromisso atual e próximos compromissos calculados pela fonte existente, sem editar JSON. Se a previsão futura depender de estado desconhecido, indique que é condicional, não uma certeza.

### 5.4 Smart Objects

Liste objetos e seus slots, atividade compatível, ID do proprietário, estado real e posição/referência no cenário. Permita filtrar por objeto e ocupação e localizar o objeto/slot no editor. Não exponha operações de liberar reservas arbitrariamente; qualquer cancelamento deve passar pelo dono da execução. Distinga referência inválida, NPC removido e estado normal de transição. Se o sistema existente não distingue reserva de ocupação, indique a limitação; não invente uma segunda fonte de estado.

### 5.5 Histórico

Painel de eventos recentes por NPC e categoria: decisão, mudança de atividade, início/fim/cancelamento, falha, fallback, reserva/liberação, navegação/animação relevantes, comando de debug. Inclua horário do jogo e ordem/timestamp de captura, ID de execução e motivo, quando disponíveis. Sem log por frame. Permita limpar apenas o histórico de diagnóstico (não estados de gameplay), filtrar e copiar/exportar. Use um ring buffer limitado (ex.: 512 eventos globais, configurável com limite superior razoável), contabilizando eventos descartados. Evite captura de payloads grandes ou duplicação do logging existente.

## 6. Controles de simulação seguros

Os comandos devem estar habilitados apenas durante Play Mode em ambiente de desenvolvimento. Use APIs existentes e encaminhe os resultados reais. Retorne sucesso, rejeição, pendência ou falha com motivo; um pedido aceito não significa ação concluída. Não altere campos serializados, `Time.timeScale`, transforms, estado de Animator ou reservas diretamente para produzir a aparência de funcionamento.

### 6.1 Relógio

- Pausar/retomar o relógio do jogo, quando suportado pelo contrato. Explique se a pausa congela somente o calendário ou também movimento e execução; não confunda os dois.
- Avançar 15 minutos e 1 hora; definir dia/hora futura ou avançar até horário escolhido, se seguro e suportado. Não retroceder por padrão: rollback de estado, eventos e narrativa não é parte desta feature.
- Selecionar velocidade de relógio dentre valores suportados (por exemplo 1x, 5x, 10x); respeitar limites existentes. Não criar timeScale paralelo. Identificar claramente o efeito sobre relógio versus tempo físico e restaurar configurações temporárias quando pertinente.
- Alterações de horário devem seguir o fluxo de mudança existente e provocar as reavaliações corretas, sem iniciar execuções duplicadas. Nenhum controle pode contar atividade pulada como fisicamente realizada.

Caso a API do relógio não permita algum comando, implemente uma extensão mínima apenas se coerente com o contrato e testável; caso contrário, deixe o controle desabilitado com motivo. Não criar outro relógio de debug.

### 6.2 Comandos por NPC

- Solicitar reavaliação da agenda atual através do host, sem reiniciar desnecessariamente a mesma intenção.
- Solicitar cancelamento da atividade atual. Respeitar cleanup assíncrono e política de retomada existentes; explicitar o que acontece depois do cancelamento, pois a agenda pode solicitar novamente a mesma atividade.
- Override temporário de atividade para debug, somente se compatível com o mecanismo existente: selecionar atividade fisicamente suportada e destino válido, exigir prazo/expiração ou comando de remover. Um override deve usar a entrada de prioridade já prevista pelo sistema; não contornar ações críticas ou posse de recursos. Ao expirar ou remover, retomar o compromisso vigente e não um compromisso obsoleto. Não implementar novo sistema narrativo de overrides para atender esse botão. Se não for viável, registrar como pendente e explicar o motivo na UI.
- Exibir comandos pendentes, rejeitar duplicatas/conflitos de forma segura, registrar autor/origem `debug`, horário e resultado. Não teletransportar, não forçar animação nem liberar um slot de outro NPC para parecer que o comando funcionou.

A janela não deve executar comandos automaticamente ao abrir, atualizar, selecionar um NPC ou exportar. Encerrar o Play Mode elimina overrides temporários e inscrições da sessão; fechar a janela não deve deixar uma pausa ou override silenciosamente órfão: defina e documente a política de restauração usando ownership e sem sobrescrever mudanças externas. Não persistir comandos/overrides em JSON de rotinas, cenas, prefabs ou saves.

## 7. Visualização no cenário (Scene View)

Reaproveite Gizmos e ferramentas de visualização existentes. Adicione toggles para visualizar somente o NPC selecionado (padrão) ou todos os ativos, limitando custo.

- Destino solicitado/resolvido, trajetória já calculada e direção, quando acessíveis; não recalcular path para desenhar.
- Limites da região de `wander` e pontos de aproximação/interação dos Smart Objects.
- IDs de objetos/slots e indicação de estado usando cor + texto/legenda; não confie somente em cor.
- Estado/resumo de atividade sobre o NPC opcionalmente, sem poluir a Game View.
- Indicadores de falha/stuck provenientes do Navigation System ou executor, não de um novo detector concorrente.

A ferramenta é passiva. Não gerar caminhos nem editar posição/configuração do mundo ao desenhar. Remover subscriptions e overlays ao fechar/desabilitar/encerrar Play Mode, inclusive com domain reload desabilitado, se o projeto utiliza essa opção. Não criar objetos persistentes na cena só para desenhar diagnósticos.

## 8. API de diagnóstico e exportação para o Codex

Exponha consultas pequenas e reutilizáveis por testes e ferramentas de editor, sem exigir abrir a janela. Sugestões conceituais (adapte aos contratos reais): `CaptureSnapshot(filter)`, `CaptureNpc(npcId)`, `CaptureSmartObjects(filter)`, `GetRecentEvents(filter, limit)`. Não criar uma API HTTP, servidor MCP, socket ou serviço externo. Se o Unity MCP instalado oferece execução de métodos de editor/consultas, documente a chamada efetivamente suportada. Caso contrário, disponibilize um menu/comando local de editor que exporte o mesmo snapshot e um método público utilizável pelo harness de teste. Não inventar capabilities do MCP.

Exportar JSON UTF-8, com schemaVersion, contexto da cena/sessão, horário, filtros aplicados e limites. Permitir snapshot global e do NPC selecionado; incluir eventos recentes quando solicitado. Ordenar NPCs/slots de forma estável para facilitar diff; evitar dados voláteis desnecessários e serializar números em cultura invariável. Valores inválidos de navegação (NaN/Infinity) devem ser representados por null + estado explicativo, não JSON inválido. Exibir sucesso somente após escrita válida, com caminho real. Permitir copiar diagnóstico resumido para clipboard. Usar pasta de evidências/diagnóstico já adotada pelo projeto, fora de Assets e sem importar arquivos automaticamente. Não expor paths pessoais absolutos no conteúdo dos relatórios, segredos, dados privados ou enviar informações para serviços externos.

Exemplo de fluxo esperado do Validador:

1. Iniciar Play Mode na cena de referência e avançar o relógio até a janela de `sit`.
2. Capturar snapshot global antes da disputa; selecionar NPC com fallback e capturar seu detalhe e eventos recentes.
3. Identificar o motivo real, o slot e o proprietário via JSON, sem depender de captura de tela para descobrir estado lógico.
4. Cancelar uma execução através da ferramenta; aguardar conclusão do cleanup e capturar novo snapshot.
5. Correlacionar transições com o ID de execução e comprovar liberação/realocação. Usar Game View quando for necessário confirmar qualidade visual; o JSON não substitui essa observação.

Documente comandos reais, menu, paths relativos e um exemplo de saída gerada por execução real, não apenas um fixture inventado.

## 9. Performance e isolamento

- Use atualização sob demanda ou polling moderado (sugestão: 2–4 Hz enquanto a janela estiver aberta, configurável; não há budget formal). Evite polling por frame, FindObjects repetitivo, parsing de JSON, geração de caminhos, consultas de todos os clips ou alocações grandes em cada repaint.
- Preferir registros, eventos e estados já existentes. Se o histórico precisar de instrumentação runtime, habilite-a explicitamente em desenvolvimento, com buffer limitado e descarte contabilizado. Não executar coleta contínua em build de release. Mantenha código de editor em asmdefs/pastas de editor e verifique que runtime não referencia UnityEditor.
- UI fechada, mudança de cena, desativação/destruição de NPC, saída de Play Mode e nova sessão não devem acumular callbacks, caches, comandos ou referências. Testar a configuração de domain reload usada pelo projeto.
- Comparar amostras similares com janela fechada e aberta, três NPCs, mesma cena e velocidade. Registrar custo de captura/atualização, CPU/GC quando possível, quantidade de eventos e limites. Não prometer escalabilidade para 30 habitantes sem teste nem inventar métricas. Corrigir overhead evidente e localizado.

## 10. Matriz de validação obrigatória

Utilize testes unitários/EditMode para DTOs, filtros, comandos e lifecycle isoláveis; PlayMode real para integração. Reaproveite testes/harness existentes e não duplique suites sem necessidade. Registre PASS / FAIL / BLOCKED / NOT RUN para cada caso; subcasos precisam de cobertura explícita. Fakes são permitidos em testes isolados, nunca como prova de comportamento real.

| ID | Caso | Resultado esperado |
|---|---|---|
| D01 | Compilar/importar na Unity 6 | Sem erros novos; assemblies editor/runtime separados. |
| D02 | Abrir monitor fora de Play Mode | Estado vazio/preview explicitamente rotulado; comandos desabilitados. |
| D03 | Abrir monitor em Play Mode | Lista os três NPCs reais por ID, sem duplicatas. |
| D04 | Busca, filtros e seleção | Exibe NPC correto, localiza na cena e mantém seleção durante atualização. |
| D05 | Snapshot durante idle e wander | Previsto/efetivo, destino e estado correspondem aos sistemas reais. |
| D06 | Dois NPCs sentados e terceiro em fallback | Monitor distingue intenção, execução, reservas/ocupação e motivo real. |
| D07 | Mudança natural do horário | Agenda, execução e próxima fronteira são atualizadas sem reiniciar ação indevidamente. |
| D08 | Pausar/retomar relógio | Efeito real e escopo da pausa documentados, sem relógio paralelo. |
| D09 | Avançar 15 min/1h e atravessar meia-noite | Calendário e agenda coerentes; ações puladas não são marcadas como realizadas. |
| D10 | Alterar velocidade suportada | Mudança real e visível; locomocão não é artificialmente acelerada por outro mecanismo. |
| D11 | Solicitar reavaliação repetida | Não duplica execução nem reservas. |
| D12 | Cancelar sentado ou em deslocamento | Estado pendente/terminal real; cleanup e retomada corretos. |
| D13 | Override temporário válido e expiração/remoção | Atividade passa pelo executor, termina e volta ao compromisso vigente; se não suportado, registrar BLOCKED e razão. |
| D14 | Comando inválido ou conflitante | Rejeição explícita e sem alteração indevida do estado. |
| D15 | Destino inválido/caminho inacessível | Mostra falha ou fallback efetivo e motivo disponível. |
| D16 | Desativar/destruir NPC reservado | Lista reflete lifecycle e slot é liberado pelo sistema proprietário. |
| D17 | Overlays de trajetória/slot/região | Correspondem aos dados reais e não provocam novas solicitações de path ou mutações. |
| D18 | Histórico de transições | Ordem, IDs e motivos coerentes; nenhuma duplicação por repaint. |
| D19 | Estourar capacidade do buffer | Limite respeitado, descarte contabilizado, memória não cresce indefinidamente. |
| D20 | Exportar snapshot individual e global | JSON válido, campos/limites corretos; corresponde ao momento de captura. |
| D21 | Consultar/exportar com janela fechada | API funciona independentemente da UI, sem alterar estado. |
| D22 | Reabrir janela e reiniciar Play Mode | Sem callbacks duplicados, referências mortas ou overrides órfãos; repetir ao menos três vezes. |
| D23 | Recarregar cena ou remover seleção | UI segura, sem exceções ou dados antigos apresentados como atuais. |
| D24 | Medir monitor aberto/fechado | Registrar custo e condições comparáveis; sem gargalo evidente não resolvido. |
| D25 | Executar regressões de agenda, execução, navegação, animação e Smart Objects | Sem regressões relevantes; protagonistas continuam funcionando. |
| D26 | Verificar fronteira de desenvolvimento/build | Ferramentas e comandos não ficam ativos no release; compilação de player se ambiente permitir, senão registrar limite. |

D08–D10 e D13 dependem das capacidades reais do relógio e do mecanismo de overrides. Não declarar PASS se estiverem indisponíveis. Reportar a limitação, seu impacto e o que faltaria para habilitá-los. O monitor de leitura, exportação, inspeção de reservas e diagnóstico de falhas são obrigatórios para considerar a ferramenta utilizável.

## 11. Definition of Done

- [ ] A janela abre pelo menu documentado e lista os NPCs reais em Play Mode.
- [ ] É possível selecionar/localizar um NPC e inspecionar agenda prevista, atividade efetiva, estado, destino, navegação, animação, reservas e motivo de falha/fallback quando disponível.
- [ ] Smart Objects e slots refletem o registro autoritativo; nenhum estado fictício é apresentado como real.
- [ ] Controles suportados de relógio e reavaliação/cancelamento funcionam pelas APIs existentes; capacidades ausentes são explicitamente identificadas, sem atalhos que corrompam gameplay.
- [ ] Visualização Scene View é passiva; histórico limitado, filtros e exportação JSON funcionam; consultas podem ser usadas sem abrir a janela.
- [ ] Testes obrigatórios executados e registrados, com regressões corrigidas ou explicitamente bloqueadas; sem P0/P1 novos não resolvidos.
- [ ] Sem persistência involuntária de overrides ou alterações de teste, callbacks órfãos ou instrumentação ativa no release.
- [ ] Guia de uso curto, caminhos reais de arquivos e evidências reais entregues; status das tasks atualizado de acordo com a execução.

Se parte do escopo ficar bloqueada, informe status PARCIAL, não conclusão integral. Diferencie ferramenta implementada, compilada e validada fisicamente. Se o editor falhar, não usar testes simulados como evidência de Play Mode.

## 12. Fora do escopo

- Editor visual de rotinas, timeline de autoria, edição/salvamento ou hot reload de JSON: próxima feature `npc-schedule-editor`.
- Novas atividades, profissões, NPCs definitivos, diálogos, possessão, missões, pistas, testemunhos, interiores ou save/load.
- Novo scheduler, controlador, sistema de relógio, registro de slots, detector de stuck concorrente ou framework de IA.
- Debugger exposto ao jogador, servidor remoto, ferramentas externas, novas dependências comerciais, compra/geração de assets ou uso de Tripo AI.
- Refatorações amplas, mudanças nos controles dos protagonistas, na estética ou no layout da ilha. Alterações mínimas para teste devem ser isoladas e restauradas, preservando fixtures úteis e trabalho preexistente.

## 13. Relatório de entrega obrigatório

Entregue um resumo curto na sessão e relatório Markdown na pasta de specs/relatórios já existente. Inclua:

1. **Status:** APROVADO / PARCIAL / REPROVADO; baseline e eventuais defeitos remanescentes da integração anterior.
2. **Arquitetura real:** componentes reaproveitados, pontos de instrumentação, API de snapshots/comandos, asmdefs e motivo das decisões.
3. **Guia do usuário:** menu, cena, modo, como selecionar NPC, identificar fallback, inspecionar slot, avançar relógio, cancelar/reavaliar e exportar; explicitar capacidades indisponíveis.
4. **Guia do Codex:** como capturar JSON sem abrir UI, chamada efetivamente disponível via MCP ou alternativa local, filtros e paths de evidência; um snapshot real com interpretação curta.
5. **Matriz D01–D26:** status, ambiente, esperado/observado, evidência e retestes; contagens de testes separadas por dotnet/EditMode/PlayMode, comandos e resultados reais.
6. **Evidências:** poucas capturas úteis do monitor e da Scene View, JSONs correlacionados com o Play Mode (incluindo disputa de bancos, cancelamento e estado final), medições de custo e limitações.
7. **Mudanças e pendências:** arquivos modificados, estado final do Git, defeitos P0–P3, correções e restrições. Não atribua mudanças preexistentes à sua implementação nem faça commit/push automático.
8. **Gate para próxima etapa:** informe se a observabilidade e a estabilidade são suficientes para `npc-schedule-editor`. Liste contratos e fontes de dados que esse editor deverá reaproveitar. Não comece a próxima feature nesta sessão.

**Resultado esperado:** consigo abrir a Unity, identificar rapidamente o que cada NPC deveria fazer, o que realmente está fazendo e por que uma atividade falhou; consigo testar horários e intervenções suportadas e entregar ao Codex um JSON pequeno e confiável para reproduzir o problema, sem escrever código nem depender exclusivamente de screenshots.
