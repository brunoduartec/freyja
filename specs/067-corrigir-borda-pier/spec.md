# Especificação 067 — Passagem oeste do píer

**Branch** `067-corrigir-borda-pier`  
**Data** 2026-11-19  
**Status** Implementada e validada em Play Mode

## Cenário de uso e teste

Ao caminhar do píer rumo à ilha pela lateral oeste do galpão, o jogador se mantém apoiado nas tábuas visíveis até chegar ao cais de pedra. Também consegue fazer o caminho inverso. Ao ultrapassar deliberadamente a borda externa, cai e volta ao ponto seguro.

## Requisitos

- **FR-001** — Sustentar a caminhada ao longo da passagem visível pelo lado oeste do galpão, incluindo a junção com o cais de pedra.
- **FR-002** — Manter a colisão alinhada ao piso visível e a queda funcional além da borda livre.
- **FR-003** — Preservar a travessia pelo lado leste validada na entrega 054.

## Critérios de sucesso

- **SC-001** — Completar uma ida e volta em Play Mode pela passagem oeste, incluindo o ponto da captura e uma pausa na junção, sem cair ou reiniciar.
- **SC-002** — Completar uma travessia pelo lado leste e recuperar o personagem após uma saída deliberada pela borda oeste.

## Evidência e escopo

A medição no Unity localizou o protagonista próximo de X −36,36 e Z −55,77. Há piso visível a oeste do limite do colisor, em vários pontos entre Z −56 e Z −51,25. A referência é a cena principal `Ilha_Tripo_Caminhada_011`. A entrega cobre a colisão dessa passagem.
