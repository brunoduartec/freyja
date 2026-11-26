# Plano de implementação: Triplicar densidade e cobertura da grama

**Branch**: `080-grama-tripla-cobertura` | **Data**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Resumo

Ampliar a população válida de grama Alta de 116.214 para aproximadamente 348.642 raízes únicas e aumentar a cobertura visual em campos e encostas da `Ilha_Principal`. Reutilizar o renderer `RenderMeshInstanced`, três malhas/níveis de detalhe, máscaras de superfície, exclusões e noise contínuo em espaço de mundo da entrega 078. A geração do suplemento deve ser expandida deterministicamente e pontuada pelo mesmo noise, sem duplicar bases, ocupar caminhos nem introduzir padrões regulares. Validar cobertura observável e custo na câmera real e no Player Metal.

## Contexto técnico

- **Linguagem/versão**: C# com Unity 6000.3.25f1.
- **Dependências primárias**: Unity Built-in Forward existente; GPU mesh instancing (`Graphics.RenderMeshInstanced`); Unity MCP e Unity CLI para Editor, cena, Play Mode, captura e build.
- **Armazenamento**: assets Unity serializados e documentação/evidências versionadas na pasta da feature.
- **Testes**: harness reproduzível `Tools/Grass078/CheckPlacement.cs`, `CaptureIsland.cs`, `GrassBenchmark078` e `BuildBenchmark.cs`; harnesses novos somente se necessários para critérios ainda não cobertos. Unity Test Runner e Console para regressões.
- **Plataforma-alvo**: cena principal do Freyja; referência de desempenho macOS ARM64, Apple M4 Pro, Metal, Player Development a 960×540 com RenderTexture 480×270, comparável à evidência 078.
- **Tipo de projeto**: jogo Unity 3D existente.
- **Objetivos de performance**: medir pelo menos 300 quadros após aquecimento por condição; meta de aumento incremental até 2 ms de GPU e 0 B/quadro de alocação da submissão estável; registrar p50/p95, CPU, GPU disponível, draw calls, lotes, instâncias, matrizes/cache e GC.
- **Restrições**: manter Built-in, shaders e materiais de produção, iluminação, vento, input, câmera, ciclo dia/noite, terrenos e GUIDs; respeitar máscaras, volumes e espaços transitáveis. Não importar a referência Endless Meadow nem substituir arquitetura sem gargalo medido.
- **Escopo**: ampliar o campo de raízes elegíveis e a área coberta em campos/encostas, ajustar escalonamento por qualidade/LOD se necessário, manter noise/seed e registrar validação visual e benchmarks. Não alterar aparência do shader, sistema global de vento ou mecânicas de jogo sem necessidade demonstrada.

## Verificação da constituição

- Constituição 1: especificação aprovada, preservar fatos e propostas; **passa**.
- Constituição 2: Unity MCP será ambiente de edição/Play Mode e Blender não é necessário para posições de tufos já existentes; **passa**.
- Constituição 3: comparar silhuetas e leitura pixelada pela câmera de produção; **passa**.
- Constituição 4: não há mudança em dedução ou cânone narrativo; manter pistas/caminhos legíveis; **passa**.
- Constituição 5: mapear cada tarefa a código, asset, cena ou evidência; **passa**.
- Mudança de pacote, render pipeline ou sistema de jogo: nenhuma prevista; **passa**.

## Pesquisa e decisões

1. Preservar `IslandDensity078.SampleCoverage` como fonte única de variação espacial em coordenadas globais. Suplementos adicionais são amostrados nas manchas de maior coverage por geração determinística, não por incremento uniforme de contagem dentro de cada célula.
2. Rever `Tools/Grass078/IntegrateIsland.cs` para permitir mais candidatos independentes por raiz elegível, mantendo teste de ocupação espacial, altura/inclinação/superfície, filtragem de ruas/portas e máscara da ilha. Aumentar o raio/faixa de amostragem apenas se o bake demonstrar que os locais mais próximos se esgotam.
3. Rever o seletor por qualidade e os gates de LOD em `CoastalGrass.Lit.cs`: a população Alta precisa chegar ao alvo; Baixa/Média seguem subconjuntos determinísticos. Contar raízes únicas uma vez no inventário e medir matrizes/instâncias efetivamente submetidas por condição, documentando multiplicação por variantes de LOD.
4. Manter RenderMeshInstanced e limites 511 por chamada. Considerar redução de geometria distante, fade, escala de draw distance ou maior número de batches antes de qualquer arquitetura indirect/compute; nova técnica só entra com evidência de gargalo que as opções compatíveis com Built-in não resolvam.
5. Referência quantitativa e benchmark: `specs/078-grama-vento-global/validacao.md`, `evidence/benchmark-player-metal.json`, `Tools/Grass078/IntegrateIsland.cs`, `CheckPlacement.cs` e `GrassBenchmark078.cs`. O campo atual reportou +0,62 ms de GPU para 42.225 instâncias submetidas; o valor é base para comparação, não garantia para 3×.

