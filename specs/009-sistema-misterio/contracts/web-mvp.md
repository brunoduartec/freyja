# Contrato da experiência web — MVP de pistas

## Entrada e apresentação

Abrir a página servida por HTTP local carrega um caso e inicia o cenário normal. A aplicação exibe mapa, três fichas clicáveis, área de diálogo e painel permanente de evidências. A lista começa vazia e mostra somente o que foi descoberto. Nenhum conteúdo autoral oculto ou resposta correta é impresso antecipadamente.

Fichas e pontos são controles operáveis por mouse e teclado, com nomes acessíveis e foco visível. A lista de pistas não fica atrás do diálogo nem depende de abrir um modal ou trocar de aba. Em telas estreitas, as áreas são reorganizadas e têm rolagem própria quando necessário.

## Interações

- Clicar numa ficha disponível abre o personagem correto e seus assuntos permitidos.
- Avançar falas preserva sequência; efeitos acontecem uma única vez no ponto definido pela autoria.
- Escolher pergunta ou apresentar evidência pode liberar outra resposta. Opções indisponíveis não concedem conteúdo.
- Examinar um ponto registra evidência com descrição, natureza e origem; repetir não duplica nem fortalece a prova.
- Selecionar hipótese e evidências solicita avaliação explícita. A aplicação informa sustentação insuficiente ou contradição sem entregar a combinação correta.
- A conclusão sustentada encerra a amostra; não anuncia vitória sobre a maldição.
- Reiniciar restaura o cenário escolhido. Não há salvamento persistente no primeiro MVP.

## Cenários de teste de fontes

O modo de revisão permite repetir o caso com fontes indisponíveis, claramente identificado como teste de autoria. Uma pessoa morta não oferece falas atuais. Relatos obtidos antes da perda continuam legíveis com sua atribuição original. O conteúdo deve fornecer alternativas que preservem a conclusão nas sete configurações definidas na spec.

Essas configurações não são eventos obrigatórios da história e não simulam a passagem dos três dias. Não há menu de matar personagens como ação narrativa do jogador.

## Ferramentas locais previstas

Após a implementação, a aplicação será servida com `python3 -m http.server 8009 --directory Tools/MysteryWeb`. A validação e as exportações são executadas por scripts Node, sem backend em execução. Opções e resultados desses scripts estão em [quickstart.md](../quickstart.md).

O validador devolve sucesso somente quando o conteúdo é consistente e todas as configurações entregues mantêm um caminho. Em falha, imprime IDs de fontes, requisitos e cenário bloqueado; a interface de jogo continua usando nomes e textos naturais. Um fixture defeituoso proposital pertence aos testes, não à seleção de cenários do jogador.
