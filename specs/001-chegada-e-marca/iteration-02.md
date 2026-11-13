# Iteração 02 — Cais na Unity

**Estado:** pronta para avaliação. **Data:** 12/11/2026.

Após a aprovação do modelo Blender, importar somente esse trecho para uma cena estática de revisão, sem personagens ou investigação.

## Artefatos
- Cena: [Cais_Iteracao02.unity](../../Freyja/Assets/_Project/Scenes/Cais_Iteracao02.unity).
- Prefab: [Cais_VisualReview.prefab](../../Freyja/Assets/_Project/Prefabs/Cais_VisualReview.prefab).
- FBX: Freyja/Assets/_Project/Art/Environment/Cais/Cais_Iteracao01.fbx.
- [Prévia capturada na Unity](../../Source/Unity/Cais_Iteracao02.png).
- [Verificação via MCP](../../Source/Unity/Cais_Iteracao02.validation.json).

## Verificação executada
123 malhas importadas em escala métrica; barco com cerca de 5,25 m. Materiais Standard remapeados; nenhuma referência de material ausente. Câmera ortográfica e luz configuradas. 33 colliders adicionados; raycasts verticais no cais e caminho confirmaram colisão em Play Mode. Prévia gerada em 1280 × 960 e inspecionada. Play Mode encerrado após a revisão.

Faces do costão corrigidas na cópia de exportação, pois a Unity descarta o verso das faces. A cena Blender ativa e suas alterações não salvas foram preservadas; nenhuma alteração dessa fonte foi salva por esta etapa.

## Avaliar agora
Abrir a cena na Unity e observar proporções, cores, luz e enquadramento; Play Mode apresenta a mesma câmera estática. Navegação de personagem e teste completo de percurso ainda não foram implementados. Aguardar avaliação antes da próxima ampliação.

Os scripts de montagem e captura usados via MCP ficam em Tools/Unity/. A cena já está salva; não é necessário executá-los para avaliar.
