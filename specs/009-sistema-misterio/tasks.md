# Tasks: MVP web de pistas — spec 009

**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md), [contrato web](contracts/web-mvp.md) e [quickstart.md](quickstart.md).
**Status**: tarefas planejadas; nenhuma foi executada. O usuário solicitou concluir plano e tasks antes de implementar.
**Organization**: primeiro autoria e fundamentos; depois US4 (interface), US1 (investigação), US3 (continuidade das fontes). US2, campanha completa, relógio e festa não fazem parte deste MVP.

## Format: `[ID] [P?] [Story] Description`

- `[P]` indica trabalho que pode ocorrer em paralelo quando as dependências indicadas estiverem concluídas; não exige criar agentes.
- Rótulos US4, US1 e US3 correspondem às histórias da spec, preservando seus IDs.
- Marcar conclusão apenas com arquivo, resultado de teste, captura ou revisão correspondente. A criação deste documento não conclui nenhuma tarefa.

## Path Conventions

- Aplicação futura: `Tools/MysteryWeb/`.
- Fonte estruturada única: `Tools/MysteryWeb/data/case.json`.
- Dossiê e mapas exportados: `Source/Narrative/mvp-arrival/`.
- Evidências de entrega: `Source/Narrative/mvp-arrival/reviews/` e relatório de validação.
- `Freyja/Assets/`, pacotes Unity, fontes Blender e imagens originais permanecem fora das alterações do MVP.

## Phase 1: Setup — recorte e autoria inicial

**Goal**: preparar a ferramenta e fixar o conteúdo da pequena investigação antes de construir a experiência.

- [ ] T001 Criar a estrutura mínima prevista em `Tools/MysteryWeb/` e registrar em `Tools/MysteryWeb/README.md` o objetivo da amostra, comandos locais e limites, sem instalar framework ou backend.
- [ ] T002 Escrever em `Tools/MysteryWeb/data/case.json` as fichas de Artur, Teresa e Ivo, a verdade da pergunta sobre a partida e o gancho textual com Luana; separar conhecimento, crenças e omissões e identificar a associação visual provisória de Ivo.

## Phase 2: Foundational — conteúdo e regras compartilhadas

**Purpose**: esses fundamentos precisam existir antes das histórias interativas.

- [ ] T003 Autorizar seis a nove evidências e suas combinações em `Tools/MysteryWeb/data/case.json`, com origem informacional atômica, fonte imediata, condições, afirmações e contradições; criar alternativas suficientes para sobreviver à perda de uma ou duas testemunhas.
- [ ] T004 Escrever em `Tools/MysteryWeb/data/case.json` falas em sequência, perguntas e respostas a evidências, com pelo menos uma escolha que altere o acesso a um assunto; preservar um caminho alternativo e limitar marcadores aos necessários para este caso.
- [ ] T005 Definir em `Tools/MysteryWeb/data/case.json` o cenário inicial e os estados de teste com fontes perdidas, incluindo uma preparação válida com relato obtido antes da morte; o texto de cada cenário deve corresponder ao seu momento, sem repetir uma chegada com mortos falando.
- [ ] T006 Implementar o carregamento e a validação estrutural do caso em `Tools/MysteryWeb/src/case-loader.mjs`, verificando IDs, referências, tipos, coordenadas e limites, sem executar expressões arbitrárias do conteúdo.
- [ ] T007 Definir em `Tools/MysteryWeb/src/investigation.mjs` o estado de sessão e a aplicação de ações com condições explícitas, separando dados autorais, conhecimento adquirido, fontes disponíveis e marcadores.
- [ ] T008 Cobrir em `Tools/MysteryWeb/tests/content.test.mjs` referências inválidas, IDs duplicados, fontes mal atribuídas e regras vazias; corrigir o dataset e o carregador antes de liberar a interface.

**Checkpoint**: caso carregável, fichas e evidências rastreáveis, estados de teste definidos. Nenhuma história futura deve depender de corrigir silenciosamente os fatos do caso.

## Phase 3: US4 — Exercitar pistas no navegador (Priority: P1)

**Goal**: apresentar mapa, fichas, falas e painel sem investir em novos visuais.
**Independent Test**: abrir a página, clicar nas três fichas, percorrer falas e manter o painel de pistas visível.

