# Ilha do TripoAI — UV e textura, versão 1

Abra **Ilha_Tripo_Texturizada.blend** no Blender 5.2.2 LTS. A câmera geral mostra o terreno com o material final `Island_Terrain_Atlas`. As imagens estão empacotadas e também disponíveis em `textures/` com caminhos relativos.

## Arquivos para avaliar

- `previews/General.png`: visão geral.
- `previews/Top.png`: distribuição das superfícies e caminhos.
- `previews/Path_Detail.png`, `Cliff_Detail.png`, `Beach_Detail.png`: pixels, transições e emendas.
- `previews/UV_Layout.png`: contornos das ilhas UV, sem a triangulação interna para facilitar a leitura.
- `textures/Island_BaseColor.png`: atlas final, sem iluminação gravada.
- `validation.json`: verificações após reabertura da fonte.

## Texturas

| Arquivo | Resolução | Função |
| --- | --- | --- |
| Grass_Pixel.png | 128×128 | tufos e variação de grama |
| Rock_Pixel.png | 256×256 | pedra, variação mineral e fissuras |
| Dirt_Pixel.png | 128×128 | terra e pequenos grãos |
| Sand_Pixel.png | 128×128 | areia da enseada |
| Terrain_Distribution.png | 1024×1024 | vermelho: caminhos/largo; verde: praia; azul: rochedos costeiros |
| Island_BaseColor.png | 2048×2048 | atlas UV único da ilha inteira |

Os nós de imagem usam **Closest**. O atlas é maior que os padrões individuais porque reúne toda a ilha e suas superfícies. Material fosco, roughness 0,93, sem normal map. Os detalhes e fissuras estão na cor.

## Editar

O objeto `Island_Tripo` possui a camada `Island_UV`. O material `Terrain_Source_Editable` está preservado com fake user e reúne os bitmaps e as máscaras antes do bake. Para alterar a distribuição, edite a máscara: o arquivo `Terrain_Distribution.json` documenta a projeção. Para alterar os padrões, edite os quatro PNGs de material.

Os scripts em `scripts/` registram o processo: `unwrap.py`, `make_textures.py`, `build_material.py`, `bake_atlas.py`, `draw_uv_layout.py`, `render_previews.py` e `validate_blend.py`. Os scripts com bpy executam dentro do Blender; o gerador de texturas e o layout usam Python com NumPy/Pillow. Eles sobrescrevem apenas os artefatos desta pasta. Reabrir o material-fonte e repetir o bake é necessário depois de alterar os padrões ou a máscara; o atlas não se atualiza sozinho.

## Revisão e limites

A referência é `Referencias/ilha-relevo-tripoai.png`. Foram ajustados o tom da grama, a exclusão de grama nos rochedos e a areia no piso da praia. O bake inicial apresentou pontos pretos em ilhas menores que um pixel; a extensão das cores vizinhas nos texels vazios corrige essa falha de amostragem. As prévias finais foram renderizadas depois da correção.

O GLB original, a posição dos vértices, as faces e a transformação foram preservados. Continuam **290.322 vértices e 580.632 triângulos**. O UV automático contém muitos fragmentos pequenos devido à triangulação original; não equivale a uma malha retopologizada para pintura manual ou otimizada para jogo. Algumas irregularidades de caminhos e degraus pertencem ao modelo original e foram mantidas.

Não houve integração, compilação, Play Mode ou build no Unity. A entrega termina nesta primeira versão para avaliação visual.
