# Spec 004 — Relevo da ilha e recanto isolado

**Estado:** encerrada a pedido do usuário, com entrega parcial e limitações registradas em [evidence/percurso.md](evidence/percurso.md).

## Cenários e testes

### P1 — Reconhecer a ilha pela silhueta

Como pessoa avaliando o jogo, quero ver uma ilha mais compacta e um grande monte ao fundo, com a disposição geral da referência [modelagem-vista-aerea.png](../../Referencias/modelagem-vista-aerea.png), para que o terreno se aproxime da direção visual desejada.

**Aceitação:** ao comparar vistas aéreas, a ilha apresenta uma enseada de chegada, áreas em diferentes níveis e um monte claramente dominante atrás do povoado. O terreno permanece uma maquete simples, aberta a ajustes após avaliação.

### P1 — Caminhar e descobrir o outro lado

Quero sair do cais, caminhar por todo o terreno da ilha, inclusive subir o monte e contorná-lo até um recanto isolado, onde existe uma casinha simples de uma pessoa que vive sozinha e de quem ninguém fala.

**Aceitação:** qualquer ponto da superfície terrestre da ilha pode ser alcançado caminhando, inclusive o topo do monte, suas encostas e a costa; o jogador consegue voltar sem ficar preso. A casinha fica atrás do monte e não é visível a partir do largo principal. A ideia de isolamento é transmitida pela posição, sem exigir personagem ou diálogo nesta entrega.

### Casos de borda

- A passagem entre cais e terreno e todas as encostas entre níveis devem ser caminháveis; o relevo não pode depender de saltos, escalada ou teletransporte para acessar qualquer parte da superfície terrestre.
- Cair na água continua levando o jogador ao ponto inicial; o recanto não deve provocar recuperação indevida.
- O recorte aéreo não mostra o outro lado do monte; a disposição da casinha ali é uma interpretação proposta para avaliação.

## Requisitos

### Funcionais

- **FR-001:** reduzir e reorganizar a massa da ilha em relação à versão 003, preservando espaço suficiente para caminhar e reconhecer o povoado.
- **FR-002:** aproximar o relevo da referência: costa rochosa irregular, enseada junto ao cais, áreas escalonadas para o povoado e um monte grande no fundo. Não exigir reprodução pixel a pixel.
- **FR-003:** manter cais, barco, largo, três setores de moradias, hortas e ruína reconhecíveis; ajustar seus posicionamentos e caminhos quando necessário para o novo relevo.
- **FR-004:** manter as moradias do povoado como blocos provisórios; apresentar no recanto uma casinha isolada em forma simples, sem interior detalhado.
- **FR-005:** disponibilizar um trajeto contínuo e identificável do cais ao povoado e ao recanto atrás do monte, com retorno possível; aproximar os caminhos das hortas e da ruína.
- **FR-006:** preservar a caminhada em terceira pessoa, a câmera, as colisões e a recuperação após queda na água.
- **FR-007:** disponibilizar a maquete editável e a cena jogável para avaliação, com vistas aéreas que permitam comparar a silhueta com a referência.
- **FR-008:** tornar toda a superfície terrestre da ilha caminhável e acessível a partir do cais, incluindo monte, topo, encostas, costa e recanto, sem saltos, escalada ou teletransporte; evitar inclinações e colisões que bloqueiem o jogador. A água e as áreas ocupadas pelos volumes das construções não fazem parte da superfície caminhável.
- **FR-009:** substituir o visual do personagem somente na cena 004 por um modelo da pasta `Low Poly World/downloads/male`, conforme pedido durante a implementação, preservando o controlador, as referências da sessão e o personagem das cenas anteriores.

### Elementos principais

Ilha e costa; monte; cais e enseada; áreas do povoado; rotas; hortas e ruína; casinha e recanto isolado.

## Critérios de sucesso

- Uma vista aérea mostra uma ilha mais compacta que a 003, com monte dominante, enseada e áreas escalonadas identificáveis.
- Um percurso sem teletransporte liga cais, largo, três setores, hortas, ruína e recanto, permitindo voltar ao cais.
- A avaliação percorre o topo, as encostas e os limites terrestres da ilha em diferentes direções, confirmando acesso por caminhada e retorno; não há áreas de terreno isoladas por obstáculos ou inclinações intransponíveis.
- Os 12 blocos provisórios do povoado continuam identificáveis, assim como a casinha isolada atrás do monte.
- O jogador retorna ao ponto inicial ao cair na água; não há recuperação indevida nas áreas acessíveis.
- A pessoa usuária consegue comparar a maquete com a referência e experimentar a cena antes de aprovar a direção visual.

## Limites e premissas

- A referência principal é `Referencias/modelagem-vista-aerea.png`; o objetivo desta iteração é aproximar a forma e o relevo, não finalizar a arte da ilha.
- A casinha isolada é um volume simples com aparência de casa. Não inclui morador, diálogos, missões ou interior; a história permanece uma ideia para entrega futura.
- As casas do povoado continuam blocos; paisagismo e construções detalhadas não fazem parte desta iteração.
- O tamanho exato será ajustado durante o planejamento para acomodar o trajeto e a nova silhueta, sem fixar medidas a partir de uma imagem sem escala.

## Encerramento solicitado

O usuário pediu encerrar esta spec sem novo retrabalho do relevo. Na próxima spec, o terreno deve reproduzir fielmente a referência e permitir caminhar nas superfícies com inclinação menor que 45°. A exigência anterior de toda a ilha caminhável será substituída nessa próxima entrega.
