# Resumo de testes — npc-integration-validation

Execuções da baseline e regressão final após correção de lifecycle.

| Grupo | Resultado |
|---|---|
| .NET `--test` | PASS — 174 assertions. |
| .NET `--validate` | PASS — 5 schedules, 15 atividades, 3 overrides, 3 receitas. |
| .NET `--simulate-week` (2 vezes) | 7 dias lógicos; 3 NPCs fake; 1.047 eventos; 2 falhas simuladas; 19 cancelamentos; 16 reservas/16 liberações; 0 claims residuais; `physicalValidation=false`. Hash idêntico: `3d87465176c8e8d9d7c5a1d2d2031fc80822abe80e163203ddf5f7049c9b9257`. |
| Unity Edit Mode final | 40/40 pass, 0 fail/skip. |
| Unity Play Mode focal | SmartObjects 7/7 e NpcAnimation 1/1, 0 fail/skip. Inclui `NpcGameObjectDisableClearsAdapterStateForReactivation`. |
| Unity Play Mode completo final | 9/9 pass, 0 fail/skip: 7 SmartObjects, 1 NpcAnimation, 1 WorldBuilder. Inclui disputa/fallback entre bancos, approach inacessível, lifecycle owner e nova regressão de reativação. |

Limite de descoberta: não havia assemblies Unity de teste para Schedule, Activity Execution, Navigation ou World Integration. O smoke físico adicional de disable/reativação do Ivo também passou na cena principal. Testes históricos W01–W19 e seus limites estão em `specs/092-npc-world-integration/validacao.md`.
