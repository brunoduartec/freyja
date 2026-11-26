# Evidências — NPC Activity Execution Core

**Branch**: `087-npc-activity-execution-core-latest` (`origin/main` + alterações desta feature)  
**Ambiente**: .NET SDK 11.0.100; projeto Unity 6000.3.25f1 está indisponível para abrir/testar.  
**Limite**: todas as provas abaixo cobrem C# puro e fakes. Não demonstram movimento, animação, presença, Smart Objects ou limpeza física.

## Implementação observada

- `ActivityIntent`, `ScheduleResolver`, catálogo, dados e parser `StrictJson` existentes foram reutilizados. O núcleo não recalcula prioridade/agendas.
- Receitas JSON v1 whitelist executáveis: `idle`, `wander`, `sit`. As demais atividades continuam declarativas e usam fallback limitado quando a intenção chega sem executor.
- O coordenador mantém um compromisso por ocorrência, atualiza prazo sem reiniciar por minuto, cancela antes de substituir e mantém tokens/operações residuais até confirmação de limpeza.
- O cenário de sete dias está em `Tools/NpcScheduleSimulator/Fixtures/execution-week.json`; a agenda passa por `StrictJson` e `ScheduleResolver`, com overrides carregados da fonte existente. Fakes em `Tools/` modelam dois bancos de dois slots, três regiões e falhas injetadas.
- O simulador avança um minuto do calendário por tick; o tempo operacional soma 60 segundos por minuto simulado. Timeouts usam segundos operacionais e expiração usa dia/minuto do jogo.

## Verificações executadas

Todos os comandos foram executados em `Tools/NpcScheduleSimulator/` sem Unity:

| Comando | Resultado |
|---|---|
| `dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj` | Sucesso, 0 warnings e 0 erros. |
| `dotnet run --project Tools/NpcScheduleSimulator -- --test` | `PASS: 135 assertions`, inclui as 68 assertions anteriores do Schedule Core. Três warnings são fixtures temporárias de teste que criam lacunas intencionalmente; a validação dos dados originais não gera esses avisos. |
| `dotnet run --project Tools/NpcScheduleSimulator -- --validate` | `OK: 5 NPCs, 15 atividades, 3 overrides, 3 receitas executáveis`. |
| Duas execuções de `dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week`, comparadas com `cmp` | Idênticas byte a byte; 1.425 linhas; SHA-256 `34d24be74972c0267dbaa0b524d8bb7814576992bdb37c7d7d1e7313785a80ab`. |
| Sumário da semana | 7 dias, 3 NPCs, 86 mudanças de intenção, 1.335 eventos de execução, 2 falhas injetadas, 25 cancelamentos, 21 reservas, 21 liberações e `residualClaims=0`. O cenário full-bank mostra capacidade 2, terceiro claim rejeitado e limpeza em zero. |
| `git diff --check` | Sucesso, sem erros de whitespace. |
| Busca no núcleo por dependências físicas/tempo real | Sem `UnityEngine`, `UnityEditor`, `MonoBehaviour`, `GameObject`, `Transform`, `NavMesh`, `DateTime.Now`, `Thread.Sleep`, `Task.Delay` ou `Random`. |

Os testes cobrem identidade ao resolver minuto a minuto, atualização de validade, meia-noite, regressão de relógio, estados/eventos, receita inválida/sem executor, espera e saída, disputa/alternativa, movimento/alinhamento/ocupação, caminho e animação rejeitados, timeout, cancelamento de operação e retry, remoção, fallback e ausência de recursos residuais. A execução de `sit` no fake comprova somente a sequência de chamadas simbólicas.

## Checklist da futura integração Unity

- [ ] Importar os novos `.cs`, receitas e schema sob `Assets/_Project/`; deixar a Unity gerar os `.meta` e versioná-los sem inventar GUID.
- [ ] Abrir Unity 6.0.3.25f1, validar compilação e Console; confirmar Built-in e pacotes sem alterações.
- [ ] Criar um host no thread principal que injete tempo operacional monotônico e `GameTimeSnapshot`, chame `Tick` e pare o coordenador ao remover/desabilitar NPC.
- [ ] Mapear alvos/regiões simbólicos permitidos a pontos reais; não tratar intents como posição observada ou fato narrativo.
- [ ] Implementar os ports reais de movimento, pose/animação e Smart Objects; adaptar handles opacos aos tokens/owners existentes e só ocupar após chegada/alinhamento.
- [ ] Validar no Play Mode: dois NPCs disputando slots, alternativa/lotação, caminho impossível/timeout, reserva invalidada, animação rejeitada, interrupção em cada fase, falha de cancelamento/retry e remoção de NPC.
- [ ] Confirmar que nenhuma operação ou recurso fica ativo após conclusão/cancelamento; verificar saída de `stand_up` e restauração de pose de forma física.
- [ ] Testar mudança e salto do relógio, meia-noite, atualização de intent, cancelamento externo e callbacks tardios.
- [ ] Confirmar eventos de execução separados da resolução e sem conexão automática ao journal/pistas/diálogo.
- [ ] Capturar a câmera real apenas se a integração alterar aparência; registrar evidência de Play Mode e limitações.

Nenhum item deste checklist foi validado no Editor/Play Mode nesta entrega.
