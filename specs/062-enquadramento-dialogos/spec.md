# Enquadramento dos personagens em diálogo

**Branch** `062-enquadramento-dialogos`  
**Status** Aprovada em 2026-11-19  
**Criada em** 2026-11-19

## Cenário de uso e aceitação

Ao iniciar uma conversa na ilha, a câmera aproxima suavemente o quadro e mostra, ao mesmo tempo, o protagonista à esquerda e o morador à direita. O enquadramento mostra os dois rostos em três quartos e parte do tronco. O painel de perguntas fica abaixo dos rostos. Ao encerrar ou cancelar, a câmera e os controles de exploração voltam.

- Ao iniciar uma conversa com Nuno no cais, o rosto dele e o do protagonista ficam visíveis mesmo se a conversa começar com a câmera atrás do jogador.
- Se uma parede, um desnível ou outro objeto bloquear a vista, a conversa usa um enquadramento alternativo que mantenha os dois rostos legíveis.
- Enquanto o jogador escolhe assuntos ou lê respostas, os dois continuam em quadro.

## Requisitos

- **FR-01** — Mostrar os dois personagens 3D na própria cena, cada um em um lado da tela, com os rostos visíveis.
- **FR-02** — Posicionar o texto e as escolhas sem sobrepor os rostos.
- **FR-03** — Retomar a câmera e os controles de exploração ao encerrar ou cancelar o diálogo.
- **FR-04** — Aplicar o enquadramento às conversas do piloto com Nuno, Luana e Ivo.

## Critérios de sucesso

- Em Play Mode, validar as três conversas a partir de pelo menos dois ângulos de aproximação por morador. Em cada caso, os dois rostos aparecem em lados opostos, sem cortes, sobreposição do painel ou oclusão pelo cenário.
- Conferir o retorno à exploração ao encerrar pelo painel e pelo Esc.
- Entregar uma captura de conversa no cais para a avaliação visual.

## Premissas

Plano médio curto com fundo da ilha, protagonista à esquerda e morador à direita. Primeira revisão visual com Nuno no cais. Recorte aprovado pelo usuário.
