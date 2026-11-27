# Tasks: Diálogos contextuais

**Input**: Design documents em `specs/082-dialogos-contextuais/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

## Phase 1: Setup e compatibilidade

- [x] T001 Confirmar branch `084-dialogos-contextuais`, ponteiro `feature.json` → `specs/082-dialogos-contextuais`, diff dos arquivos do usuário e estado do Editor antes de qualquer escrita.
- [x] T002 Instalar Yarn Spinner Unity `v3.2.8` por URL Git fixada em `Freyja/Packages/manifest.json`; importar/compilar e validar package lock, licença MIT e API com Unity 6000.3.25f1.
- [x] T003 [P] Definir assembly runtime/testes Narrative sem quebrar `Assembly-CSharp`, scripts existentes ou GUIDs; localizar pastas/asmdefs após importação.
- [x] T004 Criar estrutura `Freyja/Assets/_Project/Narrative/{Runtime,Content/Demo,Localization,Editor,Tests/EditMode,Tests/PlayMode}` e respectivos `.meta`.

## Phase 2: Foundational

- [x] T005 [P] Criar estado narrativo puro tipado, relógio injetável e persistência JSON versionada isolada para dados Demo, em `Freyja/Assets/_Project/Narrative/Runtime/`.
- [x] T006 [P] Allow-list `discover`, `communicate`, `effect_once` e `checkpoint`; comandos verificam NPC ativo, ID, categoria e destino. Descobertas persistem categoria e fonte.
- [x] T007 Integrar variáveis de consulta e comandos Yarn autorizados; `when:` no conteúdo Yarn controla elegibilidade contextual e temporal.
- [x] T008 Criar catálogo `.yarnproject` e CSV `pt-BR`/`en-US`; compiler 3.2.8 retornou `errors=False`, 38 IDs permanentes de linha/opção.
- [x] T009 Testar estado ausente/versão antiga e round-trip JSON sem cena principal em `NarrativeProgressTests.SaveRoundTripAndLegacyDefault`.

## Phase 3: User Story 1 — Retomar conversa com novas informações (P1)

**Goal**: Três NPCs reagem apenas ao conhecimento comunicado e liberam perguntas/revelações por contexto, em ambas as ordens, sem duplicar o caderno existente.

**Independent Test**: EditMode cobre sem pista, pista só do jogador, comunicação isolada, ordem A/B e B/A, seleção determinística, Once/Repeatable, cancelamento e idempotência.

### Tests

- [ ] T010 [P] [US1] Parcial: 7 testes EditMode cobrem conhecimento privado/duas ordens, repetição, cooldown, idempotência, round-trip e CSV; faltam os cenários de domínio restantes de N01–N08, N10–N11 e N17.
- [ ] T011 [P] [US1] Parcial: compiler Yarn 3.2.8 compila o projeto Demo (`errors=False`) e a execução PlayMode valida os comandos usados; falta automatizar checagem de todos os nodes/IDs e detectar chamada para comando não registrado.

### Implementation

- [x] T012 [US1] Criar slice Demo em `Content/Demo/Demo.yarn`: três NPCs, cinco pistas, 38 IDs estáveis de linha/opção (dentro do intervalo 30–50), sem conteúdo de lore oficial; compilado por Yarn 3.2.8 (`errors=False`).
- [x] T013 [US1] `IslandDialogue` preserva conteúdo padrão e expõe modo Demo Yarn por F2; E/proximidade inicia Yarn no mesmo `CaisSession` modal/`ConversationFraming`. `OnGUI` desenha no modal existente. Clique/Enter avançam e escolhem, setas navegam, L troca locale e Esc/saída de alcance cancela. Testes de runner estão cobertos; percurso físico no scene não foi validado (T018/T031).
- [x] T014 [US1] `discover` roda após confirmação da linha; progresso mantém IDs únicos com tipo e fonte. Caderno mostra metadados Demo e suprime duplicatas contra registros legados. Teste JSON cobre metadados e idempotência.
- [x] T015 [US1] PlayMode cria fixture aditiva temporária em memória (`Narrative_PlayMode_Fixture` e `Narrative_Locale_PlayMode_Fixture`); nenhuma cena foi salva, `IslandDialogueSetup` não foi executado e `Ilha_Principal.unity` não foi editada nesta slice.
- [x] T016 [US1] EditMode passou 7/7 em `/tmp/freyja-validation/editmode-current.xml` (Unity CLI, cópia temporária).

## Phase 4: User Story 2 — Escolher, avançar e retomar com segurança (P1)

**Goal**: Sessão modal consistente, efeitos confirmados uma vez, Cooldown temporal e progresso narrativo restaurado após round-trip.

**Independent Test**: PlayMode inicia conversa, escolhe, cancela, reabre, conclui, round-trip; verificações confirmam input/movimento restaurados.

### Tests

- [ ] T017 [P] [US2] Parcial: teste save ausente/versão antiga, conhecimento e efeitos/cooldown; ainda falta serializar e conferir conhecimento, histórico e efeitos no mesmo round-trip.
- [ ] T018 [P] [US2] Parcial: PlayMode cobre runner/bridge/presenter, compartilhamento contextual em sequência e locale. Ainda faltam cancelar/retomar pela interação E, estado modal e restauração de input no `CaisSessionController`.

### Implementation

- [x] T019 [US2] Integrar `CoastalDayNight.Hour` por adaptador e relógio fake injetável; storylets são avaliados ao abrir nó, sem polling de elegibilidade por frame.
- [x] T020 [US2] Versionar round-trip narrativo Demo e persistir somente estado de domínio; não acoplar ao inventário ou alegar SaveSystem de produção. (persistência JSON isolada entregue)
- [ ] T021 [US2] Parcial: cancelamento normal fecha runner, framing e modal idempotentemente; tratamento de erro/falha de projeto Yarn ainda não foi exercitado.
- [x] T022 [US2] Executado em cópia Unity 6000.3.25f1: EditMode 7/7 e PlayMode 2/2. XML final em `/tmp/freyja-validation/editmode-verified-current.xml` e `/tmp/freyja-validation/playmode-final.xml`. Uma primeira tentativa transitória terminou com SIGBUS e a reexecução passou.

## Phase 5: User Story 3 — Localizar e inspecionar autoria (P2)

**Goal**: Tradutor mantém CSV utilizável, troca de idioma não altera lógica e designer inspeciona candidates/diagnósticos isolados.

**Independent Test**: Round-trip CSV conserva traduções/metadados/strings especiais e debugger valida demo sem tocar save real.

### Tests

- [ ] T023 [P] [US3] Parcial: testes cobrem RFC4180/acentos/aspas/multiline, merge idempotente, stale/missing/orphan/placeholders e rejeição de ID duplicado; falta testar importação pelo menu Editor e saída nativa reimportada.
- [ ] T024 [P] [US3] Parcial: PlayMode confirma texto inglês e ID/estado inalterados; faltam comparar todas as escolhas/efeitos nos dois locales e fallback sem tradução.

### Implementation

- [x] T025 [US3] Criar export/merge Editor e arquivos nativos Yarn com o esquema consolidado; preserva traduções, rejeita IDs duplicados e registra stale/missing/orphan/placeholders. Round-trip de merge é coberto por teste.
- [ ] T026 [US3] Parcial: debugger read-only mostra snapshot persistido, NPC, knowledge, hora persistida/histórico e regra contextual de Ivo. Não calcula candidates Yarn ou razões reais para cada opção; janela ainda precisa validação visual no Editor.
- [ ] T027 [US3] Parcial: tecla L alterna locale no modal; locale não é serializado no progresso. Fallback por catálogo sem tradução ainda não foi testado.
- [x] T028 [US3] CSV mestre consolidado e catálogos nativos têm 38 IDs únicos; PT-BR/EN-US foram conferidos por teste locale e projeto compilado via Yarn 3.2.8.

## Phase 6: Polish e validação final

- [x] T029 [P] Atualizar `quickstart.md`, `research.md`, `data-model.md` e `contracts/narrative-runtime.md` com caminhos finais, comandos, dependência/licença, resultados e limites observados; quickstart também é o guia curto de autoria da demo.
- [x] T030 Compiler Yarn 3.2.8 (`errors=False`), EditMode 7/7 e PlayMode 3/3; XMLs finais estão fora de Assets. Unity CLI compilou/testou a cópia temporária; o Editor principal continuou com recompile `triggered` e listagem PlayMode incompleta.
- [ ] T031 Pendente: nenhuma Game View real foi capturada; o teste visual exige Editor visível e o host reportou bloqueio de tela. Abrir uma cópia temporária no Editor visível após desbloqueio (a cena principal não precisa ser aberta/salva) e coletar PNGs de greeting/opções/reação/`en-US`.
- [ ] T032 Solicitar revisão independente Game Designer e Validador; incorporar defeitos identificados e repetir verificações afetadas.
- [ ] T033 Revisar `git diff --stat` e alterações de cena/meta/manifesto; integrar branch validada à `main` somente preservando alterações locais, registrar resultado e confirmar estado final.

## Dependencies & Execution Order

- Setup (T001–T004) → Foundation (T005–T009) → US1 (T010–T016) → US2 (T017–T022) → US3 (T023–T028) → Final (T029–T033).
- Regras e fixtures fundacionais precedem as histórias; US2 depende do checkpoint de efeitos definido em US1; US3 usa IDs/linhas aprovados em US1.
- T010/T011 e T017/T018 e T023/T024 podem ser escritos em paralelo após a fundação e respeitando assembly criado.
- Implementar vertical slice jogável primeiro (US1/US2); localização/debugger fecha a integração transversal.
