# Praça central no Unity

O calçamento de 64 px está em `Freyja/Assets/_Project/Art/Praca027/`. O grupo `Island023` da cena `Ilha_Tripo_Caminhada_011` usa o `TerrenoDaPraca.asset` para o chão visual e para colisão. O TerrainData anterior `TerrenoComAcessos.asset` está preservado. O backup da cena anterior é `Cena-antes-plaza.unity`.

As duas vistas `geral` e `travessia` usam os mesmos enquadramentos no antes e no depois. `terreno.json` registra a comparação da inclinação no centro e as alturas nas três portas. `praca-result.json` registra a chegada às portas da venda, oficina e salão, a caminhada do início da via do cais à praça e a subida da praça até a igreja.

## Acabamento do cais

A plataforma mantém a geometria e a colisão. O novo acabamento usa calçamento na face superior e alvenaria nas laterais, com escala consistente. Capturas `cais-antes.png` e `cais-depois.png`. O percurso do convés ao caminho em terra foi verificado em Play Mode em `cais-result.json`.
