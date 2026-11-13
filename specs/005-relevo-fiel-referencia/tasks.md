# Tarefas — Spec 005: Relevo idêntico à referência

**Entrada:** [spec.md](spec.md) · [plan.md](plan.md) · [research.md](research.md) · [data-model.md](data-model.md) · [contrato de navegação](contracts/navegacao.md) · [quickstart.md](quickstart.md).

**Estado:** T001–T011 executadas e revisadas até a rodada 04. A proposta atual e as comparações estão em `evidence/revisao-04/`. Fidelidade visual ainda não aprovada; T012 em diante permanecem pendentes.
**Regra do projeto:** iterações curtas com artefatos visuais avaliáveis. A aprovação da spec autorizou este plano; as tarefas de produção depois do primeiro protótipo visual ficam condicionadas à avaliação do usuário naquele checkpoint. Não construir a sequência inteira antes dessa avaliação.

## Fase 1 — Preparação

**Objetivo:** conhecer o estado real dos editores e registrar uma linha de base para preservar o trabalho anterior.

- [x] T001 Criar `specs/005-relevo-fiel-referencia/evidence/`; confirmar via MCP as versões, projeto Unity, cenas abertas, arquivo Blender aberto, estado não salvo e disponibilidade dos editores; registrar em `specs/005-relevo-fiel-referencia/evidence/ambiente.md`.
- [x] T002 [P] Registrar SHA-256 da fonte Blender 004, cena Unity 004, export, prefab de ilha, personagem e prefabs compartilhados em `specs/005-relevo-fiel-referencia/evidence/source-hashes.json`.
- [x] T003 [P] Registrar em `specs/005-relevo-fiel-referencia/evidence/ambiente.md` o conjunto de seis referências mandatórias: top-down, perspectiva aérea e perfis norte, sul, leste e oeste.

## Fase 2 — Fundação isolada

**Objetivo:** preparar cópias independentes de produção sem tocar nas entregas existentes.

- [x] T004 [P] Criar via Blender MCP em background `Source/Blender/Ilha_Relevo_005.blend` a partir da fonte mais útil da 004, inspecionando malhas, materiais, pivôs e marcadores; preservar alterações não salvas da sessão e registrar origem em `specs/005-relevo-fiel-referencia/evidence/ambiente.md`. Depende de T001.
- [x] T005 [P] Criar via Unity MCP `Freyja/Assets/_Project/Scenes/Ilha_Relevo_005.unity` como cópia independente da cena `Ilha_Relevo_004.unity`; preservar referências de jogador, câmera, sessão, cais e barco e registrar cópia e GUID em `specs/005-relevo-fiel-referencia/evidence/ambiente.md`. Depende de T001.
- [x] T006 Preparar em `Freyja/Assets/_Project/Art/Environment/IlhaRelevo/` o destino de importação do export 005 e verificar `.meta`/GUIDs existentes sem recriá-los; registrar caminhos em `specs/005-relevo-fiel-referencia/evidence/ambiente.md`. Depende de T005.

## Fase 3 — US1: reconhecer o mesmo relevo da imagem (P1) · primeiro incremento avaliável

**Objetivo:** reproduzir a silhueta e topologia visível antes de detalhar a integração jogável.

**Teste independente:** confrontar vistas aérea e de perfil com a referência alinhada; localizar costa recortada, enseada/praia, terraços da vila, patamares de horta/ruína, crista posterior e paredões de 70–80°.

- [x] T007 [US1] Refazer a forma base na fonte `Source/Blender/Ilha_Relevo_005.blend`, aproximando contorno costeiro recortado, saliências rochosas e enseada frontal com praia em arco; registrar medidas criadas sem inferir escala da imagem. Depende de T004.
- [x] T008 [US1] Modelar em `Source/Blender/Ilha_Relevo_005.blend` o maciço largo ao fundo com crista irregular, patamares rochosos e faces íngremes de aproximadamente 70–80°; manter as inclinações acessíveis destinadas à caminhada para a fase US2. Depende de T007.
- [x] T009 [US1] Reorganizar em `Source/Blender/Ilha_Relevo_005.blend` os níveis provisórios do povoado, largo, horta e ruína de acordo com suas posições relativas na referência, distinguindo as inferências do verso em `specs/005-relevo-fiel-referencia/evidence/comparacao.md`. Depende de T008.
- [x] T010 [US1] Produzir em `specs/005-relevo-fiel-referencia/evidence/` vistas do modelo e comparações alinhadas com todas as imagens mandatórias: planta top-down, perspectiva aérea e perfis norte, sul, leste e oeste; registrar correspondências e qualquer divergência entre referências em `specs/005-relevo-fiel-referencia/evidence/comparacao.md`. Depende de T009.
- [x] T011 [US1] Salvar e reabrir `Source/Blender/Ilha_Relevo_005.blend`, conferir correspondência com as seis imagens do conjunto de exatidão e apresentar comparações de todas as vistas ao usuário. Depende de T010.

