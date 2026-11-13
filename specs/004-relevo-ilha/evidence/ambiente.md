# Ambiente inicial — Spec 004

Registro de 12/11/2026, feito antes de criar a cena 004. Hashes SHA-256 dos arquivos de origem e base estão em [`source-hashes.json`](source-hashes.json).

## Blender MCP

- MCP global disponível e conectado; verificação do estado informada pelo agente responsável pelo Blender em 12/11/2026.
- Blender 5.2.2 LTS.
- Arquivo aberto: `Source/Blender/Ilha_Bruta_003.blend`.
- Cena ativa: `Cais_Iteracao01`.
- Arquivo limpo, sem alterações não salvas (`dirty=false`). A fonte 003 será preservada.

## Unity MCP

- MCP global disponível e conectado ao projeto `Freyja/`.
- Editor Unity 6000.3.25f1 (revision `e1dba0a9aba4`), renderização Built-in.
- Cena ativa e única cena carregada antes das alterações: `Assets/_Project/Scenes/Ilha_Bruta_003.unity`.
- Cena limpa, sem alterações não salvas (`isDirty=false`).
- `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` e `Packages/packages-lock.json` registrados por hash em `source-hashes.json`.
- Prefabs compartilhados encontrados em `Assets/_Project/Prefabs/`: `Cais_VisualReview.prefab`, `IlhaBruta.prefab` e `PlayerPrototype.prefab`. Os arquivos e seus `.meta` estão registrados por hash; os GUIDs foram preservados.

## Preparação da arte

- Pelo Unity MCP/AssetDatabase, criada a pasta `Assets/_Project/Art/Environment/IlhaRelevo/`.
- A pasta `Environment` e seus assets anteriores não foram alterados. O Unity gerou o `.meta` da pasta nova; caminho e GUID verificados na criação.
- Criada a pasta de evidências desta spec: `specs/004-relevo-ilha/evidence/`.

## Integridade inicial

`source-hashes.json` contém hashes SHA-256 da fonte Blender 003, cena 003 e respectivo `.meta`, todos os prefabs compartilhados e `.meta`, arquivos de `ProjectSettings/` e arquivos de dependências `Packages/`. Esses hashes constituem a linha de base para comparação após a criação independente da cena 004.

## Cena 004 (T004)

- Criada via Unity MCP/AssetDatabase como cópia independente de `Assets/_Project/Scenes/Ilha_Bruta_003.unity`: `Assets/_Project/Scenes/Ilha_Relevo_004.unity` (GUID `f6ce8bc6685844de19ffb7f1648710fa`).
- SHA-256 da cena 003 e da cópia 004: `8b31820fb3a9b551e70b50ee8e680a1d10fe66fe0522fc5c162c6f0b1b60a147` em ambas; a cena 003 e seus assets compartilhados continuam com os hashes de linha de base.
- A cena 003 permaneceu aberta e limpa. Na validação posterior, o Unity informou que a sessão estava em Play Mode; para preservar o estado em execução, a cena 004 não foi aberta. A igualdade byte a byte com a fonte confirma a cópia dos objetos e referências, mas a inspeção carregada no editor permanece pendente.


## Integração Unity T011–T012

Executada em 12/11/2026 pelo Unity MCP no projeto `Freyja/`, Unity 6000.3.25f1, Built-in. O preflight confirmou o FBX `Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_004.fbx` importado com 34 transforms; terreno, 12 blocos, corpo e telhado da casinha tinham MeshFilter direto. A cena 004 estava ativa e limpa, com `Cais_VisualReview`, `PlayerPrototype`, câmera, sessão, spawn, prefab antigo `IlhaBruta`, plano antigo `Mar_IlhaBrutaVisual` e marcador da rampa presentes. Play Mode já estava encerrado; nenhuma cena foi fechada ou descartada.

Foi criado `Assets/_Project/Prefabs/IlhaRelevo.prefab` e integrado em `Assets/_Project/Scenes/Ilha_Relevo_004.unity`. O FBX permanece em origem/rotação zero, sem anchor exportado. O ponto equivalente a Blender `(0,4,1)` foi convertido para Unity `(0,1,-4)` e usado somente como início dos rays de diagnóstico até `Cais_Rampa_Juncao` (Unity `(0,0.82,-2.515)`). O preflight de junção executou 27 rays: 3 faixas laterais × 9 pontos; todos acertaram collider `CaisSolid` do terreno ou cais, e os hits da linha central não excederam degrau de 0,25 m.

