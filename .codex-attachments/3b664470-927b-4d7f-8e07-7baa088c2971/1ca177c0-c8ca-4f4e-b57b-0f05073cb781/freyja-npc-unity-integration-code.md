# Freyja — NPC Unity Integration (code only)

Feature: `npc-unity-integration-code`  
Objetivo: conectar os núcleos de agenda e execução aos sistemas reais da Unity 6 por meio de adaptadores mínimos, preparando uma primeira fatia vertical de gameplay com `idle`, `wander` e `sit`. Esta entrega não inclui configuração física ou validação em Play Mode.

## 1. Contexto, pré-condições e limites

Freyja é um jogo de investigação em uma ilha pequena, com três protagonistas jogáveis. Rotinas dos moradores devem ser determinísticas e futuramente sustentar testemunhos e pistas. As features anteriores são `smart-objects-foundation`, `npc-schedule-core` e `npc-activity-execution-core`. O relatório do Schedule indica ausência de validação na Unity, necessidade de gerar `.meta`, destinos ainda simbólicos, checkout com alterações preexistentes, ausência de merge/push e RTK indisponível. A conclusão e os testes do Activity Execution Core ainda precisam ser confirmados no repositório.

O Mac e o editor Unity estão indisponíveis. Não chame Unity MCP, Blender ou Tripo AI; não tente iniciar a Unity, baixar ferramentas para substituir o editor, nem insistir em ferramentas indisponíveis. Use dotnet nativo se RTK falhar. Não modifique cenas, prefabs, Animator Controllers, animações, NavMesh, assets, ProjectSettings ou arquivos `.meta` manualmente. Não invente GUIDs, referências de assets, IDs de objetos reais ou nomes de animações. Não configure automaticamente moradores oficiais.

Antes de implementar:
- Leia AGENTS.md, instruções locais aplicáveis, constituição do Spec Kit e documentação relevante das três features anteriores.
- Registre branch, HEAD e `git status --short` (incluindo arquivos não rastreados) antes/depois. Identifique mudanças preexistentes. Não use reset, stash, clean, checkout destrutivo, merge, push ou commit automático. Não sobrescreva trabalho alheio; caso haja colisão, preserve-o e avance apenas nas partes independentes.
- Confirme existência dos contratos reais de Schedule, Activity Execution e Smart Objects; execute os testes anteriores disponíveis e registre baseline. Inspecione versão Unity, packages, asmdefs, compatibilidade de C#, serialização, fluxo de dados e serviços existentes de relógio, movimentação, animação e carregamento de cenas.
- Se o executor não estiver presente, ou seus testes relevantes falharem, não recrie seus contratos por suposição. Corrija somente um defeito pequeno, comprovado, diretamente necessário e sem conflito local, documentando-o. Caso contrário, implemente apenas partes independentes verificáveis, entregue o plano de integração restante e identifique a pré-condição bloqueada. Se um contrato de produção específico estiver ausente, não invente API nem declare o adaptador pronto.
- Confirme que está no repositório do Freyja e que as alterações das features anteriores estão acessíveis. Não assuma que um relatório anterior prova existência dos arquivos no checkout atual.

## 2. Processo de trabalho

Use o Spec Kit conforme já configurado. Reutilize specs existentes, crie somente os artefatos requeridos para esta feature e execute o fluxo até implementação e validação possível. Use os agentes existentes, caso disponíveis: Game Designer define comportamento e casos de teste; Implementador desenvolve os adaptadores; Validador revisa lifecycle, ownership, falhas e evidências. Se não houver subagentes, desempenhe os papéis sequencialmente e reporte. Não instale frameworks nem repita leituras integrais. Compartilhe contratos, paths, diffs pequenos e resultados resumidos.

Trabalhe em fatias: inventário de contratos e riscos; host/clock/data; destinos e Smart Objects; navegação/pose/animação; diagnóstico; testes e roteiro de integração física. Evite refatorações amplas. Não crie infraestrutura para funcionalidades que não serão utilizadas na primeira fatia vertical.

