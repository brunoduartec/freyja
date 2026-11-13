# Análise de caminhabilidade — T013

**Resultado inicial (12/11):** a varredura geométrica dos triângulos não encontrou inclinações acima de 35°. A grade inicial encontrou dois componentes. Após a desativação da trilha de cais, a reanálise em 13/11 encontrou um único componente na grade de 0,5 m; ainda não é prova matemática nem substitui percurso físico em Play Mode.

## Execução

- Unity 6000.3.25f1, cena ativa `Assets/_Project/Scenes/Ilha_Relevo_004.unity`.
- Data UTC: 2026-11-12; execução final ~1,3 s, sem alteração/salvamento da cena.
- Configuração: [walkability-config.json](walkability-config.json); analisador: `Tools/Unity/AnalyzeIlhaRelevoWalkability.cs`.
- Collider analisado: `IlhaRelevo/Terrain_004_Walkable_Island_Collider`, `MeshCollider` não convexo, malha `Terrain_004_Walkable_Island`, 6.541 vértices e 7.290 triângulos. Escala mundial `[1, 0.9999998, 0.9999998]`; determinante `0.9999996`, finito e não singular.
- Bounds mundiais: mínimo `[-87,5; 0,993; -180]`, máximo `[87,5; 32,700; -4]`, dimensão `175 × 31,707 × 176 m`.
- `CharacterController` lido na cena: raio `0,30 m`, altura `1,80 m`, `stepOffset=0,25 m`, `slopeLimit=60°`, `skinWidth=0,04 m`, centro `[0; 0,9; 0]`. A análise usou cápsula de consulta com raio `0,285 m` (margem configurada de `0,015 m`), borda de raio `0,29 m`, 8 probes e tolerância residual de degrau `0,25 m`.

## Malha e grade

- Triângulos examinados: 7.290; degenerados: 0; transformações inválidas: 0.
- Inclinação dos planos: mínimo `0°`, máximo `32,696°`; 6.385 triângulos em `≤30°`, 905 em `>30–35°`, 0 acima de `35°`. Violações do limite de 35°: **0**.
- Grade horizontal de `0,5 m`: `351 × 353 = 123.903` células. Resultado: 87.693 células com suporte, margem do raio e volume livre; 32.090 sem suporte dentro do retângulo dos bounds; 1.515 rejeitadas por falta de suporte no perímetro ou descontinuidade; 2.605 obstruídas por outros colliders.
- As obstruções incluem os footprints dos 12 blocos (`B1–B4`, `A1–A4`, `L1–L4`, 195 células por bloco), a casa/telhado do recanto e colliders do cais/caminho. São espaços ocupados que não devem ser atravessados; os números não significam por si só que toda a superfície terrestre falhou.
- O capsule overlap e o capsule cast excluem o `MeshCollider` de terreno selecionado para não confundir o piso de suporte com barreira. A colisão do terreno é estimada por raios, amostras perimetrais, limite de inclinação e desnível residual. Outros colliders continuam bloqueando as células/arestas.

## Componentes e pontos isolados

1. **Componente principal:** 87.691 células, área de grade aproximada `21.922,75 m²`, bounds XZ `[-87; -179,5]` a `[87; -4,5]`. Abrange largo, setores, horta/ruína, subida oeste, topo, encosta leste, retorno e uma amostra caminhável a cerca de `3,0 m` do centro do waypoint do recanto.
2. **Componente secundário junto ao cais:** duas células válidas, área aproximada `0,5 m²`, centradas em `(-2; 1,0; -5)` e `(-2; 1,0; -4,5)` Unity. As vizinhas para oeste/norte falham as amostras de suporte do perímetro; acessos para leste/sul intersectam `Cais_VisualReview/Inicio_do_caminho.001` e/ou `Degrau_cais_01/02.001`. A amostra está na borda norte dos bounds do terreno. É um pequeno trecho de solo sustentado pela malha, mas o teste em grade não o conecta ao componente principal. Só o teste com o `CharacterController` em deslocamento contínuo pode determinar se os colliders do cais permitem atravessá-lo.

## Waypoints verificados e diferenças

