# Inventário de escadas da cena principal

Foram revisadas as 18 peças implantadas e a circulação externa do cenário ativo. Foram encontrados dois lances acessíveis. As outras entradas têm soleiras ou patamares baixos. Os caminhos do terreno são contínuos. Os grupos de cenários anteriores que têm escadas próprias estão inativos na cena de jogo.

| Local | Peça | Percurso verificado | Evidência |
| --- | --- | --- | --- |
| L3, no terraço superior direito | 06 | Base da escada, sete degraus, varanda e porta superior. | [Subida](l3-subida.png) · [Chegada](l3-chegada.png) · [Varanda](l3-varanda.png) |
| B3, subida à esquerda | 07 | Base da escada frontal, degraus e patamar da porta. | [Subida](b3-56.png) · [Patamar](b3-94.png) |

[Vista da ilha ativa](ilha.jpg) · [Peças 01–06](pecas-01-06.jpg) · [Peças 07–12](pecas-07-12.jpg) · [Peças 13–18](pecas-13-18.jpg)

Os percursos foram executados em Play Mode aplicando movimento ao controlador do jogo via Unity Pipeline. Na L3, o protagonista permaneceu apoiado durante a pausa no meio do lance e na varanda. O guarda frontal bloqueou a saída lateral. As duas descidas terminaram no chão com zero recuperações. [Percursos](percursos.txt) · [Passada L3](passada-l3.txt) · [Passada B3](passada-b3.txt)

Unity 6000.3.25f1. O Console registrou zero erros e avisos ao fim da validação.

[Vídeo dos dois percursos](escadas-l3-b3.mp4). A L3 aparece primeiro e a B3 começa em 00:10.

Após a revisão da animação, a subida ganhou passada própria. O movimento alterna apoio e elevação de cada pé, mira o próximo degrau e limita a velocidade de subida a 1,25 m/s. O controle retorna à velocidade solicitada ao sair do lance. A componente existente de caminhada segue usada na saída e na descida. [Vídeo corrigido](escadas-passada-corrigida.mp4) · [Amostras de pose](passada-corrigida.tsv) · [Percursos repetidos](percursos-pos-correcao.txt)

A passada de subida usa o início físico de cada degrau para determinar o contato e mantém o alvo de apoio fixo enquanto o outro pé avança. O terreno cobre parte do primeiro degrau da B3, então o alvo considera a superfície mais alta naquele ponto. Em Play Mode, foram medidos 12 contatos em cada velocidade solicitada (3,00 e 0,75 m/s), somando 463 amostras de apoio. O maior desvio do tornozelo em relação ao alvo foi de 1 mm na horizontal e 1 cm na vertical. Os dois percursos chegaram ao topo e retornaram ao chão nas duas velocidades. [Captura](sincronismo-degraus.gif) · [Medições](sincronismo-degraus.tsv)
