# Plano: teste de terreno modular

**Branch:** 020-teste-terreno-modular · **Data:** 2026-11-14 · [Spec](spec.md)

## Resumo

Criar um trecho costeiro de 48 × 56 m, com praia, cais, caminho sinuoso e dois patamares. Lote A: 5 × 6 m a 2 m; lote B: 7 × 8 m a 5 m. Terreno por heightmap, materiais de grama/areia e kit modular de pedra, alvenaria e madeira. Dois volumes de construção indicativos. Comparação com a ilha atual antes de expansão.

## Contexto técnico

Unity 6000.3.25f1 Built-in, projeto existente Freyja; Blender 5.2.2 via MCP. C# para construção no editor; Python para produção dos módulos no Blender. TerrainData 513 × 513, materiais com texturas PNG repetíveis, FBX com pivôs em metros. Sem novas dependências. Validação no editor macOS e Play Mode; vídeo MP4 por AVFoundation, como na entrega 018.

## Preservação e integração

Fontes, scripts e documentação são produzidos na worktree da branch 020. O Unity aberto no checkout principal serve como ambiente de prévia; somente os novos arquivos Terrain020 são copiados para ele. Não mover, commitar nem reorganizar alterações das entregas anteriores. O builder é a fonte reproduzível da integração na cena principal; o novo conjunto também será salvo como prefab. Cópia da cena anterior e estado da sessão permitem restauração. Novos exports e seus metadados retornam à worktree para manter os artefatos da feature reunidos.

A cena principal continua Ilha_Tripo_Caminhada_011.unity. Novo conjunto em X=200 m, ilha anterior desativada durante o teste. Alternância apenas em Edit Mode, pelo menu Freyja/Terreno 020: ativa/desativa conjuntos e troca spawn, endpoint e limites de recuperação. Controle, câmera, renderização pixelada e protagonista de 1,80 m existentes são preservados.

## Constituição

Aprovação recebida. Unity MCP e Blender MCP usados. Nenhuma alteração narrativa. Fontes editáveis fora de Assets, exports sob Assets/_Project. Evidências vinculadas às tarefas. Check inicial e pós-design: aprovado; sem contratos externos ou nova arquitetura.

## Estrutura

- Source/Blender/Terrain020/: kit editável e script de geração.
- Tools/Terrain020/: builder C#, alternância, captura e codificação.
- Freyja/Assets/_Project/Art/Terrain020/: FBX, PNG, heightmap RAW.
- Freyja/Assets/_Project/Data/Terrain020/: TerrainData, camadas e estado inicial.
- Freyja/Assets/_Project/Prefabs/Terrain020/: conjunto e peças.
- Freyja/Assets/_Project/Editor/Terrain020Preview.cs: menu de alternância.
- specs/020-teste-terreno-modular/evidence/: medidas, imagens e vídeo.

## Validação

Medir os dois lotes e desnível; amostrar apoio dos blocos; conferir corredor de pelo menos 2 m. Inspecionar vistas geral e próximas. Entrar em Play normalmente e validar inicialização/recuperação; depois dirigir o controlador existente ao longo do percurso e capturar tempos reais por quadro. Consultar Console. Parecer final distingue resultado visual, navegação e limitações. Sem build, pois não é entrega de plataforma.
