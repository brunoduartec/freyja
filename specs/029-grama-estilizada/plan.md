# Plano de implementação da grama costeira

**Branch** `029-grama-estilizada` · **Data** 2026-11-18 · [Spec aprovada](spec.md)

## Escopo e contexto técnico

Piloto na subida do cais da cena `Ilha_Tripo_Caminhada_011`, Unity 6000.3.25f1 Built-in, C# e shader de vértice/fragmento. A câmera existente renderiza a 480×270. Referência de validação local em macOS com Apple M4 Pro e Metal. Meta inicial de até +10% no p95 do quadro diante da cena sem grama.

## Constituição

O Unity MCP valida a cena e o Blender MCP produz os tufos originais. A comparação usa a câmera do jogo e preserva a leitura dos caminhos. A implementação é decorativa e o estado de investigação fica fora da entrega. A evidência está nos arquivos, capturas e relatório da spec.

## Estrutura

- `Source/Blender/Grass029/` guarda a fonte editável.
- `Freyja/Assets/_Project/Art/Grass029/` guarda FBX, material, shader e distribuição.
- `Freyja/Assets/_Project/Scripts/Environment/CoastalGrass.cs` e `CoastalGrassLayout.cs` desenham lotes de células na câmera do jogo.
- `Freyja/Assets/_Project/Editor/CoastalGrassAuthoring.cs` prepara a distribuição a partir das superfícies existentes e integra a cena.
- A cena principal e dependências ainda existem como alterações locais no projeto aberto. Aplicar somente a integração da grama na cena existente, preservando as alterações prévias. Os assets e o código da entrega ficam também na worktree da feature.

Os detalhes e alternativas estão em [pesquisa](research.md), o contrato interno está no [modelo de dados](data-model.md) e os passos de validação estão no [guia](quickstart.md).
