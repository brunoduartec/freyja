# Direção proposta e reprodução

**Estado**: proposta visual para avaliação; ainda não instalada no jogo. A [imagem de personagens no cais](../../Referencias/ilha-dos-trinta-personagens.png) é a referência mais próxima da meta, conforme indicado pelo usuário. A [captura de teste](proposta-640x360-1920x1080.png) ainda difere dela em enquadramento, materiais, luz e cenário. A comparação e limitações estão em [research.md](research.md).

## Receita visual

1. Renderizar a câmera de mundo em 640 × 360 (16:9), preservando enquadramento e FOV do jogo. Usar uma Render Texture com filtro Point, sem mipmaps e sem MSAA como ponto inicial; a captura usou HDR e MSAA desativados na câmera. Manter Built-in.
2. Apresentar a imagem com vizinho mais próximo, escala inteira uniforme e sem esticamento: 2× para 1280 × 720, 3× para 1920 × 1080. Para uma saída maior ou de outra proporção, escolher `floor(min(largura/640, altura/360))`, centralizar o retângulo e preencher a sobra com faixas. Em 1280 × 960, a área é 1280 × 720 com 120 pixels de faixa em cima e embaixo. A política para saída menor que 640 × 360 permanece aberta.
3. Compor interface e texto depois do mundo, na resolução de saída, para manter a leitura; tratar a escala da UI separadamente. Testar a interface real antes de fechar essa decisão.
4. Para assets novos, produzir geometria de silhueta simples e texturas pequenas quando fizer sentido; usar Point nos mapas que precisam de texels definidos, comparando mipmaps e filtros em movimento para evitar cintilação. O tamanho deve seguir o objeto e sua distância à câmera, não um número universal. Não reduzir nem refiltrar os modelos existentes sem comparação visual.
5. Não adicionar dithering, distorção ou contornos como regra por enquanto. Avaliar iluminação e contraste na cena; garantir que rostos e pistas ocupem área suficiente na câmera real. A opção de aproximação de pista deve ser avaliada se detalhes desaparecerem.

## Implementação futura no Built-in

Criar uma Render Texture 640 × 360 e atribuí-la ao `Target Texture` da câmera de mundo; exibi-la em um elemento de apresentação que preserve a escala inteira e o aspecto, com UI em camada posterior. Atenção: a câmera renderiza toda a textura, independentemente do seu `rect`; o ajuste e as faixas pertencem à apresentação. Validar que não haja segunda renderização inesperada, entradas bloqueadas ou perda da textura ao mudar resolução. Nenhum componente dessa solução foi instalado nesta entrega.

## Reproduzir a captura desta entrega

1. Abrir `Freyja/` no Unity 6000.3.25f1, cena `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`; selecionar `Cais Gameplay Camera` (FOV 58°, perspectiva).
2. Capturar essa câmera diretamente em 640 × 360, fora do Play Mode, com HDR e MSAA desativados na câmera e materiais existentes. Salvar PNG sem interface.
3. Ampliar o PNG em 3× usando vizinho mais próximo, sem suavização, para 1920 × 1080. A entrega usa essa ampliação externa apenas para demonstrar o resultado.
4. Comparar personagem, telhado e cais com a imagem de referência. Testar também 480 × 270 conforme o GDD, com tomada de caminhada e enquadramento próximo de diálogo, antes de escolher definitivamente a resolução. Após implementar o renderer, testar em Play Mode caminhada e rotação, rostos e pistas reais, UI, 720p, 1080p e proporção diferente; registrar cintilação, enquadramento e faixas. Essas validações ainda estão pendentes.
