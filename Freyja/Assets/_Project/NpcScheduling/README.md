# NPC Schedule Core (fixtures sintéticos)

Esta ferramenta resolve **intenções planejadas** com o mesmo C# puro usado pelo harness. Não executa movimento, animação ou atividade e não consulta Unity/Smart Objects. Todos os NPCs e destinos deste diretório são demonstrações fictícias; `Day 1–7` é calendário técnico sintético, não duração da campanha ou lore. `home`, `pier`, `fish_market`, `village_square`, `farm`, `demo_cow`, `demo_apiary`, `demo_shed` e `hill_ruins` são exemplos de aliases lógicos sem validação física; `demo_cow` está declarado como alvo lógico `object`, não como Smart Object existente.

## Executar

Na raiz do checkout, com SDK .NET 11:

```sh
dotnet run --project Tools/NpcScheduleSimulator -- --validate
dotnet run --project Tools/NpcScheduleSimulator -- --test
dotnet run --project Tools/NpcScheduleSimulator -- --query demo_fisherman 2 07:00 rain
dotnet run --project Tools/NpcScheduleSimulator -- --query demo_fisherman 4 23:00 clear demo_secret_meeting_unlocked=true
dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week
```

`--query` recebe clima opcional e depois flags em lista `nome=true|false`. O parser estrito compartilhado em `Core/StrictJson.cs` rejeita JSON malformado, propriedades desconhecidas/duplicadas e não depende de `System.Text.Json`, Unity ou pacotes. A CLI é a borda que faz I/O e conversão para os modelos. `--validate` também verifica versões, IDs, referências, atividades, alvo obrigatório, intervalos, operações, sobreposição na agenda-base e em cada regra. Lacunas são válidas e geram aviso; em runtime resultam em `idle`. Conflitos estáticos entre overrides de mesma prioridade geram aviso com desempate ordinal. O autoteste não usa framework ou pacote NuGet e executa assertions contra o resolver e loader reais.

## Arquivos de autoria

- `Data/activities.json`: catálogo e tipo de alvo (`none`, `location`, `object`); cadastrar atividade significa apenas declarar um contrato.
- `Data/templates/*.json`: agenda reutilizável com `entries` ordenadas por horário.
- `Data/npcs/*.json`: `templateId` opcional, agenda própria (`entries`), `replaceEntries`, `removeEntries` e `addEntries`. Substituir/remover exige ID existente; adição exige ID novo.
- `Data/overrides/*.json`: `npcId`, `priority` 50 (mundo/clima/calendário), 70 (missão), 90 (narrativa), `conditions` e entries.
- `Data/schema/v1.json`: JSON Schema 2020-12 versionado para catálogo, template, agenda de NPC e override. O loader é a validação semântica de referências cruzadas, alvos e conflitos; o JSON Schema não cobre essas relações.

Exemplo de entrada: `{"id":"market","start":"08:00","end":"12:00","activityId":"sell_goods","targetId":"demo_market"}`. Para adicionar um NPC, crie um arquivo em `npcs/` com `schemaVersion: 1` e `templateId`, ou forneça `entries` próprias. Uma alteração de horário/atividade no JSON entra em vigor sem recompilar o domínio.

### Receitas rápidas

Cadastrar uma atividade declarativa em `activities.json`:

```json
{"id":"tend_bees","targetKind":"location","label":"Demo tend bees"}
```

Criar um NPC de demonstração associado a uma profissão/template em `npcs/demo_beekeeper.json`:

```json
{"schemaVersion":1,"npcId":"demo_beekeeper","templateId":"resident","replaceEntries":[{"id":"morning","start":"08:00","end":"12:00","activityId":"tend_bees","targetId":"demo_apiary"}]}
```

Para manter a agenda de 24 horas sem sobreposição, substitua o bloco `morning` de `resident` (08:00–12:00). Para trocar o horário, preserve o ID do bloco e edite `start`/`end`; para adicionar outro bloco, remova ou remaneje um existente para liberar esse intervalo.

Adicionar uma exceção climática em `overrides/demo_beekeeper_rain.json`:

```json
{"schemaVersion":1,"id":"demo_beekeeper_rain","npcId":"demo_beekeeper","priority":50,"conditions":{"weather":["rain"]},"entries":[{"id":"rain_work","start":"08:00","end":"12:00","activityId":"repair","targetId":"demo_shed"}]}
```

O ID da atividade deve existir no catálogo, os blocos de uma mesma regra não podem sobrepor-se e o destino é somente uma referência lógica. Dia/clima/flags presentes são combinados por AND; arrays de valores do mesmo campo são OR.

IDs de horários usam minutos inteiros e intervalos `[start,end)`. O fixture começa a semana na segunda-feira (`weekday 0`), logo domingo é `6`; `absoluteDay` continua começando em 1. `24:00` só pode ser final. Para atravessar meia-noite, divida em dois blocos: `22:00–24:00` e `00:00–06:00`; o segundo bloco pertence ao dia absoluto seguinte. Condições de dia aplicam-se a cada bloco. Campos de condição distintos são AND; listas do mesmo campo são OR. Flag ausente não corresponde nem a `true` nem a `false`.

Uma mesma agenda-base não pode sobrepor horários. Overrides podem sobrepor base, mas seus próprios entries não podem sobrepor. Em conflitos de prioridade igual, o ID ordinal menor é escolhido e o trace informa o empate. Organize IDs/targets estáveis e confira a saída de validação depois de editar.

Um adaptador futuro pode consultar e entregar a proposta ao limite do executor:

```csharp
ResolutionResult result = resolver.Resolve(npcId, timeSnapshot, worldSnapshot);
ActivityIntent intent = result.Intent; // proposta planejada, não confirmação da ação
// Um executor separado validará destino/contexto e tentará executar a atividade.
```

Esse adaptador deve reavaliar quando contexto mudar e confirmar separadamente qualquer ação física. O timestamp/trace desta simulação registra intenção planejada; não é prova, testemunho ou posição observada.

## Integração futura

Mapeie relógio/calendário do jogo para `GameTimeSnapshot`, clima e flags explícitas para `WorldContextSnapshot`; valide o mapeamento em uma feature separada. Depois, um adaptador Unity pode receber `ActivityIntent`, resolver os destinos simbólicos com dados autorizados e delegar ao executor apropriado. Esta entrega não assume que qualquer destino ou executor já exista.
