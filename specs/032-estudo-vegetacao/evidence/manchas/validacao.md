# Manchas de grama irregulares

As células de 8 m organizam os lotes para culling. A distribuição usa posições sorteadas pela coordenada global, manchas de densidade contínuas e uma faixa de transição de 3,6 m entre os perfis das zonas. Um ruído espacial desloca a transição e a altura varia em conjunto com as manchas. A densidade também diminui gradualmente perto do limite de declive e nos encontros entre solo gramado e areia.

A cena principal tem **52.443 tufos**, ante 98.479 no layout salvo anterior. O bake no checkout 032 produziu **51.823** sobre a geometria local daquela cópia. A auditoria da cena principal conferiu todos os pivôs, incluindo as folgas de caminhos e portas e as exclusões da praia e do eixo frontal da escadaria. [Auditoria das folgas](exclusoes.txt)

## Comparação no Player macOS

Build de desenvolvimento do Unity 6000.3.25f1, Apple M4 Pro, Metal e câmera de 480×270. O mesmo executável alternou os dois layouts e reproduziu 180 poses idênticas da subida à igreja. Foram 9 rodadas intercaladas e 1.620 quadros por variante, com VSync desligado. O tempo medido é do quadro inteiro. [Capturas nas mesmas poses](comparativo-player.jpg) · [Praia](praia.png)

| Layout | Tufos médios enviados | Pico de tufos enviados | Lotes de grama médios | p50 do quadro | p95 do quadro |
| --- | ---: | ---: | ---: | ---: | ---: |
| Anterior | 16.872 | 24.700 | 34,43 | 0,773 ms | 0,955 ms |
| Manchas irregulares | 9.005 | 12.595 | 19,06 | 0,724 ms | 0,903 ms |

A variante reduziu em **46,6%** o envio médio e atingiu a faixa de 8–10 mil prevista para o teste. O culling descartou todos os tufos nos testes de distância e enquadramento. Ao voltar à mesma pose, o renderer repetiu a submissão de 9.563 tufos.

O build terminou com zero erros. O log contém avisos de shaders do pacote de inferência, nenhum atribuído aos shaders da grama. O executável comparativo foi gerado numa cópia isolada do projeto. [Amostras](amostras-player.csv) · [Observações](observacoes-player.txt).
