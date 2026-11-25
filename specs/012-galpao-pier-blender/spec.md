# Galpão com píer no Blender

**Branch** `012-galpao-pier-blender`  
**Data** 2026-11-13  
**Estado** Modelo entregue para avaliação

## Cenário e teste

Como responsável pela arte de Freyja, quero abrir no Blender um modelo editável do galpão com píer e avaliá-lo ao lado da [imagem de referência](../../Referencias/Construcoes-TripoAI/11-cais-pier-e-galpao.png).

1. Ao abrir o arquivo, consigo selecionar e editar separadamente o galpão e o píer.
2. Ao comparar a imagem renderizada com a referência, reconheço o telhado de duas águas, a baia frontal aberta e o píer de tábuas apoiado em estacas, ligado ao galpão pelo patamar.

### Partes encobertas

Completar as faces traseiras e inferiores com a mesma estrutura e os mesmos materiais das faces visíveis. A vista aérea da ilha serve de contexto para a ligação do píer com a margem.

## Requisitos

- **FR-001** Modelar pelo MCP do Blender um galpão low poly com paredes de pedra clara, telhado de terracota, vigas de madeira e vão frontal aberto.
- **FR-002** Modelar o píer com deck de tábuas, estacas quadradas e uma ligação lateral curta até o patamar do galpão. A superfície de passagem deve ser contínua.
- **FR-003** Entregar um `.blend` editável com materiais no próprio arquivo e uma imagem em três quartos para avaliação aqui no chat. O modelo representa apenas o conjunto do galpão e do píer.
- **FR-004** Salvar um export para importação no Unity em `Freyja/Assets/_Project/Art/`. A avaliação é do modelo isolado.

## Critérios de sucesso

- **SC-001** O arquivo salvo reabre no Blender com as duas partes editáveis e os materiais disponíveis.
- **SC-002** A imagem mostra todo o modelo e permite conferir a abertura frontal, o telhado e a ligação entre o píer e o galpão.
- **SC-003** A passagem do píer ao galpão é contínua e a abertura frontal tem altura livre para o protagonista de 1,80 m.

## Premissas

- “Casa com ponte” refere-se ao galpão com píer mostrado na imagem 11.
- A imagem 11 orienta a aparência. As medidas finais serão propostas em metros, usando o protagonista de 1,80 m como referência de escala.
- Fonte e imagem de revisão ficam em `Source/Blender/GalpaoPier/`. A colocação do modelo na cena da ilha pertence a outra revisão.
