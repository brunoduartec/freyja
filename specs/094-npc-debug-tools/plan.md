# Plano de implementação: NPC Debug Tools

**Branch**: `094-npc-debug-tools` | **Data**: 2026-11-27 | **Spec**: [spec.md](spec.md)

## Resumo

Adicionar uma janela IMGUI de Editor, consultas estruturadas e exportação JSON para observar a execução atual dos NPCs reais e seus Smart Objects. A captura lê o driver, controllers, walker, Animator e registry autoritativos; registra eventos já emitidos num ring buffer Editor-only. Comandos de relógio, reavaliação e cancelamento passam pelo `CoastalDayNight`, pelo tick existente do host e pelo `ActivityCoordinator`. Não será criado mecanismo de override, pois os contratos atuais não oferecem override temporário de sessão.

## Contexto técnico

**Linguagem/versão**: C#, Unity 6 `6000.3.25f1`.  
**Dependências**: Unity Built-in, IMGUI, `UnityEditor`; nenhuma dependência nova.  
**Persistência**: nenhuma para estado de diagnóstico; export JSON em arquivo local fora de `Assets`.  
**Validação**: testes existentes, Unity MCP, consultas públicas de Editor, Play Mode físico e Console.  
**Plataforma alvo**: Editor Unity; nada da ferramenta deve entrar em builds de jogador.  
**Projeto**: ferramenta interna de Editor sobre a cena de jogo existente.  
**Desempenho**: atualização manual ou até 2 Hz; ring buffer de 512 eventos; sem coleta por repaint/frame e sem recálculo de caminho.  
**Restrições**: preservar arquivos locais de `093`; não mudar cenas ou JSON de rotina; não usar `Time.timeScale`; não persistir comandos/overrides; não instalar pacotes.  
**Escala validada**: três NPCs reais; população maior não é declarada validada.

## Diagnóstico observado e decisões

| Domínio | Fonte real | Capacidade aproveitada ou limite |
|---|---|---|
| Relógio | `CoastalDayNight` + `WorldClockProjection` | `Hour`, `AdvancesAutomatically`, `SetHour`; sem dia/set speed expostos. Saltos só para frente; cruzar meia-noite passa pela projeção real. Um multiplicador transitório, compilado somente no Editor, estenderá o próprio relógio e não afetará movimento/tempo operacional. |
| Rotina | `NpcActivityController`, `ScheduleResolver`, `ResolutionTrace` | Intenção e snapshot efetivo são públicos; trace e host list não são. Expor leitura/captura somente no Editor e um pedido explícito de reavaliação consumido pelo próximo tick. Não calcular uma segunda decisão nem prever próximos compromissos ausentes no contrato. |
| Execução e eventos | `ActivityCoordinator`, `ActivitySnapshot`, `ActivityExecutionEvent` | Já fornecem estado, etapa, fallback, razão, recurso, ID e eventos de transição; controller hoje só escreve eventos no Console. Um evento Editor-only alimentará ring buffer de 512 sem logs por frame. |
| Navegação/animação | `IslandWaypointWalker`, `NpcUnityActivityPorts`, `NpcAnimatorBindings` | Reusar status, path/cantos existentes, target simbólico e o estado atual do Animator. Campos privados sem fonte consultável serão apresentados como indisponíveis, sem cálculo ou reflexão. |
| Recursos | `SmartObjectRegistry`, `SmartObject`, `InteractionSlot`, `SmartObjectReservationOwner` | Registry permite reservar/buscar, mas não enumerar. Adicionar cópia de leitura Editor-only dos objetos registrados; ler todos os slots diretamente, sem chamar `FindCandidates` (que omite slots ocupados). Nunca liberar slot pela janela. |
| Controles | Host/coordinator e clock existentes | Cancelamento usa `CancelExternal`; cleanup e retomada ficam com o executor. Reavaliação sinaliza o host sem reiniciar compromisso ativo. Pausa é apenas calendário. Velocidade multiplica avanço do calendário no mesmo clock; movimento continua em tempo físico normal. Override permanece desabilitado com razão explícita. |
| Interface e API | Padrão de Editor existente | EditorWindows atuais usam `OnGUI`; a janela e API ficam em `Assets/_Project/Editor/NpcDebug/` (assembly Editor padrão), com DTOs sem referências vivas. Menu `Freyja/NPC/Debug Monitor`. |

O relatório de `093` permanece **PARCIAL**. A evidência real anterior cobre rotinas dos três moradores, assentos e cleanup, mas não aceite visual da pose sentada. No baseline, o Console reportava 22 erros correntes/2 warnings; os 12 mais recentes amostrados eram falhas antigas de transporte do Unity Pipeline, anteriores a esta implementação. A compilação estava parada e sem falha (`compilationFailed=false`, `compiling=false`); não limpar nem atribuir os erros existentes à feature.

