# Tarefas — Spec 004: Relevo da ilha e recanto isolado

**Referências:** [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [contrato de navegação](contracts/navegacao.md), [contrato de integração](contracts/integracao.md) e [quickstart.md](quickstart.md).

**Estado:** encerrada a pedido do usuário. As tarefas não concluídas permanecem abertas como registro; não serão executadas nesta spec. Use os MCPs globais de Blender e Unity quando disponíveis. Preserve alterações não salvas de sessões existentes e não modifique cenas, fontes ou prefabs anteriores.

## Fase 1 — Preparação

- [x] T001 Conferir via MCP Blender e Unity as versões, o projeto e o estado das cenas abertas; registrar alterações não salvas, disponibilidade dos MCPs e hashes da fonte `Source/Blender/Ilha_Bruta_003.blend`, cena `Freyja/Assets/_Project/Scenes/Ilha_Bruta_003.unity`, prefabs compartilhados e configurações em `specs/004-relevo-ilha/evidence/ambiente.md` e `specs/004-relevo-ilha/evidence/source-hashes.json`.
- [x] T002 Preparar `specs/004-relevo-ilha/evidence/` e a pasta de arte `Freyja/Assets/_Project/Art/Environment/IlhaRelevo/` pelo editor Unity; confirmar preservação de `.meta`, GUIDs, pacotes e versão do editor em `specs/004-relevo-ilha/evidence/ambiente.md`. Depende de T001.

## Fase 2 — Base compartilhada

- [x] T003 [P] Criar via Blender MCP `Source/Blender/Ilha_Relevo_004.blend` como fonte independente da 003, mantendo a sessão aberta e suas eventuais alterações não salvas; registrar unidades, coleções e referência ao cais em `specs/004-relevo-ilha/evidence/ambiente.md`. Depende de T002.
- [x] T004 [P] Criar via Unity MCP `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity` a partir da cena 003, preservando a cena original, o cais, barco, jogador e controles; registrar a cópia em `specs/004-relevo-ilha/evidence/ambiente.md`. Depende de T002.

## Fase 3 — US1: reconhecer a ilha pela silhueta (P1)

**Objetivo:** aproximar a composição de `Referencias/modelagem-vista-aerea.png` com ilha mais compacta, enseada, povoado em níveis e monte dominante.

**Teste independente:** comparar a vista aérea da nova maquete com a referência; medir largura e profundidade menores que as da 003 e identificar enseada, monte, povoado e os 12 blocos.

- [x] T005 [US1] Reorganizar a malha editável em `Source/Blender/Ilha_Relevo_004.blend` com contorno menor nas duas dimensões horizontais que o da 003, enseada frontal, costa irregular e monte dominante ao fundo; manter a superfície terrestre conectada e caminhável, sem paredões inacessíveis. Depende de T003.
- [x] T006 [US1] Ajustar terraços, caminhos e transições em `Source/Blender/Ilha_Relevo_004.blend` para que as faces de terreno destinadas à caminhada não excedam 35° e preferencialmente fiquem até 30°, sem degraus incompatíveis com o jogador; aproximar caminhos das hortas e da ruína. Depende de T005.
- [x] T007 [US1] Reposicionar os 12 blocos B1–B4, A1–A4 e L1–L4, largo, hortas e ruína em `Source/Blender/Ilha_Relevo_004.blend`, preservando três setores reconhecíveis e acesso ao terreno ao redor; não detalhar as casas do povoado. Depende de T006.
- [x] T008 [US1] Salvar e reabrir `Source/Blender/Ilha_Relevo_004.blend`, medir dimensões e alturas e gerar vista aérea comparável à referência em `specs/004-relevo-ilha/evidence/`; registrar medidas, contagem e diferenças visuais em `specs/004-relevo-ilha/evidence/medidas.md`. Depende de T007.

## Fase 4 — US2: caminhar e descobrir o outro lado (P1)

**Objetivo:** acessar toda a superfície terrestre, inclusive o topo e as encostas do monte, e descobrir o recanto com casinha simples atrás dele.

**Teste independente:** iniciar no cais, percorrer o povoado, subir o monte e contorná-lo por lados diferentes, visitar o recanto e voltar; verificar faces, conectividade e ausência de recuperação indevida em toda a área amostrada.

- [x] T009 [US2] Modelar em `Source/Blender/Ilha_Relevo_004.blend` o recanto acessível atrás do monte e uma casinha simples com telhado, sem interior ou morador; verificar que o relevo bloqueia a linha de visão da casinha a partir do largo. Depende de T008.
- [x] T010 [US2] Salvar e reabrir `Source/Blender/Ilha_Relevo_004.blend` e exportar via Blender MCP `Freyja/Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_004.fbx`, verificando unidades, eixos, normais, materiais e âncora do cais; preservar os arquivos 003. Depende de T009.
- [x] T011 [US2] Importar `Freyja/Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_004.fbx`, criar `Freyja/Assets/_Project/Prefabs/IlhaRelevo.prefab` e integrá-lo à cena `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity`; desativar apenas na cena 004 o terreno e mar 003 sobrepostos e manter cais, barco, jogador e controles. Depende de T004 e T010.
- [x] T012 [US2] Configurar em `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity` colisores da malha terrestre, blocos e casinha, mar visual sem collider, conexão com cais e spawn seguro; ajustar por instância os bounds de recuperação e o alcance da câmera sem alterar parâmetros ou prefabs anteriores. Depende de T011.
- [x] T013 [US2] Medir todos os triângulos de colisão importados em `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity`, conferindo inclinações até 35°, continuidade e transições; executar análise de conectividade considerando raio, altura e limites do personagem, e registrar método, resolução, regiões não cobertas e falhas em `specs/004-relevo-ilha/evidence/caminhabilidade.md`. Corrigir falhas antes de avançar. Depende de T012.
- [ ] T014 [US2] Executar em Play Mode o percurso em `specs/004-relevo-ilha/quickstart.md`, incluindo acesso e retorno por diferentes lados do monte, topo, encostas, costa, regiões fora dos caminhos, três setores, hortas, ruína e recanto; testar câmera, colisões, queda na água e ausência de recuperação indevida e registrar resultados e limitações em `specs/004-relevo-ilha/evidence/percurso.md`. Depende de T013.
- [ ] T015 [US2] Verificar na cena `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity` que a casinha não é visível do largo e que o recanto é acessível; testar os controles existentes e distinguir entrada física de eventos sintéticos em `specs/004-relevo-ilha/evidence/percurso.md`. Depende de T014.

## Fase 5 — Evidências e entrega

- [x] T016 [P] Gerar vistas aéreas, comparação com `Referencias/modelagem-vista-aerea.png` e capturas da cena `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity` em `specs/004-relevo-ilha/evidence/`, identificando imagens estáticas e pontos visitados em Play Mode. Depende de T015.
- [x] T017 [P] Atualizar `specs/004-relevo-ilha/quickstart.md` com dimensões reais, caminhos, controles, resultados e limitações observadas, incluindo a diferença entre testes sintéticos e avaliação manual. Depende de T015.
- [ ] T018 Reabrir `Source/Blender/Ilha_Relevo_004.blend` e `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity`; verificar console, referências, hashes da T001 e evidências; atualizar `specs/004-relevo-ilha/spec.md` e `specs/README.md` e apresentar a entrega para avaliação sem registrar aprovação visual presumida. Depende de T016, T017 e T019.

## Ajuste solicitado durante a implementação

- [x] T019 [US2] Substituir apenas na cena `Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity` o visual do jogador por modelo da pasta `Low Poly World/downloads/male`, criando asset separado, mantendo controlador, escala e referências de sessão e câmera e preservando os prefabs e cenas anteriores; registrar origem e validação em `specs/004-relevo-ilha/evidence/personagem.md`. Depende de T012.

## Dependências e paralelismo

`T001 → T002 → (T003 || T004)`. No mesmo arquivo Blender, executar `T005 → T006 → T007 → T008 → T009 → T010` em sequência. A integração segue `(T004 + T010) → T011 → T012 → T013 → T014 → T015 → (T016 || T017) → T018`. T016 e T017 podem ocorrer em paralelo: só T016 opera editores e produz imagens; T017 edita apenas o guia.

## Estratégia de entrega

O primeiro ponto avaliável é a silhueta e composição da US1 (T008). A entrega completa inclui também a US2, a casinha isolada e a validação de caminhabilidade. A inspeção de todas as faces combinada a testes amostrados aumenta a cobertura, mas o registro deve declarar a resolução e não prometer prova absoluta de infinitos pontos.

## Encerramento

T014 foi executada parcialmente: 49/54 pontos, sem recuperações, com parada por falta de progresso. T015 e T018 não foram integralmente concluídas. O usuário pediu finalizar a spec e tratar o novo relevo em outra, com fidelidade à referência e caminhada abaixo de 45°. Evidências e preservação final foram registradas em `evidence/percurso.md` e `evidence/preservacao-final.json`.
