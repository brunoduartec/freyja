# Chegada dos três visitantes ao píer

**Branch** `052-chegada-pier`  
**Data** 2026-11-19  
**Estado** Proposta para aprovação

## Cenário de uso e teste

O jogador vê os três visitantes desembarcarem do bote azul. Três moradores da referência já estão no píer. O primeiro encontro termina com o protagonista sob controle do jogador. A entrega é um trecho curto, revisável em uma única passagem pela cena.

- Partindo do bote junto ao píer, os três visitantes chegam ao piso de madeira e se distinguem dos três moradores.
- Ao terminar o encontro, a câmera de exploração enquadra o protagonista e o caminho de saída permanece livre.
- Durante o balanço do barco, os personagens mantêm apoio coerente na transição entre casco e píer. Os moradores aguardam fora da passagem.

## Requisitos

- **FR-001** Reaproveitar a cena principal e o elenco dos visitantes, o bote azul, o píer, o galpão, o mar e as gaivotas.
- **FR-002** Produzir os três moradores com figurino e silhuetas da referência, incluindo a faixa do pai, com materiais e articulação para poses e movimento.
- **FR-003** Mostrar a saída do bote e a reação dos moradores com movimentos legíveis na câmera do jogo. Incluir amarra visível se o enquadramento mostrar a atracação.
- **FR-004** Entregar o primeiro encontro e a passagem ao controle do jogador. A proposta começa com o bote já encostado. A aproximação em mar aberto, dublagem e investigação posterior ficam fora deste recorte.

## Critérios de sucesso

- **CS-001** Uma captura da chegada mostra os três visitantes e os três moradores distinguíveis. A faixa é legível no enquadramento escolhido.
- **CS-002** Em uma passagem completa, os três visitantes saem do bote com apoio coerente e sem atravessar o casco ou o píer. Ao final, o jogador consegue deixar o cais.
- **CS-003** A câmera mantém rostos e silhuetas legíveis com o visual pixelado adotado pelo projeto.

## Inventário de produção

| Item | Situação |
| --- | --- |
| Protagonista, amiga de coral e amigo de verde | Modelos existentes. Reaproveitar caminhada e repouso onde couber. Conferir poses a bordo e clipes de saída do bote. |
| Pai com faixa, mãe e filha | Modelos 3D correspondentes não localizados. O projeto tem referências da família B2 em `specs/010-diversidade-visual-moradores/revisao/B2/`. Validar se é a família pretendida antes de modelar. |
| Píer, galpão, bote azul, barco do pescador, mar e gaivotas | Presentes na cena principal. As entregas 048–051 registram balanço dos cascos, ondulação e iluminação do píer. |
| Desembarque e reação | Prever pose a bordo, passagem do casco ao piso, caminhada curta, espera e reação. Conferir o desnível antes de decidir se um apoio físico deve aparecer. |
| Amarração e efeitos | Conferir cabo, ponto de amarração e necessidade de espuma local pelo enquadramento. Esteira de navegação depende de mostrar a aproximação. |
| Som | Inventariar e, quando necessário, obter ondas, vento, rangido da madeira/casco, passos e gaivotas. Motor e água de deslocamento dependem de mostrar o bote navegando. |
| Câmera e texto | Preparar plano de chegada, enquadramento do encontro, entrega do controle e texto curto se houver fala. |

## Premissas para aprovação

O GDD atual descreve a chegada sem aviso e Artur vendo o grupo depois do desembarque. Três moradores já no píer podem estar ali por rotina e notar o grupo ao encostar. Se já esperavam especificamente os visitantes, a cena altera essa premissa do GDD. Artur já tem modelo próprio. O GDD associa a tira azul à escolha pública de um morador, então o significado da faixa do pai e a identidade do trio precisam ser confirmados antes da produção.
