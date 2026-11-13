# Ilha dos Trinta — personagens e visual PS1

## Modelos e elenco

O [elenco de moradores](ELENCO_ILHA_DOS_TRINTA.md) registra nomes, funções e relações familiares nas 12 casas.

Usar personagens humanos low poly com proporções próximas das reais e mãos, sapatos e acessórios um pouco maiores para ajudar na leitura à distância. Construir cabelo em massas simples, mãos em bloco e roupa com poucos volumes. Concentrar a geometria no contorno da cabeça, nos ombros, nas mãos e nas articulações. Pintar olhos, sobrancelhas, costuras e botões no atlas. Usar faces planas no cabelo e nos acessórios e suavização seletiva no rosto e nas articulações.

| Grupo | Direção de arte | Meta inicial por personagem |
| --- | --- | --- |
| Protagonista | Jaqueta azul desbotada e mochila pequena, reconhecíveis de costas na chegada. | 650–900 triângulos e atlas de 128 × 128. |
| Dois amigos | Um com camisa coral e bolsa lateral, outro com corta-vento verde e boné claro. Variar altura, largura dos ombros e postura. | 500–750 triângulos e atlas de 128 × 128. |
| Trinta moradores | Três bases corporais com variações de altura e proporções. Diferenciar idade, cabelo, postura e roupa pela rotina de cada pessoa. | 350–650 triângulos e atlas de 64 × 64 ou 128 × 128, conforme o enquadramento. |

As faixas são pontos de partida para o protótipo. Contar corpo, roupa, cabelo e acessórios juntos na malha triangulada. Usar a mesma estrutura de esqueleto para compartilhar caminhada e movimentos básicos. Criar variações de cabeça, cabelo e roupa por cima das bases e personalizar postura, ritmo e gesto de descanso. Distribuir as 30 fichas de morador pelas 12 casas do [plano da ilha](PLANO_ILHA_E_VILAREJO.md), com nome, endereço, rotina e um traço visual que o jogador consiga lembrar.

Começar pelo protagonista, os dois amigos e três moradores que aparecem cedo. A pessoa do cais usa botas e avental de trabalho, Teresa Faria tem silhueta mais estreita e cardigã claro, e a da venda usa mangas dobradas e avental curto. Usar roupas em creme, índigo, verde e coral para os personagens se destacarem das fachadas e da vegetação. Testar o elenco piloto em grupo no cais e no largo antes de produzir os demais.

Para a possessão, criar uma variação de atuação compatível com qualquer pessoa elegível. Durante uma conversa, o personagem pode parar uma tarefa habitual por um instante, sustentar o olhar um pouco mais ou responder após uma pausa. Usar a mesma aparência e o mesmo material antes e depois da possessão para que a pista venha da mudança de comportamento.

## Renderização

| Efeito | Como obter e conferir |
| --- | --- |
| **Serrilhado de silhueta** | Renderizar o mundo 3D em um buffer de **480 × 270** para o primeiro teste. Comparar com **320 × 180** para um efeito mais forte e **640 × 360** para diálogos mais legíveis. Desligar antialiasing no mundo e ampliar por vizinho mais próximo, de preferência em escala inteira. Manter a interface em resolução de tela. Verificar degraus no ombro, no cabelo e na alça da mochila no quadro final. |
| **Texturas pixeladas** | Usar amostragem por vizinho mais próximo nos atlas de personagens. Pintar olhos, cabelo e roupa em grupos de texels. Conferir se a textura continua nítida em close e se detalhes pequenos piscam à distância. |
| **Tremor de vértices** | Após animar a malha e projetá-la na tela, encaixar as posições dos vértices na grade do buffer. Testar o passo de 1 pixel para ver a silhueta saltar levemente quando a câmera se move. Aplicar o efeito à projeção visual e manter o movimento e as colisões contínuos. |
| **Texturas que deformam em perspectiva** | Testar interpolação afim de UVs em uma faixa da roupa, uma bolsa e uma superfície maior do cenário. Girar a câmera e observar a deformação em ângulos oblíquos. Dar ao efeito um controle separado para comparar a leitura de rosto e roupa em diálogo. |
| **Cor e dithering** | Testar um modo com redução para 5 bits por canal RGB e dithering ordenado em grade de 4 × 4, na resolução interna. Conferir o padrão nas sombras da roupa e nos tons de pele durante uma panorâmica. |
| **Animação** | Testar poses a 12 e 15 quadros por segundo em caminhadas e gestos curtos, com deslocamento contínuo. Esse ritmo é uma escolha de direção para o jogo. Conferir contato dos pés no chão e leitura dos gestos. |

O serrilhado depende da renderização na câmera do jogo. A malha low poly e os atlas definem a forma e os detalhes visíveis a cada distância. O padrão de cor, a interpolação afim e a grade de vértices tomam como referência a documentação técnica do console. [PSX-SPX — GPU](https://psx-spx.consoledev.net/graphicsprocessingunitgpu/) · [Interpolação da GPU](https://psx-spx.consoledev.net/ps1/gpu/misc/)

## Teste de aceitação

1. Modelar o protagonista e um morador no Blender. Preparar uma caminhada, uma virada e um gesto de conversa. Exportar as duas malhas para o passeio.
2. Gravar a mesma tomada no cais com os três tamanhos de buffer, mantendo enquadramento, luz e animação. Usar uma caminhada a média distância e um diálogo próximo. Comparar os efeitos de vértices, UVs e cor ligados individualmente e depois em conjunto.
3. Aprovar a configuração quando os degraus de pixel forem visíveis na imagem final, as duas silhuetas continuarem distinguíveis a média distância e olhos e boca permanecerem legíveis em diálogo. Conferir se a imagem fica estável com a câmera parada e se o tremor aparece de modo perceptível durante a caminhada ou uma panorâmica.
4. Registrar a resolução escolhida e os efeitos ativos com uma captura e um clipe curto. Usar o resultado para calibrar o restante do elenco.
