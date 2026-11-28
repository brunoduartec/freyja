# Freyja — NPC World Integration

**Feature:** `npc-world-integration`  
**Ambiente:** Unity 6, editor e Unity MCP novamente disponíveis.  
**Objetivo:** conectar as cinco fundações existentes e entregar uma primeira fatia vertical real: três NPCs executando autonomamente `idle`, `wander` e `sit`, a partir das suas rotinas JSON e do relógio do Freyja.

## 1. Contexto e pré-condições

O Freyja é um jogo de mistério em uma pequena ilha. A previsibilidade das rotinas dos habitantes é importante para a investigação. Não se pretende criar IA genérica ou autônoma irrestrita. As seguintes features foram relatadas como concluídas pelo usuário:

- `smart-objects-foundation`: descoberta de objetos, slots, reservas e poses.
- `npc-schedule-core`: catálogo, templates, rotinas JSON, condições e prioridades.
- `npc-activity-execution-core`: ciclo de execução, operações, cancelamento e cleanup.
- `npc-navigation-system`: movimentação física, regiões, aproximação e cancelamento.
- `npc-animation-system`: locomoção, bindings, poses e ações de sentar/levantar.

Houve também uma solicitação anterior de `npc-unity-integration-code`, quando a Unity estava indisponível. Verifique se seus adaptadores e host existem no checkout e em que estado estão. Reaproveite-os; não assuma que foram concluídos. A conclusão relatada das cinco features não prova que todos os testes passaram ou que elas estão presentes na mesma branch.

Esta especificação substitui as restrições temporárias da integração apenas por código: agora é permitido configurar os componentes, prefabs e destinos necessários com o editor e validar em Play Mode. Não elimina as restrições de segurança, escopo e autoria dos dados das features anteriores.

## 2. Processo, segurança e economia de tokens

1. Leia AGENTS.md, instruções locais e constituição do Spec Kit. Utilize o fluxo já instalado de especificação, planejamento, tasks, implementação e validação. Não instale novas ferramentas ou frameworks.
2. Registre branch, HEAD, `git status --short` incluindo não rastreados, mudanças locais relevantes e versão do editor. Confira specs, contratos reais, asmdefs e implementações das cinco features. Localize os adaptadores de integração existentes, o relógio, estado do mundo, componentes dos NPCs, carregamento dos JSON e sistema de diálogos, se houver.
3. Use os agentes já configurados, caso disponíveis: Game Designer delimita a fatia e os casos de aceite; Implementador integra e configura; Validador revisa os diffs, executa testes e confirma evidências em Play Mode. Se não disponíveis, execute os papéis sequencialmente. Não faça múltiplos agentes reescreverem os mesmos arquivos.
4. Preserve as alterações locais e trabalho de outras sessões. Não faça reset, stash, clean, checkout destrutivo, merge, commit ou push automático. Não altere arquivos compartilhados que estejam sendo editados concorrentemente. Não descarte mudanças para obter baseline limpo. Caso haja conflito, identifique-o e avance nas partes seguras.
5. Antes de editar, confirme compilação/importação na Unity e execute testes anteriores relevantes, registrando falhas preexistentes. Use o dotnet nativo se RTK estiver indisponível. Corrija apenas defeitos diretamente necessários, com regressão e escopo pequeno. Se alguma dependência crítica não existir ou tiver falhas impeditivas, não invente contrato, não simule sucesso e não reimplemente o subsistema inteiro. Documente o bloqueio e entregue o que for verificável.
6. Trabalhe em incrementos: baseline; composição e dados; relógio e destinos; primeiro NPC; três NPCs; testes e documentação. Compartilhe nomes reais de contratos e resumos pequenos entre agentes. Faça leituras seletivas e agrupe consultas ao Unity MCP; não use screenshots repetitivas nem transcreva grandes logs. Não encerre apenas com specs/tasks se houver condições de implementar.

## 3. Arquitetura e limites de responsabilidade

```text
Relógio e estado do mundo existentes
                 |
          NPC Runtime Host (por NPC)
                 | contexto + configuração
          Schedule Resolver existente
                 | ActivityIntent
          Activity Execution Core existente
                 | ports/adaptadores existentes
     +-----------+------------+-------------+
     |           |            |             |
  Locations   Smart Objects  Navigation   Animation/Pose
     |           |            |             |
     +-----------+----- cenário Unity ------+
```

Os nomes são ilustrativos; utilize tipos e métodos reais. A composição conecta serviços; não substitui os respectivos subsistemas. Núcleos de agenda/execução permanecem independentes de UnityEngine e UnityEditor. Preserve uma fonte de código compartilhada entre Unity e harness dotnet. Não copie classes para dentro de Assets apenas para resolver compilação; ajuste referências/asmdefs de forma mínima, compatível com a estrutura existente. Evite ciclos e não importe fakes ou executáveis dotnet no runtime de produção.

