# Plano: Otimização do Codex no Freyja

**Branch** `075-otimizacao-codex` · **Spec** [spec.md](spec.md)

## Resumo

Aplicar um perfil Codex local, preservar as integrações existentes, instalar o plugin oficial Unity e disponibilizar três agentes e duas skills locais. Comparar catálogos dos editores e usar consultas reais de leitura para decidir os filtros.

## Contexto técnico

- Codex CLI 0.169.0, TOML por projeto confiável e skills em `.agents/skills/`.
- Spec Kit 1.0.1. Unity 6000.3.25f1, Built-in e CLI 1.0.1. Blender 5.2.2 LTS e bridge existente.
- Testes pontuais de configuração efetiva, descoberta/invocação, MCP e integridade Git. Validação de uso em sessão nova.

## Constituição

O GDD continua sendo a fonte das decisões do jogo. O fluxo mantém Unity e Blender via MCP e práticas de validação visual. As mudanças se limitam ao ambiente de trabalho e à documentação. Cada tarefa aponta para arquivo ou evidência.

## Estrutura

- `.codex/config.toml` e `.codex/agents/*.toml`
- `.agents/skills/freyja-unity-edit/SKILL.md` e `.agents/skills/freyja-blender-export/SKILL.md`
- `AGENTS.md` e `docs/codex-token-optimization.md`
- [Pesquisa](research.md), [entidades](data-model.md) e [validação](quickstart.md)

## Execução

1. Preservar a base publicada `26ac056`, os arquivos da 074 e as configurações anteriores. Registrar o estado antes.
2. Verificar a origem e instalar o plugin oficial. Abrir o projeto com o lançador existente e consultar os bridges. Medir catálogos completos, agrupados e filtrados antes de decidir.
3. Gravar o perfil local e a regra de delegação. Criar e validar os papéis e as duas skills. Concentrar regras de uso frequente no guia de agentes e mover os procedimentos específicos para as skills.
4. Validar a configuração e as integrações. Registrar evidências, rollback e protocolo de medição. Nenhum contrato público novo.
