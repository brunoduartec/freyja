# Decisões técnicas

- **Decisão**: manter a geometria e usar Smart UV Project com atlas único. **Motivo**: o pedido é UV e textura de uma malha já aprovada; a triangulação densa não tem costuras. **Alternativas**: projeção apenas superior distorce falésias; retopologia altera o escopo.
- **Decisão**: materiais-fonte pixelados, máscara geográfica e bake de cor sem iluminação. **Motivo**: separar grama, pedra, areia e caminhos de forma editável e manter a textura portátil. **Alternativas**: apenas cores por inclinação não recuperam caminhos; projetar a ilustração inteira introduz luz e perspectiva incorretas.
- **Decisão**: atlas 2048² com Closest. **Motivo**: ilha completa com muitas superfícies; fontes de material até 256² conservam o acabamento em pixels. Ajustar somente se a inspeção comprovar insuficiência.
- **Decisão**: avaliar no Blender nesta entrega. **Motivo**: nenhuma integração Unity foi aprovada.
- Nenhuma incerteza de produto bloqueia o trabalho. Parâmetros visuais serão refinados na primeira versão.
