# Revisão no Unity

1. Abra o projeto Unity da branch 033 e a cena `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`.
2. Entre em Play Mode e olhe para cima com o mouse durante a caminhada. O ciclo leva 96 s por volta por padrão.
3. Para comparar horários, selecione `Coastal Sun`, desmarque `Advance Clock` e ajuste `Hour` para 6, 12, 18 e 0. Observe o céu e a diferença de sombra no mesmo enquadramento.
4. Com a hora fixa em 12 e 0, caminhe do cais à praça e compare a leitura do caminho e dos pontos de interação existentes.

## Resultado da validação

- Unity 6000.3.25f1, Built-in, Metal. Compilação com zero erros e zero avisos. O shader foi confirmado como suportado e sem erros. O Console registrou mensagens do Package Manager e da indexação do próprio editor durante a sessão.
- [Comparativo do cais](evidencias/comparativo.png) em um enquadramento fixo a 480 × 270 nas quatro fases. O disco solar aparece junto ao horizonte no pôr do sol.
- [Vídeo do ciclo](evidencias/ciclo-dia-e-noite.mp4) com 38 s de captura a 15 fps. Play Mode com duração ajustada para 36 s durante a gravação. A hora passou de 05:37 a 06:55 do dia seguinte. Os 570 quadros registraram uma única passagem pela meia-noite. O valor padrão da cena é 96 s.
- Caminhada em Play Mode usando o controlador do jogo. As duas passagens completaram os 11 pontos do percurso, cerca de 70 m, sem recuperação. O piso e as fachadas são visíveis nas capturas [diurna](evidencias/percurso-dia.png) e [noturna](evidencias/percurso-noite.png). A cena avaliada não expõe interações ativas nesse trecho.
- [Céu visto da praça ao meio-dia](evidencias/ceu-da-praca.png), com a câmera inclinada para cima pelo controlador de câmera do jogo.