- [ ] T009 [P] [US4] Copiar apenas mapa e folha de personagens para `Tools/MysteryWeb/assets/`, preservando os originais e registrando procedência e associação dos rostos em `Tools/MysteryWeb/assets/manifest.json` conforme `specs/009-sistema-misterio/assets.md`.
- [ ] T010 [US4] Medir e registrar os três retângulos de rosto em `Tools/MysteryWeb/data/case.json`; apresentar máscaras circulares sem raster novo e verificar Artur/Teresa contra o GDD e Ivo como referência provisória documentada.
- [ ] T011 [US4] Criar `Tools/MysteryWeb/index.html`, a estrutura de `Tools/MysteryWeb/styles.css` e `Tools/MysteryWeb/src/views/map.mjs`, com mapa, pontos examináveis e botões de personagens identificados por nome.
- [ ] T012 [P] [US4] Criar `Tools/MysteryWeb/src/views/dialogue.mjs` com nome do interlocutor, sequência de falas, avanço, assuntos disponíveis e seleção de perguntas, seguindo o contrato sem antecipar efeitos de falas futuras.
- [ ] T013 [P] [US4] Criar `Tools/MysteryWeb/src/views/evidence.mjs` com painel permanente, estado vazio, itens descobertos e atribuição de fonte; evidências desconhecidas e verdade autoral não podem aparecer.
- [ ] T014 [US4] Integrar as áreas em `Tools/MysteryWeb/src/app.mjs` e concluir `Tools/MysteryWeb/styles.css` para mouse, teclado, foco visível e telas estreitas, mantendo o painel acessível durante diálogos.

**Checkpoint**: mapa e três fichas funcionam como interface de teste; nomes e falas corretos; nenhuma pista oculta é revelada por apresentação.

## Phase 4: US1 — Investigar sem depender da ordem (Priority: P1)

**Goal**: exercitar aquisição, perguntas e explicação explícita com evidências.
**Independent Test**: percorrer conversa → objeto e objeto → conversa, chegando à mesma base factual sem duplicar pistas.

### Tests

- [ ] T015 [P] [US1] Escrever em `Tools/MysteryWeb/tests/investigation.test.mjs` casos E/OU, origens compartilhadas, documento com afirmação falsa, repetição e permutação de descoberta; hipótese sem sustentação não deve ser aceita nem virar prova de si mesma.

### Implementation

- [ ] T016 [US1] Completar em `Tools/MysteryWeb/src/investigation.mjs` os efeitos de conversar, apresentar evidência e registrar observação, com idempotência e condições consistentes com os testes de T015.
- [ ] T017 [US1] Ligar pontos examináveis e aquisições à interface em `Tools/MysteryWeb/src/views/map.mjs` e `Tools/MysteryWeb/src/views/evidence.mjs`, preservando origem, natureza e histórico sem tratar inscrição observada como verdade automática.
- [ ] T018 [US1] Acrescentar em `Tools/MysteryWeb/src/views/dialogue.mjs` e `Tools/MysteryWeb/src/app.mjs` a proposta de hipótese com seleção de evidências, feedback sem entregar a resposta e encerramento identificado como fim da amostra, não vitória sobre a maldição.
- [ ] T019 [US1] Criar `Tools/MysteryWeb/scripts/export-dossier.mjs` para gerar `Source/Narrative/mvp-arrival/dossier.md` e `Source/Narrative/mvp-arrival/clue-map.mmd` a partir do mesmo JSON, com IDs e fontes rastreáveis, sem conteúdo paralelo editado à mão.
- [ ] T020 [US1] Executar os dois percursos de descoberta no navegador, registrar sequências e conclusão em `Source/Narrative/mvp-arrival/reviews/order-review.md` e corrigir condições que produzam resultados factuais divergentes.

**Checkpoint**: a conclusão depende da proposta do jogador e de fontes adequadas; trocar a ordem não inventa fatos nem elimina a primeira investigação.

## Phase 5: US3 — Continuar depois de uma perda (Priority: P2)

**Goal**: provar que a perda de fonte não apaga memória nem torna a pequena investigação insolúvel.
**Independent Test**: bloquear uma conversa antes de ocorrer e preservar o mesmo relato se foi obtido antes da perda.

### Tests

- [ ] T021 [P] [US3] Escrever em `Tools/MysteryWeb/tests/validation.test.mjs` perdas antes/depois da aquisição, ramos incompatíveis, ciclo sem evidência inicial, alternativas válidas e um caso intencionalmente insolúvel que precisa ser rejeitado.

### Implementation

- [ ] T022 [US3] Implementar em `Tools/MysteryWeb/src/investigation.mjs` a preparação e a perda controlada de fonte, bloqueando novas conversas e preservando conhecimento adquirido; fonte morta não pode aparecer falando no cenário posterior.
- [ ] T023 [US3] Implementar em `Tools/MysteryWeb/src/validation.mjs` e `Tools/MysteryWeb/scripts/validate.mjs` a busca de estados usando as mesmas ações da interface, com deduplicação, predecessores e relato de percurso ou requisitos bloqueados.
- [ ] T024 [US3] Gerar `Source/Narrative/mvp-arrival/validation-report.md` com as sete configurações de zero, uma ou duas perdas e o percurso de memória anterior; revisar a autoria quando uma configuração entregue ficar sem solução, sem inventar novas pistas em execução.
- [ ] T025 [US3] Integrar em `Tools/MysteryWeb/src/app.mjs` a repetição dos estados de revisão e o reinício limpo, identificando os cenários como testes de fontes; confirmar que nenhum estado anterior vaza para a próxima rodada.

