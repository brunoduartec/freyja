# Validação da grama e do vento global

A cena jogável usa o renderer instanciado nos três perfis de grama. A grama opaca reage ao sol, ao ambiente Trilight, às sombras do cenário e às luminárias locais. A projeção de sombras próprias fica disponível por qualidade. A distribuição combina as áreas já autoradas com um suplemento determinístico, filtrado pelo terreno real e modulado por duas escalas de ruído em espaço de mundo.

## Distribuição

A qualidade Alta passou de 49.885 para **116.214 raízes elegíveis**. São 31.354 de grama baixa, 76.263 selvagens e 8.597 costeiras. O suplementar adiciona cobertura próxima e participa seletivamente do LOD intermediário. A camada mais distante conserva a densidade-base.

O campo de ruído medido variou de 0,416 a 1,0. Na maior diferença observada entre pontos separados por 1 cm ao cruzar as divisas amostradas, a diferença foi 0,00199. O Game Designer conferiu manchas irregulares e estáveis durante a ida e volta, sem cortes visíveis nos três enquadramentos examinados. Há uma borda de mancha mais marcada perto do personagem em uma encosta. Ela se manteve no mesmo lugar nos três FOVs.

## Evidência de jogo

- [Encosta na câmera jogável](evidence/island/14-nova-grama.png) · [Mapa de ruído](evidence/mapa-ruido.png) · [Sequência de vento](evidence/island/vento.gif) · [Travessia](evidence/island/travessia.gif)
- [06h](evidence/island/01-06h.png) · [12h](evidence/island/02-12h.png) · [17h](evidence/island/03-17h.png) · [22h](evidence/island/04-22h.png) · [Luminária ligada](evidence/island/05-luminaria-ligada.png) · [Desligada](evidence/island/06-luminaria-desligada.png)
- [Enquadramento próximo](evidence/island/11-encosta-zoom-proximo.png) · [Aberto](evidence/island/12-encosta-zoom-aberto.png) · [Caminho sob rajada](evidence/island/22-caminho-rajada.png) · [Praia](evidence/island/08-praia.png) · [Casa](evidence/island/09-casas.png) · [Posições e contagens](evidence/island/observacoes.json)

A revisão de Game Designer identificou tufos no piso arenoso. A projeção dos pixels de volta ao layout mostrou raízes sobre a camada TerraBatida. A correção consultou a pintura do Terrain e os acessos reais. A revisão independente confirmou o piso livre após a correção. Os assets existentes permanecem recuperáveis pelo histórico. Da cobertura autoral, 24 dos 69 tufos antes ativos entraram nos lotes. O filtro rejeitou 1.242 posições do layout procedimental de 52.443 raízes.

O menu do World Builder ainda pode gerar a distribuição histórica. Rode o integrador após uma alteração de terreno, vias ou distribuição para atualizar máscara e suplemento. No benchmark, Legacy mede o renderer procedimental antigo, e a referência de 49.885 tufos é a primeira versão do sistema Lit, anterior ao pedido de aumento.

## Verificação

| Critério | Evidência |
| --- | --- |
| Luz, sombra, contraluz e vento | Piloto em Metal com grama instanciada. Dois quadros de calmaria são idênticos. Vento forte e alteração de direção mudam as folhas. Luminária e sombras foram comparadas com as fontes desligadas. O relógio real dirigiu os quatro horários na ilha. [Piloto](evidence/pilot/observacoes.json) |
| Perfis, autoria e limites espaciais | Reconstrução determinística, exclusões atualizáveis, retenção dos três perfis, raízes únicas, assentamento e limites expandidos pela deformação. [Testes](evidence/placement.json) |
| Câmera e regressão | A saída de 480 × 270 usa a mesma câmera de produção. A travessia usa o controlador real. Os limites de FOV são condições de ensaio. [Percurso](evidence/island/observacoes.json) |
| Ciclo do vento | O teste limpa os valores globais ao desligar o controlador, republica na reativação e mede fase constante durante oito quadros de pausa. [Pausa](evidence/wind-pause.json) |

A build macOS Development compilou sem erros. Os 345 avisos de shader verificados são dos kernels Sentis já presentes no projeto. [Build](evidence/build-macos.json) O ambiente de referência é macOS/Metal no Apple M4 Pro. A integração de cultivo ou de construção jogável depende de um fluxo que ainda não existe. O volume de exclusão é o ponto de integração testado. Os testes existentes do World Builder passaram em [Edit Mode, 16 de 16](evidence/worldbuilder-editmode.xml) e [Play Mode, 1 de 1](evidence/worldbuilder-playmode.xml).

