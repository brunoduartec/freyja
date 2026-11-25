# Tarefas — estudo de vegetação

## Preparação

- [x] T001 Confirmar projeto e cena no editor em `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`.

## Primeira avaliação visual

- [x] T002 [US1] Exportar o coqueiro e candidatos da paleta para `Freyja/Assets/ThirdParty/Nature032/`.
- [x] T003 [US1] Assentar o coqueiro e compor a vegetação do eixo com variantes de densidade em `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`.

## Validação

- [x] T004 [US1] Registrar comparação e validar Play Mode no percurso indicado em `quickstart.md`.

## Dependências

T001 → T002 → T003 → T004. O conjunto é um recorte visual independente.

## Implantação do estudo de grama

- [X] T005 [US1] Registrar as oito zonas e restrições em `Freyja/Assets/_Project/Art/Grass032/IslandGrassZones.json`.
- [X] T006 [US1] Gerar o layout determinístico em `Freyja/Assets/_Project/Art/Grass032/IslandGrassLayout.asset` e integrar na cena principal via Unity.
- [X] T007 [US1] Verificar proporções, exclusões e culling em Play Mode. Salvar capturas e medições em `specs/032-estudo-vegetacao/evidence/grama/`.

[Evidências da implementação e Play Mode](evidence/validacao.md).

[Evidência da implantação da grama](evidence/grama/validacao.md).

- [X] T008 Harmonizar o chão gramado com os tufos em `Freyja/Assets/_Project/Art/Grass032/Ground/`. [Validação visual](evidence/solo/validacao.md).

- [X] T009 Ampliar a cobertura do solo apto no perfil Grass032, ajustar lotes e validar a câmera. [Comparativo e medições](evidence/cobertura/validacao.md).

- [X] T010 Suavizar as divisões entre zonas, reduzir o envio de tufos e comparar os layouts no Player macOS. [Capturas e medição](evidence/manchas/validacao.md).
