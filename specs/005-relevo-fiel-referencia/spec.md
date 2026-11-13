# Spec 005 — Relevo idêntico à referência

**Criada:** 2026-11-12 · **Estado:** aprovada para execução por etapas; primeira maquete aguardando avaliação visual.
**Pasta da feature:** `specs/005-relevo-fiel-referencia` · **Branch:** não se aplica; workspace sem repositório Git.
**Pedido:** reaproveitar o que for possível da spec4 e reproduzir fielmente a ilha de cima usando `ilha-vista-topdown-004.png`, complementada pelas vistas de relevo.

## Cenários e testes

### US1 — Reconhecer o mesmo relevo da imagem (P1)

Como pessoa avaliando o jogo, quero comparar a ilha com a [referência aérea](../../Referencias/modelagem-vista-aerea.png) e reconhecer o mesmo terreno, com seus recortes, alturas relativas e paredões.

**Prioridade:** é o objetivo central; uma ilha apenas semelhante não satisfaz esta entrega.
**Teste independente:** comparar referência e vista do jogo com orientação, perspectiva e enquadramento equivalentes, lado a lado e por sobreposição.

**Aceitação:**
1. Dada a comparação alinhada, o contorno costeiro, a enseada frontal, os terraços do povoado, as hortas à esquerda e o maciço rochoso posterior ocupam as mesmas posições relativas e apresentam as mesmas proporções visíveis.
2. Ao inspecionar o perfil, o maciço se estende ao longo do fundo da vila, com cristas irregulares, patamares e falésias marcadas; não se reduz a um morro arredondado com rampas contínuas.
3. Após apresentar a primeira versão do relevo para avaliação, a produção aguarda a avaliação do usuário antes de ampliar o escopo ou detalhar outros elementos.

### US2 — Caminhar no terreno compatível com a inclinação (P1)

Como jogador, quero percorrer as superfícies livres com inclinação menor que 45°, preservando os paredões da referência.

**Prioridade:** substitui a exigência da 004 de tornar toda a ilha caminhável, que conflitava com a forma desejada.
**Teste independente:** partir do cais, visitar largo, três setores de moradias, hortas, ruína e recanto posterior, e retornar; testar também superfícies livres fora dos caminhos e o limite de inclinação.

**Aceitação:**
1. Nas superfícies terrestres livres com inclinação menor que 45°, o jogador caminha sem travamento, saltos, escalada ou teletransporte; os patamares destinados à exploração têm acesso a partir do cais.
2. Faces com inclinação igual ou superior a 45° podem permanecer inacessíveis; o jogador não sobe paredões caminhando nem atravessa rochas. Não há obrigação de acesso a cada ponto do topo ou das encostas íngremes.
3. Cais e terreno se conectam sem obstáculos sobrepostos; o percurso completo permite retorno, e cair na água recupera o jogador sem recuperação indevida em terra.

### US3 — Continuar a partir da entrega existente (P2)

Como responsável pelo projeto, quero aproveitar o trabalho útil da 004 sem preservar defeitos que impeçam a fidelidade do relevo.

**Prioridade:** reduz retrabalho e mantém a experiência já construída.
**Teste independente:** conferir os elementos herdados na nova cena e a preservação dos arquivos das entregas anteriores.

**Aceitação:** o cais, barco, visual atual do jogador, controles, câmera, recuperação, blocos de moradias e casinha posterior são reutilizados quando compatíveis; elementos podem ser reposicionados. A 004 permanece preservada e suas validações parciais não são tratadas como aprovação da 005.

### Casos de borda

- A imagem não revela o verso da ilha nem fornece escala ou alturas medidas: essas regiões e medidas são inferências identificadas, sem alterar o relevo visível para acomodá-las.
- Exatamente 45° não integra a faixa exigida de caminhada. Inclinação é medida em relação à horizontal da superfície física, não estimada apenas pela aparência.
- Uma área livre abaixo de 45° não deve ficar isolada involuntariamente por paredões. Seus acessos devem respeitar a forma da referência; se houver conflito, apresentá-lo para avaliação antes de deformar o relevo.
- Água e volumes ocupados por construções não contam como terreno livre; costuras entre superfícies não podem prender o personagem.
- O recanto posterior e sua casinha devem continuar acessíveis e ocultos do largo, sem modificar a silhueta visível para isso.

## Requisitos

### Funcionais

