# Estudo: shader, iluminação e acabamento

**Estado:** protótipos estáticos testados; escolha final não aprovada. Referência visual: [personagens no cais](../../Referencias/ilha-dos-trinta-personagens.png). Comparação: [quatro variantes](comparacao-shader-luz.png).

## Observação e diagnóstico

A referência tem luz de fim de tarde, cores quentes nas superfícies iluminadas, sombras azul-violeta, texturas em blocos com variações de cor, personagens bem separados do fundo e rostos legíveis. São observações da ilustração: não sabemos quais técnicas a produziram. O último render tem luz mais neutra, sombras acinzentadas, materiais suaves e cenário ainda incompleto; a posição da câmera e a produção das texturas e modelos também contribuem para a diferença.

Inspeção da cena ativa no Unity 6000.3.25f1: materiais visíveis amostrados usam `Standard`; há uma directional light `Coastal Sun` (RGB 1,00/0,95/0,85; intensidade 1,15; rotação 45°/35°/0°; sombras suaves), ambiente `Trilight` com azul acinzentado acima e tons cinza-esverdeados no horizonte e solo; fog desligado. O pacote `com.unity.postprocessing` não consta no manifesto nem no lock. Não se conclui a contribuição exata de cada ajuste sem comparação experimental.

## Opções

| Abordagem | Potencial | Limitação |
| --- | --- | --- |
| **Standard + ajuste de luz e materiais** | Usa o que já existe; permite comparar o impacto da luz, contraste, cor e textura antes de mudar shader. | A resposta de luz e brilho permanece mais próxima do material atual; menos controle artístico sobre as faixas de sombra. |
| **Shader difuso estilizado com rampa suave** | Controla separadamente sombra fria, tons médios e luz quente; preserva volume com poucas faixas de transição; compatível em princípio com Built-in. | Requer shader e materiais próprios, suporte a sombras e avaliação de luz indireta, animação e rostos; faixas duras podem cintilar ou perder detalhes. |
| **Cel shading duro, contorno e paleta global** | Linguagem gráfica forte e controlada. | Pode gerar aparência de desenho contornado e destruir sutilezas de pele e texturas. Não é demonstrado que a referência use essa técnica. |
| **Somente pós-processamento** | Corrige cor e contraste globalmente sem trocar materiais. | Não recupera detalhes ausentes, não cria texturas pintadas nem controla de forma independente todos os materiais; pode alterar pele e pistas. |

## Recomendação para teste

Começar por **Standard calibrado**, para estabelecer uma base comparável: sol dourado, ambiente com sombra mais fria e menos cinza, materiais predominantemente foscos, exposição e contraste controlados. Depois comparar um **Surface Shader Built-in difuso com rampa de luz suave** em cópias de materiais do protagonista e do galpão, sem mexer nos originais. Começar com três regiões artísticas — sombra, meio-tom e luz — e transições ajustáveis; são parâmetros iniciais, não uma contagem extraída da imagem. A rampa precisa responder à atenuação e sombras, preservar textura e iluminação ambiente e não escurecer olhos e pistas. O exemplo oficial da Unity mostra o princípio de [toon por rampa no Built-in](https://docs.unity3d.com/6000.0/Documentation/Manual/SL-SurfaceShaderExamples-ToonRamp.html) e a documentação descreve [modelos de iluminação personalizados](https://docs.unity3d.com/6000.0/Documentation/Manual/SL-SurfaceShaderLighting.html).

Usar pós-processamento **leve e opcional** para harmonizar exposição, contraste e matiz depois da iluminação, antes da ampliação; manter a interface fora do efeito. Comparar com e sem o ajuste. A Unity disponibiliza [Post Processing v2](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.postprocessing.html); a versão específica compatível com o editor do projeto deve ser verificada antes de qualquer instalação. Não há necessidade comprovada de instalar pacote nesta pesquisa. Evitar assumir bloom, vinheta, granulação ou contornos como requisitos.

O filtro Point e a resolução das texturas devem ser avaliados por asset: as texturas do galpão já usam 128 × 128 Point, enquanto protagonista e cenário estão em 2048 × 2048 Bilinear. Point na ampliação final e Point nas texturas são escolhas diferentes. Testar mipmaps e distância, pois detalhes finos podem cintilar; a Unity descreve o papel dos [mipmaps](https://docs.unity3d.com/2022.3/Documentation/Manual/texture-mipmaps-introduction.html). Texturas pintadas em blocos e uma paleta por material podem exigir trabalho de arte, não apenas troca de filtro.

## Comparação proposta

1. Mesma câmera e pose no cais: A) cena atual; B) Standard com luz e materiais calibrados; C) B + rampa suave; D) C + correção de cor discreta. Alterar um grupo por vez e registrar valores, referências e PNGs.
2. Comparar em 480 × 270, 640 × 360 e 320 × 180 conforme o GDD, com a mesma ampliação Point e sem UI pixelizada. Examinar rosto em diálogo próximo e silhueta em caminhada, com câmera parada e em movimento.
3. Aceitar apenas se pele, olhos, roupa e pistas continuarem distinguíveis; sombras frias e luz quente aproximarem a referência sem esmagar detalhes; e movimento não produzir cintilação incômoda. Registrar vídeo curto e testar outros ângulos e proporções de tela.
4. Testar dithering 5 bits e matriz 4 × 4, encaixe de vértices e UV afim separadamente, conforme o plano existente; só combiná-los após comparar legibilidade e estabilidade. Não são decisões já aprovadas.

