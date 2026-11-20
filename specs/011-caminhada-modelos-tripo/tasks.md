# Tarefas — Caminhar no cenário do TripoAI

**Branch** `011-caminhada-modelos-tripo`  
**Referências** [Spec](spec.md) · [Plano](plan.md) · [Pesquisa](research.md) · [Estados da cena](data-model.md) · [Controles](contracts/player-controls.md) · [Validação](quickstart.md)

## Fase 1 — Preparação

- [x] T001 Confirmar a branch 011, abrir o projeto `Freyja/` no Unity 6000.3.25f1, conferir o pacote local apontado por `Freyja/Packages/manifest.json` e testar os MCPs do Unity e do Blender conforme `Tools/MCP/README.md`. Conferir o projeto, a cena e o arquivo Blender ativos antes de editar. Registrar conexões, versões e limitações em `specs/011-caminhada-modelos-tripo/evidence/ambiente.md`. [Evidência](evidence/ambiente.md).

## Fase 2 — Fontes editáveis

- [x] T002 Importar `TripoAi/cenario/stylized island terrain 3d model.glb` pelo Blender MCP, preservar a malha visual, proporções, UVs e textura de cor, conferir os eixos e salvar `Source/Blender/Ilha_Tripo_011/Cenario.blend`. Exportar `Freyja/Assets/_Project/Art/Ilha_Tripo_011/Cenario.fbx` e a textura para a mesma pasta. (FR-001) [Evidência](evidence/assets.md).
- [x] T003 Importar `TripoAi/principal/stylized hiker 3d model.glb` pelo Blender MCP e verificar jaqueta azul, mochila, deformação, escala, frente e pivô nos pés. Usar a alternativa FBX descrita em `specs/011-caminhada-modelos-tripo/research.md` se a primeira opção falhar. Salvar `Source/Blender/Ilha_Tripo_011/Protagonista.blend` e registrar a fonte escolhida em `specs/011-caminhada-modelos-tripo/evidence/assets.md`. (FR-001) [Evidência](evidence/assets.md).

## Fase 3 — US1 · Controlar o protagonista na ilha (P1)

**Teste independente.** Abrir a nova cena em Play Mode, caminhar do início ao marco e voltar. Durante o percurso, girar a câmera, mudar de direção e parar. Testar uma obstrução e uma queda, observando o retorno ao início.

