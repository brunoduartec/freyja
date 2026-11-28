# Quickstart de validação

## Pré-condições

- Abrir `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` no Unity `6000.3.25f1`.
- Confirmar Editor MCP conectado e Console compilado; registrar cursor do Console. Não limpar o Console.
- Cena com o driver de integração e residentes Ivo, Helena e Luana reais. Não substituir por harness fake para critérios Play Mode.

## Fluxos

1. **Edit Mode**: menu `Freyja > NPC > Debug Monitor`; confirmar contexto Edit, dados ao vivo indisponíveis e comandos desabilitados. Fechar/reabrir sem mudar cena.
2. **Snapshot sem janela**: via Unity MCP `script_eval`, chamar `NpcDebugDiagnostics.CaptureJson()` e `CaptureNpcJson("world_ivo")`; validar JSON, schema, contexto e ausência de referências vivas. Usar nomes finais do API contract após implementação.
3. **Play Mode / lista**: iniciar Play, abrir monitor, atualizar e filtrar por ID/nome. Esperado: exatamente os três IDs reais, intenção e execução distintas, sem mutação provocada por captura.
4. **Recursos**: avançar pelo relógio real até a disputa `sit`; comparar os dois slots oeste e slots restantes com estado de execução. Não deduzir ocupação por pedido de reserva.
5. **Comando cancelamento**: cancelar um NPC sentado ou em deslocamento; registrar resultado imediato, observar cleanup pela fonte real, depois capturar snapshot e eventos novamente. Não aceitar soltura prematura de slot.
6. **Relógio**: pausar/reiniciar avanço automático; avançar 15 min/1 h e cruzar meia-noite uma vez. Confirmar dia projetado, alteração de intenção e que nenhuma atividade pulada foi marcada completa. Escolher taxa do calendário e confirmar locomocão/operational time não mudaram por `Time.timeScale`.
7. **Visualização/exportação**: selecionar NPC e ligar caminho/destino; confirmar que o número de pedidos de navegação e estado de cena não mudou. Exportar global e individual para `Temp/NpcDebugTools/`, abrir e parsear JSON; copiar resumo.
8. **Lifecycle**: fechar a janela e sair de Play Mode; conferir restauração da pausa/taxa somente quando ainda pertence ao monitor e remoção dos overlays/listeners. Repetir abrir/fechar e start/stop três vezes.
9. **Custo**: no mesmo contexto de três NPCs, coletar amostras comparáveis fechada/aberta; registrar tempo de captura/atualização, CPU/GC disponíveis, polling, capacidade de buffer e condições.

## Registros/evidência a entregar

- Resultado por D01–D26: PASS, FAIL, BLOCKED ou NOT RUN, com ambiente e path de evidência.
- Snapshot JSON real global e de um NPC correlacionado a Play Mode, com interpretação curta.
- Capturas do monitor e Scene View legíveis; screenshots são evidência visual complementar ao JSON.
- `Console` e importação/compilação, regressões existentes e limitações de capacidades ausentes.
- Custos medidos sem extrapolar de três NPCs para 30.
