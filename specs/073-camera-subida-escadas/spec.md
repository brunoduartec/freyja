# Enquadramento na casa com escadas

**Branch** `073-camera-subida-escadas`
**Data** 2026-11-20
**Status** Concluída e aceita em 2026-11-20

## Cenários e testes

Subir o lance L3 da casa com escadas, pelo centro e próximo de cada lateral. O personagem deve permanecer visível quando a câmera passar perto da fachada e do corrimão. Ao chegar ao patamar, a câmera deve acompanhar a saída suavemente.

## Requisitos

- **RF-01** Manter o personagem enquadrado ao subir o lance e atravessar o patamar.
- **RF-02** Havendo obstáculos atrás do personagem, a câmera deve manter uma vista utilizável, sem atravessar a geometria ou avançar até o corpo.
- **RF-03** Preservar o controle de câmera pelo jogador durante a subida e o enquadramento normal ao voltar à exploração.

## Critérios de sucesso

- Completar três subidas na L3, cobrindo o centro e as duas laterais, com o tronco visível durante o lance.
- Repetir uma subida com ajuste manual de câmera e confirmar que a saída no patamar não produz um salto brusco de enquadramento.
- Registrar uma comparação visual da subida pela câmera do jogo e conferir uma passagem no lance B3 como regressão.

## Premissas

A L3 foi a referência usada na reprodução. Em três subidas, a distância da câmera ao pivô caiu a zero. A geometria exata responsável ainda precisa ser isolada. A revisão trata do enquadramento da câmera.
