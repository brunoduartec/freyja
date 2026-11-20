# Especificação · Waypoints pela ilha

**Branch** `068-waypoints-ilha`  
**Data** 2026-11-19  
**Estado** Proposta para aprovação

## Cenários e testes

### P1 · Circular entre as regiões da ilha

Um personagem não jogável pode sair de sua região e caminhar até um destino em outra parte do mapa, escolhendo o percurso conforme o destino.

**Teste independente** — Em Play Mode, direcionar um personagem do píer ao largo, do largo à área alta e da área alta ao píer por uma rota disponível. Testar também um destino em cada ramal acessível.

1. Dado um destino alcançável, o personagem segue a pé pelos caminhos e chega ao ponto escolhido.
2. Dado um novo destino durante o percurso, o personagem consegue mudar de rota a partir da posição atual.
3. Dado um destino inacessível, o personagem permanece em posição segura.

### Casos de borda

- Escadas, desníveis, trechos estreitos e bordas do píer devem respeitar os limites da caminhada.
- Portas fechadas e obstáculos devem impedir a travessia quando bloquearem o caminho.

## Requisitos

- **FR-001** — Na cena principal `Ilha_Tripo_Caminhada_011`, distribuir waypoints pelas áreas caminháveis existentes, incluindo as vias principais, os ramais, os acessos a edificações e os interiores atualmente acessíveis.
- **FR-002** — Conectar os waypoints por percursos que os personagens possam fazer a pé. Se houver uma área caminhável isolada, cobrir sua circulação local e registrar a limitação.
- **FR-003** — Permitir que personagens não jogáveis compartilhem a rede e recebam destinos variados, inclusive durante a caminhada.
- **FR-004** — Tratar destinos inacessíveis sem atravessar obstáculos, cair em áreas inseguras ou teleportar o personagem.

### Entidades

- **Waypoint** — ponto em uma superfície caminhável, usado como destino ou passagem.
- **Percurso** — sequência de ligações transitáveis até o destino escolhido.

## Critérios de sucesso

- **CS-001** — O inventário de cobertura relaciona todas as zonas caminháveis da cena a pelo menos um destino demonstrado em Play Mode. Nas zonas ligadas à rede principal, cada destino é alcançável a partir do largo.
- **CS-002** — Pelo menos dois personagens não jogáveis demonstram o uso da mesma rede, incluindo uma troca de destino em movimento e uma travessia por escadas.
- **CS-003** — Um teste com acesso bloqueado confirma que o personagem para em segurança. Nos percursos demonstrados, nenhum personagem atravessa obstáculos, cai ou se teleporta.

## Premissas

- “Mapa inteiro” corresponde às áreas caminháveis da cena atual. O GDD orienta a conferência das regiões e a cena define quais acessos existem hoje.
- “Andar livremente” corresponde a aceitar destinos variados na rede. Rotinas diárias e horários por morador ficam fora desta entrega.
