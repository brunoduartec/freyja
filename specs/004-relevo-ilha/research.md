# Pesquisa e decisões — Spec 004

## Forma e escala

**Decisão:** usar a imagem aérea como referência de composição, reduzindo largura e profundidade frente à 003, com enseada frontal, povoado escalonado e monte dominante ao fundo. **Motivo:** reflete o pedido sem inventar uma escala exata inexistente na imagem. **Alternativa:** reduzir tudo por um único fator; rejeitada porque poderia desalinhá-la do cais e comprometer as rotas.

## Caminhabilidade integral

**Decisão:** superfície terrestre contínua, conectada e com todos os triângulos de colisão até 35°, preferindo 30°. Validar a geometria importada, as transições e o deslocamento do personagem. **Motivo:** há margem em relação ao limite existente de 60° do controlador, sem alterar os controles. **Alternativa:** paredões decorativos terrestres ou um único caminho de subida; rejeitados por deixarem parte do terreno inacessível. A água e o interior ocupado por construções ficam fora da superfície caminhável conforme a spec.

## Encaixe e preservação

**Decisão:** nova fonte e nova cena, preservando o cais e os controles e ajustando apenas a instância 004. **Motivo:** permite comparar e retornar à 003. **Alternativa:** editar o prefab e a cena anteriores diretamente; rejeitada por afetar entregas existentes.

## Outro lado do monte

**Decisão:** casinha provisória simples, sem morador ou interior, situada atrás do monte e não visível do largo. **Motivo:** atende à proposta narrativa sem ampliar o escopo. **Alternativa:** implantar história, NPC ou arquitetura final; fora da iteração.

## Validação

**Decisão:** combinar análise das faces, teste de conectividade e percursos físicos com o controlador, além de comparar vistas de cima e da câmera de jogo. **Motivo:** uma rota demonstrada não comprova a acessibilidade de toda a ilha. **Alternativa:** testar apenas a trilha principal; insuficiente. Documentar resolução da amostragem, tolerâncias e limites de prova.
