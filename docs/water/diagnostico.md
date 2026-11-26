# Diagnóstico inicial — água 079

- Unity 6000.3.25f1; projeto `Freyja/`; pipeline Built-in, GraphicsSettings sem pipeline customizado, cor linear. Manifesto sem URP. Cena principal `Ilha_Principal`.
- Câmera real `Cais Gameplay Camera`: perspectiva, FOV 58°, near 0,08, far 150, HDR e MSAA desativados; saída em `Mundo_480x270.renderTexture`, filtro point, 480×270, MSAA 1. Apresentação é preservada.
- Sistema existente `Art/Mar049/ShelteredSea.shader`: oceano opaco com ondas e mapa fixo de distância costeira; não demonstra fundo visível. Vento compartilhado expõe `_FreyjaWind` e `_FreyjaWindGust`; existe `CoastalDayNight`.
- Console da cena principal acumulava exceções de `CoastalGrass.LateUpdate` linha 81 antes das alterações de água. Não confundir com erros da prova; na cena isolada o Console atual não apresentou erros de compilação ao verificar.
- Prova isolada `WaterShowcase`: câmera perspectiva, RT 480×270 e componente que solicita depth/depth normals; praia de 32×32 m, fundo e controles simples. Geometria e inclinação da cena principal, preço de depth normals e desempenho permanecem a medir.
- Estado Git inicial: main quatro commits à frente de origin/main, anexos não rastreados. Branch 079 criada preservando esses itens. No diagnóstico inicial, a cena principal não foi alterada; a integração provisória autorizada depois é documentada em `specs/079-sistema-agua/integracao.md`, junto das alterações preexistentes preservadas.
