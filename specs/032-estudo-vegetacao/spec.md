# Estudo de vegetação da ilha

**Branch** `032-estudo-vegetacao` · **Data** 18/11/2026 · **Estado** proposta para avaliação

## Cenário de avaliação

O responsável pelo jogo compara as regiões da ilha, escolhe a paleta vegetal e ajusta as faixas de densidade. O eixo cais–praça–igreja concentra a primeira avaliação de composição, do chão quase exposto na chegada aos pequenos núcleos floridos junto aos edifícios. A faixa de banho fica aberta e as copas se concentram no interior abrigado.

## Requisitos e critérios de avaliação

- **RF-01** Cada região deve ter uma paleta de plantas, um alvo de cobertura baixa e uma quantidade inicial de arbustos, flores ou árvores aplicável na escala humana.
- **RF-02** Os acessos e as linhas de visão do cais, da praça, da igreja e do mirante devem ficar legíveis. A vegetação acompanha as bordas das rotas e deixa livres soleiras, degraus, pistas e interações.
- **RF-03** A paleta deve partir dos modelos locais do Nature e aproveitar o estudo de grama costeira da spec 029. As flores têm cor concentrada em poucos pontos. O fundo verde-oliva deixa terreno, fachadas e costões à vista.
- **RF-04** Uma avaliação posterior na câmera em movimento compara dois níveis de cobertura no mesmo trecho do eixo, confirma a leitura dos marcos e observa cintilação nas lâminas e pétalas.

### Distribuição proposta

**Cobertura** é a projeção de capins, ervas e plantas rasteiras sobre o solo plantável de cada região, com um exemplo de referência de 10 × 10 m. Água, rocha nua, calçamento, caminhos, construções e área de banho ficam fora da área plantável. Arbustos e moitas por área usam a mesma base de 100 m². Densidades são faixas de partida e podem ser concentradas em manchas dentro da região. Uma moita florida reúne 2–4 tufos próximos. Copas são contadas à parte.

| Região | Plantas, flores e distribuição | Cobertura baixa | Quantidade inicial |
| --- | --- | ---: | --- |
| Cais e galpão | Capim baixo verde-oliva e ervas nas juntas entre terra e pedra, em bolsões do lado abrigado do galpão. | 5–10% | 0–1 arbusto baixo/100 m². Zero árvores e moitas floridas. |
| Praia de banho e costão | Na faixa seca posterior e nas fendas com terra, capim curto e rasteiras de folha firme. Deixar livre toda a faixa usada para banho e circulação. | 0% no banho e 5–12% no solo plantável atrás da praia e no costão | 0–1 arbusto baixo/100 m² abrigados. Avaliar 1 coqueiro isolado na borda seca atrás da praia. Zero moitas floridas. |
| Subida do cais e trilhas | Capim em manchas descontínuas, plantas de folha larga junto a muros e uma moita clara em curvas abertas. | 18–30% | 2–4 arbustos/100 m². 0–1 moita florida a cada 10 m lineares de trecho. Copas apenas nas encostas adjacentes. |
| Praça, venda e sala comunitária | Tufos curtos e flores creme ou amarelo suave em canteiros periféricos. Uma copa larga lateral como ponto de sombra. | 10–18% | 2–3 moitas floridas por canteiro selecionado, até dois canteiros. 0–1 árvore na praça inteira. |
| Casas e quintais | Ervas e folhagens médias em quintais, ervas baixas nas soleiras laterais e flores creme ou coral suave em algumas entradas. | 20–35% | 1–2 arbustos e 0–2 moitas floridas por quintal. 1 árvore de copa larga a cada 3–4 quintais com espaço. |
| Igreja e adro | Capim curto e folhagem baixa junto a muros laterais, com pequenas moitas creme. Manter abertas as vistas da escadaria, fachada e torre. | 8–15% | 0–1 arbusto baixo/100 m². 2–3 moitas floridas no adro inteiro. Zero árvores no adro. |
| Hortas e encosta cultivada | Folhas largas em fileiras, com milho pontual no fundo dos canteiros. Corredores de terra entre as linhas. | 35–50% dentro dos canteiros | 2–4 fileiras por canteiro, com 2–3 plantas por metro de fileira. Até 1 fileira de milho por canteiro. Zero árvores dentro dos canteiros. |
| Encostas e fundos da vila | Capim irregular, arbustos oliva agrupados e poucas copas largas em depressões abrigadas. | 30–45% | 4–7 arbustos e 0–1 moita florida/100 m². 1–2 árvores/100 m² nos bolsões abrigados. |
| Ruína e acesso à casa afastada | Ervas junto às pedras e trechos de capim entre solo exposto, com arbustos pontuais atrás de muros. Manter legíveis a entrada, as pistas e o isolamento da casa. | 20–35% | 2–4 arbustos/100 m². 0–1 árvore no fundo de cada local. Zero moitas floridas junto às pistas. |
| Crista e mirante | Capim rente e pequenas rasteiras em manchas baixas, com fundo de rocha e horizonte abertos. | 8–15% | 0–1 arbusto baixo/100 m². Zero árvores e moitas floridas no mirante. |

