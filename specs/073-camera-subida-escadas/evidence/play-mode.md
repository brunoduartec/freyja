# Validação da câmera

Unity 6000.3.25f1, cena Ilha_Tripo_Caminhada_011. Compilação concluída e Console sem erros.

- Antes, na L3, a sonda na altura do tronco atingia os colliders Guarda 057 01 a 14. A distância ao pivô caiu a 0,00 m na posição (35,27, 14,48, 25,95).
- Depois, os três percursos na L3, a subida com órbita manual e a subida na B3 chegaram ao patamar. O tronco permaneceu dentro do quadro nas amostras de todos os percursos. A câmera ficou a pelo menos 5,02 m do pivô central e os testes de linha de visão e de sobreposição com obstáculos ficaram livres.
- Na subida com órbita manual, o maior deslocamento entre amostras foi 6,1 cm. Na lateral esquerda da L3 houve um ajuste de 25,5 cm em uma amostra, com o personagem ainda enquadrado.
- Seis ângulos de reposicionamento junto à fachada mantiveram a câmera fora da geometria e o tronco visível.
- Capturas pelo Game View na mesma posição do personagem, com horários de luz distintos. [Antes](l3-antes.png) · [Depois](l3-depois.png)

```text
FREYJA073FINAL trial=0 flight=1 reached=True min=5.04 max=5.23 offscreenSeconds=0.000 blocked=0/2484 overlap=0/2484 largestFrameMove=0.255 yaw=99.4
FREYJA073FINAL trial=1 flight=1 reached=True min=5.03 max=5.23 offscreenSeconds=0.000 blocked=0/2630 overlap=0/2630 largestFrameMove=0.015 yaw=98.5
FREYJA073FINAL trial=2 flight=1 reached=True min=5.02 max=5.23 offscreenSeconds=0.000 blocked=0/2475 overlap=0/2475 largestFrameMove=0.021 yaw=97.6
FREYJA073FINAL trial=3 flight=1 reached=True min=5.03 max=5.13 offscreenSeconds=0.000 blocked=0/2665 overlap=0/2665 largestFrameMove=0.061 yaw=120.0
FREYJA073FINAL trial=4 flight=0 reached=True min=5.04 max=5.05 offscreenSeconds=0.000 blocked=0/615 overlap=0/615 largestFrameMove=0.006 yaw=20.0
FREYJA073FINAL complete
```
