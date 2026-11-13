# Visibilidade do recanto 004

Teste estático com Blender 5.2.2 LTS/MCP CLI sobre `Ilha_Relevo_004.blend`. O raio partiu do largo em `(0, 68, 6,480)`, equivalente ao ponto de observação a 1,7 m acima do solo amostrado por BVH. O destino foram seis pontos representativos nas paredes superiores e no telhado da `Casa_Recanto_004` / `Casa_Recanto_Telhado_004`.

- Raios bloqueados pela malha de terreno: **6/6**.
- Primeira interseção: 19,25–20,22 m do observador; altura Z da interseção: 9,37–9,83 m.
- Distância até os pontos-alvo: 89,73–96,13 m.
- Resultado: o monte intercepta a linha de visão do largo para a casinha em todos os pontos amostrados.

Este é um teste geométrico de linha de visão a partir do largo; não substitui validação da câmera real do jogador após importação no Unity.