As posições foram convertidas de Blender XYZ para Unity como `xUnity=-xBlender`, `yUnity=zBlender`, `zUnity=-yBlender`. Para enseada, largo, B/A/L, horta/ruína, subida oeste, topo, encosta leste e retorno ao cais, as células mais próximas ficaram a menos de `0,0001 m` em XZ e no componente principal. A altura amostrada pelo collider ficou cerca de `0,300 m` abaixo da altura informada pelo waypoint em todos esses pontos; no cais, a diferença foi `−0,200 m`. O deslocamento vertical observado não foi corrigido nem explicado por esta análise e deve ser conferido na integração/âncora.

| Região | Resultado local |
|---|---|
| Cais `(0; 1,2; -4)` | Centro tem suporte, mas a amostra de perímetro falha; célula válida mais próxima a `2,06 m`, no componente secundário do cais. |
| Enseada, largo, setores B/A/L, horta/ruína, subida oeste, topo, encosta leste, retorno | Célula mais próxima válida no componente principal. |
| Recanto `(12; 17,406; -159)` | Célula sob o ponto do waypoint obstruída por `Casa_Recanto_004_Collider`; célula válida mais próxima a `3,00 m`, no componente principal. Isso verifica o solo acessível ao redor, não o percurso físico de entrada/retorno ao recanto. |

Relatório integral por triângulo, célula, obstáculo, região e componente: [walkability-analysis-20261112-234755.json](walkability-analysis-20261112-234755.json).

## Reanálise de conectividade após desativar a trilha antiga

- Execução em 2026-11-13 00:05:50 UTC, com cena 004 ativa em Play Mode. O Unity permaneceu responsivo; execução ~1,2 s. Nenhum input foi enviado, nenhuma movimentação foi iniciada pela análise e nenhuma cena foi alterada ou salva.
- A trilha `Cais_VisualReview/Inicio_do_caminho.001` não apareceu como collider ativo na consulta. O analisador agora também ignora `CharacterController` de atores na consulta de obstáculos, para que o jogador em Play Mode não conte como barreira estática. O scan de triângulos foi repetido pelo harness junto da grade; permaneceu em 7.290 faces e 0 violações >35°.
- Componentes da grade: **1**, com 87.783 células válidas e área aproximada `21.945,75 m²`. As duas células antes isoladas, `(-2; ~1; -5)` e `(-2; ~1; -4,5)`, agora são válidas no componente principal. O total cresceu em 92 células válidas frente à análise inicial, enquanto 90 células deixaram de ser classificadas como obstruídas. As amostras vizinhas ainda identificam água/margem sem footprint a oeste e colliders de degrau do cais a leste; isso limita o acesso local, mas não desconecta mais as duas células na grade.
- Restante da grade: 32.090 sem suporte no retângulo dos bounds, 1.515 rejeitadas por borda/descontinuidade e 2.515 obstruídas por colliders. Os colliders de degrau (`Degrau_cais_01/02`) ainda aparecem junto à junção; blocos e casa seguem como obstruções esperadas nos seus footprints.
- Waypoint do cais continua sobre região de margem (`edge-or-step-discontinuity`); a célula válida mais próxima está a `1,50 m`, em `z≈−5,5`, no componente principal. Isto não mede a passagem pelo cais. Centro do waypoint do recanto continua ocupado pela casa; suporte livre mais próximo fica a `3,00 m`.
- JSON comparativo atualizado: [walkability-analysis-20261113-000550.json](walkability-analysis-20261113-000550.json). A análise foi solicitada como rechecagem de conectividade, mas o harness também repetiu seu scan geométrico curto; não executou teste de movimento físico.

## Limites e continuação necessária

O componente secundário deixou de aparecer na grade após desativar a trilha antiga, mas a área de amostragem continua discreta e pode omitir gargalos menores que `0,5 m`; o capsule cast também é aproximação, não `CharacterController.Move`. O relatório varre `MeshCollider.sharedMesh`, não a malha interna cozida do PhysX. Não foi executado percurso em Play Mode nem input físico. Para decidir se o trecho junto ao cais afeta a navegação, percorrer continuamente a junção e testar acesso/retorno com WASD e registrar o trajeto; não alterar geometria com base apenas nesta amostra.
