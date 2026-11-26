# Pesquisa e decisões

## Fontes observadas

- `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/ScheduleModels.cs`: `ActivityIntent` contém NPC, activityId, targetId, dia, janela, prioridade, origem, sourceId e entryId. Seu `Identity` atual combina atividade/alvo/origem/fonte/bloco/prioridade, sem dia ou parâmetros.
- `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/ScheduleResolver.cs`: resolve uma intenção para um `GameTimeSnapshot` e contexto de mundo; não executa efeitos.
- `Tools/NpcScheduleSimulator/Program.cs`: loader, catálogo, resolver, simulação e CLI existentes.
- `Tools/NpcScheduleSimulator/Tests.cs`: testes anteriores rodam por `--test`, sem framework externo.
- `Freyja/Assets/_Project/SmartObjects085/Runtime/ReservationHandle.cs` e `SmartObjectRegistry.cs`: token e owner validam, ocupam e liberam handle; tipos fazem parte da Unity e não podem ser usados pelo executor puro.
- Unity `6000.3.25f1`; SDK acessível `11.0.100`. Não há Unity/Editor no ambiente do usuário.

## Decisões

### Fonte de verdade e parsing

**Decisão**: manter catálogo e agendas existentes como única fonte das intenções; adicionar JSON de receitas que referencia `activityId`, sem duplicar labels ou metadados. Reusar `StrictJson` e validar referências à carga do harness.

**Razão**: separar o significado da atividade da forma de executá-la e preservar o loader já testado.  
**Alternativa rejeitada**: replicar catálogo/agenda no executor ou criar runtime separado.

### Identidade da execução

**Decisão**: comparar compromisso com dados do intent e ocorrência corrente, não com `ActivityIntent.Identity` sozinho nem com horário do tick. Incorporar dia absoluto/intervalo efetivo para discriminar ocorrência. Atualização de validade da ocorrência vigente atualiza o prazo sem reiniciar.

**Razão**: a identidade atual omite o dia, e intent pode manter IDs em dias recorrentes; bloquear reinício por tick e, ao mesmo tempo, reconhecer novo compromisso.  
**Alternativa rejeitada**: usar `ActivityId` ou referência de objeto como chave única.

### Execução e relógios

**Decisão**: host fornece tempo operacional monotônico em segundos e snapshot de calendário. Cada operação recebe ticks síncronos; deadline de operação usa tempo operacional e `wait` de agenda usa a janela recalculada. Uma redução do tempo operacional ou do calendário é erro de contrato.

**Razão**: timeout independente da velocidade de simulação, sem relógio de parede.  
**Alternativa rejeitada**: `DateTime.Now`, `Task.Delay`, acumular agenda atrasada após salto de dias.

### Handles e concorrência

**Decisão**: port de interação abstrai busca, tentativa de reserva, validade, ocupação e liberação. Token opaco carrega ownership pela execução; fake impõe exclusividade em quatro slots. Cleanup mantém e reporta handle até confirmação de liberação.

**Razão**: respeitar modelo observado do Smart Objects sem acoplar `UnityEngine` nem duplicar reserva em produção.  
**Alternativa rejeitada**: usar só objectId/slotId ou depender do Registry estático.

### Receitas e compatibilidade

**Decisão**: receitas v1 lineares com whitelist fixa; validar argumentos, ordem e recurso adquirido antes de executar. Implementar somente `idle`, `wander`, `sit`. Usar C# e BCL conservadores e código fonte compartilhado entre Unity e harness.

**Razão**: extensibilidade data-driven dentro do limite aprovado, sem linguagem genérica.  
**Alternativa rejeitada**: scripts, expressões arbitrárias, behavior tree ou DSL genérica.

### Harness e validação

**Decisão**: estender CLI e testes existentes; três NPCs fictícios, fixtures não narrativas, eventos ordenados e falhas injetadas deterministically. Executar build, testes prévios, `--validate` e `--simulate-week` via `dotnet` nativo se RTK continuar ausente.

**Razão**: validar integração e não duplicar simulador. Nenhuma simulação certifica NavMesh, presença, Smart Objects no Editor ou Animator.  
**Alternativa rejeitada**: criar um segundo projeto/harness que copie o Schedule Core.
