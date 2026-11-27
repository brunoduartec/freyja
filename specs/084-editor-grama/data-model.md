# Modelo de dados: edição manual da grama

## Edit operation

Registro serializado de autoria manual em `Freyja/Assets/_Project/Art/Relevo037/VegetacaoReassentada.asset` (`CoastalGrassLayout` atribuído em `layout`), distinto das células procedurais. `authoredLayout` e `densityLayout` são fontes de colocação observadas, não alvos de gravação.

| Campo | Uso |
|---|---|
| `Kind` | Add, Replace ou Remove |
| `SourceLayer` | `layout` base, `authoredLayout` costeiro, `densityLayout` suplementar, ou nenhum para novo tufo |
| `SourceRootKey` | Mesh + X/Z em milímetros da raiz mascarada em Replace/Remove |
| `Destination` | Root, Yaw, Scale e MeshIndex usados por Add/Replace |
| `StableId` | Identificador estável para seleção/Undo no painel |

## Brush settings

Estado transitório da janela: modo (Adicionar/Apagar/Selecionar), raio, espaçamento, mesh/perfil, faixa de escala e yaw aleatórios, seed do pincel. Não é incorporado ao build/runtime.

## Composition

- As células/tufts em `VegetacaoReassentada.asset` (`layout`), `AuthoredCoastalTufts078.asset` (`authoredLayout`) e `IslandDensity078` (`densityLayout`) continuam sendo fontes baked/procedurais.
- Replace/Remove oculta uma raiz de origem pela chave quantizada; Replace acrescenta a posição destino ao overlay.
- Add acrescenta um destino sem raiz de origem.
- `RebuildPlacement` combina origens elegíveis e operações manuais, executa filtro/exclusão, distribui por região e recalcula bounds com deformação de vento.
- O bake substitui apenas células/tufts gerados e mantém operações manuais; se mudar o asset de layout, transfere o overlay antes de atualizar a referência do renderer.
