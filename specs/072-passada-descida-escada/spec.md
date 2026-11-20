# Passada na descida de escadas

**Branch** `072-passada-descida-escada`
**Data** 2026-11-20
**Status** Implementada e validada em Play Mode

## Cenários e testes

- Descer uma escada do patamar até o chão. Os pés alternam apoios nos degraus e o corpo acompanha o ritmo dos passos.
- Soltar o direcional no meio do lance e retomar. O personagem mantém o apoio na pausa e continua do ponto onde parou.
- Repetir na escada L3, da fachada, e na B3, que tem um lance mais curto.

## Requisitos

- **RF-01** A descida deve ter cadência controlada e contatos reconhecíveis nos degraus.
- **RF-02** Um pé apoiado deve permanecer no degrau durante a transferência de peso para o próximo passo. A entrada e a saída do lance devem ter transição contínua.
- **RF-03** Preservar a passada de subida e o bloqueio lateral nos degraus já avaliados na entrega 071.

## Critérios de sucesso

- Registrar uma comparação visual do antes e depois na L3, pela câmera do jogo.
- Em três descidas por lance, observar passos alternados, sem deslizamento visível do pé de apoio ou sequência de degraus atravessada sem contatos reconhecíveis.
- Em cada lance, pausar por dois segundos no meio, retomar até o chão e confirmar que a subida continua disponível.

## Premissa

A primeira entrega foca na descida a pé nos lances existentes. A implementação deve partir da versão que contém o controle de escadas da entrega 071.