Invariantes:

- Um ID estável por NPC, um host ativo e um executor proprietário por NPC.
- O Scheduler é autoridade sobre a intenção e prioridades; o Activity Core é autoridade sobre o workflow, fallback e cleanup; Smart Objects são autoridade sobre reservas; Navigation controla locomoção; Animation/Pose controla apresentação e alinhamento no handoff já definido.
- Não crie outro relógio, outro catálogo, outro registro de reservas, outro controlador de navegação ou um workflow paralelo no MonoBehaviour.
- Rotina prevista não é atividade realizada nem prova de presença. Preserve essa distinção em logs e estados.
- Nenhum adaptador fake no gameplay real. Dependências inválidas geram diagnóstico explícito, sem teletransporte ou animação fictícia para marcar sucesso.

## 4. Composição e configuração dos NPCs

Implemente ou complete a camada de composição/runtime host existente. Ela deve receber referências ao relógio, dados e estado do mundo, resolver os serviços, criar/conectar a instância do executor e conduzir o NPC usando os ports reais. Configure os NPCs pelo Inspector/prefab e pelo mecanismo de dados já adotado.

Configuração mínima: ID estável, referência à rotina existente (e template, se aplicável), referências do personagem às camadas de navegação e animação, destinos pessoais quando requeridos e opções de diagnóstico. Use o sistema de identidade existente se houver. Não crie uma segunda identidade para os moradores.

- Inicialização somente quando dependências e dados estiverem prontos. Detecte IDs duplicados, hosts duplicados e referências faltantes. Evite depender da ordem implícita de Awake/Start entre objetos; utilize a inicialização existente ou composição explícita.
- Carregue e valide JSON usando os loaders/schemas reais. Use um mecanismo compatível com o runtime e builds do projeto (por exemplo, TextAsset se já adotado); não dependa de working directory, paths absolutos ou AssetDatabase no código runtime. Mantenha JSON como fonte de verdade. Não reimplemente parsing ou copie os mesmos dados para ScriptableObjects.
- Uma referência de configuração válida não garante atividade física disponível. Diferencie atividade conhecida no catálogo de atividade executável neste recorte. Templates com `fish`, `sleep` etc. devem permanecer preservados; não os execute fisicamente nem reporte sucesso simulado. Configure agendas de demonstração com apenas os três IDs suportados, usando o schema real.
- Documente os arquivos de rotina, como associá-los a um NPC e quando uma edição entra em vigor (por exemplo, após reimportação e reinício do Play Mode). Hot reload não é exigido.

## 5. Relógio e estado do mundo

Conecte o Schedule Core ao relógio/calendário já existente do Freyja. Não crie outro calendário. Mapeie dia, minuto, dia da semana e condições reais disponíveis ao contrato do resolver. Se clima ou flags narrativas não existirem, utilize as condições padrão documentadas pelo core; não crie sistemas novos nesta entrega.

- Resolva ao inicializar, nas fronteiras de agenda e quando o contexto relevante mudar. Use eventos existentes ou polling barato se necessário; não faça parsing ou resolução global em cada frame.
- Atualize o executor na cadência necessária aos ports físicos. Mudanças repetidas no mesmo compromisso não podem reiniciar a ação, disparar novo trigger ou renovar reservas continuamente. Use a identidade de intenção/execução já existente, não compare apenas `activityId` (dois compromissos iguais podem ter destino ou prazo diferentes).
- Ao mudar de compromisso, encaminhe cancelamento/retomada conforme contrato do core e só inicie nova execução quando a anterior estiver em estado seguro. Se o horário mudar múltiplas vezes durante a saída, respeite o compromisso vigente após cleanup, sem filas obsoletas.
- Separe tempo do calendário simulado de tempo operacional das ações. Use as políticas reais de pause, time scale e timeout. Avanço rápido e meia-noite devem resolver o compromisso atual, sem executar retroativamente todos os horários saltados. Documente retrocesso/reset do relógio. Não use DateTime.Now, Thread.Sleep, Task.Delay ou callbacks de background como relógio de gameplay.
- Para testar, reaproveite controles de relógio existentes. Se necessário, adicione um controle temporário/isolado de editor ou harness usando o mesmo relógio de produção, sem expor cheat obrigatório no build.

## 6. Registro de destinos reais

Reaproveite/complete o Location Registry ou mecanismo equivalente. Faça o mapeamento explícito dos IDs simbólicos reais do Schedule para locais Unity:

