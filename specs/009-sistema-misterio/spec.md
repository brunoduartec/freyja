# Feature Specification: Sistema de mistério e investigação

**Feature Branch**: `main` (sincronização inicial); identificador da feature: `009-sistema-misterio`.
**Created**: 2026-11-13
**Status**: Planejamento do primeiro MVP web autorizado pelo usuário. Plano e tarefas devem ser concluídos antes da implementação; nenhuma implementação foi autorizada nesta etapa.
**Input**: Investigar referências e propor pistas, conhecimento dos moradores e investigação com mortes variáveis.

## Clarifications

### Session 2026-11-13

- Q: Quando você fala em personagens morrerem aleatoriamente, como imagina isso funcionando? → A: Sorteamos quem será possuído. A pessoa morre apenas se o jogador não conseguir salvá-la. Não há mortes inevitáveis sorteadas neste sistema.
- Q: Se uma morte reduzir a população de 30 para 29 pessoas, deve haver uma nova possessão no dia seguinte? → A: A condição correta é haver mais de 30 pessoas ao amanhecer. A ilha começa com 30 moradores e chega a 33 com os visitantes, permitindo três ciclos se houver uma morte por dia. Ao pôr do sol, uma mensagem informa a morte e o nome da vítima. É possível resolver a maldição no primeiro dia, antes de Ivo partir, embora seja difícil. A partida dura no máximo três dias; sem solução, ocorre GAME OVER e começa de novo.
- Q: O que o jogador precisa fazer para vencer dentro desses três dias? → A: Descobrir como a maldição funciona e executar uma ação que a encerre definitivamente. A ação concreta ainda precisa ser definida na autoria do mistério.
- Q: Como o tempo deve passar durante cada dia? → A: O relógio avança continuamente enquanto o jogador anda e investiga. Diálogos e leitura pausam o tempo.
- Q: O que acontece se o jogador entregar a lagarta rosa à pessoa errada? → A: A lagarta escapa. O jogador pode procurá-la novamente e tentar outra pessoa antes do pôr do sol.

**Atualizações posteriores do usuário:** a bruxa era boa, refugiou-se na ilha e se irou com a intolerância. A festa de ações de graças encerra a maldição se todos contribuírem de bom grado sem conhecer essa finalidade. Um idoso que conheceu a bruxa sabe disso, vive afastado e é desacreditado. O possuído mata outra pessoa sorteada entre três vítimas possíveis definidas para ele; uma única exceção tira a própria vida. O idoso nunca pode ser possuído nem morto por um possuído. Essa correção substitui a interpretação inicial de que todo possuído morreria.

**Participantes confirmados pelo usuário:** a festa exige contribuição dos moradores vivos ainda presentes na ilha; os visitantes e moradores que morreram ou partiram não são obrigatórios.

**Exceção confirmada pelo usuário:** o idoso é o único morador que pode conhecer a finalidade contra a maldição e ainda assim contribuir para a festa; sua contribuição deve ser de bom grado.

**Elegibilidade confirmada pelo usuário:** somente os moradores vivos presentes podem ser possuídos, exceto o idoso. Os visitantes contam na população, mas não são possuídos.

**Proteção dos visitantes confirmada pelo usuário:** protagonista e dois amigos não podem ser possuídos nem mortos por um possuído. Eles contam na população, mas ficam fora dos sorteios de possuído e vítima.

## Proposta para avaliação

Recomenda-se um **modelo híbrido**: a verdade central é escrita e consistente; a pessoa possuída e as oportunidades de investigação podem variar. O jogador investiga duas perguntas: **quem precisa ser salvo hoje?** e **o que sustenta a maldição e como encerrá-la?** Salvar uma pessoa resolve a urgência, mas não resolve automaticamente o mistério central. A condição de vitória confirmada exige compreender o funcionamento da maldição e executar uma ação que a encerre definitivamente.

O ciclo proposto é: conhecer a rotina → encontrar um desvio → examinar vestígios e ouvir pessoas → confrontar versões → formular uma hipótese → procurar a lagarta rosa → decidir quem tratar → observar as consequências. A busca da lagarta pode acontecer antes da identificação.

### Base decidida no GDD