**Checkpoint obrigatório:** parar após T011 e aguardar a avaliação do usuário. Não iniciar T012 ou seguintes até receber retorno explícito sobre a maquete. Se houver revisão pedida, atualizar a fonte 005 e repetir comparação/capturas antes de prosseguir.

## Fase 4 — US2: caminhar no terreno compatível com a inclinação (P1)

**Objetivo:** integrar o terreno avaliado e garantir navegação nas superfícies livres abaixo de 45°, preservando paredões.

**Teste independente:** iniciar no cais; percorrer largo, setores, horta/ruína e recanto; testar ida e volta, superfícies fora dos caminhos e bloqueio nas faces íngremes. Distinguir análise estática de movimento Play Mode.

- [ ] T012 [US2] Ajustar na fonte `Source/Blender/Ilha_Relevo_005.blend` patamares, ramps e rotas compatíveis com a forma aprovada, conectando os espaços funcionais em faces menores que 45° e preservando as faces rochosas de 70–80°. Depende da aprovação do checkpoint T011.
- [ ] T013 [US2] Reutilizar ou reposicionar na fonte `Source/Blender/Ilha_Relevo_005.blend` o cais como âncora, barco, 12 blocos B1–B4/A1–A4/L1–L4, largo, horta, ruína e casinha do recanto; registrar componentes adaptados e substituídos em `specs/005-relevo-fiel-referencia/evidence/reaproveitamento.md`. Depende de T012.
- [ ] T014 [US2] Exportar via Blender MCP `Freyja/Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_005.fbx` e conferir orientação, unidade, normais, pivô e encaixe da referência do cais; registrar medidas em `specs/005-relevo-fiel-referencia/evidence/integracao.md`. Depende de T013.
- [ ] T015 [US2] Importar `Ilha_Relevo_005.fbx`, criar `Freyja/Assets/_Project/Prefabs/IlhaRelevo005.prefab` e integrar à cena `Freyja/Assets/_Project/Scenes/Ilha_Relevo_005.unity`; desativar apenas as instâncias 004 sobrepostas na cena 005 e preservar prefabs e cenas anteriores. Depende de T006 e T014.
- [ ] T016 [US2] Configurar na cena `Ilha_Relevo_005.unity` collider do terreno e volumes, junção contínua com o cais, água visual sem collider, spawn, bounds de recuperação e alcance de câmera; conferir controlador e referências sem alterar assets compartilhados. Depende de T015.
- [ ] T017 [US2] Adaptar `Tools/Unity/AnalyzeIlhaRelevoWalkability.cs` e sua configuração em arquivos próprios de `specs/005-relevo-fiel-referencia/` para a cena e malha de colisão 005; relatar faixas `<45°`, `45–70°`, `70–80°` e `>80°`, degenerações e limites do scan em `specs/005-relevo-fiel-referencia/evidence/inclinacoes.md`. Depende de T016.
- [ ] T018 [US2] Analisar conectividade amostrada da superfície na cena `Ilha_Relevo_005.unity`, incluindo volume/raio do jogador, limites de degrau, destinos funcionais, terreno fora das trilhas e células inacessíveis; registrar método, espaçamento, cobertura e limitações em `specs/005-relevo-fiel-referencia/evidence/inclinacoes.md`. Corrigir destinos abaixo de 45° isolados sem aplainar paredões. Depende de T017.
- [ ] T019 [US2] Percorrer em Play Mode a cena `Ilha_Relevo_005.unity` do cais ao largo, setores, horta/ruína e recanto e retornar; testar espaço livre fora dos caminhos, faces de 70–80°, câmera, colisões, queda na água e recuperação; registrar pontos, falhas e tipo de entrada em `specs/005-relevo-fiel-referencia/evidence/percurso.md`. Depende de T018.
- [ ] T020 [US2] Confirmar em `Ilha_Relevo_005.unity` que a casinha do recanto está oculta do largo e acessível pela rota; confirmar em Play Mode que o controlador não sobe paredões nem fica preso nos patamares. Depende de T019.

