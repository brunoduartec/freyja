# Implantação pelos limites do terreno — revisão 019

13 lotes medidos na captura ortográfica `terreno-planta.png` (110 m / 2048 px). Solo exposto segmentado por cor dentro de contornos selecionados, seguido por ajuste de retângulo orientado aos percentis 1–99% da mancha. Os limites da textura são arredondados; não constituem uma planta retangular exata. `lotes-medidos.png` mostra os contornos cianos e retângulos vermelhos.

As frentes foram orientadas para os acessos próximos, preservando a projeção medida. Nos lotes sem marcação, a posição é uma estimativa pela referência e pelo espaço livre no terreno. Alturas incluem cobertura e são estimadas pela tipologia da imagem. O bloco se prolonga até a menor altura amostrada para não flutuar; a coluna H box inclui esse embasamento.

| Construção | Lote | Base XYZ (m) | L × P (m) | H visual / H box (m) | Y | Desnível (m) |
|---|---|---|---|---|---|---|
| Casa_08 | 01 | (-23.80, 7.15, 23.17) | 7.60 × 4.46 | 3.80 / 4.07 | 201° | 1.11 |
| Casa_05 | 02 | (-26.34, 8.07, 15.58) | 7.98 × 3.69 | 6.00 / 6.25 | 10° | 0.88 |
| Sala_Comunitaria | 03 | (-11.78, 6.77, 14.52) | 4.69 × 7.45 | 4.50 / 4.94 | 108° | 1.03 |
| Casa_07 | 04 | (-18.84, 9.20, 3.03) | 9.21 × 4.64 | 4.00 / 4.28 | 241° | 1.19 |
| Loja | 05 | (-9.03, 7.07, 4.55) | 7.31 × 4.55 | 5.60 / 5.90 | 66° | 1.45 |
| Casa_01 | 06 | (13.64, 5.35, 4.57) | 9.98 × 3.99 | 4.00 / 4.32 | 39° | 0.66 |
| Galpao_Cais | 07 | (22.38, 5.17, -0.24) | 8.56 × 4.51 | 4.30 / 4.62 | 32° | 0.78 |
| Casa_04 | 08 | (-2.62, 7.47, -9.34) | 9.50 × 4.81 | 6.00 / 6.77 | 12° | 1.81 |
| Venda | 09 | (8.60, 7.07, -9.06) | 4.40 × 7.31 | 5.80 / 6.10 | 282° | 0.92 |
| Casa_02 | 10 | (17.71, 6.71, -10.52) | 5.59 × 9.99 | 4.00 / 4.59 | 294° | 1.30 |
| Casa_06 | 11 | (1.13, 8.46, -17.10) | 2.99 × 6.50 | 4.00 / 4.18 | 138° | 0.47 |
| Igreja | 12 | (-11.25, 9.33, -16.01) | 4.47 × 8.53 | 5.20 / 5.59 | 122° | 1.00 |
| Casa_10 | 13 | (-22.42, 9.36, -10.29) | 7.32 × 3.25 | 4.00 / 4.46 | 25° | 1.20 |
| Casa_03 | sem marca | (19.20, 8.10, -19.62) | 5.00 × 5.60 | 6.00 / 6.91 | 35° | 1.64 |
| Casa_09 | sem marca | (-10.85, 6.05, 27.10) | 5.80 × 6.00 | 6.00 / 6.42 | 35° | 0.71 |
| Casa_11 | sem marca | (-34.74, 10.51, 4.47) | 4.50 × 5.20 | 4.00 / 4.35 | 35° | 0.90 |
| Casa_12_Afastada | sem marca | (1.99, 9.89, -36.16) | 3.60 × 4.20 | 3.20 / 4.11 | 35° | 1.79 |

Igreja implantada no lote 12, com nave e torre separadas. Casa_10 e Casa_11 são complementares à contagem visível da referência. Nenhum lote marcado fica vazio. As construções sem marcação são Casa_03, Casa_09, Casa_11 e Casa_12_Afastada; não devem ser tratadas como encaixes determinados pelo GLB.

Interpenetrações entre volumes: nenhuma.


## Validação desta revisão

Os 13 lotes marcados estão ocupados. As bases exportadas do Unity foram sobrepostas à captura original sem construções em `planta-validada.png`; os 13 retângulos correspondem às medições aplicadas. O encaixe é aproximado à borda visual suavizada de cada mancha; a precisão numérica da aplicação não transforma a imagem conceitual em levantamento métrico.

Foram examinadas cinco vistas: geral, planta, centro, igreja e cais. Nenhum par de volumes de construções se interpenetra. Percurso cais–centro concluído em Play Mode em 14,52 s nesta revisão, às 00:56 UTC de 2026-11-15. Console atual sem erros e sem falha de compilação. Cena principal salva. Acessos individuais ainda não testados.
