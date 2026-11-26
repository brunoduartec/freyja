# Plano de implementação: NPC Schedule Core

## Contexto técnico

- Projeto: Unity 6000.3.25f1, Built-in; a feature não depende do Editor.
- Verificação disponível: .NET SDK 11.0.100 (macOS arm64).
- Estratégia: domínio em C# conservador e sem Unity, compilado diretamente pelo harness .NET; JSON desserializado por parser próprio pequeno para manter a mesma fonte C# compatível com Unity e rejeitar propriedades desconhecidas sem adicionar pacote.
- Testes: suíte executável sem framework externo, via comando `dotnet run -- --test`.
- Integração: nenhuma nesta entrega. Smart Objects, relógio de gameplay e narrativa permanecem fora da implementação.

## Decisões

1. C# do domínio fica em `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/`, e o projeto CLI inclui esses mesmos arquivos por `Compile Include`; não há cópia do domínio.
2. Dados de demonstração e schema ficam em `Freyja/Assets/_Project/NpcScheduling/Data/`; o harness e seus arquivos de projeto ficam em `Tools/NpcScheduleSimulator/`, fora da importação da Unity.
3. Parser JSON e validação semântica ficam na borda do harness; resolução consome definições normalizadas e snapshots explícitos, sem I/O ou estado global.
4. Calendário de fixture declara o dia da semana inicial. Dias 1–7 são dias absolutos sintéticos do simulador; isso não altera a duração canônica da campanha (três dias).
5. Para não atribuir lore, encontro, moradores ou objetos físicos ao jogo, todos os IDs de demonstração recebem prefixo/descrição demo e os destinos são simbólicos. O evento do dia 4 é sintético e não significa que há campanha canônica nesse dia.
6. Pastas/arquivos novos sob `Assets` não recebem GUIDs fabricados; Unity gera `.meta` ao importar. Essa importação e compilação ficam pendentes para a volta do Mac.

## Portas da Constituição

- Separação intenção/execução: atendida por contratos sem Unity e sem efeitos colaterais.
- Evidência/lore: exemplos marcados como ficção técnica; logs são horários planejados, jamais testemunhos.
- Dependências: nenhuma dependência de runtime ou pacote novo.
- Validação: testes e CLI .NET independentes. Unity/Editor/Play Mode expressamente fora do ambiente atual e ficam pendentes.

## Fases

1. Pesquisa e contratos: confirmar parser, calendário, fronteiras e entidades; produzir modelo e quickstart.
2. Base: parser JSON estrito, validação agregada, catálogo e leitura dos arquivos.
3. Resolução: expansão de templates/overrides, snapshots, prioridades, trace e validade.
4. Demonstração: cinco profissões/NPCs fictícios, exceções de clima, semana e narrativa.
5. CLI e qualidade: consulta, validação, simulação de sete dias e testes executáveis.

## Verificação

Executar `dotnet run --project Tools/NpcScheduleSimulator -- --validate`, `--test` e `--simulate-week`; revisar dependências proibidas, saídas determinísticas, cobertura dos casos do anexo e estado do Git. Não executar Unity, MCP, Blender ou alterar cenas/prefabs.
