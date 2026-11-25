# Tarefas — Grama e vento global

## Referências e piloto

- [x] T001 Auditar projeto, Endless Meadow e alternativas e medir a grama atual na câmera jogável. [Evidência](auditoria.md)
- [x] T002 [US2] Implementar campo de vento e shader Built-in em `Scripts/Environment` e `Art/Grass029`. [Shader](../../Freyja/Assets/_Project/Art/Grass029/LitCoastalGrass.shader)
- [x] T003 [US2] Executar piloto isolado para luz, sombra, luminária e vento antes da integração. [Piloto](evidence/pilot/observacoes.json)

## Distribuição e escala

- [x] T004 [US3] Adaptar `CoastalGrass.cs`, perfis e layout para instancing, LOD, qualidade e exclusões localizadas. [Renderer](../../Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.Lit.cs) e [mapa](evidence/mapa-ruido.png)
- [x] T005 [US3] Verificar determinismo, exclusões, fronteiras, vento e ciclo de vida com ensaios reproduzíveis. [Colocação](evidence/placement.json) e [pausa](evidence/wind-pause.json)

## Ilha jogável

- [x] T006 [US1] Configurar `Ilha_Principal.unity` com os três perfis, controlador de vento, câmera e relógio reais. [Cena](../../Freyja/Assets/_Project/Scenes/Ilha_Principal.unity)
- [x] T007 [US1] Capturar matriz visual via harness, revisar com Game Designer e corrigir problemas observáveis. [Travessia](evidence/island/travessia.gif) e [caminho sob rajada](evidence/island/22-caminho-rajada.png)
- [x] T008 [US3] Medir Metal em Editor e Player quando disponível, revisar com QA e corrigir gargalos medidos. [Medição Metal](evidence/benchmark-player-metal.json)
- [x] T009 Validar regressões e documentar uso, perfis, scripts, resultados, limitações e evidências em `specs/078-grama-vento-global/`. [Relatório](validacao.md)
