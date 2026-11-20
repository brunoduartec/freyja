# Especificação: teste da ilha entre o cais e o centro

**Branch:** `018-teste-ilha-cais-centro`  
**Data:** 2026-11-14  
**Estado:** proposta para avaliação

## Objetivo

Testar o novo modelo Tripo com o protagonista de Freyja e apresentar um vídeo caminhando do cais até o centro, permitindo avaliar o cenário e a proporção entre cais, personagem e terrenos de casas.

## Cenário de avaliação

Ao assistir ao vídeo, o usuário vê o protagonista no desembarque, acompanha sua caminhada até o centro pela câmera do jogo e consegue observar o tamanho relativo dos caminhos e dos espaços para construções. Uma avaliação complementar informa as medidas e limitações observadas.

## Requisitos

- **RF-001:** Usar o arquivo fornecido `TripoAi/cenario/tripo_convert_5646c58d-74cc-4b9a-b04d-86cb22d83fa8.glb` como terreno desta avaliação e preservar sua fonte original.
- **RF-002:** Usar o protagonista existente, calibrado com 1,80 m, a câmera e o movimento adotados pelo projeto; avaliar a escala do cenário com referências humanas, cais, caminhos e lotes, sem presumir que todas as proporções do modelo são coerentes.
- **RF-003:** Configurar o cenário de teste na cena principal existente, preservando os elementos e alterações anteriores, sem substituição destrutiva.
- **RF-004:** Tornar percorrível o trajeto do cais ao centro, usando colisão apropriada. Registrar qualquer ajuste ao cenário e trechos intransitáveis, sem esconder defeitos da malha original.
- **RF-005:** Gravar e entregar vídeo real do percurso em Play Mode, do cais até o centro, com visão suficiente do personagem e do ambiente; não apresentar animação ou simulação como gravação do jogo.
- **RF-006:** Medir e documentar escala aplicada, dimensões do cais, largura dos caminhos e amostras dos terrenos de casas, com indicação de como foram medidas e limitações. Comparar com o protagonista e as dimensões residenciais iniciais de 5 × 6 m a 7 × 8 m; apontar quando não houver dados para concluir.
- **RF-007:** Preservar a cena e os assets anteriores; salvar alterações realizadas e entregar os caminhos dos artefatos, vídeo e verificações executadas.

## Critérios de sucesso

- **CS-001:** O personagem percorre em Play Mode um caminho identificável do cais até o centro; falhas reais de navegação são documentadas.
- **CS-002:** O vídeo mostra início no cais, caminhada e chegada ao centro, com personagem e cenário visíveis.
- **CS-003:** O relatório contém medidas do personagem, cais e amostras de lotes, permitindo avaliar a suspeita de desproporção sem transformá-la em conclusão antecipada.
- **CS-004:** O usuário recebe a cena, o vídeo e as limitações verificadas, sem perda dos arquivos anteriores.

## Premissas

- O novo modelo é uma avaliação, não uma aprovação do mapa definitivo. Sua escala e proporções ainda não foram inspecionadas.
- O centro é o largo identificável no modelo; se não estiver claro, documentar a escolha.
- A escala final, correções e uso de objetos provisórios serão explicados; não remodelar a ilha inteira nem produzir construções finais.
