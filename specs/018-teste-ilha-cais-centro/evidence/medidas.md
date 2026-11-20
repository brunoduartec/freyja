# Avaliação inicial — ilha Tripo 018

## Escala experimental e fonte

- Modelo fornecido: `TripoAi/cenario/tripo_convert_5646c58d-74cc-4b9a-b04d-86cb22d83fa8.glb`. Importado no Blender como uma malha de 1.927.757 faces; textura incorporada de 512 × 512 pixels.
- Escala uniforme experimental no Unity: 163,2482, obtida ao definir a largura total como 160 m. Isso é uma escolha para testar, não uma medida física fornecida pelo modelo nem uma escala aprovada.
- Limites aproximados da malha no Unity: 160 × 147,84 × 21,60 m (largura × profundidade × altura). Personagem e cápsula: referência de 1,80 m.

## Medições visuais aproximadas

Feitas sobre `modelo-planta.png`, projeção ortográfica do Blender com enquadramento de 1,1 unidade de origem em 1.200 pixels; o fator de escala equivale a cerca de 0,150 m por pixel. Os limites das manchas e do píer são irregulares e sua interpretação é aproximada, não medição de projeto:

- Píer: cerca de 20–23 m de comprimento e 5–7 m de largura aparente, incluindo aproximações de borda.
- Um lote próximo da via ocidental: cerca de 13 × 10 m em sua área clara aparente; outros variam. Isso não prova área plana, acesso ou capacidade real de construção.
- As referências antigas de habitações eram 5 × 6 a 7 × 8 m. Nessa escala experimental, o lote amostrado pode parecer suficientemente amplo em planta; não se pode concluir que todos os lotes sejam pequenos. A geometria precisa ser conferida com blocos e o personagem.
- Um trecho de caminho aparece com aproximadamente 25–40 pixels de largura na projeção, cerca de 3,7–6 m; bordas irregulares e perspectiva da textura tornam a leitura aproximada. Algumas bordas e pedras da malha são altas em relação ao personagem e impediram rotas testadas.

## Teste jogável

Foi instanciado o cenário na cena principal e preservados desativados os objetos da ilha anterior e o galpão anterior. A malha simplificada de colisão (89.546 polígonos) deixou lacunas nos pontos testados; a colisão da malha visual completa foi usada no teste. Foi criada uma rampa provisória de acesso do píer ao cais, mas o personagem ficou preso entre partes do píer mesmo assim. A partir do patamar do cais, duas tentativas ficaram bloqueadas por bordas; uma rota alternativa mais cuidadosa chegou ao largo central, sem teletransporte durante o percurso. A rota foi guiada automaticamente usando o controlador real do personagem.

`cais-ao-centro.mp4` registra a caminhada bem-sucedida do patamar do cais até o centro; não inclui a caminhada desde a ponta do píer. `tentativa-cais-visual.mp4` registra a tentativa bloqueada a partir do píer. São capturas reais da câmera do jogo em Play Mode, gravadas em quadros e codificadas a 10 fps, sem áudio. A captura não manteve taxa constante: o percurso levou 24,26 s no jogo e o arquivo final dura aproximadamente 21,4 s.

O Console não mostrava erros de compilação ou erros atuais na amostragem de estado; o histórico do servidor continha erros de conexão anteriores ao teste. Não houve build.

## Comparação de escalas e ajuste posterior

Após o pedido de reduzir o cenário, foram comparadas visualmente larguras de 120, 90 e 70 m com o mesmo personagem de 1,80 m. A referência `specs/017-referencia-terreno-tripo/vila-casa-isolada.png` também foi revista para considerar casas térreas e de dois pavimentos e seus acessos. Essas imagens de escala são comparações visuais, sem blocos de casas colocados no terreno.

A escala salva para avaliação é de **100 m de largura** (escala uniforme 102,0301), 62,5% da escala anterior de 160 m. Por proporcionalidade, a amostra aproximada de lote de 13 × 10 m passaria a cerca de 8,1 × 6,3 m, mas a área útil plana deve ser medida com blocos. A redução não resolve diferenças de proporção internas entre píer, pedras e lotes.

Na escala de 100 m, a sessão iniciou com o personagem apoiado no patamar do cais e o percurso guiado pelo controlador chegou ao centro em 14,52 segundos de jogo. Evidência: `caminhada-escala-100m.mp4` e `captura-escala-100-status.txt`. O vídeo foi montado a 10 fps a partir de 129 quadros capturados, sem áudio; a duração apresentada não é medição exata do tempo real. O início em Play Mode anterior, usado durante a comparação de escalas, registrou erro de SpawnPoint obstruído; o novo spawn foi ajustado e a sessão inicializou corretamente. O acesso desde a ponta do píer não foi retestado nesta escala. Nenhum modelo de casa ou bloco dimensionado foi colocado nos lotes para este teste.
