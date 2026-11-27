# Freyja — melhoria da vegetação e ambientação da ilha

## Objetivo

Melhore a vegetação do cenário jogável de Freyja no Unity, utilizando prioritariamente os modelos, prefabs, materiais e texturas já existentes na pasta **Low Poly Game** (localize o nome e o caminho reais no projeto).

Quero uma ilha mais viva, natural e visualmente rica, com flores, plantas costeiras, arbustos, gramíneas ornamentais e mais árvores, sem descaracterizar a estética low-poly estilizada do jogo. A vegetação deve complementar a arquitetura, reforçar a identidade costeira e criar variedade entre as diferentes regiões do mapa.

A tarefa é de implementação efetiva no cenário, não apenas de planejamento ou criação de scripts. Ao final, as melhorias devem estar presentes na cena realmente utilizada pelo jogo.

## Referências de direção de arte e regras adicionais (estudo de vegetação)

Use estas referências como princípios de composição, não como exigência de copiar modelos, texturas ou código de terceiros. Não é necessário navegar pelos links durante a implementação: a síntese abaixo contém as decisões relevantes.

- **Alba: A Wildlife Adventure** — ilha mediterrânea, massas de árvores, campos de flores e relação entre natureza e vila. [Artigo da environment artist](https://ustwogames.co.uk/news/the-environment-art-of-alba-a-wildlife-adventure/).
- **A Short Hike** — mundo compacto, leitura de trilhas e silhuetas pela câmera distante, alternância de áreas densas e abertas. [Relato do desenvolvedor](https://blog.playstation.com/2021/08/05/crafting-a-tiny-open-world-a-look-behind-the-scenes-at-the-creation-of-a-short-hike/).
- **Link’s Awakening** — composição de diorama, flores maiores e elementos arquitetônicos legíveis. [Entrevista Nintendo](https://www.nintendo.com/en-gb/News/2019/October/Discover-what-graphic-refining-director-Yoshiki-Haruhana-aimed-for-with-the-visual-style-of-The-Legend-of-Zelda-Link-s-Awakening--1656228.html).
- **Dredge** — contraste entre vila acolhedora e litoral desolado; utilizar silhuetas e vegetação irregular nas regiões de mistério.
- **Zoneamento costeiro real** — [Coastal Restoration Trust](https://monitoring.coastalrestorationtrust.org.nz/about/zonation-vegetation/): areia exposta, gramíneas pioneiras, cobertura rasteira, arbustos e árvores mais protegidas. Adaptar à geografia fictícia sem simular ecologia.
- **Ghost of Tsushima** — [relato da equipe de VFX](https://blog.playstation.com/2021/01/12/how-stunning-visual-effects-bring-ghost-of-tsushima-to-life/): utilizar um campo de vento compartilhado entre vegetações, sem reproduzir o pipeline AAA.

### Gramática de composição

- Camada alta: árvores dominantes; camada média: arbustos e árvores pequenas; camada baixa: tufos, flores, cobertura rasteira. Trabalhe as três escalas em conjuntos, sem preencher todo espaço vazio.
- Paleta proposta: verde oliva, verde sálvia, verde profundo, palha, flores creme/amarelo suave e lilás. Harmonizar com materiais existentes e luz do jogo; não forçar recoloração global nem adicionar variantes de material desnecessárias.
- Posicione primeiro árvores marcantes manualmente (praça, igreja, transição de trilha e mirantes, se existentes), depois massas secundárias, e por fim flores e gramíneas. Preserve os eixos visuais na câmera real.
- Litoral: areia molhada e água sem plantas; tufos espaçados na areia seca; manchas de rasteiras perto de rochas; arbustos atrás da praia; árvores nas áreas protegidas. Não introduzir palmeiras automaticamente.
- Vila: jardins diferentes entre casas; região pública mais cuidada; áreas privadas e fundos de casas com variação; ruas, escadas e pontos de interação desobstruídos.
- Ruínas: massas irregulares, cor mais contida, vegetação junto às bordas e muros sem esconder pistas. Não transformar vegetação decorativa em mecânica ou modificar a narrativa.
- Plantação: manter separação visual entre culturas interativas e plantas decorativas.

### Parâmetros iniciais de composição (hipóteses para ajustar após inspeção)

| Parâmetro | Ponto de partida |
|---|---|
| Distância mínima entre árvores médias | 4–7 m, ajustando pelo diâmetro real da copa |
| Grupos naturais de árvores | 3–7 árvores com variação e aberturas |
| Grupos de flores | 3–9 tufos, 1–2 cores predominantes por conjunto |
| Escala uniforme de variantes de plantas | 0,8–1,2 vezes a escala normalizada |
| Margem de trilhas | 0,3–0,8 m além do limite útil, ajustada à largura real e ao raio do agente |
| Cobertura visual alvo da vegetação na área natural | 40–65% da superfície apta, incluindo copas; não uma meta de contagem de objetos |
| Cobertura visual alvo na areia seca apta | 5–15%, preservando a faixa de areia exposta |

Todos os valores são parâmetros experimentais, não critérios rígidos. Não aplique buffers que inutilizem áreas pequenas, nem force espécies indisponíveis. Para espalhamento, caso necessário, combine máscara de área permitida + ruído de baixa frequência para manchas + espaçamento mínimo entre instâncias (amostragem simples com rejeição ou Poisson). Não aplique Poisson uniforme em toda a ilha. Faça geração no Editor, com seed fixa e possibilidade de ajustes manuais.

### Validação visual e técnica adicional

- Use um pequeno trecho piloto que contenha a transição praia/vila antes de distribuir pelo mapa inteiro. Compare na câmera real. Se estiver legível e coerente, estenda aos demais setores.
- Use pontos fixos de captura na praia, entrada da vila, praça, encosta, plantação e ruínas, quando existentes; verifique também o efeito das copas sobre luzes noturnas e o personagem.
- Se houver URP, lembre-se de que SRP Batcher, GPU instancing convencional e GPU Resident Drawer têm condições distintas. Não desligue o SRP Batcher nem altere o pipeline global sem demonstrar ganho. [Unity GPU instancing](https://docs.unity.cn/Manual/GPUInstancing.html), [GPU Resident Drawer: performance](https://docs.unity.cn/Manual/urp/gpu-resident-drawer-performance.html), [Terrain Details](https://docs.unity.cn/Documentation/Manual/terrain-Grass.html).
- Use materiais iluminados compatíveis com o ciclo de dia/noite; vento preferencialmente por deslocamento de vértices com base fixa e parâmetros compartilhados. Não criar material exclusivo por planta nem Update individual. Se usar cards, preferir alpha clip quando compatível, verificando overdraw e cintilação na resolução real.
- Como hipótese inicial de orçamento, busque regressão de frame time de no máximo 10% no trecho de maior densidade, sem comprometer a meta de FPS já existente. Registre condições, CPU/GPU e limitações; não declare aprovação apenas por atingir esse número.
- Não adicionar fauna, partículas, sistema de crescimento, coleta, mudanças narrativas ou importações externas nesta tarefa.

## 1. Inspeção inicial

- Leia o `AGENTS.md` e as instruções de desenvolvimento existentes. Utilize o fluxo Spec Kit e os agentes de Game Design, Implementação e Validação, caso estejam configurados, mantendo o escopo pequeno e evitando duplicação de trabalho.
- Identifique a cena carregada ao iniciar o jogo, o terreno, as regiões da ilha, as áreas de navegação, a vegetação existente e os sistemas de iluminação, vento e gerenciamento do mundo.
- Localize a pasta Low Poly Game e catalogue somente os assets relevantes para a tarefa: árvores, flores, arbustos, tufos de grama, plantas costeiras e variantes. Priorize prefabs já configurados e reutilize seus materiais quando compatíveis.
- Capture referências visuais do estado atual a partir da câmera real do jogador, preferencialmente em pontos representativos da ilha.
- Não leia ou importe indiscriminadamente todos os assets do projeto. Não use Tripo AI, serviços pagos, downloads externos nem gere novos modelos 3D.

## 2. Direção artística e distribuição

A distribuição precisa parecer intencional e orgânica, não uma dispersão uniforme de objetos aleatórios. Preserve a leitura do cenário na câmera isométrica e a atmosfera de mistério de Freyja.

| Região | Vegetação desejada | Direção de composição |
|---|---|---|
| Praia e litoral | Tufos de gramíneas costeiras, plantas baixas resistentes, arbustos rasteiros e flores discretas | Concentrar na transição entre areia seca e terra, nas bordas da praia e ao redor de rochas; deixar a areia molhada e a zona de ondas livres |
| Caminhos da vila | Flores silvestres, tufos de grama e pequenos arbustos | Agrupar nas bordas, junto a cercas e muros; preservar toda a largura útil dos caminhos |
| Casas e jardins | Flores de cores variadas, pequenos arbustos e árvores ornamentais compatíveis com o pacote | Criar pequenos conjuntos assimétricos, evitando repetir a mesma composição em todas as casas |
| Praça e edifícios públicos | Árvores pontuais, flores e canteiros quando houver espaço | Valorizar marcos arquitetônicos e preservar áreas de circulação e encontros |
| Encostas e áreas naturais | Árvores de portes variados, arbustos e vegetação de sub-bosque | Criar núcleos mais densos, intervalos e clareiras; respeitar a inclinação do terreno |
| Áreas agrícolas | Vegetação baixa nas bordas e árvores esparsas fora das zonas produtivas | Não ocupar lotes de plantação, áreas de interação ou rotas de trabalho dos NPCs |
| Ruínas e áreas misteriosas | Vegetação irregular e discreta, arbustos e árvores estrategicamente posicionados | Reforçar a atmosfera sem esconder pistas, entradas ou elementos narrativos |

Adapte as categorias às regiões que realmente existirem na cena. Se não houver modelos específicos de plantas costeiras, selecione os assets existentes de silhueta e porte mais apropriados; não invente uma dependência ou substitua por plantas tropicais incompatíveis com a direção artística.

### Flores e plantas pequenas

- Crie pequenos agrupamentos naturais, com variação de espécie, tamanho, orientação e espaçamento.
- Evite padrões de grade, linhas artificiais em áreas naturais, cores excessivamente saturadas e repetição evidente.
- Use flores como pontos de interesse visual e contraste, não como cobertura uniforme de toda a ilha.
- Priorize áreas próximas a caminhos, jardins e transições entre tipos de solo.

### Árvores

- Aumente a presença de árvores especialmente em áreas naturais, encostas e espaços vazios que não tenham função de gameplay.
- Combine espécies e portes existentes no pacote, variando a rotação em Y e a escala uniforme dentro de limites coerentes com o asset e a escala do mundo.
- Não bloqueie a vista do personagem, das portas, da igreja, do cais, dos marcos do mapa ou dos objetos interativos.
- Evite copas sobrepostas de forma excessiva e aglomerações que escureçam caminhos importantes, inclusive à noite.

## 3. Implementação no Unity

- Preserve a cena, os sistemas existentes e todas as alterações não relacionadas à tarefa. Não reconstrua o terreno, o mapa ou o World Builder.
- Reutilize o pipeline de distribuição de vegetação existente, se houver. Caso não exista, use a solução mais simples que permita posicionar, ajustar e reproduzir a distribuição com segurança no Editor.
- Combine posicionamento manual de pontos importantes com distribuição procedural determinística apenas onde fizer sentido, usando seed fixa, densidade por região, distância mínima entre instâncias e máscaras de exclusão.
- Se for necessária uma ferramenta de Editor, mantenha-a pequena, com geração explícita, possibilidade de desfazer e regeneração idempotente. Não execute geração automática em todo Play Mode.
- Separe as novas instâncias por região ou categoria em uma hierarquia organizada, ou siga a estrutura já utilizada pelo projeto. Não apague a vegetação anterior indiscriminadamente.
- Posicione os assets sobre o terreno real, respeitando o pivot, a base dos modelos, a inclinação e o nível de água. Árvores devem permanecer naturalmente verticais; plantas pequenas podem acompanhar moderadamente a superfície.
- Use exclusões para edifícios, interiores, estradas, escadas, portas, cais, áreas de interação, cultivos, spawn points, rotas de NPCs, pistas e água. Não cubra vegetação funcional com vegetação decorativa.
- Se houver NavMesh ou outro sistema de navegação, preserve a conectividade. Configure obstáculos ou colliders somente nos elementos que efetivamente precisam bloquear passagem, como troncos relevantes. Flores, folhagens e gramíneas decorativas não devem impedir a movimentação.
- Respeite o pipeline de renderização do projeto. Verifique compatibilidade de materiais e shaders antes de converter qualquer asset. Preserve a aparência do pacote e integre ao vento global e à iluminação dinâmica existentes quando houver suporte adequado, sem criar um novo sistema de vento.
- Não altere a câmera, o ciclo dia/noite, o shader de água ou a lógica de gameplay para resolver problemas de distribuição de vegetação. Se houver conflito, ajuste a vegetação primeiro.

## 4. Performance

Freyja deve continuar leve. A melhoria visual não justifica uma queda significativa de desempenho.

- Reutilize meshes e materiais compartilhados. Evite material instanciado individualmente por objeto.
- Para vegetação repetitiva, aproveite as técnicas que já forem compatíveis com o projeto: GPU instancing, Terrain Details, LODs, culling ou batching. Não presuma que todas são compatíveis entre si ou que qualquer uma delas seja automaticamente mais rápida.
- Utilize Terrain Details para elementos pequenos apenas se a solução respeitar a estética, o shader e a organização existentes. Prefira prefabs para árvores e conjuntos que exigem controle individual.
- Controle a quantidade de instâncias e a distância de renderização por categoria. Evite sombras em objetos pequenos quando o benefício visual for insignificante e preserve sombras relevantes das árvores.
- Não adicione Update por instância nem scripts de comportamento desnecessários em plantas decorativas.
- Meça antes e depois nas mesmas condições (câmera, resolução, horário, configuração gráfica e trecho do mapa), observando frame time CPU/GPU, batches/draw calls, memória e quantidade de instâncias quando disponíveis. Se uma métrica não puder ser coletada, declare a limitação em vez de inventar valores.
- Se houver regressão significativa, reduza densidade, sombras, distância de desenho ou custo dos assets antes de finalizar. Não sacrifique a legibilidade nem esconda a regressão.

## 5. Validação obrigatória

Não considere a tarefa concluída apenas porque o código compila ou os prefabs existem.

1. Abra a cena efetivamente jogável e execute o Play Mode. Confirme que a vegetação aparece na Game View, incluindo o fluxo de câmera/render texture existente, se aplicável.
2. Compare capturas antes/depois, usando os mesmos enquadramentos da câmera do jogo em áreas costeiras, vila, caminhos e áreas naturais que existirem.
3. Verifique pelo menos um horário diurno e um noturno quando houver ciclo dia/noite, observando sombras, legibilidade dos caminhos e materiais incorretos.
4. Valide árvores e plantas flutuando, enterradas, dentro de construções, na água ou atravessando objetos de maneira inadequada. Inspecione especialmente encostas e transições de terreno.
5. Percorra caminhos, entradas, escadas e áreas interativas representativas. Verifique movimentação do jogador e navegação dos NPCs quando disponíveis.
6. Confirme que os modelos vieram do pacote solicitado e que não existem erros novos no Console, referências quebradas ou materiais magenta.
7. Compare as métricas de desempenho e revise o diff do Git. Garanta que não foram introduzidas alterações fora do escopo.

Se o agente validador ou as ferramentas de automação não puderem abrir a cena, jogar ou capturar imagens, registre explicitamente quais verificações ficaram pendentes. Não declare aprovação visual sem evidência real.

## 6. Critérios de aceite

- [ ] A cena jogável apresenta mais árvores e diversidade de vegetação em relação ao estado inicial.
- [ ] Flores e plantas de aparência costeira foram distribuídas nas regiões apropriadas usando os assets existentes.
- [ ] A ilha ganhou composição orgânica, com áreas densas e clareiras, sem aparência de distribuição aleatória uniforme.
- [ ] Arquitetura, estilo visual, pistas, caminhos, plantações e interações foram preservados.
- [ ] A vegetação está corretamente apoiada no terreno e não invade áreas proibidas.
- [ ] Não houve erros novos de compilação, materiais incompatíveis ou referências quebradas.
- [ ] A validação visual e de gameplay foi executada ou suas limitações foram explicitamente registradas.
- [ ] O impacto de performance foi medido quando tecnicamente possível e eventuais regressões foram tratadas.
- [ ] Nenhum novo asset externo ou crédito de geração foi utilizado.

## 7. Entrega final

Implemente a melhoria e salve as alterações na cena e/ou estruturas de dados utilizadas pelo jogo. Entregue um relatório curto com:

- assets utilizados e respectivos caminhos;
- quantidade aproximada de novas instâncias por categoria e regiões alteradas;
- cenas, prefabs, scripts e demais arquivos modificados;
- capturas de antes/depois, quando disponíveis;
- resultados dos testes de gameplay e performance;
- limitações, problemas pendentes e decisões relevantes.

Use os agentes já configurados de forma proporcional: Game Designer define as regras de distribuição e composição; Implementador aplica as mudanças; Validador verifica visualmente e testa o resultado. Evite rodadas de análise redundantes, logs extensos e leitura integral desnecessária de arquivos. Não crie agentes, frameworks ou sistemas novos apenas para esta tarefa.

Execute a implementação agora. Não encerre a tarefa entregando somente uma especificação, um plano ou recomendações.
