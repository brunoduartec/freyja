# Prompt para Codex — implementar o Freyja World Builder em Unity (sem geração 3D externa)

Você é um **Staff Unity Tools Engineer + Technical Artist + Gameplay QA Engineer** atuando diretamente no repositório existente do jogo **Freyja**. Seu objetivo é **implementar e validar**, e não somente propor um plano, uma ferramenta de autoria de mapas chamada **Freyja World Builder**.

> **Documento de referência:** `ESPECIFICACAO_TECNICA_FREYJA_WORLD_BUILDER.md` (colocar este arquivo no contexto do Codex). Se o documento não estiver acessível, siga também os requisitos autossuficientes abaixo. Se houver conflito com o estado real do projeto, preserve a integridade do projeto e registre a adaptação fundamentada.

## Restrição absoluta: não consumir créditos nem gerar modelos em serviços externos

**Você, Codex, não está autorizado a acionar, delegar, pedir nem preparar geração de modelos no TripoAI.** Esta restrição vale para qualquer LLM, subagente, MCP, browser, ferramenta, automação, API ou interface web, e também para geradores 3D equivalentes.

1. **Não executar nenhuma geração no TripoAI ou em serviços similares, mesmo se houver créditos ou chave de API disponíveis.** Não realizar chamadas de geração nem criar jobs pagos ou gratuitos.
2. **Não implementar integração com a API, MCP, SDK ou endpoint do TripoAI**, nem instalar clientes, gravar credenciais, adicionar comandos de geração ou adicionar dependência de rede para geração 3D.
3. **Não solicitar ao usuário que gere ou compre novos modelos** para que a ferramenta, os testes ou a demo funcionem. Modelos novos são decisão manual futura, fora deste trabalho.
4. **Trabalhar exclusivamente com arquivos locais que já estejam no projeto**. Se não houver casas reais, criar *proxy prefabs* com primitivas da Unity ou meshes locais simples para demonstrar a montagem, escala, fundação e substituição.
5. O módulo chamado `TripoAssetNormalizer` é apenas um **importador/normalizador offline de arquivos existentes**, não um gerador de assets. Se preferir, renomeie-o para `ExternalModelNormalizer` para evitar ambiguidade.
6. No relatório final, incluir `External 3D generation jobs: 0` e confirmar por inspeção que não foram adicionados SDKs, endpoints, MCPs, integrações ou comandos de geração 3D. Em caso de dúvida sobre custo ou rede, **não executar a operação**; use fallback local.

Esta política tem precedência sobre qualquer sugestão de ferramentas do projeto que tente produzir novos modelos por IA.

## Missão e requisitos inegociáveis

Implementar no **Editor da Unity** um World Builder para um cenário **híbrido**: `Unity Terrain` para relevo/vegetação natural; meshes independentes para ruas, calçadas, guias, bases e fundações; prefabs para casas, prédios e ambientação.

Configuração da demo:

- Mapa: **160 × 160 metros**, centrado na origem global `(0,0,0)`; X,Z de `-80` até `+80`.
- `1 Unity unit = 1 metro` e Y para cima.
- Terrain inicial: tamanho `(160,32,160)` com origem `(-80,-4,-80)`; superfície plana inicial global em `Y=0` (height sample `0.125`); heightmap `257 × 257`.
- Grid visual e posicionamento com snap de **1 metro** (opcional 0,5 m).
- Lotes: **12 × 14 metros** (X local × Z local), cada um com ID estável, centro, yaw, footprint de construção, cota de fundação, prefab e margem.
- Demo: **uma rua** com largura de referência de 5 m e pelo menos 40 m de comprimento, **três lotes**, **três casas**, relevo suave, transição de altura e props simples. Se não houver modelos reais no projeto, criar protótipos com primitivas e deixar o fluxo de substituição pronto.
- Importar **FBX exportado do TripoAI** ou outro FBX e produzir um prefab normalizado, sem alterar fonte; GLB só com importador já presente ou via conversão explícita.
- Implementar **validação automática** de coordenadas, bounds, overlaps, escala, fundação, idempotência, smoke test e capturas visuais quando disponíveis.
- Criar **documentação para utilização**, relatório de execução e critérios claros de aprovação/pendência.

