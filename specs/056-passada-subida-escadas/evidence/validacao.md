# Validação em Play Mode

Após a revisão visual, o Animator ganhou o estado StairAscent com o clipe humanoide SubidaEscada. A fase acompanha o avanço entre contatos. A nova gravação usa quadros do ciclo normal do Unity e mostra a flexão dos joelhos. Os quatro percursos confirmaram o estado ativo durante a subida, pausa estável e retorno à base. [Vídeo atualizado](clipe-subida-play-mode.mp4) · [Percursos com o clipe](percursos-com-clipe.txt)

## Captura da iteração anterior

A cena principal foi testada por comandos aplicados ao controlador em Play Mode. L3 e B3 foram percorridas com aproximações solicitadas de 3,00 m/s e 0,75 m/s. Os quatro percursos incluíram pausa no meio do lance, retomada, chegada ao topo e retorno à base. A posição permaneceu estável durante as quatro pausas.

Foram coletadas 217 amostras da ponta aproximada do sapato no trecho intermediário do balanço. A menor folga medida foi de 5 mm acima da geometria de colisão. Nos 280 quadros de apoio, o maior desvio entre a sola estimada e o alvo foi de 3,4 cm, na aproximação lenta. As métricas usam pontos aproximados do sapato derivados do modelo, sem medir cada vértice da malha. A câmera da L3 foi posicionada do lado interno do corrimão para mostrar a passada. A captura tem câmera lenta na aproximação normal.

[Captura lateral](subida-lateral.mp4) · [Amostras](amostras.tsv)

## Trechos da captura

- 0.0s — L3, aproximação normal
- 8.7s — B3, aproximação normal
- 13.3s — L3, aproximação lenta
- 22.2s — B3, aproximação lenta
