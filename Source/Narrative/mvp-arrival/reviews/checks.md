# Verificações da entrega 009

Executadas em 13/11/2026, na branch `009-sistema-misterio`. Aplicação estática local; sem build ou alteração do projeto Unity.

## Pré-requisitos e contexto

- Skill `speckit-implement`: `check-prerequisites.sh --json --require-tasks --include-tasks` resolveu a pasta absoluta `specs/009-sistema-misterio` depois de corrigir o apontamento local da feature.
- Checklist de requisitos: 16 itens completos, zero pendências; arquivo preservado.
- `.specify/extensions.yml` ausente: nenhum hook anterior ou posterior configurado.
- Repositório conferido; branch própria criada a partir da `main` existente. Alterações da spec 010 foram preservadas.
- `.gitignore` já cobria os artefatos Unity e locais. Acrescentado apenas `.playwright-mcp/` para logs de QA; capturas entregues ficam em `Source/Narrative/`.

## Automação

Comandos executados com RTK; o conteúdo completo do resultado está em `automated-checks.txt`.

```sh
rtk proxy node --test Tools/MysteryWeb/tests/*.test.mjs
rtk proxy node Tools/MysteryWeb/scripts/validate.mjs --case Tools/MysteryWeb/data/case.json --all-losses 2
rtk proxy node Tools/MysteryWeb/scripts/export-dossier.mjs --case Tools/MysteryWeb/data/case.json --out Source/Narrative/mvp-arrival
```

Resultado: **19 testes, zero falhas**; sete combinações de perdas aprovadas, mais preparação de memória anterior à perda. A busca final levou aproximadamente **10 ms**, dentro da meta de cinco segundos. Os testes incluem ciclo sem semente, ramos incompatíveis, caso insolúvel, origem repetida, documento falso, ordem de descoberta, aquisição por fala e preservação após perda.

O dossiê, mapa e relatório foram reexportados após a revisão da autoria. Caderno + aviso não aparece mais como sustentação aceita. Conversa e bilhete de Ivo compartilham uma origem. Os títulos dos cenários do relatório correspondem às configurações autorais.

Verificação sintática final com `node --check`: **13 módulos válidos**, incluindo aplicação, views, testes, validador e scripts.

## Navegador

Servidor Python na porta 8009 respondeu HTTP 200. QA em navegador Chromium por Playwright, com cliques nos controles reais e navegação por teclado. Na navegação final: **zero erros de Console**; mapa e três imagens de retrato carregados.

- As três fichas exibem o interlocutor e seus assuntos. Artur e Teresa correspondem à folha; Ivo permanece provisório.
- O quadro inicial tem zero pistas. Falas futuras não concedem evidências: a primeira fala de Ivo manteve zero, a segunda registrou seu depoimento.
- Os dois percursos descritos em `order-review.md` adquiriram os mesmos quatro IDs e aceitaram a mesma proposta.
- Caderno + aviso foi rejeitado, sem revelar a combinação correta. Bilhete + aviso permitiu concluir mesmo com Artur e Ivo indisponíveis.
- Encerrar o assunto com Artur removeu perguntas diretas e a resposta à apresentação do caderno. Outras fontes continuaram disponíveis.
- Foram selecionados os oito cenários. As fontes removidas ficaram desabilitadas; os sete estados sem preparo começaram vazios. O cenário de memória começou apenas com `ev-ivo-selection`.
- Perda manual depois da aquisição preservou esse depoimento, fechou a conversa e bloqueou novas falas. O texto de abertura foi atualizado. Reiniciar voltou a zero pistas.
- Operação por Tab e Enter alcançou Ivo, iniciou e percorreu a conversa e devolveu o foco ao interlocutor. O foco visível mediu 3 px. Examinar e apresentar evidência também funcionaram por teclado.

## Capturas inspecionadas

| Resolução | Captura | Resultado |
| --- | --- | --- |
| 1366×768 | `desktop-1366-dialogue.png` | Mapa, conversa e duas evidências visíveis; sem rolagem horizontal |
| 1920×1080 | `desktop-1920-dialogue.png` | Mesmos painéis e área de hipótese legíveis |
| 390×844 | `mobile-390-dialogue.png` | Conversa e evidências juntas, após rolagem; sem modal, aba ou corte horizontal |

`desktop-initial.png` registra o mapa com nomes e recortes; `completed.png` mostra a proposta aceita. Rostos foram corrigidos e reinspecionados. Pontos não se sobrepõem; rótulos aparecem no foco ou hover. A lista preserva a rolagem ao selecionar evidências.

O servidor de QA foi encerrado ao final. Para avaliar a amostra, execute o comando de abertura em `Tools/MysteryWeb/README.md` e acesse `http://localhost:8009`.

## Limites e avaliação pendente

Esta prova cobre a pergunta da partida e disponibilidade de fontes. Não valida relógio, sorteios, campanha, vítimas, cura, festa ou integração Unity. A revisão narrativa interna está em `narrative-review.md`. A avaliação do usuário e a confirmação dos detalhes autorais propostos permanecem abertas em `user-review.md`; T030 não foi marcada como concluída.

Hash SHA-256 do `case.json` verificado: `fb97a35c298e3cdd4c316d4730c294699fa41b1a3b026d0cd864fb42e3a36650`.