Colliders em layer `CaisSolid` (layer 8):

- `IlhaRelevo/Terrain_004_Walkable_Island_Collider`: MeshCollider não convexo.
- `IlhaRelevo/B1_Collider` a `B4_Collider`, `A1_Collider` a `A4_Collider`, `L1_Collider` a `L4_Collider`: BoxCollider.
- `IlhaRelevo/Casa_Recanto_004_Collider` e `IlhaRelevo/Casa_Recanto_Telhado_004_Collider`: BoxCollider.
- Rotas e caminhos não receberam colliders adicionais. `Mar_IlhaRelevoVisual` é um plano visual sem collider.
- Somente a instância de cena do prefab 003 `IlhaBruta` e o plano d’água anterior foram desativados; `Cais_VisualReview`, rampa, barco, jogador e controles permaneceram ativos.

Bounds do terreno: centro `(0, 16.85, -92)`, extents `(87.5, 15.85, 88)`. `recoveryBounds`: centro `(0, 16.51, -91.01)`, extents `(107.5, 56.19, 108.99)`. Spawn mantido em `(0, 0.94, 7.5)`. Câmera aumentada para far clip `344.1055`. Cena salva e limpa (`isDirty=false`). Os 34 hashes-base de fonte 003, cena/prefabs anteriores e configurações permanecem idênticos; `source-hashes.json` continua como referência inicial.

## Visual do jogador na cena 004

Após a integração, aplicado `Assets/_Project/Art/Characters/Player004/Player004_Male_Casual.fbx` como filho de cena `PlayerPrototype/CharacterVisual_004`; o visual `ProvisionalVisual` ficou inativo somente na instância de cena 004. O FBX importado tem um Renderer, materiais `Shirt`, `Skin`, `Pants`, `Eyes`, `Socks` e `Hair`, sem textura usada; bounds de origem de aproximadamente `0.74 × 1.84 × 0.42 m`, escala de importação 1 e rotação de root zero. Escala uniforme ajustada para altura `1.70 m`; pés em Y `0.94`, igual ao root do jogador, centro XZ `(0, 7.5)`, yaw local `0°`. A captura frontal confirma postura e orientação no cais.

O `CharacterController` permanece com altura `1.8`, raio `0.3`, stepOffset `0.25`; `CaisSessionController.player` e `CaisCameraRig.target` continuam referenciando os mesmos objetos. `PlayerPrototype.prefab` compartilhado não foi editado nem recebeu Apply; o hash-base permaneceu igual. Evidências visuais: [`unity-player004.png`](unity-player004.png) (cena) e [`unity-player004-front.png`](unity-player004-front.png) (personagem).

Limites: os rays registram apenas a junção cais/terreno e não substituem a análise completa de inclinações/conectividade T013, nem o percurso manual Play Mode previsto em T014–T015.


## Correção da junção cais/terreno

Em 12/11/2026, os rays de pré-correção mostraram `Inicio_do_caminho.001` (MeshCollider de `Cais_VisualReview`, arquivo compartilhado `Cais_VisualReview.prefab`) em Y 1.48, 1.85, 2.20, 2.55 e 2.88 m nos Z -6, -8, -10, -12 e -14; o collider do terreno 004 permanecia em Y≈1.00 m. O offset de até 1.88 m explica a subida visual/física indevida. `Path_Cais_Plaza` pertence ao FBX novo, é somente Renderer (sem collider), e seus 15 vértices amostrados ficam 0.12 m acima da superfície do terreno, cobrindo Z -18 até -68.

Desativei `Inicio_do_caminho.001` inteiro somente como override da instância de cena 004, sem editar/aplicar `Cais_VisualReview.prefab`; `Cais_VisualReview`, tábuas, degraus, praia, `Cais_Rampa_Juncao` e jogador continuam ativos. Após a correção, rays em X -1.5/0/1.5 e Z -6 a -24 retornam o collider do novo terreno continuamente em Y≈1.00–1.10 m; o collider antigo não aparece. O trecho segue da rampa/praia e dos degraus existentes (Z -3 a -5) para o novo terreno. Salvei a cena 004. Captura: [`unity-cais-path-after.png`](unity-cais-path-after.png). A validação física Play Mode de ida e volta fica para a próxima execução de validação solicitada; esta correção foi verificada por rays/collider e captura Scene View.
