# Validação — Caminhada na ilha do TripoAI

[Cena Unity](../../../Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity) · [Vídeo da Game View](caminhada.mp4) · [Captura com HUD](game-view.png)

Unity 6000.3.25f1 com pipeline Built-in. Cena montada e testada em Play Mode pelo MCP do Unity. Os ciclos editáveis ficam em `Source/Blender/Ilha_Tripo_011/Protagonista.blend` e foram exportados pelo MCP do Blender. O Unity importou `Idle` (2 s) e `Walk` (1 s) em rig Generic com Avatar válido. O Animator usa a velocidade planar efetiva em solo e os clipes têm root motion desativado.

- **SC-001 — Percurso.** Com eventos de teclado e mouse injetados pelo Input System em Play Mode, completar duas idas e voltas de 18 m por sentido. O percurso saiu de (0, 14), virou em (0, 23) e alcançou o marco em (9, 23), no plano XZ. As duas voltas terminaram a menos de 2 cm do início. [Registro da execução](percurso-play-mode.txt)
- **SC-002 — Animação e imagem.** Confirmar `Walk` nos oito segmentos e `Idle` nas paradas. O teste com uma barreira registrou velocidade efetiva zero ao bloquear. A captura mostra mochila, pernas e silhuetas pixeladas, com o HUD na resolução da Game View. A câmera usa 480 × 270, filtro pontual, sem MSAA e apresentação 16:9. [Controles e colisões](controles-e-colisoes.txt) · [Captura](game-view.png)
- **SC-003 — Câmera e queda.** Na encosta perto de (5, 37), a câmera encurtou a distância para 1,08 m e voltou a 3,98 m em área livre. Para a queda, iniciar perto da margem em (10, 37) e caminhar em +Z. A posição retornou a (0; 9,127; 14) e a câmera voltou ao enquadramento inicial. [Registro do terreno](terreno-play-mode.txt)
- **SC-004 — Gravação.** O MP4 da Game View mostra a ida e a volta, com as paradas, mudança de direção e instruções na tela. Foram gravados 339 quadros em 960 × 540 a 24 fps, com duração de 14,125 s. Um quadro do MP4 foi decodificado e conferido perto do marco. [Gravação](caminhada.mp4) · [Registro da captura](gravacao-play-mode.txt)
- **Comandos e regressão.** WASD e órbita responderam a eventos do Input System. Esc liberou e o clique retomou o controle. A perda de foco foi simulada pelo callback do Unity. O teste axial e diagonal registrou 3 m/s em ambos. Na cena do cais, a captura, a tecla W, a órbita e o retorno ao ponto inicial foram confirmados via MCP. Os assets da cena têm os pares `.meta` e as cenas anteriores seguem sem alterações no Git. [Controles](controles-e-colisoes.txt) · [Cais](regressao-cais.txt)

Após os testes, o editor voltou à cena 011 em modo de edição. A consulta final registrou compilação inativa e zero erros, avisos ou mensagens no Console. O macOS indicou que esta sessão não está autorizada a enviar eventos pela janela. A avaliação com dispositivos físicos e a impressão visual da passada ficam para a revisão da entrega.

## Revisão de escala

As capturas e o vídeo de caminhada anteriores documentam a escala inicial. A cena atual usa os degraus como parâmetro e mede 57,600 × 48,396 × 9,857 m. Duas escadas foram percorridas em Play Mode pelo MCP na escala atual. [Resultado](escala-degraus-teste.txt) · [Escada atual](escada-escala-pelos-degraus.png) · [Ilha atual](ilha-escala-pelos-degraus.png).

## Human Basic Motions FREE

Repouso HumanM@Idle01 e caminhada in-place HumanM@Walk01_Forward de Kevin Iglesias, versão 2.4.2, adquiridos gratuitamente na Unity Asset Store. Arquivos de terceiros em Assets/ThirdParty/Kevin Iglesias/Human Basic Motions FREE. O Avatar do protagonista foi configurado como Humanoid válido. A velocidade de referência da caminhada é 2 m/s. No Play Mode, o controlador percorreu 5 m, ativou a caminhada e voltou a repouso ao parar. O Animator foi verificado reproduzindo HumanM@Walk01_Forward. Avaliação visual contínua e teste manual permanecem pendentes.
