# Orientações para a referência de terreno

## Uso

`referencia-terreno.png` é uma proposta visual, não uma planta métrica nem malha pronta. Compare-a com `specs/016-mapa-ambientes-ilha/mapa.png`, que continua sendo a referência de implantação. Avalie a imagem antes de submetê-la ao Tripo.

A referência mostra a enseada e o píer na frente, via principal até o largo, quatro blocos residenciais em cada setor, igreja indicativa, áreas de comércio/comunidade, hortas e ruína à esquerda e mirante ao fundo. As caixas coloridas e a igreja representam locais aproximados, não prédios finais; a função de cada bloco amarelo não está identificada na imagem. O contorno e os caminhos são uma interpretação artística, não uma correspondência ponto a ponto com a planta. Os lotes não têm dimensões ou orientação comprovadas, e alguns caminhos e patamares precisam ser examinados em 3D.

## Orientação para uma geração futura

Solicitar uma base de terreno de ilha low poly com costa, enseada e patamares amplos; sem fundir casas, igreja, árvores, água, píer e construções à malha quando o processo permitir. Preferir superfícies de chão acessíveis e deixar edificações para modelos separados. A imagem inclui esses elementos para comunicar a disposição; o Tripo pode interpretá-los como parte da geometria ou incluir o mar, portanto será necessário verificar e possivelmente remover ou reconstruir partes no Blender.

No Blender/Unity, comparar a malha com a planta, calibrar uma largura de referência próxima de 260 m apenas como ponto de partida, usar personagem de 1,80 m e blocos de casas, medir largura dos caminhos, escadas e acessos e avaliar visualmente os lotes. Ajustar proporções conforme teste; dimensões, navegação e tempo de percurso não foram verificados nesta etapa. O mirante, a ruína e os elementos naturais também são sugestões visuais que não alteram decisões narrativas.

## Prompt utilizado

Ferramenta integrada de geração de imagens, com `specs/016-mapa-ambientes-ilha/mapa.png` como referência:

> Use case: stylized-concept. Create an isometric aerial 3D concept image for a Tripo image-to-3D terrain reference for the game Freyja. Input image is the approved overhead layout, use it as spatial reference: south bottom foreground has a sheltered turquoise-water inlet and a simple wooden pier, a central winding dirt path rises gently to an open village square, four low residential plots to the left, four elevated plots on the far left/back, four plots on the right/east, a church plot above/right of the square, commercial and community plots near the square, gardens and a small ruin at rear left, a lookout on the northern ridge at the back. Show the complete island, including shoreline and top, in a single unobstructed high-angle wide view, north at the back. Terrain is the main subject: broad usable flat terraces connected by gradual traversable slopes and narrow footpaths, big faceted low-poly rock, low coastal vegetation, sunlit Mediterranean fishing island, warm sandy earth and turquoise sea, no towering cliffs or monumental stairs. Suggest the building sites by subtle simple untextured low blockout volumes of plausible human scale, with one slightly larger church-shaped placeholder, all clearly secondary to terrain, no complete buildings. The island should feel large enough for a dozen 5–8m homes and surrounding paths, about 260 by 180 m conceptually. Cohesive charming game art, stylized low-poly geometry, three-dimensional isometric render, bright daylight, clean neutral background, no text, labels, borders, icons, diagrams, grid, people or vehicles. Do not add other islands. Reference is approximate and must keep the broad path and geography relationships.