- [x] T004 [P] [US1] Criar e conferir os ciclos `Idle` e `Walk` no rig, com passada no lugar e sem translação da raiz, em `Source/Blender/Ilha_Tripo_011/Protagonista.blend`. Exportar os clipes e o rig em `Freyja/Assets/_Project/Art/Ilha_Tripo_011/Protagonista.fbx` e salvar as texturas correspondentes na mesma pasta. (FR-001, FR-004) [Evidência](evidence/validacao.md).
- [x] T005 [P] [US1] Acrescentar em `Freyja/Assets/_Project/Scripts/Prototype/CaisPlayerController.cs` a medição da velocidade planar efetiva em solo após `CharacterController.Move`, preservando o contrato de `PlanarVelocity`. Zerar a medição na recuperação e durante a queda. (FR-004) [Evidência](evidence/validacao.md).
- [x] T006 [US1] Criar `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` pelo Unity. Conferir as importações e materiais Built-in e definir a escala uniforme do terreno, um início em solo firme com folga para a cápsula, um marco visível, uma curva e uma saída para testar queda. Registrar a escala e o trajeto em `specs/011-caminhada-modelos-tripo/evidence/assets.md`. (FR-001, FR-002) [Evidência](evidence/assets.md).
- [x] T007 [US1] Preparar a malha de colisão simplificada do trecho e das encostas em `Source/Blender/Ilha_Tripo_011/Cenario.blend` e exportar `Freyja/Assets/_Project/Art/Ilha_Tripo_011/Cenario_Colisao.fbx`. Na cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`, alinhar os colisores estáticos ao relevo e conferir continuidade, declive e saída para queda. (FR-002, FR-005) [Evidência](evidence/terreno-play-mode.txt).
- [x] T008 [US1] Configurar o jogador, a cápsula e a câmera em `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` usando os componentes do cais e `Freyja/Assets/InputSystem_Actions.inputactions`. Ligar WASD, mouse, Esc e retomada por clique, exibir os comandos e ajustar máscaras, altura de queda, limites e reposicionamento. Se necessário, tornar o título do HUD configurável em `Freyja/Assets/_Project/Scripts/Prototype/CaisSessionController.cs` com o título atual como padrão. (FR-003, FR-005) [Evidência](evidence/controles-e-colisoes.txt).
- [x] T009 [US1] Configurar o rig Generic, os ciclos e o Animator em `Freyja/Assets/_Project/Art/Ilha_Tripo_011/Protagonista.controller` e na cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. Integrar a velocidade efetiva ao Animator pelo código em `Freyja/Assets/_Project/Scripts/Prototype/`. Ajustar a orientação visual, desligar root motion e calibrar velocidade e cadência para evitar deslizamento visível e passos contra a barreira. (FR-004) [Evidência](evidence/validacao.md).
- [x] T010 [US1] Configurar em `Freyja/Assets/_Project/Art/Ilha_Tripo_011/Mundo_480x270.renderTexture` o mundo a 480 × 270, com filtro pontual e sem MSAA. Em `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`, apresentar a imagem em 16:9, manter o HUD na resolução da tela e conferir a proporção da câmera usada para detectar obstáculos. Ajustar enquadramento e luz para deixar pernas, mochila e silhuetas pixeladas visíveis. (FR-006) [Evidência](evidence/game-view.png).

## Fase 4 — Validação e entrega

- [ ] T011 Conferir a compilação e o Console do Unity. Executar `specs/011-caminhada-modelos-tripo/quickstart.md` em Play Mode com teclado e mouse. Registrar duas idas e voltas, três partidas e paradas, contato dos pés, bloqueio, diagonal, foco, retomada, obstrução da câmera e queda em `specs/011-caminhada-modelos-tripo/evidence/validacao.md`. Corrigir falhas e repetir os casos afetados. (SC-001, SC-002, SC-003) Teclado e mouse exercitados por eventos do Input System. O teste de aceitação com dispositivos físicos segue para avaliação do usuário.
- [x] T012 Abrir `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity` e verificar captura do cursor, movimento, câmera e recuperação após as mudanças no controlador compartilhado. Conferir a preservação das cenas anteriores e dos GLBs de origem, e verificar os pares de assets e arquivos `.meta` da entrega. Registrar o resultado em `specs/011-caminhada-modelos-tripo/evidence/validacao.md`. [Evidência](evidence/regressao-cais.txt).
- [x] T013 Gravar uma ida e volta com parada e mudança de direção na Game View, incluindo o HUD. Salvar em `specs/011-caminhada-modelos-tripo/evidence/caminhada.mp4` e vincular a gravação, a cena e os resultados em `specs/011-caminhada-modelos-tripo/evidence/validacao.md`. Atualizar `specs/011-caminhada-modelos-tripo/tasks.md` com a evidência de cada tarefa concluída. (SC-004) [Evidência](evidence/caminhada.mp4).

## Dependências e ordem

- T001 → T002 → T003. Após T003, T004 e T005 podem ocorrer em paralelo. T004 edita o rig no Blender e T005 altera apenas o controlador no Unity.
- T004 → T006 → T007 → T008. T009 depende de T005 e T008. T010 integra a apresentação após T009.
- T010 → T011 → T012 → T013. Repetir a validação afetada se houver correção antes da gravação.

## Estratégia de entrega

Entregar a US1 como uma única iteração avaliável. Primeiro integrar modelos e percurso, depois acertar movimento, animação e imagem no próprio trecho. Apresentar a cena e o vídeo quando os critérios da spec estiverem verificados.

## Revisão solicitada — escala pelos degraus

- [x] T014 Restaurar a geometria original, aplicar escala uniforme do cenário usando os degraus como parâmetro, ajustar spawn e limites na cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` e verificar as duas subidas em Play Mode. [Evidência](evidence/escala-degraus-teste.txt).
- [x] T015 Registrar medidas e regra de importação em `Source/Blender/Ilha_Tripo_011/README.md` e indicar essa referência em `AGENTS.md`.
