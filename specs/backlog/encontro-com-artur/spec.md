# Spec 003 — Primeiro encontro com Artur

**Criada:** 12/11/2026 · **Estado:** proposta curta para avaliação do usuário.

## Cenário e teste

### Conversar com Artur no cais (P1)

O jogador encontra Artur no cais e inicia uma breve conversa. O encontro apresenta o primeiro morador e estabelece que ele quer saber quando os visitantes vão partir.

**Teste independente:** aproximar-se de Artur, iniciar e concluir a conversa; depois continuar caminhando.

**Aceitação:**
1. Artur está visível no cais, próximo ao caminho do jogador, representado por um personagem provisório identificável.
2. Ao se aproximar, o jogador vê como iniciar a conversa e consegue fazê-lo.
3. Artur pergunta quando os visitantes vão embora e oferece ajuda para soltar as amarras enquanto há luz, conforme o GDD.
4. O jogador consegue avançar pelas falas, encerrar a conversa e voltar a caminhar.

**Casos de borda:** fora de alcance, a conversa não abre; se o jogador sair da conversa ou a repetir, o controle continua funcionando. Não há prazo que impeça encontrar Artur.

## Requisitos

- **FR-001:** apresentar Artur no cenário existente com nome e aparência provisória que permitam identificá-lo como trabalhador do cais.
- **FR-002:** indicar a interação quando Artur estiver ao alcance e permitir iniciá-la com teclado.
- **FR-003:** exibir falas legíveis, identificando quem está falando, e permitir avançar ou sair da conversa.
- **FR-004:** suspender a caminhada durante a conversa e devolver o controle ao encerrá-la, respeitando o comportamento de cursor já existente.
- **FR-005:** permitir conversar novamente sem duplicar personagens nem prender o jogador.
- **FR-006:** manter o percurso e a recuperação após queda da entrega anterior funcionando.

## Critérios de sucesso

- **SC-001:** iniciar e concluir a conversa duas vezes, sem travamento e conseguindo caminhar após cada uma.
- **SC-002:** após a conversa, o jogador consegue dizer quem é Artur e o que ele quer saber sobre a partida.
- **SC-003:** tentar conversar de longe não abre o diálogo; aproximar-se disponibiliza a interação.

## Premissas e limites

Reutilizar o cais e os controles existentes. Artur será provisório; seu visual e biografia finais ainda não estão aprovados. A conversa é curta e segue o que o GDD estabelece.

**Fora do escopo:** outros moradores ou visitantes modelados, diálogos ramificados, caderno, pistas, dublagem, animações finais, horário jogável e expansão do mapa.

**Próximo passo:** avaliação desta spec pelo usuário antes do planejamento e da implementação.
