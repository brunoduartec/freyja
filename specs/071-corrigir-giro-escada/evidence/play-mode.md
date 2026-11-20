# Validação em Play Mode

Unity 6000.3.25f1, cena Ilha_Tripo_Caminhada_011. O editor recarregou os scripts (191 → 192) sem erro de compilação. O Console registrou zero erros na sessão de teste.

- L3, o lance da captura, identificado pela posição inicial do personagem e pela fachada. Foram três tentativas por lado, com comando lateral mantido por dois segundos. A velocidade real ao final ficou entre 0,012 e 0,033 m/s. A passada permaneceu ativa durante as seis pausas e foi liberada nos seis comandos laterais. A retomada funcionou nas seis tentativas.
- B3, uma tentativa por lado. A pausa manteve a passada e o comando lateral encerrou a fase de subida. Nos dois casos, o personagem já havia saído do alcance do lance quando recebeu o comando de retomada.
- L3, alternância rápida retomou a subida. A descida liberou a fase da subida. No salto iniciado com apoio, a fase da escada ficou inativa. O personagem alcançou o patamar acima do último degrau e encerrou a fase de subida.
- A câmera de gameplay esteve ativa no display 1 nas oito amostras. O aviso da captura inicial não reapareceu nessas observações.
- [Captura após o giro lateral](giro-lateral-l3.png), com o enquadramento orientado para a escada. A captura enviada pelo usuário é a referência visual anterior. O estado anterior à correção não teve reprodução controlada nem captura equivalente.

## Registros do Console

```text
FREYJA071 flight=1 trial=0 up=True pause=True lateral=False blend=0 anchors=False speed=0.012 camera0=True resume=True
FREYJA071 flight=1 trial=1 up=True pause=True lateral=False blend=0 anchors=False speed=0.033 camera0=True resume=True
FREYJA071 flight=1 trial=2 up=True pause=True lateral=False blend=0 anchors=False speed=0.015 camera0=True resume=True
FREYJA071 flight=1 trial=3 up=True pause=True lateral=False blend=0 anchors=False speed=0.030 camera0=True resume=True
FREYJA071 flight=1 trial=4 up=True pause=True lateral=False blend=0 anchors=False speed=0.014 camera0=True resume=True
FREYJA071 flight=1 trial=5 up=True pause=True lateral=False blend=0 anchors=False speed=0.031 camera0=True resume=True
FREYJA071 flight=0 trial=0 up=True pause=True lateral=False blend=0 anchors=False speed=3.000 camera0=True resume=False
FREYJA071 flight=0 trial=1 up=True pause=True lateral=False blend=0 anchors=False speed=3.000 camera0=True resume=False
FREYJA071 complete
FREYJA071 edges rapid=True descent=True jump=False ascentDuringJump=False
FREYJA071 capture ready
FREYJA071 jump groundedBefore=True airborne=True ascent=False
FREYJA071 top final=(39.17, 16.86, 25.02) ascent=False highestTread=16.63
```
