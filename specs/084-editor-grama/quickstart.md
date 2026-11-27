# Quickstart — Editar grama

1. Abrir `Freyja > Vegetação > Editar Grama`, ou selecionar `Coastal Grass — Ilha` e clicar **Editar Grama** no Inspector.
2. Deixar `Ilha_Principal` em Edit Mode; escolher Adicionar, Apagar ou Selecionar/Editar. O painel mostra atalhos e estado do brush.
3. Na Scene View, posicionar o cursor no Terrain e pintar ou clicar em um tufo. O círculo mostra o raio; caminhos, areia, construções, inclinações inválidas e exclusões rejeitam a edição.
4. Para mover, selecionar o tufo e arrastar o handle; yaw/escala podem ser ajustados no painel. Cada passada/transformação vira uma ação Undo.
5. Salvar a cena/asset. Fechar e reabrir `Ilha_Principal`; conferir os mesmos tufos e a câmera de gameplay.
6. Executar o bake procedural existente. A camada manual permanece no mesmo asset ou é transferida caso o bake gere outro layout.

## Aceite

- Menu, busca e botão do Inspector abrem a mesma janela em até duas ações.
- Add/erase/select/move/rotate/scale aparecem na câmera e sobrevivem ao reload; Ctrl/Cmd+Z e Redo restauram a passada.
- Pontos proibidos não são persistidos; IDs não duplicam e nenhum GameObject por tufo é criado.
- Bake, salvar e recompilar não apagam edições manuais; runtime mantém profiles, LOD, instancing, luz e vento.
