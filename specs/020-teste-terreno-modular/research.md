# Pesquisa e decisões

- **Terreno editável:** Terrain nativo permite alturas e camadas de textura separadas. Malha inteira Tripo foi avaliada na 019, mas seus lotes exigiram adaptações. Escolha do usuário: testar heightmap. Referência: https://docs.unity.com/en-us/engine/6000.3/manual/creating-environments/script-terrain/terrain-textures
- **Módulos:** Blender via MCP produz pedras facetadas, trechos de alvenaria e cais. O relevo fica no heightmap; bordas e silhuetas fortes ficam nos módulos. Texturas pequenas repetíveis evitam depender de uma imagem única da ilha.
- **Sessão existente (pesquisa somente leitura):** cápsula 1,80 m, raio 0,30; velocidade 3 m/s; stepOffset 0,25; layer sólida 8. Session.Start valida spawn por raycast de 2 m, bounds e cápsula. Troca de ambiente ocorre em Edit Mode: desativar e reativar a sessão em Play não reinicializa Start. Preservar referências da câmera e controlador; alternar spawn, endpoint e recoveryBounds por SerializedObject.
- **Evidência:** gravação usa movimento real do controlador e timestamps para preservar duração. Inicialização e recuperação serão testadas antes, pois o gravador isolado não prova toda a sessão.
- **Worktree:** mantém autoria da 020 isolada. Editor aberto serve como destino de prévia; a ilha e suas fontes permanecem intactas. Não abrir um segundo projeto Unity nem atualizar pacotes.
