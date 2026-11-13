# Modelo da maquete — Ilha bruta

Dados de organização para construção; não há save ou novos estados narrativos.

| Elemento | Identificação / dados | Regra |
| --- | --- | --- |
| Ilha | dimensões, altura máxima, mar e costa | Referências aproximadas; 1 unidade Unity = 1 m |
| Local | cais, largo, hortas, ruína, mirante | Identificável no Blender e no percurso |
| Casa provisória | B1–B4, A1–A4, L1–L4 | 12 volumes distintos: 4 baixas, 4 altas, 4 leste |
| Serviço provisório | venda, sala comunitária, galpão | Se representados, volumes distintos dos 12 de casa |
| Rota | início, destino, largura e inclinação | Caminho principal e retorno pelos fundos conectados |
| Área caminhável | relevo e colliders sólidos | Sem buracos ou colisão invisível sobre mar |
| Recuperação | spawn, altura de queda e bounds | Contém ilha e mirante; retorno ao spawn na água |
| Câmera | alvo, máscara de obstáculos e distância de visão | Preserva controles; não corta a ilha próxima indevidamente |

Blocos são fechados e não representam interiores. Os nomes B/A/L servem à conferência dos lotes, não a dados permanentes do jogo. Fontes e exports são arquivos editáveis; posição exata pode mudar após a caminhada.
