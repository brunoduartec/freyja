# Validação — Spec 002

Executada em 12/11/2026, Unity6000.3.25f1 Built-in, cena Assets/_Project/Scenes/Cais_Caminhada.unity. Modelo de1,8m produzido via Blender MCP. Três frentes com gpt-6-luna: movimento, câmera e figura; integração e validação pelo agente principal.

## Resultados
| Caso | Resultado e evidência |
| --- | --- |
| Compilação | Concluída com zero erros/avisos de compilação; recompile confirmou up_to_date após revisão da câmera |
| Percurso | Duas idas e voltas reais com WASD, cada perna por cerca de4,7s; cais, junções e degraus sem salto ou travamento |
| Posições | walk-134314.json: cais [0;0,945;7,50]; walk-195228.json: início do caminho [0;1,635;-6,604] |
| Queda real | Saída lateral do cais com A; retorno seguro, recovery=1 em walk-294195.json |
| Axial/diagonal | 0,5999999m em ambos sob mesma duração; controlled-validation.json |
| Câmera | Testes controlados de boom diante de sólidos, near plane e pivô obstruído: todos livres de sobreposição; controlled-validation.json |
| Esc/cursor | Esc libera e mostra instrução; W com cursor livre não move; clique retoma |
| Foco real | Troca real para outro editor liberou captura; focus-loss.json. O campo wHeld=false é estado após a perda de foco; não comprova tecla física ainda pressionada |
| Foco com W | Teste separado com estado sintético de W e callback de perda de foco: movimento bloqueado e retorno exige clique; controlled-validation.json |
| Limites/reset | Bounds recuperam ao spawn com velocidade zerada; controlled-validation.json |
| Reinício sem Domain Reload | initialized=true, captured=false, recovery=0, posição [0;0,94;7,5]; reload-validation.json |
| Configuração de reload | Opções anteriores restauradas para enabled=false, None após o teste |
| Preservação | Hashes de cena anterior, prefab-base e manifest idênticos; source-hashes.json |

## Evidências visuais
walk-134314.png (cais), walk-195228.png (caminho) e walk-294195.png (após recuperação) são capturas da Game View em execução. A sequência ilustra estados do percurso; não é vídeo contínuo. Os testes de câmera/foco controlados complementam a inspeção manual e não são apresentados como todos executados fisicamente pela janela.

## Limites
Figura estática e provisória, sem animações finais. Não foi gerado build. Aprovação do usuário sobre sensação de câmera/movimento e proporções continua pendente. A tentativa final de ampliar a inspeção por UI encontrou noWindowsAvailable no serviço de computador; os testes de percurso e cursor anteriores foram observados com sucesso, e a câmera foi verificada por consultas físicas em Play Mode. Aviso gráfico Metal de depth memoryless observado no editor, sem falha de renderização ou compilação; não confundir com erro de gameplay.

## Estado final
Console atual: compilationFailed=false, compiling=false, consoleErrors=0, consoleWarnings=0; console-final.json. O buffer histórico também contém a falha de uma consulta de ferramenta com nome ambíguo, não um erro do jogo. Cena final em Play Mode, initialized=true, captured=false, pés no spawn e grounded=true; pronta para clicar. Prévia final: cais-caminhada-final.png (Game View capturada via Unity MCP).
