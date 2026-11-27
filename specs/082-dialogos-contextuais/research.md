# Pesquisa — Diálogos contextuais

**Estado**: concluída para o planejamento em 2026-11-26.

## Arquitetura observada

- `IslandDialogue` em `Freyja/Assets/_Project/Scripts/Prototype/IslandDialogue.cs` mantém residentes, assuntos, texto PT hardcoded, notebook de depoimentos/observações, disponibilidade e UI modal.
- `CaisSessionController` controla bloqueio de gameplay/câmera e já expõe estado de modal; `ConversationFraming` trata enquadramento.
- `IslandInventory`/`InventoryPickup` implementam coleta em memória; não foi identificado save global.
- `CoastalDayNight` expõe `Hour` e `SetHour`; candidato para política temporal usando relógio injetado/testável.
- Cena principal já possui diff local da tarefa de iluminação; nenhuma edição nela deve ser salva sem checagem precisa pelo Editor.
- `061-sistema-dialogos` aprovou a pergunta de Ivo quando ambos os depoimentos estão registrados, em qualquer ordem; observação da tira azul e hipótese do jogador são categorias distintas.

## Dependências

- Unity 6000.3.25f1; Built-in; Input System 1.20.0; uGUI 2.0.0; Test Framework 1.6.0 no lock.
- Busca no manifesto, lock e assets não encontrou Yarn Spinner nem Unity Localization.
- Decisão: Yarn Spinner for Unity `v3.2.8` por Git tag fixada. A documentação oficial afirma compatibilidade da linha 3 com Unity 2022.3+; release `v3.2.8` ensina instalar por URL `...git#v3.2.8`; licença upstream `LICENSE.md` é MIT. Unity 6000.3.25f1 satisfaz o mínimo. A API real ainda precisa compilar no projeto depois da instalação.
- Yarn 3 oferece node groups e condições `when:` e expõe política de saliency customizável; usar o runtime para fluxo/storylets, e domínio C# somente para estado/propriedade do jogo e efeitos autorizados. Não duplicar as condições no C#.
- Yarn oferece export/import nativo de CSV por IDs de linha e suporta provedor de localização próprio ou Unity Localization. Usar o built-in CSV/provider do Yarn nesta slice, sem segundo pacote; ferramenta editor acrescenta `speaker_id`, `context`, `source_hash` e `translation_status` em merge compatível. Verificar preservation/idempotência por round-trip automático.
- Fonte PT-BR em `.yarn`; idioma teste `en-US` no strings CSV. Fonte de texto Yarn, tradução CSV; save/seleção independem de locale.
- Fontes oficiais consultadas: [instalação Yarn Unity](https://docs.yarnspinner.dev/yarn-spinner-for-unity/installation-and-setup), [releases Yarn Unity](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/releases), [licença MIT Yarn Unity](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/main/LICENSE.md), [node groups](https://docs.yarnspinner.dev/write-yarn-scripts/advanced-scripting/node-groups), [saliency 3.1](https://docs.yarnspinner.dev/3.1/write-yarn-scripts/advanced-scripting/saliency), [localização/CSV](https://docs.yarnspinner.dev/yarn-spinner-for-unity/assets-and-localization).

## Incógnitas

- Unity CLI 1.0.1 identifica instância main `/Freyja`, pronta na porta 7800, Unity 6000.3.25f1. Status/Console/Hiearchy foram consultados com `--project-path`; cena ativa é `Ilha_Principal`, `isDirty=false`, possui `IslandDialogue`, `CaisSessionController` e `CoastalDayNight`. A hierarquia extensa não foi retida. Console no snapshot: `compilationFailed=false`, `consoleErrors=0`, 2 warnings; counts históricos retornados pelo Console não representam erros correntes. MCP direto é ambíguo com dois projetos, CLI com caminho explícito funciona. CLI self-update reporta 1.1.0 disponível; não atualizada nesta tarefa.
- Diff local na cena principal existe no working tree, embora Editor informe `isDirty=false`; não executar setup/save nela. Confirmar o diff imediatamente antes de qualquer integração. A demonstração visual pode ser executada em uma cópia temporária por script/fixture em PlayMode sem persistir a cena principal; se nova cena for necessária, criar demo isolada dentro de `Assets/_Project/Scenes/` e não substituir principal.
- Não há assembly definitions para scripts Prototype encontrados; testes terão que referenciar assembly padrão do projeto ou adotar asmdefs de forma compatível, a decidir após inspecionar compilação gerada pelo Unity.

## Verificação de implementação em 2026-11-26

- Yarn Spinner Unity `v3.2.8` foi fixado no manifesto/lock com licença MIT. Unity CLI compilou e executou testes na cópia temporária `/tmp/freyja-validation/Freyja`, sem salvar cenas do projeto principal.
- O compilador Yarn carregou `Demo.yarnproject`, `pt-BR` como fonte, e compilou o único source `Demo.yarn` com `errors=False`. PlayMode provou o comando de descoberta tipada, comunicação contextual entre duas pistas e leitura `en-US` mantendo o mesmo line ID.
- `IslandDialogue` mantém o conteúdo legado como padrão; F2 habilita o Demo Yarn e E inicia o node do NPC pelo mesmo modal, framing e controle de sessão. Não houve teste visual do fluxo real no host, que estava bloqueado na tentativa de inspeção; o teste PlayMode atual cobre runner/presenter/bridge isolados.
- A ferramenta CSV produz master consolidado de 10 colunas e CSV nativo Yarn de 3 colunas por locale; testes verificam escape RFC4180, merge stale/missing/orphan, placeholder incompatível e duplicata. O menu de export/import ainda não foi executado pelo Editor conectado.
