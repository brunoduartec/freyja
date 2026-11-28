# Contrato: API Editor de diagnóstico NPC

Assembly: `Assembly-CSharp-Editor` padrão; namespace proposto `Freyja.NpcDebug.Editor`. A assinatura final deve acompanhar os tipos reais do projeto e permanecer compilável apenas no Editor.

## Consultas sem efeitos

```csharp
NpcDebugSnapshot CaptureSnapshot(NpcDebugFilter filter = null);
NpcDebugNpcSnapshot CaptureNpc(string npcId);
NpcDebugSmartObjectSnapshot[] CaptureSmartObjects(NpcDebugSmartObjectFilter filter = null);
NpcDebugEvent[] GetRecentEvents(NpcDebugEventFilter filter = null, int limit = 100);
string CaptureJson(NpcDebugFilter filter = null, bool includeEvents = true);
string CaptureNpcJson(string npcId, bool includeEvents = true);
```

- Métodos só leem o estado já existente; não chamam resolver, coordenador, pathfinding, Animator mutável, reservas ou ticks.
- Coleções retornadas são cópias defensivas ordenadas por ID/ordem estável.
- DTOs contêm apenas valores serializáveis, status de disponibilidade e números finitos; não retêm referências Unity.
- `CaptureJson` e `CaptureNpcJson` não escrevem arquivo. Exportação é um comando Editor separado que devolve o path relativo e o resultado real da escrita.

## Comandos explícitos

```csharp
NpcDebugCommandResult SetClockPaused(bool paused);
NpcDebugCommandResult AdvanceClockMinutes(int minutes); // 1..1439, positivo
NpcDebugCommandResult SetClockRate(float rate); // apenas os rates listados pelo clock
NpcDebugCommandResult RequestReevaluation(string npcId);
NpcDebugCommandResult CancelActivity(string npcId);
NpcDebugCommandResult ExportJson(string npcIdOrNull, bool includeEvents);
```

- Comandos exigem Play Mode, Editor/desenvolvimento e fontes de runtime prontas.
- `AdvanceClockMinutes` encaminha para o relógio existente e não permite recuo. A projeção existente define a mudança de dia; saltos não marcam etapas puladas como realizadas.
- `SetClockPaused` altera apenas avanço do calendário. É restaurado ao fechar janela/encerrar Play Mode apenas se o valor ainda corresponder ao último valor aplicado por esta ferramenta.
- `SetClockRate` usa um multiplicador transitório no próprio relógio; nunca escreve `Time.timeScale` e não acelera movimento/animações.
- `RequestReevaluation` sinaliza resolução no próximo tick real do driver; não cria uma execução duplicada se o compromisso continua igual.
- `CancelActivity` chama o cancelamento do host existente; cleanup pode ficar pendente e ownership não é liberado pela API de diagnóstico.
- Override retorna `unsupported` com razão `no_temporary_override_contract`; não modifica JSON de rotina nem cria um sistema paralelo.

## Eventos e lifecycle

- `NpcActivityController` publica `ActivityExecutionEvent` sob `#if UNITY_EDITOR`; o Editor assina sem depender da janela aberta e mantém ring buffer de 512.
- Assinaturas são iniciadas uma vez na sessão do Editor, limpas/repostas em assembly reload e transições de Play Mode; o buffer da sessão anterior é marcado como encerrado.
- `NpcWorldRuntimeDriver` expõe cópia dos hosts registrados e `SmartObjectRegistry` expõe cópia dos objetos registrados somente ao código Editor.
- Scene View usa somente dados de captura, registra `SceneView.duringSceneGui` ao habilitar overlay e remove assinatura em fechar/desabilitar/Play Mode exit.

## JSON e comandos locais

- UTF-8, `schemaVersion: 1`, chaves em ordem estável, números com cultura invariável, valores não finitos como `null` com status explicativo.
- Caminhos exportados são relativos ao repositório sob `Temp/NpcDebugTools/`; conteúdo não contém caminho pessoal absoluto.
- Menu: `Freyja/NPC/Debug Monitor`. API via `script_eval` Unity, por exemplo `Freyja.NpcDebug.Editor.NpcDebugDiagnostics.CaptureJson()`; confirmar o nome/retorno final na ferramenta após compile.
- Não há endpoint HTTP/MCP novo. O MCP Unity existente continua sendo o canal de execução no Editor.
