# Alinhamento dos apoios na entrada da escada

Na sessão do relato, o corpo estava alinhado ao eixo da L3, mas o centro dos pés estava deslocado cerca de 9 cm em relação ao corpo. A aproximação em diagonal podia mudar a faixa após o cálculo inicial dos apoios.

No Unity 6000.3.25f1, testei a entrada em diagonal pelos dois lados da L3 e da B3 em Play Mode. A faixa do corpo mudou de 10,9 a 12,1 cm durante as passadas observadas. A referência de apoio ficou a até 2,1 cm da faixa atual após a entrada na passada. O comando lateral manteve o deslocamento horizontal e a mudança de ângulo em zero nos quatro testes. O Console registrou zero erros.

[Captura da L3 após a correção](l3-apoios-alinhados.png)

```text
FREYJA071POSE f=1 entry=-1 activeFrames=778 driftFromEntry=0.109 maxAnchorError=0.020 footOffset=0.007 lateralDrift=0.0000 yawChange=0.00 facingError=0.03
FREYJA071POSE f=1 entry=1 activeFrames=827 driftFromEntry=0.121 maxAnchorError=0.021 footOffset=-0.019 lateralDrift=0.0000 yawChange=0.00 facingError=0.03
FREYJA071POSE f=0 entry=-1 activeFrames=564 driftFromEntry=0.112 maxAnchorError=0.021 footOffset=0.010 lateralDrift=0.0000 yawChange=0.00 facingError=0.00
FREYJA071POSE f=0 entry=1 activeFrames=509 driftFromEntry=0.112 maxAnchorError=0.021 footOffset=-0.010 lateralDrift=0.0000 yawChange=0.00 facingError=0.00
FREYJA071POSE complete
```
