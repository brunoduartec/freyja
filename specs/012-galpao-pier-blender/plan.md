# Plano do galpão com píer

**Spec** [Escopo aprovado](spec.md)

## Abordagem

Usar o MCP do Blender em uma sessão isolada para modelar o ativo em metros. Separar a malha em coleções de galpão e píer. Montar primeiro os volumes, a abertura e a passagem. Em seguida, aplicar mapas de cor com texels visíveis a pedra, terracota, madeira e piso.

## Arquivos e verificação

- Fonte editável, texturas e imagens de revisão em `Source/Blender/GalpaoPier/`.
- FBX e texturas em `Freyja/Assets/_Project/Art/Environment/GalpaoPier/`.
- Reabrir a fonte, conferir UVs, texturas empacotadas e medidas. Importar o FBX em sessão Blender separada para conferir escala e conteúdo.
- Comparar a vista geral com o PNG 11 e usar uma vista adicional para verificar a entrada.

## Constituição

Blender via MCP, geometria low poly e acabamento pixelado. Evidências salvas com o ativo. A avaliação desta etapa é isolada.
