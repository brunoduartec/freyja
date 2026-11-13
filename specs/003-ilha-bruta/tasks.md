# Tarefas — 003 · Ilha bruta caminhável

**Estado:** tarefas geradas; modelagem e implementação não iniciadas.
**Entrada:** [spec](spec.md), [plano](plan.md), [pesquisa](research.md), [modelo](data-model.md), [navegação](contracts/navegacao.md), [integração](contracts/integracao.md) e [validação](quickstart.md).

Cada caixa representa trabalho concluído com evidência. Caminhos são relativos à raiz do projeto. [P] indica trabalho independente em arquivos e editores distintos; [US1] identifica a única história da spec.

**Testes:** o percurso em Play Mode, a contagem dos blocos e as verificações de integração são obrigatórios. A spec não solicita TDD ou suíte automatizada; automação é opcional e não substitui a caminhada real.

## Fase 1 — Preparação

- [x] T001 Conferir versões fixadas, conexão MCP aos projetos corretos e cenas ou arquivos com alterações não salvas; reutilizar evidências ainda válidas e registrar o estado necessário em `specs/003-ilha-bruta/evidence/ambiente.md`. Registrar hashes ou cópias de referência de `Source/Blender/Cais_Iteracao01.blend`, `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity` e dos prefabs compartilhados; não salvar nem descartar alterações do usuário silenciosamente.
- [x] T002 Preparar pelo editor a pasta `Freyja/Assets/_Project/Art/Environment/IlhaBruta/` e o diretório de evidências `specs/003-ilha-bruta/evidence/`, preservando `.meta` e GUIDs; confirmar em `specs/003-ilha-bruta/evidence/ambiente.md` que `Freyja/Packages/manifest.json` e a versão do editor não foram alterados. Depende de T001.

## Fase 2 — Base da maquete

Concluir as duas tarefas antes de construir a história. Os editores e arquivos são distintos; nenhuma das tarefas deve alterar a cena ou fonte compartilhada.

- [x] T003 [P] Criar via Unity MCP a cena `Freyja/Assets/_Project/Scenes/Ilha_Bruta_003.unity` a partir de `Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity`, preservando a cena original, o jogador, o cais e os controles. Salvar a nova cena e registrar sua referência em `specs/003-ilha-bruta/evidence/ambiente.md`. Depende de T002.
- [x] T004 [P] Criar via Blender MCP a fonte separada `Source/Blender/Ilha_Bruta_003.blend` e coleções identificáveis para relevo/costa, rotas, blocos e marcos; definir metros, eixos e referência do encaixe do cais sem sobrescrever `Source/Blender/Cais_Iteracao01.blend` ou suas alterações não salvas. Depende de T002.

**Checkpoint:** cena e fonte novas existem; os arquivos anteriores continuam preservados. Ainda não há percurso aprovado.

## Fase 3 — US1 · Explorar a ilha em construção (P1 / MVP)

**Objetivo:** caminhar pela ilha inteira em uma maquete com 12 blocos de casas e avaliar relevo, caminhos e escala.
**Teste independente:** começar no cais, percorrer largo, três áreas de casas e mirante, voltar pela rota dos fundos e conferir os 12 blocos.

- [x] T005 [US1] Modelar em `Source/Blender/Ilha_Bruta_003.blend` uma ilha inteira low poly com enseada sul, costa, encosta e crista norte; usar aproximadamente 260 × 180 m e até 24 m de altura como referências ajustáveis, registrando dimensões reais em `specs/003-ilha-bruta/evidence/medidas.md`. Depende de T003 e T004.
- [x] T006 [US1] Em `Source/Blender/Ilha_Bruta_003.blend`, estabelecer o encaixe do cais existente e caminhos contínuos até largo, casas baixas, altas, leste e mirante, com circuito pelos fundos passando por hortas e ruína. Começar com caminho principal de 2,5–3 m e subidas graduais; identificar rampas/junções a conferir com o personagem. Depende de T005.
- [x] T007 [US1] Posicionar em `Source/Blender/Ilha_Bruta_003.blend` 12 blocos grandes e editáveis identificados como B1–B4, A1–A4 e L1–L4, quatro por área; conferir espaço e escala humana, sem criar fachadas, telhados ou interiores detalhados. Depende de T006.
- [x] T008 [US1] Representar hortas, ruína, mirante, largo e, se necessários para orientação, venda, sala comunitária e galpão por formas ou marcações simples em `Source/Blender/Ilha_Bruta_003.blend`; manter serviços distintos dos 12 blocos de casas e registrar a distribuição em `specs/003-ilha-bruta/evidence/medidas.md`. Depende de T007.
- [x] T009 [US1] Salvar e reabrir a fonte `Source/Blender/Ilha_Bruta_003.blend`; exportar `Freyja/Assets/_Project/Art/Environment/IlhaBruta/Ilha_Bruta_003.fbx` via Blender MCP e verificar metros, eixos, pivôs, normais e materiais simples compatíveis com o pipeline existente. Preservar os arquivos da 002. Depende de T008.
- [x] T010 [US1] Importar o FBX e criar `Freyja/Assets/_Project/Prefabs/IlhaBruta.prefab` na Unity; integrar à cena `Freyja/Assets/_Project/Scenes/Ilha_Bruta_003.unity`, alinhando o cais e reutilizando jogador e controles. Desativar somente na nova cena os volumes antigos de terreno/mar sobrepostos, sem aplicar alterações aos prefabs compartilhados. Depende de T009.
- [x] T011 [US1] Configurar em `Freyja/Assets/_Project/Scenes/Ilha_Bruta_003.unity` colisões de relevo e blocos, máscaras compatíveis com CaisSolid/CaisPlayer e junção contínua com o cais. Ampliar o mar visual sem collider, conferir faces visíveis e evitar chão invisível sobre a água. Depende de T010.
- [x] T012 [US1] Ajustar por instância em `Freyja/Assets/_Project/Scenes/Ilha_Bruta_003.unity` os bounds de recuperação para toda a ilha e mirante, a distância de visão e o spawn seguro; manter o comportamento de queda e controles da 002. Salvar a cena sem modificar a cena original ou o prefab do jogador. Depende de T011.
- [x] T013 [US1] Executar `specs/003-ilha-bruta/quickstart.md` em Play Mode e registrar `specs/003-ilha-bruta/evidence/percurso.md`: circuito cais → largo → três bairros → mirante → retorno pelos fundos, contagem 4 + 4 + 4, colisões, câmera, extremos dos bounds, queda no mar, controles, dimensões e tempos observados. Corrigir falhas antes de marcar; comparar fontes e cena anterior com T001. Depende de T012.

