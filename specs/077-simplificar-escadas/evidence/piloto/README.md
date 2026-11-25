# Aceite do piloto L3

A rampa invisível de 36,23° substituiu as 14 caixas de piso. O componente de IK deixou de receber a L3, preservando a B3 durante o piloto. A primeira travessia confirmou a geometria, mas encontrou 0,23 s consecutivos sem apoio em descida corrida e closes que escondiam o protagonista. A comparação controlada com a geometria anterior confirmou a perda de enquadramento na subida pela borda interna.

O controlador acompanha a inclinação do colisor registrado enquanto apoiado. Uma sonda curta usa a face da rampa, inclusive na transição a partir do patamar. A câmera conserva seu enquadramento central de escada e suas verificações de visibilidade. Perto da rampa, pode usar uma das posições elevadas já consideradas quando o próprio guarda bloqueia o início da sonda de órbita.

## Rodada de aceite

| Roteiro | Quadros | Resultado |
|---|---:|---|
| Caminhada longitudinal | 352 | Subiu, parou na entrada, no meio e na varanda e retornou ao terreno com apoio contínuo. |
| Corrida longitudinal | 267 | Subiu, parou e desceu com apoio contínuo. O Animator mostrou Run durante a travessia. |
| Duas bordas e tentativa junto ao guarda | 416 | Percursos pelos dois lados concluídos. O guarda reteve a tentativa lateral. |
| Duas diagonais com inversão | 221 | Entradas e retorno ao terreno nos dois lados, sem perda de apoio. |
| Salto no terreno inclinado junto ao pé da escada | 132 | Aproximadamente 0,50 s no ar, estado Jump e retorno ao apoio. |

Zero recuperações e zero amostras com o tronco fora do viewport. Caminhada e corrida usaram a câmera real e a RenderTexture de 480 × 270. O chão e o topo foram verificados juntos aos limites do lance.

Na revisão visual independente, o Game Designer aceitou subida, descida, pausa e borda interna com Walk e Run. Em algumas poses, uma sola aparece ligeiramente acima do degrau. A sequência não mostrou flutuação persistente ou atravessamento aparente dos guardas. As amostras deixam em aberto pequenos deslizamentos entre quadros. Os resultados não justificam novos clipes.

[Comparação visual](comparacao.jpg) · [Sequência de caminhada](caminhada-sequencia.webp) · [Sequência de corrida](corrida-sequencia.webp) · [Caminhada](caminhada.json) · [Corrida](corrida.json) · [Bordas](bordas.json) · [Diagonais](diagonais.json) · [Salto](salto.json)
