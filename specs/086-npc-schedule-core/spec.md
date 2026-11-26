# Freyja — NPC Schedule Core

## 1. Missão

Implemente a feature `npc-schedule-core`: núcleo determinístico, orientado a dados e independente da execução da Unity, responsável por decidir qual atividade cada NPC deveria realizar em função do horário, calendário e contexto do mundo.

O resultado deve permitir criar e alterar rotinas, associar atividades reutilizáveis, herdar templates de profissão e simular vários dias sem abrir a Unity. A saída é uma intenção de atividade (`ActivityIntent`), não uma ordem de movimentação nem a confirmação de que uma ação aconteceu.

### Contexto e restrições

- Freyja é um jogo de mistério em uma ilha pequena, com três protagonistas jogáveis, rotinas de moradores e eventos narrativos relacionados à possessão.
- Existe uma feature de Smart Objects que o Codex informou ter implementado, mas cuja validação física ainda está pendente. Ela pode estar em outra branch ou somente no Mac. **Não dependa dela, não a recrie e não a considere validada.**
- O Mac do desenvolvedor está temporariamente indisponível. Não tente acessar o Mac, Unity MCP, Blender, Tripo AI, Play Mode, cenas, prefabs ou assets. Trabalhe com o checkout remoto disponível e testes de C# independentes da Unity.
- Se não houver acesso ao checkout, entregue apenas a especificação/plano e registre o bloqueio; não invente alterações ou resultados. Se o checkout estiver disponível, implemente de fato.
- Não introduza dependências comerciais, LLMs em runtime, GOAP, Behavior Trees, ECS/DOTS ou frameworks genéricos de IA.
- Não altere a lore, os NPCs oficiais, o relógio ou os sistemas narrativos sem evidência de necessidade e autorização. Os dados de exemplo são fictícios.

## 2. Fluxo de trabalho e economia de tokens

1. Leia `AGENTS.md` e instruções locais aplicáveis, a constituição e as convenções do Spec Kit, as specs relevantes e somente os pontos de integração necessários. Verifique branch, estado do Git, versão de Unity/API C# e disponibilidade de SDK .NET e infraestrutura de testes. Não descarte alterações existentes.
2. Utilize o fluxo Spec Kit existente (specify/plan/tasks/implement e validações, conforme configuração real). Reutilize specs já existentes quando aplicável, sem duplicação. Identifique claramente esta feature como independente de Smart Objects.
3. Use os agentes já configurados, se estiverem disponíveis: Game Designer revisa escopo, coerência dos horários e implicações de investigação; Implementador escreve código, dados e testes; Validador revisa contratos e executa os testes em uma etapa independente. Se o ambiente não oferecer subagentes, execute essas responsabilidades sequencialmente e informe a limitação. Não instale frameworks ou novos agentes nesta feature.
4. Trabalhe por fatias pequenas: domínio/resolver, configuração/validação, dados de exemplo, simulador/testes, documentação. Compartilhe entre agentes apenas contratos, paths, diffs e resultados relevantes. Não repita leituras completas do repositório, não despeje logs extensos nem faça polling de ferramentas indisponíveis.
5. Não encerre apenas com specs ou tasks se houver ambiente capaz de implementar. Não realize push, merge ou alterações em branches alheias sem autorização. Preserve a integração futura com o projeto.

## 3. Arquitetura e fronteiras

```text
JSON (catálogo, templates, NPCs, overrides)
  -> loader + validator + template expansion
  -> resolved schedule data
GameTimeSnapshot + WorldContextSnapshot
  -> ScheduleResolver (C# puro)
  -> ActivityIntent + ResolutionTrace
  -> [executor simulado nesta etapa]
  -> [ActivityExecutor / Smart Objects / NavMesh / Animator no futuro]
```

O núcleo não referencia `UnityEngine`, `UnityEditor`, `MonoBehaviour`, `GameObject`, `Transform`, NavMesh, Smart Objects ou singleton de cena. Não use `DateTime.Now`, relógio real, I/O ou aleatoriedade no resolver. Entradas explícitas, dados imutáveis ou tratados como somente leitura, resultado determinístico e sem efeitos colaterais. Parsing e I/O ficam nas bordas. Uma mesma chamada não deve disparar ações nem escrever estado narrativo.