## Benchmark no Player

Unity 6000.3.25f1, macOS Development ARM64, Apple M4 Pro, Metal, preset gráfico Ultra, janela de 960 × 540 e RenderTexture de 480 × 270. VSync desligado, sem limite de FPS. Cada condição tem cinco segundos de aquecimento e 300 quadros. As submissões incluem instâncias presentes nas transições de LOD. O arquivo completo contém FPS, tempos de CPU principal, thread de renderização, GPU, chamadas de desenho, lotes, triângulos, memória e GC por condição. [Medição final](evidence/benchmark-player-metal.json) · [Execução anterior ao ruído](evidence/benchmark-player-densidade-sem-ruido.json)

| Cenário | Quadro p50 (ms) | Quadro p95 (ms) | GPU p50 (ms) | Instâncias de grama | Lotes de grama |
| --- | ---: | ---: | ---: | ---: | ---: |
| campo-desligada | 0,80 | 1,51 | 0,83 | 0 | 0 |
| campo-anterior | 0,80 | 1,69 | 1,00 | 14.979 | 31 |
| campo-baixa | 0,90 | 1,79 | 0,99 | 9.372 | 24 |
| campo-media | 1,20 | 1,70 | 1,13 | 21.348 | 49 |
| campo-alta | 1,69 | 1,82 | 1,44 | 42.225 | 88 |
| campo-alta-sombra | 2,10 | 2,21 | 1,93 | 42.225 | 88 |
| campo-zoom-proximo | 1,80 | 2,10 | 1,54 | 51.178 | 107 |
| campo-zoom-aberto | 1,50 | 2,40 | 1,48 | 30.426 | 64 |
| vila-alta | 1,50 | 2,50 | 1,56 | 22.159 | 50 |
| costa-noite-alta | 1,79 | 2,59 | 1,36 | 27.341 | 125 |
| campo-desligada-volta | 0,80 | 1,51 | 0,82 | 0 | 0 |
| campo-alta-volta | 1,70 | 2,20 | 1,44 | 42.225 | 88 |

A qualidade Alta acrescentou 0,62 ms de GPU no campo na rodada inicial e na repetição. Mediu 0 B por quadro na submissão da grama em todos os cenários estáveis. Sombras próprias custaram mais 0,48 ms de GPU no campo e ficam desligadas no padrão. A GPU não forneceu amostras no controle de zoom aberto com a grama desligada. Os tempos de quadro desse controle permanecem no JSON.

Na qualidade Alta, as matrizes preparadas para os níveis de detalhe somaram 11.837.376 bytes. Esse cache também existe quando o modo de comparação desliga a submissão da grama. A troca de exclusão restaurou as matrizes anteriores e reconstruiu apenas as regiões atingidas. [Teste de região](evidence/placement.json)

## Status dos critérios

| Critério | Status |
| --- | --- |
| SC-001 · Iluminação e vento | PASS nos cenários exercitados de dia, noite, luminária, calmaria, brisa, vento forte e mudança de direção. |
| SC-002 · Cobertura, caminhos e enquadramento | PASS no trajeto de ida e volta, nas tomadas de campo, casas, praia e sob rajada. Fluxo de cultivo BLOCKED por ausência da mecânica no projeto. |
| SC-003 · Colocação e exclusões | PASS nas 16 checagens de colocação e no ensaio adicional de pausa. |
| SC-004 · Desempenho | PASS nas metas medidas no Mac M4 Pro. GPU do controle com zoom aberto NOT RUN por indisponibilidade de valor no coletor. Outras plataformas NOT RUN. |
| SC-005 · Regressão e entrega | PASS no Play Mode jogável, nos testes do World Builder e na compilação de shaders da entrega. |

## Arquivos

- `Freyja/Assets/_Project/Art/Grass029/` contém material, shader, include de vento, máscara, suplemento e layout de ensaio. A decisão sobre Endless Meadow e a licença MIT estão na [auditoria](auditoria.md).
- `Freyja/Assets/_Project/Scripts/Environment/` contém renderer, exclusão, controlador de vento e bancada de desenvolvimento.
- `Ilha_Principal.unity` vincula esses dados. `Grama_Piloto_078.unity` reproduz o ensaio isolado.
- [Guia de uso](quickstart.md) e [tarefas](tasks.md) ligam os scripts e os resultados.
