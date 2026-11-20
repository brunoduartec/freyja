# Modelo de dados

- InventoryPickup contém ID único da unidade, ID do tipo e nome exibido. Começa ativo e fica inativo após coleta bem-sucedida.
- IslandInventory mantém IDs coletados e agrupa unidades pelo ID do tipo. Disponibiliza quantidade por tipo para futuras missões.
- Uma coleta aceita incrementa a quantidade uma vez. Desativar o objeto acontece após essa aceitação. Uma sessão nova parte da cena inicial.
