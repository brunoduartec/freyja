# Validação do piloto

1. Abra `Ilha_Tripo_Caminhada_011`, use a visualização Game e entre em Play Mode.
2. Do spawn, avance pelo píer, contorne o galpão pela passagem da direita e vire à esquerda após desembarcar. Suba em direção às casas para chegar ao trecho. Observe os tufos junto à trilha, passe pelos desníveis, gire a câmera e retorne ao trecho inicial.
3. Selecione o componente Coastal Grass e compare `Off`, `All Instances` e `Optimized` no mesmo percurso. Os contadores registram a submissão. O Profiler/Frame Debugger mostra o desenho real.
4. Use câmera, resolução e equipamento fixos. Registre p50 e p95 após aquecimento para os três modos. Consulte [tarefas](tasks.md) para a evidência.

## Reaplicar à cena local

A cena já está salva no projeto Unity principal. Se for preciso reaplicar a integração em outro checkout com a cena e o terreno atuais, abra a cena, execute `Freyja > Environment > Bake Coastal Grass Pilot` e salve. A fonte editável é `Source/Blender/Grass029/CoastalGrass.blend`, exportada para FBX com unidades de arquivo e eixos Y para cima e −Z para frente. A opção de exportar a transformação nos vértices mantém a altura correta ao instanciar as malhas diretamente.
