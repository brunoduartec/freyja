# Validação — Smart Objects Foundation

## Ambiente

- Unity `6000.3.25f1`, Editor do worktree `085-smart-objects-foundation` (Pipeline 7801), cena isolada `Freyja/Assets/_Project/Scenes/SmartObjects_Demo.unity`.
- Não foi necessário atualizar dependências nem usar créditos Tripo. O banco provisório reutiliza a malha low poly local em FBX, com fonte Blender editável.
- O poço não tem prefab/asset adequado no projeto; nenhum foi criado. O cais de pedra existente foi verificado no NavMesh e recebeu somente configurações futuras `fish` e `watch_sea`.

## Resultados executados

- `unity command --project-path /private/tmp/Freyja-085-smart-objects/Freyja script recompile` — concluído; `recompile_status`: `failed=false`, `errors=[]`, `compilationFailed=false`.
- `unity command --project-path /private/tmp/Freyja-085-smart-objects/Freyja test run --mode EditMode --filter Freyja.SmartObjects --async-tests true --timeout 600` — **12/12 aprovados**. Núcleo, autoria de prefab/IDs, descoberta, ordenação, compatibilidade, concorrência, handles antigos e limpeza.
- `unity command --project-path /private/tmp/Freyja-085-smart-objects/Freyja test run --mode PlayMode --filter Freyja.SmartObjects --async-tests true --timeout 600` — **6/6 aprovados**. Inclui ciclo de vida, fallback A/A/B com três pedidos, chegada antes de ocupação, cancelamento isolado e destino inalcançável com liberação.
- Play Mode manual em `SmartObjects_Demo`: `Elisa Neves`, `Ivo Batista` e `Amélia Faria` chegaram à referência final; `Bench_A.sit_left/right` ficaram `Occupied`, `Bench_B.sit_right` ficou `Occupied` e o quarto slot permaneceu `Available`. Os pontos de pose foram ajustados ao piso e recuados para a borda frontal do banco para não aparentar que os NPCs carregam o móvel. Isso continua sendo apenas pose de referência, sem animação de sentar.
- No cais, as quatro aproximações/poses foram amostradas no NavMesh do cenário. As duas rotas de uma residente até `fish_west` e `watch_sea_east` retornaram `PathComplete`; as aproximações passaram a checagem local de colisores.
- A cena principal não foi alterada. Capturas Unity estão em `specs/085-smart-objects-foundation/evidence/`: `smartobjects-playmode-camera.png`, `smartobjects-bench-a-gizmos.png` e `smartobjects-dock-gizmos.png`.
- `unity command ... console clear`, seguido de `console status` — **0 errors, 0 warnings, 0 logs** no Editor isolado após eliminar as mensagens de transporte transitórias da tentativa cancelada abaixo.
- `git diff --check` — passou na revisão atual do diff; sem whitespace errors.

## Limites observados

- O harness demonstra deslocamento, ocupação e alinhamento curto a uma pose. Não anima sentar/levantar e não implementa atividade, executor ou scheduler, conforme escopo aprovado.
- A validação visual foi feita no Editor Unity em Play Mode. O Game View usa captura por câmera, sem o painel de depuração de `OnGUI`; os estados acima foram consultados diretamente no runtime e também são cobertos pelos testes Play Mode.
- Uma primeira rodada de testes expôs fixtures Edit Mode que dependiam de callbacks de runtime/`SendMessage("OnValidate")`; os fixtures foram corrigidos para registrar objetos diretamente e chamar a validação por reflexão. As duas suítes foram executadas novamente e passaram integralmente.
- Uma tentativa adicional de repetir Edit Mode depois da limpeza do Console ficou em `running` enquanto o Pipeline registrava resets de conexão HTTP; foi cancelada e não foi contabilizada como resultado. Os resultados 12/12 e 6/6 acima são das execuções completas imediatamente anteriores. Os erros transitórios de transporte foram limpos; a compilação da Unity permaneceu sem falhas.
