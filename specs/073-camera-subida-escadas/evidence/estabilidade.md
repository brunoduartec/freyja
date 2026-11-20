# Estabilidade da câmera na escada

Validação no Unity 6000.3.25f1, na cena Ilha_Tripo_Caminhada_011. O Console registrou zero erros.

O rig mantém a vista central durante a passada e suaviza a altura do pivô. Ao deixar a escada, retoma a vista de ombro pela transição existente, se houver espaço.

- Na L3, o maior deslocamento lateral por quadro registrado antes da mudança foi 28,2 cm. Nas duas verificações após a mudança, foi de 6,2 cm e 2,6 cm na subida e abaixo de 1 cm na descida.
- A primeira repetição da subida teve uma variação vertical isolada de 22 cm no mesmo quadro em que a raiz subiu 16,5 cm. Na repetição seguinte, o pico vertical foi de 2,5 cm.
- A regressão cobriu três trajetórias na L3, uma subida com órbita manual e uma na B3. O tronco permaneceu visível, com linha de visão e volume da câmera livres nas amostras. As cinco subidas chegaram ao patamar.

```text
FREYJA073SWAY sign=1 cameraYJump=0.077 rootYJump=0.000 sideJump=0.282 switches=0 shoulder=0.65..0.65
FREYJA073SWAY sign=-1 cameraYJump=0.005 rootYJump=0.000 sideJump=0.048 switches=1 shoulder=0.65..0.65
FREYJA073STABLE sign=1 cameraYJump=0.220 rootYJump=0.165 sideJump=0.062 switches=1 shoulder=0.00..0.65 centeredFrames=2187 invisibleFrames=0 tooCloseFrames=0
FREYJA073STABLE sign=-1 cameraYJump=0.002 rootYJump=0.004 sideJump=0.008 switches=0 shoulder=0.00..0.65 centeredFrames=2832 invisibleFrames=0 tooCloseFrames=0
FREYJA073TRACE sign=1 cameraYJump=0.025 rootYJump=0.017 sideJump=0.026 switches=1 shoulder=0.00..0.65 centeredFrames=2321 invisibleFrames=0 tooCloseFrames=0 where=(36.43, 15.30, 25.64) prev=(31.52, 17.15, 26.96) next=(31.55, 17.17, 26.95) tY=16.54976 stairs=True heightDelta=0.001
FREYJA073TRACE sign=-1 cameraYJump=0.001 rootYJump=0.003 sideJump=0.007 switches=0 shoulder=0.00..0.65 centeredFrames=2912 invisibleFrames=0 tooCloseFrames=0 where=(36.80, 15.51, 25.54) prev=(32.08, 17.71, 26.78) next=(32.08, 17.71, 26.78) tY=16.96243 stairs=True heightDelta=0.000
FREYJA073REG trial=0 flight=1 reached=True min=5.04 max=5.23 offscreenSeconds=0.000 blocked=0/3235 overlap=0/3235 largestFrameMove=0.026 yaw=99.2
FREYJA073REG trial=1 flight=1 reached=True min=5.03 max=5.23 offscreenSeconds=0.000 blocked=0/2764 overlap=0/2764 largestFrameMove=0.019 yaw=98.5
FREYJA073REG trial=2 flight=1 reached=True min=5.02 max=5.23 offscreenSeconds=0.000 blocked=0/3102 overlap=0/3102 largestFrameMove=0.039 yaw=97.6
FREYJA073REG trial=3 flight=1 reached=True min=5.03 max=5.24 offscreenSeconds=0.000 blocked=0/3131 overlap=0/3131 largestFrameMove=0.013 yaw=119.9
FREYJA073REG trial=4 flight=0 reached=True min=5.04 max=5.10 offscreenSeconds=0.000 blocked=0/684 overlap=0/684 largestFrameMove=0.011 yaw=20.0
FREYJA073REG complete
```
