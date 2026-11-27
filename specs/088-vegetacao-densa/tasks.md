# Tasks: Vegetação regional da ilha

**Input**: `specs/088-vegetacao-densa/` — spec, plan, research e data-model  
**Strategy**: validar piloto praia-vila antes de expansão.

## Phase 1 — Setup e baseline

- [x] T001 Confirmar branch `088-vegetacao-densa` criada a partir de `main` local `73803dd`, cena alvo e instância Unity 7800 conforme `Tools/MCP/README.md`.
- [x] T002 [P] Inventariar modelos, variantes, materiais e escala dos candidatos em `Freyja/Assets/ThirdParty/Nature032/`, `Freyja/Assets/_Project/Art/KitPS1_022/` e `Freyja/Assets/_Project/Art/Grass029/`; candidatos e limites em `research.md`.
- [x] T003 Capturar baseline da câmera real do jogador, de dia e à noite, em `specs/088-vegetacao-densa/evidence/pilot-baseline/`.

## Phase 2 — US1: piloto praia-vila (P1)

**Objetivo**: tornar a transição mais viva sem fechar praia, caminho, personagem ou entrada.  
**Teste independente**: comparação de câmera pareada dia/noite e travessia em Play Mode.

- [x] T004 [US1] Criar grupos regionais praia-vila e campo coral nos quintais gramados com assets disponíveis em `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` (árvores omitidas na transição costeira para preservar escala e navegabilidade).
- [x] T005 [US1] Ajustar transforms, espaçamento e escala do piloto para manter água, faixa molhada, largura útil e interações livres; correção visual confirmada nas capturas finais do campo.
- [x] T006 [US1] Confirmar materiais compartilhados, resposta à iluminação/ciclo e vento existente, e ausência de colliders nos elementos decorativos em `Ilha_Principal.unity`; flores usam Standard e não suportam o shader de vento, sem comportamento artificial adicionado.
- [x] T007 [US1] Capturar câmera real de dia/noite e inspecionar Console em Play Mode; evidências atuais em `specs/088-vegetacao-densa/evidence/pilot-field/`.
- [x] T008 [US1] Corrigir problemas de orientação e repetir capturas; a conversão do eixo local Z para Y e rotações variáveis foram salvas e confirmadas visualmente.

**Checkpoint obrigatório**: expandir somente quando piloto estiver legível em ambas as condições, navegável e sem bloqueios.

## Phase 3 — US2: expansão regional (P2)

**Objetivo**: aplicar composições próprias a setores existentes.  
**Teste independente**: revisão real de câmera dia/noite em cada região e travessia.

- [x] T009 [US2] Estender grupos irregulares de plantas costeiras e gramíneas existentes à borda seca da praia e rochas, preservando água/faixa molhada em `Ilha_Principal.unity`.
- [x] T010 [US2] Compor jardins e bordas de caminho, praça e entorno de edifícios públicos sem bloquear acessos, NPCs ou marcos em `Ilha_Principal.unity`.
- [x] T011 [US2] Compor núcleos e clareiras nas encostas; incluir `Tree_Cypress` e `Tree_Pine` existentes na crista noroeste e vegetação baixa fora de lotes produtivos, rotas e pontos de interação em `Ilha_Principal.unity`.
- [x] T012 [US2] Ruínas: não aplicável à cena atual. A busca na cena e nos assets próprios/terceiros não encontrou objeto ou modelo de ruína; não foi inventado um local para plantio.
- [x] T013 [US2] Validar os setores e o teste arbóreo pela câmera real de dia/noite; capturas densas em `evidence/revalidation-dense/`. Igreja, caminhos, NPC e água permanecem legíveis; costa foi reduzida após a revisão. `CharacterController` percorreu dois trechos gramados de 6 m na rodada anterior.

## Phase 4 — Entrega

- [x] T014 [P] Registrar ativos usados, setores, contagens, snapshots de performance, Console e limitações em `specs/088-vegetacao-densa/validacao.md`.
- [x] T015 Conferir cena salva/limpa, Console, colliders e ausência de novos assets/metas/GUIDs; auditoria final: 632 flores, 11 arbustos, 2 árvores novas, 645 objetos estáticos e zero colliders.
- [ ] T016 Integrar `088-vegetacao-densa` em `main` sem remover ou sobrescrever alterações locais preexistentes. A cena da branch foi salva; revisar o estado da branch principal imediatamente antes do merge.

## Dependências

- Setup T001–T003 → US1 T004–T008 → checkpoint visual → US2 T009–T013 → entrega T014–T016.
- T002 e coleta documental independente podem correr em paralelo; operações de cena/captura usam um único Editor sequencialmente.

## Estratégia

MVP é US1. O resultado diurno/noturno e a travessia precisam passar antes da distribuição integral. Só depois de expansão são feitas as capturas finais e integração.
