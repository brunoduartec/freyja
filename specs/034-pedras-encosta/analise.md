# Análise técnica — Pedras da encosta

A [referência da vila](../017-referencia-terreno-tripo/vila-casa-isolada.png) forma a costa com grupos altos de blocos inclinados e parcialmente enterrados. À esquerda, junto ao cais, os grupos descem até a água e alternam faces expostas com patamares de grama. Nas áreas altas e entre caminhos, os afloramentos são mais baixos e espaçados. As faces iluminadas puxam para bege e ocre, com cinzas mais quentes nas sombras. A [captura da ilha compactada](../../Artifacts/IlhaCompacta030/ilha-depois-enquadrado.png) mostra uma faixa quase contínua de pedras cinza, de porte semelhante, enquanto boa parte dos taludes internos permanece lisa.

Na cena ativa `Ilha_Tripo_Caminhada_011`, o grupo `Island023/Pedras e árvores editáveis` contém 125 instâncias de três prefabs `Rock_1`, `Rock_2` e `Rock_3` do Kit PS1 022, além de 34 árvores. As três malhas têm 20 triângulos cada. O [gerador do Island023](../../Freyja/Assets/_Project/WorldBuilder/Editor/Island023.cs) originalmente distribui as pedras costeiras ao longo de um contorno elíptico, com escala uniforme. Isso explica a repetição observada e dá um ponto de partida para a montagem, pois as pedras existentes já são editáveis por instância.

| Abordagem | Aplicação ao trecho |
| --- | --- |
| Montagem no Unity com os prefabs atuais | Bom primeiro teste. Remover a repetição no trecho e montar blocos em grupos com alturas e escalas diferentes, orientação coerente com a inclinação e bases enterradas. Usar as pedras existentes como formas secundárias e experimentar um material mais quente. Se as peças ainda parecerem pedregulhos soltos, a silhueta é o ponto a corrigir. |
| Blender para as silhuetas principais | Melhor opção caso a recomposição precise de malha nova. Criar duas peças editáveis, uma formação costeira alta com faces alongadas e outra baixa e larga para emergir no talude. Dá controle sobre onde o perfil se enterra, a inclinação das faces e o encaixe com o caminho. Exportar e montar como prefabs no Unity. |
| Tripo para uma formação isolada | Alternativa se um rochedo específico pedir uma forma orgânica difícil de obter no piloto. Usar um recorte isolado. Para integrar a malha, ainda seria necessário revisar no Blender a base, o pivô, a densidade da geometria e a paleta. A referência geral em perspectiva tem pouca informação sobre as faces ocultas e o contato com o solo. |
| Pintura do terreno | Complementa os grupos com terra exposta ou pedra na junção com a grama. A silhueta costeira depende dos volumes das malhas. |

**Recomendação** — Testar a composição na encosta oeste com os prefabs atuais e comparar a vista geral com a câmera jogável. Se faltar altura contínua ou uma base que encaixe no talude, modelar os dois perfis no Blender e reaproveitar as pedras pequenas ao redor. A direção de cor deve ser testada sob a luz do jogo. Para colisão perto da curva, conferir o espaço real que o jogador tem para passar. Os prefabs existentes usam caixas, que podem avançar além da silhueta quando a peça é alongada. A aprovação do piloto orienta uma eventual expansão.

A inspeção no Unity foi somente de leitura. A comparação visual usa capturas existentes da spec 030.
