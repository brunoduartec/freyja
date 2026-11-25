# Revisão de densidade

O trecho da subida do cais passou de 1.246 para 6.539 tufos em 20 células. O espaçamento diminuiu de 31 para 23 cm, a densidade mínima das manchas aumentou e a distribuição passou a incluir a encosta gramada junto à mesma trilha. As máscaras de areia, caminhos, acessos e obstáculos foram mantidas. [Antes e depois, nos mesmos enquadramentos](antes-e-depois.png) · [Vento animado](vento.webp).

A caminhada de 3,91 m e o retorno foram validados em Play Mode. O teste com câmera distante e o teste com câmera apontada para fora submeteram zero tufos. Ao retornar à pose do giro, a submissão voltou a 2.926 tufos. [Observações](observacoes.txt).

## Medição no Unity

Apple M4 Pro, Metal, câmera 480×270. Foram 1.620 quadros por modo e as mesmas poses no trecho. As instâncias são contadas na submissão e o tempo corresponde ao quadro completo em Play Mode.

| Modo | Instâncias por quadro | Lotes de grama por quadro | p50 | p95 |
| --- | ---: | ---: | ---: | ---: |
| Sem grama | 0 | 0 | 1,10 ms | 2,26 ms |
| Todos os tufos | 6.539 | 27 | 1,15 ms | 2,40 ms |
| Culling e detalhe por distância | 4.433,83 | 18,54 | 1,13 ms | 2,22 ms |

A submissão caiu 32,2% em relação ao envio integral. O p95 da versão otimizada ficou dentro da meta original na amostra do editor. [Amostras CSV](amostras.csv).