- Existem 30 moradores; os três visitantes contam enquanto estão em terra. A abertura chega a 33.
- Em cada amanhecer com mais de 30 pessoas em terra, um morador elegível é possuído. Sem cura até o pôr do sol, mata outra pessoa, sorteada entre três vítimas possíveis definidas para ela. Uma única pessoa excepcional tira a própria vida em vez de matar outra. A mensagem noturna informa quem morreu. O idoso que conheceu a bruxa é imune à possessão e não pode ser morto por um possuído. A partida tem no máximo três dias, com vitória possível no primeiro e GAME OVER sem solução até o fim do terceiro.
- A lagarta rosa cura a pessoa possuída. Depois de uma cura, a próxima pessoa possuída deve ser outra.
- Marca de Ivo, partidas, registros do cais, foto comunitária e ruína sustentam a investigação.
- Verdade da maldição, conhecimento dos moradores e hipóteses do jogador são coisas diferentes.

Os blocos identificados como propostas ainda aguardam avaliação. As decisões fornecidas pelo usuário estão registradas separadamente e refletidas no GDD. A pesquisa e as alternativas estão em [research.md](research.md).

### Origem e solução confirmadas pelo usuário

A bruxa era boa e foi viver na ilha buscando paz e refúgio de perseguições. Encontrou preconceito, brigas entre famílias e intolerância entre gerações; sua ira deu origem à maldição. A possessão manifesta essa ira em seu extremo e leva o possuído a decidir matar. Isso não torna os comportamentos habituais dos moradores equivalentes à possessão.

A crença predominante da vila, compartilhada inicialmente pelo jogador, é que a bruxa era má. Pistas sobre sua vida e seus atos revelam gradualmente sua bondade e os conflitos da comunidade. Um dos idosos conheceu a bruxa pessoalmente. Ela lhe contou que a maldição terminaria quando fosse realizada a festa de ações de graças e todos levassem alguma coisa de bom grado, sem saber que participavam para encerrar a maldição. Ele tenta aproximar os moradores, mas é visto como louco e vive numa casa afastada. Sua identidade entre os 30 moradores existentes e a localização dessa casa ainda precisam ser escolhidas.

O objetivo final é favorecer uma união sincera e realizar a festa nessas condições. Conhecer a solução não deve permitir simplesmente revelar aos moradores que precisam participar para se salvar. Os conflitos, motivações e gestos voluntários devem constar das fichas e das pistas. Todos significa os moradores vivos que ainda estejam na ilha no momento da festa. Mortos, moradores que partiram e visitantes não são participantes obrigatórios. O idoso é a única exceção entre os moradores à exigência de desconhecer a finalidade da festa: ele sabe o segredo e contribui de bom grado, conforme D4. O protagonista pode conhecer o segredo e não é participante obrigatório. O idoso não pode ser possuído nem morto por um possuído; sua proteção está confirmada em D5. Seu testemunho sobre a bruxa é preservado ao longo dos três dias, mas o jogador ainda precisa encontrá-lo, avaliar sua credibilidade e unir a vila.

### Gancho inicial — proposta para avaliação

**Necessidade confirmada pelo usuário:** assim que chegam, os viajantes precisam presenciar algo que desperte curiosidade e dê motivo concreto para procurar as primeiras pistas. A disposição para investigar não pode depender de um marcador de missão ou de o jogador já conhecer a maldição.

**Proposta:** ao chegar ao cais, os visitantes encontram uma discussão ligada à partida de Ivo. A tira azul, a família apreensiva e a proximidade do embarque são legíveis antes de qualquer explicação sobre a bruxa. Luana contesta a versão pública de que o pai parte por vontade própria e pede ajuda para entender por que ele foi escolhido. O jogador começa com a pergunta “por que esse homem precisa abandonar a família?” e com uma ação imediata: falar com Ivo antes de ele embarcar.

Essa discussão, a explicação pública de partida voluntária e o pedido de Luana são novos elementos propostos, não fatos já aprovados do cânone. A escolha de quem sustenta essa versão e seus motivos será revisada para preservar as personalidades existentes.

A investigação inicial admite dois acessos: conversar com Ivo e sua família ou examinar o registro de partida no cais e depois confrontá-lo com as falas. O registro concreto e seu conteúdo serão escritos no catálogo de pistas. Uma fala continua sendo depoimento; a divergência abre uma pergunta, sem provar automaticamente uma hipótese. O resultado deve apontar para outro morador ou lugar, formando a primeira ligação do mapa.

A abertura deve apresentar um vínculo humano e uma pergunta alcançável antes da explicação geral da maldição. O gancho pode acontecer antes da primeira possessão; não depende de uma morte anterior nem da partida efetiva de Ivo. A cronologia da chegada em relação ao primeiro amanhecer será ajustada na autoria, preservando três dias de investigação e a possibilidade de vitória no primeiro. Curiosidade para investigar e motivo para permanecer na ilha são relacionados, mas a permanência dos três visitantes ainda precisa ser sustentada na narrativa.