## Fase 5 — US3: continuar a partir da entrega existente (P2)

**Objetivo:** comprovar o reaproveitamento seletivo e a preservação das entregas anteriores.

**Teste independente:** inspecionar cena e hierarquia 005, validar referências herdadas e comparar hashes finais com a linha de base.

- [ ] T021 [US3] Registrar na cena 005 e em `specs/005-relevo-fiel-referencia/evidence/reaproveitamento.md` a proveniência do jogador, controles, câmera, sessão, cais/barco, 12 blocos, marcos e materiais reutilizados, adaptados ou recriados. Depende de T015.
- [ ] T022 [US3] Comparar hashes dos arquivos de origem 003/004, prefabs compartilhados, versão, pacotes e configurações com `specs/005-relevo-fiel-referencia/evidence/source-hashes.json`; salvar resultados em `specs/005-relevo-fiel-referencia/evidence/preservacao-final.json` e investigar qualquer diferença. Depende de T019.

## Fase 6 — Acabamento e entrega

**Objetivo:** reunir as evidências e apresentar a cena completa para avaliação.

- [ ] T023 Reabrir `Source/Blender/Ilha_Relevo_005.blend` e `Freyja/Assets/_Project/Scenes/Ilha_Relevo_005.unity`; verificar referências, erros pertinentes do Console e estado de salvamento em `specs/005-relevo-fiel-referencia/evidence/final.md`. Depende de T020 e T022.
- [ ] T024 Atualizar `specs/005-relevo-fiel-referencia/quickstart.md`, `specs/005-relevo-fiel-referencia/spec.md` e `specs/README.md` com arquivos finais, evidências, resultados e limitações observadas, sem registrar aprovação visual presumida. Depende de T023.
- [ ] T025 Apresentar a cena `Ilha_Relevo_005.unity`, a comparação alinhada, capturas de perfil e resumos de inclinação/percurso para nova avaliação do usuário. Depende de T024.

## Dependências e execução em paralelo

`T001 → (T002 || T003) → (T004 || T005) → T006`. No Blender, a sequência da maquete é `T007 → T008 → T009 → T010 → T011`. Aguarda-se avaliação em T011 antes de iniciar `T012 → T013 → T014`; em paralelo com T014, T006 já pode estar concluída. Integração Unity: `(T006 + T014) → T015 → T016 → T017 → T018 → T019 → T020`. A preservação da origem (T022) pode ocorrer após a execução, sem alterar o mesmo arquivo do trabalho Blender. Finalização: `(T020 + T022) → T023 → T024 → T025`.

As únicas tarefas marcadas `[P]` trabalham em arquivos/editores separados após a mesma pré-condição. As demais editam o mesmo asset ou dependem da saída anterior e devem seguir em sequência.

## Estratégia de entrega

O primeiro incremento demonstrável é a maquete visual US1 em T011; US2 e US3 aguardam avaliação favorável/retorno do usuário, com revisão do primeiro artefato se necessária. A análise de malha e a grade de conectividade são aproximações; o percurso com controlador em Play Mode verifica comportamento real. A avaliação visual continua pertencendo ao usuário.

## Revisão visual solicitada em 2026-11-13

T007–T011 repetidas na revisão 02: correção da topografia, capturas nas seis vistas, reabertura da fonte e comparação documentada. O usuário definiu a imagem aérea como oráculo em divergências. As tarefas marcadas indicam execução da maquete e apresentação, não aprovação de exatidão nem validação de navegação. Ver `evidence/comparacao.md`.

### Revisão 03

Repetidas T007–T011 após nova avaliação: coroa contínua, contrafortes, patamares locais e enseada revisados. Capturas e verificações em `evidence/revisao-03/`. A revisão 03 substitui a 02 como proposta atual; aprovação visual e T012 em diante continuam pendentes.

### Revisão 04

T007–T011 repetidas após nova solicitação do usuário. Crista, costa e níveis do povoado reconstruídos com pontos traçados no oráculo; parte posterior revisada; sete capturas regeneradas. Fonte salva e reaberta para avaliação. Ver `evidence/revisao-04/`. Fidelidade visual ainda aguarda avaliação, sem avançar a integração Unity.
