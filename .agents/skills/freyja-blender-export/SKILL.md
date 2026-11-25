---
name: freyja-blender-export
description: Editar fontes Blender e validar exports de modelos, cenários ou animações no Unity do Freyja.
---

# Blender → Unity no Freyja

1. Use o MCP global do Blender. Confirme o arquivo `.blend` e a cena alvo seguindo `Tools/MCP/README.md`. Consulte apenas as coleções, objetos e propriedades relevantes. Agrupe operações repetitivas após confirmar o alvo e retorne um resumo curto. Use processamento em lote para conversões determinísticas quando a verificação no editor continuar viável.
2. Salve a fonte editável em `Source/Blender/` e exporte para `Freyja/Assets/_Project/Art/` conforme a convenção do asset. Documente os caminhos adotados. Preserve o par asset/`.meta` e o GUID em reexports e movimentos.
3. Confira unidade, escala, eixos, pivôs, materiais e colisões no Unity, em relação aos elementos de referência. Para modelos TripoAI da spec 011, consulte `Source/Blender/Ilha_Tripo_011/README.md` para a escala dos degraus e do protagonista de 1,80 m.
4. Verifique a direção low poly, silhueta pixelada e legibilidade na câmera real do jogo. Se o fluxo interativo mudou, teste-o em Play Mode. Registre a evidência visual necessária e os caminhos da fonte e do export.
