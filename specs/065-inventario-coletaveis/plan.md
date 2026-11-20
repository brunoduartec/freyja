# Plano — Inventário e itens coletáveis

Branch `065-inventario-coletaveis`. Spec aprovada em 19/11/2026.

## Contexto técnico

Unity 6000.3.25f1, Built-in, C# e Input System. Usar a cena principal e a suspensão de controle de CaisSessionController. Reaproveitar IslandDialogue como ponto único para resolver a interação E e os painéis da exploração. Implementar o estado dos itens em IslandInventory, com objetos de cena configurados por InventoryPickup. Adotar I para o inventário e manter Tab para o caderno.

## Constituição

Arte low poly avaliada na câmera real. Pistas e objetos físicos têm estados independentes. Entrega limitada ao fluxo aprovado na spec. Cena salva pelo editor e fluxo verificado em Play Mode.

## Estrutura

Código em Assets/_Project/Scripts/Prototype, props em Assets/_Project/Art/Inventory065 e fonte em Source/Blender/Inventory065. Os três objetos são instâncias na cena principal.
