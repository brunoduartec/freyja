# Plano — 002 · Caminhar pelo cais

**Data:** 12/11/2026 · **Spec:** [spec.md](spec.md)
**Identificador:** 002-caminhada-no-cais. O setup retornou esse identificador como BRANCH, mas a pasta não tem repositório Git; nenhuma branch foi criada.
**Estado:** implementado e validado tecnicamente; aguardando avaliação do usuário.

## Resumo

Criar uma cena de teste separada a partir do cais aprovado. Acrescentar figura provisória, caminhada, câmera em terceira pessoa, recuperação de queda e instruções de controle. Reutilizar o cenário; não adicionar sistemas narrativos nem ampliar a ilha.

## Contexto técnico

| Item | Decisão |
| --- | --- |
| Ambiente | Unity 6000.3.25f1, Built-in, raiz Freyja/; validação em Play Mode no macOS |
| Linguagem | C# compatível com o editor existente |
| Dependências | Input System 1.20.0, uGUI 2.0.0, Test Framework 1.6.0; nenhuma nova dependência |
| Produção visual | Blender 5.2.2 LTS via MCP para figura provisória de escala humana; sem animação final |
| Estado | Somente memória por execução; sem save/load |
| Verificação | Play Mode para física/input e percurso manual; automação de regras e integração opcional |
| Escala | Uma cena, um jogador, uma câmera de gameplay e o trecho de cenário já existente |
| Desempenho | Referência de 60 fps a medir no editor; não é requisito de lançamento nem resultado já obtido |
| Limites | Sem salto, corrida, natação, diálogos, caderno ou novos cenários |

## Constituição: verificação antes e depois do desenho

| Princípio | Resultado |
| --- | --- |
| Decisões no GDD e entrega verificável | Atendido: recorte da spec preservado; terceira pessoa permanece proposta de protótipo |
| Unity/Blender via MCP | Atendido: implementar e validar nos editores; figura provisória produzida no Blender |
| Direção visual e legibilidade | Atendido: conservar cenário e avaliar silhueta/escala na câmera; não antecipar acabamento PS1 |
| Investigação e cânone | Atendido: sem alteração narrativa ou sistemas de pistas nesta entrega |
| Evidências recuperáveis | Atendido: nova cena, prefab e roteiro de validação com resultados registrados |

**Gates:** aprovados na entrada e após o desenho; sem exceções. A pesquisa resolveu as decisões técnicas necessárias. Nenhuma mudança de versão, pipeline ou escopo é necessária.

## Base existente e organização

A cena Cais_Iteracao02 e o prefab Cais_VisualReview já existem. A [validação anterior](../../Source/Unity/Cais_Iteracao02.validation.json) cobre render, materiais e dois raycasts, não caminhada. A revisão confirmou colliders de piso, mas não de todos os obstáculos.

Destinos planejados em Freyja/Assets/_Project/:
- Scenes/Cais_Caminhada.unity: cena nova, preservando a revisão anterior.
- Prefabs/PlayerPrototype.prefab: representação provisória, controlador e referências.
- Scripts/Prototype/: CaisPlayerController, CaisCameraRig e CaisSessionController.
- Tests/EditMode/ e Tests/PlayMode/: verificações delimitadas desta feature.
- Art/Characters/Prototype/: export da fonte Source/Blender/Player_Provisorio.blend.

Usar referências às ações existentes Player/Move, Player/Look, UI/Cancel e UI/Click. Habilitar somente as ações necessárias, com ciclo de vida próprio, sem editar o asset original ou ativar seus comandos de salto/corrida. Instruções em um Canvas uGUI simples.

## Desenho técnico

1. **Cena:** copiar para novo caminho pelo editor via MCP. Manter iluminação/materiais; substituir a câmera de revisão por um único rig ativo e um AudioListener. Ajustes de colisão ficam na nova cena, sem aplicar alterações ao prefab-base.
2. **Jogador:** figura estática de cerca de 1,8 m, sem identidade final. CharacterController separado da malha; movimento no plano da câmera com magnitude limitada a 1, gravidade explícita e rotação na direção da caminhada. Valores iniciais ajustáveis: velocidade 3 m/s, raio 0,3 m, step offset 0,25 m e skin width 0,04 m. Ajustar pela passagem real, sem aumentar obstáculos vencíveis artificialmente.
3. **Câmera:** órbita em perspectiva, atualização após o jogador, sensibilidade ajustável e pitch limitado. Delta do mouse é deslocamento: não multiplicar novamente por deltaTime. SphereCast e verificação do volume do near clip contra cenário/obstáculos; excluir jogador, mar e triggers. Recolher rapidamente na obstrução e suavizar o retorno; não impor distância mínima que empurre a câmera através da parede. Verificar também pivô inicialmente encostado ou dentro de obstáculo.
4. **Colisões:** conferir tábuas, junções e degraus; adicionar proxies simples a caixas, pedras, costão e barco onde jogador/câmera podem alcançá-los. Não criar um piso invisível sobre o mar. Usar masks/layers explícitos, preservando configurações existentes.
5. **Sessão e foco:** iniciar em estado de cursor livre, com instrução para clicar. Clique captura; Esc, perda de foco ou perda do lock interrompem comandos. Voltar ao jogo não recaptura automaticamente: exigir novo clique e descartar o delta da captura.
6. **Recuperação:** SpawnPoint no centro do cais, com chão e espaço livre validados. Candidato inicial em coordenadas Unity: pés próximos de (0; 0,94; 7,5), voltado para o caminho; confirmar no editor. Queda abaixo de -0,5 m ou saída dos limites configurados retorna ao spawn, limpando velocidades e suavização da câmera. Reiniciar Play Mode recria o estado, inclusive sem Domain Reload.

Parâmetros são iniciais de implementação, não alterações da spec. Modelo de estados e contrato de controle estão nos documentos vinculados abaixo.

## Verificação e cobertura

| Requisitos | Evidência esperada |
| --- | --- |
| FR-001 e FR-002 | Cena anterior preservada; nova cena com figura de escala e um jogador |
| FR-003 | Movimento axial e diagonal com mesma velocidade máxima; orientação relativa à câmera |
| FR-004 | Órbita, recolhimento e retorno em obstáculos sem atravessar superfícies |
| FR-005 | Duas idas e voltas completas, junções/degraus e obstáculos sólidos |
| FR-006 | Queda, saída de limites e reinício devolvem ao spawn seguro |
| FR-007 | Controles visíveis, Esc, perda/retorno de foco e clique para retomar |

Validação funcional em Play Mode e teste manual de percurso, câmera e foco são obrigatórios. Automação Edit Mode para normalização/estados e Play Mode com input sintético permanece opcional: a spec não solicitou suíte automatizada ou TDD. Registrar resultados em evidence/ desta feature, com cena, condições e capturas; não declarar testes futuros como executados.

## Próxima etapa

As tarefas de [tasks.md](tasks.md) foram executadas após autorização do usuário, com frentes paralelas de movimento, câmera e figura provisória. Ver [evidências](evidence/validacao-us1.md). A próxima etapa é avaliação do usuário; não ampliar o escopo. Build e plataforma de distribuição não fazem parte do recorte.

Referências: [pesquisa](research.md), [dados e estados](data-model.md), [contrato de controles](contracts/player-controls.md), [guia de validação](quickstart.md).