### Pistas e conhecimento distribuído

| Fonte | O que oferece | Como investigar |
| --- | --- | --- |
| Objetos e documentos | Datas, nomes, rasuras, fotografias, pertences e marcas de uso | Examinar detalhes e comparar registros |
| Lugares | Relações espaciais, obras interrompidas, ausências e mudanças de rotina | Revisitar o local em outro contexto |
| Moradores | O que presenciaram, ouviram, acreditam ou querem esconder | Perguntar sobre assuntos específicos e apresentar pistas |
| Comportamento | Diferença entre rotina habitual e sinais de possessão descritos no GDD | Comparar o morador consigo próprio e consultar quem o conhece |

Cada morador tem um recorte de experiência, não uma porcentagem obrigatória da resposta. Alguns sabem pouco; outros conhecem regras práticas sem entender sua origem. Família, trabalho e acontecimentos presenciados justificam esse conhecimento. Dois relatos do mesmo boato contam como uma única origem. Medo, luto, segredo pessoal e possessão não são equivalentes.

O caderno registra **observação**, **depoimento** e **hipótese**, com fonte, local e momento. Um documento é um objeto observado; a afirmação escrita nele ainda pode ser falsa. O jogador relaciona evidências a perguntas concretas, como “por que Ivo foi marcado?” ou “o que mudou em Artur?”. Coletar todas as pistas não completa a dedução automaticamente. A interface não mostra porcentagens de culpa nem revela o possuído ao reunir determinada quantidade de itens.

### Aleatoriedade e mortes

O sorteio do amanhecer escolhe o possuído apenas entre os moradores vivos presentes na ilha, excluindo o idoso. Os três visitantes contam para a população, mas nunca são possuídos. Os visitantes também são protegidos de mortes causadas pela possessão e não integram nenhuma lista de vítimas. Para cada possuído comum, o dossiê define três vítimas possíveis e justifica cada relação; uma delas é sorteada para a morte caso não haja cura. A única exceção tira a própria vida e não usa essa lista de três vítimas. As identidades do idoso e da exceção ainda precisam de autoria. O idoso e os três visitantes não integram nenhuma lista de vítimas. As três vítimas de um possuído comum devem ser outras pessoas, excluindo o próprio possuído. Como proposta de consistência, vítimas já mortas ou fora da ilha não podem ser sorteadas; o mapa deve verificar os casos em que a lista disponível se esgota, sem inventar uma quarta vítima durante a partida.

Para cada conclusão indispensável do mistério central, preparar três caminhos com origens diferentes; pelo menos um usa evidência que persiste após a morte de uma testemunha. Antes de aceitar uma combinação de eventos, verificar se ainda existe um caminho completo para cada conclusão necessária. Se a última fonte durável puder ser perdida, essa combinação não é válida. Não criar cartas convenientes depois de uma morte nem transferir lembranças para quem não presenciou o fato.

A morte preserva o que já foi registrado, encerra conversas da vítima e altera as reações de pessoas próximas. O possuído e a vítima são pessoas distintas, salvo a única exceção de morte da própria pessoa possuída. A mensagem ao pôr do sol identifica quem morreu; ela não revela automaticamente o autor do homicídio. Detalhes íntimos e reconciliações podem se perder, mas a explicação necessária permanece acessível. A festa pode resolver a maldição no primeiro dia; sem solução em três dias, ocorre GAME OVER e reinício.

Com 33 pessoas e uma morte por dia, os amanheceres têm 33, 32 e 31 pessoas; a terceira morte leva a 30 ao fim do terceiro dia. A morte pode retirar uma testemunha que não era possuída. Curar o possuído evita a morte do dia, sem conceder dias extras. O mapa precisa combinar os possíveis autores, seus três alvos e as fontes de informação restantes. A festa deve ser alcançável no primeiro dia, sem depender da morte de alguém ou da partida de Ivo.

### Exemplo apoiado no GDD

Se Artur for possuído, passa a tratar vizinhos como números e risca nomes de pessoas ainda em terra. O jogador pode observar sua fala, ouvir Rosa sobre a mudança e comparar os riscos com a presença desses moradores. São três acessos ao problema; nenhum depoimento isolado prova a possessão. Se Rosa já tiver morrido, a fala de Artur e o registro confrontado com pessoas presentes devem continuar permitindo a dedução. Este exemplo descreve uma variante futura; não implica que Artur ou Rosa devam morrer na abertura.