- **FR-001:** tratar como referências mandatórias de exatidão o conjunto composto por `Referencias/ilha-vista-topdown-004.png`, `Referencias/modelagem-vista-aerea.png` e `Referencias/ilha-vistas-laterais/vista-norte.png`, `vista-sul.png`, `vista-leste.png` e `vista-oeste.png`. Reproduzir a composição da vista superior, os volumes da perspectiva aérea e o perfil correspondente de cada lado. Nenhuma imagem desse conjunto é apenas referência de estilo. Se as vistas divergirem em uma região, registrar a divergência e resolvê-la com o usuário antes de mudar essa parte do relevo.
- **FR-002:** reproduzir as seis características visíveis: (a) contorno rochoso irregular com saliências; (b) enseada frontal côncava com praia em arco e cais à direita; (c) terraços do povoado ao redor do largo; (d) patamares das hortas e ruína à esquerda; (e) maciço posterior largo, elevado e de crista irregular; (f) paredões e patamares rochosos, especialmente ao fundo e à direita, incluindo faces muito íngremes na faixa visual aproximada de 70–80° indicada pelo usuário. Preservar posições relativas, proporções, alturas aparentes e transições.
- **FR-003:** priorizar a fidelidade do relevo sobre a reutilização da malha 004. Não manter o contorno quase circular, o monte suave ou rampas generalizadas quando divergirem da imagem; cores e texturas não substituem volumes e desníveis.
- **FR-004:** reutilizar os elementos e comportamentos compatíveis da 004, incluindo os 12 blocos dos três setores, hortas, ruína e casinha simples; ajustar posições e caminhos ao novo terreno, mantendo sua identificação. Não exigir que a imagem tenha a mesma contagem de casas do jogo.
- **FR-005:** permitir caminhada nas superfícies livres abaixo de 45° e bloquear a subida normal nas faces a partir de 45°, preservando falésias e faces das colinas com 70–80° quando representadas pela referência. Substituir nesta entrega os limites de 30°/35° e a exigência de toda a superfície caminhável da 004.
- **FR-006:** manter um percurso conectado de ida e volta entre cais, largo, setores, hortas, ruína e recanto. O terreno percorrível fora das trilhas também deve funcionar; não limitar a validação ao caminho principal.
- **FR-007:** preservar personagem, controles, câmera, colisões e recuperação da 004, ajustando apenas o necessário para o novo relevo e a regra de inclinação; não alterar velocidade para compensar escala.
- **FR-008:** entregar fonte editável e cena jogável próprias da 005, preservando fontes, cenas e assets anteriores; registrar o que foi reaproveitado, adaptado ou substituído e por quê.
- **FR-009:** apresentar primeiro uma versão avaliável do relevo com comparação alinhada; após a avaliação, concluir integração e validação da versão aceita. Distinguir aprovação visual, análise de inclinações e testes reais de caminhada.

### Elementos principais

Relevo de referência; costa e enseada; terraços; maciço e falésias; superfícies percorríveis; elementos herdados da 004; recanto posterior; comparações visuais e registros de validação.

## Critérios de sucesso

- **SC-001:** comparações alinhadas com as seis imagens mandatórias permitem conferir a planta, a perspectiva e os perfis norte, sul, leste e oeste. As formas visíveis de todas as imagens coincidem com as respectivas vistas do terreno; divergências entre referências são identificadas e resolvidas com o usuário antes da aprovação. Sobreposição auxilia a comparação; semelhança genérica não encerra o critério.
- **SC-002:** o jogador completa ida e volta por todos os destinos de FR-006, com zero travamentos, travessias de rocha ou recuperações indevidas, incluindo verificações fora das trilhas e dos dois lados do limiar de 45°.
- **SC-003:** o registro de caminhabilidade identifica superfícies abaixo e a partir de 45°, acessos, cobertura amostrada e limitações; nenhum destino obrigatório permanece inacessível.
- **SC-004:** os 12 blocos, cais, barco, marcos e personagem herdado estão presentes; a casinha posterior permanece oculta do largo e acessível. As entregas anteriores permanecem preservadas.
- **SC-005:** a entrega inclui comparação aérea, vistas de perfil, capturas na câmera do jogador e resultados do percurso completo, com pendências explícitas e aprovação visual registrada somente quando concedida.

## Premissas, reaproveitamento e limites

- “Idêntico” significa reproduzir as formas e relações visíveis em cada imagem do conjunto de referência, segundo seu ângulo. As vistas laterais fixam os perfis dos lados correspondentes. Não se exige reprodução pixel a pixel de iluminação, vegetação ou arquitetura; a geometria do relevo, incluindo as cristas, paredões e patamares visíveis, deve corresponder às referências.
- Base disponível: [fonte Blender 004](../../Source/Blender/Ilha_Relevo_004.blend), [cena 004](../../Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity), export do relevo, prefab e personagem Male_Casual já integrado. Malha, materiais e ferramentas de validação podem ser reaproveitados após inspeção de compatibilidade, sem obrigação de conservar sua geometria ou resultados.
- A [004 foi encerrada parcialmente](../004-relevo-ilha/evidence/percurso.md): o percurso alcançou 49/54 pontos e parou sem progresso. Queda na água, controles físicos e revisão final não foram integralmente aprovados; exigem verificação na 005.
- A dimensão inicial do GDD é uma referência de produção revisável. Não se imporá redução em relação à 003 nem escala inferida como medida da imagem; ajustes necessários serão explicitados no planejamento.
- Permanecem a direção 3D low poly e a leitura na câmera real. Casas continuam provisórias; não entram interiores, NPCs, diálogos, novas pistas ou novas decisões narrativas. A casinha isolada não estabelece um novo morador no cânone.
- Esta spec não autoriza implementação. Planejamento e tarefas aguardam aprovação explícita desta proposta, conforme as orientações do projeto.

## Prioridade das referências — decisão de 2026-11-13

Por instrução explícita do usuário, `Referencias/modelagem-vista-aerea.png` é o oráculo. Em qualquer divergência de relevo, proporção, altura, silhueta ou disposição dos patamares, essa imagem prevalece sobre a topdown e as quatro vistas laterais. As demais imagens continuam obrigatórias para comparação, mas não podem impor uma forma incompatível com o oráculo. A vista norte não autoriza abrir um vale que interrompa o maciço contínuo da aérea. A aprovação de exatidão depende da comparação visual da maquete; contagem de faces íngremes não prova semelhança.
