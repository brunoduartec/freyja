# Validação no Unity

1. Abrir `Ilha_Principal` no Unity 6000.3.25f1. Confirmar compilação e Console pelo Unity Pipeline.
2. Entrar em Play Mode. Usar o procedimento de `Tools/Island023/Record.cs` para dirigir o protagonista real e capturar `Mundo_480x270`. Manter a sessão ativa com captura de entrada liberada para acompanhar recuperações.
3. Na L3, partir do terreno, fazer pausa no meio, chegar à varanda e voltar. Repetir em corrida, entrar em diagonal e pelas bordas, inverter e tentar entrar pela lateral. Registrar posição, tempo, apoio e recuperações. Comparar imagens de subida, descida, parada e transições com a baseline.
4. Verificar terreno plano e inclinado, soleira, salto e queda. Conferir o enquadramento e a leitura da passada com os agentes. Se o piloto passar, repetir na B3 e verificar o patamar e a rotação.
5. Para NPC, usar os destinos da rede de waypoints e o walker existente. Registrar distância final, variação de altura e bloqueios. Quando viável, comparar CPU sob a mesma configuração.
