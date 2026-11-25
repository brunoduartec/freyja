# Plano do estudo de iluminação

## Contexto técnico

Unity 6000.3.25f1, Built-in, C# e shader de skybox. A cena Ilha_Tripo_Caminhada_011 usa a câmera Cais Gameplay Camera com renderização de baixa resolução e uma luz Coastal Sun já configurada. As referências visuais aprovadas são The Wind Waker HD para o céu diurno, Firewatch para a paleta horária e DREDGE para a noite costeira.

## Constituição

Usar a cena principal e as referências existentes. Validar na câmera de jogo e registrar evidências. O ciclo representa a passagem de tempo visual da sessão. As regras narrativas continuam sob os sistemas do GDD.

## Design

Usar um controlador com hora contínua e paleta interpolada, um material de céu próprio e o sol direcional existente. Integrar e salvar com a API do editor. Ajustar o limite superior da câmera na cena para enquadrar o céu. Revisar quatro horas e o ciclo completo em Play Mode.

A cópia de trabalho da branch 033 recebe uma cópia do projeto ativo para preservar a cena e seus assets locais em andamento. A implementação e os artefatos permanecem na branch 033.
