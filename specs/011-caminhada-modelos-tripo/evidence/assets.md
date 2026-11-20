# Integração inicial — Spec 011

A [cena](../../../Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity) foi criada e salva via MCP do Unity na pasta principal. A [captura](integracao-inicial.png) mostra o enquadramento inicial, o terreno texturizado, o protagonista com jaqueta azul e mochila e o marco amarelo.

- Fontes editáveis em `Source/Blender/Ilha_Tripo_011/`. O protagonista veio de `TripoAi/principal/stylized hiker 3d model.glb`. O FBX importado pelo Unity tem uma malha com esqueleto. A pose atual é a da importação.
- O terreno visual usa o GLB texturizado de `TripoAi/cenario/` e tem 580.632 triângulos. A malha estática de colisão deriva do mesmo terreno e tem 46.450 triângulos. A largura da ilha na cena é 160 m, com escala uniforme. O protagonista mede 1,8 m.
- Percurso inicial em metros, nas coordenadas do Unity. Início em (0; 9,09; 14), curva em (0; 9,11; 23) e marco em (9; 8,55; 23).
- Em Play Mode, a sessão inicializou. Um teste controlado pelo MCP aplicou movimento ao controlador nos quatro segmentos, completou uma ida e volta de 18 m por sentido e confirmou a volta exata ao ponto inicial ao chamar a recuperação. O teste usou entradas injetadas no controlador.
- Animação, título do HUD, renderização em 480 × 270, percurso com teclado e mouse, obstrução da câmera e queda pelo terreno aguardam validação.

## Escala atual

A escala anterior foi substituída por 57,600 × 48,396 × 9,857 m, com fator uniforme 0,36. Os degraus originais são a referência de proporção com o protagonista de 1,80 m. [Guia de reutilização](../../../Source/Blender/Ilha_Tripo_011/README.md).
