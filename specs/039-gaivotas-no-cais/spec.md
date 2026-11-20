# Sobrevoo diurno das gaivotas no cais

Branch `039-gaivotas-no-cais` · Aprovada para implementação

## Cenários de avaliação

Na chegada ao píer, o jogador pode notar gaivotas sobre a enseada pela câmera normal de exploração. Ao voltar ao cais durante o dia, pode encontrar o grupo novamente. Entre passagens, as aves podem sair do enquadramento.

## Requisitos

- FR-001. Testar três gaivotas ao redor do píer na cena principal `Ilha_Tripo_Caminhada_011`, usando a silhueta da ilustração aprovada como referência.
- FR-002. Distribuir as aves em três órbitas a alturas e distâncias diferentes, com passagens pelos dois lados do píer e acima dos obstáculos nas extremidades. Preservar a leitura da rota de desembarque.
- FR-003. Manter os sobrevoos disponíveis durante todo o período diurno, inclusive nas revisitas, e suspendê-los nos períodos noturnos que a cena já oferecer.
- FR-004. Registrar um trecho curto do teste pela câmera do jogador para avaliação visual.

## Critérios de sucesso

- CS-001. Em Play Mode, pelo menos uma ave cruza o enquadramento em até 20 segundos tanto na chegada ao píer quanto no retorno diurno, olhando naturalmente para a enseada.
- CS-002. No trecho registrado é possível reconhecer uma gaivota pela silhueta e distinguir o caminho de desembarque durante o sobrevoo.
- CS-003. Se houver período noturno disponível, verificar a ausência de sobrevoos nele.

## Premissas

Três aves e 20 segundos são parâmetros iniciais de avaliação. O teste admite arte provisória e cobre sobrevoo ambiente. Som, interação e criação de um ciclo dia e noite ficam fora desta etapa.
