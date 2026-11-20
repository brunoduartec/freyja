# Pesquisa de renderização

## Referências

- [Personagens no cais](../../Referencias/ilha-dos-trinta-personagens.png): referência apontada pelo usuário como a mais próxima da meta. Apresenta personagens grandes e expressivos, olhos e sobrancelhas legíveis, contornos facetados, detalhes em blocos, luz quente, sombras violáceas e contraste com o mar turquesa. É uma ilustração de referência, não evidência de um renderer em tempo real ou de parâmetros técnicos.
- [A Short Hike, página oficial](https://store.steampowered.com/app/1055540/A_Short_Hike/): referência visual de ilha 3D com aparência pixelada. A página permite observar a imagem, mas não comprova o método interno usado pelo jogo.
- [Return of the Obra Dinn, diário de desenvolvimento de Lucas Pope](https://obradinn.01101101.fr/): o criador relata testes em 640 × 360, ampliação e dificuldades para reconhecer rostos com imagem monocromática e dithering. Trata-se de uma direção muito mais restritiva que a de Freyja; é evidência do compromisso entre estilo e legibilidade, não uma receita a copiar.
- [Experimento de David Holland](https://www.davidhol.land/articles/3d-pixel-art-rendering/): relata instabilidade temporal na câmera 3D e uma estratégia de alinhamento de grade com compensação. Essa solução avançada é uma alternativa, não uma necessidade comprovada para a cena de Freyja.
- [Estudo acadêmico de renderização 3D pixel art](https://upcommons.upc.edu/server/api/core/bitstreams/8b9fd8f2-f027-48a8-af95-b96b782a4310/content): discute resolução interna, ampliação e filtragem em um protótipo; não determina automaticamente a melhor resolução para Freyja.
- [Unity: Camera.targetTexture](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-targetTexture.html): a câmera pode renderizar para uma textura; ao fazê-lo, ocupa a textura inteira.
- [Unity: FilterMode](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/FilterMode.html): Point mantém texels em blocos; bilinear e trilinear misturam amostras.

## Alternativas e hipótese

| Alternativa | Vantagens | Custos e riscos |
| --- | --- | --- |
| Resolução nativa e texturas pequenas com Point | Detalhes e silhuetas finas preservados; materiais facetados | Texturas pixeladas não tornam as silhuetas pixeladas; aparência inconsistente com a constituição |
| Cena inteira em resolução muito baixa, ampliada com Point | Pixels evidentes e linguagem uniforme | Rostos, pistas e linhas finas podem desaparecer; maior cintilação em movimento |
| Cena em resolução intermediária, ampliada com Point; texturas pequenas seletivas e interface separada | Pixels visíveis, com margem maior para pistas e texto | Exige calibrar resolução e escala de saída; ainda pode cintilar; UI não segue automaticamente a grade do mundo |

**Hipótese de trabalho**: terceira alternativa, começando por 640 × 360 em 16:9 e comparando resoluções próximas. A resolução é provisória e depende de captura e avaliação na câmera do jogo. Ampliação inteira evita pixels de tamanhos diferentes; saídas incompatíveis exigem faixas ou outra escolha documentada. A filtragem das texturas de modelos é uma escolha independente da ampliação final, e não deve ser alterada em massa sem inspeção.

## Experimento na cena — 14/11/2026

Unity 6000.3.25f1, Built-in, cena `Ilha_Tripo_Caminhada_011`, câmera `Cais Gameplay Camera`, perspectiva, FOV vertical 58°, posição (0, 5,81, 0,74), rotação (18,63°, 0°, 0°). HDR e MSAA da câmera desativados; qualidade global informa antialiasing 2×. Capturas fora do Play Mode, pela câmera do Unity, sem interface. Cada resolução foi renderizada diretamente pela câmera; os PNGs foram ampliados depois por vizinho mais próximo. Não é uma configuração de renderer em execução no jogo.

| Captura | Observação |
| --- | --- |
| [1280 × 720 nativa](base-1280.png) | Mais detalhe, bordas finas; pixels da imagem pouco perceptíveis. |
| [640 × 360, ampliada 3×](proposta-640x360-1920x1080.png) | Silhueta e acessórios do protagonista, telhado, pedras e cais distinguíveis; bordas em degraus visíveis. Proposta para avaliação. |
| [320 × 180, ampliada 6×](proposta-320x180-1920x1080.png) | Blocos mais fortes, mas mãos, cabelo e detalhes do galpão se perdem mais. |

O protagonista aparece de costas, e a placa não é uma pista legível: esta cena não permite concluir legibilidade de rostos ou pistas. O céu aparece como fundo azul; não se deve inferir que seja o mar final. Comparação subjetiva em imagens estáticas, não um teste com jogadores.

**Captura adicional:** [protagonista de frente](rosto-camera-1920x1080.png), na mesma câmera e 640 × 360 ampliados 3×. O personagem foi girado temporariamente em 180° e restaurado após a captura; a cena permaneceu sem alterações pendentes. O rosto é visível, mas pequeno nesse enquadramento, e detalhes de expressão ainda são difíceis de julgar; pistas e movimento continuam sem validação.

**Direção informada pelo usuário:** a imagem de personagens no cais é a referência mais próxima da meta. A captura atual fica distante dela: personagem e rosto ocupam uma área muito menor, o chão e os modelos têm texturas mais suaves, a paleta é diferente e a cena ainda não apresenta o mar e a composição da referência. A diferença envolve câmera, assets, materiais e iluminação além da resolução; não se pode atribuí-la somente ao renderer. A recomendação de 640 × 360 continua provisória. O GDD também pede testar 480 × 270, além de 320 × 180 e 640 × 360, em caminhada e diálogo próximo; 480 × 270 e esse diálogo ainda não foram testados.

**Enquadramento no cais solicitado pelo usuário:** [captura ampliada](cais-referencia-1920x1080.png), render em [640 × 360](cais-referencia-640x360.png). Protagonista temporariamente em (-3,35; 1,84; 22,65), rotação Y 110°; câmera em (-0,9; 3,35; 24,7), apontada para (-4,05; 2,85; 21,7), FOV 42°. A cena e câmera foram restauradas sem alterações pendentes. É uma aproximação de enquadramento com os assets atuais, não uma reprodução da composição completa da ilustração.

A inspeção encontrou `Galpao_Madeira`, `Galpao_Terracota`, `Galpao_Lajotas` e `Galpao_Pedra` em 128 × 128 com Point; `Cenario_BaseColor` e `Protagonista_BaseColor` em 2048 × 2048 com Bilinear. Dimensões são as reportadas pelo Unity para texturas carregadas, não uma regra de produção aprovada. O teste manteve os filtros existentes.

Foram geradas saídas [1280 × 720](proposta-640x360-1280x720.png), [1920 × 1080](proposta-640x360-1920x1080.png) e [1280 × 960 com faixas](proposta-640x360-1280x960.png) a partir do mesmo render 640 × 360. Suas dimensões foram verificadas; as faixas são uma simulação estática de apresentação, não teste no jogador. Movimento, cintilação, interface e rostos/pistas reais não foram validados: dependem de um renderer jogável e de conteúdo adequado. A captura demonstra a proposta, não prova desempenho ou estabilidade.
