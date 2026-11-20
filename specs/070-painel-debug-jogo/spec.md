# Especificação 070 — Painel de debug em jogo

**Branch** `070-painel-debug-jogo`  
**Data** 2026-11-20  
**Status** Implementada e validada em Play Mode

## Cenários de uso

- Durante a partida, abrir o painel por um atalho, escolher a hora do dia e conferir na hora a mudança de iluminação. Manter a hora fixa para inspecionar a cena ou retomar o ciclo automático.
- Alternar a visualização dos colisores sobre a imagem do jogo, fechar o painel e continuar andando com a sobreposição ligada para comparar a física com o cenário.

## Requisitos

- **FR-001** — Abrir e fechar o painel durante a partida. Ao fechar, devolver os controles ao jogador.
- **FR-002** — Exibir a hora atual, permitir escolher qualquer hora de um ciclo de 24 horas e alternar entre hora fixa e ciclo automático. Céu, luz do sol e iluminação noturna devem acompanhar a hora selecionada.
- **FR-003** — Alternar uma sobreposição dos colisores ativos do cenário e do personagem na câmera de jogo. Ela deve representar caixas, cápsulas, esferas, malhas e terreno quando presentes e permitir observar o alinhamento entre arte e física.
- **FR-004** — Começar com o painel fechado e a sobreposição desligada. A ferramenta é voltada ao Play Mode e a builds de desenvolvimento, com alterações limitadas à sessão.

## Critérios de aceitação

- Selecionar meio-dia e meia-noite no mesmo Play Mode e verificar a mudança no céu e nas luzes. Fixar a hora e confirmar que volta a avançar ao retomar o ciclo.
- Ligar a sobreposição no píer e perto de uma escada. Conferir o contorno do piso e a cápsula do personagem, caminhar com a sobreposição ativa e ocultá-la novamente.
- Reabrir e fechar o painel sem reiniciar a cena ou perder o controle da câmera e do personagem.

## Premissas

Atalho proposto F3. A câmera de jogo é a referência para a sobreposição. A ferramenta reutiliza o ciclo da cena principal.
