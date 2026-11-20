# Especificação 055 — Escadas transitáveis na ilha

**Branch** `055-revisar-escadas-ilha`  
**Data** 2026-11-19  
**Status** Implementada e validada em Play Mode

## Cenários de uso e testes

### Alcançar o andar superior da casa (P1)

O jogador caminha do pé da escada até o piso ou patamar existente do andar superior. Ao chegar, consegue parar, mudar de direção e descer pela mesma escada, usando os controles normais.

### Percorrer as demais escadas da ilha (P2)

O jogador sobe e desce todas as escadas visíveis com acesso a partir da área jogável, incluindo a da casa B3. A passada acompanha a altura dos degraus durante a subida e volta ao normal no piso plano.

### Casos de borda

O jogador consegue parar e retomar o movimento no meio de um lance. Nas laterais dos degraus, paredes e guardas visíveis continuam bloqueando a passagem, e bordas livres permitem queda.

## Requisitos

- **FR-001** — Inventariar as escadas da cena principal por localização e destino, com evidência visual suficiente para identificar cada uma.
- **FR-002** — Permitir subida e descida contínuas entre a base e o destino visível de cada escada inventariada, sem travar, atravessar degraus ou exigir salto.
- **FR-003** — Garantir apoio no topo da escada do andar superior para parar, virar e iniciar a descida.
- **FR-004** — Aplicar a adaptação da passada nas escadas e preservar a caminhada normal fora delas.

## Critérios de sucesso

- **SC-001** — Registrar um ciclo completo de subida e descida por escada inventariada em Play Mode, incluindo a B3.
- **SC-002** — Na escada do andar superior, registrar uma pausa no meio do lance e uma parada seguida de mudança de direção no topo, sem queda ou reposicionamento.
- **SC-003** — Conferir na câmera de jogo a passada e a chegada ao piso plano da casa do andar superior e dos demais tipos de escada encontrados.

## Premissas

- A referência é a cena principal `Ilha_Tripo_Caminhada_011`. O inventário cobre os lances visíveis alcançáveis na área jogável.
- O destino da escada da casa é o piso ou patamar superior já visível. Criar interiores e abrir portas ficam fora desta entrega.
- A spec 054 fornece a referência de comportamento da B3 e de adaptação da passada.
