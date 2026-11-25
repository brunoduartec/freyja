# Galpão e píer na cena jogável de Freyja

**Branch** `013-integrar-galpao-pier`  
**Data** 2026-11-14  
**Estado** Integração entregue para avaliação

## Cenário e teste

Como jogador, quero ver o galpão com píer no ponto de desembarque da ilha e conseguir caminhar da entrada do píer até o galpão e o caminho da vila.

1. Ao abrir a cena `Ilha_Tripo_Caminhada_011`, encontro o galpão na margem da enseada sul, junto à extremidade do caminho, com o píer voltado para a água.
2. Em Play Mode, o protagonista atravessa a junção entre o píer, o patamar e a margem e entra no galpão. O percurso existente na vila continua acessível.

## Requisitos

- **FR-001** Usar o modelo da entrega 012 e posicioná-lo na cena jogável existente pelo MCP do Unity. A [vista aérea](../../Referencias/modelagem-vista-aerea.png) orienta a posição na margem.
- **FR-002** Ajustar posição, rotação e, se necessário, escala uniforme. Usar a ilha calibrada pelos degraus e o protagonista de 1,80 m como referência. Sustentar o galpão na margem e orientar o píer para fora da costa.
- **FR-003** Configurar colisões para que o piso seja transitável. Se houver desnível na junção com o caminho, ajustar a ligação imediata.
- **FR-004** Salvar a cena com o ativo e apresentar uma captura na câmera do jogo para avaliação.

## Critérios de sucesso

- **SC-001** Uma vista geral confirma a ligação entre o caminho, o galpão e o píer na enseada. As superfícies transitáveis ficam livres de interseções visíveis com rochas ou terreno.
- **SC-002** Uma passagem em Play Mode confirma que o protagonista percorre o píer, entra no galpão e retorna ao caminho.
- **SC-003** A cena abre com referências válidas, e a verificação do Console cobre os erros relevantes à integração.

## Premissas

- A cena alvo é `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`, confirmada como ativa no editor.
- A posição proposta é na extremidade do caminho no lado direito da enseada visto da vista aérea. O ponto exato e o ângulo dependem da malha local.
- Qualquer mudança de terreno ou colisão fica restrita ao encontro entre o modelo e a margem.
