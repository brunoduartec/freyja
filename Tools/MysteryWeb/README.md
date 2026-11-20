# Freyja — A partida de Ivo

Amostra web local da spec `009-sistema-misterio`: três personagens, oito evidências, falas sequenciais, apresentação de pistas e proposta explícita de explicação. O conteúdo autoral está em `data/case.json`; aplicação, testes, busca de caminhos e exportações usam as mesmas regras. Sem instalação de pacotes, framework ou backend.

## Abrir

Na raiz do repositório, com Python 3:

```sh
python3 -m http.server 8009 --directory Tools/MysteryWeb
```

Abra <http://localhost:8009>. O servidor usado no QA foi encerrado após a revisão; o comando acima inicia uma nova sessão. Não abra `index.html` diretamente por `file://`, pois o navegador precisa carregar o JSON por HTTP.

Clique num rosto, escolha um assunto e avance as falas. Examine os pontos do mapa; seus nomes aparecem ao passar o mouse ou focar por teclado. As evidências conhecidas permanecem disponíveis durante a conversa. Selecione uma explicação e marque as fontes que a sustentam para propor a conclusão. A coleta sozinha não encerra a amostra.

A barra de revisão troca cenários ou simula a perda de uma fonte. Isso bloqueia falas novas e preserva relatos já registrados. Reiniciar limpa o estado da rodada e reaplica somente a preparação do cenário escolhido. Não há salvamento entre sessões.

## Verificar e exportar

Com Node.js 22, na raiz do repositório:

```sh
node --test Tools/MysteryWeb/tests/*.test.mjs
node Tools/MysteryWeb/scripts/validate.mjs --case Tools/MysteryWeb/data/case.json --all-losses 2
node Tools/MysteryWeb/scripts/export-dossier.mjs --case Tools/MysteryWeb/data/case.json --out Source/Narrative/mvp-arrival
```

O validador gera `Source/Narrative/mvp-arrival/validation-report.md` e encerra com erro se faltar caminho em qualquer cenário entregue. O exportador gera `dossier.md` e `clue-map.mmd` a partir do JSON; não edite essas cópias à mão.

## Assets e autoria

`assets/map.png` e `assets/characters-sheet.png` são cópias das duas referências existentes. Caminhos originais, dimensões, hashes e retângulos estão em `assets/manifest.json`. Os círculos são máscaras CSS do atlas; nenhum raster novo foi criado. Artur e Teresa seguem as associações do GDD. O adulto da extrema direita representa Ivo **provisoriamente**.

O pedido de Luana na chegada, a inscrição «a pedido», o aviso, o bilhete e as falas específicas são autoria proposta para este piloto. A revisão interna está em `Source/Narrative/mvp-arrival/reviews/narrative-review.md`; a avaliação do usuário permanece pendente. Observações registram o que foi visto ou escrito, sem transformar afirmações em verdade automática. Bilhete e fala de Ivo compartilham uma origem informacional.

## Evidências da entrega

- 19 testes automatizados aprovados; sete configurações de perdas e memória anterior verificadas.
- Dois percursos no navegador produziram as mesmas quatro evidências e a mesma conclusão.
- QA com mouse, teclado, reinício e perda de fontes, sem erros de Console na versão final.
- Capturas a 1366×768, 1920×1080 e 390×844 em `Source/Narrative/mvp-arrival/reviews/`.
- Comandos, resultados e limites: `reviews/checks.md` e `reviews/automated-checks.txt`.

Esta amostra valida somente a primeira investigação e a disponibilidade das fontes. Relógio, campanha com 30 moradores, possessões, vítimas, cura, festa e integração Unity ficam fora desta entrega. Os assets e pacotes de produção do Unity não foram alterados. A tarefa T030 aguarda avaliação da amostra pelo usuário.
