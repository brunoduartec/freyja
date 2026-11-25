# Validação do piloto de grama

Unity 6000.3.25f1, Built-in, Apple M4 Pro com Metal. Cena `Ilha_Tripo_Caminhada_011`. Câmera do jogo com alvo de 480×270.

## Aparência e fluxo

- Três malhas originais produzidas no Blender e importadas em Y para cima, com 24 a 35 cm no asset e 9 a 18 triângulos por tufo. O layout final tem **1.246 tufos em 14 células**. A distribuição respeita a camada Grass, o relevo, as superfícies da trilha e as áreas de acesso.
- A rota a partir do spawn passou pelo convés, pela lateral do galpão e pelo terreno até o piloto, usando o controlador real em Play Mode. A chegada ficou em aproximadamente `x −31,57, y 2,15, z −31,83`, com contato com o chão.
- O controlador existente caminhou 3,91 m e voltou ao ponto inicial no trecho do piloto, em contato com o solo em 1.016 de 1.016 quadros em cada direção.
- Capturas da câmera real em [aproximação](01-aproximacao.png), [giro](02-giro.png) e [vista aberta](03-vista-aberta.png). A [mesma pose com a grama desligada](04-mesmo-giro-sem-grama.png) mostra a diferença na borda da trilha. Recorte animado do [vento](vento.webp).
- Com a câmera afastada além do alcance, foram submetidas zero células. Com a câmera próxima e apontada para fora do trecho, foram submetidas zero células. Na volta à pose fixa original, a submissão voltou a 4 células, 336 tufos e 3 lotes. [Observações brutas](observacoes.txt).

## Renderização e tempo de quadro

Três modos reproduziram as mesmas 180 poses do jogador e da câmera. Foram 9 rodadas intercaladas por modo, total de 1.620 quadros por modo, após aquecimento de 20 quadros por passagem. VSync desligado durante a amostra. Tempo de quadro por `unscaledDeltaTime` em Play Mode. Draw calls do quadro inteiro pelo Profiler Recorder. As outras contagens medem submissões do componente.

| Modo | Células médias | Instâncias médias | Chamadas de grama médias | Draw calls totais médias | p50 | p95 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Sem grama | 0 | 0 | 0 | 811,79 | 1,00 ms | 2,07 ms |
| Todos os tufos | 14 | 1.246 | 6 | 817,79 | 1,01 ms | 2,13 ms |
| Otimizado | 8,22 | 864,54 | 4,93 | 816,72 | 1,01 ms | 2,21 ms |

O culling e a redução de detalhe diminuíram em **30,6%** as instâncias submetidas diante do envio integral. Após filtrar as células, o renderer reúne instâncias da mesma malha em buffers reutilizáveis. Nas três capturas, essa etapa reduz os lotes de 29, 12 e 30 para 6, 3 e 6, respectivamente. O p95 do modo otimizado ficou **6,7%** acima da cena sem grama na amostra agregada, abaixo da meta inicial de 10%. As nove rodadas variaram e o p95 com culling ficou 0,08 ms acima do envio integral na amostra agregada. A medição comprova redução de trabalho enviado e registra o custo no editor. O custo isolado de GPU não foi medido. [Amostras CSV](amostras.csv).

A oclusão estática ficou fora deste piloto. A cena não contém dados de bake.

## Compilação e build

A recompilação final retornou zero erros e zero avisos de C#. A sessão de validação terminou sem erros novos do jogo. O build temporário explícito da cena para macOS foi concluído com sucesso. O binário é universal (Apple Silicon e Intel). O log de build registrou o shader da grama em Metal com quatro programas internos e incluiu o layout. As capturas e as métricas foram obtidas em Play Mode no Editor.
