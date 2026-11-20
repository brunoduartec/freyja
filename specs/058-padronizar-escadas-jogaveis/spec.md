# 058 — Padronizar as escadas jogáveis

**Status** Implementada e validada em Play Mode.

## Cenário

O protagonista percorre cada escada acessível com passos regulares. A sola acompanha o degrau e os pés apontam na direção da subida. O jogador consegue parar no lance, retomar, chegar ao destino e descer.

## Requisitos

- Conferir os lances acessíveis nas cenas jogáveis ativas. O inventário da ilha registra a L3 e a B3. Incluir qualquer lance adicional encontrado.
- Usar a L3 ajustada como referência. Adaptar cada lance que precisar de correção ao espaço disponível, com desníveis de 18–20 cm e piso útil de pelo menos 26 cm. Medir também a entrada pelo terreno e a chegada ao patamar.
- Alinhar colisão e piso visível. Na B3, considerar a terra que cobre parte do primeiro degrau. Manter o acesso à porta e a circulação ao redor da casa.
- Sincronizar a alternância das pernas com os degraus, mantendo o apoio e a orientação da bota durante a transferência de peso.

## Critérios de aceitação

Registrar medidas por lance. Percorrer cada escada em Play Mode em duas velocidades, incluindo pausa, retomada e retorno à base. Gravar os pés de lado durante a subida e conferir a sola, a quina e a rotação de cada bota.

## Premissa

O escopo abrange as escadas que o jogador pode alcançar no jogo atual.
