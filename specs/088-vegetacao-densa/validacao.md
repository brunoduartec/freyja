# Validação — vegetação regional da ilha

## Ativos e composição

- Reutilizados `Nature032/CoastalFlowers.fbx`, `CoastalShrub.fbx` e os materiais compartilhados de pétalas creme, amarela e coral suave e folhagem. A distribuição toma como base as posições de grama já gravadas em `Art/Grass032/IslandGrassLayout.asset`.
- `KitPS1_022/Tree_Cypress.fbx` e `Tree_Pine.fbx` foram acrescentados em escala reduzida numa clareira gramada da crista noroeste, após validar eixo Z-up e silhuetas pela câmera. A ilha também preserva os 12 `Nature_Pine` já existentes.
- Nenhum modelo, material, shader, collider, sistema de vegetação ou dependência foi criado. Não houve uso de Tripo AI nem aquisição de assets.
- Contagem final na cena: **632 flores**, **11 arbustos** e **2 árvores novas**; os 12 pinheiros anteriores permanecem. Campos: quintais coral 233, crista norte 153, encosta leste 122, jardim da igreja 92, costa 28 e quatro flores isoladas em pontos baixos de encosta.
- As 645 instâncias adicionadas/revisadas usam `BatchingStatic` e não têm colliders. As flores continuam estáticas no vento, pois os materiais Standard existentes não suportam o shader de vento da grama.

## Revisão visual em Play Mode

Capturas da `Cais Gameplay Camera` em 1280×720, com hora fixada em 12h/22h, estão em `evidence/revalidation-dense/`: pares de coral, norte, leste, igreja e costa (`coast-final-day/night.png` registra a correção final). A planta superior atualizada, com posições reais da cena, está em `evidence/vegetation-map.png` e no vetor fonte `vegetation-map.svg`.

- A solicitação de mais flores acrescentou 499 instâncias à primeira distribuição. A inspeção mostrou a faixa costeira densa demais para o ressalto estreito; removi 34 flores desse acréscimo e recuei 22 outras para pontos gramados internos. A cena final mantém 28 flores na costa; água, margem e caminho ficam legíveis nas capturas finais.
- Coral, crista norte, encosta leste e jardim da igreja agora formam campos mais cheios. Caminhos permanecem aparentes; a porta da igreja, NPCs, entradas e marcos continuam livres nas capturas.
- As árvores novas têm escala compatível e não ocultam rota ou marcos; uma copa sobreposta foi afastada antes desta revisão.
- À noite as pétalas ficam pouco contrastadas, sobretudo longe das luzes já existentes. A iluminação e o ciclo atual permanecem intactos; nenhuma luminária foi adicionada.
- Console no Editor 7800: sem erros novos de compilação ou runtime durante as capturas; houve um aviso recorrente do Unity AI Toolkit sobre indisponibilidade da Account API. A cópia para a branch `088-vegetacao-densa` foi salva por Unity batch mode (`VEGETATION_IMPORT_DONE`, 645 objetos, 9 grupos); não houve erro `CS` nem novo arquivo de modelo/material.

## Performance observada

`diagnostics get_performance` na rodada densa, antes de retirar 34 flores costeiras, marcou **4.094 draw calls, 533 batches, 4,81 M triângulos, CPU 5,78 ms e GPU 3,63 ms**. Como a medição foi no Editor e variava com câmera/estado, ela não é comparável às leituras anteriores (1.852–2.360 draw calls). A medição da cena final, já com 34 instâncias a menos, não foi repetida; benchmark de build com câmera/configuração fixa permanece pendente.

Essas leituras não permitem atribuir custo às plantas nem provar desempenho final. As instâncias usam batching estático e não têm colliders; não foi criado sistema próprio de instancing. Um benchmark de build com câmera/configuração fixas continua pendente.

## Pendências e limitações

- A cena `Ilha_Principal` não contém objeto ou setor identificável como ruína (busca na cena e referência ao GDD). A vegetação das ruínas não foi colocada arbitrariamente; T012 depende de o local existir/ser identificado.
- Não foram acrescentadas outras árvores além do par Cypress/Pine validado na crista noroeste; o restante das clareiras mantém os pinheiros que já existiam.
- O `CaisPlayerController`/`CharacterController` percorreu 6 m em cada um de dois trechos gramados e permaneceu grounded. O movimento foi dirigido via API do próprio controller, sem input manual de teclado; não há colliders decorativos.
- A branch `088-vegetacao-densa` partiu do `main` local `73803dd`; os transforms, rotações, escalas e materiais da cena visualmente validada foram reaplicados à `Ilha_Principal` dessa branch. As capturas foram feitas na cópia de validação 086; a cópia 088 não recebeu uma segunda captura, pois a mudança de integração apenas reproduz os mesmos objetos e transforms. O fast-forward para `main` foi concluído no commit `8094021`; alterações locais não rastreadas de NPC permaneceram fora do merge.
