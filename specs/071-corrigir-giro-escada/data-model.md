# Estado de locomoção

| Situação | Condição | Pose esperada |
| --- | --- | --- |
| Subida | Comando válido de avanço no lance | Passada calculada pelo avanço no lance |
| Pausa | Sem comando, no mesmo lance, com apoio previamente estabelecido | Mantém a passada plantada |
| Mudança de direção | Comando lateral ou de descida | Libera a passada suavemente e volta ao apoio local |
| Retomada | Novo comando de subida após mudança de direção | Refaz os apoios a partir da posição atual |

A intenção planar vem do controlador. O deslocamento real vem da resolução de colisão. Os apoios são temporários por sequência de subida. A mudança é interna à locomoção e dispensa contrato externo ou persistência.