Organize de acordo com os padrões existentes. Nomes sugeridos (adapte com justificativa):

- `GameTimeSnapshot`: dia absoluto da campanha (convenção documentada), minuto do dia e dia da semana derivado de um calendário explícito; não aceite campos contraditórios.
- `WorldContextSnapshot`: clima e flags narrativas tipadas, com semântica explícita para flags ausentes.
- `ActivityDefinition`: ID estável, descrição/label opcional e tipo de alvo aceito (sem alvo, localização lógica ou objeto lógico); identifica capacidades futuras, não implementa animações.
- `ScheduleEntry`: ID estável, intervalo, activity ID, alvo e parâmetros mínimos, se necessários.
- `ScheduleTemplate`, `NpcScheduleDefinition`, `ScheduleOverride`.
- `ActivityCatalog`, `ScheduleDataValidator`, `ScheduleResolver`, `ActivityIntent`, `ResolutionTrace`.
- `ScheduleSimulator` e executor simulado/recording sink para registrar mudanças de intenção.

Não crie interfaces para tudo: interfaces apenas onde há uma fronteira real, como fonte de dados ou futura ponte de execução. Não crie um novo relógio de gameplay; se existir um contrato de relógio, documente como mapear seu estado para o snapshot. Não implemente ponte Unity nesta etapa, salvo se já houver um adaptador puro que possa ser reaproveitado sem dependências.

## 4. Dados e autoria

JSON é a fonte de verdade para agendas e catálogo. Use um formato legível, versionado (`schemaVersion: 1`), IDs estáveis em snake_case e arquivos pequenos. Reaproveite o serializador/estrutura do projeto se compatível com Unity e com testes fora da Unity; caso contrário, escolha um parser suportado pelos dois ambientes, explicite a decisão e evite uma segunda implementação divergente. Não dependa de recursos de linguagem incompatíveis com a versão Unity do projeto. Se a versão não puder ser verificada, use um subconjunto conservador de C# e documente a suposição.

Estrutura ilustrativa (adapte à árvore real):

```text
NPC/Scheduling/
  Core/
  Data/
    activities.json
    templates/
      fisherman.json
      farmer.json
      livestock_keeper.json
      merchant.json
      resident.json
    npcs/
      demo_fisherman.json
      demo_farmer.json
      demo_livestock_keeper.json
      demo_merchant.json
      demo_resident.json
    overrides/
      demo_story_event.json
    schema/
  Tests/
  Tools/ScheduleSimulator/
  README.md
```

Mantenha arquivos .cs compartilhados entre Unity e harness externo; não copie o domínio para outro projeto. Mantenha fontes e build artifacts do harness fora da importação de assets da Unity (ou use isolamento já adotado). Não mova pastas/asmdefs existentes sem necessidade. Não invente GUIDs de assets nem altere arquivos de cena.

### 4.1 Catálogo inicial

IDs: `idle`, `wander`, `sit`, `sleep`, `eat`, `rest`, `chat`, `fish`, `sell_goods`, `water_crops`, `harvest`, `feed_animals`, `milk_cow`, `repair`, `secret_meeting`.

São **definições declarativas**, não mecânicas implementadas. Um ID reconhecido pelo catálogo não significa que um executor físico exista. `idle` pode operar sem alvo como fallback. Os demais IDs devem explicitar se exigem um alvo; IDs de destinos como `pier`, `home`, `village_square`, `farm`, `fish_market` são referências simbólicas, não IDs de Smart Objects confirmados. Não tente resolvê-los contra cenas nesta fase. Se necessário, forneça uma lista de aliases lógicos de demonstração para validar a consistência dos fixtures.

### 4.2 Exemplo de template