## Estrutura do projeto

```text
Freyja/Assets/_Project/Scripts/Environment/
├── CoastalGrass.Lit.cs                 # seleção/LOD e reconstrução das submissões
└── GrassDensityLayout.cs               # sampling de noise e asset dos candidatos adicionais
Freyja/Assets/_Project/Editor/
└── CoastalGrassAuthoring.Island.cs     # amostragem e filtro sobre terreno e superfícies
Tools/Grass078/
├── IntegrateIsland.cs                  # bake da máscara e suplemento determinístico
├── CheckPlacement.cs                   # determinismo, exclusões, noise e integridade
├── CaptureIsland.cs                    # capturas da câmera real e travessia
└── BuildBenchmark.cs                   # Player Development para medições Metal
Freyja/Assets/_Project/Art/Grass029/
├── IslandDensity078.asset              # noise e novas raízes pré-computadas
└── IslandSurfaceMask078.asset          # exclusões de superfícies inválidas
Freyja/Assets/_Project/Scenes/
└── Ilha_Principal.unity                 # integração jogável
specs/080-grama-tripla-cobertura/
├── research.md
├── data-model.md
├── quickstart.md
└── evidence/                            # contagens, screenshots, testes e benchmarks
```

**Decisão estrutural**: alterar somente o renderer de ambiente existente, ferramenta de bake associada, assets 078, cena principal e evidências/spec. Guardar fontes editáveis na pasta original e não duplicar cena. Se mudanças locais aparecerem ao integrar a `main`, preservá-las e combinar as alterações conscientemente.

## Portões de design

- Meta de inventário Alta: 313.778 a 383.506 posições válidas e únicas, por algoritmo estável.
- Meta visual: amostras antes/depois em posições repetíveis de campos e encostas; nenhuma grade, costura ou invasão de áreas excluídas. Um aumento no asset sem aumento observável não conta como sucesso.
- Medida visual quantitativa: na resolução interna de 480 × 270, comparar capturas pareadas de grama ligada/desligada sob vento em calmaria; nos 75% inferiores (`y >= altura/4`) em dois campos e duas encostas, pixels cuja diferença RGB Euclidiana excede 20/255 medem área atribuível à grama. Exigir aumento agregado de pelo menos 25% sobre a referência 078 e revisão humana da leitura dos degraus, caminho, entradas e silhuetas.
- Integridade espacial: reutilizar o ensaio de divisas de `CheckPlacement.cs` (variação máxima <0,02 entre amostras vizinhas) e exigir zero raízes em máscaras inválidas de caminho, degraus/acessos, área de casas, praia e volumes de exclusão.
- Respeitar a máscara atual: contar raízes suplementares sobre superfícies inválidas como falha, mas preservar tufos autorais costeiros em transições que o filtro 078 aceita.
- Metas de qualidade: Baixa, Média e Alta permanecem ordenadas e distintamente mais densas; não duplicar o cache/processamento Alta sem necessidade.
- Metas de performance: benchmark reproduzível no mesmo equipamento, configuração, câmera e condições descritas na spec. Comparar grama OFF/078/080; registrar desvios às metas em vez de esconder perda com média de FPS.
- Gate pós-design: avaliação cruzada de `spec.md`, `plan.md`, `tasks.md` com `speckit-analyze` antes de implementar.

## Complexidade

Nenhuma exceção à constituição ou dependência nova. O aumento de população e cache é requisito da entrega; será dimensionado com medições de memória, batches e GPU.
