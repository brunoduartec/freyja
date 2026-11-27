# Modelo de dados — Diálogos contextuais

## Entidades

- **NarrativeFact**: ID estável `demo.*`, valor tipado e origem; verdade do cenário distinta de depoimento, crença ou hipótese.
- **ClueDefinition**: ID, source/context IDs, linha descritiva localizável; definições são imutáveis durante sessão.
- **NarrativeProgress**: versão de schema, conjunto de pistas descobertas pelo jogador, conhecimentos por NPC, comunicações e histórico semântico.
- **NpcKnowledge**: `npcId`, `clueId` e origem/estado; informação de um NPC não implica conhecimento dos demais.
- **DialogueUnit/Storylet**: IDs de NPC, gatilho e unidade de conversa; condições, prioridade e linhas/opções têm fonte autoritativa no Yarn; repetição específica e efeitos semânticos usam comandos/checkpoints de domínio autorizados.
- **DialogueHistory**: inícios, linhas/efeitos confirmados e checkpoints de conclusão; abrir ou cancelar não consome revelação não confirmada.
- **DialogueLine**: ID permanente, node/speaker IDs, source/context, hash da fonte e textos `pt-BR`/`en-US`; tradução não participa da lógica.
- **NarrativeEffect**: comando enumerado/validado, alvo/ID estável, idempotência por efeito; nunca execução arbitrária de método via conteúdo.

## Regras

- `.yarn` contém fluxo e condições; `strings.csv` Yarn mantém IDs e traduções. Exportador enriquece metadados, sem alterar source/fluxo e sem uma segunda fonte de lógica.
- Conteúdo PT-BR e CSV secundário en-US pertencem ao módulo Demo; locale não é chave de seleção.

1. Descoberta do jogador e conhecimento de NPCs são conjuntos separados.
2. Compartilhar atualiza apenas destinatário explícito e registra comunicação uma vez.
3. Seleção considera condições e políticas antes da prioridade; desempate é estável/testável.
4. Once consome no checkpoint de revelação confirmado definido pelo conteúdo; Repeatable nunca marca consumido; Cooldown usa relógio injetável de jogo.
5. Estado ausente ou versão antiga inicia defaults e não sobrescreve módulos de gameplay.
6. Idioma é preferência de apresentação; não serializar idioma, texto ou índice transitório de opção como progresso narrativo.

## Implementação Demo em 2026-11-26

- `NarrativeProgress` atualmente persiste versão, hora acumulada, IDs descobertos, detalhes de descoberta (`kind`/`source`), conhecimento por NPC, IDs de efeitos e histórico de checkpoints. Não há entidade genérica `NarrativeFact`, início de conversa ou histórico de cancelamento.
- Os tipos Demo usados no caderno são depoimento e observação; hipótese do jogador não foi implementada como registro de domínio.
- O Demo usa IDs `demo.*`; os IDs que Yarn entrega à API de localização são `line:demo.*`. As condições de nodes/Storylets vivem no `.yarn`; o domínio só implementa os comandos autorizados.
- A pergunta contextual de Ivo e o checkpoint `Once` são testados. Save geral, seleção saliency prioritária, migrações futuras e história completa de sessão permanecem fora da evidência atual.
