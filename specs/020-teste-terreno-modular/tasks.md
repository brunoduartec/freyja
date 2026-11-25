# Tarefas — terreno modular 020

## Preparação
- [x] T001 Registrar aprovação, plano e pesquisa em specs/020-teste-terreno-modular/.
- [x] T002 Preservar estado inicial e cena em specs/020-teste-terreno-modular/evidence/.

## Fundação
- [x] T003 Produzir kit e texturas editáveis no Blender em Source/Blender/Terrain020/ e exports em Freyja/Assets/_Project/Art/Terrain020/.

## US1 — caminhar no trecho
- [x] T004 [US1] Construir heightmap, patamares e caminho com Tools/Terrain020/BuildTerrain020.cs.
- [x] T005 [US1] Montar cais, rochas, alvenaria e dois volumes em Freyja/Assets/_Project/Prefabs/Terrain020/.
- [ ] T006 [US1] Validar medidas, apoio, largura livre, inicialização e recuperação em specs/020-teste-terreno-modular/evidence/validacao.json.
- [ ] T007 [US1] Gravar percurso real com Tools/Terrain020/RecordTerrain020.cs e vídeo em specs/020-teste-terreno-modular/evidence/caminhada.mp4.

## US2 — comparação e edição
- [ ] T008 [US2] Criar alternância em Edit Mode em Freyja/Assets/_Project/Editor/Terrain020Preview.cs e validar restauração da ilha.
- [ ] T009 [US2] Capturar geral e detalhes e conferir editabilidade em specs/020-teste-terreno-modular/evidence/.

## Entrega
- [ ] T010 Salvar fontes/exports da feature na worktree, estado da prévia e parecer em specs/020-teste-terreno-modular/evidence/parecer.md.

Dependências: T001 → T002 → T003 → T004 → T005 → T006 → T007; T008 depende de T004/T005; T009 após T005; T010 após as verificações. Não há trabalho paralelo de implementação no mesmo editor. Pesquisa e desenho dos módulos podem ocorrer em paralelo antes da integração.

Estratégia: validar primeiro os patamares e a caminhada (US1), depois alternância e apresentação (US2); parar ao concluir esta prova, sem expandir para a ilha inteira.
