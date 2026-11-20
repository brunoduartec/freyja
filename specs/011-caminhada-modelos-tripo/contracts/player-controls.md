# Contrato de controles — Spec 011

A nova cena segue os comandos do [cais](../../002-caminhada-no-cais/contracts/player-controls.md).

| Entrada ou evento | Resposta visível |
| --- | --- |
| Abrir a cena em Play Mode | Protagonista em repouso, sobre o solo e enquadrado de corpo inteiro. Os controles aparecem na tela. |
| Clique na Game View com foco | Captura o cursor e habilita caminhada e câmera. |
| WASD | Caminha em relação à câmera, vira na direção do deslocamento e anima os passos. A velocidade diagonal equivale à das demais direções em solo plano. |
| Soltar WASD ou pressionar contra um obstáculo sem avançar | A caminhada dá lugar à pose de repouso. |
| Mouse | Orbita em terceira pessoa. A câmera se aproxima ao encontrar um obstáculo e recupera a distância ao sair da obstrução. |
| Esc ou perda de foco | Libera o cursor e interrompe os comandos. Retomar com um clique. |
| Queda fora do trecho | Retorna ao ponto inicial em repouso, com a câmera reposicionada. |
