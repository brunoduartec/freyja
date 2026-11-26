# Plano de implementação: NPC Activity Execution Core

**Branch**: `087-npc-activity-execution-core-latest` | **Data**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Resumo

Adicionar um executor determinístico orientado a ticks que recebe `ActivityIntent` do resolver já existente e interpreta receitas lineares para `idle`, `wander` e `sit`. O núcleo fica em C# puro compartilhado com o harness. Ports permitem simular movimento, animação e Smart Objects sem dependências da Unity. O simulador semanal atual passa a avançar agenda e execução para três NPCs fictícios e emitir eventos diagnósticos; as fixtures não afirmam atividade física.

## Contexto técnico

**Linguagem/versão**: C# compatível com Unity 6 (6000.3.25f1), evitando APIs Unity e recursos modernos que não sejam necessários; SDK local observado: .NET 11.0.100.  
**Dependências primárias**: BCL e parser `StrictJson` já existente; sem novos pacotes.  
**Armazenamento**: receitas JSON junto aos dados de `NpcScheduling`; sem persistência de execuções.  
**Testes**: harness existente `Tools/NpcScheduleSimulator --test`; CLI `--validate` e `--simulate-week`.  
**Plataforma alvo**: núcleo compartilhado que compila fora da Unity e poderá ser importado pelo Unity 6.  
**Tipo de projeto**: biblioteca interna C# + CLI/simulador existente.  
**Metas**: tick sem I/O/esperas reais; transições limitadas por tick; resolução e execução determinísticas.  
**Restrições**: não editar cenas, prefabs, pacotes ou metadados; não executar Unity; não mudar resolver/catálogo sem necessidade; preservar arquivos locais e não criar commit/merge/push.  
**Escopo**: uma execução por NPC; operações e limpeza; três receitas; fixtures fake; sete dias; testes negativos e integração ao harness.

## Verificação de constituição

- Decisões do GDD e distinção entre fatos, relatos e hipóteses preservadas: sim; simulação não produz pistas nem evidência narrativa.
- Spec aprovada antes do plano e tarefas: sim.
- Unity e Blender: indisponíveis e fora deste escopo; nenhuma validação no Editor será alegada.
- Mudanças de código verificadas por compilação/testes C# no ambiente acessível; comportamento físico ficará pendente.
- Fontes próprias permanecem em `Assets/_Project/Scripts`; fontes compartilhadas são ligadas ao harness sem cópia.
- Sem dependências, sistemas genéricos de workflow ou alterações amplas.
- O ramo `087-npc-activity-execution-core` já existia na base anterior ao Schedule Core. Para manter os contratos rastreados acessíveis sem rebase/reset/merge, o trabalho segue em `087-npc-activity-execution-core-latest`, criada diretamente da branch `086`.

## Decisões de pesquisa

Ver [research.md](research.md). Contratos observados: `ActivityIntent` expõe NPC, atividade, alvo, dia, validade, prioridade, origem, fonte e bloco; `ScheduleResolver.Resolve` produz a intenção vigente. O catálogo e o loader estão em `Tools/NpcScheduleSimulator/Program.cs`; `StrictJson` e os modelos permanecem reutilizados. O projeto Smart Objects contém handle proprietário/token, validade, ocupação e liberação, mas usa Unity; o núcleo novo mantém handle opaco através de portas próprias e não referencia esses tipos. O target net11 do harness não define a API disponível na Unity: o código compartilhado usa apenas sintaxe/API compatível com o projeto e isso será registrado como não validado no Editor.

## Desenho e organização

```text
Freyja/Assets/_Project/Scripts/NpcScheduling/
├── Core/                 # Schedule Core existente, inalterado
└── Execution/            # modelos, operações, coordenador, receitas e ports puros
Freyja/Assets/_Project/NpcScheduling/Data/
└── execution/             # receitas v1 e schema
Tools/NpcScheduleSimulator/
├── Program.cs             # loader e CLI/harness existentes, ampliados
└── Tests.cs               # testes anteriores preservados + cobertura de execução
specs/087-npc-activity-execution-core/
└── evidencias.md           # validação executada e pendências de integração
```

`ActivityCoordinator` consulta somente a intenção já resolvida e a compara por identidade semântica: NPC, atividade, alvo, parâmetros disponíveis e origem/bloco/ocorrência (incluindo dia efetivo para mudança de ocorrência). Atualizar validade da mesma ocorrência preserva execução e atualiza o prazo. Um conjunto de ocorrências terminais por coordenador impede reinício a cada tick. Intenção nova ou cancelamento externo interrompe a operação vigente, roda limpeza e só então inicia substituta. Tokens de execução bloqueiam retornos atrasados.

`ActivityExecution` avança uma máquina de estados síncrona por tick, com orçamento configurável de transições. O host injeta um monotonic-seconds operacional e `GameTimeSnapshot`; calendário avança independentemente do tempo de operação. Há tempos limite operacionais para adaptadores e prazos de calendário para receitas. Retrocesso de qualquer relógio é diagnosticado/rejeitado. Não há threads, tarefa de fundo, relógio de parede, aleatoriedade ou I/O durante tick.

Receitas JSON lineares usam nomes whitelist. Um contexto de execução guarda estado transitório e recursos em ordem de aquisição. Os ports coesos cobrem interação/slots, deslocamento e pose/animação. Fakes ficam exclusivamente no harness. Reservas usam token opaco de tentativa, dono e execução; reserva ou liberação tardia não altera token novo. Limpeza tenta cada recurso em ordem inversa mesmo após falha e bloqueia substituição insegura se a posse residual persistir.

`idle` sem alvo aguarda no lugar; com destino faz movimento simulado e então aguarda a validade. `wander` seleciona deterministically pontos autorizados de região e intercala espera até o fim, limitado por transições; não reserva slot. `sit` busca candidatos determinísticos, reserva um entre slots exclusivos, move/alinha antes de ocupar, toca animações simbólicas, espera até fim vigente, toca saída e libera. Interrupção solicita restauração de pose através do port e garante tentativa de liberação. O fake não valida animações reais.

Receita ausente ou falha definitiva produz diagnóstico e fallback `idle` não recursivo quando seguro. Retries e busca por alternativas têm orçamento finito e cooldown determinístico. A mesma falha não se repete a cada frame. Falha de `idle` termina em estado terminal explícito.

O harness reaproveita `Load`, `ScheduleResolver`, o `Tests.Run` e `--simulate-week`. Os fakes incluem dois bancos com dois slots cada, três regiões, três NPCs fictícios, tempos de viagem configuráveis e injeção de caminho/animação/reserva invalidada. A trilha semanal é compacta e determinística, com eventos de execução separados das mudanças de intent. Cenários dirigidos no mesmo harness garantem disputa, atualização da agenda, cancelamento e falhas sem fabricar sucesso.

## Portas de constituição após o desenho

Passam: fonte de verdade única para agenda/catálogo; portas somente nas fronteiras externas; nenhuma dependência Unity no núcleo; dados de agenda preservados; validação fica limitada ao ambiente disponível; execução simulada não é evidência de mundo/cânone. A integração Unity e importação/geração de `.meta` não podem ser validadas agora.

## Complexidade

Nenhuma violação. A máquina de estados é específica a receitas NPC lineares e não é um framework genérico de workflows.
