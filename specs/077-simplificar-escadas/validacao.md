# Entrega — Escadas da L3 e da B3

A L3 recebeu a primeira rampa e passou pela matriz de Play Mode e pela revisão visual antes da expansão. O inventário da cena ativa confirmou dois lances de casa. A B3 foi a segunda conversão. A L3 tem 14 pisos visuais e a B3 tem quatro, com patamar plano na porta.

## Implementação

A solução anterior sincronizava direção e velocidade do jogador, estado do Animator, fase da passada e alvos de cada pé com caixas de apoio individuais. A câmera também consultava a pose procedural para escolher o enquadramento de escada.

Cada lance agora usa uma cunha invisível estática de seis vértices e oito triângulos, na layer `CaisSolid`. Os modelos visuais, as barreiras laterais e os patamares continuam nos grupos originais da cena. Foram desativadas 14 caixas de piso na L3 e quatro na B3. O componente de Foot IK foi desligado na cena e seus métodos respeitam esse estado. A câmera conserva a centralização junto às rampas e pode escolher uma posição elevada com visibilidade quando o guarda alcança o início da sonda de órbita.

| Escada | Largura | Comprimento | Desnível | Inclinação |
|---|---:|---:|---:|---:|
| L3 | 1,42 m | 3,92 m | 2,87 m | 36,23° |
| B3 | 1,39 m | 1,46 m | 0,95 m | 32,99° |

A primeira prova mostrou perda de apoio ao correr na descida e perda de enquadramento. O controlador agora usa a normal da face da rampa à frente do centro da cápsula para acompanhar a inclinação em descida. A consulta só atinge um colisor de rampa registrado próximo ao trajeto previsto. Em uma quina, a normal da face evita o erro observado com a normal arredondada do contato. O ajuste atua durante movimento apoiado, com a gravidade e o salto originais. O `CharacterController.Move`, os parâmetros da cápsula, o FOV e a RenderTexture continuam na configuração de produção.

O bake usou o comando existente de navegação do World Builder com a fonte `PhysicsColliders`. O asset de navegação foi atualizado com o GUID anterior. O gerador retornou 12 destinos principais, 17 acessos, 340 pontos de terreno e um ponto isolado. Os ajustes de pontos e moradores produzidos em memória pelo gerador ficaram fora da cena salva. As duas casas são instâncias do mesmo prefab genérico. A autoria ficou localizada nas instâncias, preservando o uso do prefab nas demais casas.

## Evidência no Unity

Unity 6000.3.25f1 no macOS arm64, cena `Ilha_Principal`, protagonista real e câmera de produção com `Mundo_480x270`. Os comandos de movimento reaproveitaram o procedimento de `Tools/Island023/Record.cs`, com a sessão ativa. A captura mostra quadros amostrados da RenderTexture. Os dados em CSV registram as amostras por etapa.

| Ensaio | Resultado |
|---|---|
| L3 | 1.256 amostras nas travessias, sem perda de apoio ou saída do tronco do quadro. Caminhada, corrida, pausas, inversão, diagonais, bordas, guarda e chegada à varanda passaram. |
| B3 | 865 amostras nas travessias, sem perda de apoio ou saída do tronco do quadro. A cápsula chegou a Y local 1,55 m, parou e se deslocou lateralmente no patamar, com a porta limitando o avanço. |
| Cópia da B3 a 121° | Os percursos em caminhada e corrida completaram ida, pausa, deslocamento lateral no piso superior e retorno com apoio contínuo. A cópia foi usada só em Play Mode. |
| Navegação | Teresa completou subida e descida nas duas casas após o bake. A distância final a cada destino projetado ficou abaixo de 0,14 m. |
| Regressão | O protagonista atravessou o convés plano, a junção do cais, o terreno inclinado da igreja e uma soleira de teste de 16 cm. Saltou no terreno e na B3, caiu por uma borda real do píer e retornou ao ponto de recuperação. |
| Test Runner e Console | World Builder com 16 testes de Edit Mode e um de Play Mode aprovados. Compilação concluída, zero erros e um aviso no Console. A cena principal ficou salva em Edit Mode. |

[Referência anterior](evidence/baseline/README.md) · [Piloto L3](evidence/piloto/README.md) · [B3 em imagens](evidence/b3/sequencia.jpg) · [B3 em caminhada](evidence/b3/caminhada-sequencia.webp) · [B3 em corrida](evidence/b3/corrida-sequencia.webp) · [Cópia rotacionada](evidence/b3/copia-121/corrida.json) · [Navegação](evidence/navegacao.json) · [Regressão](evidence/regressao/regressao.json)

As revisões independentes aprovaram a leitura das passadas na câmera do jogo. Há pequenos vãos entre uma sola e o degrau em algumas poses. Os quadros amostrados deixam em aberto pequenos deslizamentos entre capturas. Walk e Run foram suficientes, sem novos clipes. O custo de CPU ficou sem medição comparável. A implementação usa uma sonda local junto a rampas registradas, sem consultas por pé ou alocações recorrentes no novo fluxo.

## Arquivos de produção

- Cena `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` e malhas `L3_RampaColisao.asset` e `B3_RampaColisao.asset` com seus metadados em `Freyja/Assets/_Project/Art/Escadas059/`.
- `Freyja/Assets/_Project/Scripts/Prototype/CaisPlayerController.cs`, `CaisCameraRig.cs` e `StairFootPlacement.cs`.
- `Freyja/Assets/_Project/Data/IlhaWaypointNavigation.asset`.

## Configurar uma escada

1. Na instância selecionada, mantenha os degraus visíveis e os guardas. Duplique um filho de rampa e atribua a malha de colisão que corresponde ao lance em espaço local. Separe mudanças de direção e patamares.
2. No Inspector, confirme `MeshCollider` em `CaisSolid`, sem renderer ou Rigidbody. Use o gizmo do colisor para conferir largura, inclinação e encontro com os dois pisos. Desative as caixas de piso conflitantes. Mantenha os apoios planos e as barreiras.
3. Cadastre o colisor em `stairRamps` do `CaisPlayerController` na cena. Valide o percurso nos dois sentidos e reconstrua o NavMesh com **Freyja → Navegação → Gerar waypoints da ilha**, revisando as alterações do gerador antes de salvar a cena.

Escadas com curvas ou rampas sobrepostas precisam de verificação específica. A configuração atual foi verificada nos dois lances retos da ilha e em uma cópia rotacionada. Para estes dois lances, os controles do Inspector e o fluxo de autoria existente foram suficientes.

## Reverter

Na cena, retire os dois colisores de `stairRamps`, desative as rampas e reative as caixas `Piso 057 01…14` e os quatro degraus da B3. Reative `StairFootPlacement` e volte a incluir `Apoio dos degraus L3` depois da entrada B3 em `stairFlights`. Refaça o bake. Esse procedimento preserva outras edições da cena. Os backups locais do começo da entrega estão em `.git/freyja-stairs-077/` para comparação.
