# Mapa de integração e waypoints 004

Coordenadas abaixo estão em **Blender XYZ, metros**. Referência do cais: `(0, 4, 1)`. O FBX foi exportado com `axis_forward=-Z` e `axis_up=Y`; confirme a conversão de eixos e a âncora ao montar o prefab Unity.

Os empties `WP_*` existem somente na fonte `.blend` para orientar a integração/testes; não foram selecionados no FBX. O FBX contém os meshes de terreno, caminhos, blocos e marcos listados abaixo, mas exclui `Cais_*`, `Barco_Cais_004` e `Mar_Preview_004`, pois o cais/barco permanecem na cena Unity existente.

## Waypoints de percurso

| Empty Blender | X | Y | Z | Uso |
|---|---:|---:|---:|---|
| `WP_Cais` | 0 | 4 | 1,200 | início, no cais |
| `WP_Enseada` | −24 | 28 | 1,303 | praia curva a oeste do cais |
| `WP_Largo` | 0 | 68 | 5,080 | centro do largo |
| `WP_Setor_B` | −34 | 68 | 4,842 | setor B |
| `WP_Setor_A` | 0 | 103 | 16,256 | setor A, subida norte |
| `WP_Setor_L` | 34 | 68 | 4,475 | setor L |
| `WP_Horta_Ruina` | −55 | 91 | 9,639 | horta e ruína oeste |
| `WP_Subida_Oeste` | −28 | 109 | 19,685 | subida pelo flanco oeste |
| `WP_Topo_Monte` | −12 | 130 | 31,608 | crista/topo amplo |
| `WP_Encosta_Leste` | 44 | 113 | 21,330 | contorno pelo flanco leste |
| `WP_Recanto` | −12 | 159 | 17,406 | casinha isolada atrás do monte |
| `WP_Retorno_Cais` | 0 | 34 | 1,917 | retorno ao cais |

## Nomes de objetos a manter na integração Unity

- Solo: `Terrain_004_Walkable_Island`.
- Rotas: `Path_Cais_Plaza`, `Path_West_Horta_Ruins`, `Path_East_Homes`, `Path_North_Ridge`, `Path_Back_Overlook`, `Path_West_Return`, `Path_Back_Return`, `Path_Casa_Recanto`.
- Largo e blocos provisórios: `Largo_004`, `B1`–`B4`, `A1`–`A4`, `L1`–`L4`.
- Marcos: `Horta_Canteiro_01`–`Horta_Canteiro_05`, `Ruina_Murete_01`–`Ruina_Murete_04`, `Casa_Recanto_004`, `Casa_Recanto_Telhado_004`, `Barco_Cais_004`, `Cais_Deck_00`–`Cais_Deck_09`, `Cais_Longarina_-1.55`, `Cais_Longarina_1.55`, `Cais_Pile_*`.

## Centros dos 12 blocos e base amostrada

| Objeto | X | Y | Z do solo no centro |
|---|---:|---:|---:|
| `B1` | −29 | 52 | 2,625 |
| `B2` | −35 | 62 | 4,263 |
| `B3` | −36 | 74 | 5,648 |
| `B4` | −30 | 85 | 7,634 |
| `A1` | −21 | 90 | 9,395 |
| `A2` | −7 | 95 | 12,063 |
| `A3` | 7 | 95 | 11,853 |
| `A4` | 21 | 90 | 9,480 |
| `L1` | 29 | 52 | 3,400 |
| `L2` | 35 | 62 | 3,955 |
| `L3` | 36 | 74 | 5,042 |
| `L4` | 30 | 85 | 7,780 |
