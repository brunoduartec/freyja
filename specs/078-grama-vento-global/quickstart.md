# Uso e validação

Abra `Ilha_Principal` no Unity 6000.3.25f1. No objeto **Coastal Grass — Ilha**, o componente **Coastal Grass** contém:

- `Short Grass`, `Wild Grass` e `Coastal Grass` definem cor de raiz e ponta, largura, altura, suavização da normal, flexão e contraluz. Elevações configuráveis distribuem os perfis sobre o layout existente. Os tufos costeiros autorais conservam o perfil costeiro.
- `Quality` seleciona baixa, média ou alta. A seleção estável usa frações de 40%, 70% e 98% da distribuição elegível, com redução adicional perto da costa. Média e Alta incorporam 25% e 96% do suplemento de densidade, respectivamente. O suplemento usa o detalhe próximo e entra parcialmente no LOD intermediário. A cobertura mais distante usa a distribuição-base. `Variation Seed` determina essa seleção e a variação nas fronteiras dos perfis. `LOD Range Scale` ajusta o alcance e `Show Regions` desenha as regiões na Scene View.
- `Cast Grass Shadows` permite medir a projeção de sombra perto da câmera. O perfil de produção recebe sombras do cenário com projeção própria desligada. `Local Lights` vincula fontes pontuais reais. Cada região considera até quatro fontes sobrepostas. A grama acompanha o relógio e o ambiente Trilight da cena.
- O componente **Global Wind Controller** contém calma, brisa e vento forte, direção horizontal e suavização. Mude o preset em Play Mode pelo Inspector ou por `CurrentPreset`. Outro shader pode incluir `WindCore.hlsl` e chamar `FreyjaWindOffset` com a raiz em espaço de mundo. O material aplica um peso crescente até a ponta.

A colocação parte do layout da ilha. No asset `IslandDensity078`, `Patch Size`, `Contrast` e `Offset` controlam o mapa de ruído em coordenadas de mundo. Duas escalas de ruído deslocam e variam as manchas. Após ajustar o mapa, execute `IntegrateIsland.cs` para regenerar também o suplemento. A correção de superfície usa Terrain, vias e acessos, incluindo as camadas TerraBatida e Lajes. Depois de editar terreno, portas, raízes ou vias, inclusive ao usar o comando histórico `Bake Island Grass`, use `IntegrateIsland.cs` em Edit Mode com a cena limpa para atualizar a máscara da ilha e o suplemento determinístico de densidade. Para exclusões em runtime, coloque um **Grass Exclusion Volume** onde a grama deve desaparecer e atribua o renderer. Ligar, mover ou desligar o volume refaz somente as regiões sobrepostas. O projeto ainda não oferece um fluxo jogável de construção ou cultivo.

Em outra cena, atribua câmera, layout, material de referência e as três malhas aprovadas ao **Coastal Grass**. Atribua **Lit Coastal Grass** em `Lit Material`, vincule as luzes locais e adicione um único **Global Wind Controller** ativo. Adicione os volumes de exclusão da cena. Use `Grama_Piloto_078` como exemplo reproduzível. Ela usa a RenderTexture de 480 × 270 e fica fora das cenas de build.

## Rodar a validação

Execute os arquivos de `Tools/Grass078` com `script eval` do Unity MCP ou CLI, passando o caminho absoluto em `file`:

- `CreatePilot.cs` recria a cena de ensaio quando ela está ausente. `ProbePilot.cs`, em Play Mode no piloto, captura sol, sombra, luz pontual e vento.
- `CaptureIsland.cs`, em Play Mode na ilha, posiciona o jogador pela API real, percorre a subida nos dois sentidos via input sintético e captura a RenderTexture. Os extremos de FOV são condições de ensaio. Em produção, a câmera usa o controle e o FOV que já tinha.
- `CheckPlacement.cs` testa determinismo, geometria, exclusões, perfis e ciclo do controlador. `CheckWindPause.cs` testa pausa da simulação.
- No Editor em Play Mode, chame `GrassBenchmark078.Begin(caminhoAbsoluto)` para gravar a matriz. Para o Player, execute `BuildBenchmark.cs` em Edit Mode. Ele compila a cena jogável como Development macOS com Frame Timing Stats e restaura as configurações. Use o executável dentro do app em `$TMPDIR/FreyjaGrass078Benchmark/` com `-force-metal -screen-fullscreen 0 -screen-width 960 -screen-height 540 -freyja-grass-benchmark caminhoAbsoluto.json`. O Player termina após escrever o JSON.

O modo `Legacy` serve de comparação com o antigo renderer procedimental. O modo `Off` deixa a cobertura do terreno visível. Os tufos autorais originais estão preservados inativos na hierarquia, para reversão da cena pelo histórico.