## Gate da constituição

- **Passa**: observa os sistemas de jogo existentes e não altera canon/GDD nem lógica narrativa.
- **Passa**: Unity MCP é o ambiente de integração e validação solicitado.
- **Passa**: a UI é ferramenta interna de desenvolvimento e não entra no gameplay/build release.
- **Passa com limite documentado**: campos não expostos permanecem indisponíveis; sem estimativa inventada de desempenho para 30 NPCs.
- **Passa**: preserva cena, rotinas JSON, GUIDs e alterações locais da validação anterior.

## Pesquisa

Ver [research.md](research.md), baseado na auditoria de contratos e relatórios `093`, `092` e `090`.

## Modelo e contratos

Ver [data-model.md](data-model.md) e [contracts/editor-diagnostics-api.md](contracts/editor-diagnostics-api.md). O contrato de Editor contém DTOs de cópia defensiva, filtros, captura, histórico, exportação e comandos explícitos. Ele não expõe APIs aos builds de jogador.

## Estrutura

```text
Freyja/Assets/_Project/
├── Editor/NpcDebug/
│   ├── NpcDebugDiagnostics.cs       # API Editor, snapshots e comandos
│   ├── NpcDebugModels.cs            # filtros e DTOs puros serializáveis
│   ├── NpcDebugEventBuffer.cs       # histórico limitado e lifecycle Editor
│   ├── NpcDebugDiagnostics.cs       # inclui serializer estável e gravação JSON fora de Assets
│   ├── NpcDebugMonitorWindow.cs     # IMGUI, seleção, filtros e controles
│   └── NpcDebugSceneOverlay.cs      # SceneView passiva, subscriptions curtas
├── Scripts/
│   ├── Environment/CoastalDayNight.cs                 # rate de calendário Editor-only
│   ├── NpcScheduling/Unity/NpcWorldRuntimeDriver.cs   # hosts registrados Editor-only
│   └── NpcScheduling/Unity/NpcActivityController.cs  # trace, pedido de reavaliação e evento Editor-only
├── SmartObjects085/Runtime/SmartObjectRegistry.cs # cópia da enumeração Editor-only
└── Editor/NpcDebug/Tests/ (se suportado pelo assembly/test runner atual)
```

Não criar asmdef runtime para incluir UnityEditor; a API é compilada na assembly Editor padrão que já alcança `Assembly-CSharp`. O evento condicional do controller é removido fora do Editor. Provas isoláveis serão executadas via Unity MCP/editor; eventuais testes formais só serão adicionados se a fronteira de assembly atual permitir sem refatoração ampla.

## Plano de execução

1. Implementar DTOs/enum status, ring buffer, captura e filtros em API pública Editor-only, com cópias de valores e ordem determinística.
2. Acrescentar pontos de observabilidade condicionais no controller, driver, clock e registry sem criar autoridade nova; usar a API real do host/executor para reavaliação/cancelamento.
3. Implementar JSON UTF-8 dentro de `NpcDebugDiagnostics`, com schema versionado, campos não finitos como null + estado, e destino de exportação sob `Temp/NpcDebugTools/` na raiz do repo; documentar opção de saída.
4. Implementar monitor IMGUI com polling máximo 2 Hz, seleção/search/filtros, detalhe/histórico, controles condicionais e mensagens de capacidade.
5. Implementar SceneView passiva usando path já calculado e referências reais, com seleção como padrão e modo todos opcional.
6. Validar compilação/Console, API e export com Unity MCP, Edit Mode/Play Mode físico com três NPCs e lifecycle repetido; capturar JSON real, imagens úteis, custos comparáveis e resultados D01–D26.

## Estratégia de validação

- API/JSON sem janela: chamar método Editor público via Unity MCP `script_eval`, validar parse JSON, filtros, ordenação e snapshot individual/global.
- Editor: abrir/fechar janela em Edit e Play Mode, confirmar menu, comandos desabilitados fora de Play, seleção/filtros e exportação.
- Play Mode: consultar idle, wander, contenda de bancos, cancelar uma execução, observar cleanup e cruzar uma fronteira do relógio; confirmar o resultado pelo snapshot e estado vivo.
- Lifecycle: fechar/reabrir monitor e repetir start/stop de Play Mode ao menos três vezes; confirmar subscriptions, pause/rate, overlay e buffers sem órfãos.
- Performance: amostras comparáveis janela fechada/aberta em mesma cena, três NPCs e velocidade; registrar valores e limites observados, sem extrapolar para 30 NPCs.
- Não declarar PASS para override, rota bloqueada ou animação associada à execução se não houver fonte ou teste real para provar.

## Complexidade

Nenhuma violação da constituição. O pequeno ponto de instrumentação condicional no controller e clock é justificado por observabilidade/controle do runtime real; todos os outros dados e comandos permanecem Editor-only.
