# Especificação 054 — Física da escada e do píer

**Branch** `054-fisica-escada-pier`  
**Data** 2026-11-19  
**Status** Aprovada

## Cenários de uso e testes

### Caminhar da ilha até o píer (P1)

Ao seguir pelo acesso da ilha, o jogador deve chegar ao piso de madeira sem cair. Para validar, caminhar da ilha até o píer e retornar pelos controles normais. Repetir pelo centro e em diagonal, mantendo o personagem sobre a passagem visível.

### Subir a escada externa da casa (P2)

Ao caminhar do pé da escada, o jogador deve alcançar o patamar superior sem ficar preso nos degraus e com uma passada que acompanhe a subida. Para validar, subir até o topo, parar, retomar a caminhada e descer até o solo.

### Casos de borda

Uma pausa na junção do píer deve manter o personagem apoiado. Ao sair deliberadamente pela borda livre do píer, a queda e a recuperação existentes devem continuar funcionando.

## Requisitos

- **FR-001** — Permitir subida e descida pela escada externa da casa usando os controles atuais, sem atravessar os degraus.
- **FR-002** — Assegurar apoio contínuo entre o acesso da ilha e o piso do píer nos dois sentidos.
- **FR-003** — Preservar os limites físicos da casa e o comportamento de queda nas bordas livres do píer.
- **FR-004** — Adaptar a passada do protagonista ao subir os degraus e retomar a caminhada normal ao chegar ao patamar.

## Critérios de sucesso

- **SC-001** — Completar um ciclo de subida, parada no topo e descida sem bloqueio, queda ou reposicionamento.
- **SC-002** — Completar dois ciclos de ida e volta pela junção do píer, um pelo centro e outro em diagonal, sem queda, incluindo uma pausa sobre a junção.
- **SC-003** — Confirmar uma queda deliberada pela borda livre do píer com a recuperação já prevista no jogo.
- **SC-004** — Na câmera do jogo, confirmar durante uma subida completa que os pés acompanham a altura dos degraus sem atravessamento visível e que a passada volta ao normal no patamar.

## Premissas

- A referência é a cena principal `Ilha_Tripo_Caminhada_011`.
- A escada referida é a externa da casa. O percurso desta entrega vai do solo ao patamar superior.
- A entrega abrange a escada e a ligação da ilha com o píer, preservando a aparência atual.

## Pesquisa de animação

- O pacote instalado [Human Basic Motions FREE](https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271) tem caminhada em oito direções e não inclui clipe dedicado a degraus.
- A [lista oficial da amostra gratuita da VanillaLoopStudio](https://docs.google.com/spreadsheets/d/1lLdKOE2V5-o6Zh3qnVMmuzn79eRrcRQeWnsq3Djdjo4/edit?rm=minimal) confirma um ciclo de caminhada para subida. A entrega usa adaptação da passada existente às alturas dos degraus. O histórico da tentativa de acesso à amostra está na [pesquisa](research.md).
