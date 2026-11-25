# Resultado do estudo do mar

**Revisão visual** Na praia, uma faixa de espuma se forma no raso, avança até a areia e se dissolve. As duas variantes usam uma ondulação contínua, larga e de baixo contraste em alto-mar. A espuma circular aparece ao pé dos pilares do píer. B mantém o relevo perto da câmera e o reduz progressivamente na direção do horizonte.

**Recomendação** Manter A como padrão nesta iteração. A espuma e a transição de cor dão leitura de água rasa junto ao píer. B se distingue mais na vista do costão, onde o volume torna a ondulação aberta mais visível. Junto ao píer, a diferença é mais discreta porque a enseada amortece as ondas em ambas as propostas.

- [Quadro comparativo](evidencias/comparacao.png)
- [Píer · referência, A e B lado a lado](evidencias/comparacao-pier.mp4)
- [Costão · referência, A e B lado a lado](evidencias/comparacao-encosta.mp4)
- [Praia · referência, A e B lado a lado](evidencias/comparacao-praia.mp4)

As três tomadas duram 10 segundos a 24 quadros por segundo, com fase inicial de câmera parada e deslocamento no restante. A câmera de jogo renderizou cada versão a 480 × 270. Na praia, os quadros mostram a faixa chegando à linha de areia e enfraquecendo entre ondas. No píer, a espuma aparece junto a pilares e rochas. Da vista elevada do costão, o recorte distante ainda é perceptível como transição para o céu.

## Tempo mediano por quadro

| Vista | Referência | A | B |
| --- | ---: | ---: | ---: |
| Píer | 1,57 ms | 1,56 ms | 1,60 ms |
| Costão | 0,80 ms | 0,79 ms | 0,81 ms |
| Praia | 1,04 ms | 1,04 ms | 1,10 ms |

Medição do tempo total por quadro em Play Mode no Unity 6000.3.25f1, Built-in, Metal, Apple M4 Pro e câmera a 480 × 270. VSync e limite de quadros desativados para a medição. Três passagens com ordem alternada, 40 quadros de aquecimento e 160 amostras por combinação em cada passagem. A coleta ocorreu sem gravação. As diferenças são pequenas e retratam apenas este editor e estes enquadramentos. [Amostras brutas](evidencias/tempos-por-quadro.csv)

## Verificação

- Unity abriu e salvou a cena 011 em `Freyja/` dentro da worktree `031-estudo-mar`. Essa cópia usa os assets locais do cenário que já estavam em desenvolvimento na árvore principal de trabalho.
- O editor confirmou que o shader da revisão compilou sem erros e registrou zero erros durante a captura. Houve um aviso preexistente do pacote Unity AI sobre acesso à API de conta.
- As teclas 1, 2 e 3 foram exercitadas com eventos de teclado no Play Mode e selecionaram as três variantes. A cena abre com A habilitada.
- Cada vídeo foi decodificado com 240 quadros e duração de 10 segundos.
