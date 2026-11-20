# Especificação: blocos de construções na ilha nova

**Branch:** `019-blocos-ilha-nova`  
**Data:** 2026-11-14  
**Estado:** aprovada pelo usuário; implantação em avaliação visual

## Objetivo

Posicionar blocos na ilha Tripo testada na entrega 018 para avaliar, com o protagonista, a distribuição, os tamanhos e as orientações das construções mostradas na referência, preparando sua substituição posterior por modelos separados.

## Cenário de avaliação

O responsável pelo jogo observa uma vista geral e percorre o cenário com o personagem. Identifica as construções, sua relação com os caminhos, praia, cais e relevo, e consegue comparar a implantação com `specs/017-referencia-terreno-tripo/vila-casa-isolada.png`.

## Requisitos

- **RF-001:** Usar a cena principal `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` e a ilha nova na escala experimental de 100 m; preservar os elementos anteriores e o protagonista de 1,80 m.
- **RF-002:** Comparar a imagem com o terreno e posicionar volumes individuais para as 12 habitações, inclusive uma casa pequena e afastada do velho; sua identidade e o remanejamento das famílias permanecem pendentes.
- **RF-003:** Posicionar blocos para igreja, venda e loja, sala comunitária e galpão do cais conforme o programa da entrega 016 e a imagem; não confundir estes volumes com casas.
- **RF-004:** Propor posição, largura, profundidade, altura e orientação da frente de cada bloco conforme a imagem, a topografia, a escala humana e as referências residenciais anteriores de 5 × 6 m, 7 × 8 m e 6 × 8 m. A imagem é conceitual, não uma medição ou projeção exata; marcar como estimativa as dimensões, ângulos e locais não determináveis.
- **RF-005:** Identificar cada volume individualmente e registrar seus dados de implantação e frente, para facilitar futura substituição. Não atribuir silenciosamente uma identidade ao velho nem alterar o GDD.
- **RF-006:** Conferir acesso aos blocos, circulação pelo cais e centro e visualização na câmera do jogo; registrar vistas gerais, vistas próximas e conflitos de terreno ou de escala. Não modificar o terreno nem reduzir o personagem silenciosamente para forçar encaixe.
- **RF-007:** Preservar praia e píer e manter a entrega em volumes provisórios, sem modelos finais ou interiores.

## Critérios de sucesso

- **CS-001:** Os 17 volumes previstos estão individualizados: 12 residências, igreja, duas lojas, sala comunitária e galpão. Um dos 12 representa a casa afastada, sem acrescentar uma 13ª família.
- **CS-002:** Todos os volumes possuem posição, dimensões, orientação e frente documentadas; divergências em relação à referência e entraves de espaço estão identificados.
- **CS-003:** A cena, vistas gerais e capturas de personagem permitem avaliar a implantação e as proporções antes de gerar modelos.
- **CS-004:** A navegação afetada é verificada em Play Mode e qualquer acesso bloqueado ou não testado é relatado.

## Premissas e limites

- Os 100 m são escala de teste, não decisão definitiva. A implantação poderá revelar necessidade de revisão.
- A imagem não garante contagem, medidas, ângulos ou distâncias exatos; usar o programa da entrega 016 para completar locais que ela omita. Um galpão anterior permanece preservado, sem duplicação visual acidental.
- A casa do velho integra as 12 habitações e não aumenta os 30 moradores; sua identidade e a revisão da ocupação das casas do GDD continuam pendentes.
- O papel da loja adicional e o local definitivo da casa isolada permanecem propostas a avaliar.
