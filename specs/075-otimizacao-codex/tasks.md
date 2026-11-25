# Tarefas: Otimização do Codex no Freyja

## Fase 1 — Preparação

- [x] T001 Preservar a base e backups antes de gravar `.specify/feature.json` e `specs/075-otimizacao-codex/spec.md`.

## Fase 2 — Compatibilidade

- [x] T002 Conferir a procedência, instalar o plugin Unity e registrar versão e alterações de perfil em `docs/codex-token-optimization.md`.

## Fase 3 — US1 Perfil local

**Teste independente** — Verificar perfil efetivo, opt-in e leitura nos dois editores.

- [x] T003 [US1] Comparar catálogos, executar leituras reais seguindo `Tools/MCP/README.md` e registrar evidência no relatório.
- [x] T004 [US1] Definir o perfil em `.codex/config.toml` e verificar precedência, opt-in e disponibilidade do fluxo browser/Tripo.
- [x] T005 [US1] Criar e validar `.agents/skills/freyja-unity-edit/SKILL.md` e `.agents/skills/freyja-blender-export/SKILL.md` e enxugar `AGENTS.md`.

## Fase 4 — US2 Papéis sob demanda

**Teste independente** — Acionar cada papel em uma sessão efêmera, um de cada vez.

- [x] T006 [US2] Criar `.codex/agents/implementer.toml`, `.codex/agents/validator.toml` e `.codex/agents/game-designer.toml`.
- [x] T007 [US2] Verificar descoberta e invocação e registrar evidência em `docs/codex-token-optimization.md`.

## Fase 5 — US3 Operação e evidências

**Teste independente** — Recuperar instruções operacionais, resultados e rollback no relatório.

- [x] T008 [US3] Completar `docs/codex-token-optimization.md` com inventário, comparação, limitações, rollback e protocolo de medição.

## Fase final — Integridade

- [x] T009 Verificar Git, hashes do jogo e arquivos locais preservados e fechar a matriz neste `tasks.md`.

## Dependências e entrega

Preparação → compatibilidade → perfil e papéis → relatório e integridade. Validar os papéis em sequência. Consultas independentes ao status dos editores podem ocorrer no mesmo lote sem disputar uma sessão. US1 e US2 compõem a configuração mínima. US3 consolida a entrega.
