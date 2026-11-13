# Referências das construções para o TripoAI

**Criada em** 2026-11-13  
**Estado** Imagens geradas e conferidas

## Cenários e testes

### Usar cada construção como referência individual

Como responsável pela modelagem, quero abrir uma imagem por construção para gerar os modelos separadamente no TripoAI.

**Teste independente** Comparar cada arquivo com sua posição e aparência na [vista aérea oficial](../../Referencias/modelagem-vista-aerea.png).

1. Ao abrir qualquer imagem, a construção correspondente aparece inteira, centralizada e isolada.
2. Ao abrir a imagem do cais, o píer de madeira e o galpão aparecem juntos, com a posição relativa da referência.

### Casos de borda

- Quando árvores ou terreno encobrirem parte de uma construção, completar apenas o necessário para fechar sua silhueta, seguindo as formas e os materiais visíveis.
- Diferenciar casas parecidas pela volumetria, fachada e posição na vista aérea. Cada uma recebe um arquivo próprio.

## Requisitos

- **FR-001** Entregar 11 PNGs individuais com nomes que indiquem a construção e, no caso das casas, sua posição na vista aérea.
- **FR-002** Preservar as proporções aparentes, os telhados, as aberturas, os materiais e o estilo low poly da referência. Usar vista em três quartos, com fachada, lateral e cobertura legíveis.
- **FR-003** Enquadrar cada construção inteira com margem, fundo neutro e iluminação uniforme. O quadro contém somente a construção, com o píer e o galpão juntos no caso do cais.

### Inventário

| Nº | Construção na vista aérea |
| --- | --- |
| 01 | Sobrado com varanda no alto à esquerda |
| 02 | Casa no alto, logo atrás e à direita do sobrado |
| 03 | Casa à esquerda da igreja, junto ao largo |
| 04 | Igreja com campanário |
| 05 | Casa à direita da igreja, junto ao largo |
| 06 | Casa isolada no terraço superior direito |
| 07 | Casa ao lado da escada das hortas |
| 08 | Casa do terraço baixo à esquerda do caminho principal |
| 09 | Casa no terraço intermediário à direita, com duas chaminés |
| 10 | Casa no terraço baixo à direita, acima do galpão |
| 11 | Cais com píer de madeira e galpão |

## Critérios de sucesso

- **SC-001** O conjunto contém exatamente 11 imagens separadas, com correspondência individual ao inventário.
- **SC-002** Em 11 de 11 imagens, a silhueta está inteira e os elementos visíveis que distinguem a construção são reconhecíveis lado a lado com a vista aérea.

## Premissas

- A contagem segue o pedido e o inventário visual. São nove casas, a igreja e o conjunto do cais. A ruína, o poço e o barco ficam fora da seleção.
- Fundo neutro e vista em três quartos são escolhas propostas para facilitar a leitura das formas na imagem.
- A entrega consiste nas imagens para uso no TripoAI.