| Destino ilustrativo | Representação |
|---|---|
| `home` | Ponto de retorno externo, específico de cada NPC |
| `village_square` | Região navegável e pontos válidos para wander |
| `beach` | Região ou ponto navegável na praia |
| Destino de `sit` | Região/objeto compatível que permite descobrir bancos e reservar slots |

Os nomes acima não impõem novo schema; confirme os IDs efetivos. Se `home` é simbólico por NPC, resolva-o usando a configuração individual ou o escopo já suportado. Não associe todos os NPCs à mesma residência por engano. Não invente coordenadas ou use o nome do GameObject como ID implícito. Utilize pontos/volumes/Transforms existentes; destinos não precisam ser Smart Objects se não houver interação.

Valide registro/desregistro, IDs duplicados, destinos ausentes e objetos desabilitados. Use referências configuráveis pelo Inspector e Gizmos existentes. Não faça buscas globais por frame. Não construa um World Builder novo. Não implemente navegação por interiores: utilize pontos externos próximos às residências.

## 7. Conectar os adaptadores físicos existentes

### 7.1 Smart Objects

Conecte os ports do Activity Core ao registro de Smart Objects existente. Preserve tokens reais e ownership durante descoberta, reserva, movimento, alinhamento, ocupação e saída. Tentativas de reserva respeitam a operação atômica do sistema. Não crie registro duplicado ou simulação de reservas em produção. Se não houver slot livre, aplique o fallback já definido (outro banco compatível, espera limitada ou atividade alternativa). Sem loops infinitos.

### 7.2 Navigation

Use o Navigation System concluído, não chame NavMeshAgent diretamente em paralelo ao port existente. `idle` deve chegar ao destino antes de aguardar, se essa for a semântica da receita real; caso a receita só aguarde no local, use a operação de deslocamento já suportada na configuração, sem mudar o significado global de `idle`. `wander` deve chegar à região e utilizar o comportamento regional já implementado. `sit` navega até o ponto de aproximação, não até o centro do assento. Caminho parcial/inválido, chegada falsa, agente fora do NavMesh e destino destruído devem gerar os resultados e recuperação já previstos. Não teletransporte NPC para mascarar erro de caminho.

### 7.3 Animation e pose

Utilize o Animation System existente e os bindings semânticos reais. Sincronize idle/walk/run à velocidade real, respeitando a única autoridade de movimento. `sit_down` e `stand_up` são ações finitas; `sit_idle` deve sustentar a pose sem bloquear o workflow indevidamente. Não dispare clips a cada frame. Valide que o NPC está fisicamente sentado, alinhado e depois em pé. Respeite o handoff de NavMesh e pose/root motion já implementado. Não modifique rigs/controllers dos protagonistas para ajustar NPCs.

Sequência esperada (adapte ao contrato real): descobrir e reservar → navegar à aproximação → alinhar com slot válido → confirmar ocupação → sentar → aguardar compromisso → levantar/sair em segurança → liberar slot → próxima atividade. A reserva não pode ser liberada cedo enquanto o personagem ainda ocupa fisicamente o assento. Se um clip adequado ainda estiver faltando, registre bloqueio da demonstração de `sit`, não substitua por personagem em pé ou timer de sucesso.

## 8. Ciclo de vida, interrupções e robustez

- No enable/inicialização: validar dependências, registrar uma única vez, resolver agenda para o horário atual e iniciar apenas uma execução.
- No disable/destroy/unload: parar novas solicitações, cancelar operações conforme contratos, liberar recursos/reservas, remover inscrições e registro. Cleanup idempotente; não dependa de animação futura se objeto/NPC já estiver destruído.
- Em troca de compromisso ou interrupção externa: cancelar operação vigente, preservar ownership até saída segura, ignorar callbacks antigos e resolver a intenção atual. Não permitir duas execuções simultâneas para o mesmo NPC.
- Se o sistema de diálogo existente oferece evento/contrato de início e fim, conecte apenas o hook de interrupção/retomada, sem implementar diálogos novos. Caso contrário, demonstre a mesma interrupção por um comando de teste explícito e registre a integração real como pendente. Não declare diálogo validado usando o harness.
- Em falha de destino/slot/animação: exponha motivo e siga política de fallback/timeout existente. Não reinicie em loop por frame; falha persistente deve ficar diagnosticável.
- Ao reiniciar o Play Mode, confirme inexistência de inscrições duplicadas, reservas obsoletas ou estado estático indevido, inclusive se o projeto utiliza opções de Enter Play Mode sem domain reload. Não implemente save/load persistente nesta feature; preserve o sistema de saves existente se houver, e não declare restauração de atividades validada sem teste real.

