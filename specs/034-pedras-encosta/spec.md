# Especificação — Pedras da encosta

**Branch** `034-pedras-encosta`  
**Criada em** 18/11/2026  
**Estado** Proposta aguardando aprovação  
**Pedido** Aproximar as pedras da encosta da referência visual da vila.

## Cenários de uso e teste

### P1 — Reconhecer os afloramentos da referência na subida do cais

O jogador vê rochedos que parecem sair da encosta e acompanha o caminho do cais até a praça. O piloto cobre a encosta oeste, junto à primeira curva da subida. Esse trecho reúne costa, talude gramado e caminho em uma área que dá para rever de perto.

1. Vista da costa, a encosta apresenta um grupo alto e inclinado, com blocos parcialmente enterrados, alturas diferentes e interrupções de terra ou vegetação.
2. Vista do caminho, a rocha baixa se integra à grama, enquanto a rota, o galpão e a faixa de areia continuam legíveis.

### Casos de borda

- Na junção com o terreno, a base dos rochedos aparenta contato contínuo com o solo. A água pode cobrir a base das peças costeiras.
- Perto da curva, a passagem pode ser percorrida nas duas direções sem travar ou subir nas pedras involuntariamente.

## Requisitos

- **FR-001** — Introduzir dois perfis no trecho piloto, uma formação costeira alta e um afloramento baixo em terra, seguindo a composição da [referência da vila](../017-referencia-terreno-tripo/vila-casa-isolada.png).
- **FR-002** — Variar altura, silhueta e espaçamento dos blocos, com faces facetadas em tons quentes de pedra e vegetação pontual nas transições.
- **FR-003** — Preservar as passagens, as entradas dos edifícios e a faixa livre de areia no trecho.

## Critérios de sucesso

- **SC-001** — Duas comparações antes/depois, uma vista geral e uma da câmera jogável, permitem conferir os dois perfis, os intervalos e o encaixe no terreno.
- **SC-002** — Uma ida e volta entre o cais e a praça em jogo, incluindo a curva do piloto e a passagem lateral do galpão, termina sem bloqueios novos.

## Premissas

- A imagem da vila usada nas specs 028 e 030 orienta forma, distribuição e paleta. A escala é aferida na cena atual.
- O trecho oeste é uma proposta para a primeira avaliação visual. A revisão da crista, da costa leste, do cais construído e dos muros de contenção fica fora deste piloto.
- A [análise técnica](analise.md) registra a comparação e a recomendação de produção para avaliação.
