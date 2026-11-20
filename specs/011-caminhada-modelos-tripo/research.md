# Pesquisa técnica — Spec 011

## Modelos e importação

**Decisão.** Usar `TripoAi/principal/stylized hiker 3d model.glb` como primeira opção de personagem e `TripoAi/cenario/stylized island terrain 3d model.glb` como cenário. Importar no Blender, salvar fontes editáveis e exportar FBX com texturas explícitas para o Unity.

**Evidência e motivo.** Os metadados do GLB do personagem indicam esqueleto com 65 juntas, pesos e zero clipes. O terreno texturizado tem 580.632 triângulos e uma imagem de cor embutida. Preservar UV, proporções e textura de cor. Conferir materiais, pivô e orientação após a importação. O GDD já prevê o fluxo Blender → FBX e texturas. [GDD](../../GDD.md) · [Manual do importador glTF](https://docs.blender.org/manual/en/5.2/addons/scene_gltf2.html)

**Alternativas.** Conferir o FBX do protagonista como alternativa se o primeiro arquivo apresentar um problema de deformação ou material. O Tripo Bridge já consta no projeto como dependência local com caminho absoluto. A cena será montada com assets importados pelo Unity para que seu funcionamento independa do uso do bridge. A presença do pacote local ainda deve ser conferida na abertura do projeto. [Manifesto](../../Freyja/Packages/manifest.json)

## Animação e movimento

**Decisão.** Criar clipes cíclicos de repouso e caminhada no rig do protagonista, sem translação da raiz. Usar rig Generic para os clipes do próprio esqueleto. Vincular a caminhada ao deslocamento horizontal efetivo em solo, com limiar para repouso e velocidade de reprodução ajustada à passada.

**Evidência e motivo.** `CaisPlayerController.PlanarVelocity` é calculada antes de `CharacterController.Move` e representa a velocidade pretendida. Medir a diferença de posição ao redor da chamada de movimento permite voltar ao repouso contra uma parede e evita contar o teleporte de recuperação. Manter a raiz física em escala unitária e corrigir orientação, altura e pivô na parte visual. [Controlador existente](../../Freyja/Assets/_Project/Scripts/Prototype/CaisPlayerController.cs)

**Alternativas.** Humanoid fica disponível se surgir necessidade de retargeting. Root motion exigiria conciliar deslocamento da animação com a movimentação e a colisão existentes.

## Escala, relevo e câmera

**Decisão.** Escolher e medir o trecho no editor, com escala uniforme do terreno. Começar pela cápsula de 1,8 m usada no protótipo. Produzir uma malha de colisão estática simplificada e alinhada ao relevo renderizado no trecho. Manter a câmera e a recuperação do cais, com referências e limites próprios da nova cena.

**Evidência e motivo.** O terreno importado e o personagem estão em escalas de origem distintas das unidades do jogo. A topografia vai orientar o início, o marco, a inclinação praticável e a área de queda. A câmera existente já sonda obstáculos. Incluir chão e encostas nas máscaras e excluir jogador, mar e gatilhos da sonda. [Câmera](../../Freyja/Assets/_Project/Scripts/Prototype/CaisCameraRig.cs) · [Recuperação](../../Freyja/Assets/_Project/Scripts/Prototype/CaisSessionController.cs)

**Alternativas.** Usar toda a malha visual como colisão simplifica o protótipo inicial, mas aumenta a geometria de colisão e pode reproduzir irregularidades irrelevantes para a caminhada. Deformar a ilha por eixo prejudicaria a comparação com a fonte.

## Imagem em jogo

**Decisão.** Usar uma Render Texture de 480 × 270, 1 amostra e filtro pontual para o mundo, com interface na resolução da tela. Validar o enquadramento e a oclusão na saída real, inclusive a proporção usada pela sonda da câmera. Os materiais serão calibrados no Built-in com a iluminação da cena.

**Motivo e alternativas.** 480 × 270 é a base do GDD. Resoluções alternativas, dithering e deformação de vértices permanecem como experimentos do GDD. [Direção visual](../../GDD.md)

## Revisão de escala pelos degraus

A pedido do usuário, o cenário passou a usar escala uniforme calibrada pelos degraus originais. Medidas finais de 57,600 × 48,396 × 9,857 m, protagonista de 1,80 m e desníveis típicos de aproximadamente 0,18 m. O fator é 0,36 em relação à primeira integração. O modelo original foi restaurado e as escadas adicionais foram removidas.

A referência para reutilização está em [Source/Blender/Ilha_Tripo_011/README.md](../../Source/Blender/Ilha_Tripo_011/README.md). Esta revisão substitui as medidas anteriores de escala.
