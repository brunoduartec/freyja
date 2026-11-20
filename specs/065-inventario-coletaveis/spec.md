# Especificação — Inventário e itens coletáveis

**Branch** `065-inventario-coletaveis`  
**Criada em** 2026-11-19  
**Status** Aprovada  
**Pedido** Guardar objetos que poderão servir a pequenas missões de investigação e colocar itens na ilha para testar a coleta.

## Cenários e testes

### P1 — Pegar um objeto na ilha

O jogador encontra um objeto no percurso, aproxima-se e usa o controle de interação já adotado. A coleta confirma o nome do objeto, guarda uma unidade e retira o objeto da cena.

1. Perto de um item disponível, a indicação de coleta identifica o objeto e o controle. Ao interagir, o jogador recebe a confirmação e o objeto desaparece.
2. Fora do alcance, a interação não coleta o item. Repetir o comando depois de pegá-lo não duplica a unidade.
3. Perto de um morador e de um objeto, a indicação corresponde à ação executada. Uma única interação não inicia conversa e coleta ao mesmo tempo.

### P2 — Consultar o que foi guardado

O jogador abre o inventário durante a exploração, confere os itens e volta ao jogo pelo mesmo controle.

1. Antes da primeira coleta, o painel apresenta um estado vazio.
2. Após a coleta, o painel mostra os nomes e as quantidades. Fechar e reabrir o painel conserva o conteúdo da sessão.
3. Duas unidades do mesmo tipo aparecem numa entrada com quantidade 2. Ao iniciar uma sessão nova, o inventário começa vazio e os objetos voltam às posições de teste.

## Requisitos

- **FR-001** Disponibilizar três objetos de teste no percurso da cena principal da ilha, em pontos acessíveis entre o cais e a praça. Usar dois tipos de item para testar nomes distintos e agrupamento.
- **FR-002** Usar a interação existente para coletar. Exibir indicação quando o objeto for o alvo disponível e confirmar a coleta pelo nome.
- **FR-003** Registrar cada objeto da cena uma única vez por sessão. Remover o objeto do mundo após registrá-lo.
- **FR-004** Permitir abrir e fechar o inventário durante a exploração e consultar o conteúdo em português.
- **FR-005** Identificar cada tipo de item e cada objeto coletável de forma estável, independentemente do nome exibido. Manter os itens físicos separados do registro narrativo de pistas.
- **FR-006** Preservar a interação com moradores e evitar ações simultâneas entre coleta, diálogo e consulta do inventário.

### Entidades

- **Tipo de item** Identidade estável e nome exibido.
- **Objeto coletável** Unidade de um tipo de item colocada em um ponto da ilha.
- **Inventário da sessão** Tipos e quantidades guardados pelo protagonista.

## Critérios de sucesso

- **SC-001** Os três objetos podem ser encontrados e coletados na cena principal. Fazer a coleta em duas ordens diferentes produz a mesma lista de dois tipos, com quantidades 2 e 1.
- **SC-002** Três tentativas consecutivas de repetir uma coleta deixam a contagem igual. Fechar e reabrir o painel mantém as quantidades e iniciar nova sessão restaura o estado inicial.
- **SC-003** No percurso de teste, é possível coletar um objeto, conferir o inventário e conversar com um morador. Cada ação acontece somente quando indicada.

## Premissas

- Objetos provisórios para avaliação visual e funcional. A proposta é usar dois carretéis de linha e uma boia de pesca.
- A primeira entrega cobre coleta, armazenamento na sessão e consulta. Entrega a NPCs, novas missões, salvamento entre sessões, uso e descarte de itens ficam para entregas posteriores.