## 3. Arquitetura e invariantes

```text
World Clock + World State (existentes)
             |
        Unity NPC Host
             |    dados/ID de NPC
    Schedule Resolver existente
             | ActivityIntent
    Activity Coordinator / Execution Core existente
             | ports já definidos
    Unity Integration Layer
       |       |        |         |
  Locations  Smart    NavMesh    Animator/pose
             Objects
```

Adapte nomes e fronteiras aos tipos reais. Núcleos continuam C# puro, sem `UnityEngine`, `UnityEditor` ou dependência de adaptadores concretos. Use o mesmo código-fonte dos núcleos no harness e na Unity; não copie classes para resolver referências. Configure somente asmdefs necessárias e justificadas, preservando dependências unidirecionais e sem GUIDs inventados. Mantenha artefatos dotnet, fakes e testes fora dos assets de produção importados, conforme organização existente. Não introduza novos packages sem necessidade comprovada.

Uma agenda é intenção, não prova de presença. Apenas um executor ativo por NPC. O host não deve reimplementar regras de prioridade, identidade de intenção, fallback, retentativas, cleanup ou reservas já existentes. Não use simuladores como fallback silencioso em produção. Ausência de dependência precisa produzir erro diagnóstico e impedir inicialização ou ação inválida.

## 4. Adaptadores mínimos de produção

### 4.1 Clock / World State / carregamento de dados

- Conecte ao relógio e estado do mundo reais do Freyja quando acessíveis. Não crie segundo calendário nem relógio paralelo. Mapeie dia, minuto, condições e mudanças de contexto ao contrato real de entrada do resolver.
- Separe calendário simulado de tempo monotônico de operação. Use a política real de pausa do jogo e documente o comportamento de time scale, avanço rápido, meia-noite e retrocesso/reset de relógio. Nenhuma execução retroativa de compromissos saltados. Não use DateTime.Now, timers de parede, Thread.Sleep ou tarefas de background para simular o jogo.
- Resolva na inicialização, nas fronteiras relevantes de agenda e em mudanças de contexto; faça polling barato apenas se eventos não existirem. Não faça parsing de JSON nem resolução integral de agendas a cada frame. O executor pode precisar de Tick a cada frame para operações físicas; diferencie as frequências.
- Reutilize catálogo, templates, receitas e JSON existentes. Defina como os dados chegam ao runtime Unity por um mecanismo compatível com o projeto e builds (não caminhos absolutos, working directory ou APIs UnityEditor no runtime). Se ainda não houver mecanismo real, prefira uma entrada explícita por referências TextAsset ou provider existente, sem mover/copiar os JSON nem criar uma segunda fonte de verdade. Documente associação de dados no Inspector ou passo de importação posterior.
- Erros de schema, arquivo ausente ou ID duplicado não podem resultar em personagem aparentemente funcional com dados fictícios.

### 4.2 Location Registry

Crie uma ponte simples entre IDs simbólicos e destinos reais configuráveis por componentes/referências de cena. Exemplos ilustrativos: `home`, `village_square`, `beach`; use IDs reais do Schedule e preserve escopo por NPC para destinos como casa, se o contrato suportar. Diferencie ponto, região e destino de Smart Object. Uma região pode oferecer pontos navegáveis explícitos, sem gerar destinos aleatórios arbitrários.

Registro e desregistro por lifecycle; detecte IDs duplicados, referências inválidas, objetos desabilitados/destruídos, carregamento/descarregamento de cenas. Não use nomes de GameObject como IDs implícitos, buscas globais por frame ou coordenadas inventadas. Não crie locais reais nesta etapa. Documente como cadastrar destinos posteriormente, com exemplo de configuração e Gizmos opcionais simples, se úteis. Não crie um novo World Builder.

### 4.3 Smart Object Adapter

