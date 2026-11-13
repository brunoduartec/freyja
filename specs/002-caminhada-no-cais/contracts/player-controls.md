# Contrato de controle — Caminhar pelo cais

Interface implementada; resultados registrados em [validação](../evidence/validacao-us1.md). Avaliação do usuário pendente.

| Entrada / evento | Comportamento |
| --- | --- |
| Clique esquerdo na Game View com foco | Captura cursor e habilita caminhada/olhar; primeiro delta descartado |
| WASD | Caminha em relação à câmera, sem ganho diagonal e sem salto/corrida |
| Mouse | Orbita terceira pessoa; pitch limitado; sem controle quando cursor livre |
| Esc | Libera cursor, interrompe comandos e mostra como retomar |
| Perda de foco ou lock | Interrompe comandos; voltar à janela não recaptura |
| Queda / saída dos limites | Retorna ao desembarque; zera velocidade e suavização da câmera |
| Reiniciar Play Mode | Começa no spawn com instrução para clicar |

HUD mínimo: WASD para caminhar, mouse para olhar, Esc para liberar, clique para retomar. Texto permanece legível na resolução de avaliação.

A cena de revisão visual Cais_Iteracao02.unity permanece preservada. A nova Cais_Caminhada.unity permite percorrer cais → degraus → início do caminho e retornar. Obstáculos sólidos bloqueiam o personagem e a câmera; o mar não é superfície caminhável.

Conformidade: cenários e SC-001 a SC-004 da [spec](../spec.md), executados pelo [guia](../quickstart.md). Nenhum comando de diálogo, caderno, salto, corrida ou natação é introduzido.
