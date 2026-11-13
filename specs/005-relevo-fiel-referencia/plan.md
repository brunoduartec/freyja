# Plano de implementação — Spec 005: Relevo idêntico à referência

**Branch:** `005-relevo-fiel-referencia` (nome lógico; workspace sem Git) · **Data:** 2026-11-12 · **Spec:** [spec.md](spec.md)

## Resumo

Refazer a forma do terreno para reproduzir fielmente as seis imagens do conjunto de referência: vista top-down, perspectiva aérea e perfis norte, sul, leste e oeste. Cada imagem é mandatória para sua vista. A malha 004 serve apenas como fonte de componentes, materiais, pontos de ancoragem e convenções de exportação; sua forma não define o resultado. Preservar o fluxo de exploração compatível e permitir caminhada nas superfícies livres abaixo de 45°. Apresentar a maquete comparada a todas as imagens para avaliação antes de expandir para integração final.

## Contexto técnico

- **Linguagem e versões:** arte em Blender 5.2.2 LTS via MCP global; Unity 6000.3.25f1, Built-in e Input System 1.20.0 conforme registros da 004. Confirmar estado atual dos editores e do projeto no início; não alterar versões ou dependências.
- **Dependências:** Blender MCP e Unity MCP globais; sem pacotes novos. Unity é a raiz `Freyja/`, identificada por `Assets/`, `Packages/` e `ProjectSettings/`.
- **Armazenamento:** fontes editáveis fora de `Assets/`; exports e cena próprios em `Freyja/Assets/_Project/`. Evidências dentro desta pasta da spec.
- **Plataforma:** jogo 3D desktop em Unity; nenhum build ou mudança de plataforma previsto.
- **Estrutura:** continuação da cena de exploração Unity 004, em arquivos da 005 separados; inspeção/modelagem no Blender e integração/validação pelo editor Unity via MCP.
- **Métrica de sucesso visual:** coincidência visual das seis características listadas em FR-002 quando referência e vista do modelo têm orientação, projeção e enquadramento alinhados. Não inventar cotas a partir da imagem. Uma aproximação ou uma nota de similaridade sem critério topológico não basta; o usuário avalia a primeira maquete antes da expansão.
- **Caminhabilidade:** classificar cada triângulo da superfície de colisão em relação ao plano horizontal. O alvo é garantir acessibilidade abaixo de 45° e preservar faces a partir de 45°. Analisar malha efetivamente exportada/importada e confrontar a classificação com o `slopeLimit` real do controlador; não mudar velocidade nem limite do controlador para esconder falha geométrica. A classificação geométrica, amostragem de conectividade e movimento real são evidências distintas.
- **Escopo técnico:** reproduzir e integrar o terreno; deslocar elementos existentes quando necessário; colisões, caminhos e comparação. Sem interiores, NPCs, diálogos ou criação de novos sistemas de navegação.

## Verificação da constituição

| Princípio | Como o plano atende |
| --- | --- |
| Spec e GDD governam | A imagem guia relevo; GDD continua fonte de decisões do jogo e da disposição funcional. Geometria invisível da referência é marcada como interpretação. |
| Unity e Blender via MCP | Inspecionar estado e versão; produzir no Blender e integrar/testar pelo Unity MCP quando conectados. Registrar limitações se algum editor estiver indisponível. |
| Low poly e legibilidade | Paredões e patamares usam facetas legíveis; conferir vista aérea, perfil e câmera de exploração. |
| Cânone | A casinha herdada não cria morador, depoimento, pista ou fato novo. Nenhuma decisão narrativa entra nesta feature. |
| Preservação e evidência | Criar fonte, export, prefab e cena exclusivos; manter pares `.meta` e GUIDs existentes; comparar hashes da linha de base e anexar evidências a cada validação. |

**Gate inicial:** aprovado. Nenhuma violação constitucional ou dependência nova identificada. A regra específica da spec5 substitui, somente nesta entrega, “toda a ilha caminhável” e o alvo anterior de 35°; a geometria anterior continua preservada como arquivo da 004.

## Pesquisa — decisões de pré-planejamento

### Forma visível e reconstrução das referências

**Decisão:** tratar a referência original como ortografia visual do terreno e descrever a forma por marcos relativos: recortes da linha costeira e ilhotas rochosas; arco da praia/enseada no primeiro plano; elevação em terraços da área construída e horta; muralha de cristas irregulares atravessando o fundo; e faixas de faces rochosas íngremes entre níveis. Fazer comparação com projeção e orientação equivalentes, registrando também vistas de perfil.

