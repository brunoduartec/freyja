# Estudo de shaders e comportamentos do mar

**Branch** `031-estudo-mar`  
**Data** 18/11/2026  
**Estado** Aprovada em 18/11/2026

## Cenários e aceitação

**P1 — Escolher uma direção visual.** A direção do jogo deve conseguir comparar o mar atual com duas propostas turquesa. A primeira destaca o deslocamento das ondulações na superfície. A segunda acrescenta volume às ondas. A comparação usa o mesmo trecho do píer e uma vista elevada da encosta, com enquadramento, iluminação e pixelização equivalentes.

**P2 — Avaliar o movimento junto à costa.** No píer, a espuma deve surgir em trechos, avançar e dissipar junto a pilares e rochas. Da encosta, as ondas abertas devem continuar legíveis enquanto os detalhes mais finos perdem contraste. A avaliação inclui câmera parada e em movimento para revelar cintilação, faixas repetidas e cortes na água distante.

## Requisitos

- **FR-001** — Comparar a referência atual e duas propostas animadas na cena `Ilha_Tripo_Caminhada_011`, com a câmera do jogo e a aparência low poly pixelada do projeto.
- **FR-002** — Mostrar o abrigo da enseada com movimento mais suave e o mar aberto com ondulações mais largas. Incluir a transição de cor entre costa e mar aberto e a resposta da espuma ao encontro com rochas e pilares.
- **FR-003** — Registrar o custo de cada proposta em relação à referência na mesma máquina, com o mesmo percurso e configuração visual. Apontar onde cada opção ajuda ou prejudica a leitura da costa e das interações no cais.
- **FR-004** — Entregar a comparação visual e uma recomendação para avaliação do usuário. Registrar eventuais limitações de visibilidade e desempenho.

## Critérios de sucesso

- **SC-001** — Cada uma das três versões aparece em dois enquadramentos comparáveis, em trechos animados de pelo menos 10 segundos por vista. Pelo menos um trecho por versão inclui movimento de câmera.
- **SC-002** — A comparação mostra o contato com um pilar e um trecho de costão. Na vista elevada, registra se o padrão cintila e se a borda da superfície fica visível.
- **SC-003** — O parecer registra o tempo mediano por quadro das três versões, as condições da medição e a opção proposta para a próxima iteração.

## Premissas

- O GDD define um litoral ensolarado, mar turquesa e píer em enseada abrigada. A regra que faz certos moradores voltarem ao mesmo píer é narrativa. A primeira prova compara o movimento da água e a leitura da costa.
- A avaliação usa a configuração atual de câmera e renderização. Nado, pilotagem, flutuabilidade e estados narrativos do mar ficam fora desta primeira iteração.
- A direção final depende da avaliação das capturas pelo usuário.
