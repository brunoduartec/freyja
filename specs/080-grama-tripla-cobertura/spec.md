# Feature Specification: Triplicar densidade e cobertura da grama

**Feature Branch**: `080-grama-tripla-cobertura`
**Created**: 2026-11-25
**Status**: Approved for implementation
**Input**: Aumentar a densidade e a área coberta por grama para cerca de três vezes o estado atual, preservando a distribuição aleatória modulada pelo mapa de noise.

## User Scenarios & Testing

### User Story 1 — Explorar campos e encostas mais cobertos (Priority: P1)

Como jogador, quero encontrar muito mais grama distribuída pelos campos e encostas da ilha para que a vegetação pareça abundante e menos esparsa durante a exploração.

**Why this priority**: A mudança visual na cena jogável é o objetivo central desta entrega.

**Independent Test**: Iniciar a cena principal em uma qualidade comparável, percorrer os mesmos pontos de campo e encosta do registro da entrega 078 e comparar capturas alinhadas. Contar as raízes elegíveis da qualidade Alta com o mesmo método e confirmar o alvo aproximado de três vezes a referência atual de 116.214 raízes.

**Acceptance Scenarios**:

1. **Given** a cena principal e a qualidade Alta, **When** a vegetação é gerada, **Then** a distribuição tem aproximadamente três vezes as 116.214 raízes elegíveis registradas na entrega 078, com meta nominal de 348.642 e tolerância de ±10%.
2. **Given** as capturas de referência e ampliada no mesmo ponto e enquadramento, **When** são comparadas em campos e encostas elegíveis, **Then** a área com vegetação visível aumenta substancialmente e as manchas continuam irregulares, sem padrão em blocos ou grade evidente.
3. **Given** o personagem atravessa campo e encosta, **When** a câmera acompanha o percurso nos enquadramentos de jogo, **Then** a grama mantém continuidade visual e os caminhos e superfícies transitáveis permanecem legíveis.

### User Story 2 — Manter variação natural do mapa de noise (Priority: P1)

Como responsável pela direção visual, quero manter a variação espacial aleatória e as manchas de noise ao elevar densidade e cobertura, evitando distribuição uniforme, repetição previsível ou bordas retangulares.

**Why this priority**: O aumento de densidade só atende à direção visual se preservar a distribuição orgânica aprovada na entrega 078.

**Independent Test**: Amostrar cobertura em coordenadas de mundo nos mesmos limites exercitados pelo harness 078, exigir variação máxima de até 0,02 entre amostras vizinhas da divisa e comparar uma repetição com a mesma seed e uma distribuição com seed distinta.

**Acceptance Scenarios**:

1. **Given** a mesma geometria, seed e configuração, **When** a distribuição é regenerada, **Then** as posições e o padrão de manchas são reproduzíveis.
2. **Given** uma seed diferente, **When** a distribuição é regenerada, **Then** as posições mudam sem sair das superfícies elegíveis, sem criar duplicatas ou padrão regular evidente.
3. **Given** uma fronteira de região ou célula do mapa de noise, **When** a cobertura é amostrada dos dois lados nos pontos do harness 078, **Then** a diferença entre amostras vizinhas é inferior a 0,02 e não aparece costura retangular nas visualizações.

### User Story 3 — Preservar desempenho e superfícies livres (Priority: P2)

Como jogador, quero que o aumento da grama mantenha a cena responsiva e os caminhos, construções, praia e demais áreas excluídas livres de tufos.

**Why this priority**: A densidade ampliada deve continuar utilizável na cena, preservando as exclusões que garantem leitura e navegação.

**Independent Test**: Executar a cena principal no Player Metal de referência, aquecer e medir ao menos 300 quadros no campo e na vila, além dos enquadramentos próximo e aberto, registrando instâncias, lotes, CPU, GPU e GC; repetir inspeções das áreas excluídas.

**Acceptance Scenarios**:

