# Auditoria inicial

## Projeto e reaproveitamento

| Área | Evidência |
| --- | --- |
| Editor e máquina | Unity 6000.3.25f1 em Mac16,8, Apple M4 Pro, Metal. O MCP confirmou Built-in, caminho Forward, instancing, compute e argumentos indiretos disponíveis, com `StandaloneOSX` selecionado. Não encontrei perfis de distribuição para outros alvos. [Configuração gráfica](../../Freyja/ProjectSettings/GraphicsSettings.asset#L41) |
| Câmera e cena | `Ilha_Principal` estava carregada e limpa. A câmera jogável é perspectiva, FOV 58°, com alvo `Mundo_480x270`, filtro point e sem HDR/MSAA. A tela apresenta a mesma textura. [Cena](../../Freyja/Assets/_Project/Scenes/Ilha_Principal.unity#L10768) |
| Terreno | O gerador histórico usa 160 × 160 m. O terreno ativo mede 132,8 × 132,8 m com faixa vertical de 40 m. O lote padrão de 12 × 14 m pertence à autoria do World Builder. A ilha jogável usa terreno compactado, malhas de vias, praça e acessos, e a implantação existente define os lotes construídos. [Gerador](../../Freyja/Assets/_Project/WorldBuilder/Editor/Island023.cs#L42) · [Lote padrão](../../Freyja/Assets/_Project/WorldBuilder/Runtime/WorldDefinition.cs#L25) · [Compactação](../../Tools/IlhaCompacta030/Compact030.cs#L107) · [Cena](../../Freyja/Assets/_Project/Scenes/Ilha_Principal.unity#L40450) |
| Grama | O layout ativo de relevo contém 52.443 matrizes em 129 células e 445 lotes gravados. O renderer reutiliza três malhas, agrupa até 511 matrizes por chamada e aplica frustum culling e transições por distância. O shader só aplica gradiente, fog e vento com direção fixa. O bake histórico usa máscaras de solo, declive e exclusões, mas aponta para um layout anterior. [Vínculo da cena](../../Freyja/Assets/_Project/Scenes/Ilha_Principal.unity#L47391) · [Renderer](../../Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.cs) · [Autoria](../../Freyja/Assets/_Project/Editor/CoastalGrassAuthoring.Island.cs#L250) |
| Ambiente | `CoastalDayNight` controla sol, ambiente e céu e permite fixar horário. O vento observado na grama pertence ao shader local. A revisão inicial não encontrou controlador global de vento nem sistema jogável de construção ou cultivo. [Relógio](../../Freyja/Assets/_Project/Scripts/Environment/CoastalDayNight.cs) · [Shader](../../Freyja/Assets/_Project/Art/Grass029/CoastalGrass.shader#L45) |
| QA | O harness registra movimento e RenderTexture do jogador. A rota do script é histórica, e o procedimento da spec 077 já foi adaptado à cena atual. A spec 032 contém medições históricas de standalone em Metal. [Harness](../../Tools/Island023/Record.cs) · [Procedimento recente](../077-simplificar-escadas/evidence/baseline/README.md) · [Benchmark histórico](../032-estudo-vegetacao/evidence/manchas/validacao.md) |

## Referências externas

| Referência | Resultado da auditoria |
| --- | --- |
| Endless Meadow | Código e licença MIT acessíveis no commit `a4b6cbb5ef47c793a1eb482a1f7e80e77b89bf8a`. O exemplo requer Unity 6 e URP 17+, usa Terrain, compute com append buffers e envio indireto. O código se liga a `RenderPipelineManager.beginCameraRendering` e os dois shaders declaram `UniversalPipeline` e `UniversalForward`. A iluminação usa sol, sombras recebidas e ambiente SH. Não há contribuição de luminárias adicionais nem sombras próprias. O README descreve várias câmeras, com teste publicado em DX12. A RenderTexture e Metal não foram verificados para o pacote. Seu uso direto foi descartado para Built-in antes de importar, e o teste isolado condicionado à compatibilidade não se aplica. Vento espacial, distribuição por camadas e variação de folhas ficam como referências de comportamento. [README](https://github.com/Parrot222/EndlessMeadow/blob/a4b6cbb5ef47c793a1eb482a1f7e80e77b89bf8a/README.md) · [Licença](https://github.com/Parrot222/EndlessMeadow/blob/a4b6cbb5ef47c793a1eb482a1f7e80e77b89bf8a/LICENSE.md) · [Renderer](https://github.com/Parrot222/EndlessMeadow/blob/a4b6cbb5ef47c793a1eb482a1f7e80e77b89bf8a/Runtime/MeadowGrass.cs) · [Shader](https://github.com/Parrot222/EndlessMeadow/blob/a4b6cbb5ef47c793a1eb482a1f7e80e77b89bf8a/Shaders/MeadowGrassBlades.shader) |
| Cyanilux | O exemplo descreve instancing, matrizes de instâncias e compute culling em URP/Shader Graph. Serve de referência técnica, com implementação Built-in a validar se uma otimização indireta se justificar. [Artigo do autor](https://www.cyanilux.com/tutorials/gpu-instanced-grass-breakdown/) |
| Unity Grass Instancer | MIT. O exemplo Unity 6 usa URP 17.0.3 e oferece exemplos de chunking e culling de frustum, distância e oclusão. Shaders publicados são para URP ou HDRP. [Repositório](https://github.com/MangoButtermilch/Unity-Grass-Instancer/tree/f5983739d6159dbad115950e9ac349a1dd44e78e) · [Licença](https://github.com/MangoButtermilch/Unity-Grass-Instancer/blob/f5983739d6159dbad115950e9ac349a1dd44e78e/LICENSE) |
| Staggart | Referência de acabamento e interação publicada em produto comercial. [Publicação do fornecedor](https://assetstore.unity.com/packages/vfx/shaders/stylized-grass-shader-for-unity-6-357954) |
| Sucker Punch e AMD | Referências conceituais para vento espacial, folhas curvas e simplificação por distância. A implementação de exemplo da AMD usa mesh shaders. [Palestra](https://gdcvault.com/play/1027214/) · [Artigo da AMD](https://gpuopen.com/learn/mesh_shaders/mesh_shaders-procedural_grass_rendering/) |
| Unity | `RenderMeshInstanced` é a base já ativa. `RenderMeshIndirect` requer compute e buffers de argumentos e fica como opção se o custo medido justificar. [Instanced](https://docs.unity3d.com/ja/current/ScriptReference/Graphics.RenderMeshInstanced.html) · [Indirect](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshIndirect.html) |

A hipótese inicial é preservar o fluxo de autoria e os lotes do Freyja, comparar a solução atual com um ensaio mínimo de luz e vento Built-in e só então estender a distribuição e o renderer. Nenhum código externo foi importado.

## Baseline nova no Editor

Amostras feitas em Play Mode na câmera de produção, no ponto inicial do cais às 12h, por Unity MCP. Cinco segundos de aquecimento e 300 quadros por condição. A captura PNG veio depois da amostragem de cada condição. O relógio e o componente foram manipulados apenas em execução. A ordem foi grama atual, seguida de grama desligada. `vSyncCount=1` e `targetFrameRate=-1` foram observados. A taxa reportada é do loop do Editor. O Console registrou zero erros.

| Condição | Quadro p50 | Quadro p95 | CPU principal p50 | CPU render p50 | GPU p50 | Tufos enviados |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Grama atual | 2,014 ms | 2,542 ms | 1,026 ms | 1,740 ms | 1,767 ms | 686 |
| Grama desligada | 2,009 ms | 2,576 ms | 0,962 ms | 1,693 ms | 1,778 ms | 0 |

Este ponto é pouco denso. Uma rodada por condição não permite atribuir a diferença observada de GPU à grama. A matriz de campo, noite, zoom e qualidade e a confirmação em Player permanecem pendentes.

[Valores observados](evidence/baseline/amostras.json) · [Grama atual](evidence/baseline/grama-atual.png) · [Grama desligada](evidence/baseline/grama-desligada.png)

Na leitura independente pelo Game Designer, o personagem, o cais, as bordas de areia e o acesso à vila permaneceram legíveis nos dois quadros. A diferença na cobertura é difícil de perceber nesse ponto. Os ensaios visuais da entrega precisam incluir campo e as divisas caminho–campo e areia–talude para distinguir os perfis. Aves e ondas variam entre as duas capturas.
