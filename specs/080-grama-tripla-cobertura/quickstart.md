# Guia rápido de validação — Grama tripla

## Pré-requisitos

- Unity 6000.3.25f1 aberto no projeto `Freyja/`, em Edit Mode, com `Ilha_Principal` e sem alterações de cena pendentes.
- Instância Unity MCP conectada à raiz correta. Consultar `Tools/MCP/README.md` e [spec 078](../078-grama-vento-global/quickstart.md) para setup completo.
- M4 Pro/Metal Player Development para benchmark de referência.

## Preparar a distribuição

1. Abrir `Ilha_Principal.unity` e, antes do bake, iniciar Play Mode e rodar `Tools/Grass078/CaptureCoverage080.cs` para salvar a referência 078 ON/OFF nas quatro poses pareadas. O script classifica o baseline pela contagem atual e grava a métrica dos 75% inferiores.
2. Parar Play Mode sem salvar alterações transitórias da captura e confirmar Edit Mode com a cena limpa.
3. Executar `Tools/Grass078/IntegrateIsland.cs` com `script eval` do Unity MCP. O script atualiza layout autoral, máscara e asset de suplemento. Confirmar a quantidade projetada e a validação de faixa antes de salvar.
4. Recarregar a cena após recompilar assets. Em Play Mode, confirmar o componente `Coastal Grass — Ilha`, `Quality=High`, câmera de produção e materiais Lit/instanced ativos.

## Verificar regras espaciais e noise

- Executar `Tools/Grass078/CheckPlacement.cs` via `script eval`. Confirmar repetição da mesma seed, distribuição alternativa com seed diferente, raízes únicas válidas, exclusões, perfil, boundaries e continuidade amostrada do noise.
- Conferir caminhos, acessos, casas, praia e encosta em Scene/Game view. Mover e remover uma exclusão no harness/runtime e confirmar atualização só nas regiões afetadas e restauração ao removê-la.
- Confirmar que raiz elegível é contada uma única vez, separada de matrizes de apresentação LOD.

## Conferir a aparência no jogo

- Rodar `Tools/Grass078/CaptureCoverage080.cs` em Play Mode para salvar os pares 080 nas mesmas quatro poses do baseline. O `coverage.json` candidato calcula a variação agregada e informa se atinge +25%.
- Usar `Tools/Grass078/CaptureIsland080.cs` para salvar evidência sem sobrescrever 078 e revisar percurso, campo/encosta, vila/caminho, costa, vento e iluminação. Inspecionar pelo menos dois campos e duas encostas, zoom próximo e aberto e travessia sob vento.
- Verificar que patches continuam orgânicos, contíguos e sem blocos/grelha/costuras, que as silhuetas do jogador e cenário preservam leitura e que luz e vento continuam ativos.

## Benchmark

- Rodar `Tools/Grass078/BuildBenchmark.cs` em Edit Mode para produzir Player Development macOS, usando a cena de produção.
- Rodar o Player com `-force-metal -screen-fullscreen 0 -screen-width 960 -screen-height 540 -freyja-grass-benchmark <caminho-do-json>`. O executável e o coletor estão descritos em `specs/078-grama-vento-global/quickstart.md`.
- Para cada condição estável: 5 s de aquecimento e 300 quadros ou mais; medir OFF, base 078 quando disponível e Alta 080 nos pontos de campo, vila e enquadramentos próximo/aberto. Registrar p50/p95, CPU, GPU disponível, instâncias, lotes, draws, matrizes/cache e alocações.
- Não aceitar o resultado pelo count isolado. Relacionar aumento de cobertura nas capturas ao custo incremental medido. Documentar indisponibilidade de telemetria e qualquer desvio da meta antes da avaliação.

## Saída esperada

O relatório em `specs/080-grama-tripla-cobertura/validacao.md` liga commits/artefatos à cena, contagem única por perfil, imagens alinhadas, resultados placement/Edit/Play Mode, benchmark Player e limitações.
