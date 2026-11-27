# Feature Specification: Vegetação regional da ilha

**Feature Branch**: `083-vegetacao-ilha`  
**Created**: 2026-11-26  
**Status**: Aprovada para implementação  
**Input**: Anexo `freyja_vegetacao_prompt_v2.md`

## Escopo da primeira entrega

Implementar e validar uma região piloto na transição praia seca/entrada da vila da cena `Ilha_Principal`. O piloto informa a distribuição posterior; depois de corrigir problemas visuais e confirmar os critérios de aceite, expandir às demais regiões da ilha.

## Cenários e testes

### História 1 — Vegetação piloto legível (P1)

Como jogador, quero ver uma transição costeira com vegetação variada e natural, preservando a leitura da praia, do caminho e da vila.

**Teste independente**: comparar capturas da câmera real do jogador no mesmo ponto durante o dia e a noite, percorrer o trecho em Play Mode e inspecionar a hierarquia/ativos usados.

**Aceite**

1. Há agrupamentos assimétricos de assets existentes em escalas compatíveis, com áreas abertas entre eles; sem dispersão uniforme ou nova dependência.
2. Água, areia molhada, largura útil do caminho, acesso à vila, personagem e interações permanecem desobstruídos e legíveis de dia e à noite.
3. Materiais acompanham a iluminação/ciclo existente; movimento usa o vento compartilhado somente nos assets que já o suportem, sem sistema novo.
4. A vegetação decorativa não bloqueia navegação e não recebe colliders desnecessários.
5. São registradas capturas diurnas/noturnas da câmera real, inventário dos ativos usados, contagens e custo/performance observados, incluindo limitações.

### História 2 — Expansão regional (P2)

Depois de validar e corrigir o piloto segundo seus critérios de aceite, aplicar a gramática validada aos setores existentes (caminhos/jardins, praça, encosta, plantação e ruínas), adaptando espécies e densidade ao espaço, às rotas e aos marcos de cada setor.

**Teste independente**: capturas da câmera real em pontos representativos de cada setor, dia/noite e percurso jogável; revisão das máscaras/exclusões e medição comparável de performance.

**Aceite**

1. Cada setor tem composição própria: litoral baixo e espaçado; vila/jardins cuidados e assimétricos; áreas naturais em núcleos com clareiras; áreas agrícolas, rotas, pistas e interações livres; ruínas discretas.
2. O conjunto mantém silhuetas e paleta low-poly existentes, vila acolhedora e atmosfera mais contida restrita a zonas misteriosas.
3. Caminhos, personagens, portas, marcos e pontos interativos são legíveis nas capturas de dia e noite; a navegação funciona em Play Mode.
4. Custo é comparado no mesmo equipamento, cena, câmera e configuração antes/depois; regressões e limitações ficam documentadas.

## Requisitos funcionais

- **FR-001**: Usar prioritariamente os modelos, materiais e texturas já disponíveis no projeto, incluindo os ativos compatíveis de `ThirdParty/Nature032`, `KitPS1_022` e `Grass029`; catalogar somente os candidatos relevantes.
- **FR-002**: Começar pela transição praia seca/entrada da vila e validar visualmente o piloto antes de distribuir a composição em outros setores.
- **FR-003**: Combinar espécies e portes disponíveis em conjuntos orgânicos, sem forçar camadas ou espécies ausentes; variações de paleta são limitadas aos materiais existentes compatíveis.
- **FR-004**: Excluir água/areia molhada, caminhos úteis, escadas, edifícios, portas, áreas de interação, cultivos, rotas de NPC e elementos narrativos relevantes.
- **FR-005**: Reutilizar vento, iluminação, ciclo de dia/noite e infraestrutura de grama existentes; não alterar câmera, ciclo, terreno ou lógica de gameplay para acomodar a vegetação.
- **FR-006**: Manter instâncias decorativas sem colisão quando não bloquearem gameplay e usar organização por região compatível com a cena.
- **FR-007**: Não usar Tripo AI, compras, downloads, modelos ou sistemas complexos novos, fauna, partículas ou mecânicas narrativas.
- **FR-008**: Após validar e corrigir o piloto segundo seus critérios de aceite, estender a distribuição às regiões existentes e validar cada setor pela câmera real do jogador, durante o dia e a noite, corrigindo problemas encontrados.

## Casos de borda

- Se o trecho piloto não comportar três escalas sem apertar a navegação, usar somente as escalas que cabem.
- Se um modelo não responder ao vento compartilhado ou não integrar bem à iluminação, manter estático ou omiti-lo; não criar shader/sistema novo nesta entrega.
- Se a área segura disponível for pequena, priorizar clareiras e leitura do caminho em vez da meta experimental de cobertura do anexo.

## Critérios de sucesso

- **SC-001**: O piloto tem capturas reais pareadas de dia e noite, sem bloquear o trecho jogável nem ocultar personagem/interações.
- **SC-002**: O percurso piloto é concluído em Play Mode sem colisões decorativas ou perda de conectividade.
- **SC-003**: Toda instância adicionada é reproduzível e rastreável na hierarquia/artefato de authoring; não há dispersão repetitiva visível.
- **SC-004**: As regiões expandidas têm evidência visual diurna/noturna e comparação de performance antes/depois sem regressão não documentada.

## Pressupostos

- A cena de jogo é `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` e a grama 080 e seus filtros existentes são preservados.
- A expansão integral depende da avaliação visual e técnica do piloto e da correção dos problemas encontrados.
- `Alba` orienta massas vegetais costeiras, `Link’s Awakening` a composição de diorama e `Dredge` a atmosfera restrita às áreas misteriosas; sem cópia de ativos ou código.
- Distâncias, cobertura e paleta do anexo são referências experimentais ajustadas à escala real, não metas rígidas.
