# Validação da passada descendente

Unity 6000.3.25f1, cena Ilha_Tripo_Caminhada_011. Compilação concluída e Console sem erros.

- As três descidas da L3 (f=1) e as três da B3 (f=0) chegaram ao chão. O ritmo máximo foi de 0,84 m/s na L3 e 1,09 m/s na B3, em função da distância entre degraus.
- Nas fases com peso completo de IK, o maior deslocamento medido do pé de apoio durante um passo foi de 1,5 cm.
- A pausa de dois segundos com direção lateral manteve posição e fase. A subida completou os dois lances e a inversão em movimento funcionou na L3.
- [Antes](l3-antes.png) e [depois](l3-depois.png) na L3. As capturas estáticas foram feitas em ângulos diferentes. O registro numérico abaixo foi coletado durante o movimento.

```text
FREYJA072 baseline speed=0.00 guided=False
FREYJA072 pause f=1 held=True phaseDelta=0.0000 drift=0.0000
FREYJA072 f=1 trial=0 exited=True guidedFrames=2740 steps=14 speed=0.84 supportDrift=0.007 pos=(34.58, 13.97, 26.14)
FREYJA072 f=1 trial=1 exited=True guidedFrames=2609 steps=14 speed=0.84 supportDrift=0.015 pos=(34.58, 13.97, 26.14)
FREYJA072 f=1 trial=2 exited=True guidedFrames=2770 steps=14 speed=0.84 supportDrift=0.005 pos=(34.58, 13.97, 26.14)
FREYJA072 ascent f=1 guided=True y=16.86 top=16.63
FREYJA072 pause f=0 held=True phaseDelta=0.0000 drift=0.0000
FREYJA072 f=0 trial=0 exited=True guidedFrames=737 steps=5 speed=1.09 supportDrift=0.005 pos=(-34.94, 9.63, 1.40)
FREYJA072 f=0 trial=1 exited=True guidedFrames=816 steps=5 speed=1.09 supportDrift=0.008 pos=(-34.94, 9.63, 1.40)
FREYJA072 f=0 trial=2 exited=True guidedFrames=819 steps=5 speed=1.09 supportDrift=0.010 pos=(-34.94, 9.63, 1.40)
FREYJA072 ascent f=0 guided=True y=10.55 top=10.51
FREYJA072 complete
FREYJA072 reverse down=True up=True facing=0.03
```
