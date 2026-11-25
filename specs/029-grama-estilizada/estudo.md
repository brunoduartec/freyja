# Estudo de grama estilizada e culling

## Direção visual

Em *Breath of the Wild*, a grama dá movimento e volume ao campo. Para Freyja, a proposta é traduzir esse efeito em vegetação costeira baixa, com manchas irregulares e ondas de vento perceptíveis na câmera do jogo. Começaria com dois ou três tufos originais de lâminas largas e afuniladas, de 15 a 35 cm de altura, base oliva próxima da cor do chão e pontas um pouco mais claras. Vale concentrar os tufos nas bordas de trilhas e em manchas sobre o solo, deixando as áreas de circulação e de investigação legíveis. [Referência visual da Nintendo](https://www.youtube.com/watch?v=vLMGrmf4xaY).

O primeiro teste usaria malhas opacas simples e um material compartilhado. O shader inclina as pontas de acordo com a posição no mundo, mantendo as bases fixas. Lâminas largas, contraste moderado e vento lento ajudam a manter a leitura quando a câmera se move. Conferir a imagem na resolução interna real, porque lâminas finas e transições granuladas podem cintilar ao passar de um pixel para outro.

## Referências para o primeiro teste

- **Breath of the Wild — movimento e volume.** Observar os planos abertos e as tomadas próximas do campo. Para Freyja, usar manchas que se inclinam em ondas, com lâminas largas o suficiente para ler na câmera pixelada. [Trailer da Nintendo, E3 2016](https://www.youtube.com/watch?v=1rPxiXXxftE).
- **The Wind Waker HD — composição costeira.** Observar em Outset Island como o caminho claro, a vegetação e o mar se distinguem à distância. Em Freyja, usar esse contraste para delimitar as bordas do caminho e manter acessos legíveis. [Captura da trilha costeira](https://elkinfencer10.weebly.com/uploads/2/5/0/2/25029560/ww2_orig.jpg).
- **Four Million Acres, GDC 2019 — geometria e cor do solo.** O slide *Grass Blades* mostra lâminas de poucos triângulos agrupadas em grade, densidades sobrepostas e cor ligada ao chão. Serve de referência para a continuidade entre tufos próximos e o solo ao fundo. [Slides de Alen Ladavac](https://media.gdcvault.com/gdc2019/presentations/Ladavac_Alen_Four_Million_Acres.pdf).
- **Ghost of Tsushima, GDC 2021 — visibilidade.** O slide *Placing a Grass Blade* mostra os testes de distância, enquadramento e oclusão. No primeiro trecho de Freyja, aplicar distância e enquadramento à célula inteira. A oclusão entra como comparação em área com barreira sólida. [Palestra de Eric Wohllaib](https://gdcvault.com/play/1027033/) · [Slides da palestra](https://archive.thedatadungeon.com/ghost_of_tsushima_2020/documents/gdc_2021/gdc_2021_procedural_grass_in_got.pdf).

## Opções no Unity 6.3 Built-in

| Opção | Onde faz sentido | Custo a verificar |
| --- | --- | --- |
| **Malhas instanciadas por células** | Proposta principal. Funciona em superfícies Terrain e em malhas da ilha. Dá controle sobre distância, densidade, silhueta e vento. | Células grandes mantêm grama fora da vista dentro de lotes visíveis. Células pequenas aumentam testes e chamadas de desenho. [Unity — RenderMeshInstanced](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html) |
| **Terrain Details com GPU Instancing** | Opção de menor esforço de edição se o trecho escolhido estiver todo sobre Unity Terrain. Permite pintar com pincel e usar material próprio para cor e vento. | A distribuição fica vinculada ao Terrain. Vale comparar a transição de densidade e o custo com a opção por células. [Unity — Grass and other details](https://docs.unity3d.com/6000.3/Documentation/Manual/terrain-Grass.html) |
| **Instanciamento indireto com culling na GPU** | Opção para reavaliar se o perfil apontar gargalo de CPU no envio de instâncias ou na seleção de células. | Exige controle de buffers, shader e validação na API gráfica alvo. [Unity — RenderMeshIndirect](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshIndirect.html) |

## Culling proposto

1. **Distribuição em células.** Espalhar tufos de forma determinística em áreas de solo autorizadas, respeitando inclinação e máscaras de exclusão. Começar com células de 8 m e comparar com 4 m e 16 m. Reutilizar as posições e agrupar instâncias por célula, malha e material. O Unity testa os limites de cada lote como uma unidade. [Unity — RenderMeshInstanced](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html).
2. **Enquadramento.** Usar o volume visível da câmera para excluir células inteiramente fora da vista antes de enviá-las para desenho. Dar margem nos limites para incluir a altura dos tufos e o deslocamento máximo do vento.
3. **Distância e nível de detalhe.** Usar a câmera do jogo como referência e calibrar pelo tamanho que a grama ocupa na tela. Conservar posições entre níveis e usar subconjuntos estáveis de tufos, para evitar que o campo mude de padrão enquanto a câmera anda.

| Distância inicial | Aparência proposta |
| --- | --- |
| Até ~12 m | Tufos completos e vento visível. |
| ~12 a 28 m | Menos tufos, com silhuetas mais simples. |
| ~28 a 35 m | Redução progressiva dos tufos restantes. |
| Além de ~35 m | A cor do solo dá continuidade ao campo. Células inteiramente além do alcance saem do envio. |

4. **Oclusão estática como comparação.** A cena local tem componentes Terrain e a câmera permite oclusão, mas o arquivo da cena não referencia dados de bake. Medir o ganho ao cruzar um muro, uma construção ou um costão sólido. Com bake apropriado, `CullingGroup` pode consultar a visibilidade de volumes que contêm as células, incluindo oclusão estática. Medir o custo de CPU junto com o ganho, especialmente na vista aberta. [Unity — CullingGroup](https://docs.unity3d.com/6000.3/Documentation/Manual/CullingGroupAPI.html), [Unity — Occlusion Culling](https://docs.unity3d.com/6000.3/Documentation/Manual/OcclusionCulling.html).

## Como avaliar o primeiro trecho

Escolher uma borda de trilha com vista aberta e, se houver nas proximidades, uma curva com obstáculo sólido. Fixar câmera, iluminação, densidade inicial, resolução e máquina. Comparar três passagens com o mesmo trajeto: cena sem grama, grama com todos os grupos enviados no detalhe máximo e grama com culling e níveis de detalhe. Depois alternar cada otimização individualmente para identificar a origem do ganho. Para medir o baseline sem culling, contar o que é enviado e considerar que o Unity ainda pode rejeitar lotes por seus limites.

Registrar quantidade de células e instâncias enviadas, chamadas de desenho, mediana e p95 do tempo de quadro. Começar sem projeção de sombra por tufo e verificar se a iluminação do solo e do material dá contraste suficiente. Usar o Frame Debugger para conferir os lotes e o Profiler para a CPU. Em macOS com Metal, usar as ferramentas de GPU do Xcode para medir o custo na GPU. Comparar um trecho com vento e câmera em movimento para detectar cintilação, transições bruscas e falhas nas bordas da tela. [Unity — GPU Usage Profiler](https://docs.unity3d.com/6000.3/Documentation/Manual/ProfilerGPU.html).

O estudo foi feito a partir dos arquivos locais e da documentação. Os alcances e a meta de desempenho são pontos de partida. A cena principal existe como arquivo local no checkout de trabalho e ainda precisa estar acessível na etapa de validação no editor.