## 9. Demonstração com três NPCs reais

Prepare inicialmente uma cena/harness de integração isolado ou uma área de teste segura usando os assets reais do Freyja. Reaproveite os bancos, NPCs e prefabs existentes. Depois, conecte e valide a mesma fatia no cenário real da ilha (não somente em uma cena artificial). Se o cenário principal estiver sendo editado por outra sessão, preserve-o, valide em ambiente isolado e identifique explicitamente a pendência de validação na ilha.

Configure três NPCs com IDs e rotinas individuais, pelo menos dois bancos de dois lugares independentes (ou equivalentes já existentes), uma região de praça, uma de praia e três pontos de retorno externos. Não distribua todos os habitantes nem refaça terreno, NavMesh global, iluminação ou construções. Utilize o Unity Editor/MCP para configurar referências, salvar alterações e gerar .meta normalmente; não invente GUIDs nem edite YAML de cena/prefab à mão. Não compre assets, não use Tripo AI, nem baixe pacotes sem autorização.

Rotina ilustrativa (adapte ao relógio, distâncias, schema e receitas reais):

| Período | Atividade | Destino |
|---|---|---|
| 00:00–08:00 | idle | residência externa |
| 08:00–12:00 | wander | praça |
| 12:00–14:00 | sit | bancos da praça |
| 14:00–18:00 | wander | praia |
| 18:00–24:00 | idle | residência externa |

Use horários parcialmente sobrepostos para exercitar competição. As rotinas devem diferir pelo menos em um horário ou destino; inclua uma janela em que os três procuram sentar no mesmo banco de dois slots. Teste também saturação de TODOS os slots compatíveis (por fixture ou ocupação controlada). O terceiro NPC deve usar alternativa válida; não pode sobrepor outro ocupante. Garanta tempo simulado suficiente para deslocamentos e transições. Documente escala do relógio e distância percorrida. Não considere `sit` concluído apenas porque o horário terminou antes de chegar ao banco.

A demonstração deve ser acionada automaticamente ao iniciar Play Mode e usar relógio + JSON + execução + ports reais, sem comandos manuais disparando cada ação. Um controle de avanço de horário pode acelerar testes, mas também deve haver pelo menos uma sequência com passagem normal de tempo e ações físicas completas.

## 10. Diagnóstico mínimo (não criar editor completo)

Reaproveite ferramentas anteriores. Exponha no Inspector/debugger, para cada NPC: ID, horário/dia, compromisso planejado, atividade efetiva, executionId (quando disponível), destino resolvido, estado da navegação/animação, slot e owner, último resultado/falha e motivo da mudança. Registre transições e erros significativos, não spam por frame. Diferencie chegada, início real da ação e conclusão. Disponibilize um resumo de reservas ativas.

Não implemente agora timeline de edição, painel global sofisticado, telemetria externa ou diário narrativo. A ferramenta de autoria completa será outra feature.

## 11. Plano de testes e critérios de aceite

Execute testes anteriores de Smart Objects, Schedule, Activity, Navigation e Animation quando disponíveis, além de novos testes focados na integração. Use EditMode para configuração/resolução e PlayMode para fluxo físico. Reaproveite harness dotnet para regressão de lógica. Não reescreva testes para esconder falhas.

| ID | Caso | Resultado obrigatório |
|---|---|---|
| W01 | Baseline/importação | Unity compila sem novos erros; regressões anteriores identificadas; jogo anterior permanece utilizável. |
| W02 | Três hosts configurados | IDs únicos, rotinas diferentes, componentes reais e nenhum executor duplicado. |
| W03 | Dados inválidos | JSON/ID/destino ausente produz diagnóstico claro; sem falso sucesso. |
| W04 | Inicializar em horário intermediário | NPC executa compromisso vigente, sem replay do dia. |
| W05 | Idle em casa | Desloca-se ao destino quando previsto e aguarda, sem caminhar no lugar. |
| W06 | Wander | Alcança praça/praia e caminha na região com animação coerente. |
| W07 | Sit completo | Reserva, chega, alinha, senta, mantém pose, levanta, sai e libera. |
| W08 | Três disputam banco de dois slots | Dois lugares distintos, terceiro usa alternativa, nenhum slot duplicado. |
| W09 | Todos os slots ocupados | Fallback/timeout limitado, nenhuma espera infinita. |
| W10 | Horário muda durante movimento | Cancelamento seguro e início do compromisso atual. |
| W11 | Horário muda enquanto sentado | Levanta/sai antes de liberar espaço e inicia nova ação sem sobreposição. |
| W12 | Interrupção externa | Cancela e retoma compromisso atual; se diálogo real existir, testar também seu hook. |
| W13 | Caminho/destino/slot invalidado | Falha explícita e cleanup; nenhuma chegada/ocupação fictícia. |
| W14 | NPC desativado durante ação | Recursos liberados, callbacks obsoletos ignorados, inscrições removidas. |
| W15 | Avanço rápido/meia-noite/pausa | Resolver sincronizado com relógio; sem replay retroativo ou tempestade de reinícios. |
| W16 | Edição de JSON | Alterar um horário/destino e comprovar mudança de comportamento sem editar C#. |
| W17 | Dia completo | Três NPCs completam ciclo idle/wander/sit/wander/retorno conforme agendas; registrar transições reais. |
| W18 | Reiniciar Play Mode | Nenhuma reserva órfã, duplicação de hosts ou eventos residuais. |
| W19 | Regressão no cenário real | NPCs funcionam na ilha e controles/câmera/relógio dos protagonistas continuam funcionando. |

