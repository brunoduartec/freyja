# Tarefas — Inventário e itens coletáveis

## Base

- [x] T001 Implementar estado de sessão e contagem em Freyja/Assets/_Project/Scripts/Prototype/IslandInventory.cs.

## P1 — Coletar

- [x] T002 [US1] Definir unidades coletáveis em Freyja/Assets/_Project/Scripts/Prototype/InventoryPickup.cs.
- [x] T003 [US1] Arbitrar coleta e conversa em Freyja/Assets/_Project/Scripts/Prototype/IslandDialogue.cs.
- [x] T004 [P] [US1] Criar dois modelos provisórios em Source/Blender/Inventory065/ e exportar para Freyja/Assets/_Project/Art/Inventory065/.
- [x] T005 [US1] Posicionar os três objetos e salvar Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity.

## P2 — Consultar

- [x] T006 [US2] Abrir e fechar o inventário e exibir nomes e quantidades em Freyja/Assets/_Project/Scripts/Prototype/IslandDialogue.cs.

## Validação

- [x] T007 Verificar compilação, diálogo e coleta em duas sessões conforme specs/065-inventario-coletaveis/quickstart.md.

## Dependências

T001 precede T002 e T003. T004 pode ocorrer independentemente do código. T005 depende dos objetos e do código. T006 reaproveita o estado e a seleção de interação. T007 valida a cena integrada.

## Evidências

- A cena principal salva contém as referências ao inventário e aos três objetos.
- Os modelos editáveis estão em `Source/Blender/Inventory065/Inventory065.blend` e os FBX em `Freyja/Assets/_Project/Art/Inventory065/`.
- A compilação do Unity passou. As duas rodadas e as capturas da Game View estão em `Artifacts/Inventory065/`.