1. **Given** o benchmark com a densidade ampliada e seus controles, **When** cada condição estável é medida após aquecimento, **Then** o relatório registra configuração e custo incremental e busca manter até 2 ms adicionais de GPU e 0 B/quadro de GC atribuível à submissão da grama no equipamento de referência.
2. **Given** segmentos do caminho da travessia 078, degraus/acessos da encosta, áreas junto às casas, praia e volumes de exclusão exercitados pelo harness, **When** a nova cobertura é exibida ou uma exclusão é movida e removida, **Then** nenhuma raiz fica sobre a máscara inválida ou dentro da exclusão, e as áreas restauradas recuperam a cobertura elegível.
3. **Given** os níveis de detalhe atuais, **When** a câmera percorre as distâncias usuais, **Then** as transições não produzem saltos visíveis nem perda abrupta da cobertura ampliada.

### Edge Cases

- O alvo de três vezes excede o custo de GPU ou memória observado no equipamento de referência.
- O ruído seleciona concentrações que obscurecem caminhos, entradas, pistas, degraus ou silhuetas do cenário nas imagens de produção em 480 × 270.
- O suplemento de raízes encontra locais ocupados, terreno inválido, inclinação excessiva ou exclusões já existentes.
- A distribuição alcança bordas de região, terreno ou enquadramento e revela costuras, margens vazias ou saltos de LOD.
- As qualidades Baixa e Média precisam continuar oferecendo níveis menores e utilizáveis, sem reservar ou processar desnecessariamente toda a densidade Alta.

## Requirements

### Functional Requirements

- **FR-001**: A qualidade Alta MUST produzir entre 90% e 110% de três vezes a referência de 116.214 raízes elegíveis da entrega 078 (faixa alvo: 313.778–383.506), contabilizando as raízes elegíveis sem contar duplicatas como cobertura adicional.
- **FR-002**: O sistema MUST ampliar a cobertura visível em campos e encostas usando posições adicionais válidas nas superfícies elegíveis, preservando instancing e LOD existentes.
- **FR-003**: A distribuição MUST continuar modulada pelo mapa espacial de noise aprovado, com manchas contínuas e irregulares; densidade adicional MUST seguir o mesmo campo espacial em vez de formar grade ou faixas por região.
- **FR-004**: A geração MUST permanecer determinística para geometria, configuração e seed iguais, e variar de forma válida quando a seed mudar.
- **FR-005**: Caminhos, faixa central de areia e outras superfícies de praia marcadas como inválidas, construções, acessos, áreas cultivadas quando aplicável e volumes de exclusão MUST permanecer sem novas raízes conforme as máscaras existentes; tufos costeiros autorais em transições já elegíveis são preservados.
- **FR-006**: Os níveis de qualidade Baixa, Média e Alta MUST continuar distinguíveis; as qualidades inferiores MUST selecionar subconjuntos adequados sem exigir a apresentação da densidade Alta.
- **FR-007**: A cena principal MUST manter grama com resposta à iluminação e ao vento existentes, sem regressões perceptíveis nas transições de LOD ou no ciclo de vento.
- **FR-008**: A entrega MUST registrar benchmark comparável à referência da spec 078, com pelo menos 300 quadros após aquecimento em campo e vila, em enquadramento próximo e aberto, incluindo instâncias, lotes, tempos de quadro, custo de GPU quando disponível e GC.
- **FR-009**: A validação visual MUST incluir a câmera e o percurso reais da cena jogável, com capturas antes/depois nos mesmos pontos, além de inspeção independente da leitura de caminho e cenário.
- **FR-010**: A cobertura em tela MUST ser comparada nos 75% inferiores da imagem (pixels com `y >= altura/4`) em campos e encostas, em capturas pareadas da RenderTexture 480 × 270 com câmera, horário, seed, vento e pose iguais; pixels atribuíveis à grama serão aqueles cuja diferença RGB Euclidiana exceda 20 em escala de 8 bits. O agregado das regiões avaliadas MUST aumentar ao menos 25% em relação à referência 078.
- **FR-011**: As verificações espaciais MUST reutilizar amostras de fronteira do harness 078 com diferença de noise menor que 0,02 e confirmar zero novas raízes suplementares sobre as máscaras de caminho, acesso, casas e superfícies de praia inválidas, além das exclusões dinâmicas exercitadas.