**Referências de altura e margem.** Capim de 10–20 cm no cais, adro e crista, e de 15–35 cm nas encostas, em linha com a spec 029. Flores de 20–35 cm, arbustos de 35–70 cm e copas largas de 4–5,5 m. Reservar 30 cm a partir da borda do piso jogável para tufos, 60 cm para a projeção dos arbustos e 1 m livre em torno das interações externas. Nas passagens estreitas, a faixa livre tem prioridade sobre a meta de cobertura. Plantar em manchas de bordas irregulares com solo visível entre elas, especialmente nas transições entre ambientes.

### Seleção do pack Nature

| Papel | Arquivos conferidos em `Low Poly World/downloads/nature` | Uso proposto |
| --- | --- | --- |
| Capim costeiro | `Grass_Short.blend`, `Grass.blend`, `Grass_2.blend` | Comparar os tufos do pack com os largos e baixos da spec 029. |
| Rasteiras e folhagens | `Plant_1.blend` a `Plant_5.blend` | Selecionar as silhuetas mais baixas para fendas, muros e bordas de trilha. As variantes de folha larga podem sugerir hortaliças sem espécie definida. |
| Arbustos | `Bush_1.blend`, `Bush_2.blend` e `BushBerries_1.blend` | Duas formas básicas para encostas e quintais. Testar a variante com bagas em poucos quintais. |
| Flores | `Flowers.blend` | Candidato para moitas creme, amarelo suave e coral. Cores propostas para a composição, sujeitas à revisão do material. |
| Copa e cultivo | `CommonTree_1.blend`, `CommonTree_2.blend`, `Corn_1.blend`, `Corn_2.blend` | Árvores nos pontos de sombra e milho no fundo das hortas. `CommonTree_1` já tem uma adaptação local em `Assets/ThirdParty/Nature022/Nature_Common.fbx`. |

### Coqueiro para a praia

O pack traz quatro palmeiras. A inspeção das malhas no Blender confirmou cocos modelados em `PalmTree_3.blend` e `PalmTree_4.blend`. A variante 4 tem tronco curvo e é a primeira candidata para um exemplar isolado na borda seca, fora da faixa de banho e do acesso. A variante 3 tem tronco mais reto. A cobertura baixa proposta continua em 5–12% na retaguarda e 0% na faixa de banho.

[Comparação das quatro palmeiras, com cores de inspeção](palmeiras-nature.png)

## Critérios de sucesso

- **CS-01** As dez regiões apresentam faixas de cobertura e quantidades em unidades explícitas. A praia de banho e os eixos visuais do cais, igreja e mirante têm regras de espaço livre.
- **CS-02** Cada tipo vegetal proposto aponta para um arquivo local conferido ou para a grama da spec 029. O quadro visual representa as mesmas faixas da tabela.
- **CS-03** O recorte da próxima avaliação visual cabe em uma caminhada cais–praça–igreja, em duas variantes de densidade com o mesmo percurso e enquadramento.

## Premissas e escopo

- As quantidades são propostas de direção de arte. Usam densidade local porque a implantação compacta da spec 030 altera as áreas disponíveis. Hortas, ruína e mirante seguem o programa do GDD, com posição a confirmar na implantação mais recente.
- O litoral ensolarado, a vegetação baixa e a leitura clara das pistas vêm do GDD. O estudo cobre composição e seleção de candidatos. A aparência e os materiais dos candidatos ainda dependem de avaliação visual na câmera do jogo.
- A entrega atual é documental e visual. O primeiro teste em cena depende da aprovação desta proposta.

**Referências do projeto** · `GDD.md`, specs 016, 029 e 030 e `docs/KitPS1_022/README.md`.

[Quadro visual de densidade por região](densidade.png)

[Estudo de posição e densidade da grama](estudo-grama.md)
