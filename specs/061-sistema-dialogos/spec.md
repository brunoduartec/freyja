# Sistema de diálogos com moradores

**Branch** `061-sistema-dialogos`  
**Status** Aprovada em 2026-11-19  
**Criada em** 2026-11-19

## Cenários de uso e aceitação

### P1 — Conversar na ilha

Ao se aproximar de um morador do piloto, o jogador vê a ação **Conversar**. Pode iniciá-la, identificar quem fala, escolher um assunto, ler a resposta e encerrar a conversa para voltar a caminhar.

### P1 — Voltar com uma nova pergunta

No recorte proposto, o jogador fala com Nuno sobre a próxima viagem e com Luana sobre Helena. A pergunta sobre a relação entre a partida e o nascimento aparece com Ivo após os dois depoimentos, em qualquer ordem. Antes disso, Ivo já pode conversar sobre os assuntos iniciais. A pergunta e o recorte vêm do [backlog da chegada](../001-chegada-e-marca/backlog-chegada-e-marca.md).

### Casos de borda

- Fora do alcance, a ação de conversar fica indisponível.
- Sair antes de ouvir um depoimento novo deixa o assunto disponível na próxima conversa.
- Repetir uma conversa mantém o progresso da sessão e a entrada daquele depoimento aparece uma única vez.

## Requisitos

- **FR-01** — Indicar qual morador pode ser abordado e permitir iniciar e encerrar a interação.
- **FR-02** — Mostrar o nome de quem fala, a fala atual e as escolhas disponíveis em texto legível na câmera do jogo.
- **FR-03** — Exibir a pergunta de Ivo assim que os dois depoimentos estiverem disponíveis na sessão, inclusive se o jogador já tiver falado com ele.
- **FR-04** — Registrar depoimentos com o nome da fonte. A pergunta representa uma hipótese do jogador. A tira azul, quando observada, é uma observação.

### Entidades principais

- **Morador** — Interlocutor com nome e assuntos próprios.
- **Assunto** — Pergunta ou fala que pode depender de uma descoberta anterior.
- **Depoimento** — Informação ouvida de um morador, associada à sua fonte.

## Critérios de sucesso

- Os três moradores do piloto permitem iniciar, conduzir e encerrar ao menos uma conversa cada na cena principal.
- A pergunta de Ivo permanece indisponível com zero ou um depoimento e aparece nas duas ordens possíveis de descoberta.
- Reabrir os diálogos mantém a pergunta disponível e cada depoimento registrado tem exatamente uma entrada com sua fonte.

## Premissas e decisão de design

Proposta inicial de uma conversa textual, com painel discreto no rodapé e lista de assuntos, mantendo os personagens em cena. O piloto cobre Nuno, Luana e Ivo na cena principal em uma sessão de jogo. As falas finais dependem de revisão narrativa. Opção A aprovada, com assuntos que se abrem conforme o jogador reúne depoimentos.

| Referência | Possibilidade para Freyja |
| --- | --- |
| [Ace Attorney: Apollo Justice](https://static.capcom.com/manuals/aaaj/AAAJ_DS_DMNL_EN.pdf) | Escolher um assunto e voltar a uma testemunha quando uma pista abrir uma nova pergunta. A apresentação manual de evidência é uma decisão separada. |
| [Pentiment](https://news.xbox.com/en-us/podcast/pentiment-herman-miller-gaming/) | Escolher como abordar o assunto. O tom da fala pode mudar o rumo da conversa. |
| [Oxenfree](https://nightschoolstudio.com/oxenfree/) | Conversar durante o deslocamento, com respostas surgindo enquanto a cena continua. |
