# Tarefas

- [x] T001 Confirmar projeto, cena e relógio no Unity. Evidência em plan.md.
- [x] T002 Criar fonte Blender e exportar arte provisória. Fonte em `Source/Blender/Gaivotas039/Gaivota.blend` e export em `Freyja/Assets/_Project/Art/Gaivotas039/Gaivota.fbx`.
- [x] T003 Configurar três trajetórias na cena principal pelo componente `CoastalGullFlight`. Usa o relógio existente. Amostrei 72 pontos por trajetória com raycasts descendentes e não houve colisões com o terreno ou o cais.
- [x] T004 Verificar compilação, chegada, retorno e noite em Play Mode. Ave no enquadramento sem obstrução na primeira amostra da chegada e na primeira amostra do retorno (0,50 s). O retorno foi simulado reposicionando o jogador no ponto inicial após afastá-lo. Às 22h, zero aves ativas. Console sem erros durante a validação.
- [x] T005 Registrar a câmera normal do jogador. GIF com 96 quadros a 480 × 270 em `Artifacts/Gaivotas039/sobrevoo-no-cais.gif`.

## Revisão das trajetórias

- [x] T006 Centralizar as três órbitas no píer, com raios e alturas distintos. Verificada a folga sobre obstáculos em 360 amostras por órbita.
- [x] T007 Validar e registrar as novas rotas em Play Mode. Ave no enquadramento aos 0,11 s na chegada e aos 0,50 s no retorno simulado. Às 22h, zero aves ativas. GIF da câmera em `Artifacts/Gaivotas039/gaivotas-ao-redor-do-pier.gif`.
