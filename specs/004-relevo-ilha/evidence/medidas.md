# Medições da maquete 004

Fonte editável: `Source/Blender/Ilha_Relevo_004.blend`  
Exportação: `Freyja/Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_004.fbx`  
Capturas: `vista-aerea-004.png` e `vista-topdown-004.png` nesta pasta.

## Geometria

- Malha caminhável: `Terrain_004_Walkable_Island`, construída em grade regular de 2,5 m e conectada em uma única região após a remoção de um vértice solto.
- Vértices: 3.791; faces triangulares: 7.290.
- Limites da superfície no Blender: X = −87,5…87,5 m; Y = 4…180 m; Z = 0,99…32,70 m. Extensão XY: **175 × 176 m**, abaixo dos 264 × 180 m da 003.
- Maior inclinação medida sobre todos os triângulos do solo: **32,70°**; triângulos acima de 35°: **0**.
- A transição costeira é modelada em superfície contínua e cai até aproximadamente o nível do mar; não foram criadas paredes ou costões verticais terrestres.
- A enseada curva fica a oeste (esquerda) do cais; a crista ampla ocupa o norte. O cais mantém a referência Blender **(0, 4, 1)**.
- Uma extensão terrestre central de 5 m de largura liga o cais ao solo. Raycasts verticais em X=0, Y=4…18 m atingem o chão continuamente a Z=1,000…1,002 m. Ela sobrepõe o encaixe do cais para eliminar a lacuna ao desligar o terreno 003; não há tábuas ou barco incluídos nesta extensão.

## Caminhos e acessos

Fitas conformadas por raycast vertical (BVH) na malha final. Inclinação máxima observada por fita:

| Objeto | Faces | Máxima |
|---|---:|---:|
| `Path_Cais_Plaza` | 8 | 9,84° |
| `Path_West_Horta_Ruins` | 82 | 21,66° |
| `Path_East_Homes` | 68 | 13,58° |
| `Path_North_Ridge` | 86 | 30,50° |
| `Path_Back_Overlook` | 156 | 30,59° |
| `Path_West_Return` | 84 | 29,51° |
| `Path_Back_Return` | 34 | 29,04° |
| `Path_Casa_Recanto` | 44 | 31,54° |

Os extremos planejados compartilham coordenadas nos nós: subida/recanto, mirante/retorno, mirante/retorno oeste e subida/retorno oeste. As fitas seguem a superfície final após a reconstrução e não excedem 35° nas faces medidas. A conectividade foi inspecionada geometricamente; o percurso ainda precisa ser validado na cena Unity/Play Mode.

## Materiais

`04 Grass Deep`, `04 Grass Sun`, `04 Moss Ridge`, `04 Sand`, `04 Mountain Stone`, `04 Water`, `04 House Plaster`, `04 Roof Terracotta`, `04 Wood` e `04 Crop`.

O plano `Mar_Preview_004` é apenas de apresentação Blender e foi excluído do FBX. Os meshes `Cais_*` e `Barco_Cais_004` também foram excluídos para preservar cais e barco existentes na cena Unity. Os empties `WP_*` são referências de teste e não foram exportados; suas posições estão em `waypoints-004.md`.

## Validação e limite

Medições realizadas na cópia 004 reaberta pelo Blender MCP CLI, depois de salvar. O FBX foi exportado dessa mesma cena. A verificação de declive e conectividade acima é sobre a malha Blender; ainda não confirma a malha de colisão resultante após importação no Unity.
