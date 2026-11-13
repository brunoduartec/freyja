# Acompanhamento das specs

O [GDD](../GDD.md) guarda as decisões gerais. Cada pasta de feature contém `spec.md` para comportamento e aceitação, `plan.md` para implementação e `tasks.md` para execução. Os estados abaixo acompanham entregas. O progresso de uma entrega fica nas caixas de seleção do seu `tasks.md`.

| Entrega | Estado | Dependência |
| --- | --- | --- |
| [009 · Sistema de mistério](009-sistema-misterio/spec.md) | Plano do MVP web e 30 tarefas definidos; implementação não iniciada | Próxima etapa: executar o recorte de pistas e apresentar para avaliação |
| [001 · Cais — iterações curtas](001-chegada-e-marca/spec.md) | Blender aprovado; revisão Unity pronta para avaliação | Nenhuma |
| [002 · Caminhar pelo cais](002-caminhada-no-cais/spec.md) | Cena jogável e validação técnica prontas; aguardando avaliação | Cena de revisão da 001 |
| Backlog · Visual PS1 e primeiro elenco em movimento | Proposta futura; sem spec própria | Trecho jogável e revisão visual |
| [003 · Ilha bruta caminhável](003-ilha-bruta/spec.md) | Cena e modelo prontos para avaliação; testes técnicos registrados | Cais e caminhada das entregas anteriores |
| [004 · Relevo da ilha e recanto isolado](004-relevo-ilha/spec.md) | Encerrada a pedido do usuário; entrega parcial e validação limitada | Ilha bruta da 003 |
| [005 · Relevo idêntico à referência](005-relevo-fiel-referencia/spec.md) | Primeira maquete apresentada; aguardando avaliação antes de integrar e validar caminhada | Reaproveitamento compatível da 004 e seis referências de exatidão |
| [Backlog · Primeiro encontro com Artur](backlog/encontro-com-artur/spec.md) | Proposta adiada | Cais e caminhada |
| Backlog · Vila explorável e rotinas | Proposta futura; sem spec própria | Cais, caminhada e validação visual |
| Backlog · Arquivos, partidas e dedução da regra | Proposta futura; sem numeração de spec reservada | 001 e lugares relevantes da 003 |
| Backlog · Possessão e consequências | Proposta futura; sem numeração de spec reservada | Arquivos, partidas e decisões narrativas do GDD |

A lista de entregas é uma proposta de produção. No Codex, as skills ficam em `.agents/skills/`. Use `$speckit-specify`, `$speckit-plan` e `$speckit-tasks` para novas features. Para revisar uma entrega antes de implementar, use `$speckit-analyze`. Para retomar com outro modelo, compartilhe os arquivos Markdown ou abra a mesma pasta e indique o caminho da spec. A sintaxe dos comandos varia por integração. [Fluxo oficial do Spec Kit](https://github.github.com/spec-kit/quickstart.html)

### Repasse para outro modelo

> Consulte `.specify/feature.json` para localizar a entrega ativa. A 005 tem primeira maquete e vistas comparativas apresentadas; as tarefas posteriores ao checkpoint aguardam avaliação visual. A 004 foi encerrada com entrega parcial. Cada nova entrega começa por uma spec curta para avaliação, mesmo quando o usuário disser "pode seguir" após avaliar a anterior. O encontro com Artur e o recorte narrativo da 001 permanecem no backlog.

Ao trocar de modelo ou máquina, compartilhe o GDD, a constituição e a pasta da feature. A integração com Unity MCP e Blender MCP precisa estar disponível na sessão que for usar os editores.


## Branches para novas specs

Cada nova spec usa uma branch própria no padrão `<NNN>-<nome-curto>`, alinhada à pasta da feature. Resolver o nome com o modo `--dry-run` de `.specify/scripts/bash/create-new-feature.sh`, criar/selecionar a branch Git e então gerar a spec e seus metadados. O script base local não cria a branch sozinho. Ao retomar a entrega, usar sua branch e manter `.specify/feature.json` coerente.

A regra completa, incluindo preservação de alterações existentes e hooks oficiais, está em [AGENTS.md](../AGENTS.md#git-e-branches-do-spec-kit). Continua obrigatória a aprovação explícita da spec antes de plan, tasks e implementação.