Para W17, simule um dia acelerado com cadência que permita chegar, sentar e levantar; não apenas salte o calendário sem dar tempo à execução. Execute ainda um teste separado de salto de horário (W15). Se a infraestrutura permitir, automatize a captura dos resultados e assertivas de estados, poses/reservas e localização. Não substitua validação visual por testes de mocks. Use logs de sequência e evidências visuais pontuais em Play Mode para W06–W08/W17/W19, observando especialmente alinhamento, clipping, foot sliding e conflito de slots. Captura estática isolada não comprova uma transição.

A próxima feature `npc-integration-validation` será dedicada a ampliar a cobertura, rodar regressões prolongadas e corrigir problemas reais de integração; não use essa separação como desculpa para adiar os smoke tests físicos desta entrega.

## 12. Fora do escopo

- Novas atividades profissionais (pesca, agricultura, comércio, etc.), interiores, dormir/comer, equipamentos, IK e animações avançadas.
- Editor visual de rotinas completo, população de todos os moradores, testemunhos, possessão, quests, confiança e sistema narrativo.
- Reescrita dos cinco subsistemas, ECS/DOTS, GOAP, Behavior Trees, LOD, simulação offscreen ou novo framework de IA.
- Save/load novo, navegação entre interiores e exteriores, reconstrução do mundo e alteração de personagens jogáveis.

## 13. Definition of Done e relatório final

Considere a feature concluída somente se o projeto compilar na Unity, os ports reais estiverem conectados, os três NPCs demonstrarem as atividades pela agenda e relógio reais, houver disputa de slots/cancelamento seguros, edição de JSON comprovada e evidência de Play Mode na ilha. Caso algum critério falhe ou esteja bloqueado, entregue a parte validada com status parcial, sem transformar ausência de erro no console em prova de funcionamento. Testes de regressão abrangentes adicionais pertencem à próxima feature, mas os casos obrigatórios acima precisam ter resultado registrado.

Entregue relatório objetivo com:

1. Inventário das cinco fundações e adaptadores reaproveitados (paths e contratos reais), baseline e correções necessárias.
2. Arquivos novos/modificados, cenas/prefabs/configurações alterados e mudanças preexistentes preservadas, incluindo .meta gerados pelo editor.
3. Matriz W01–W19 com PASS/FAIL/BLOCKED/NOT RUN, comando/método, resultado esperado e observado; contagem de testes e evidências com paths. Diferencie revisão, compilação, testes unitários, integração simulada e Play Mode real.
4. Evidências de rotina de um dia, disputa por banco, interrupção/cleanup e mudança de JSON. Inclua horários do jogo, IDs de NPC, execução, slot e destino real nos registros. Forneça screenshots suficientes para verificar poses e localização, sem usar imagens como única evidência.
5. Instruções reproduzíveis: qual cena abrir, como iniciar a demonstração, como selecionar NPC, alterar rotina, registrar destino e avançar o relógio, com paths reais e nomes de campos encontrados no projeto.
6. Bloqueios, riscos e divergências desta especificação; status final das tarefas do Spec Kit. Não marque tarefa pendente como feita. Se Unity MCP falhar, limite tentativas, use o editor/runner disponível e registre o que não foi verificado.
7. Checklist residual para `npc-integration-validation`, restrito a regressões, estabilidade e problemas observados; nenhuma nova atividade profissional deve ser implementada agora.

**Resultado final:** a partir de uma única infraestrutura compartilhada, o Freyja possui três moradores com comportamento autônomo real, ajustável por JSON, e pronto para ser estabilizado antes de expandir as atividades e popular a ilha.