**Checkpoint:** FR-001 a FR-007 e SC-001 a SC-003 têm resultado registrado. A existência de arquivos ou imagens não substitui caminhar pelo circuito.

## Fase 4 — Entrega para avaliação

- [x] T014 [P] Gerar vista geral da maquete e sequência de capturas do percurso via Blender/Unity MCP em `specs/003-ilha-bruta/evidence/`; registrar em `specs/003-ilha-bruta/evidence/percurso.md` quais pontos foram visitados em Play Mode e quais imagens são apenas vistas estáticas. Depende de T013.
- [x] T015 [P] Atualizar `specs/003-ilha-bruta/quickstart.md` com caminhos, controles, medidas observadas e limitações reais da entrega, mantendo referência ao registro `specs/003-ilha-bruta/evidence/percurso.md`. Depende de T013.
- [x] T016 Conferir reabertura da fonte e cena, referências, compilação/Console e preservação dos arquivos anteriores; atualizar `specs/003-ilha-bruta/spec.md` e `specs/README.md` com links à cena, modelo e evidências. Apresentar a maquete ao usuário e parar para avaliação, sem marcar aprovação visual como recebida. Depende de T014 e T015.

## Dependências e ordem

~~~text
T001 → T002 → ┬→ T003 ─┐
              └→ T004 ─┴→ T005 → T006 → T007 → T008 → T009
                                                    → T010 → T011 → T012 → T013
                                                                               ├→ T014 ─┐
                                                                               └→ T015 ─┴→ T016
~~~

Existe somente US1, de prioridade P1. Preparação e base bloqueiam a história; relevo, rotas e blocos compartilham a fonte Blender e são sequenciais. Entrega vem depois da validação.

## Paralelismo possível

- Após T002, T003 prepara a cena na Unity enquanto T004 organiza uma fonte Blender separada; coordenar apenas os dados do encaixe do cais.
- Após T013, T014 produz capturas enquanto T015 atualiza o guia. T014 é a única tarefa que opera os editores; coordenar qualquer atualização do registro de percurso.
- Não alterar a mesma cena ou arquivo Blender simultaneamente. [P] identifica independência técnica, sem determinar quantidade ou modelo de agentes.

## Cobertura

| Requisito | Tarefas principais |
| --- | --- |
| FR-001 — ilha editável e jogável | T004, T005, T009, T010, T013 |
| FR-002 — escala aproximada | T005, T009, T013 |
| FR-003 — locais e caminhos | T006, T008, T013 |
| FR-004 — 12 blocos / três áreas | T007, T008, T013 |
| FR-005 — navegação e colisão | T006, T011, T013 |
| FR-006 — controles, queda e preservação | T001, T003, T012, T013 |
| FR-007 — fonte e cena para avaliação | T009, T010, T014, T016 |

## Estratégia de implementação

Preparar a base, construir a maquete completa, integrar e caminhar, então apresentar para revisão. O MVP é a US1 inteira: ilha, 12 blocos e percurso navegável; acabamento e narrativa continuam fora do escopo. Medir e registrar metas exploratórias do GDD sem mudar a velocidade da 002 para forçar um tempo. Ao concluir, aguardar a avaliação do usuário.