**Razão:** a especificação pede identidade com uma imagem sem escala, e a vista 004 mostra um platô quase oval e um morro suave, estruturalmente diferentes da referência. Textura ou cor por si só não corrige a silhueta.

**Alternativas consideradas:** manter a forma 004 e adicionar textura rochosa; rejeitada porque o maciço e o contorno permanecem errados. Converter a imagem em cotas exatas; rejeitada porque a imagem não fornece medidas nem elevações e não mostra a face posterior.

### Superfícies caminháveis e paredões

**Decisão:** criar patamares e conexões caminháveis onde a superfície tem inclinação inferior a 45°; modelar faces rochosas entre 70° e 80° como impedimentos naturais. Analisar inclinação após integração e não forçar acesso ao topo dessas faces. Corrigir acessos isolados abaixo de 45° dentro da topologia aprovada ou apresentá-los ao usuário.

**Razão:** esta é a regra solicitada na especificação e resolve o conflito da 004, que aplainou a ilha toda para permitir acesso irrestrito. O controlador usa `slopeLimit=60°`, `stepOffset=0,25 m`, raio 0,3 m e altura 1,8 m segundo o plano 004; confirmar os valores na cena antes de assumir validade operacional. O limiar de 45° não garante por si só travessia no Unity; faces de 70–80° excedem o limite registrado e devem barrar movimento.

**Alternativas consideradas:** deixar todo o relevo abaixo de 35°; contradiz os paredões da imagem e a nova regra. Alterar o `slopeLimit` para 45° sem inspeção; pode afetar todos os movimentos e não substitui conectividade, colisões e testes do controlador.

### Reaproveitamento e preservação

**Decisão:** inspecionar `Source/Blender/Ilha_Relevo_004.blend`, export FBX, `IlhaRelevo.prefab` e cena Unity 004; reutilizar componentes e configurações individualmente quando servirem à nova forma. Criar `Source/Blender/Ilha_Relevo_005.blend`, export `Ilha_Relevo_005.fbx`, prefab `IlhaRelevo005.prefab` e cena `Ilha_Relevo_005.unity`. Nunca salvar sobre os arquivos 003/004 ou aplicar mudanças em prefabs compartilhados.

**Razão:** a entrega anterior tem malha, material, personagem, rotas e integração úteis, mas desenho e validação incompletos. Arquivos separados permitem comparação e retorno sem perder a referência de trabalho.

**Alternativas consideradas:** editar a 004 em lugar; rejeitada porque destrói a linha de base necessária à comparação. Copiar cegamente a 004 inteira; rejeitada porque preservaria elementos incompatíveis sem avaliação.

### Validação

**Decisão:** usar três evidências complementares: comparação visual alinhada revisada pelo usuário, análise geométrica da inclinação/topologia da colisão importada (relatando faixas <45°, 45–70°, 70–80° e >80°) e percurso real em Play Mode. Reutilizar e adaptar o analisador `Tools/Unity/AnalyzeIlhaRelevoWalkability.cs` após confirmar seus pressupostos de grade, capsule cast e faixa de declive; produzir configuração específica da 005 e conservar o relatório original como histórico, não como teste da 005.

**Razão:** a evidência da 004 registra 7.290 faces e análise em grade de 0,5 m, mas também deixa claro que ela não consultava a malha cozida do PhysX e não substituía `CharacterController.Move`. O percurso anterior parou em 49/54 pontos.

**Alternativas consideradas:** aprovar somente pela malha Blender ou somente por imagem; rejeitadas porque não cobrem juntas colisão real, acesso e fidelidade de silhueta.

## Estrutura e preservação

```text
specs/005-relevo-fiel-referencia/
  spec.md, plan.md, research.md, data-model.md, quickstart.md
  contracts/navegacao.md
  evidence/                         # capturas, medições, hashes e relatórios
Source/Blender/Ilha_Relevo_005.blend
Freyja/Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_005.fbx
Freyja/Assets/_Project/Prefabs/IlhaRelevo005.prefab
Freyja/Assets/_Project/Scenes/Ilha_Relevo_005.unity
```

Os nomes de arquivo são alvos de produção, não autorização para criar ainda. Antes de qualquer mudança nos editores, registrar estado não salvo e hashes da fonte 004, cena 004, assets e prefabs compartilhados, arquivos de projeto e configurações. Se a sessão estiver suja, trabalhar em cópia separada via MCP. Exportar terreno sem duplicar cais/barco; integrar sob uma raiz própria. Criar e preservar `.meta` por meio do Unity/AssetDatabase, sem recriar metadados existentes. Confirmar encaixe e orientação de coordenadas no cais, em vez de presumir que medidas da 004 ainda são válidas após mudança de forma.

