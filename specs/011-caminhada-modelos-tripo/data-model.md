# Modelo de dados e estados — Spec 011

| Entidade | Dados e referências | Validação |
| --- | --- | --- |
| Cena de avaliação | Terreno visual, colisões estáticas, ponto inicial, marco, iluminação, câmera, jogador e texto de controles | Referências explícitas. O ponto inicial tem apoio, folga para a cápsula e orientação voltada para o trajeto. |
| Protagonista | Raiz física com controlador, filho visual com rig e Animator, clipes `Idle` e `Walk` | Raiz física em escala unitária e pivô nos pés. Modelo voltado para a frente do controlador. Clipes cíclicos e root motion desligado. |
| Percurso | Início, marco, superfície contínua, mudança de direção, obstrução e saída para teste de recuperação | Colisão acompanha o piso e as encostas. O percurso permite ida e volta. Limites e altura de recuperação abrangem todo o trecho. |
| Câmera e saída | Alvo, máscara de obstáculos, parâmetros de órbita, alvo de renderização e apresentação 16:9 | Máscara exclui jogador, mar e gatilhos. O mundo usa filtro pontual e a interface permanece legível. |
| Sessão | Foco, controle capturado, ponto inicial, velocidade planar efetiva, orientação e estado da câmera | Ao recuperar, zerar velocidade e suavização e restaurar pose e enquadramento iniciais. |

A sessão inicia com o controle liberado e o protagonista em repouso. Com foco, o clique captura o cursor. Esc ou a perda de foco liberam o controle e limpam os comandos pendentes. O deslocamento em solo ativa a caminhada. Ao parar, encostar numa barreira sem avançar ou entrar em queda, a animação volta ao repouso. A recuperação reposiciona o personagem e a câmera, preservando o estado de captura válido.
