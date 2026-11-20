# Peças da referência na ilha de Freyja

A prancha original está em `TripoAi/cenario/referencias.png`. Os GLBs vieram da mesma pasta, com o número da peça no nome. A cena ativa é `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. O grupo `Island023` contém as construções separadas em lotes e o cais da prancha. As instâncias podem ser selecionadas e trocadas pelo Unity.

| Peça | Implantação |
|---|---|
| 1 Sobrado com varanda | B4, subida esquerda |
| 2 Casa térrea | A1, atrás do sobrado |
| 3 Casa do largo esquerdo | B2 |
| 4 Igreja | Patamar alto central |
| 5 Casa do largo direito | A3 |
| 6 Casa do terraço superior direito | L3 |
| 7 Casa da escada e hortas | B3 |
| 8 Casa do terraço baixo esquerdo | B1 |
| 9 Casa do terraço intermediário direito | A2 |
| 10 Casa do terraço baixo direito | L1 |
| 11 Cais, píer e galpão | Enseada, canto esquerdo |

A casa isolada do velho, as outras casas sem peça própria, venda, oficina, salão e armazém em terra usam os modelos provisórios já implantados. A planta mantém 12 residências e os cinco lotes de serviço. O galpão no píer faz parte da peça portuária.

O arquivo `PecasIlha.blend` é a fonte editável desta conversão. O arquivo `pieces.json` registra os GLBs, dimensões de origem e a contagem de faces de cada export. As texturas de terreno permanecem em 32/64 px. Cada modelo Tripo reúne as cores de todas as faces numa única textura de 256 × 256 px, com filtro Point. A conversão reduziu a malha das construções para cerca de 26 mil triângulos por peça e manteve a geometria original do píer. O visual foi verificado no editor e a malha ainda pode receber uma redução adicional para produção.

Os marcos da ilha vêm da composição anterior, `specs/017-referencia-terreno-tripo/vila-casa-isolada.png`. A folha nova identifica as peças e suas relações. O terreno tem pintura própria para que as transições fiquem no chão, sem recortes causados por malhas sobrepostas.

As capturas, a relação das peças por lote, o teste das portas e o backup da cena anterior estão em `Artifacts/Island023/Tripo/`. A validação de porta percorre cada trecho desde a rua com o controlador do jogo. A casa 6 tem duas entradas na malha. O alvo testado foi a porta térrea. Interiores ficam fora deste trecho de composição.

## Venda e oficina

As peças 12 e 13 substituem os blocos de toldo da praça. Os GLBs originais ficam junto às ilustrações em `TripoAi/cenario`. Cada FBX no Unity tem 26 mil triângulos e uma textura de 256 px com filtro Point. As fontes editáveis são `Piece12.blend` e `Piece13.blend`. A validação dos dois acessos está em `Artifacts/Island023/Tripo/plaza-result.json`.

## Peças 14 a 18

As três casas restantes, o salão e o armazém em terra agora têm prefabs próprios. Os cinco acessos foram percorridos em Play Mode. A implantação das 18 peças, incluindo o píer, está registrada em `Artifacts/Island023/Tripo/implantacao-1-18.json`.