Não criar uma cena inteira no Tripo. Não gerar todas as casas como mesh única. Não supor que trocar `Transform.localScale` manualmente resolve a normalização de importação.

## Forma obrigatória de execução

**TRABALHE NO CÓDIGO.** Não se limite a uma resposta conceitual. Primeiro faça uma auditoria rápida; em seguida implemente incrementalmente, rode testes e corrija falhas. Faça commits lógicos somente se o workflow e permissões do projeto permitirem; caso contrário, descreva os arquivos alterados e checkpoints. Não peça confirmação para decisões pequenas já cobertas pela especificação.

Se alguma ferramenta externa não estiver disponível (Unity Editor, build local, Blender CLI, acesso a FBX Tripo), **continue implementando tudo que for possível**, mas não invente resultado de teste. Registre `not_run`, causa, comando de reprodução e artefatos faltantes.

### ETAPA 0 — Reconhecer o projeto e proteger o que já existe

1. Inspecione `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, packages de teste, render pipeline, asmdefs, plataformas e organização de cena/assets.
2. Descubra como abrir/compilar/testar o projeto neste ambiente. Verifique se Unity Editor está instalado, se há executor de testes, se existe Blender CLI e se há assets FBX de referência.
3. Mapeie sistemas existentes de geração de cenário, blocagem, grid, prefabs, QA e screenshots. **Reutilize** componentes existentes quando possível; não criar duplicatas com comportamento divergente.
4. Garanta preservação de cenas e assets existentes. Criar cena isolada `Freyja_WorldBuilder_Demo` e novo módulo/namespace (adaptar aos padrões reais do repo).
5. Faça inventário inicial de problemas conhecidos e riscos: resolução de Unity, ausência de importador GLB, ausência de FBX real, falta de GPU em headless etc.
6. Produza breve relatório `Docs/WorldBuilder/IMPLEMENTATION_LOG.md` e atualize-o ao longo do trabalho.

**Gate 0:** versão e estrutura conhecidas, sem mudanças destrutivas nas cenas originais.

### ETAPA 1 — Modelo de dados, coordenadas e grid

Implemente (nomes adaptáveis ao projeto):

- `WorldDefinition : ScriptableObject`: mundo, seed, tamanho, Terrain settings, layers, listagem de lotes e vias.
- `LotDefinition`: ID persistente, centro XZ, tamanho XZ 12×14, yaw, footprint edifício, setback, targetY e prefab.
- `AssetDescriptor : ScriptableObject`: referência fonte, prefab resultante, target footprint, orientação, bounds medidos, escala uniforme, warnings.
- `WorldInstanceId : MonoBehaviour`: marca instâncias gerenciadas por gerador.
- `GridMath`: snap métrico, inclusive números negativos e valores de borda.
- `TerrainSpace`: world XZ ↔ UV ↔ índices 0..256; height normalized ↔ worldY.
- `OrientedRectXZ` e `CollisionMath`: teste SAT para lotes orientados, edifícios, vias (via footprint ou segmentos).

UI em `Tools > Freyja > World Builder` com selecionar/criar WorldDefinition, seção de grid, seção de lotes e ações Preview/Apply/Validate. Mostrar grid com `SceneView.duringSceneGui` + `Handles`; permitir criar/mover/rotacionar lotes, desenhar footprint e indicador de orientação, snap 1 m e `Undo`.

Regras:

- Coordenadas XZ são globais; lote armazena `centerXZ` e `yaw`.
- `world XZ [-80,80]` deve mapear exatamente para `[0,1]` e índices `[0,256]`.
- Posição de grid tem rounding determinístico; não permitir posicionamento fora dos limites sem issue.
- IDs de lotes estáveis e únicos; persistir mudanças no ScriptableObject.
- Usar `PrefabUtility.InstantiatePrefab` para preservar ligação com prefab.
- Nunca executar geração pesada em `OnValidate` ou em loop de `SceneView`.

**Gate 1:** testes unitários de coordenadas, snap e SAT passando; window abre e manipula lotes sem erros.

### ETAPA 2 — Geração do Terrain híbrido e fundações

Implemente Terrain 160×160 m com TerrainData EXCLUSIVO da cena demo, altura 32 m, offset Y=-4 m, baseline Y=0 m, heightmap 257×257 e seed 1138. Relevo parametrizado, leve, determinístico.

Algoritmo recomendado para um lote:

1. Ler o **terrain base procedural**, sem deformações acumuladas de gerações anteriores.
2. Em cada amostra do heightmap, converter mundo para espaço local do footprint do edifício, considerando yaw do lote.
3. Determinar uma área de apoio = footprint estrutural + margem 0,5 m por lado.
4. Escolher targetY da mediana do terrain base sob a área (ou cota manual configurada).
5. Nivelar amostras dentro do apoio para targetY.
6. Aplicar blend `smoothstep` durante 2 m ao redor da área de apoio; fora do blend manter terreno natural.
7. Verificar se stamp invade via, outro lote, limites ou uma zona de blend conflitante; emitir issue e impedir operação insegura, em vez de gerar relevo quebrado.
8. Posicionar base da casa/fundação em targetY e criar saia/mesh da fundação se a diferença entre corte/aterro e terreno vizinho pedir acabamento.
9. Usar `SetHeightsDelayLOD` para interação, `SyncHeightmap` ao concluir; registrar Undo/dirty de forma apropriada.

Infraestrutura:

- Criar uma rua de 5 m de largura como mesh independente do Terrain e, se possível, calçadas/guias modulares.
- Gerar uma faixa de apoio sob a rua com perfil longitudinal suave; não deixar mesh flutuar ou atravessar terra.
- Detectar overlap rua–lote–construção com matemática geométrica, não depender exclusivamente de `Physics.OverlapBox`.
- Não construir um sistema complexo de splines se não houver necessidade para três lotes da demo.

**Gate 2:** após gerar, a região sob o prédio é plana (delta vertical <= 0,05 m em 16 pontos de inspeção por edifício); fora do blend o terreno permanece inalterado; rebuild com mesma seed e dados produz mesma geometria.

### ETAPA 3 — Posicionamento de prefabs, preview e idempotência

- Permitir atribuir prefab a um lote e posicioná-lo pela base da fundação, com escala da instância raiz `(1,1,1)`.
- Permitir preview ghost antes do commit, colore conflito e mensagens acionáveis na janela.
- Criar prefabs/fundações com `WorldInstanceId` para rastrear quem é gerenciado.
- Ao reconstruir, atualizar por ID; não duplicar casas, não modificar objetos manuais, não apagar outras cenas.
- Preferir operação explícita `Rebuild Managed Only`; antes de operações destrutivas, registrar alterações e garantir `Undo`/backup onde aplicável.
- Evitar `PrefabUtility.UnpackPrefabInstance` exceto quando absolutamente necessário e justificado.
- Usar bound e footprint estruturais separados quando o telhado possuir beiral.

**Gate 3:** em dois rebuilds sucessivos continuam existindo exatamente três lotes e três casas; trocar prefab de um lote preserva posição, fundação e identidade do lote.

### ETAPA 4 — Importação e normalização de assets TripoAI

Implemente um assistente Editor que recebe um **FBX selecionado** (não exige API Tripo):

- Inspeção de hierarquia, meshes, materiais e bounds combinados em metros.
- Correção manual/configurável da orientação para `Y up / +Z forward`.
- `targetFootprintXZ` configurável; cálculo de **escala uniforme** para enquadrar geometria dentro da área-alvo sem esticar janelas/portas.
- Ajuste visual de pivô para centro inferior (*bottom center*); orientação/pivô devem ficar numa camada filha, mantendo `NormalizedRoot` em `(1,1,1)`.
- Geração de prefab wrapper sem alterar o FBX de origem; salvar `AssetDescriptor` com bounds de origem, escala, final bounds e avisos.
- Collider simples opcional por footprint estrutural; não criar MeshCollider caro por padrão.
- Verificação de materiais/texturas em pipeline já presente; erros de material rosa ou referência nula devem aparecer como issues.
- Auto-warnings para modelo sem mesh, escala zero/NaN, altura desproporcional, proporção XZ incompatível, modelo com piso embutido, origem de pivô estranha.
- Se houver FBX real no projeto: testar ponta a ponta e atribuir à demo. Caso contrário: usar prefab proxy para cena e registrar `Tripo real import: not_run (no source FBX)`; garantir que a interface aceite um FBX futuro.

**Importante:** não implementar integração direta com a API do Tripo neste MVP, não armazenar chaves e não adicionar dependência de rede. Não assumir que Unity abre GLB nativamente. Importar via FBX; GLB só via glTFast existente ou conversão explícita. `AssetPostprocessor` só se necessário e restrito a pasta prefixada, para não interferir em assets do resto do jogo.

**Gate 4:** um FBX já fornecido pode virar prefab normalizado, vinculado à fonte, com escala raiz 1 e relatório de dimensões. Sem FBX fornecido, interface + testes sintéticos devem existir, mas a integração real fica pendente. **Não gerar, encomendar ou solicitar FBX por IA para destravar esse gate.**

### ETAPA 5 — Harness automatizado de validação e evidências

Crie testes `EditMode` e `PlayMode` compatíveis com o Unity Test Framework disponível. Ao menos:

1. Conversão world ↔ heightmap e origem Y deslocada.
2. Snap de 1 m / 0,5 m para valores negativos e positivos.
3. Retângulos orientados: colisão, ausência de colisão e bordas contíguas.
4. Stamp em lotes com yaw 0°, 90° e 45°; centro plano, blend local e exterior preservado.
5. Mundo determinístico com seed igual e resultado distinto com seed diferente.
6. Limites de mapa 160×160; lotes 12×14; nenhum lote sobreposto ou atravessando vias.
7. Fundação: conferir 16 amostras por edifício; `max |height-targetY| <= 0.05 m`.
8. Prefabs: vínculo presente, root scale `(1,1,1)`, bounds final correto.
9. Rebuild idempotente e preservação de objetos manuais.
10. Smoke de carregamento da demo e ausência de exceções.
11. Relatório registra `passed`, `failed`, `not_run` ou `needs_review`, nunca verde sem execução.

Implemente `Tools > Freyja > World Builder > Validate & Capture` ou comando equivalente na interface. Exportar:

```text
Artifacts/WorldBuilder/<run-id>/
  report.json
  report.md
  editmode-results.xml      (se executado)
  playmode-results.xml      (se executado)
  top_down.png              (quando possível)
  isometric_overview.png    (quando possível)
  street_level.png          (quando possível)
  editor.log                (quando disponível)
