# Especificação da iluminação das 12 casas

**Branch** — `038-luzes-todas-casas`  
**Data** — 2026-11-18  
**Estado** — Aprovada para implementação

## Cenários e testes

### P1 — Ver todas as casas acenderem

Ao atravessar a vila do entardecer à noite, o jogador vê pelo menos uma janela ou abertura iluminada em cada uma das 12 casas. A luz âmbar alcança a superfície próxima de cada fonte. A cobertura inclui as casas baixas (B1–B4), as altas (A1–A4) e as do leste (L1–L4), inclusive a casa isolada L4.

### P2 — Reconhecer as casas à noite

A partir dos acessos às residências, o jogador distingue a abertura acesa e o trecho de parede ou piso que ela ilumina. A combinação de luzes localizadas e ambiente azul mantém o contraste entre as fachadas e os espaços entre elas.

### Casos de borda

- A meia-noite mantém todas as fontes no estado noturno.
- Um salto de horário atualiza as 12 casas conforme a fase selecionada.
- A volta seguinte reproduz a mesma sequência de acendimento.

## Requisitos funcionais

- **RF-001** — Cobrir as 12 residências da cena principal `Ilha_Tripo_Caminhada_011`, com ao menos uma abertura visivelmente acesa e iluminação sobre uma superfície próxima em cada casa.
- **RF-002** — Reutilizar o ciclo visual da entrega 035. As luzes acendem em sequência entre 17h30 e 18h30, ficam acesas até 5h30 e apagam até 6h30.
- **RF-003** — Aproveitar as fontes já instaladas nas casas, sem duplicação. A venda e os dois lampiões existentes continuam sincronizados com o mesmo ciclo.
- **RF-004** — Ajustar o alcance das fontes a cada fachada para manter uma área iluminada legível na câmera de jogo e preservar áreas de sombra.

## Critérios de sucesso

- **CS-001** — Uma lista de verificação registra 12 de 12 casas, incluindo L4, com evidência diurna e noturna para cada residência.
- **CS-002** — Uma captura ou vídeo por área mostra a luz nas aberturas e em superfícies próximas, com o aspecto pixelado da cena.
- **CS-003** — Em Play Mode, verificar as 12 casas às 12h, 18h30, 0h e 6h30 e conferir a progressão em pelo menos uma casa por área no entardecer e no amanhecer.

## Premissas

- A cobertura residencial usa os identificadores do inventário da ilha. A casa isolada faz parte das 12 e a venda é um ponto comercial independente.
- O acendimento segue o horário visual da cena. O vínculo com rotinas ou ocupação das casas fica para outra etapa.