Implemente o port real do executor sobre o registro e as APIs efetivamente existentes de Smart Objects. Não crie outro registro de reservas de produção. Mapeie activityId/capacidade, destino/região, proprietário, slot, pose e ponto de aproximação; preserve identidade do token original sem reduzi-lo a ID do objeto.

- Descubra candidatos compatíveis e respeite ordenação determinística, caso já definida. A tentativa de reserva deve utilizar a operação atômica do sistema existente, sem assumir que candidato ainda está livre. Alternativas limitadas conforme política do core.
- Valide handle durante movimento e ocupação. Só confirme ocupação após aproximação/alinhamento bem-sucedidos. Em perda de reserva ou destruição do objeto, informe falha ao core; libere recursos conforme contrato real.
- Cancelamento antigo, callback tardio ou liberação repetida nunca pode afetar reserva nova ou de outro NPC. Não declare liberação bem-sucedida quando houver falha residual; preserve diagnóstico.
- O core controla cleanup; o adapter executa operações e retorna resultados. Não implemente outro workflow ou política concorrente de cancelamento.

### 4.4 Navigation Adapter

Reutilize controlador de movimento já existente; use `NavMeshAgent` apenas se for o mecanismo adequado no projeto. Não coloque dois componentes controlando o mesmo personagem, não reconfigure NavMesh global e não altere sistemas dos protagonistas.

Implemente início, polling e cancelamento de movimento de acordo com o port real. Considere componente desabilitado, agente fora do NavMesh, falha ao solicitar caminho, caminho pendente, parcial/inválido, destino destruído, distância restante não finita, tolerância de chegada e timeout. Caminho parcial não equivale a chegada. Não teletransporte personagem para simular sucesso, não use Warp como recuperação automática. Ao cancelar, interrompa apenas a operação do dono/executionId vigente e restaure somente estados sob sua responsabilidade. Não chame SetDestination em todos os frames para o mesmo compromisso. Se houver destino móvel, não implemente perseguição nesta entrega.

A chegada deve ser constatada por critérios reais do controlador (caminho válido resolvido + distância/tolerância e condição de parada apropriada). Para `wander`, utilize pontos de região configurados e política existente de seleção/espera; não reinicie infinitamente quando não houver ponto navegável.

### 4.5 Pose / Animation Adapter

Use o Animator/controlador já existente e faça mapeamento explícito de IDs simbólicos (`sit_down`, pose sentada, `stand_up` ou os IDs efetivos das receitas) para estados/parâmetros reais configuráveis. Não invente clips, parâmetros ou transitions; referências não verificáveis ficam pendentes de configuração e retornam falha clara quando ausentes.

- Alinhamento só depois de chegada e reserva válida, com posição/orientação vindas do slot. Defina tolerâncias configuráveis e interação entre agente, Transform, root motion e Animator. Não desabilite componentes permanentemente nem mova personagem de longa distância por snap. Se o slot não for alcançável, falhe de forma explícita.
- Respeite contrato de início e término de animação, transições, loops e interrupções. Não trate `Animator.SetTrigger` como confirmação de animação concluída. Use verificação de estado/evento existente com timeout e identificação de execução; se não houver suporte, documente o requisito e não reporte sucesso falso.
- `sit` deve permanecer em pose sentada até conclusão/interrupção e restaurar a locomoção antes de liberar o fluxo quando seguro. Limpeza também precisa ocorrer em erro, cancelamento ou remoção. Não force animação inexistente; ausência de clips é bloqueio da validação física.
- Prop/equipamentos, IK avançada e novas animações ficam fora de escopo.

## 5. Unity host por NPC e ciclo de vida

Implemente componente mínimo, nome conforme convenções reais (ex.: `NpcActivityController`), para inicializar configuração e dependências, resolver agenda, enviar ActivityIntent ao coordenador existente e avançar operações. Prefira composição explícita ou DI existente; não faça outro service locator global.

