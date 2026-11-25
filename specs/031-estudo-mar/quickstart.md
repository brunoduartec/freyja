# Avaliar o mar na cena 011

Abra o projeto Unity `Freyja/` da branch 031 e carregue `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. Em Play Mode, use **1** para a referência, **2** para A e **3** para B.

No píer, observe se a espuma se fragmenta junto às estacas e se o contraste permite ler a cena. Na praia, acompanhe uma faixa de espuma até a margem e observe como ela se dissipa. Na encosta, compare o recorte distante e o movimento durante uma rotação curta de câmera.

Os materiais ficam em `Assets/_Project/Art/SeaStudy031/`. A máscara usa a cota do mar, o contato com o terreno, as rochas e a malha do píer, além da camada de terreno `BeachSand`. Para refazê-la após modificar a costa, com a cena salva e fora do Play Mode execute na raiz da worktree:

```sh
unity command script run --project-path "$PWD/Freyja" \
  --file ../specs/031-estudo-mar/SeaStudyAuthoring.cs --entry SeaStudyAuthoring.Run
```

As [capturas e medições](resultado.md) foram produzidas com `SeaStudyEvidence.cs` em Play Mode.
