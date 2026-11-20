# Diversidade visual dos moradores

**Criada em** 2026-11-13  
**Estado** Conjunto aprovado e 16 poses substituídas em `Referencias/Poses-Moradores/` em 2026-11-19. Pranchas de comparação em [Batista, Neves e Costa](revisao/batista-neves-costa.png) e [Pires, Seixas e Gomes](revisao/pires-seixas-gomes.png).

## Cenários e testes

### Reconhecer um elenco mais diverso com a mesma direção de arte

Ao comparar as novas poses com as referências atuais, a equipe deve reconhecer os mesmos moradores, com suas idades, roupas e silhuetas, em um elenco que inclui famílias negras e de ascendência leste-asiática.

**Teste independente** Comparar cada nova imagem com sua versão anterior e com os moradores das outras casas, lado a lado e na mesma escala.

1. Cada morador das seis casas selecionadas aparece em sua própria imagem, de corpo inteiro, em pose A e com o mesmo tratamento visual do conjunto.
2. Na comparação por família, os parentes têm semelhanças naturais, com variações individuais de feições, cabelo e tom de pele.

### Casos de borda

- O [GDD](../../GDD.md) agrupa 30 moradores em 12 casas, incluindo a casa de Cecília. Três retratos ficam em `Poses-Cais`. A seleção proposta cobre seis casas com todos os retratos já em `Poses-Moradores`.
- Artur e Raul são irmãos, assim como Teresa e Amélia são irmãs. A seleção preserva as referências das duas casas de cada par.

## Requisitos

- **FR-001** Recriar e substituir os 16 PNGs das seis casas propostas, nos caminhos e nomes atuais de [Poses-Moradores](../../Referencias/Poses-Moradores/).
- **FR-002** Preservar pose A, enquadramento, fundo, iluminação, acabamento low poly, proporções, faixa etária, figurino e elementos de identificação, incluindo a tira azul de Ivo e a gravidez visível de Helena.
- **FR-003** Representar moradores negros e de ascendência leste-asiática com variação natural de pele, feições e cabelo. Redesenhar a estrutura do rosto de cada personagem, trabalhando os volumes de nariz, pálpebras, maçãs do rosto ou mandíbula conforme seu desenho individual. Preservar o vestuário cotidiano e evitar feições caricatas, adereços usados como marcadores étnicos ou um molde facial repetido por família.
- **FR-004** Apresentar uma comparação visual da primeira família para avaliação antes de ampliar para as demais. Após a avaliação do conjunto, substituir as imagens aprovadas.

## Critérios de sucesso

- **SC-001** Seis das 12 casas têm todas as poses revisadas, totalizando 16 dos 30 moradores. As 30 referências individuais continuam disponíveis nos caminhos originais.
- **SC-002** Em 16 de 16 revisões, a comparação confirma que o corpo aparece inteiro, a pose e o figurino correspondem ao original e a estilização é coerente com o restante do elenco.
- **SC-003** A revisão visual por casa confirma a variação individual e os critérios de representação do FR-003.

## Premissas

- **Famílias negras** — Batista (A3, primeira amostra), Neves (A1) e Costa (L2).
- **Famílias de ascendência leste-asiática** — Pires (B2), Seixas (A4) e Gomes (L3).
- O escopo desta entrega são as imagens de referência. Nomes, idades, parentescos, funções e história seguem o GDD.