### Primeiro MVP — dossiê, mapa e teste web das pistas

**Recorte confirmado pelo usuário:** manter a spec 009 e planejar um jogo web simples para exercitar pistas e decisões antes de investir na apresentação em Unity. Esta etapa produz plano e tasks; não implementa a aplicação.

O MVP terá três personagens interativos — Artur, Teresa e Ivo —, seis a nove evidências, uma pergunta principal sobre a partida de Ivo e uma conclusão sustentada por pistas. Luana participa do acontecimento inicial por texto; sua inclusão como personagem clicável fica para uma expansão. A autoria deve preservar os fatos estabelecidos no GDD e diferenciar o que cada pessoa sabe da verdade do mundo.

A tela apresenta o mapa ilustrado existente, fichas circulares de personagens clicáveis, falas em sequência e um painel de pistas descobertas permanentemente visível. A interação permite escolher perguntas, examinar pontos do mapa, apresentar uma evidência e propor uma explicação. Pistas desconhecidas e a solução não aparecem antecipadamente na lista. O painel permanece acessível durante as conversas.

A entrega inclui testes automatizados das regras de pistas, das condições de acesso e da preservação de evidências após perda de uma fonte. O conteúdo estruturado deverá gerar o dossiê legível e o mapa de dependências E/OU. Um validador examina sequências possíveis de ações, incluindo a perda de fontes e a preservação de relatos já obtidos. O caso normal e cenários controlados sem uma ou duas testemunhas testam a estrutura; são ferramentas de autoria, não mortes predeterminadas da campanha.

**Limites:** o piloto não implementa os 30 moradores, o ciclo completo de três dias, cronômetro, sorteio de possessão/vítimas, cura, festa final ou movimentação 3D. As regras anteriores permanecem como contexto da campanha. A viabilidade temporal do jogo completo não será comprovada por este MVP. O encerramento da amostra corresponde a explicar a partida de Ivo, não a vencer a maldição.

Os retratos reutilizam a folha existente. Artur e Teresa já estão identificados no GDD. O terceiro adulto da direita pode representar Ivo provisoriamente na ferramenta de teste, com essa associação registrada no dossiê e submetida à revisão; não fixa sua aparência definitiva. Não gerar novos assets nesta entrega. A lista de três vítimas por possuído e a identidade do velho e da exceção continuam trabalho da expansão narrativa.

**Aceitação ativa deste MVP:** US4, US1 e a continuidade de fontes de US3; SC-004, a parte de independência de ordem de SC-003, SC-010 apenas para rastreabilidade de fontes e SC-014 a SC-017. As metas de salvar possuídos, validar a festa, cronômetro e testes quantitativos com cinco jogadores ficam para etapas posteriores.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Investigar sem depender da ordem (Priority: P1)

O jogador começa por uma conversa ou por um objeto e chega a uma hipótese fundamentada.

**Why priority**: estabelece o ato de investigar e evita uma sequência única de cliques.
**Independent Test**: percorrer o caso começando pelo documento e depois pelo depoimento.

**Acceptance Scenarios**:

1. **Given** uma pista e um depoimento desconhecidos, **When** são encontrados em ordens diferentes, **Then** os mesmos fatos e perguntas pertinentes ficam disponíveis.
2. **Given** um boato registrado, **When** alguém o repete citando a mesma origem, **Then** o caderno preserva a dependência e não o apresenta como confirmação independente.
3. **Given** uma hipótese com evidência contrária, **When** o jogador revisa suas relações, **Then** pode alterá-la sem apagar as fontes originais.
4. **Given** os viajantes recém-chegados e sem conhecimento da maldição, **When** presenciam o acontecimento inicial, **Then** conseguem identificar uma pessoa ou situação que dá motivo à primeira conversa ou exame de pista.

### User Story 2 — Identificar e tentar salvar alguém (Priority: P1)

O jogador compara a rotina com sinais atuais, encontra a lagarta e decide a quem entregá-la.

**Why priority**: conecta dedução, prazo e consequência humana.
**Independent Test**: testar cada candidato como possuído, buscando a lagarta antes e depois das evidências.

**Acceptance Scenarios**:

