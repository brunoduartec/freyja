# Especificação: teste de terreno e cenário modular

**Branch:** `020-teste-terreno-modular`  
**Data:** 2026-11-14  
**Estado:** aprovada pelo usuário em 2026-11-14  
**Pedido:** testar a alternativa de heightmap, texturas de grama e areia e cenário montado com pedras e alvenaria separadas.

## Cenários e avaliação

### 1. Caminhar e avaliar o trecho costeiro (P1)

O responsável pelo jogo observa um trecho com praia, cais, subida e dois lotes em alturas diferentes e percorre o local com o protagonista existente de 1,80 m. A prova deve permitir julgar relevo, escala, aparência low poly e circulação antes de reconstruir a ilha.

**Aceitação:** partindo do cais, o personagem alcança os dois lotes pelo caminho, sem saltar, atravessar o chão, ficar preso ou depender de teletransporte. Uma vista geral e uma gravação real da caminhada mostram o resultado na câmera do jogo.

### 2. Avaliar o controle da implantação (P2)

O responsável compara as dimensões planejadas dos lotes com seus blocos e verifica que terreno, materiais e peças podem ser ajustados separadamente.

**Aceitação:** os dois blocos ocupam as áreas reservadas, com frentes voltadas para seus acessos; pedras e alvenaria permanecem elementos editáveis, sem estarem fundidas ao terreno. A ilha atual continua disponível para comparação.

### Casos de borda

- Junções entre terreno, cais, caminho e módulos não podem prender o personagem ou deixar buracos no percurso.
- Paredes de contenção e pedras não podem invadir os lotes nem bloquear suas entradas.
- A base não deve ser achatada para facilitar a caminhada: os dois níveis precisam permanecer perceptíveis na câmera do jogo.

## Requisitos

- **RF-001:** Criar somente um trecho experimental com praia, cais, subida, dois lotes e dois blocos de construções; não reconstruir a ilha inteira nesta entrega.
- **RF-002:** Usar uma base de alturas editável (heightmap), com áreas planas dimensionadas antes do acabamento visual. Proposta para o teste: lotes de 5 × 6 m e 7 × 8 m, separados por 3 m de altura, conectados por caminho de pelo menos 2 m de largura livre.
- **RF-003:** Aplicar texturas de grama e areia com transições legíveis e acabamento compatível com a direção low poly e os pixels visíveis do Freyja.
- **RF-004:** Acrescentar pedras de costa/encosta e módulos de alvenaria para contenção, bordas ou pavimentação. As peças devem ser separadas e reutilizáveis; não é necessário construir cada tijolo como objeto individual.
- **RF-005:** Reutilizar o protagonista de 1,80 m e o controle/câmera existentes; validar a escala sem reduzir o personagem. O trecho deve funcionar como prova alternável na cena principal, preservando a ilha e os blocos atuais.
- **RF-006:** Salvar fontes editáveis, materiais e cena; entregar uma vista geral, pelo menos duas vistas próximas e um vídeo real do percurso entre o cais e os dois lotes.
- **RF-007:** Ao concluir, recomendar manter a base atual, adotar a alternativa ou fazer outra iteração, com base no visual, na circulação e no controle dos lotes. Apontar limitações observadas; não pressupor que a alternativa será superior antes do teste.

## Critérios de sucesso

- **CS-001:** Os dois lotes têm as dimensões propostas, com desvio máximo de 0,10 m nas bordas e na diferença de altura entre eles.
- **CS-002:** O personagem percorre cais → lote inferior → lote superior sem bloqueios e sem saltos.
- **CS-003:** Nenhum bloco invade a faixa livre do caminho; não há vãos aparentes sob suas bases nas vistas de avaliação.
- **CS-004:** As quatro evidências mínimas — vista geral, duas vistas próximas e vídeo — permitem comparar escala, relevo e acabamento com a ilha atual.
- **CS-005:** A entrega contém um parecer fundamentado e mantém o cenário anterior recuperável sem reconstrução.

## Premissas e limites

- As medidas são uma proposta de teste, não uma decisão sobre o tamanho definitivo da ilha. A inclinação e o comprimento da subida serão ajustados ao movimento já existente.
- Esta prova não inclui casas finais, interiores, toda a vila, geração de outra ilha no Tripo ou alterações da narrativa e do elenco.
- A cena principal continua sendo `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`; a implementação e a produção de peças deverão respeitar os ambientes e ferramentas estabelecidos no projeto.
- A documentação desta feature está em worktree própria para preservar o trabalho não commitado da entrega 019. A cena e os assets atuais permanecem no checkout principal; não foram alterados nesta fase.
