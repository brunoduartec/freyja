# Feature Specification: Diálogos contextuais

**Feature Branch**: `084-dialogos-contextuais`  
**Created**: 2026-11-26  
**Status**: Aprovada em 2026-11-26  
**Input**: Especificação em `.codex-attachments/.../freyja_prompt_codex_dialogos_contextuais.md`

## User Scenarios & Testing

### User Story 1 — Retomar uma conversa com novas informações (Priority: P1)

O jogador conversa com moradores já existentes, encontra pistas em ordens diferentes e retorna para perguntas e reações liberadas pelo conhecimento que realmente compartilhou com cada pessoa. Uma pista que só o jogador conhece não provoca reação de NPC.

**Why this priority**: Amplia o piloto de diálogo existente e atende o núcleo da investigação sem duplicar interação, câmera ou inventário.

**Independent Test**: Fixture determinística com três NPCs e cinco pistas; verificar ausência de spoiler, comunicação explícita e desbloqueio pelas duas ordens de descoberta.

**Acceptance Scenarios**:

1. **Given** que o jogador descobriu uma pista sem contá-la, **When** conversa com um NPC que não a conhece, **Then** o NPC não reage como se soubesse dela.
2. **Given** que pistas demonstrativas foram descobertas em qualquer ordem e comunicadas ao NPC pertinente, **When** o jogador retorna, **Then** perguntas e reações elegíveis aparecem sem repetir revelações já confirmadas.
3. **Given** que uma conversa é cancelada antes de confirmar a revelação, **When** o jogador inicia outra sessão, **Then** o conteúdo permanece disponível.

### User Story 2 — Escolher, avançar e retomar com segurança (Priority: P1)

O jogador inicia a conversa pela interação existente, lê falas e opções, escolhe uma resposta com consequência explícita e retorna à exploração sem deixar controles presos. Estado narrativo persiste entre sessões de jogo.

**Why this priority**: Entrega o ciclo jogável e conserva o comportamento existente de diálogo e exploração.

**Independent Test**: Play Mode na cena de demonstração: iniciar, avançar, escolher, cancelar, reabrir e verificar estado e controles.

**Acceptance Scenarios**:

1. **Given** um NPC dentro do alcance, **When** o jogador interage, **Then** vê falante, texto, opções e saída em UI legível e pode usar os controles existentes.
2. **Given** uma escolha com consequência, **When** o jogador a confirma, **Then** o efeito é aplicado uma vez e continua após salvar/carregar.
3. **Given** uma sessão ativa, **When** ela é cancelada ou a cena fecha, **Then** input e movimento são restaurados e nenhuma revelação não confirmada é consumida.

### User Story 3 — Localizar conteúdo e inspecionar a autoria (Priority: P2)

A equipe altera falas em conteúdo textual, mantém IDs estáveis, exporta/importa traduções e verifica elegibilidade e erros sem editar scripts de gameplay.

**Why this priority**: Permite iterar narrativa e validar progressão sem acoplar texto à lógica.

**Independent Test**: Exportar/importar CSV com acentos, aspas e multiline; alternar idioma na fixture e validar candidatos/erros de conteúdo no debugger.

**Acceptance Scenarios**:

1. **Given** conteúdo demonstrativo nos dois idiomas, **When** o idioma muda, **Then** texto muda e opções, seleção e progresso permanecem iguais.
2. **Given** uma tradução ausente ou desatualizada, **When** a validação/exportação é executada, **Then** o problema é apontado pelo ID e o idioma fonte continua disponível.
3. **Given** estado e contexto de demonstração, **When** o debugger é aberto, **Then** mostra elegibilidade e limites conhecidos sem alterar o save real.

### Edge Cases

- Nenhuma conversa contextual é elegível: apresentar fallback válido ou não iniciar sessão.
- Erro, cancelamento, perda de foco ou troca de cena durante conversa: restaurar controles e preservar efeitos já confirmados.
- NPC diferente do destinatário recebe informação: conhecimento permanece isolado.
- Save sem dados narrativos: aplicar defaults sem sobrescrever dados preexistentes.
- IDs duplicados, condição/linha inválida, CSV malformado ou placeholders incompatíveis: reportar arquivo e ID.
- Conteúdo temporal: usar relógio de jogo disponível e permitir relógio determinístico em testes.

## Requirements

### Functional Requirements