### Key Entities

- **Raiz elegível**: Posição única de grama válida no terreno e fora das máscaras e exclusões, contabilizada para a meta de densidade.
- **Campo de cobertura**: Variação espacial de noise que determina onde as manchas de grama se concentram e como suavizam entre regiões.
- **Perfil de qualidade**: Seleção de densidade e detalhe apresentada nos níveis Baixa, Média e Alta.
- **Condição de benchmark**: Cena, câmera, resolução, qualidade e estado ambiental registrados para comparar custo de desempenho.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A qualidade Alta contém de 313.778 a 383.506 raízes elegíveis únicas, aproximadamente três vezes as 116.214 da referência 078.
- **SC-002**: Em imagens pareadas de grama ligada/desligada, nos 75% inferiores de pelo menos dois campos e duas encostas a 480 × 270, a proporção agregada de pixels atribuíveis à grama (diferença RGB Euclidiana >20/255) aumenta pelo menos 25% em relação à captura equivalente da 078; entradas, degraus, caminho e silhuetas marcadas permanecem identificáveis.
- **SC-003**: Repetir geometria, configuração e seed reproduz a distribuição; mudar a seed gera outra distribuição válida. A diferença de cobertura em amostras vizinhas de fronteira é menor que 0,02 e a travessia não revela costuras nem saltos de LOD.
- **SC-004**: Contagem de raízes suplementares sobre as máscaras inválidas de caminho, degraus/acessos, áreas de casas e superfícies de praia marcadas como inválidas é zero. Tufos costeiros autorais existentes são preservados; ciclos de ativar, mover e remover exclusões restauram o estado nas regiões liberadas.
- **SC-005**: No Mac M4 Pro com Metal e condições comparáveis à spec 078, o custo incremental de GPU da qualidade Alta permanece até 2 ms como meta, e a submissão da grama registra 0 B/quadro de GC em regime estável; desvios são documentados com medições e decisão de aceite antes da conclusão.
- **SC-006**: A cena Ilha_Principal é demonstrada em Play Mode com iluminação dinâmica, vento global, a densidade ampliada e transições de LOD em operação.

## Assumptions

- A referência quantitativa é a qualidade Alta da entrega 078: 116.214 raízes elegíveis e 42.225 instâncias visíveis na condição de campo do Player naquela medição. Contagens de raízes elegíveis e de instâncias submetidas são métricas diferentes.
- “Triplo” se aplica à densidade total elegível da qualidade Alta, não à duplicação de cada perfil ou a cada quadro desenhado.
- A área de cobertura cresce sobretudo em campos e encostas elegíveis; as exclusões, caminhos e áreas construídas mantêm suas regras atuais.
- O mapa de noise, seed e seleção determinística existentes devem ser reutilizados e ajustados apenas se medições mostrarem necessidade.
- A máscara de areia cobre faixas centrais/superfícies costeiras inválidas; tufos autorais costeiros que já ocupam transições elegíveis não contam como falha e não devem ser removidos por esta entrega.
- O equipamento de referência permanece o Mac M4 Pro/Metal registrado na spec 078; outras plataformas podem ser documentadas separadamente.
- Pixels atribuíveis à grama são medidos como diferença de cor entre captura com a grama ativa e captura pareada com ela desligada, nos 75% inferiores da RenderTexture 480 × 270 (`y >= altura/4`), com luz/horário/pose/seed iguais e vento em calmaria. O teste mede área visual e não substitui revisão humana de legibilidade.
- Os pontos de comparação da cena e da travessia usam as poses registradas em `specs/078-grama-vento-global/evidence/island/observacoes.json`; a área de casas, praia, caminhos e acessos é inspecionada pela máscara espacial e pelo percurso registrado.
- A integração ocorre na cena principal existente, preservando Built-in Forward, os assets e as funcionalidades atuais.
