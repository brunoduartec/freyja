# Pesquisa: Editor manual de grama baked

## Decisões

- **UI Toolkit para janela Editor** — não há janelas de authoring atuais com padrão a preservar; o Unity 6 recomenda UI Toolkit para novas ferramentas de Editor. Controles oferecem raio, espaçamento, densidade, mesh/perfil, escala, yaw, modo e seleção.
- **Scene View callback e Handles** — a ferramenta deve desenhar o raio e a seleção no Scene View e converter cliques em raycasts no terreno. A documentação Unity descreve `SceneView.duringSceneGui` para receber chamadas durante o OnGUI da Scene View.
- **Uma entrada única reutilizada** — item principal `Freyja > Vegetação > Editar Grama` e botão no Inspector chamam o mesmo `EditorWindow`; a busca de comandos encontra o menu.
- **Overlay manual no asset de layout existente** — armazenar operações autorais em `VegetacaoReassentada.asset` (`CoastalGrassLayout` referenciado pelo campo `layout` de `Coastal Grass — Ilha`) e compor em memória no renderer. `authoredLayout` (`AuthoredCoastalTufts078.asset`) e `densityLayout` (`IslandDensity078.asset`) são fontes de raízes, não alvos de escrita. Isto preserva o bake 080 e evita mudança da cena ou GameObjects por instância.
- **Undo serializado** — `Undo.RecordObject` antes da mutação, `EditorUtility.SetDirty` após edição e salvar só no fim do gesto. As operações agrupadas permitem reverter uma passada completa.
- **Bakes preservam overlay** — `SetCells` no mesmo layout não altera a coleção manual. Quando `CoastalGrassAuthoring.Bake` cria/troca o `CoastalGrassLayout` atribuído, deve transferir os registros antes de `Configure`; não há limpeza implícita.

## Fontes primárias

- [Unity 6.0 SceneView.duringSceneGui](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneView-duringSceneGui.html)
- [Unity 6.0 EditorTools.EditorTool](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/EditorTools.EditorTool.html)
- [Unity 6.0 Undo.RecordObject](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Undo.RecordObject.html)
- [Unity 6.0 EditorUtility.SetDirty](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/EditorUtility.SetDirty.html)
- Código/refs Freyja: cena `Ilha_Principal.unity` aponta `layout` para `Art/Relevo037/VegetacaoReassentada.asset`, `authoredLayout` para `Art/Grass029/AuthoredCoastalTufts078.asset` e `densityLayout` para `Art/Grass029/IslandDensity078.asset`; demais pontos: `CoastalGrassLayout`, `GrassDensityLayout`, `CoastalGrass.Lit`, `CoastalGrassAuthoring.FilterGrassSites`, `IntegrateIsland.cs`.

## Alternativas consideradas

- **GameObject por tufo**: rejeitado; cresce a Hierarchy e ignora o batching existente.
- **Editar matrizes geradas diretamente**: rejeitado; mistura autoria e distribuição procedural e deixa bake destrutivo.
- **Persistir grama em TerrainData**: rejeitado para v1; Terrain detail prototype não usa o renderer Lit/LOD atual e separaria a fonte de verdade.
- **EditorTool toolbar como único acesso**: rejeitado como única entrada; pode ficar escondido. A janela/menu e botão do Inspector são visíveis, com integração Scene View na janela.
