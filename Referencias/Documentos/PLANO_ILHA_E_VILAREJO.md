# Low Poly World — plano da ilha e do vilarejo

O novo cenário é um povoado pesqueiro para **30 moradores permanentes**, acessível somente de barco. O espaço precisa parecer pequeno o bastante para que três rostos novos mudem a rotina de todo mundo. As medidas abaixo são metas para o primeiro blockout no Blender.

## Direção visual aprovada

A [opção A — litoral ensolarado](../assets/concepts/ilha-dos-trinta-opcao-a.png) é a referência visual da ilha. Usar fachadas claras, telhados de terracota, venezianas verdes, vegetação baixa e mar turquesa na enseada. A luz quente do fim da tarde deve destacar os patamares, o costão facetado e o caminho que sobe do píer ao largo.

Manter a estética low poly de PS1 com silhuetas simples, faces visíveis no relevo e texturas pixeladas. Priorizar superfícies claras e legíveis. A ilustração orienta a paleta, a luz e a forma geral. O plano abaixo define a distribuição das casas e dos serviços.

## Relevo e circulação

| Zona | Medidas iniciais | Uso no jogo |
| --- | --- | --- |
| Ilha | Cerca de 260 × 180 m; ponto alto a 24 m acima do mar | Silhueta baixa e irregular. Costa sul recortada em enseada; norte e leste com costões. Água em todo o perímetro. |
| Enseada sul | Boca de 45–55 m; píer de 18 × 3 m; desembarque a cerca de +2 m | Único desembarque público. Do barco, ler primeiro o píer, depois os telhados e a encosta. |
| Terraço do vilarejo | Aproximadamente 105 × 65 m, entre +4 e +10 m | Casas e serviços assentados em patamares escalonados. Usar muretas curtas de pedra nos desníveis. |
| Encosta oeste | Entre +10 e +18 m | Hortas, caminho dos fundos e vestígios de uma ampliação abandonada. Volta ao miolo por uma rota secundária. |
| Crista norte | Entre +18 e +24 m | Mirante para reconhecer a ilha e avistar os barcos. Costões e vegetação marcam o limite da área caminhável. |

Planta de referência, norte para cima:

```text
                COSTÃO NORTE / MAR
                     Mirante
                        |
  Hortas e ruína — Casas altas (4)
          |             |
  Casas baixas (4) — Largo — Casas leste (4)
                     /    \
                Venda    Sala comunitária
                     |
             Caminho da enseada
                     |
          Galpão — Cais — ENSEADA SUL
```

Do píer ao largo, prever 60–80 m por um caminho de 2,5–3 m de largura. Deixar a subida principal contínua, com inclinação média de 6–10% e patamares onde ela muda de direção. Entre as casas, usar vielas de 1,5–2 m e degraus nos atalhos. O caminho oeste fecha um circuito e permite voltar ao largo por trás das casas. Meta de percurso a pé: 1 minuto do píer ao largo e 3–4 minutos para uma volta pelos principais lugares, sem paradas. O primeiro teste deve usar a câmera e a velocidade reais do passeio.

## Casas e serviços

| Setor | Casas | Ocupação proposta | Total |
| --- | ---: | --- | ---: |
| Baixas, perto da enseada | 4 | 3 + 3 + 2 + 2 | 10 |
| Altas, na encosta | 4 | 4 + 3 + 3 + 2 | 12 |
| Leste, junto ao largo | 4 | 3 + 2 + 2 + 1 | 8 |
| **Total permanente** | **12** | | **30** |

Junto ao largo ficam uma venda de mantimentos e uma sala comunitária para reuniões e arquivos. No cais, um galpão guarda redes, ferramentas e o caderno de chegadas. Esses três espaços e duas casas ligadas à investigação formam o primeiro conjunto de interiores. As outras dez casas entram com fachada, quintal e soleira legível.

Criar três bases residenciais: casa compacta de 5 × 6 m, familiar de 7 × 8 m e casa de encosta de 6 × 8 m com fundação escalonada. Variar orientação, varanda, anexos e vegetação para distinguir os endereços. Usar lotes de cerca de 9 × 12 a 12 × 15 m, com frentes de profundidades diferentes e fundos voltados para hortas ou muros. Aplicar reboco claro, telha de terracota e detalhes discretos de madeira nas fachadas. À noite, luzes quentes no largo e nas portas ajudam a orientar o caminho.

## Transição da cidade para a ilha

| Base atual | Decisão para o novo cenário |
| --- | --- |
| Biblioteca em `blender/scenes/cidade_modelos.blend` | Selecionar casas térreas compatíveis, simplificar fachadas e adaptar portas e fundações aos patamares. |
| Mercadinho e oficina | Aproveitar balcão, prateleiras, ferramentas e portas para a venda e o galpão. |
| Avenida, asfalto, calçadas contínuas e pontos de ônibus | Substituir por terra batida, pedra irregular, vielas e escadas. |
| Torres, shopping, conjuntos de apartamentos e garagem de ônibus | Retirar do programa da ilha. Concentrar a vida pública no largo e no cais. |
| Materiais da [cena urbana anterior](ART_DIRECTION.md) | Adaptar madeira e reboco à paleta aprovada e acrescentar telha de terracota, pedra costeira e areia. Trabalhar com PNG de 64 a 256 px e filtragem Closest. |

## Ordem de produção

1. **Relevo e acesso.** Criar uma cena independente, `blender/scenes/ilha_dos_trinta.blend`, com mar, costa, patamares e píer. Conferir a silhueta a partir do barco e o acesso à vila na altura dos olhos.
2. **Implantação.** Marcar as 12 casas, a venda, a sala comunitária e o galpão, a ruína e o mirante com volumes simples. Caminhar do cais ao largo e percorrer os dois ramos do circuito. Ajustar inclinações e distâncias antes de detalhar.
3. **Casas e pistas.** Adaptar a biblioteca de modelos, construir os cinco interiores iniciais e posicionar o caderno do cais, a foto da sala comunitária e os vestígios da ampliação. Usar a [lore](LORE_ILHA.md) para conferir o papel de cada lugar.
4. **Acabamento e integração.** Aplicar materiais, iluminação e referências de navegação. Exportar a ilha em arquivos próprios no passeio, com colisões, entradas e rotas ajustadas ao relevo. Verificar se o jogador encontra o largo a partir do cais, percorre os dois caminhos de volta e consegue distinguir os três grupos de casas.