1. **Given** cada variante, **When** o jogador investiga, **Then** pelo menos dois indícios de origens independentes, juntos, distinguem o alvo dos inocentes.
2. **Given** o alvo definido, **When** o jogador repete diálogos ou muda o percurso, **Then** alvo e acontecimentos passados permanecem iguais.
3. **Given** o possuído e a lagarta, **When** a entrega correta ocorre antes do prazo, **Then** o homicídio do dia é impedido; sem cura, uma das três vítimas elegíveis morre ao pôr do sol. Para a única exceção, morre o próprio possuído. A mensagem informa o nome de quem morreu.
4. **Given** um dia em andamento, **When** o jogador caminha ou investiga no mundo, **Then** o relógio avança continuamente; ao entrar em diálogo ou leitura, pausa e depois retoma do mesmo horário, sem cobrar o intervalo pausado.
5. **Given** uma entrega à pessoa errada e tempo restante, **When** a lagarta escapa, **Then** o inocente permanece vivo, o possuído continua capaz de matar e a lagarta pode ser reencontrada para nova tentativa antes do pôr do sol; o relógio não reinicia.

### User Story 3 — Continuar depois de uma perda (Priority: P2)

O jogador perde uma testemunha e procura uma fonte alternativa que já existia no mundo.

**Why priority**: permite consequências permanentes sem bloquear o jogo por acaso.
**Independent Test**: retirar individualmente cada testemunha e reconstruir a conclusão necessária com as fontes restantes.

**Acceptance Scenarios**:

1. **Given** uma testemunha morta, **When** o jogador consulta suas anotações, **Then** depoimentos obtidos permanecem identificados e nenhuma fala futura fica disponível.
2. **Given** uma conclusão indispensável desconhecida, **When** a testemunha morre, **Then** permanece um percurso demonstrável até essa conclusão, sem conhecimento inventado em outro morador.
3. **Given** uma morte, **When** o jogador encontra alguém próximo, **Then** há reação ligada à perda, sem transformar luto em prova automática de possessão.

### User Story 4 — Exercitar pistas no navegador (Priority: P1)

O jogador usa o mapa ilustrado, clica nos rostos e acompanha as falas sem perder de vista as pistas descobertas.

**Why priority**: permite avaliar clareza e interesse da investigação com os assets já disponíveis.
**Independent Test**: abrir a amostra, conversar com um personagem e examinar um ponto do mapa, mantendo a lista de pistas visível.

**Acceptance Scenarios**:

1. **Given** a tela inicial, **When** o jogador clica numa ficha, **Then** vê o nome correto e as falas do personagem em sequência.
2. **Given** um diálogo em andamento, **When** avança ou escolhe uma pergunta, **Then** o painel de evidências conhecidas permanece disponível, sem revelar pistas ainda não descobertas.
3. **Given** uma evidência obtida, **When** volta ao mesmo diálogo ou objeto, **Then** a entrada mantém sua origem e não é duplicada.
4. **Given** o reinício do caso, **When** começa uma nova sessão, **Then** pistas, escolhas e hipóteses retornam ao estado inicial do cenário escolhido.

### Edge Cases

