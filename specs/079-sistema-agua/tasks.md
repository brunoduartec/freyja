# Tarefas: água estilizada e costa

## Preparação e fundação
- [ ] T001 Diagnosticar pipeline, câmera, RT, profundidade, geometria, ambiente, ferramentas e baseline em `docs/water/diagnostico.md`.
- [x] T002 Avaliar candidato externo e registrar decisão e licença em `docs/water/adr.md` (inspeção documental; teste funcional em Built-in inviável sem migração de pipeline).
- [ ] T003 Construir prova isolada e validar profundidade e saída equivalente à câmera em `Freyja/Assets/_Project/Scenes/WaterShowcase.unity`.

## US1 — mar e costa (P1)
Teste independente: dois ciclos completos e controles positivos e negativos na saída final.
- [ ] T004 [US1] Criar ou adaptar superfície, transparência, ondulações e espuma de contato em `Freyja/Assets/_Project/Art/Mar049/` e assets necessários.
- [ ] T005 [US1] Construir costa com avanço, recuo e areia molhada e geração determinística em `Freyja/Assets/_Project/`.
- [ ] T006 [US1] Verificar AC-01 a AC-07 e AC-14, corrigir e recapturar em `artifacts/water-validation/` e registrar em `docs/water/validacao.md`.

## US2 — ambiente e integração (P2)
Teste independente: horários, vento e saída de câmera real.
- [ ] T007 [US2] Integrar cáusticas, luz, vento e qualidade aos assets em `Freyja/Assets/_Project/`.
- [ ] T008 [US2] Verificar AC-08 a AC-10 e AC-13 com capturas em `artifacts/water-validation/` e resultados em `docs/water/validacao.md`.
- [x] T009 [US2] Integrar provisoriamente após autorização do usuário e verificar AC-11 na câmera e Render Texture reais; capturas e limites em `integracao.md`.

## US3 — configuração e interações (P3)
Teste independente: regeneração repetida e movimento/parada/saída de água.
- [ ] T010 [US3] Criar perfis configuráveis e validações em `Freyja/Assets/_Project/`.
- [ ] T011 [US3] Após aprovação do núcleo, implementar e verificar AC-12 para personagem e barco em `Freyja/Assets/_Project/` e `docs/water/validacao.md`.

## Finalização
- [ ] T012 Executar testes pertinentes, Play Mode, Console e profiling comparável; documentar resultados em `docs/water/validacao.md`.
- [x] T013 Produzir guia, limitações, defeitos e comandos reproduzíveis em `docs/water/` e `specs/079-sistema-agua/quickstart.md`.
- [ ] T014 Revisar resultados independentemente, corrigir falhas e revalidar em `docs/water/validacao.md`.
- [ ] T015 Integrar à main apenas após conclusão e validação, preservando alterações alheias; registrar evidência em `specs/079-sistema-agua/tasks.md`.

## Dependências
T001–T003 precedem US1; T006 é gate para integração T009 e interações T011. US2 e US3 dependem do núcleo funcional. T012–T015 dependem das etapas anteriores. Diagnóstico de câmera e pesquisa externa podem ocorrer em paralelo, sem edição simultânea dos mesmos arquivos.

## Estado de execução — 25/11/2026

A prova isolada foi implementada e executada, mas T003/T004/T006 permanecem abertas: QA independente aprovou AC-01 na cena isolada e reprovou AC-02–04. Ver `docs/water/validacao.md` e `artifacts/water-validation/directional-rt.gif`. As fases posteriores permanecem bloqueadas pelo gate; não houve integração nem profiling de GPU.

### Adendo — integração provisória (25/11/2026)

T009 foi concluída por solicitação explícita do usuário, com evidência da câmera principal e RT em `integracao.md`. Permanecem abertas as limitações dos gates isolados, as cáusticas sobre o terreno real, perfis, horários, profiling e interações; essa integração não aprova o restante dos critérios nem altera o estado independente de T003/T004/T006.

Verificação adicional em Editor isolado e a captura da cena principal estão registradas em `integracao.md` e `docs/water/validacao.md`. T013 foi concluída com quickstart reproduzível. T012/T014/T015 seguem abertas: há gate visual reprovado, um erro interno do Unity Search após Play Mode, faltam validações e profiling, e não houve merge/commit.

Atualização: o shader da água consome `_FreyjaWind`/`_FreyjaWindGust`; Breeze e Strong foram lidos no Editor isolado. T007 permanece aberto por faltarem cáusticas sobre fundo opaco, horários/qualidade e profiling. A espuma da costa real usa `CoastDistance`, mas capturas da Game View não demonstraram ciclos legíveis; AC-05 segue BLOCKED e o alargamento tentado foi revertido. Ver `docs/water/validacao.md`.

Revisão de continuidade: as direções e fases das ondas voltaram a ser fixas; o vento agora modula apenas a amplitude com `_FreyjaWind.w` integrado. Trocas Breeze↔Strong amostradas em `_PreviewTime = 10000` mantiveram força/fase contínuas. A direção do preset não está integrada; sem vídeo contínuo, não se declara revisão visual temporal aprovada.

Atualização de QA: o protótipo `TerrainShoreFoamOverlay` gerou frentes móveis por mais de dois períodos, mas a revisão visual rejeitou as faixas brancas largas que atravessavam a praia até perto das casas. A instância foi removida de `Ilha_Principal`; scripts, shader, material e capturas foram preservados. Depois da remoção, Play Mode confirmou zero componentes/filhos de overlay; ver `079-overlay-removed-main.png` e `docs/water/validacao.md`. T005/T006 seguem abertas. `IslandWater.shader` mantém o ajuste noturno de azul escuro/cáusticas; comparativo de 21h em `079-main-night21-before-clean.png`/`079-main-night21-after-clean.png`. Não declarar a spec concluída: demais gates visuais, cáusticas sobre terreno, qualidades/horários, perfil e profiling continuam pendentes.
