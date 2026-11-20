# Spec 011 — Caminhar no cenário do TripoAI

**Branch** `011-caminhada-modelos-tripo`  
**Criada em** 13/11/2026  
**Estado** Cena implementada e validada em Play Mode com entradas automatizadas. Avaliação manual pendente.

## Cenário e teste

### Controlar o protagonista na ilha (P1)

Quero caminhar com o protagonista no cenário que gerei no TripoAI para avaliar a escala, a animação e a câmera de Freyja.

**Teste independente.** Abrir a cena de avaliação, caminhar do ponto inicial até um marco visível e voltar. Durante o percurso, mudar a direção, girar a câmera e parar.

1. Ao iniciar a cena, o protagonista aparece em solo firme, de corpo inteiro e de frente para a direção inicial do percurso.
2. Ao usar WASD, o protagonista caminha em relação à câmera, vira na direção do movimento e anima os passos. Ao soltar as teclas, para e volta à pose de repouso.
3. Ao mover o mouse, consigo olhar ao redor em terceira pessoa. Ao me aproximar de uma encosta, a câmera evita atravessar o relevo e recupera a visão do personagem ao sair da área obstruída.
4. Ao forçar uma passagem fora do trecho acessível, obstáculos sólidos bloqueiam o personagem. Se ele cair para fora do terreno, retorna automaticamente ao ponto inicial, com a câmera reposicionada.

**Casos de borda.** Caminhar na diagonal mantém a velocidade das demais direções. Esc ou a perda de foco interrompem o controle. Um clique na visão do jogo retoma o controle.

## Requisitos

- **FR-001** — Montar uma cena de avaliação no Unity com o protagonista de `TripoAi/principal/` e o cenário texturizado `TripoAi/cenario/stylized island terrain 3d model.glb`. Preservar os arquivos de origem e as cenas de avaliação existentes.
- **FR-002** — Oferecer um percurso curto de ida e volta sobre o terreno do modelo, entre um ponto inicial seguro e um marco visível. O trecho deve ter chão contínuo e pelo menos uma mudança de direção.
- **FR-003** — Usar o [contrato de caminhada do cais](../002-caminhada-no-cais/contracts/player-controls.md) como referência para WASD, mouse, Esc e retomada por clique. Exibir esses comandos na cena.
- **FR-004** — Mostrar a transição entre repouso e caminhada conforme o personagem para ou se desloca. Os pés devem acompanhar o deslocamento sem deslizamento evidente no trecho de avaliação.
- **FR-005** — Impedir a passagem pelo solo e por obstáculos sólidos, tratar a obstrução da câmera e recuperar o personagem após uma queda.
- **FR-006** — Manter o visual low poly com pixels visíveis nas silhuetas e permitir distinguir a mochila e o movimento das pernas pela câmera de jogo, conforme a [direção visual do GDD](../../GDD.md).

## Critérios de sucesso

- **SC-001** — Em Play Mode, completar duas idas e voltas consecutivas entre o início e o marco, usando os comandos exibidos, sem atravessar o terreno ou ficar preso.
- **SC-002** — Em uma sequência com três partidas e três paradas, observar a caminhada durante o deslocamento e a pose de repouso em cada parada. Durante a caminhada, distinguir a mochila e o movimento das pernas.
- **SC-003** — Testar uma aproximação a uma obstrução da câmera e uma queda de propósito. A câmera recupera a visão ao sair da obstrução, e o personagem volta ao ponto inicial após a queda.
- **SC-004** — Entregar a cena jogável e uma gravação curta mostrando o percurso, com o caminho da cena e o resultado da validação registrados para avaliação.

## Premissas e limites

- O modelo proposto para o protagonista é `TripoAi/principal/stylized hiker 3d model.glb`. O arquivo tem esqueleto e texturas embutidas, mas nenhum clipe de animação. A entrega inclui preparar as animações básicas de repouso e caminhada. A escolha entre os arquivos disponíveis depende da importação e da conferência visual da jaqueta azul e da mochila descritas no GDD.
- O cenário texturizado é a proposta para esta iteração. O ponto de partida e o marco serão escolhidos conforme a escala e a topografia do modelo. A cena serve para avaliar um trecho, com teclado e mouse, em terceira pessoa. O GLB do cenário está em `TripoAi/cenario/` e ainda está fora do Git.
- Exploração da ilha inteira, corrida, salto, moradores, diálogos, pistas e acabamento visual definitivo ficam para outras entregas.

## Revisão de escala pelos degraus

A pedido do usuário, o cenário passou a usar escala uniforme calibrada pelos degraus originais. Medidas finais de 57,600 × 48,396 × 9,857 m, protagonista de 1,80 m e desníveis típicos de aproximadamente 0,18 m. O fator é 0,36 em relação à primeira integração. O modelo original foi restaurado e as escadas adicionais foram removidas.

A referência para reutilização está em [Source/Blender/Ilha_Tripo_011/README.md](../../Source/Blender/Ilha_Tripo_011/README.md). Esta revisão substitui as medidas anteriores de escala.
