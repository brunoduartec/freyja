# Especificação de corrida e salto

**Branch** `063-corrida-salto`  
**Data** 19/11/2026  
**Status** Aprovada e implementada  
**Pedido** “Quero que o personagem possa correr e pular”

## Cenários e testes

### Correr pela ilha · P1

Em solo firme, segurar Shift esquerdo junto com WASD faz o protagonista correr na direção do movimento. Soltar Shift retoma a caminhada. Soltar WASD leva ao repouso, mesmo com Shift pressionado.

### Saltar parado, caminhando ou correndo · P1

Pressionar Espaço em solo firme faz o protagonista saltar. Durante o salto, é possível direcionar o movimento com WASD. Ao pousar, o personagem retoma a ação correspondente às teclas pressionadas.

### Casos de borda

- Pressionar Espaço no ar ou mantê-lo pressionado após o pouso não dispara outro salto. Um novo toque em solo firme permite saltar de novo.
- Obstáculos sólidos bloqueiam o movimento durante a corrida e o salto. Cair fora dos limites aciona a recuperação existente e zera a velocidade.
- Com o controle de exploração suspenso, por exemplo ao liberar o cursor ou durante um diálogo, Shift e Espaço não iniciam ações.

## Requisitos

- **FR-001** — A corrida deve ser mais rápida que a caminhada e seguir a direção relativa à câmera, sem ganho de velocidade na diagonal.
- **FR-002** — O salto deve funcionar em repouso, caminhada e corrida, com direção ajustável no ar e retomada da locomoção ao pousar.
- **FR-003** — A câmera do jogo deve permitir distinguir repouso, caminhada, corrida e salto pelas animações do protagonista.
- **FR-004** — As instruções de controle da cena devem indicar Shift esquerdo e Espaço. Os controles existentes, as colisões, a recuperação de queda e o bloqueio de movimento durante diálogos devem continuar funcionando.

## Critérios de sucesso

- **SC-001** — Em três percursos de ida no mesmo trecho plano, correr cobre mais distância que caminhar no mesmo intervalo. Em cada percurso, soltar Shift retoma a caminhada e soltar WASD leva ao repouso.
- **SC-002** — Na câmera do jogo, observar três saltos completos com pouso, um partindo de cada estado inicial. Em cada um, confirmar que pressionar Espaço no ar não inicia outro salto e que o próximo toque após pousar volta a funcionar.
- **SC-003** — Na cena principal, verificar subida da escada do píer a pé, bloqueio por um obstáculo sólido em corrida e salto, recuperação após uma queda e bloqueio dos comandos de corrida e salto com o controle de exploração suspenso.

## Premissas

- A entrega usa a cena principal da ilha. Shift esquerdo e Espaço são os comandos propostos para teclado.
- Velocidade de corrida, altura do salto e resposta direcional no ar serão ajustadas em teste com a câmera do jogo.
