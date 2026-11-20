# Diagnóstico

- `LimitAscentSpeed` encerra a classificação de subida quando a direção sai do cone de avanço. O controlador passa a girar o corpo pela direção solicitada. Após colisão lateral, a velocidade real pode continuar zero.
- `OnAnimatorIK` usa a velocidade real perto de zero para manter a passada plantada. `TryGetAscentAnimationTime` também consulta essa velocidade para voltar à fase da escada. Os dois consumidores precisam de uma regra de pausa coerente.
- Os apoios são calculados ao entrar no lance e guardam a posição lateral daquele momento. A retomada rápida no mesmo lance precisa invalidar esse cache quando a passada tiver sido interrompida.
- A alternativa de restringir giro lateral mudaria a resposta ao controle. Preservar o giro e corrigir a transição atende à spec aprovada.
- A captura confirma a mensagem de display. A relação com o giro continua em aberto até a reprodução.
