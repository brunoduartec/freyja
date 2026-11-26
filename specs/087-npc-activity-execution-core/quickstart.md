# Quickstart — núcleo de execução

Pré-requisitos: checkout do Freyja, .NET SDK instalado e fontes C# compartilhadas. Unity não é necessária.

Na raiz do repositório:

```sh
dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj
dotnet run --project Tools/NpcScheduleSimulator -- --test
dotnet run --project Tools/NpcScheduleSimulator -- --validate
dotnet run --project Tools/NpcScheduleSimulator -- --simulate-week
```

Resultados esperados: build sem erros; testes do Schedule Core e do executor passam; loader valida catálogo, agendas e receitas; simulação conclui sete dias para três NPCs fictícios e mostra eventos determinísticos de execução, disputa, reavaliação, cancelamento e falha. Uma nova execução do comando deve produzir a mesma trilha.

Casos direcionados adicionais: receitas inválidas apontam arquivo/atividade/passo; executar `sit` com os slots concorridos mantém exclusividade; caminho ou animação injetados como falha deixam cancelamento/erro observável; cancelamento depois de reservar tenta restaurar pose e libera token antes da próxima atividade. Essas são evidências do fake, não de NavMesh, Smart Objects ou animação física.

Consulte [contrato](contracts/execution-contract.md), [modelo de dados](data-model.md) e [spec](spec.md) para invariantes e semântica completa.
