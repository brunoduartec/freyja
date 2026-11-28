# Custo e isolamento

- A janela usa polling nominal de 2 Hz; o overlay de Scene View desenha em repaint e lê snapshots existentes. Não é executado polling a cada frame nem pathfinding diagnóstico.
- Histórico limitado a 512 eventos. Overflow validado com 514 eventos: buffer retornou 512, descarte contado 2. Evidência: `diagnostics-ring-overflow-editmode.json`.
- Microbenchmark em Play Mode, 3 NPCs, 10 aquecimentos e 100 capturas: janela aberta 2,99 ms / 0 B (0,030 ms por captura); fechada 2,92 ms / 0 B (0,029 ms por captura). Evidência: `diagnostics-capture-microbenchmark-playmode.json`.
- Escopo: mede chamadas síncronas a `CaptureSnapshot` no thread principal e delta reportado por `GC.GetAllocatedBytesForCurrentThread`. A chamada bloqueia o loop durante a amostra; não mede repaint/update completo, CPU por frame, alocações do Editor entre updates ou custo no Player. D24 permanece PARTIAL e estes números não são budget de performance.
- A build StandaloneOSX final, executada após a correção de cleanup, terminou com 0 erros e 0 avisos. `ScriptingAssemblies.json` e as DLLs Managed não contêm tipos/assemblies de diagnóstico Editor.
- Esta checagem confirma isolamento dos tipos observados nessa build; não substitui comparação de custo nem validação de todos os alvos/plataformas.
