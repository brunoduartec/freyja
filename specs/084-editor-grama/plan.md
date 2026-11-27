# Plano de implementação: Editor manual de grama baked

**Branch**: `084-editor-grama` | **Data**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Resumo

Criar uma janela UI Toolkit chamada **Editar Grama** com entrada fácil de achar em `Freyja > Vegetação > Editar Grama`, botão no Inspector de `Coastal Grass` e ferramenta de pincel/seleção na Scene View. As edições ficam como operações manuais persistentes no `VegetacaoReassentada.asset`, o `CoastalGrassLayout` usado no campo `layout` da cena, e são compostas sobre raízes base/autorais e suplementos de `IslandDensity078`. O bake atual atualiza somente dados procedurais e conserva a camada manual separada.

## Contexto técnico

- **Linguagem/versão**: C#, Unity 6000.3.25f1.
- **Dependências primárias**: Unity Editor UI Toolkit, `EditorTool`/SceneView Handles, Undo, `CoastalGrassAuthoring.FilterGrassSites`; sem pacote novo.
- **Armazenamento**: operações de edição serializadas em `Freyja/Assets/_Project/Art/Relevo037/VegetacaoReassentada.asset`, `CoastalGrassLayout` atribuído no campo `layout` de `Coastal Grass — Ilha`; não gerar um GameObject por tufo.
- **Testes**: compilação no Editor; harness editável para add/select/move/delete, validação de superfícies e Undo/Redo; abrir ferramenta via menu e Inspector; persistência após salvar/reabrir asset/cena; inspeção na Scene View e câmera real.
- **Plataforma-alvo**: Editor Unity macOS na cena `Ilha_Principal`.
- **Tipo**: ferramenta interna de authoring para Unity Editor.
- **Metas de performance**: repintura altera somente o conjunto manual necessário e reconstrói regiões afetadas; nenhum custo ou alocação adicional em runtime quando não há edições; sem GameObjects por tufos.
- **Restrições**: preservar IDs/GUIDs e layouts atuais; não trocar renderizador, LOD, shader ou fluxo do terreno; undo deve operar nos assets via Undo/SerializedObject; bake nunca pode descartar operações manuais silenciosamente.
- **Escopo**: Scene View para adicionar/remover tufos e selecionar/mover/girar/escalar tufo existente; menu principal, botão contextual no Inspector e instruções no painel. Pincel edita a camada manual; raízes procedurais existentes podem receber override de remoção/substituição sem serem regravadas.

## Constituição e portões

- **Constituição 1**: spec aprovada antes de planejar; sem mudança em fatos do jogo — passa.
- **Constituição 2**: Unity Editor/CLI será o ambiente de edição e validação; Blender não se aplica — passa.
- **Constituição 3**: ferramenta é de authoring; verificar legibilidade de tufos e caminhos na câmera jogável — passa.
- **Constituição 4**: cânone e pistas não mudam; validar caminhos/interações livres — passa.
- **Constituição 5**: cada tarefa ligará código, assets e evidência — passa.
- **Gate**: manter built-in pipeline, runtime instanciado e arquivos de cena não relacionados intactos — passa.

## Pesquisa e decisões

1. **UI**: UI Toolkit em `EditorWindow`, pois o projeto não tem padrão de janela de Editor existente. Scene View interage via callback/handles do Editor. Fontes Unity: [SceneView.duringSceneGui](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneView-duringSceneGui.html), [Undo.RecordObject](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Undo.RecordObject.html) e [EditorUtility.SetDirty](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/EditorUtility.SetDirty.html).
2. **Acesso**: item de menu curto e indexável (`Freyja/Vegetação/Editar Grama`), botão no Inspector ao selecionar `Coastal Grass — Ilha` e `EditorTool` contextual quando o Terrain está selecionado. Os acessos abrem/ativam o mesmo fluxo de authoring.
3. **Dados**: adicionar um overlay serializado de operações manuais ao `CoastalGrassLayout` do `VegetacaoReassentada.asset`, separando adições e overrides da geração. Identidade da raiz de origem usa camada (layout/base, authoring ou density), mesh e coordenadas X/Z quantizadas a 1 mm. Override de movimento suprime a raiz de origem e cria destino manual; remoção registra tombstone.
4. **Bake**: `SetCells` atualiza somente a distribuição gerada. Se o authoring bake trocar o asset `layout`, copia o overlay anterior para o novo layout antes do `Configure`; não há reset implícito.
5. **Renderer**: durante `RebuildPlacement`, filtrar raízes do `layout`, `authoredLayout` e `densityLayout` com tombstone e acrescentar destinos manuais nos lotes/regions, recalculando bounds com margem de vento. O batching GPU existente e os perfis são reutilizados.
6. **Validação espacial**: raycast seleciona Terrain de produção. Antes de persistir um ponto, usar `FilterGrassSites`, máscara e exclusões ativas; rejeitar caminho, areia, construção, borda e inclinação acima do limite com mensagem no painel. Espalhamento/aleatoriedade é determinístico por seed de pincel.

## Estrutura e pontos de mudança

```text
Freyja/Assets/_Project/Scripts/Environment/
├── CoastalGrassLayout.cs              # structs das operações manuais serializadas
└── CoastalGrass.Lit.cs                # composição overlay, tombstones e rebuild de bounds
Freyja/Assets/_Project/Editor/
├── GrassPlacementEditorWindow.cs      # UI Toolkit e Scene View brush/selection
└── CoastalGrassEditor.cs              # botão acessível no Inspector do componente
Tools/Grass078/
└── CheckManualGrassEditing.cs         # harness de operações e validação Undo/persistência
specs/084-editor-grama/
└── evidence/                           # capturas, resultados e percurso de validação
```

**Estrutura escolhida**: janela e custom inspector vivem em `Assets/_Project/Editor`; nenhuma cena nova e nenhuma mudança de cena é necessária. Dados manuais vivem em `Assets/_Project/Art/Relevo037/VegetacaoReassentada.asset`, o layout já apontado pelo componente, de modo que reinstanciar a cena continua lendo o mesmo overlay. `AuthoredCoastalTufts078.asset` e `IslandDensity078.asset` são fontes apenas de leitura para os overrides.

## Estratégia de implementação

1. Introduzir modelo overlay e acesso determinístico às colocações sem GameObjects.
2. Integrar overlay ao rebuild existente, às exclusões, LOD e bounds; proteger overlay no bake.
3. Implementar janela UI Toolkit, entrada principal, ação contextual, EditorTool e instruções acessíveis.
4. Implementar pincel add/erase e seleção/movimentação/rotação/escala via Scene View, com Undo agrupado e validação do terreno.
5. Validar Edit Mode e Unity Editor/Scene View; capturar janela e cena, salvar/reabrir e confirmar caminho livre.
6. Atualizar `validacao.md`, tarefas e convergência; integrar somente após a avaliação da entrega.

## Riscos e mitigação

- **Edição de asset binário grande**: manter overlay pequeno e armazenado separadamente de matrizes procedurais; persistir ao fim do gesto, não a cada evento Scene GUI.
- **Undo do ScriptableObject**: registrar cada gesto como uma transação com Undo e validar reversão/redo.
- **Bakes posteriores**: teste que regenera dados e/ou troca o layout sem remover o overlay manual.
- **Conflito local da cena**: não salvar nem editar `Ilha_Principal.unity` nesta entrega; scene reference atual já aponta ao layout asset.
- **Editor disponível**: confirmar instância conectada antes da captura; se a instância da raiz estiver indisponível, não declarar validação no Editor.
