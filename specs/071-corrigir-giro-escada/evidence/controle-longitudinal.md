# Validação do controle longitudinal

Unity 6000.3.25f1, cena Ilha_Tripo_Caminhada_011, Play Mode via MCP. O Console terminou com zero erros.

- L3 (f=1), três tentativas por lado. B3 (f=0), uma tentativa por lado. Dois segundos de comando lateral mantiveram a posição horizontal, o ângulo do corpo e a fase de apoio nas oito tentativas.
- Diagonais de subida e descida avançaram no eixo dos dois lances, com desvio lateral de 0,000 m na precisão registrada.
- A travessia de degrau a degrau terminou em ambos os sentidos nos dois lances. O filtro devolveu integralmente a velocidade lateral nos quatro extremos, incluindo o patamar da B3.
- Sequência na L3: [subida](l3-1-subida.png), [lateral mantida](l3-2-lateral.png) e [retomada](l3-3-retomada.png).

```text
FREYJA071OK f=1 trial=0 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=1 trial=0 diagonalUp=0.301 across=0.000
FREYJA071OK f=1 trial=1 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=1 trial=1 diagonalUp=0.301 across=0.000
FREYJA071OK f=1 trial=2 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=1 trial=2 diagonalUp=0.300 across=0.000
FREYJA071OK f=1 trial=3 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=1 trial=3 diagonalUp=0.301 across=0.000
FREYJA071OK f=1 trial=4 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=1 trial=4 diagonalUp=0.300 across=0.000
FREYJA071OK f=1 trial=5 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=1 trial=5 diagonalUp=0.301 across=0.000
FREYJA071OK f=1 diagonalDown=-0.362 across=0.000
FREYJA071OK f=1 bottomLateral=1.00 bottomY=13.94 topLateral=1.00 topY=16.86 treadY=16.63
FREYJA071OK f=0 trial=0 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=0 trial=0 diagonalUp=0.300 across=0.000
FREYJA071OK f=0 trial=1 lateral=0.0000 yaw=0.00 planted=True intent=False
FREYJA071OK f=0 trial=1 diagonalUp=0.301 across=0.000
FREYJA071OK f=0 diagonalDown=-0.361 across=0.000
FREYJA071OK f=0 bottomLateral=1.00 bottomY=8.54 topLateral=1.00 topY=10.55 treadY=10.51
FREYJA071OK complete
FREYJA071TRAVERSE f=1 sign=1 passed=True lateral=1.00 end=(38.87, 16.86, 24.99)
FREYJA071TRAVERSE f=1 sign=-1 passed=True lateral=1.00 end=(34.74, 14.10, 26.10)
FREYJA071TRAVERSE f=0 sign=1 passed=True lateral=1.00 end=(-34.34, 10.55, 3.06)
FREYJA071TRAVERSE f=0 sign=-1 passed=True lateral=1.00 end=(-34.89, 9.75, 1.55)
FREYJA071TRAVERSE complete
```