- Testemunha morta antes da primeira conversa; mortes sucessivas removendo caminhos diferentes.
- Lagarta encontrada antes do alvo; entrega errada fazendo-a escapar; buscas e tentativas repetidas; erro perto do pôr do sol sem tempo suficiente para nova cura.
- Dois inocentes com comportamento estranho, explicado por causas comuns.
- Relato verdadeiro apenas para o dia anterior; morto citado como se estivesse vivo.
- Mudanças por partida, retorno ou nascimento; a condição de possessão é estritamente maior que 30 no amanhecer. Com 30 ou menos não há novo alvo.
- Cura mantendo a população acima de 30 sem ampliar o prazo; vitória no primeiro dia antes da partida de Ivo; terceira morte levando a 30 e GAME OVER sem solução central.
- Lagarta inacessível dentro do prazo: variante inválida, não azar aceitável.
- Caminho necessário inexistente: impedir a combinação antes do dia ou concluir um fracasso já definido; nunca manter uma investigação impossível.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Registrar pistas, fontes, momento e natureza da informação, distinguindo observação, depoimento e hipótese.
- **FR-002**: Permitir observar, examinar, conversar, apresentar evidência e revisar hipóteses; a ordem não pode eliminar informação equivalente sem consequência prevista.
- **FR-003**: Limitar cada fala ao que o morador sabe ou acredita naquele momento; identificar na autoria suas omissões e mentiras.
- **FR-004**: Oferecer referência acessível da rotina de cada candidato e dois indícios independentes para identificar o possuído no recorte.
- **FR-005**: Fixar no amanhecer o morador possuído, entre vivos presentes com caminhos preparados, excluindo o idoso e os três visitantes. Após cura, excluir o possuído anterior do próximo sorteio.
- **FR-013**: Manter separados possuído, conjunto de três vítimas possíveis e vítima efetiva. A cura do possuído impede a morte do dia. O idoso não pode ser possuído nem integrar qualquer lista de vítimas de um possuído.
- **FR-014**: Limitar a partida a três dias; sem resolver a maldição até o fim do terceiro dia, mostrar GAME OVER e reiniciar. Curas não concedem dias extras.
- **FR-015**: Quando ocorrer uma morte ao pôr do sol, apresentar uma mensagem com o nome da vítima. Identificar quem morreu não confirma automaticamente quem era o possuído.
- **FR-016**: Permitir realizar a festa e encerrar a maldição no primeiro dia, antes de Ivo partir; nenhuma pista ou ação indispensável pode exigir outro dia, uma morte ou a partida dele. Os participantes obrigatórios são os moradores vivos presentes na ilha; o idoso é a única exceção entre os moradores à condição de desconhecer a finalidade da festa, conforme D4.
- **FR-017**: Encerrar definitivamente a maldição quando a festa acontecer com todos os moradores vivos ainda presentes na ilha contribuindo de bom grado. Os moradores devem desconhecer a finalidade contra a maldição, exceto o idoso, que conhece o segredo e também contribui sinceramente. Não exigir contribuição de mortos, moradores que partiram ou visitantes. A cura diária não substitui essa condição.
- **FR-018**: Na entrega à pessoa errada, fazer a lagarta escapar sem matar o inocente ou curar o alvo. Permitir reencontrá-la e tentar novamente enquanto houver tempo antes do pôr do sol. A busca segue o relógio contínuo, sem reiniciar o dia nem conceder tempo adicional.
- **FR-019**: Documentar para cada pessoa o que presenciou, acredita e oculta, os vínculos e conflitos que afetam sua participação voluntária e as pistas correspondentes. Para cada possuído comum, definir três vítimas possíveis com justificativas. A única exceção e o idoso terão regras próprias explicitadas.
- **FR-020**: Fazer o mapa distinguir informação conhecida pelo jogador, informação conhecida pelo personagem e motivação de contribuição. O jogador descobrir o segredo não transmite essa informação à vila automaticamente.
- **FR-021**: Na chegada, apresentar um acontecimento observável, uma pergunta compreensível e uma ação inicial de investigação motivada por esse acontecimento. O jogador não deve precisar conhecer a maldição para entender por que perguntar ou examinar a primeira pista.
- **FR-006**: Manter fatos centrais e cronologia consistentes entre variantes, sem mudar retroativamente a verdade.
- **FR-007**: Garantir acesso inicial à lagarta e a um percurso viável de solução dentro do prazo em toda variante aceita, inclusive sem uma testemunha. Erros e novas buscas podem consumir o tempo restante; não concedem prazo extra.
- **FR-008**: Aplicar morte, preservação do caderno, indisponibilidade de conversas e reações sociais coerentes.
- **FR-009**: Preparar três caminhos de origens distintas por conclusão indispensável e validar sua acessibilidade após cada combinação de perdas permitida.
- **FR-010**: Avançar o relógio continuamente durante deslocamento e investigação no mundo; pausá-lo durante diálogos, leitura e consulta ao caderno, retomando do mesmo horário ao sair. Comunicar de forma legível o período do dia e a aproximação do pôr do sol. A duração real de cada dia será calibrada no planejamento.
- **FR-011**: Diferenciar a identidade do possuído da origem da maldição; a cura diária não encerra o mistério central.
- **FR-012**: Com mais de 30 pessoas em terra ao amanhecer, sortear um possuído apenas entre os moradores elegíveis, excluindo o idoso. Contar os visitantes na população sem incluí-los nesse sorteio. Sem cura, sortear a vítima entre três alvos definidos para o possuído comum, com uma única exceção de morte da própria pessoa possuída. Com 30 ou menos não há nova possessão. Os três visitantes e o idoso são excluídos de todas as listas de vítimas.

