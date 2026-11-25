# Especificação de grama estilizada para Freyja

**Branch** `029-grama-estilizada`  
**Criada em** 2026-11-18  
**Status** Aprovada para implementação em 2026-11-18  
**Pedido** Grama inspirada em Zelda com renderização otimizada por culling.

## Cenários de uso e testes

### P1 — Caminhar por um trecho com grama costeira

Ao caminhar pela ilha, o jogador vê tufos baixos e irregulares que se inclinam em ondas de vento. As silhuetas funcionam na câmera pixelada do jogo e os caminhos e pistas próximas continuam legíveis.

1. Ao caminhar por uma borda de trilha, o jogador vê variação de altura, cor e densidade, com solo aparecendo entre as manchas.
2. Ao aproximar-se de uma pista, porta ou escadaria, a vegetação deixa livre a área necessária para ler e interagir.

### P2 — Observar o trecho de perto e de longe

Ao caminhar, parar ou girar a câmera, a grama perde detalhe gradualmente com a distância. O campo distante mantém a cor de vegetação no solo.

1. Ao mover a câmera através da transição, os tufos visíveis diminuem de forma progressiva e voltam nas mesmas posições no retorno.
2. Com o culling ativo, áreas fora do enquadramento ou além do alcance deixam de ser enviadas para desenho.
3. Nas curvas, nos desníveis e nas bordas da tela, a grama que ainda está visível permanece presente, inclusive durante o balanço do vento.

## Requisitos

- **FR-001** — Aplicar grama costeira estilizada em um trecho caminhável da cena principal, com variações de tufos e rajadas coerentes.
- **FR-002** — Restringir a distribuição a áreas de solo aptas e preservar a leitura de caminhos, acessos e pontos de investigação.
- **FR-003** — Reduzir o detalhe com a distância e omitir a grama 3D fora do enquadramento ou do alcance. Manter a continuidade visual do solo distante.
- **FR-004** — Comparar o mesmo percurso sem grama, com grama sem culling e com culling ativo, sob as mesmas condições.

## Critérios de sucesso

- **SC-001** — Três tomadas da câmera do jogo mostram a grama em uma aproximação, um giro e uma vista aberta. A borda da trilha e o ponto de interesse escolhido continuam legíveis nas tomadas em que aparecem.
- **SC-002** — O percurso cruza as transições de distância em ida e volta sem aparecimento ou sumiço abrupto de uma mancha inteira.
- **SC-003** — Em dois enquadramentos de teste, um com grama fora da vista e outro com grama além do alcance, nenhum grupo inteiramente excluído é enviado para desenho. O percurso com culling envia menos grama no total que o mesmo percurso com todos os grupos ativos.
- **SC-004** — Registrar custo de quadro mediano e p95 nos três cenários. Usar como meta inicial de avaliação um aumento de até 10% no p95 com grama e culling em relação à cena sem grama, após definir máquina e resolução de referência e verificar a variação entre execuções.

## Premissas

- A referência visual adotada é *Breath of the Wild*. Freyja mantém vegetação baixa, formas low poly, silhuetas pixeladas e a paleta do litoral.
- A primeira entrega proposta cobre um trecho da cena `Ilha_Tripo_Caminhada_011.unity`. A grama dessa etapa é decorativa.
- O [estudo técnico](estudo.md) reúne opções e valores de partida para avaliação.
