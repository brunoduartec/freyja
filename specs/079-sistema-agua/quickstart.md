# Quickstart — água 079

Execute a partir da raiz do worktree `079-sistema-agua`. O teste usa Unity `6000.3.25f1` e não abre o workspace principal:

```sh
UNITY_PROJECT="$(pwd)/Freyja"
unity open "$UNITY_PROJECT" --until-ready
unity status --project-path "$UNITY_PROJECT" --format json
unity command scene open --path Assets/_Project/Scenes/Ilha_Principal --project-path "$UNITY_PROJECT"
unity command scene list_open --project-path "$UNITY_PROJECT"
unity command editor.playmode start --project-path "$UNITY_PROJECT"
unity command capture game_view --source screen --width 960 --height 540 \
  --output-path "$PWD/Artifacts/water-validation/079-main-play.png" \
  --project-path "$UNITY_PROJECT"
unity command console status --project-path "$UNITY_PROJECT"
unity command console get --level error --project-path "$UNITY_PROJECT"
unity command editor.playmode stop --project-path "$UNITY_PROJECT"
```

Use `gameobject find --name Mar` e `component get_properties` para confirmar que `/Island023/Mar` usa `Water079/IslandWaterMain.mat`. A câmera `Cais Gameplay Camera` deve manter `Mundo_480x270.renderTexture` (480×270). O material deve usar `Freyja/Island Water 079`.

Capturas de referência incluem `Artifacts/water-validation/079-main-play.png`, `079-main-wind-play.png`, `079-main-wind-strong.png` e os quadros de fase longa `079-wind-long-breeze.png`, `079-wind-long-strong.png` e `079-wind-long-breeze-return.png`. O teste confirmou água visível na câmera real e espuma no píer/casco. Para testar presets, `_PreviewTime` foi fixado em 10.000 s na instância runtime do material; o shader mantém direção/fase fixas e modula amplitude com o vento. `079-coast-00.png` a `079-coast-04.png` registram a inspeção da costa; os ciclos não ficaram legíveis na câmera principal. O Console isolado registrou exceções internas do Unity Search; detalhes e todos os resultados dos critérios estão em `docs/water/validacao.md`. A prova ainda falha nos gates de ondas, leitura do fundo e contato do casco; a cena principal é uma integração provisória, não uma aprovação de AC-01–14. Não declarar performance, ciclos costeiros completos, quatro horários, níveis de qualidade ou interações sem executar esses testes.
