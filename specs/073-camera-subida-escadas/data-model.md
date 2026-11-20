# Estado da câmera

- **Órbita solicitada** guarda yaw, pitch e distância definidos pelo jogador e pela exploração.
- **Vista aplicada** guarda posição, rotação e distância depois da resolução de obstáculos.
- **Alternativa segura** centraliza ou eleva a vista enquanto a órbita solicitada estiver obstruída. A posição precisa caber no espaço e manter visão do tronco.
- **Retorno** interpola para a órbita solicitada assim que ela voltar a ser utilizável. Obstáculos novos exigem resposta imediata.

O estado é local ao rig e temporário.
