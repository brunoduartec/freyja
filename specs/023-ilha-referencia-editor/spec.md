# Ilha da referência no editor
**Branch:** 023-ilha-referencia-editor
**Data:** 2026-11-17
**Estado:** implementada e em avaliação visual

## Objetivo
Recriar a composição de specs/017-referencia-terreno-tripo/vila-casa-isolada.png na cena principal usando terreno editável, kit PS1 e construções modulares substituíveis. Preservar as versões anteriores.

## Cenários e aceite
1. Comparar uma vista geral no mesmo ângulo: reconhecer enseada e praia na frente, cais à esquerda, praça central, igreja elevada, casa do velho isolada à esquerda e crista rochosa ao fundo.
2. Percorrer cais–praça–igreja e o acesso à casa isolada com o personagem de 1,80 m, sem quedas nem bloqueios.
3. Selecionar e substituir uma construção mantendo implantação e apoio; editar materiais e objetos com o kit existente.

## Requisitos
- Recriar costa orgânica, praia ampla, encosta com patamares, curvas dos caminhos e rochas; evitar repetir a ilha quadrada do teste.
- Implantar 12 habitações, incluindo a pequena casa isolada do velho, mais igreja, venda, loja/oficina, sala comunitária e galpão. Casas e locais de trabalho seguem o inventário 016; não inventar novos moradores ou atribuir identidade ao velho.
- A referência é conceitual e não mostra fielmente toda a contagem: completar os locais necessários mantendo sua composição; documentar adaptações.
- Usar construções provisórias reconhecíveis por silhueta: telhados, alturas variadas, frentes e torre da igreja. Sem interiores ou modelos finais nesta entrega.
- Escala física pelas casas e personagem; dimensões, rotações e cotas registradas. Tamanho total ajustado ao programa, sem herdar automaticamente 64, 100 ou 160 metros dos testes.
- Texturas de 32/64 pixels e transições pixeladas do kit 022; árvores da biblioteca local já indicada. Nenhuma geração 3D externa.
- Manter circulação coerente com relevo; rampas, patamares e escadas apenas onde fizer sentido, conforme a orientação mais recente do usuário.
- Trabalhar na cena Ilha_Tripo_Caminhada_011 com grupo separado e estado anterior recuperável. Não remodelar a base Tripo para forçar encaixe.

## Critérios de sucesso
- 17 construções identificadas e substituíveis, com posição, dimensões e orientação documentadas.
- Todos os marcos visuais do cenário 1 presentes; comparação lado a lado e desvios registrados, sem alegar precisão de medidas extraídas da imagem.
- Percursos principais e chegada às portas das 17 construções verificados em Play Mode. Requisito ampliado explicitamente pelo usuário durante a validação.
- Capturas geral, cais/praia, centro/igreja e casa isolada, além de vídeo do cais ao centro.

## Casos de borda e limites
Completar casas omitidas pela imagem sem lotação excessiva; evitar fundações suspensas, praia ocupada por construções e caminhos muito inclinados. Preservar pintura e objetos manuais durante reconstrução. Ruína, hortas e mirante podem ter áreas reservadas, sem expandir para novos sistemas narrativos. Aprovação desta spec necessária antes do plano e implementação conforme AGENTS.md.

## Aprovação
Aprovada explicitamente pelo usuário: “sim”, em 17/11/2026.

## Ajuste durante a entrega
O usuário forneceu a prancha `TripoAi/cenario/referencias.png` e os onze GLBs numerados. As dez construções foram encaixadas como novos modelos visuais dos lotes existentes, e o píer recebido substituiu o píer provisório. A casa do velho e os usos sem peça própria permanecem com volumes prévios.