```

**Screenshots obrigatórios quando graphics device permitir:** no mínimo três câmeras estáveis (`top_down`, `isometric_overview`, `street_level`), 1280×720 e captura real via câmera/RenderTexture (ou técnica apropriada ao pipeline). Registrar posição, orientação, fov/orthographic size, resolução e hash/ID de cena. Se a execução usar `-nographics` ou GPU indisponível, deixar visual como `not_run`, não alegar que passou.

**Comparação visual:** screenshot existente != screenshot aprovado. Se houver sistema de baseline confiável no projeto, integrar; senão, gerar imagens e marcar `needs_review`. Não inventar métricas perceptuais sem implementar efetivamente o cálculo. Conferir também a integridade do relatório com teste automatizado.

Use comandos equivalentes à versão realmente detectada, por exemplo no macOS:

```bash
UNITY="/Applications/Unity/Hub/Editor/<versao>/Unity.app/Contents/MacOS/Unity"
PROJECT="$(pwd)"
mkdir -p "$PROJECT/Artifacts/WorldBuilder"
"$UNITY" -batchmode -quit -projectPath "$PROJECT" -runTests \
  -testPlatform EditMode \
  -testResults "$PROJECT/Artifacts/WorldBuilder/editmode-results.xml" \
  -logFile "$PROJECT/Artifacts/WorldBuilder/editmode-editor.log"
