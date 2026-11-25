# Plano das luzes noturnas

## Contexto técnico

Unity 6000.3.25f1, Built-in. A cena principal e os assets do estudo 033 são a base da branch 035. A câmera existente mantém a saída de 480 × 270. O relógio do estudo passa a expor a hora para consumidores como as luzes da vila.

## Design e constituição

Aplicar brilho em superfícies pequenas e usar luzes locais em tempo real com alcance limitado. Cada ponto terá horário ligeiramente deslocado dentro do intervalo aprovado. O controlador recebe referências da cena e calcula a intensidade diretamente a partir da hora. Salvar pela API do editor e verificar dia, entardecer, meia-noite e amanhecer na câmera do jogo. O efeito segue o relógio visual e não participa da investigação.