- **FR-022**: Exibir o mapa existente e fichas com rostos em círculos; clicar numa ficha inicia a interação com o personagem identificado.
- **FR-023**: Permitir percorrer falas em sequência e escolher perguntas cujas condições foram atendidas; novas pistas podem liberar novos assuntos.
- **FR-024**: Manter visível a lista das evidências descobertas, com descrição, natureza e origem, inclusive durante diálogos; não listar a solução ou evidências desconhecidas.
- **FR-025**: Permitir propor uma explicação e indicar as evidências que a sustentam; a simples coleta de pistas não conclui a amostra automaticamente.
- **FR-026**: Produzir dossiê e mapa de dependências rastreáveis ao mesmo conteúdo usado pelo protótipo, evitando versões conflitantes das regras e pistas.
- **FR-027**: Validar caminhos por sequências de ações possíveis e gerar evidência de caminho existente ou de bloqueio, sem tratar dois relatos do mesmo boato como duas origens independentes.

### Key Entities *(include if feature involves data)*

- **Fato do mundo**: acontecimento verdadeiro com participantes, lugar e momento.
- **Morador**: identidade, rotina, relações, conhecimento, crenças, segredos e condição atual.
- **Evidência**: observação ou relato, origem, conteúdo, disponibilidade e validade temporal.
- **Hipótese**: interpretação do jogador relacionada a evidências favoráveis e contrárias.
- **Dia de investigação**: população em terra, alvo, prazo, fontes acessíveis e consequências.
- **Conclusão indispensável**: pergunta necessária para avançar e caminhos alternativos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: As três variantes permitem identificar o alvo por dois indícios independentes antes do prazo.
- **SC-002**: Todas as combinações válidas de uma variante com a morte de uma testemunha mantêm solução; a pessoa possuída permanece viva no início do dia e a revisão registra o percurso e as fontes usados.
- **SC-003**: Inverter a ordem entre conversa, objeto e busca da lagarta preserva os fatos e o resultado de uma decisão equivalente.
- **SC-004**: Nenhum diálogo da amostra atribui informação sem origem ou trata um morto como disponível.
- **SC-005**: Em avaliação futura com cinco pessoas sem conhecimento do caso, quatro distinguem fato, depoimento e hipótese e justificam a escolha com duas fontes, sem instrução externa sobre o alvo.
- **SC-006**: Sem curas e sem outras mudanças populacionais, verificar amanheceres com 33, 32 e 31 pessoas e três avisos de morte; o terceiro pôr do sol termina com 30 e GAME OVER sem solução. Com curas e sem solução central, o limite continua sendo o terceiro dia.
- **SC-007**: Demonstrar no mapa um percurso de pistas e ações que permite a festa no primeiro dia, antes da partida de Ivo, respeitando voluntariedade e segredo conforme D4.
- **SC-008**: Demonstrar uma entrega errada seguida de nova busca e cura correta dentro do mesmo dia; demonstrar também que a falta de tempo após um erro não concede extensão do prazo nem impede a consequência do pôr do sol.
- **SC-009**: Em cada cenário revisado, a mensagem nomeia a vítima efetiva; nos casos comuns ela difere do possuído. A exceção de morte da própria pessoa possuída aparece uma única vez na autoria e o idoso nunca aparece como possuído nem como vítima, inclusive nas listas de três alvos. Os três visitantes nunca aparecem como possuídos; também nunca aparecem como vítimas nem nas listas de três alvos.
- **SC-010**: A amostra documental permite rastrear uma conclusão até fontes independentes, relacionar cada personagem às suas três vítimas e verificar o efeito de perder uma dessas fontes antes da festa.
- **SC-011**: No mapa da festa, excluir dos participantes obrigatórios os moradores mortos e os que já partiram e não exigir contribuição dos visitantes. Cada morador vivo ainda presente continua tendo um caminho de contribuição voluntária.
- **SC-012**: Na validação da festa, aceitar o idoso como participante obrigatório e sincero mesmo conhecendo a finalidade contra a maldição; para os demais moradores, verificar a condição de desconhecimento. O conhecimento do protagonista não invalida a festa.
- **SC-013**: Na revisão do roteiro da chegada, identificar o acontecimento, a pergunta e a primeira ação, além de dois caminhos de descoberta. Em avaliação futura com cinco pessoas sem conhecimento do mistério, pelo menos quatro devem explicar o motivo da primeira investigação sem depender de um objetivo escrito na interface.

- **SC-014**: Clicar nas três fichas abre as respectivas interações; mapa, falas e lista de pistas são utilizáveis a 1366×768 e 1920×1080, com teclado e mouse. Em 390×844, as áreas têm disposição adaptada e a lista não depende de modal ou aba para ser consultada.
- **SC-015**: Dois percursos começando por fontes diferentes chegam à conclusão da amostra com a mesma base factual; apresentar uma hipótese exige evidências conhecidas e não aceita mera contagem de pistas.
- **SC-016**: O relatório cobre as sete configurações de zero, uma ou duas perdas entre as três testemunhas e indica uma sequência viável ou o bloqueio encontrado. Antes da entrega, cada configuração deve manter caminho à conclusão; falhas exigem revisão da autoria.
- **SC-017**: Matar uma testemunha antes de uma conversa bloqueia aquela aquisição; fazê-lo depois preserva a evidência registrada. Um caso de teste intencionalmente insolúvel é detectado, sem ser entregue como cenário jogável.