- Valide ID de NPC, referências e bindings necessários. Inicialização única e idempotente; não assine eventos duas vezes. Ao habilitar após pausa/desabilitação, resolva a intenção corrente, nunca retome reserva obsoleta.
- Ao desabilitar/destruir NPC, trocar cena ou sair de Play Mode, cancele execução, desinscreva callbacks e libere recursos conforme ownership. Se o cleanup normal depender de ticks futuros, defina mecanismo síncrono/seguro permitido pelos contratos para teardown; se não houver garantia, exponha bloqueio e proposta mínima. Não espere que Update continue depois de OnDisable.
- Considere Domain Reload desabilitado: evitar estáticos antigos, registros duplicados e reservas sobreviventes. Não use global reset que libere recursos de NPCs ativos em outras cenas.
- Exponha uma entrada simples de interrupção externa e retomada, somente se já existir no core; não integre diálogos, possessão ou missões nesta etapa. NPC controlado pelo jogador não pode receber comandos de agenda concorrentes; não modifique a troca dos três protagonistas agora.
- Diagnóstico de produção não é simulação: mostre origem da decisão, ID, destino simbólico/efetivo, estado, step, reserva (sem vazamento para UI de jogador), tempos e reason code. Use logs estruturados sob flag/configuração existente, log por transição ou falha, não por frame. Inspector/Gizmos mínimos; sem EditorWindow de schedules.

## 6. Fatia vertical alvo (configuração e validação posteriores)

Prepare código para um NPC de teste executar `idle` em casa, `wander` na praça, `sit` em banco com dois slots, `wander` na praia e voltar à rotina. Não altere o cenário agora. Use as receitas e IDs reais existentes; não crie outra agenda se o harness já tiver dados adequados. Para testar disputa, roteiro posterior com três NPCs e dois bancos de dois lugares (ou um banco e terceiro NPC sem lugar), incluindo indisponibilidade de todos os slots, fallback e liberação.

Ações não executáveis do catálogo devem retornar diagnóstico/fallback do core sem erro repetido a cada frame. Não implemente pesca, dormir, comércio ou demais atividades. Não produza assets novos nem use Tripo AI.

## 7. Validação possível no ambiente atual

1. Execute novamente todos os testes/harness relevantes dos núcleos, incluindo simulação de sete dias já existente. Não altere expectativas para esconder regressões. Registre comandos reais, versão SDK, resultados, contagem e falhas; se ferramenta ausente, reporte.
2. Adicione testes unitários fora da Unity somente para mapeamentos ou lógica de fronteira realmente portáveis (ex.: IDs, identidade de operação, mudanças de tempo), reutilizando harness e fakes já existentes. Não crie um clone da API Unity para obter build artificialmente verde.
3. Revise referências/asmdefs, APIs dos packages instalados, lifecycle e compatibilidade estática. Se o ambiente já possuir compilador/referências reais de Unity e permitir compilação independente do editor, registre precisamente o que compilou; caso contrário, classifique scripts Unity como escritos e revisados, **não compilados**. `dotnet test` dos núcleos não comprova compilação de MonoBehaviours ou funcionamento físico.
4. Adicione testes Edit Mode/Play Mode somente se alinhados à infraestrutura de testes Unity existente e aos contratos verificáveis. Não rode nem declare executados sem editor. Evite fixtures que dependam de assets/refs inexistentes; deixe roteiro manual reproduzível quando necessário.
5. Validador revisa diff e invariantes: nenhuma duplicação de core, uma execução por NPC, token de reserva protegido, nenhuma operação atrasada contaminando nova execução, teardown seguro, ausência de teletransporte/fake de produção, ausência de buscas/allocs/logs desnecessários por frame, preservação do checkout.

## 8. Roteiro obrigatório para quando o Mac voltar

Entregue um README/checklist operacional com paths e nomes reais de scripts e componentes, ordem de importação/configuração, campos do Inspector e resultado esperado. Incluir:

