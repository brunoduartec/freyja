# Diálogos contextuais — execução e validação

## Fluxo atual

Com `Ilha_Principal` aberta em Play Mode, capture o controle como no protótipo e use **F2** para ativar o modo de demonstração Yarn. Aproxime-se de Nuno, Luana ou Ivo e pressione **E**. Yarn apresenta linhas e opções dentro do modal inferior já usado por `IslandDialogue`; **Enter** avança/confirma, **↑/↓** selecionam, **L** alterna `pt-BR`/`en-US` e **Esc** cancela. `CaisSessionController` mantém os controles suspensos durante o modal e `ConversationFraming` enquadra o NPC. O conteúdo aprovado existente permanece como modo padrão. O arquivo `demo-narrative-progress.json` é progresso de demonstração isolado, não o SaveSystem de produção.

O projeto de conteúdo fica em `Freyja/Assets/_Project/Narrative/Content/Demo/`. `Demo.yarn` tem três NPCs, cinco pistas e 38 IDs permanentes para linhas e opções. O estado separa descoberta pelo jogador (categoria e fonte), conhecimento comunicado por NPC, efeitos idempotentes, checkpoints e hora de jogo. A pergunta contextual de Ivo depende das duas comunicações. Yarn Spinner Unity está fixado na tag MIT `v3.2.8` pelo manifesto/lock do projeto; não há Unity Localization.

## Testes e evidência

Os testes foram executados em uma cópia temporária do projeto (`/tmp/freyja-validation/Freyja`), Unity 6000.3.25f1, porque a instância aberta do projeto principal devolveu estado de recompilação `triggered` e uma lista PlayMode incompleta. Resultados preservados fora do repositório:

- `/tmp/freyja-validation/editmode-final.xml`: 7/7 EditMode; domínio, round-trip CSV, merge de traduções e rejeição de IDs duplicados.
- `/tmp/freyja-validation/playmode-final.xml`: 3/3 PlayMode passaram (conhecimento contextual, descoberta tipada após confirmação e locale), 0 falhas.
- O compilador Yarn 3.2.8 carregou `Demo.yarnproject`, localizou `Demo.yarn` e retornou `errors=False`.

Para repetir com o Unity CLI em um projeto executável:

```sh
unity test Freyja --mode EditMode --filter NarrativeProgressTests --output Artifacts/narrative-editmode.xml
unity test Freyja --mode PlayMode --filter NarrativeYarnFlowTests --output Artifacts/narrative-playmode.xml
```

No Editor aberto, a CLI de pipeline oferece testes assíncronos: `unity command test run --project-path "$PWD/Freyja" --mode editor --async-tests true`, seguida de `unity command test status --project-path "$PWD/Freyja"`. Os testes de interface devem rodar em Game View visível. A fixture PlayMode cria uma cena aditiva temporária em memória; não há cena demo persistente.

## Tooling de CSV e limites

`Freyja/Narrative/Export or merge translation CSV` mantém `Localization/strings.csv` consolidado e exporta os arquivos nativos `pt-BR.csv`/`en-US.csv` do Yarn. O merge mantém traduções existentes, denuncia IDs duplicados, marca estados `missing`, `stale`, `invalid_placeholders` e `orphan`, e escapa CSV RFC4180. `Freyja/Narrative/Validate demo CSV` checa IDs, colunas e placeholders. O menu e a atualização por AssetDatabase ainda precisam ser exercitados no Editor principal.

`Freyja/Narrative/Debugger (read-only)` mostra snapshot do progresso persistido, conhecimento do NPC escolhido, elegibilidade simplificada da pergunta de Ivo, histórico e efeitos. Ele não consulta candidates internos do Yarn nem revela motivos para cada opção; não representa debugger geral de storylets.

A validação PlayMode atual exercita `DialogueRunner`/bridge/presenter isolados, não o percurso físico real por E, `IslandDialogue.OnGUI`, cursor e restauração de controles. A conexão e o desenho no modal existente estão implementados no código, mas captura Game View e aceite visual continuam pendentes. A cena principal tem alterações locais preexistentes e não foi salva nem alterada por esta entrega.
