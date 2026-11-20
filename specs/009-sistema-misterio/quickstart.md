# Guia de validação — MVP web de pistas

**Situação atual:** aplicação implementada e comandos executados em 13/11/2026. Dezenove testes aprovados, sete configurações de perdas e memória anterior validadas, capturas nas três resoluções previstas. Evidências em `Source/Narrative/mvp-arrival/reviews/checks.md`. A avaliação do usuário permanece pendente.

## Pré-requisitos

Na raiz do projeto, Node.js 22 e Python 3 disponíveis. Foram observadas as versões locais Node 22.12.0 e Python 3.14.7. Não é necessário instalar pacotes nem abrir Unity ou Blender para esta amostra.

Antes da primeira verificação no navegador, seguir a referência local da skill Sites já selecionada: `/Users/bruno.duartec/.codex/plugins/cache/openai-curated-remote/sites/1.1.6/skills/sites/references/preview/local.md`.

## Comandos locais

```sh
node --test Tools/MysteryWeb/tests/*.test.mjs
node Tools/MysteryWeb/scripts/validate.mjs --case Tools/MysteryWeb/data/case.json --all-losses 2
node Tools/MysteryWeb/scripts/export-dossier.mjs --case Tools/MysteryWeb/data/case.json --out Source/Narrative/mvp-arrival
python3 -m http.server 8009 --directory Tools/MysteryWeb
```

Abrir `http://localhost:8009`. O comando de servidor fica ativo enquanto a revisão usa o navegador. Uma porta ocupada deve ser substituída por outra livre e o endereço efetivo registrado na entrega.

## Resultados esperados

1. Testes de regras passam e o caso defeituoso de controle é rejeitado.
2. Validador cobre sete configurações de zero a duas perdas entre três testemunhas, com percurso de ações em todas e sem contar boatos repetidos como confirmações independentes.
3. Exportações geram `dossier.md`, `clue-map.mmd` e `validation-report.md`, com referências aos mesmos IDs do caso.
4. Ao abrir a página, clicar nos três rostos e num ponto examinável revela as interações corretas, mantendo a lista de pistas visível.
5. A conclusão exige proposta explícita e evidências; reordenar conversa e exame preserva os fatos.
6. Em revisão de perdas, novas conversas com o morto são bloqueadas e relatos anteriores são preservados.
7. Reiniciar limpa a sessão; a página não conserva pistas de outra rodada.

## QA no navegador e revisão narrativa

Conferir 1366×768, 1920×1080 e 390×844; clique, teclado, foco, rolagem, rostos e ausência de erros de carregamento ou Console. Capturas documentam o painel de pistas durante um diálogo e a amostra concluída.

Na revisão com o autor, registrar: qual acontecimento motivou investigar, que pergunta o jogador quis fazer, quais duas fontes sustentaram a conclusão e onde uma fala ou pista causou confusão. Avaliação pendente não deve ser marcada como aprovada.

Essas verificações não validam relógio real, escolhas de vítimas da campanha, festa ou integração Unity. Interromper a expansão ao entregar a amostra para avaliação.
