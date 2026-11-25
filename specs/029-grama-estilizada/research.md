# Decisões técnicas

- Usar meshes opacas instanciadas, com deformação no shader e bounds com margem de vento. O tamanho dos lotes é limitado e a câmera é explícita. `RenderMeshInstanced` considera o lote como unidade. [Unity](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html).
- Começar com células de 8 m. Usar distância do ponto mais próximo dos bounds e testes do frustum antes da submissão. Reutilizar matrizes e planos. Duas densidades estáveis e uma transição individual por tufo mantêm o padrão ao retornar. O terreno mantém sua cor de grama além do alcance.
- Distribuir no editor com a máscara Grass existente. Filtrar trilhas renderizadas por malhas sem collider, inclinação, água e obstáculos. Não é necessário lançar raios durante o jogo.
- Testar oclusão com bake apenas se houver dados da cena e um ganho mensurável. O estudo cobre Terrain Details e instanciamento indireto. [Estudo e referências](estudo.md).
