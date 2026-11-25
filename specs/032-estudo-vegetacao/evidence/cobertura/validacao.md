# Cobertura ampliada de grama

A cena passou de **13.471 para 98.479 tufos** em 127 células. O bake selecionou **3.063,7 de 3.085,1 m²** aptos para receber grama, ou **99,3%**. Essa é a área que recebe tufos. A projeção das folhas varia com a altura, o vento e o nível de detalhe.

Os núcleos mais densos chegam a 35 tufos/m² na subida do cais e a 34 nas encostas. A distribuição passou a preencher o solo apto entre as zonas. Em solo com predominância forte da camada Grass, inclui encostas até 52°. Os trechos de solo misto usam 40° como limite. A praia protegida, a escadaria frontal e as passagens continuam fora da distribuição. A auditoria dos 98.479 pivôs encontrou folga mínima de 0,3001 m dos caminhos e 1,2506 m dos marcadores de portas.

[Antes e depois](antes-e-depois.png) · [Praia](09-praia.png) · [Contagens e áreas por zona](zonas.txt).

## Validação na câmera do jogo

Unity 6000.3.25f1, câmera a 480×270, Metal e Apple M4 Pro. O controlador real percorreu 3,01 m na subida. Os testes de distância e enquadramento descartaram todas as células quando a câmera saiu do alcance ou apontou para fora da ilha. A pose de retorno repetiu a contagem de 17.487 tufos.

| Modo | Instâncias médias | Lotes médios de grama | p50 | p95 |
| --- | ---: | ---: | ---: | ---: |
| Sem grama | 0 | 0 | 1,39 ms | 2,73 ms |
| Envio integral | 98.479 | 194 | 3,11 ms | 3,56 ms |
| Culling e detalhe por distância | 16.866,89 | 34,42 | 1,70 ms | 2,68 ms |

Os três modos repetiram 1.620 quadros por modo. O culling reduziu **82,9%** das instâncias enviadas. Após os testes de visibilidade, o renderer agrupa até 511 instâncias da mesma malha por chamada. Na pose da subida, a contagem caiu de 70 para 36 lotes mantendo os mesmos tufos. O layout gerado foi idêntico nas duas cópias do projeto. A compilação do C# terminou com zero erros e avisos e o Console ficou sem erros durante o teste. [Amostras](amostras.csv) · [Observações](observacoes.txt).
