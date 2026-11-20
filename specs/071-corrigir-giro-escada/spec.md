# Controle longitudinal nas escadas

**Branch** `071-corrigir-giro-escada`
**Data** 2026-11-20
**Status** Implementada e validada em Play Mode

A captura após a primeira correção ainda mostra o corpo virado de lado. O comportamento desejado nesta revisão é restringir a caminhada ao eixo do lance enquanto o personagem estiver apoiado nos degraus. O controle livre volta nos patamares.

## Cenários e testes

1. **Lateral no meio do lance.** Durante a subida, apertar só para um dos lados por dois segundos. O personagem fica no mesmo lugar, mantém a orientação da subida e conserva o apoio dos pés. Repetir para o outro lado e retomar a subida.
2. **Diagonal e inversão.** Comandar em diagonal com uma componente clara de subida ou descida. O personagem avança ao longo da escada. Comandar a descida para conferir que ele pode girar para baixo e descer.
3. **Entrada e saída.** Passar de lado perto da escada e caminhar nos patamares inferior e superior. Nesses trechos, o personagem mantém o movimento e a rotação livres.

## Requisitos

- **RF-01** Enquanto apoiado dentro de um lance jogável, o personagem deve ignorar o comando puramente lateral ao decidir deslocamento e rotação.
- **RF-02** Comando diagonal com avanço suficiente deve mover e orientar o personagem ao longo do lance. Subida, pausa e descida devem continuar disponíveis.
- **RF-03** A restrição termina ao sair do lance. O salto existente permanece disponível onde já era permitido.
- **RF-04** Aplicar o comportamento aos lances jogáveis já configurados. O aviso de câmera é uma questão separada.

## Critérios de sucesso

- **CS-01** Em três tentativas por lado na L3, o comando lateral por dois segundos mantém a posição e a orientação longitudinal, com os pés apoiados. Repetir uma vez por lado na B3.
- **CS-02** Em ambos os lances, concluir uma subida e uma descida, testar uma diagonal em cada sentido e confirmar que o movimento lateral volta no patamar.
- **CS-03** Registrar uma sequência em Play Mode com subida, comando lateral e retomada na L3.

## Premissa

A solução prioriza a travessia previsível. O giro lateral com transferência de peso e reposicionamento dos pés pode ser avaliado se a restrição se mostrar inadequada no jogo.
