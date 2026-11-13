# Método proposto de validação — T013–T015

Este guia prepara a validação da cena 004; não indica que a cena, os colliders ou os testes já foram executados. Usar o Unity MCP com a cena `Ilha_Relevo_004` ativa. Não usar o harness da 003 sem adaptá-lo: `Tools/Unity/ValidateIlhaBruta.cs` exige o caminho `Ilha_Bruta_003.unity` e só enumera waypoints ou observa uma queda.

O analisador agora está em `Tools/Unity/AnalyzeIlhaRelevoWalkability.cs`. Depois de T012, copie `walkability-config.template.json` para `evidence/walkability-config.json`; substitua `terrainRootPaths` por caminhos hierárquicos exatos da cena (incluindo o GameObject raiz), revise coordenadas de `regions` e os parâmetros lidos no `CharacterController`. No Unity MCP eval, execute o arquivo inteiro. Ele exige a cena indicada no JSON e grava um relatório com timestamp em `evidence/`. A configuração é obrigatória para evitar incluir casas, cais ou obstáculos como terreno. MeshColliders convexos não são aceitos como terreno porque o script não acessa a malha convexa cozida usada internamente pela física.

O passo padrão da grade é 0,5 m e há limite de 300 mil células. Para cada célula, o pior caso configurado é um raio central, `edgeSampleCount` raios perimetrais, um teste de volume e até oito casts de cápsula; o JSON estima as quantidades. O tempo real depende da densidade dos colliders e da cena, e ainda precisa ser medido no editor. Uma cena de bounds 160 × 120 m tem cerca de 77 mil células: com oito amostras perimetrais, até ~693 mil raycasts e ~616 mil capsule casts, além dos testes de cápsula e do scan de triângulos. Se o eval exceder o timeout, aumentar `gridSpacingMeters`, reduzir com justificativa as amostras perimetrais ou executar em lotes; registrar a configuração realmente usada.

Há uma cópia do harness da 003 adaptada à cena 004 em `Tools/Unity/ValidateIlhaRelevoRoute.cs`; ela mantém a mesma estrutura de configuração por waypoints e o modo de observar queda manual. Copie `route-config.template.json` para `evidence/validation-config.json` e substitua todas as coordenadas depois de T012. A rota deve ter waypoints adicionais para os dois lados da encosta, borda/costa e retorno. O harness continua guiando direções sintéticas no modo route; só o modo de queda observa WASD real, e ele não valida por si só câmera, teclado ou mouse.

## T013 — análise geométrica integral e conectividade

### 1. Delimitar os colliders

Na cena carregada, enumerar todos os `MeshCollider` habilitados e registrar nome, caminho hierárquico, layer, `sharedMesh`, transformação e `convex`. Selecionar explicitamente o(s) collider(s) de terreno como conjunto-alvo. Registrar à parte blocos, casa, cais e outros obstáculos; não misturar paredes de edifícios com faces de chão nem omitir triângulos do terreno por filtro de layer/nome. Falhar a análise se houver collider de terreno sem malha legível ou transformação não finita.

### 2. Medir todos os triângulos de colisão do terreno

Percorrer cada tripla de índices de cada `sharedMesh`, transformar vértices para coordenadas de mundo e computar a normal geométrica e área. Para cada triângulo, calcular `angle = acos(clamp(dot(normal, Vector3.up), -1, 1))` em graus, orientando a normal para cima apenas para classificar faces potencialmente caminháveis. Relatar contagem total, degenerados, faces descendentes/verticais, mínimo, máximo, distribuição por faixas (≤30°, >30–35°, >35–60°, >60°) e IDs do collider/triângulo de cada face acima de 35°. A condição geométrica é: **nenhum triângulo da superfície terrestre designada para caminhada excede 35°**. Triângulos de paredes de contenção intencionais precisam ser identificados e justificados; não os descartar silenciosamente.

Esta varredura cobre todos os triângulos exportados para colisão, mas sozinha não prova que entre amostras não haja frestas, sobreposições ruins ou acesso corporal bloqueado.

### 3. Amostrar suporte, espaço corporal e conectividade

Usar uma grade horizontal de **0,5 m** sobre os bounds do collider terrestre, com raios para baixo para localizar o primeiro suporte do conjunto de terreno. Guardar por célula posição de apoio, normal, collider e estado. Aceitar um centro somente se tiver suporte ascendente com inclinação ≤35°, e se o volume de personagem estiver livre: raio 0,30 m, altura 1,80 m, centro a 0,90 m acima do apoio. O analisador usa `OverlapCapsule` e exclui da consulta o collider de terreno selecionado para não registrar a própria superfície de suporte como parede; usa amostras perimetrais para conferir suporte e inclui os demais colliders como obstáculos. Isso é uma aproximação geométrica, não a consulta exata do `CharacterController`. Contabilizar e mapear células sem cobertura, inclinação excessiva ou obstrução; declarar resolução e margem usada.