## Assumptions

- Esta entrega contém documentos; nenhum comportamento foi implementado ou testado em Unity.
- O recorte diário começa em situação controlada com mais de 30 pessoas em terra. A abertura completa parte de 30 moradores e três visitantes; não exige reduzir de 33 para 30 para iniciar o ciclo.
- Possessão não equivale a culpa moral. A pessoa em risco não é necessariamente autora consciente de um crime.
- Os 30 moradores poderão receber conteúdo depois; o recorte cobre três candidatos. Aleatoriedade ampla exige autoria e validação adicionais.
- Identidade do possuído e descobertas não serão geradas por texto improvisado durante a partida.
- GDD e perfis existentes são fontes de continuidade. Aprovar a proposta precede qualquer alteração das decisões gerais.

## Decisões para aprovação

### D1 — População e duração da partida: resolvida

O usuário substituiu a premissa da pergunta: a possessão ocorre com mais de 30 pessoas ao amanhecer, não com exatamente 30. A chegada dos três visitantes eleva o total de 30 para 33. Há um limite de três dias para resolver a maldição, com vitória possível já no primeiro dia e GAME OVER seguido de reinício em caso de fracasso. Sem curas ou outras mudanças de população, três mortes levam o total a 30. Não é necessário inventar uma transição de 33 para 30 antes da primeira possessão.

### D2 — Passagem do tempo: resolvida

O usuário escolheu relógio contínuo enquanto caminha e investiga. Diálogos e leitura, incluindo consulta ao caderno, pausam o tempo. Ao encerrar a pausa, o relógio retoma do mesmo horário. A duração real de cada dia será calibrada posteriormente; o limite da partida permanece em três dias.

### D3 — Tratamento da pessoa errada: resolvida

O usuário escolheu que a lagarta escape. O jogador pode reencontrá-la e tentar outra pessoa antes do pôr do sol. O inocente sobrevive e o alvo permanece ameaçado. A busca consome tempo do relógio contínuo; o erro não reinicia o dia nem concede prazo extra.

### D4 — Participantes e exceção de conhecimento: resolvida

O usuário confirmou que todos significa todos os moradores vivos ainda presentes na ilha. Visitantes, mortos e moradores que partiram não são contribuintes obrigatórios. Cada morador deve contribuir de bom grado sem saber que a festa encerra a maldição, com uma única exceção: o idoso conhece o segredo e também contribui sinceramente. O protagonista pode descobrir a finalidade e não está entre os contribuintes obrigatórios.

### D5 — Proteção do idoso: resolvida

O usuário confirmou que o idoso não pode ser possuído nem morto por um possuído. Excluí-lo tanto do sorteio da possessão quanto de todas as listas de três vítimas. Essa proteção preserva sua presença como testemunha da bruxa durante os três dias; as outras fontes continuam necessárias para reconstruir os acontecimentos, avaliar suas palavras e resolver os conflitos da festa.

### D6 — Quem pode ser possuído: resolvida

O usuário escolheu apenas os moradores, exceto o idoso. Os três visitantes contam para ultrapassar 30 pessoas em terra, mas não integram o sorteio de possessão. D7 também confirmou sua proteção contra mortes causadas pela possessão.

### D7 — Visitantes como vítimas: resolvida

O usuário confirmou que nenhum dos três visitantes pode ser morto por um possuído. Protagonista e amigos ficam fora de todas as listas de vítimas. Eles contam na população enquanto estão em terra, mas não podem ser possuídos nem morrer por causa da possessão. Esta regra não cria um GAME OVER antecipado por morte do protagonista nesse sistema; permanece o limite de três dias sem resolver a maldição.

Identidade do idoso entre os 30 moradores, posição da casa afastada e identidade da única exceção de morte da própria pessoa possuída permanecem pendências de autoria. Não acrescentar um morador à contagem para preencher esse papel.

O sorteio da pessoa possuída foi confirmado nesta sessão. A restrição a candidatos preparados continua proposta para avaliação; [research.md](research.md) preserva a comparação original entre estruturas de mistério.
