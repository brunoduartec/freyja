# Pesquisa: NPC Schedule Core

## Decisões

- **Parser:** parser JSON pequeno em C# puro, na borda, porque o domínio deve compartilhar fonte entre Unity e .NET CLI; `JsonUtility` não oferece validação rígida de campos desconhecidos e uma dependência nova não é necessária.
- **Tempo:** `absoluteDay` começa em 1; dia da semana vem de `weekStartDay` configurável no fixture. `minuteOfDay` é 0–1439. A agenda não é dona do relógio do jogo.
- **Intervalos:** minutos inteiros semiabertos `[start,end)`, com `24:00` aceito apenas como fim. Rotina atravessando meia-noite é cadastrada em dois entries.
- **Condições:** condições de campo diferentes são AND; listas de valores no mesmo campo são OR. Flag narrativa ausente nunca satisfaz a condição.
- **Prioridade/desempate:** only 50, 70, 90 para regras de override; máximo numérico vence; em empate IDs ordinais estáveis ordenam regras e o primeiro ID vence, com diagnóstico no trace.
- **CLI/testes:** console .NET sem pacotes e com modo de autoteste, para executar no ambiente sem Unity e sem restaurar dependências externas.
- **Fixture temporal:** sete dias são carga técnica. Dia 4/evento são sintéticos e não contradizem o limite de três dias de uma campanha canônica.

## Alternativas consideradas

- `JsonUtility`: rejeitada por permissividade/incompatibilidade com validação estrita e harness externo.
- Newtonsoft/System.Text.Json: rejeitados para evitar divergência de disponibilidade entre Unity e .NET e evitar atualizar dependências.
- Framework de teste NuGet: rejeitado por aumentar dependências/restores; autoteste console continuará executando asserts reais contra o domínio.
- Execução física ou consulta à cena: rejeitadas por escopo e porque destinos são IDs lógicos ainda não mapeados.