## Experimento executado

Capturas na cena real em 320 × 180, 480 × 270 e 640 × 360, mesma pose e câmera do estudo no cais. HDR e MSAA da câmera desativados; imagens do mundo, sem interface. A escala de apresentação abaixo é 3×, sem interpolação.

| Variante | Resultado observado |
| --- | --- |
| [A — Standard original](estudo-A-1920.png) | Referência de comparação; cores mais neutras e galpão cinza. |
| [B — luz quente e ambiente violeta](estudo-B-1920.png) | Aproxima a separação quente/frio da referência; a sombra do rosto fica escura e os tons do galpão tendem ao lilás. |
| [C — B + shader de rampa](estudo-C-1920.png) | No protagonista e galpão, telhas e madeira ficam mais alaranjadas e com contraste mais gráfico; o rosto continua escuro. Cenário e rochas permanecem em Standard. |
| [D — C + correção discreta](estudo-D-1920.png) | Ajuste de cor global muito sutil; não resolve a legibilidade do rosto. Simulação aplicada ao PNG original, não pós-processamento executado no Unity. |

B e C usaram sol RGB (1, 0,78, 0,58), intensidade 1,18; ambiente céu (0,58, 0,53, 0,72), horizonte (0,48, 0,40, 0,53), solo (0,34, 0,28, 0,40). A posição do sol permaneceu igual. C usou [shader experimental](ExperimentoRampa.shader), com transições `smoothstep` 0,12–0,32 e 0,48–0,75, sombras tingidas de violeta e luz quente. Materiais temporários copiam cor, textura principal, escala e deslocamento; não reproduzem todas as propriedades dos materiais Standard, por isso C mede uma mudança de shader e de resposta de material, não um teste isolado de iluminação. D aplica aos canais de 8 bits, com saturação em 255, R = 1,025R + 2, G = 0,985G + 1, B = 1,015B + 3, antes da ampliação.

Comparação da variante C: [320 × 180](estudo-C-320-1920.png), [480 × 270](estudo-C-480-1920.png), [640 × 360](estudo-C-1920.png), todas ampliadas para 1920 × 1080 por fatores inteiros 6×, 4× e 3×. A montagem de quatro versões é apenas uma prévia; cada imagem individual permite avaliar na mesma saída.

**Avaliação:** B é a base mais simples para continuar, e C merece calibração por aproximar a cor das telhas e madeira. Nenhuma das duas reproduz ainda a referência: a pele está escura, o galpão parece lilás e faltam mar, vegetação e composição equivalentes. A resolução 480 × 270 mostra degraus mais visíveis que 640 × 360 e preserva mais detalhes que 320 × 180 nesta imagem; ainda não há base para escolha final sem movimento e close de diálogo. Não há evidência suficiente para adotar D ou instalar um pacote de pós-processamento.

O [roteiro de captura](ExperimentoCais.cs) cria shader e materiais temporários no editor, renderiza e restaura as alterações em `finally`. O shader compilou sem erro reportado pelo `ShaderUtil`; foi removido do projeto após captura, junto dos materiais temporários. O MCP confirmou cena limpa, sol e material Standard restaurados. Os artefatos do experimento permanecem nesta pasta para reprodução e revisão.

## Limites

As variantes são capturas estáticas fora de Play Mode. Caminhada, diálogo próximo, UI, sombras e cintilação em movimento, pistas e desempenho não foram validados. D é uma simulação externa, e o shader C não é uma implementação pronta para o jogo. O plano existente de comparar dithering, UV afim e encaixe de vértices individualmente também permanece pendente. Nenhuma configuração de produção foi alterada.

## Segunda calibração: luz mais clara

Após o usuário observar que a referência é mais iluminada, repetimos a mesma câmera, pose e shader experimental. O sol passou a RGB (1, 0,90, 0,72), intensidade 1,45; ambiente céu (0,78, 0,76, 0,83), horizonte (0,68, 0,62, 0,68), solo (0,52, 0,47, 0,54). Valores são pontos de experimento, não decisão final. B mantém Standard; C aplica a mesma rampa experimental ao protagonista e galpão. Renderizações em 320 × 180, 480 × 270 e 640 × 360.

- [B clara, 480 × 270](calibracao-clara-B-480-1920.png) e [C clara, 480 × 270](calibracao-clara-C-480-1920.png), ampliadas 4×.
- [B clara, 640 × 360](calibracao-clara-B-640-1920.png) e [C clara, 640 × 360](calibracao-clara-C-640-1920.png), ampliadas 3×.
- [Roteiro de captura](ExperimentoCaisClaro.cs); parâmetros e limitações do shader permanecem os descritos acima.

O personagem e a parede ficaram mais claros e a parede menos violeta na variante C em comparação com a primeira rodada. A cena ainda carece de mar, composição e assets da ilustração, e não garante a mesma aparência da referência. Olhos e expressão não estão suficientemente legíveis neste enquadramento; o ajuste em movimento e em diálogo continua pendente. A cena foi restaurada após o experimento e o editor confirmou ausência de alterações pendentes; nenhuma mudança foi aplicada ao jogo.