```json
{
  "schemaVersion": 1,
  "id": "fisherman",
  "entries": [
    {"id":"sleep_morning","start":"00:00","end":"06:00","activityId":"sleep","targetId":"home"},
    {"id":"breakfast","start":"06:00","end":"07:00","activityId":"eat","targetId":"home"},
    {"id":"morning_work","start":"07:00","end":"12:00","activityId":"fish","targetId":"pier"},
    {"id":"lunch","start":"12:00","end":"13:00","activityId":"eat","targetId":"home"},
    {"id":"afternoon_work","start":"13:00","end":"17:00","activityId":"sell_goods","targetId":"fish_market"},
    {"id":"social","start":"17:00","end":"20:00","activityId":"chat","targetId":"village_square"},
    {"id":"rest","start":"20:00","end":"22:00","activityId":"rest","targetId":"home"},
    {"id":"sleep_night","start":"22:00","end":"24:00","activityId":"sleep","targetId":"home"}
  ]
}
```

### 4.3 Personalização individual

```json
{
  "schemaVersion": 1,
  "npcId": "demo_fisherman",
  "templateId": "fisherman",
  "replaceEntries": [
    {"id":"social","start":"17:00","end":"20:00","activityId":"rest","targetId":"home"}
  ]
}
```

Suporte herança de um template por NPC, substituição integral de um bloco por ID estável, remoção explícita e adição de blocos. Um NPC sem template pode definir agenda própria. Proíba IDs duplicados, remoções/substituições de blocos inexistentes, operações conflitantes sobre um mesmo ID e herança recursiva nesta versão. Valide a agenda resultante, não apenas o template. Templates não devem ser mutados ao personalizar NPCs.

### 4.4 Exceções condicionais

```json
{
  "schemaVersion": 1,
  "id": "demo_rain_repair",
  "npcId": "demo_fisherman",
  "priority": 50,
  "conditions": {"weather": "rain"},
  "entries": [
    {"id":"rain_work","start":"07:00","end":"12:00","activityId":"repair","targetId":"home"}
  ]
}
```

Crie também exemplos de regra semanal (ex.: domingo), e narrativa (ex.: dia 4, flag `demo_secret_meeting_unlocked`, 23:00–23:45, `secret_meeting`, destino lógico `hill_ruins`). Condições mínimas: dia absoluto, dia da semana, clima e flags de história booleanas. Condições especificadas são combinadas por AND, listas de valores por OR dentro do mesmo campo, se suportadas; flags ausentes não satisfazem a condição, inclusive quando a condição pede `false`, a menos que o contrato existente tenha semântica explícita diferente. Não implemente linguagem de expressões arbitrárias, scripting ou sistema de quests.

## 5. Regras determinísticas de resolução

1. Normalize/valide dados uma vez e expanda templates antes da resolução. Use minuto inteiro do dia (0–1439) e intervalos semiabertos `[start, end)`. `24:00` é permitido somente como limite final, equivalente a 1440. `00:00–24:00` é um dia inteiro. Intervalos de duração zero são inválidos.
2. Para esta versão, represente blocos que cruzam a meia-noite como dois blocos explícitos (ex.: 22:00–24:00 e 00:00–06:00). Rejeite `end < start` com instrução de correção. Não perca o dia absoluto ao avançar de 23:59 a 00:00. Em exceções noturnas, a condição de dia se aplica a cada bloco; configure a segunda parte para o dia seguinte quando necessário. Documente este limite de autoria.
3. Uma agenda-base não pode ter sobreposição. Lacunas são permitidas, mas geram aviso e `idle` como fallback. Exceções podem sobrepor a base deliberadamente. Cada exceção só é candidata quando condições e intervalo são válidos.
4. Prioridades fixas sugeridas: 10 fallback; 30 agenda-base; 50 mundo/clima/calendário; 70 missão; 90 evento narrativo. Reserve 80 para interação com jogador e 100 para estado crítico, sem implementar diálogos ou morte nesta etapa. Override declara prioridade válida (50, 70 ou 90); o maior valor ganha. Em empate de overrides aplicáveis, use ID estável em ordem ordinal como desempate e emita diagnóstico explícito. Nunca dependa da ordem dos arquivos ou da iteração de dicionários. Dentro de uma mesma regra, blocos sobrepostos são inválidos. Validador sinaliza conflitos estáticos detectáveis entre regras de mesma prioridade; não tente provar satisfatibilidade arbitrária.
5. Ao sair de uma exceção, recalcule o bloco válido no horário atual. Não retome automaticamente uma tarefa que já expirou. Reavalie em saltos de tempo ou mudanças de contexto, sem simular deslocamento físico. Não faça replay de eventos que não ocorreram.
6. Retorne `ActivityIntent` com NPC ID, activity ID, target ID opcional, janela efetiva de validade (até a próxima possível mudança ou no máximo o limite do bloco; reavaliar imediatamente quando contexto mudar), prioridade, origem e ID do bloco/regra. Fallback tem validade limitada e deve ser reavaliado na próxima fronteira pertinente. Inclua `ResolutionTrace` compacto com motivo da escolha e eventuais conflitos. Não inclua conhecimento do jogador ou afirmações sobre a localização efetiva do NPC.
7. O resolver é puro: repetir a mesma entrada produz saída equivalente. O simulador emite uma transição de intenção quando a identidade relevante da intenção muda (atividade, alvo, origem/bloco e prioridade), não a cada tick. Não trate mudanças apenas de timestamp de consulta como novas atividades. Registre horário planejado/observado pelo simulador e origem; não chame o registro de testemunho ou prova.

