# Especificação: Otimização do Codex no Freyja

- **Branch** `075-otimizacao-codex`
- **Data** 2026-11-25
- **Status** Aprovada

## Cenários e testes

### US1 — Trabalhar com o perfil local (P1)

Em uma nova sessão do Freyja, a pessoa desenvolvedora consegue navegar no C#, editar e validar no Unity, trabalhar no Blender e recuperar ferramentas opcionais quando a tarefa pede. O perfil reduz leituras, saídas e chamadas redundantes.

**Teste independente** — Ler a configuração efetiva e realizar uma consulta real em cada editor aberto no projeto. Conferir que as ferramentas opcionais podem ser reativadas.

### US2 — Delegar uma responsabilidade quando necessário (P1)

A sessão principal resolve trabalhos pequenos. Para tarefas que se beneficiam de especialização, ela pode acionar Implementador, Validador ou Game Designer com objetivo, arquivos e critérios pertinentes.

**Teste independente** — A sessão reconhece os três nomes, invoca cada um em sequência e recebe três respostas sem alterar arquivos do jogo.

### US3 — Retomar e medir o fluxo (P2)

A pessoa desenvolvedora encontra orientações operacionais, evidência dos testes e um procedimento de reversão. Pode comparar tarefas equivalentes preservando critérios de qualidade.

**Teste independente** — Localizar no relatório a configuração, as verificações aplicadas, os comandos de reversão e o protocolo de medição.

### Casos limite

- Editor fechado ou bridge conectado a outro projeto. O teste de conexão informa o diagnóstico e exige identificação positiva do projeto.
- Projeto não confiável ou execução iniciada fora do repositório. O guia explica como identificar a configuração que entrou em vigor.
- Sessão sem cota comparável. Relatar indicadores observáveis e deixar em aberto a medição de cota.

## Requisitos

- **FR-001** Preservar branch, fluxos e artefatos do Spec Kit, Unity, Blender e Tripo. Criar ou salvar mudanças no jogo fica fora do escopo.
- **FR-002** Usar configuração por projeto onde a versão instalada oferecer suporte. Preservar as preferências do modelo e documentar opt-in e reversão dos opcionais.
- **FR-003** Configurar os agentes `implementer`, `validator` e `game-designer`. Manter um delegado ativo por vez, sem delegação em cadeia.
- **FR-004** Entregar duas skills locais de consulta sob demanda, para edição e verificação no Unity e para exportação Blender → Unity.
- **FR-005** Validar a compatibilidade do plugin oficial da Unity e evitar duplicar suas skills ou substituir o Spec Kit.
- **FR-006** Preservar validação de compilação, Play Mode e inspeção visual quando o tipo de mudança exigir. Resumir evidências e manter o original acessível para diagnóstico.
- **FR-007** Documentar inventário, decisões, alterações, testes, reversão e evidências de economia.

## Critérios de sucesso

- **SC-001** Os três agentes e as duas skills aparecem em sessão nova. Cada papel responde a uma invocação de teste.
- **SC-002** Unity e Blender respondem no projeto correto. As operações essenciais continuam acessíveis com o perfil escolhido.
- **SC-003** A verificação Git preserva os arquivos locais preexistentes e não identifica mudanças em código, cenas, prefabs, modelos ou pacotes do jogo.
- **SC-004** O relatório identifica os resultados da comparação e explicita quais métricas de cota e qualidade ainda exigem tarefas equivalentes.

## Premissas

A instalação do plugin oficial foi aprovada. A versão dos editores e os contratos do jogo permanecem os do repositório. Os nomes usam a grafia oficial Freyja.
