# Validação de corrida e salto

Play Mode via MCP na cena `Ilha_Tripo_Caminhada_011.unity`, com entradas enviadas ao Input System, medições do controlador e capturas da Game View.

- **SC-001** — Em três percursos de 0,5 s no píer, a caminhada cobriu 1,50 m e a corrida cobriu 2,70 m. Soltar Shift retomou 3 m/s e soltar WASD levou ao repouso. Os estados Walk, Run e Idle foram observados com entrada de teclado. [Corrida e HUD](corrida.png)
- **SC-002** — Espaço acionou Jump em repouso, caminhada e corrida. O salto subiu cerca de 0,59 m no píer. WASD mudou a direção no ar, um segundo toque no ar não gerou impulso e manter Espaço pressionado no pouso não repetiu o salto. Após soltar e pressionar novamente no chão, o personagem saltou. [Salto e HUD](salto.png)
- **SC-003** — A caminhada no acesso do píer avançou 11,25 m e subiu 0,19 m, terminando com o personagem no chão. Na escada B3, o salto interrompeu o estado de subida. Uma barreira temporária bloqueou corrida e salto a 1,21 m do início. Abaixo do limite de queda, a sessão recuperou o personagem no ponto inicial. Shift, WASD e Espaço não moveram o personagem com o cursor livre ou com o bloqueio modal de diálogo ativo.
- **Console** — Compilação concluída e zero erros no Console ao fim da verificação.

A avaliação do ritmo da corrida e do salto com teclado físico fica para a revisão do usuário.
