# Validação — Grama tripla cobertura

## Resultado

Validado no Unity 6000.3.25f1, cena `Ilha_Principal`, em Play Mode e em Player Development macOS/Metal no Apple M4 Pro. A distribuição final contém **331.934 raízes únicas** na qualidade Alta (referência 078: 116.214; fator 2,86×), conservando os 72.770 suplementos anteriores da entrega 078. As novas amostras seguem o noise e as máscaras válidas.

## Evidências executadas

- `evidence/placement.json`: **18/18 verificações passaram**. Determinismo por seed, seed alternativa, rebuild localizado, exclusões dinâmicas, restauração, bounds (0 fora), superfícies (`invalidSurface=0`), inclinação, unicidade (0 duplicatas), máscara (0 suplementos inválidos), três perfis e continuidade do noise (`seamDifference=0,00199`).
- `evidence/coverage/baseline/coverage.json` e `candidate/coverage.json`: pares OFF/ON em quatro poses idênticas, 480×270, hora 12h, calmaria, FOV 58°. A cobertura agregada nos 75% inferiores passou de **13,865% para 17,877%**, crescimento relativo **28,94%**, acima do requisito de 25%. Cada pose teve `recoveries=0` e distância câmera-jogador de 3,37–5,40m.
- `evidence/coverage/`: oito imagens de cada estágio (4 ON/4 OFF). A inspeção visual mostra crescimento nos campos e encostas, caminhos e degraus ainda legíveis e distribuição irregular, sem padrão quadriculado perceptível nas regiões examinadas.
- `evidence/island/observacoes.json` e PNGs: 06h/12h/17h/22h, estado de luminária, praia, casas, caminho, zoom próximo/aberto, grama antiga/nova, calmaria/brisa/rajada e percurso de ida/volta. Capturas reais da câmera de gameplay, sem recovery. A grama acompanha iluminação ambiente/ciclo do dia e vento global. O código existente preserva a contribuição do sol/sombras e seleciona até quatro luzes locais por região para o shader (`CoastalGrass.Lit.cs` e `LitCoastalGrass.shader`). As capturas ligadas/desligadas da luminária não formam um par de câmera idêntica com a grama em observação, portanto esta evidência não quantifica o efeito local visual.
- `evidence/benchmark-player-metal.json`: 16 condições, 5s de aquecimento e 300 frames por condição, Player Development, Apple M4 Pro, Metal, 960×540. Caso comparável 078 → 080:

| Condição | GPU p95 078 → 080 | Diferença | Instâncias 078 → 080 | Cache de matrizes 078 → 080 |
|---|---:|---:|---:|---:|
| Campo Alta | 1,483 → 2,331 ms | +0,848 ms | 42.225 → 98.303 | 11,84 → 29,81 MB |
| Zoom próximo | 1,558 → 2,835 ms | +1,277 ms | 51.178 → 130.639 | 11,84 → 29,81 MB |
| Zoom aberto | 1,598 → 2,166 ms | +0,568 ms | 30.426 → 72.260 | 11,84 → 29,81 MB |
| Vila Alta | 1,642 → 2,149 ms | +0,507 ms | 22.159 → 58.477 | 11,84 → 29,81 MB |
| Costa noite | 1,479 → 2,020 ms | +0,540 ms | 27.341 → 61.818 | 11,84 → 29,81 MB |

O delta máximo observado é **1,277 ms**, dentro da meta de até 2 ms. GC atribuível à submissão da grama ficou em **0 B/frame** em todas as condições. `frameGcBytes` geral de 368 B/frame inclui a cena e outros sistemas. Os draws da cena não são draws isolados da grama; `grassDraws` é reportado separadamente no JSON. O cache cresce conforme esperado pelo aumento de raízes.

## Revisão visual e limitações

As imagens confirmam grama abundante em primeiro e segundo plano, mantendo áreas pavimentadas reconhecíveis. Há uma faixa magenta no horizonte de algumas capturas de praia/encosta; ela também aparece em evidências prévias 078 e não foi introduzida pela distribuição 080, portanto fica registrada como problema visual preexistente de cenário/material, fora desta alteração. A sequência 080 prova vento por estado global e vistas temporais; não substitui uma comparação de pixels entre fases do vento. Iluminação local usa a implementação existente e não foi alterada nesta entrega; um ensaio fotométrico A/B dedicado pode ser adicionado em sequência se for necessário validar o peso visual da luminária sobre a grama.

O build do Player concluiu com 0 erros e 346 warnings agregados do projeto; não houve erro de compilação associado à mudança da grama. Avisos de API obsoleta vieram do harness de build existente. Console não foi limpo. O asset de density foi salvo e a cena foi confirmada limpa no Editor antes do benchmark; o benchmark preservou `ProjectSettings.asset`.

## Artefatos

- Runtime: `Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.Lit.cs`
- Layout: `Freyja/Assets/_Project/Art/Grass029/IslandDensity078.asset`
- Harnesses e captura: `Tools/Grass078/IntegrateIsland.cs`, `CheckPlacement.cs`, `CaptureCoverage080.cs`, `CaptureIsland080.cs`
- Capturas e relatórios: `specs/080-grama-tripla-cobertura/evidence/`