- **FR-001**: O sistema MUST ampliar a interação de diálogo já existente e preservar inventário, controles, câmera, caderno e cena principal.
- **FR-002**: O conteúdo MUST distinguir fatos demonstrativos do mundo, conhecimento do jogador, conhecimento individual de NPCs e informação comunicada.
- **FR-003**: O jogador MUST poder descobrir e comunicar pistas explicitamente; comunicação MUST atualizar apenas o destinatário e ser idempotente.
- **FR-004**: A seleção MUST filtrar escopo, condições, conhecimento e repetição antes de priorizar; deve ser determinística e revalidar condições antes de efeitos.
- **FR-005**: A vertical slice MUST demonstrar três NPCs, cinco pistas, 30–50 linhas incluindo opções, apresentação, fallback, descoberta, comunicação, pergunta desbloqueada, reação contextual, escolha com efeito, revelação Once, repetição, política temporal, ordem alternativa e persistência, sem alterar lore oficial.
- **FR-006**: O sistema MUST implementar repetição Once, Repeatable e ao menos uma política temporal; conteúdo interrompido antes da confirmação MUST continuar disponível.
- **FR-007**: Falas e opções MUST estar em arquivos textuais versionados, identificados por IDs estáveis e desacoplados de tradução e de referências de cena.
- **FR-008**: A localização MUST suportar `pt-BR` e `en-US` na demonstração, fallback para fonte e troca sem alterar lógica ou save.
- **FR-009**: Um CSV consolidado MUST incluir IDs, falas, opções e metadados de contexto; exportação/importação deve preservar traduções, detectar duplicatas/órfãs/stale e ser idempotente.
- **FR-010**: O estado narrativo MUST persistir pistas, conhecimento, efeitos e histórico sem duplicar estado proprietário de gameplay; saves sem extensão narrativa devem receber defaults compatíveis.
- **FR-011**: Uma ferramenta editor-only MUST inspecionar contexto, candidatos e validação e executar a demonstração isolada sem modificar save de produção.
- **FR-012**: Testes automatizados MUST validar elegibilidade, conhecimento isolado, ordens alternativas, seleção determinística, interrupção, repetição, persistência, localização e CSV.
- **FR-013**: A demonstração visual MUST ser executada em Play Mode com evidências reais de diálogo, escolha, reação contextual e segundo idioma.
- **FR-014**: Dependências narrativas/localização MUST ser verificadas contra Unity `6000.3.25f1`, manifesto e licença; nenhuma atualização ou adição de pacote será assumida sem compatibilidade documentada.

### Key Entities

- **NPC**: identificador estável e conjunto próprio de conhecimentos.
- **Pista**: identificador, origem, texto localizável e estado de descoberta do jogador.
- **Fato narrativo**: verdade demonstrativa ou estado consultável, distinto de crença/depoimento.
- **Storylet**: conteúdo, gatilho, condições, prioridade, repetição e efeitos confirmáveis.
- **Comunicação**: registro idempotente de informação transmitida a NPC específico.
- **Histórico de diálogo**: início, linhas/efeitos confirmados, conclusão e política de repetição.
- **Linha localizável**: ID permanente, falante, nó, fonte, contexto, hash e variantes linguísticas.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Em uma sessão demonstrativa, os três NPCs e cinco pistas suportam os cenários prescritos com 30–50 linhas de diálogo/opção.
- **SC-002**: Zero reações a pistas não conhecidas pelo NPC nos casos automatizados, em ambas as ordens alternativas.
- **SC-003**: Revelações Once e efeitos confirmados não se repetem após cancelamento, repetição de comando e save/load; conteúdo Repeatable permanece acessível.
- **SC-004**: CSV exportado e reimportado preserva texto traduzido, caracteres especiais e IDs, e sinaliza linhas stale/missing/órfãs.
- **SC-005**: Execução em Editor/Play Mode registra resultado de testes, passos e capturas reais dos dois idiomas e das mudanças contextuais.

## Assumptions

- A implementação reaproveita `IslandDialogue`, `CaisSessionController`, `IslandInventory`, `CoastalDayNight` e o Test Framework já presente.
- Não há Yarn Spinner, Unity Localization ou SaveSystem global detectados na arquitetura inicialmente inspecionada; o plano deve verificar novamente e escolher integração compatível de menor risco, sem declarar suporte Yarn se não for instalado e compilado.
- Se não existir persistência de jogo, a demonstração usa adaptador/fixture narrativa isolada, documentando a integração de produção pendente em vez de inventar um save global.
- NPCs, pistas e falas inéditos pertencem a namespace Demo e não estabelecem lore oficial.
- A cena principal possui mudanças locais do usuário e não será salva/alterada durante a fase de especificação; a estratégia de demo e integração será decidida no plano após aprovação.
