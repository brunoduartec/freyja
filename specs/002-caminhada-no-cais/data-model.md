# Dados e estados — Caminhar pelo cais

Somente dados locais de configuração e estado por execução; nenhuma persistência.

| Entidade | Dados | Regras |
| --- | --- | --- |
| PlayerConfig | velocidade, altura/raio, gravidade, step offset, skin width | Valores positivos/coerentes; step offset menor que altura; escala métrica |
| CameraConfig | alvo, distância, limites de pitch, sensibilidade, raio de proteção, mask | Distância/raio positivos; mask não inclui jogador, mar ou triggers |
| SpawnPoint | posição dos pés e orientação inicial | Sobre chão sólido, cápsula livre e dentro dos limites |
| RecoveryBounds | altura mínima e limites horizontais | Incluem spawn e trecho aprovado; sair dispara recuperação |
| SessionState | estado de controle, foco da janela, velocidade vertical, yaw/pitch e caches de câmera | Inicialização por sessão; sem estado estático ou salvo |
| ControlsHint | texto e visibilidade das instruções | Coerente com bindings reais e estado de captura |

## Transições

| Estado | Evento | Resultado |
| --- | --- | --- |
| Inicializando | Spawn validado | Cursor livre, instrução visível; jogador parado |
| Cursor livre | Clique com foco | Controle ativo; cursor capturado; descartar delta inicial |
| Ativo | Esc, perda de foco ou lock | Cursor livre quando aplicável; comandos e deltas limpos |
| Cursor livre | Ganhar foco | Continuar parado até clique explícito |
| Ativo / livre | Queda ou saída de bounds | Recuperar no spawn; zerar movimento e reposicionar câmera |
| Recuperando | Posicionamento concluído | Retomar ativo somente se antes ativo e ainda com foco/lock; caso contrário, livre |
| Qualquer | Nova execução | Recriar estado inicial e spawn |

Spawn inválido gera diagnóstico claro e bloqueia controle, sem loop infinito de recuperação. Encerrar execução ou desabilitar componentes libera cursor e remove assinaturas de eventos.

Movimento planar limita a magnitude a 1 antes de aplicar velocidade; gravidade é integrada separadamente. UI exibe estado e instruções, sem controlar a posição do jogador.
