# Plano — kit PS1 022
## Contexto
Unity 6000.3.25f1 Built-in e Blender existentes. PNGs 32/64 com Point/Closest. Aproveitar World Builder 021; não atualizar dependências. Fontes nesta worktree; instalação e validação no Unity principal existente.
## Constituição
Trecho pequeno, editável e avaliável, sem modificar cânone ou construir ilha inteira. Preservar cena principal e outros conjuntos. Aprovação da spec registrada. Nenhuma geração externa de modelos.
## Estrutura
Source/Blender/KitPS1_022: fonte editável e script de produção.
Freyja/Assets/_Project/Art/KitPS1_022: texturas e FBX.
Freyja/Assets/_Project/WorldBuilder: evolução mínima do plugin 021 para preservação da pintura, base costeira e biblioteca.
Artifacts/KitPS1_022: capturas, percurso e testes.
## Execução
1. Inspecionar editores e fontes; guardar baseline 021 sem misturar alterações alheias.
2. Produzir seis texturas de solo + casca/folhagem, três pedras e duas árvores com UVs no Blender.
3. Preservar camadas e alphamaps no rebuild; base costeira explícita e escolha de material da via.
4. Biblioteca de prefabs com colocação pelo editor e Undo; pintura nativa do Terrain.
5. Montar praia–casa, executar teste de preservação e caminhada; conferir capturas na câmera real e documentar.
## Validação
Testes novos só para preservação e comportamento alterado. Resoluções/filtragem medidas, rebuild+salvar/reabrir conferidos, vídeo curto do percurso, fonte Blender reaberta e auditada. Build fora do escopo.