Para testar o espaço ocupado pelo raio, amostrar 8 pontos ao redor do centro a 0,29 m. Exigir suporte terrestre válido nesses pontos. Comparar as alturas com o plano local definido pela amostra central e normais: rejeitar descontinuidades abruptas incompatíveis com `stepOffset=0,25 m`, mas não tratar a variação natural de um plano inclinado como degrau. A tolerância da borda é configurável (`edgeHeightToleranceMeters`, padrão 0,25 m); guardar células excluídas por falta de suporte ou por essa tolerância, em vez de as ocultar. Construir grafo entre centros válidos vizinhos (8 direções). Uma aresta só existe se uma cápsula de raio 0,30 m puder atravessar sem colisão lateral e a diferença de altura residual, após considerar a inclinação local, não ultrapassar 0,25 m. Identificar componentes conectados e gargalos; com grade de 0,5 m, obstáculos ou gargalos mais estreitos que esse passo podem passar despercebidos. A altura 1,80 m precisa ser verificada como espaço livre, não apenas como continuidade da superfície.

Usar como origem o ponto caminhável junto à junção cais/terra, e relatar se cada região de interesse pertence ao mesmo componente: largo, setores B/A/L, hortas, ruína, costa, encostas por lados diferentes, topo e recanto. Para cada região, registrar uma coordenada de amostra e a distância de grade até a origem. Uma grade conectada é evidência computacional aproximada, não substitui a prova de percurso em Play Mode.

**Nota sobre implementação:** `Physics.OverlapCapsule` e `Physics.CapsuleCastAll` não reproduzem necessariamente a regra de degrau de `CharacterController.Move`; o analisador também ignora o mesh de terreno nas consultas de bloqueio para evitar falso positivo do piso, deixando a continuidade do próprio terreno aproximada por amostras e inclinação. A regra de suporte perimetral e altura é uma triagem conservadora. O percurso com o componente real é a verificação decisiva de execução. Registrar a `skinWidth` efetiva encontrada no componente; não presumir outro valor nem alterar os parâmetros herdados.

### 4. Evidência T013 (`evidence/caminhabilidade.md`)

Incluir data, versão do Unity, cena ativa, colliders incluídos/excluídos e razão, parâmetros lidos do jogador (`radius`, `height`, `stepOffset`, `slopeLimit`, `skinWidth`), passo de grade, algoritmo, contagens/distribuições de triângulos, faces >35°, degenerados, células inválidas por motivo, componentes, regiões não cobertas e falhas com coordenadas. Incluir resumo legível e anexo JSON/CSV com resultados por triângulo/célula para permitir auditoria. Não marcar como aprovado se existir triângulo caminhável >35°, uma região requerida desconectada ou falha sem justificativa corrigida.

## T014–T015 — percurso em Play Mode

1. Confirmar cena 004 e parâmetros do jogador; iniciar no cais. Fazer uma passagem guiada por waypoints pode ajudar a repetir o trajeto, mas declarar que sua direção é sintética. A rota deve incluir ida e volta, os três setores, largo, hortas, ruína, caminhos e áreas fora deles, costa, topo e encostas por lados distintos, recanto e aproximação à casinha. Registrar coordenadas alcançadas, checkpoints, duração e recuperações.
2. Observar a câmera e colisões em estreitamentos, passagens entre blocos, junção do cais, beira da água e transições de altura. Fazer queda intencional na água e verificar recuperação no spawn. Repetir permanência em terreno no monte/recanto e confirmar que não há recuperação indevida.
3. Para T015, verificar a visibilidade da casinha a partir do largo na câmera de jogo e confirmar o acesso ao recanto e retorno. Registrar captura e posições da câmera/jogador.
4. Separar no relatório `entrada física`, `entrada sintética`, `movimento observado` e `captura`. Eventos sintéticos e waypoints não demonstram que teclado/mouse físicos funcionam. Fazer ao menos uma passagem com WASD e mouse reais para T015; se a interface ou captura de controle impedir isso, deixar essa parte pendente e descrever o bloqueio.

O harness `Tools/Unity/ValidateIlhaBruta.cs` é referência para observar posição, grounded, flags de colisão, recuperação e waypoints, mas está amarrado à cena 003. Se for adaptado para a 004, mudar a checagem de cena e os caminhos de evidência e manter o modo da rota claramente etiquetado como sintético. Não registrar T014/T015 como concluídos com esse harness sozinho.

## Cobertura e limitações que devem permanecer explícitas

- A enumeração pode cobrir 100% dos triângulos da malha de colisão selecionada; as estimativas de suporte, volume livre e conectividade têm resolução finita.
- A análise do terreno não prova desempenho de `CharacterController.Move`, input, câmera nem ausência de recuperação indevida; são necessários percursos Play Mode.
- O percurso cobre pontos e trajetos efetivamente visitados, não todos os pontos contínuos do terreno.
- Registrar qualquer collider não legível, área excluída pela margem de raio, ilha de grade desconectada e teste físico não executado. Não converter ausência de amostra em aprovação.
