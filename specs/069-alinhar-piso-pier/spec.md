# Especificação 069 — Contato dos pés com o píer

**Branch** `069-alinhar-piso-pier`  
**Data** 2026-11-20  
**Status** Implementada e validada em Play Mode

## Cenário de uso e testes

O jogador caminha pelo convés e para ao lado do guindaste, no ponto indicado na captura. As botas parecem apoiadas nas tábuas. Ao continuar até o cais ou voltar, o apoio permanece contínuo.

## Requisitos

- **FR-001** — Alinhar o apoio físico à altura visível das tábuas nas zonas transitáveis do píer, incluindo o ponto da captura.
- **FR-002** — Conferir o contato visual das botas quando o personagem está parado e durante a fase de apoio da caminhada. Ajustar a relação entre personagem e piso se a altura física correta ainda deixar uma folga visível.
- **FR-003** — Preservar as travessias oeste e leste e a recuperação ao sair pela borda livre.

## Critérios de sucesso

- **SC-001** — Medir o apoio no ponto da captura e em três pontos do trajeto. A diferença entre o piso visível e o apoio, longe das bordas, fica em até 3 cm.
- **SC-002** — Registrar uma pausa e uma caminhada no ponto da captura em Play Mode, com as botas legíveis junto às tábuas, sem flutuação aparente.
- **SC-003** — Completar uma ida e volta por cada lateral e confirmar o retorno ao ponto seguro após a saída pela borda.

## Evidência e escopo

Cena `Ilha_Tripo_Caminhada_011`. Ao lado do guindaste, próximo de X −36,26 e Z −55,20, as tábuas medem Y 1,729 m e o colisor mede Y 1,800 m nos dois pés. A diferença medida é de 7,1 cm. O escopo desta revisão é o contato com o piso do píer.
