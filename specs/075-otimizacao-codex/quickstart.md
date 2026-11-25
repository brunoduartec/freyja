# Validação da configuração

1. Na raiz, verifique `codex mcp list` e a precedência com `codex mcp get <servidor>`. Repita com `codex -c 'mcp_servers.headroom.enabled=true' mcp get headroom` para validar o opt-in.
2. Verifique a descoberta em uma sessão nova. Solicite uma resposta curta de cada papel, em sequência, fechando o anterior. Confirme a identidade nos eventos da sessão.
3. Abra os editores pelo fluxo em `Tools/MCP/README.md`. Compare o catálogo Unity completo com os modos de agrupamento e filtro. Em ambos os editores, confirme por leitura o caminho do projeto ou da fonte.
4. Confira a ausência de mudanças em `Freyja/` e `Source/Blender/` com Git. Consulte `docs/codex-token-optimization.md` para resultados, reversão e uso recorrente.
