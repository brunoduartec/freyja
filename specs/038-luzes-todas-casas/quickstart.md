# Revisão no Unity

Abra a cena `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` no projeto da branch 038 e entre em Play Mode. Em `Coastal Sun`, desligue `Advance Clock` e ajuste `Hour` para 12, 17,75, 18,5, 0, 5,75 e 6,5. O objeto `Luzes noturnas da vila` reúne as fontes residenciais, a venda e os dois lampiões.

## Cobertura verificada

| Casa | Dia | Noite |
| --- | --- | --- |
| B1 | [12h](evidencias/B1-dia.png) | [0h](evidencias/B1-noite.png) |
| B2 | [12h](evidencias/B2-dia.png) | [0h](evidencias/B2-noite.png) |
| B3 | [12h](evidencias/B3-dia.png) | [0h](evidencias/B3-noite.png) |
| B4 | [12h](evidencias/B4-dia.png) | [0h](evidencias/B4-noite.png) |
| A1 | [12h](evidencias/A1-dia.png) | [0h](evidencias/A1-noite.png) |
| A2 | [12h](evidencias/A2-dia.png) | [0h](evidencias/A2-noite.png) |
| A3 | [12h](evidencias/A3-dia.png) | [0h](evidencias/A3-noite.png) |
| A4 | [12h](evidencias/A4-dia.png) | [0h](evidencias/A4-noite.png) |
| L1 | [12h](evidencias/L1-dia.png) | [0h](evidencias/L1-noite.png) |
| L2 | [12h](evidencias/L2-dia.png) | [0h](evidencias/L2-noite.png) |
| L3 | [12h](evidencias/L3-dia.png) | [0h](evidencias/L3-noite.png) |
| L4 · isolada | [12h](evidencias/L4-dia.png) | [0h](evidencias/L4-noite.png) |

As 24 capturas usam a câmera do jogo a 480 × 270 em Play Mode. Os comparativos reúnem as [casas baixas](evidencias/casas-baixas.png), [casas altas](evidencias/casas-altas.png) e [casas do leste](evidencias/casas-do-leste.png).

## Transições e integração

- Em Play Mode as 12 fontes residenciais estavam apagadas ao meio-dia e às 6h30. Cinco entraram às 17h45. Às 18h30 todas atingiram a intensidade noturna, mantida na passagem da meia-noite e até 5h30.
- Foi observada intensidade intermediária no entardecer e no amanhecer em cada uma das três áreas. A segunda visita às 17h45 reproduziu a mesma intensidade em cada casa. O ajuste de hora pelo Inspector também ativou as 16 fontes pela atualização normal da cena.
- Foram reaproveitadas as luzes de B4, A3 e L2, com ajuste no encaixe dos vidros. Foram adicionadas nove fontes residenciais. A venda, o salão e os dois lampiões continuam sincronizados com o ciclo. Nada na hierarquia original do relevo e construções precisou ser removido.
- Projeto atualizado no Unity 6000.3.25f1 sem erros de compilação. Shader de céu sem erro. O Console registrou exceções da indexação interna do próprio editor.
