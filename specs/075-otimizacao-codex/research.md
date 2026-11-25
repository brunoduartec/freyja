# Pesquisa de compatibilidade

| Decisão | Evidência e alternativas |
|---|---|
| Spec Kit com plugin oficial Unity | O projeto já usa o Spec Kit. O catálogo oferece o plugin oficial Unity para Codex e a Unity documenta suporte a Unity 6+. BMGD e GSD têm fluxos sobrepostos. Kits de papéis e skills acrescentariam catálogo e manutenção sem benefício demonstrado. |
| Configuração por projeto | A raiz está marcada como confiável. A CLI aceita overrides de `mcp_servers.<nome>.enabled`. A precedência real será conferida após gravar o arquivo. |
| Unity CLI e MCP oficial | A versão local tem `--tool-groups`, `--tools` e `--exclude-tools`. A alternativa `unity-mcp-efficient` não tem compatibilidade demonstrada com esse servidor. A decisão de filtros depende dos catálogos observados com o editor conectado. |
| RTK explícito e Headroom opcional | RTK informa ausência de hook e já tem integração pelo guia de agentes. Headroom está em stdio, sem proxy detectado na auditoria. Desabilitar apenas no perfil do Freyja. |
| Um delegado aberto | O Codex documenta limite excluindo a sessão principal e três campos obrigatórios por agente. Controles de sandbox e aprovação são herdados da sessão principal. |
| Prioridade de economia | A amostra mostrou ferramentas com saídas muito extensas. Usar consultas específicas e agrupar validações antes de inferir ganhos de cota. |
