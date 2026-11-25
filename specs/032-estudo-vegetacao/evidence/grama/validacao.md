# Validação da grama por zona

Cena `Ilha_Tripo_Caminhada_011`, Unity 6000.3.25f1 Built-in, Apple M4 Pro e Metal. Capturas da câmera do jogo em 480×270. [Nove vistas](vistas-da-ilha.png) · [Mapa aprovado](../../mapa-grama.png).

## Distribuição gerada

| Zona | Tufos | Solo apto amostrado | Área de manchas |
| --- | ---: | ---: | ---: |
| Cais | 5 | 7,2 m² | 1,2 m² |
| Subida do cais | 5.836 | 348,4 m² | 327,6 m² |
| Praça | 375 | 338,6 m² | 74,5 m² |
| Laterais da igreja | 165 | 129,7 m² | 27,3 m² |
| Quintais | 3.161 | 944,7 m² | 377,9 m² |
| Encostas | 3.847 | 441,0 m² | 255,8 m² |
| Casa isolada | 38 | 20,4 m² | 6,6 m² |
| Crista norte | 44 | 59,4 m² | 11,9 m² |
| **Total** | **13.471** | | |

A área de solo apto usa amostragem de 0,222 m e as exclusões finais das malhas reais de acesso, 17 marcadores de porta e a máscara de areia. Os totais do estudo usavam uma estimativa em grade de 0,5 m. A distribuição concentra tufos perto de todos os pontos sugeridos. Uma verificação independente de todos os pivôs mediu **0,3003 m** de folga mínima às malhas das passagens e **1,266 m** aos marcadores das portas. Houve zero pivôs nos polígonos de exclusão de praia e escadaria. A praça, o caminho e a faixa de banho estavam livres nas capturas. A caminhada de 3,01 m na subida foi executada com o controlador real, com contato com o solo em 691 de 694 quadros.

Os dois bakes feitos nas cópias da cena produziram um layout idêntico, com 91 células e SHA-256 `a0e50ba31262f5b9c86da7f8e44d76a716fe154917e3f896771ad9aa7eff9348`. O piloto 029 mantém o layout próprio. As duas cenas salvas apontam para `IslandGrassLayout.asset`.

## Culling e tempo de quadro

Cada modo repetiu as mesmas 180 poses entre a subida e a igreja por 9 rodadas, totalizando 1.620 quadros por modo. O tempo é do quadro inteiro em Play Mode. A coluna de instâncias mede submissões; as draw calls são do quadro inteiro, lidas pelo Profiler Recorder.

| Modo | Instâncias médias | Lotes de grama médios | Draw calls médias | p50 | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Sem grama | 0 | 0 | 1.079,16 | 1,18 ms | 2,37 ms |
| Envio integral | 13.471 | 55 | 1.115,46 | 1,31 ms | 2,40 ms |
| Otimizado | 2.022,51 | 9,35 | 1.088,63 | 1,20 ms | 2,35 ms |

O modo otimizado reduziu em **85,0%** as instâncias enviadas em relação ao envio integral. O teste por distância e o teste por enquadramento resultaram em zero instâncias. A pose de retorno repetiu exatamente a contagem anterior de 6.278 instâncias. O p95 ficou na faixa da cena sem grama nesta amostra de editor. Recompilação com zero erros e avisos de C#. Console sem erros na sessão de validação. [Amostras](amostras.csv) · [Observações](observacoes.txt).
