# Plano: teste do cenário Tripo

**Branch:** 018-teste-ilha-cais-centro · [Spec](spec.md)

## Contexto

Unity 6000.3.25f1 Built-in, cena principal Ilha_Tripo_Caminhada_011, personagem existente 1,80 m, Blender MCP. Modelo GLB fornecido pelo usuário; manter original e ativos preexistentes. Nenhuma escala específica presumida.

## Método

1. Confirmar editor, cena e arquivo Blender; inspecionar GLB em cópia separada no Blender, identificar geometria, texturas, cais, caminhos, praça, lotes e proporções.
2. Calibrar a escala com cais e personagem, documentando compromissos e medindo amostras de lotes. Preparar fonte Blender, export adequado e colisão, sem alterar a geometria visual silenciosamente.
3. Na cena principal, preservar cenário anterior e configurar novo conjunto reversível, personagem, spawn, câmera e percurso.
4. Validar caminhada real no Play Mode, Console e registrar vídeo do cais ao centro; relatar problemas e medidas.

## Constituição

Aprovado: preserva fontes, faz validação no Unity e registra evidências, sem novas decisões narrativas. Reavaliar após execução.

## Artefatos

Fonte em `Source/Blender/Ilha_Tripo_018/`; export em `Freyja/Assets/_Project/Art/Ilha_Tripo_018/`; cena existente; vídeo e relatório nesta spec. Nenhuma interface externa.
