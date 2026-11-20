# Plano: blocos na ilha nova

**Branch:** 019-blocos-ilha-nova · [Spec](spec.md)

## Contexto e preservação

Unity 6000.3.25f1 Built-in, cena principal Ilha_Tripo_Caminhada_011; ilha Tripo 018 com largura experimental de 100 m e protagonista de 1,80 m. Há alterações não salvas na cena ao iniciar; inspecionar e preservar o estado, sem recarregar ou descartar trabalho do usuário. Fonte de implantação: imagem `specs/017-referencia-terreno-tripo/vila-casa-isolada.png` e inventário 016. Ângulos e proporções são estimativas, pois imagem não é levantamento métrico.

## Processo

1. Inspecionar cena e terreno; registrar estado inicial e comparar lotes com a imagem e a planta.
2. Posicionar 12 moradias individuais, uma delas pequena e afastada; igreja, duas lojas, sala e galpão; diferenciar blocos por cor e marcar frentes. Estimar dimensões humanas e assentar no terreno sem mover estruturas existentes.
3. Medir posição, dimensões e ângulos, registrar conflitos de encaixe e acessos. Se lotes forem insuficientes, reportar sem deformar ou reduzir o personagem.
4. Salvar a cena preservando alterações atuais, gerar vista geral e ao nível do personagem. Em Play Mode verificar caminhada entre cais e centro, acessos e Console; documentar limitações.

## Constituição

Atende aos princípios: Unity via MCP, informações provisórias identificadas, 30 moradores e 12 casas mantidos; não altera narrativa e requer evidência. Fonte e prefab adicionais não são necessários para volumes Unity. Não há interface externa.