## Sequência de trabalho e pontos de avaliação

1. **Preparação e inventário:** confirmar conexões Blender/Unity, cenas/projeto ativos, versão, alterações não salvas e linha de base por hashes. Inspecionar fontes 004 e todas as imagens mandatórias, identificando divergências antes de modelar. Preparar cópias independentes.
2. **Primeira maquete do relevo — entrega avaliável:** reproduzir a planta top-down e ajustar o relevo aos perfis mandatórios norte, sul, leste e oeste, incluindo faces íngremes. Produzir comparações alinhadas com todas as seis imagens, registrar qualquer divergência entre elas e aguardar avaliação do usuário antes de ampliar o escopo.
3. **Revisão após avaliação:** aplicar o retorno à forma do relevo; registrar a diferença entre sugestão, mudança aceita e inferência oculta. Não registrar aprovação se ela não tiver ocorrido.
4. **Composição e integração:** reposicionar e reutilizar cais, barco, largo, os 12 volumes de moradia, hortas, ruína, trilhas e casa posterior onde compatíveis. Exportar FBX e integrar prefab/cena exclusivos da 005. Configurar colisões do terreno e dos volumes, manter mar visual sem colisão e verificar a junção com cais, spawn, câmera, controlador e recuperação.
5. **Verificação de inclinação e conectividade:** classificar faces abaixo e a partir de 45° na malha de colisão importada, confirmando em separado as faces previstas de 70–80°; procurar faces degeneradas, emendas, degraus, pequenas ilhas de terreno e acessos interrompidos. Fazer análise amostrada adequada ao raio e altura do jogador, com grade e cobertura registradas. Corrigir áreas obrigatórias isoladas sem apagar paredões legítimos.
6. **Percurso em Play Mode e entrega:** percorrer ida e volta pelo cais, largo, setores, hortas, ruína, caminhos, terrenos livres abaixo de 45° e recanto; conferir bloqueio nos paredões, queda na água, câmera, colisões e recuperação. Registrar resultados reais separadamente de análises estáticas/sintéticas; reabrir assets, revisar console e referências, comparar hashes e apresentar a cena e evidências para avaliação final.

## Riscos e decisões pendentes

- **Imagem ilustrada, sem escala:** a fidelidade é visual e topológica para o que aparece. Extensões, dimensões e lado posterior são propostas claramente identificadas; não existem cotas exatas a extrair.
- **Limite não equivale ao controlador:** o controlador de 004 permite até 60°; superfícies abaixo de 45° podem ainda falhar devido a rugosidade, estreitamentos, colisores e geometria da junção. Faces de 70–80° são paredões que devem barrar a caminhada. A validação de movimento decidirá isso.
- **Tensão entre trilhas e imagem:** abrir caminhos pode modificar encostas e patamares. Priorizar a leitura do relevo; trilhas devem seguir terraços e rampas já plausíveis e permanecer utilizáveis sem cortar faces dominantes.
- **Versões e sessões:** os registros de MCP e estado dos editores são de 12/11/2026 e podem ter mudado. Reconferir antes de operar, e nunca salvar nem descartar alterações abertas do usuário sem necessidade aceita.
- **Ponto de parada:** entregar a primeira maquete para avaliação antes de iniciar a fase 3. As etapas restantes são o plano proposto para a feature, não autorização para ultrapassar esse checkpoint antes do retorno do usuário.

## Complexidade

Sem novas dependências. O trabalho é principalmente revisão de geometria de cenário, integração de uma cena isolada e validação visual/física. A principal incerteza está na reconstrução de uma ilustração sem escala e na compatibilidade entre paredões e conexões percorríveis.

## Gate após pesquisa

Sem clarificações técnicas bloqueadoras. O gate da constituição permanece aprovado; o ponto de avaliação visual da etapa 2 é obrigatório antes de continuar a implementação.

## Prioridade das referências — decisão de 2026-11-13

Por instrução explícita do usuário, `Referencias/modelagem-vista-aerea.png` é o oráculo. Em qualquer divergência de relevo, proporção, altura, silhueta ou disposição dos patamares, essa imagem prevalece sobre a topdown e as quatro vistas laterais. As demais imagens continuam obrigatórias para comparação, mas não podem impor uma forma incompatível com o oráculo. A vista norte não autoriza abrir um vale que interrompa o maciço contínuo da aérea. A aprovação de exatidão depende da comparação visual da maquete; contagem de faces íngremes não prova semelhança.
