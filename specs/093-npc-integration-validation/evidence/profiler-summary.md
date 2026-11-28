# Resumo do Unity Profiler

Capturas do Editor com três NPCs na cena principal, `Time.timeScale=1`. Amostras pequenas e agregadas; não representam build.

| Estado | Frames | CPU mediana / p95 / máx. | Main thread mediana | Render mediana | GPU mediana / p95 / máx. | GC Alloc mediano / p95 / máx. | >16,667 ms |
|---|---:|---:|---:|---:|---:|---:|---:|
| Movimento simultâneo | 100 | 4,627 / 5,053 / 5,523 ms | 4,583 ms | 4,143 ms | 3,111 / 3,907 / 4,349 ms | 12.345 / 12.753 / 13.012 B/frame | 0/100 |
| Idle | 20 | 4,448 / 4,918 / 5,244 ms | 4,222 ms | 4,171 ms | 3,646 / 3,772 / 3,842 ms | 12.305 / 12.652 / 12.769 B/frame | 0/20 |
| Três NPCs sentados/slots concorrentes | 20 | 4,438 / 5,046 / 5,255 ms | 4,187 ms | 4,138 ms | 3,606 / 4,060 / 7,310 ms | 12.305 / 12.849 / 12.972 B/frame | 0/20 |

Recorte de referência 16,667 ms apenas para comparar amostras; não é budget oficial. Diagnóstico separado mostrou frame CPU 4,038 ms, GPU 3,336 ms, Main 2,850 ms; draw calls 7.111, batches 140 e 5,03 M triângulos em uma amostra. Os dados de memória são do Editor global (residente ~4,34 GB, Mono ~1,3–1,4 GB) e não isolam custo NPC. Contador de requests/pathfinding não estava disponível. Nenhum gargalo evidente atribuível à integração foi encontrado; não foi feita otimização.
