# Implementation Plan: Diálogos contextuais

**Branch**: `084-dialogos-contextuais` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification at `specs/082-dialogos-contextuais/spec.md`

## Summary

Ampliar o diálogo modal já usado pelos moradores, extraindo conteúdo e regras de contexto em dados textuais e serviços de domínio pequenos. Reutilizar interação, caderno, inventário, controles, câmera e relógio existentes. A slice será isolada em conteúdo `demo.*`; a cena principal permanece sem alteração enquanto tem mudança local do usuário, e a forma segura de demonstrá-la será definida após inspeção de cenas/harness no Editor.

## Technical Context

**Language/Version**: C# no Unity 6000.3.25f1.  
**Primary Dependencies**: Yarn Spinner for Unity 3.2.8 (Git URL fixada em `#v3.2.8`, MIT); localização nativa Yarn/CSV para não duplicar provedor; Input System 1.20.0, uGUI 2.0.0 e Test Framework 1.6.0 existentes.  
**Storage**: Não há SaveSystem detectado; implementar adaptador narrativo isolado/fixture com schema versionado, sem alegar integração a save de produção.  
**Testing**: Unity Test Framework; EditMode para domínio/CSV e PlayMode para interface/controles.  
**Target Platform**: Editor e plataforma de jogo já configurada.  
**Project Type**: Jogo Unity 3D Built-in.  
**Performance Goals**: Sem polling de conteúdo por frame; resolver elegibilidade ao iniciar/reabrir tópico.  
**Constraints**: Sem atualização do manifesto, sem segunda engine narrativa, não alterar lore oficial nem sobrescrever alterações existentes da cena principal.  
**Scale/Scope**: 3 NPCs demo, 5 pistas demo e 30–50 linhas incluindo opções; uma ferramenta de diagnóstico de autoria.

## Constitution Check

- Reutilização de sistemas e manutenção de fontes proprietárias de estado: PASS.
- Observação/depoimento/hipótese separados, lore nova apenas em Demo: PASS.
- C# runtime e cena/UI editados seguindo a skill Freyja Unity e Unity MCP quando conectado: PASS pendente de implementação.
- Dependência nova e alteração de pacotes fora do plano aprovado: PASS; nenhuma prevista sem evidência de compatibilidade/licença.
- Spec aprovada antes do plano; executar tarefas e validação sem substituir entrega por documentação: PASS.
- Integração da branch à `main` após validação, preservando mudanças locais: pendente; deve ser verificada e só realizada quando segura. O Spec Kit criou branch `084-dialogos-contextuais`, mas a pasta aprovada já existia como `specs/082-dialogos-contextuais`; `feature.json` e referências apontam à pasta 082 e `Feature Branch` registra 084.

## Project Structure

```text
Freyja/Assets/_Project/Scripts/Prototype/IslandDialogue.cs  # ponto de entrada UI/interação existente
Freyja/Assets/_Project/Scripts/Prototype/CaisSessionController.cs # modal e ciclo de controles
Freyja/Assets/_Project/Narrative/Runtime/                 # domínio/contexto/persistência narrativa mínimos
Freyja/Assets/_Project/Narrative/Content/Demo/            # conteúdo textual de demonstração
Freyja/Assets/_Project/Narrative/Localization/             # CSV e ferramenta de round-trip
Freyja/Assets/_Project/Narrative/Editor/                   # debugger/validador editor-only
Freyja/Assets/_Project/Narrative/Tests/EditMode/           # domínio e validação/CSV
Freyja/Assets/_Project/Narrative/Tests/PlayMode/           # integração UI/controles
specs/082-dialogos-contextuais/                            # artefatos de especificação aprovados
```

**Structure Decision**: Módulo interno dentro de `Assets/_Project/Narrative`, integrado pelo `IslandDialogue` existente. Não criar framework de DI, singleton global, second canvas ou segundo motor narrativo. Yarn é a fonte única de fluxo/condições por `.yarn`, integrando um adaptador C# mínimo de estado/efeitos. Usar CSV nativo de localização Yarn e uma operação de exportação que acrescente metadados de tradutor preservando round-trip. Não adicionar Unity Localization nesta slice.

## Risks & Mitigations

- A cena `Ilha_Principal.unity` está modificada localmente antes da tarefa; não salvar cenas nela sem inspeção de diff e validação MCP. Preferir fixture/cena isolada para demo, mas reusar os prefabs reais via setup editor se seguro.
- `IslandDialogue` mistura UI, interação e conteúdo; extrair somente conteúdo/estado necessários, mantendo o modal e os contratos atuais.
- Ausência de SaveSystem impede integração de produção; isolar adaptador narrativo e rotular limite, cobrindo round-trip real do módulo.
- Depender de Yarn/Localization exigiria pacote, teste de licença/API e mudança de manifesto; não presumir. A decisão final será registrada em research.
- Um Editor MCP pode estar ativo, mas existem dois projetos Unity com servidores. Confirmar explicitamente a instância `/Freyja` antes de qualquer operação.

## Phase 0 — Research

- Mapear sistema atual de diálogo e registros existentes; inspecionar branches/cenas e testes sem editar gameplay.
- Confirmar APIs Unity Editor/MCP disponível para projeto `/Freyja`, cena ativa, compilação e execução dos testes.
- Yarn Spinner 3.2.8 oficial suporta Unity 2022.3+ (portanto Unity 6000.3.25f1), possui storylets/node groups e saliency, export/import CSV de linhas e licença MIT; usar URL Git fixada `https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git#v3.2.8`. Revisar APIs da versão e compilar após instalação; não copiar sintaxe/API 2.x.
- Usar localização nativa do Yarn no CSV; enriquecer export/merge para colunas speaker/context/hash/status sem substituir source Yarn. Persistência via adaptador local serializável apenas para demo, pois não há SaveSystem.
- Registrar decisões, alternativas e limites em `research.md`.

## Phase 1 — Design

- Documentar entidades e contratos mínimos; produzir `data-model.md` e `contracts/` conforme necessário.
- Documentar passos executáveis de demo, testes, import/export e limitações em `quickstart.md`.
- Reavaliar gates de constituição com integração escolhida e desenho de cena aprovada.

## Validation Strategy

- EditMode: sem spoiler, comunicação individual/idempotente, duas ordens de pistas, determinismo, Once/Repeatable/Cooldown, cancelamento e round-trip de save.
- Conteúdo/localização: IDs únicos, placeholders, hashes, troca `pt-BR`/`en-US`, round-trip CSV com acentos/aspas/multiline e detecção de stale/orphan/missing.
- PlayMode no Unity: iniciar/avançar/escolher/cancelar, mudança contextual após comunicar, persistência, idiomas e restauração de input; Console sem erros novos.
- Capturas reais pelo Game View para conversa inicial, escolha, reação contextual e `en-US`; registrar cenas, passos, comandos e caminhos de resultados.
- Revisão independente: Game Designer já revisou escopo; executar revisão posterior do Implementador na integração e do Validador após implementação. Corrigir e reexecutar verificações afetadas.
