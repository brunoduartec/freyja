# Tarefas da grama costeira

## Preparação

- [X] T001 Confirmar cena, terreno, câmera, dados de solo e ambiente. Evidência em [plano](plan.md).

## P1 — Caminhar por um trecho com grama costeira

- [X] T002 [P] [US1] Produzir tufos originais em `Source/Blender/Grass029/CoastalGrass.blend` e exportar para `Freyja/Assets/_Project/Art/Grass029/`.
- [X] T003 [P] [US1] Criar material de grama com vento estável em `Freyja/Assets/_Project/Art/Grass029/CoastalGrass.shader`.
- [X] T004 [US1] Preparar a distribuição e exclusões no editor em `Freyja/Assets/_Project/Editor/CoastalGrassAuthoring.cs` e salvar a cena principal.

## P2 — Observar o trecho de perto e de longe

- [X] T005 [US2] Renderizar lotes determinísticos por câmera, com culling, transição de distância e contadores em `Freyja/Assets/_Project/Scripts/Environment/`. T004 integra o componente após T005.
- [X] T006 [US2] Conferir compilação, Play Mode, distâncias, borda de enquadramento e legibilidade. Salvar três tomadas em `specs/029-grama-estilizada/evidence/`.
- [X] T007 [US2] Comparar os três modos no mesmo percurso e registrar p50, p95 e contadores em `specs/029-grama-estilizada/evidence/validacao.md`.

Evidência de execução em [validação](evidence/validacao.md).

- [X] T008 Revisar a densidade da distribuição em `CoastalGrassAuthoring.cs`, salvar o layout e conferir a cena no Unity. [Comparativo e medições](evidence/densidade/validacao.md).
