# Entidades de configuração

- **Perfil Freyja** — TOML local de projeto confiável. Declara overrides de servidores e o limite de delegação. Valores omitidos seguem os layers do Codex.
- **Agente** — TOML com nome único, descrição e instruções. Estados de uso: disponível, aberto para uma tarefa, encerrado antes da próxima delegação.
- **Skill** — diretório com o mesmo nome do frontmatter do `SKILL.md`. Contém procedimento especializado carregado sob demanda.
- **Evidência** — validação identificável com comando ou ferramenta, resultado e limitação. O relatório registra os resultados sem payloads e credenciais.
