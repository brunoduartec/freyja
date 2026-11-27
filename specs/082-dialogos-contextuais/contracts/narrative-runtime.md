# Contrato interno — runtime narrativo

Contrato interno alinhado a Yarn Spinner 3.2.8. Yarn é a única fonte de condições e fluxo; domínio não reimplementa elegibilidade textual.

- Leitura sem efeitos: `HasPlayerClue(clueId)`, `NpcKnows(npcId, clueId)`, `WasShared(npcId, clueId)`, consulta de relógio/fatos conhecidos.
- Comandos enumerados: descobrir pista, compartilhar pista com NPC, confirmar revelação/linha, alterar estado demo permitido.
- Seleção: Yarn node groups/saliency selecionam unidades contextuais por predicados de leitura. Adapter usa API suportada em 3.2.8 para consultar saliency; introspecção não disponível deve ser indicada como limite, sem replicar condição. Nunca iniciar unidade inelegível.
- Histórico/checkpoint: registrar início, confirmação semântica, conclusão ou cancelamento sem consumir conteúdo ainda não confirmado.
- Persistência: serializar somente progresso narrativo versionado; carregar ausência como estado vazio e aplicar migração sem tocar estado de outros sistemas.
- Apresentação/localização: `.yarn` em `pt-BR`, IDs de linha/opção estáveis, CSV native Yarn `en-US`, provider nativo Yarn e fallback fonte; CSV enriquecido mantém a forma que Yarn importa. Sem pacote adicional Unity Localization.

## Estado implementado

- A API disponível hoje é `NarrativeYarnBridge`: variáveis Yarn `$demo_*`, `discover(clueId, kind)`, `communicate(npcId, clueId)`, `effect_once(effectId)` e `checkpoint(checkpointId)`. Cada comando valida NPC/ID/categoria/alvo contra a allow-list Demo.
- `NpcKnows` e comunicação usam `NarrativeProgress`; elegibilidade permanece em condições `when:` dos nodes e avalia na abertura do node, não por polling. Não há consulta geral a candidatos de saliency nem interface `WasShared` independente.
- O progresso Demo mantém descobertas com tipo/fonte, conhecimento por NPC, efeitos, hora e checkpoints. Início/cancelamento de conversa não compõem um histórico separado; não há integração com save de produção.
- O Yarn mode é ativado por F2; o diálogo legado continua padrão. `IslandDialogue.OnGUI` é o único renderer/modal; PlayMode cobre bridge, runner e locales, mas não o ciclo real do `CaisSessionController`/controles.
- `pt-BR` e `en-US` têm CSV nativos e foram lidos pelo provider em PlayMode. Fallback quando falta tradução e execução visual no Editor continuam pendentes; sem Unity Localization.
