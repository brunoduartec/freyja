# Especificação: referência do terreno para Tripo

**Branch:** `017-referencia-terreno-tripo`  
**Data:** 2026-11-14  
**Estado:** proposta para avaliação

## Objetivo

Criar uma referência visual clara para gerar posteriormente no Tripo uma nova base de terreno da ilha de Freyja, respeitando a organização da planta aprovada e mantendo áreas úteis para as construções e caminhos.

## Cenário de avaliação

O responsável pelo jogo compara a imagem com o mapa da entrega 016 e consegue reconhecer a enseada e a chegada de barco, o acesso ao largo, as áreas residenciais e os demais espaços relevantes. A imagem é apresentada para avaliação antes de qualquer geração ou substituição do cenário 3D.

## Requisitos

- **RF-001:** Usar `specs/016-mapa-ambientes-ilha/mapa.png` e `inventario.md` como referência de distribuição, e o GDD como fonte da direção visual e das necessidades do jogo.
- **RF-002:** Apresentar uma vista que deixe legíveis o contorno da ilha, a enseada sul, o cais, os patamares e a relação entre caminhos e áreas previstas para casas, comércio, igreja e outros ambientes do mapa.
- **RF-003:** Privilegiar o terreno como base a ser gerada; apresentar construções, se necessárias para leitura, apenas como referências de implantação claramente distinguidas da futura geometria final. Preservar espaço para modelos separados.
- **RF-004:** Considerar o protagonista de 1,80 m e os aproximadamente 260 × 180 m como referências de proporção, sem representar a imagem ou a futura geração como medição exata. Evitar escadas monumentais e desníveis que pareçam inviáveis.
- **RF-005:** Manter o litoral ensolarado, mar turquesa, relevo low poly e demais princípios visuais do GDD; sinalizar diferenças relevantes em relação à planta e compromissos necessários à legibilidade.
- **RF-006:** Entregar imagem e breve orientação de uso no Tripo, com limitações e necessidade de conferir escala e navegabilidade no Blender/Unity. Não gerar ou substituir modelos 3D nesta etapa.

## Critérios de sucesso

- **CS-001:** A enseada, a rota ao povoado, os setores residenciais e as áreas de comércio e igreja são identificáveis quando comparados à planta.
- **CS-002:** A proposta contém áreas planas ou suavemente escalonadas suficientes para indicar implantação, sem prometer que suas dimensões sejam exatas.
- **CS-003:** O usuário recebe a imagem e as instruções de uso e pode aprovar ou solicitar ajustes antes de gerar a malha.

## Premissas e limites

- A aprovação da planta autoriza usá-la como referência; detalhes de igreja, loja adicional e identidade da casa afastada permanecem provisórios conforme a entrega 016.
- Imagem e geração por IA podem alterar proporções e conexões; medidas, caminhos e escadas exigem validação posterior em 3D.
- A entrega produz uma referência visual, não o cenário final nem uma atualização do GDD ou da cena Unity.