"$UNITY" -batchmode -quit -projectPath "$PROJECT" -runTests \
  -testPlatform PlayMode \
  -testResults "$PROJECT/Artifacts/WorldBuilder/playmode-results.xml" \
  -logFile "$PROJECT/Artifacts/WorldBuilder/playmode-editor.log"
```

Quando possível, execute os testes e consulte o XML/log real. Não confunda erro de compilação com falha de teste. Se não houver Unity instalado no ambiente, tente pelo menos uma revisão estática de C# e arquivos gerados, registre comandos prontos e liste riscos não validados.

**Gate 5:** relatório produzido a partir de verificações reais, sem falsos positivos; os testes disponíveis passam; as screenshots existem ou o motivo de não execução está documentado.

### ETAPA 6 — Demo, instruções e revisão de qualidade

Entregue uma demo de primeira rua com três casas, uma colina e transição natural. Selecione ângulos de câmera para verificar: alinhamento casa/calçada, encaixe da porta no solo, relação de escala humano/casa/rua, fundo do lote e ausência de fendas. Para a arte, preserve o estilo vigente do Freyja; **não imponha** uma estética nova.

Documente em `Docs/WorldBuilder/README.md`:

- Como abrir o World Builder.
- Como criar um lote 12×14 e editá-lo no Scene View.
- Como gerar terreno/hill e nivelar fundações.
- Como adicionar FBX do Tripo, configurar dimensões/correção de orientação e criar prefab.
- Como substituir uma casa de um lote sem quebrar terreno.
- Como regenerar com segurança.
- Como executar QA manualmente e via CLI, onde ler relatórios e screenshots.
- Limitações conhecidas e próximos passos.

**Gate 6 / Definition of Done:** a ferramenta abre, demo existe, três casas são instâncias de prefab, terreno 160×160 e três lotes 12×14, foundation <=5 cm, grid e snapping corretos, dados persistidos, normalizador de FBX operacional ou explicitamente limitado, QA documentado e sem regressões conhecidas nas cenas originais.

## Regras de engenharia

- Prefira **solução simples, demonstrável e testada**, não framework gigantesco.
- Código C# idiomático, métodos pequenos, módulos testáveis, interfaces só quando necessário.
- `Editor` e `Runtime` separados; não quebrar assemblies/plataformas de build.
- Evitar pacotes externos sem necessidade; informar sempre que adicionar/alterar package.
- Preservar referências `.meta`/GUID e prefab links.
- Para arrays do Terrain, respeitar `[zIndex,xIndex]`; heightmap normalizado `[0,1]` e offset Y.
- Mesclar stamps deterministicamente usando ordem estável e conflitos explícitos.
- Não deformar meshes do Tripo; ajustar orientação, escala uniforme e pivô no wrapper.
- `Undo` e `EditorUtility.SetDirty`/scene dirty corretamente, sem loops infinitos.
- Nunca sobrescrever uma cena original; isolar demo e assets gerenciados.
- Relatório de validação deve diferenciar sucesso real, falha, dependência não disponível e revisão visual pendente.

## Ao final, responda no seguinte formato

1. **O que foi implementado:** arquivos/classes principais, novos menus e caminho da cena demo.
2. **Como usar:** passos curtos para criar lote, importar FBX, substituir casa e validar.
3. **Testes realmente executados:** comandos, resultado, quantidade de testes e localização de logs/screenshots.
4. **Falhas e pendências:** lista objetiva, incluindo limites do ambiente e importações reais não verificadas.
5. **Critérios de aceite:** tabela `OK / FALHOU / NÃO EXECUTADO / REVISÃO MANUAL` com os itens do escopo.
6. **Próxima melhoria recomendada:** um único próximo incremento tecnicamente justificado, depois da demo estável.

**Não conclua apenas com uma descrição. Implemente, execute, inspecione as evidências e corrija antes de entregar.**