- [ ] Importar scripts na Unity 6; deixar o editor gerar `.meta`; verificar referências, asmdefs, Console e compilação; comparar git status e preservar arquivos preexistentes.
- [ ] Confirmar importação de dados e compatibilidade do parser em Editor e build; vincular o relógio/estado reais.
- [ ] Configurar destinos com IDs estáveis de casa, praça e praia; validar resolução e NavMesh.
- [ ] Validar Smart Objects, selecionar assets de banco disponíveis (sem Tripo), configurar dois slots por banco, ponto de aproximação e pose/orientação; confirmar reserva/ocupação/liberação.
- [ ] Configurar um NPC de demonstração, referência de dados, controlador, agente e Animator; verificar mapeamento de animações de sentar, permanecer sentado e levantar; se clips faltarem, reportar.
- [ ] Testar em Play Mode uma agenda completa `idle -> wander -> sit -> wander -> idle`, avançando o relógio de forma controlada. Verificar chegada, alinhamento, pose, encerramento e destino seguinte. Nunca aceitar NPC em pé como `sit` válido.
- [ ] Testar três NPCs concorrendo por slots, banco lotado, caminho inválido, destruição/desativação do objeto, cancelamento durante movimento e durante `sit`, avanço abrupto do relógio, desativação do NPC, troca de cena e nova sessão de Play Mode. Verificar ausência de recursos presos e callbacks antigos.
- [ ] Testar pausa e retomada conforme política do Freyja, e coletar evidências (logs, Console, vídeo/capturas, resultado de testes Unity). Validar os núcleos novamente após correções.

A entrega física será `npc-unity-integration-validation`, em outra sessão. Não execute esse checklist agora e não marque itens como concluídos por inferência.

## 9. Definition of Done desta entrega por código

- Contratos anteriores confirmados ou bloqueios individualizados. Adaptadores implementados sobre APIs reais disponíveis, sem dependências fictícias e sem duplicação. Se não for possível implementar algum adaptador, registrar exatamente a interface/arquivo ausente, o impacto e o trabalho restante.
- Cores preservados; qualquer alteração pequena e indispensável tem justificativa, compatibilidade e teste de regressão.
- Testes dotnet e harness executados se ambiente permitir; resultados reais e limitações descritos. Sem afirmação de compilação ou Play Mode sem execução.
- Roteiro de integração física reproduzível e documentação de autoria de destino/animation bindings; código sem edição de assets/cenas/prefabs/.meta.
- Status de Git final comparado ao inicial; nenhuma alteração preexistente descartada; sem commit/merge/push automático.

## 10. Relatório final do Codex

Entregue resumo conciso em português, com:

1. Status geral: implementação por código concluída, parcial ou bloqueada; status de compilação Unity separado; status de Play Mode separado.
2. Contratos reais encontrados e mapeamentos adotados (paths/classes/métodos), lista dos arquivos novos/alterados e justificativas de decisões não especificadas.
3. Tabela por adaptador: implementado/revisado/testado no dotnet/compilado Unity/testado Play Mode, conforme aplicável. Use `não executado` ou `não aplicável`, não marque verde por suposição.
4. Comandos de testes, resultados e regressões; bloqueios, riscos e pontos que dependem de inspeção do jogo real.
5. Link/path do README e checklist para a entrega de validação física. Informe ações específicas para configurar o primeiro NPC sem alterar os oficiais.
6. Git: branch, resumo do status antes/depois, alterações alheias preservadas, ausência de merge/push/commit automático e eventual falha do RTK.

**Encerramento:** não expanda a arquitetura de NPCs após esta entrega enquanto a fatia vertical não estiver validada na Unity. Não inclua editor de agendas, novas profissões, sistema de pistas, confiança, diálogos, economia, save/load completo, ECS, GOAP ou behavior trees. Priorize a integração mínima e verificável.
