# Imagem do relevo para o TripoAI

**Criada em** 2026-11-13  
**Estado** Aguardando aprovação da proposta  
**Pasta** `specs/006-imagem-relevo-tripo`

## Cenário e teste

Como responsável pela modelagem, quero uma imagem do terreno isolado da Ilha dos Trinta para usá-la como entrada no TripoAI. Na comparação lado a lado com a [vista aérea oficial](../../Referencias/modelagem-vista-aerea.png), a ilha deve manter os volumes reconhecíveis e revelar o terreno sob as casas.

## Requisitos

- **FR-001** Editar a vista aérea oficial, mantendo a perspectiva oblíqua, a silhueta costeira, a praia em arco, os patamares do povoado e das hortas e o maciço rochoso posterior com suas falésias.
- **FR-002** Remover casas, igreja, ruína, poço, cercas, cais, barco, árvores e arbustos. Reconstituir as áreas encobertas com continuidade de altura, inclinação e material em relação ao terreno vizinho. Preservar os desníveis dos terraços.
- **FR-003** Entregar um PNG com a ilha inteira, em estilo low poly, isolada sobre fundo liso e com iluminação que destaque os desníveis.

## Critérios de sucesso

- **SC-001** A comparação com a referência confirma a posição relativa da praia, dos patamares e das falésias, além da forma da costa.
- **SC-002** A inspeção do PNG confirma a ausência de construções e objetos soltos, o contorno inteiro no quadro e a continuidade das superfícies nas áreas editadas.

## Premissas

- A vista aérea oficial tem prioridade, conforme a decisão registrada na [spec 005](../005-relevo-fiel-referencia/spec.md).
- As superfícies hoje encobertas serão inferidas a partir do relevo visível ao redor. Fundo liso e remoção dos objetos de cena são escolhas propostas para facilitar o uso da imagem no TripoAI.
- A entrega termina no PNG. O envio e a geração do modelo no TripoAI ficam a cargo do usuário.
