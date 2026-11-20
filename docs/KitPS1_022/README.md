# Kit PS1 do Freyja

Abra **Tools → Freyja → Kit PS1 — praia**, depois **Abrir trecho de praia**.

- **Pintar terreno:** seleciona o Terrain. No Inspector, use Paint Texture e escolha grama, terra, areia de caminho ou areia de praia.
- **Terra / Pedra / Tijolo:** troca o acabamento dos caminhos.
- **Colocar objeto:** escolha uma das três pedras ou duas árvores e clique no terreno no Scene View. Esc cancela. Use as ferramentas normais de mover, girar e escala; Ctrl/Cmd+Z desfaz.
- **Editar lotes e relevo:** abre o World Builder no mundo correto. Ajustes dos lotes só são aplicados ao reconstruir.
- **Reiniciar pintura:** ação separada, com confirmação e Undo. Reconstruir lotes preserva pintura e objetos manuais.
- **Voltar à demonstração anterior:** retorna ao teste de três casas. A ilha original continua preservada pelo World Builder.

## Conteúdo
Nove texturas: grama, terra, areia de caminho, areia de praia, pedra, tijolo e rocha em 64×64; casca e folhas em 32×32. Filtragem Point, sem mipmaps nesta prova. Texturas repetíveis; sem geração 3D externa.

Três pedras de 20 triângulos cada. Árvores fornecidas pelo usuário: PineTree_2 e CommonTree_1, provenientes de Low Poly World/downloads/nature; cópias adaptadas com UVs, materiais e pivô inferior. Pinheiro: 1.692 triângulos; copa larga: 2.888. Originais preservados, exports separados em Assets/ThirdParty/Nature022.

Fontes editáveis: .utmp/022-kit-cenario-ps1/Source/Blender/KitPS1_022/KitPS1_022.blend. PNGs e FBX: Freyja/Assets/_Project/Art/KitPS1_022. Plugin e biblioteca: Freyja/Assets/_Project/WorldBuilder/Editor e Demo022.

## Verificado
16 testes de regressão passaram. Pintura preservada em duas reconstruções e após salvar/reabrir; Undo do reset e da colocação de árvore verificados. Base de alturas e objetos manuais preservados. Resoluções e filtragem conferidas. Caminhada real praia–casa concluída em aproximadamente 18 segundos, com entrada programada no controlador existente; não é teste manual de teclado. Vídeo e capturas em Artifacts/KitPS1_022. Fonte Blender reaberta, UVs e nove mapas empacotados conferidos.

## Limitações
Trecho de avaliação, não a ilha definitiva. Casa provisória e mar sem simulação. Escultura manual no terreno de saída é substituída ao reconstruir; alterações permanentes de relevo pertencem à base CoastalBase. Sem mipmaps pode haver cintilação à distância; avaliar junto à direção visual antes de ampliar. A câmera existente pode aproximar-se muito da casa na chegada. Build de distribuição não executado.

External 3D generation jobs: 0

Console final: scripts compilados sem falha; há registro de erro de permissão no cache Burst/InferenceEngine, originado na abertura do editor e fora do código do kit. Não foi corrigido nesta entrega; caminhada e testes do kit executaram apesar dele.
