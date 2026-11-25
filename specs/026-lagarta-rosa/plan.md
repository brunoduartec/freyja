# Plano de produção do lagarto rosa

**Branch** `026-lagarta-rosa`

## Entrega

Ilustrar a criatura em vista de três quartos, usar a imagem como entrada da geração no Tripo e preservar um modelo texturizado e editável. A verificação inclui o PNG em miniatura e a malha vista em quatro ângulos no Blender.

## Contexto técnico

ImageGen gera o PNG. O CLI oficial do Tripo gera o GLB texturizado. Blender via MCP inspeciona a malha e salva uma cópia com texturas incorporadas. A validação é visual e inclui reabrir os arquivos.

## Constituição

A direção low poly orienta a silhueta. A entrega é independente da cena do Unity e das regras de investigação. Os arquivos de produção ficam fora de `Assets/` e cada tarefa aponta para um artefato verificável.

## Estrutura

- `TripoAi/lagarto-rosa/` guarda a imagem, o resultado original do Tripo e sua prévia.
- `Source/Blender/Lagarto_Rosa/` guarda o arquivo de edição com textura empacotada.
- `specs/026-lagarta-rosa/` guarda as decisões, instruções de validação e tarefas.