**Checkpoint**: relatório demonstra caminhos concretos; fixture defeituoso falha; a prova não é apresentada como simulação do relógio ou de toda a campanha.

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T026 Rodar os comandos de testes, validação e exportação de `specs/009-sistema-misterio/quickstart.md`; registrar saídas em `Source/Narrative/mvp-arrival/reviews/checks.md` e corrigir falhas antes do teste visual final.
- [ ] T027 [P] Seguir a referência de preview local da skill Sites antes do navegador; verificar 1366×768, 1920×1080 e 390×844, teclado, erros de Console e carregamento; salvar capturas em `Source/Narrative/mvp-arrival/reviews/` com pistas visíveis durante conversa.
- [ ] T028 [P] Revisar em `Source/Narrative/mvp-arrival/reviews/narrative-review.md` se o gancho motiva a primeira ação, se duas fontes justificam a conclusão e se escolhas alteram algo compreensível; confrontar falas com GDD e dossiê, sem validar conteúdo apenas porque o grafo aceita.
- [ ] T029 Atualizar `Tools/MysteryWeb/README.md`, `Source/Narrative/mvp-arrival/dossier.md` e os registros de revisão com endereço local, comandos, proveniência dos assets, retrato provisório e limites reais do protótipo; não declarar validados tempo, festa ou 30 moradores.
- [ ] T030 Apresentar a amostra e suas evidências para avaliação do usuário, registrar o retorno em `Source/Narrative/mvp-arrival/reviews/user-review.md` quando recebido e parar antes de ampliar o elenco ou a campanha; não marcar aceitação sem o retorno correspondente.

## Dependencies & Execution Order

```text
T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008
                         fundamentos concluídos
US4: T009 → T010; T011 → (T012 e T013) → T014
US1: T015 → T016 → T017 → T018 → T019 → T020
US3: T021 → T022 → T023 → T024 → T025
Final: T026 → (T027 e T028) → T029 → T030
```

- US4 começa após os fundamentos e fornece a apresentação. T011 pode avançar enquanto T009 prepara assets; T010 e as três views precisam estar concluídas para T014.
- US1 usa a interface concluída de US4 e o estado dos fundamentos. Seus testes de regras podem ser preparados em paralelo aos assets, mas a integração depende de T014.
- US3 depende dos efeitos e aquisições de US1; só a interface de troca de cenários depende também da aplicação web integrada.
- T026 depende de todas as histórias. T027 e T028 podem ocorrer em paralelo depois dos checks, em arquivos distintos. T030 é o checkpoint de avaliação da entrega.

### User Story Dependencies

US4 → US1 → US3 para o percurso demonstrável completo. US2 permanece fora do MVP e não deve gerar tarefas de cura, relógio, vítimas aleatórias ou festa nesta rodada.

### Parallel Opportunities

Após T011, T012 e T013 trabalham em módulos diferentes. A preparação de assets em T009 e os testes puros de T015 também são independentes após os fundamentos. Nenhum paralelismo autoriza alterar os mesmos arquivos simultaneamente sem coordenação.

### MVP First

A primeira versão avaliável contém o recorte inteiro de pistas: três fichas, seis a nove evidências, uma conclusão e prova de continuidade após perda. A interface vazia isolada não satisfaz a entrega. O dossiê e o relatório fazem parte do resultado, não são trabalho substituído pela interface.

## Traceability

| Necessidade | Histórias/tarefas | Evidência futura |
| --- | --- | --- |
| Gancho e conhecimento dos personagens | T002–T005, T028 | Dossiê, falas e revisão narrativa |
| Mapa, rostos e diálogo sequencial | US4, T009–T014, T027 | Página e capturas |
| Painel permanente e descoberta atribuída | T013, T016–T018 | Interação durante diálogo e testes |
| E/OU, origem e hipótese explícita | US1, T015–T020 | Testes e dois percursos |
| Morte de fonte e memória | US3, T021–T025 | Testes e sete configurações |
| Preservar cânone e delimitar prova | T002, T028–T030 | Revisão, limitações e retorno do usuário |

**Total:** 30 tarefas: 8 de preparação/fundamentos, 6 de US4, 6 de US1, 5 de US3 e 5 de revisão/entrega. Todas pendentes.
