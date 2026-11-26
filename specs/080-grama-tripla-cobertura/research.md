# Pesquisa técnica — Grama tripla

## Decisões

### Aumentar a amostragem determinística do suplemento

- **Decisão**: reutilizar o bake de `Tools/Grass078/IntegrateIsland.cs`, a seleção de `GrassDensityLayout` e o mapa de noise por coordenadas de mundo. Aumentar amostras tentadas por raiz-base mantendo o corte de `PlacementScore < SampleCoverage`; limitar duplicação por célula de ocupação e revalidar pelo Terrain, vias, acessos e filtros de superfície.
- **Implementação planejada**: testar 21 tentativas determinísticas por raiz-base com salts fixos; o bake valida a população elegível no intervalo aprovado antes de salvar. Alterar esse orçamento só se a contagem real ficar fora da faixa.
- **Motivo**: o sistema já separa layout-base de raízes adicionais pré-computadas e já possui placement score hash determinístico. A densidade deve crescer sem mexer nas malhas ou distribuir em grade.
- **Alternativas consideradas**: escalar as raízes existentes (não adiciona área nem cobertura); aumentar uniforme todos os pontos-base (mantém lacunas em regiões esparsas e enfraquece as manchas); migrar para compute/indirect (complexidade e pipeline não justificadas antes de medir o custo).

### Medir cobertura observável

- **Decisão**: comparar pares on/off com mesma pose e ambiente, analisando os 75% inferiores (`y >= altura/4`) de cada RenderTexture. Um pixel conta como atribuível à grama quando a diferença RGB Euclidiana é maior que 20/255; agregar pixels alterados e área ROI sobre quatro capturas e calcular a variação contra a referência 078.
- **Motivo**: a contagem de raízes e de matrizes por LOD não prova aumento em tela; a comparação pareada mantém o chão e as demais geometrias como referência interna.
- **Alternativas consideradas**: estimar cobertura apenas pela contagem do asset (não prova visibilidade) ou comparar imagens da cena inteira sem uma máscara de grama desligada (confunde terreno/iluminação com vegetação).

### Definir o triplo como inventário e conferir aparência separadamente

- **Decisão**: usar de 313.778 a 383.506 raízes válidas únicas como meta Alta, mas medir cobertura visual por imagens alinhadas e instâncias submetidas por câmera/LOD. Não equiparar contagem de asset, quantidade de draws e área coberta.
- **Motivo**: a referência 078 registra 116.214 raízes elegíveis e 42.225 instâncias no enquadramento de campo no Player; transições de LOD duplicam apresentações de uma mesma raiz em fases distintas.
- **Alternativa considerada**: definir sucesso apenas por número de objetos, rejeitada pois não comprova a aparência solicitada.

### Dimensionar qualidade e LOD com o benchmark

- **Decisão**: manter a seleção ordenada das três qualidades e o renderer instanciado atual; ajustar frações adicionais e a distribuição entre detalhes apenas após captura e medição comparável no Player.
- **Motivo**: a medição 078 da qualidade Alta acrescentou 0,62 ms GPU no campo e 0 B/quadro de GC do renderer no M4 Pro, mas esse custo não deve ser extrapolado linearmente para quase 3× de raízes.
- **Alternativas consideradas**: presumir custo proporcional, ignorar o custo ou trocar o pipeline. Todas deixam risco não medido.

## Evidências de base consultadas

- `specs/078-grama-vento-global/spec.md`, `plan.md`, `tasks.md`, `research.md`, `validacao.md` e `data-model.md`.
- `specs/078-grama-vento-global/evidence/benchmark-player-metal.json` e `evidence/placement.json`.
- `Tools/Grass078/IntegrateIsland.cs`, `CheckPlacement.cs`, `CaptureIsland.cs`, `BuildBenchmark.cs`.
- `Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.Lit.cs`, `GrassDensityLayout.cs` e `CoastalGrassAuthoring.Island.cs`.

## Limitações a observar

Os 116.214 pontos elegíveis são posições únicas pré-computadas, não raízes visíveis simultâneas. As amostras por célula e a seleção por qualidade/LOD determinam o que é submetido; capturas, estatísticas do componente e benchmark por condição serão usados em conjunto. Resultados do Player de referência não predizem outras GPUs.