## 6. Validação dos arquivos

Entregue um schema JSON versionado ou equivalente documentado para cada tipo de arquivo e um validador semântico executável. Se usar JSON Schema, não alegue que ele valida referências cruzadas sozinho. Valide:

- JSON inválido, versão desconhecida, campos obrigatórios, nomes/IDs duplicados, IDs de atividades desconhecidos e alvos obrigatórios ausentes;
- formato e limites de horas/minutos, intervalos vazios/invertidos, sobreposições e lacunas;
- referências a templates/NPCs inexistentes e operações individuais inválidas;
- prioridades, condições e valores inválidos; campos desconhecidos no schema v1 devem ser rejeitados ou diagnosticados claramente para detectar typos;
- referências simbólicas a destinos: validação de formato e, quando disponível, catálogo lógico de fixtures; validação física de objetos fica pendente.

Diagnósticos devem indicar arquivo, NPC/template/regra, entry ID e motivo, com níveis erro/aviso. O harness deve falhar com código de saída não zero em erro de configuração, mas não por lacunas permitidas. Não ignore dados inválidos silenciosamente.

## 7. Simulação sem Unity

Crie um pequeno harness CLI .NET, ou reaproveite o harness independente existente, que compile as mesmas fontes de domínio usadas pelo projeto Unity. Utilize o SDK e ferramentas disponíveis no ambiente; se o .NET não estiver instalado, registre o bloqueio, não faça instalação invasiva sem autorização nem simule resultados. Documente comandos reproduzíveis reais. Testes de C# devem rodar sem Editor, licença de Unity ou MCP.

A CLI deve permitir validar dados, consultar a intenção de um NPC em um dia/horário/contexto, e simular sete dias para todos os NPCs de demonstração. Permita configurar clima/flags por fixture, incluindo um dia chuvoso e um evento narrativo. Use passos de um minuto ou fronteiras de mudança; para cinco NPCs isso é simples e suficiente. Ofereça saída JSON/CSV estruturada ou texto tabular compacto, com transições, não logs a cada minuto. Não crie interface web ou serviço remoto.

Exemplo conceitual de saída (não é evidência de execução):

```text
Day 1 07:00 demo_fisherman fish pier base:morning_work
Day 2 07:00 demo_fisherman repair home override:demo_rain_repair
Day 4 23:00 demo_fisherman secret_meeting hill_ruins override:demo_story_event
Day 4 23:45 demo_fisherman sleep home base:sleep_night
```

## 8. Dados de demonstração

Crie cinco templates e cinco NPCs fictícios: pescador, agricultor, criador de animais, comerciante e morador. Cada rotina deve ser coerente e cobrir 24 horas, com sono, alimentação, trabalho/ocupação, socialização e descanso. Pelo menos um NPC demonstra personalização de template. Inclua as exceções climática, semanal e narrativa descritas acima. Não associe automaticamente estes dados a habitantes reais do Freyja nem altere a população oficial. Use os horários ilustrativos como ponto de partida e documente ajustes feitos pelo Game Designer.

## 9. Testes obrigatórios

Cubra com testes executáveis:

- resolução de blocos no início, meio e limite final; 23:59/00:00, dia absoluto e dia da semana; limites inválidos e bloco de dia inteiro;
- divisão explícita de rotina noturna, incluindo exceções em dias consecutivos;
- carregamento de JSON, versões, IDs desconhecidos, dados malformados, referências quebradas, sobreposição, lacunas e alvo obrigatório;
- herança, substituição, remoção, adição e isolamento entre dois NPCs do mesmo template;
- prioridades de clima/semana/missão/narrativa; condições AND, flags ausentes, empate determinístico, ordem de leitura invertida;
- retorno à agenda correta quando uma exceção termina, inclusive quando atravessou vários blocos; mudança de contexto no mesmo minuto e saltos de horário;
- fallback seguro e sua reavaliação quando a próxima atividade começa;
- determinismo e ausência de mutação das entradas; duas simulações idênticas produzem transições equivalentes;
- emissão somente de transições significativas; simulação de sete dias com cinco NPCs sem exceções inesperadas, cobrindo chuva, rotina semanal e evento narrativo;
- teste de edição data-driven: alterar horário ou atividade de um fixture JSON muda o resultado sem recompilar lógica de domínio.

O Validador deve verificar especificamente se o núcleo referencia Unity/Smart Objects, se dados de exemplo não foram adicionados como NPCs reais e se os testes exercitam o código efetivo. Não confunda testes escritos com testes executados. Reporte contagens e comando/exit code somente se observados.

## 10. Fora do escopo

- Executor físico de atividades, pathfinding, animações, reservas e integração com Smart Objects;
- criação de modelos, NPCs ou objetos na Unity, alteração de cenas/prefabs, distribuição de bancos;
- editor visual de agenda, profiler Unity, Play Mode, validação visual, build do jogo;
- diário de atividades reais, testemunhos, conhecimento por protagonista, persistência em save game e regras complexas de possessão;
- autonomia probabilística, necessidades fisiológicas, comportamento econômico, crafting, sistemas de plantação/pesca;
- mudanças em produção, integrações cloud novas ou instalação de plugins.

## 11. Definition of Done e entrega

A feature lógica está pronta quando catálogo e dados existem, agendas são editáveis sem alterar C#, o resolver é determinístico, o harness usa o mesmo código do domínio, a validação reporta erros localizáveis, a simulação de sete dias funciona e os testes obrigatórios executáveis passam. Se o ambiente não permitir executar testes, marque a feature como implementada mas não validada (ou parcial, conforme o caso). Compatibilidade/compilação dentro da Unity, integração física e Play Mode continuam pendentes mesmo se todos os testes .NET passarem.

Entregue README com os comandos reais de validação/teste/simulação, exemplos de como cadastrar uma atividade, criar um NPC, editar um horário, associar uma profissão, criar uma exceção e integrar futuramente `ActivityIntent` ao executor. Explique como evitar conflitos, como configurar meia-noite e quais IDs são fictícios.

Relatório final curto e verificável:

1. Branch/checkout utilizado e estado da implementação; arquivos relevantes criados/modificados e specs/tasks.
2. Decisões e desvios em relação ao documento, com motivos.
3. Comandos executados, resultado/contagem dos testes, resumo da simulação e diagnósticos reais. Se algo não foi executado, explicite.
4. Onde editar dados e como executar o harness.
5. Pendências para quando o Mac voltar: verificar alterações dos Smart Objects e sua branch, validar Unity em Edit/Play Mode, mapear destinos lógicos e integrar o executor em uma feature separada.

Não faça afirmações de sucesso sem evidência. Finalize esta etapa sem começar automaticamente a próxima.
