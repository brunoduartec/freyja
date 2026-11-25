# Especificação — Limpeza da cena principal da ilha

**Branch** `076-limpar-cena-principal`  
**Data** 2026-11-25  
**Status** Aprovada em 2026-11-25

## Cenários e testes

### US1 — Trabalhar em uma única ilha jogável (P1)

A cena atual passa a conter os objetos da ilha usada no jogo. Testes antigos, demos e versões substituídas deixam de ocupar sua hierarquia.

**Teste independente** — Conferir o inventário por objeto junto à hierarquia final. Cada remoção tem origem ou função identificada, evidência de substituição ou obsolescência e verificação de dependências.

1. Dado um conjunto de teste ou cenário antigo confirmado como descartável, ao abrir a cena limpa, seus objetos estão ausentes.
2. Dado um objeto compartilhado, desativado intencionalmente ou usado em tempo de jogo, ele continua disponível e funcional. Dúvidas de função ou dependência ficam registradas antes de qualquer remoção.
3. Ao entrar no jogo, a câmera mostra um único cenário jogável. A chegada, o percurso do píer ao largo, o HUD, as interações existentes e a travessia de escadas do percurso continuam acessíveis.

### US2 — Abrir a cena pelo nome definitivo (P1)

A equipe encontra e abre a cena oficial como **Ilha_Principal**.

**Teste independente** — Abrir a cena pelo novo nome e conferir que as ferramentas e entradas que precisam da ilha principal resolvem o novo caminho.

## Requisitos

- **FR-001** Inventariar todos os objetos da cena atual, inclusive filhos de prefabs e objetos inativos. Registrar o destino de cada objeto, agrupando descendentes somente quando compartilham a decisão e o motivo.
- **FR-002** Conferir origem, finalidade e referências antes de remover objetos. Revisar cópias mesmo que estejam ativas e distinguir objetos de prévia do cenário visual, colisões, sistemas e agrupamentos usados no jogo.
- **FR-003** Remover da cena principal as instâncias confirmadas como pertencentes a testes, demos ou cenários substituídos. As demais cenas e os arquivos de origem dos modelos ficam fora do escopo de exclusão.
- **FR-004** Renomear a cena existente para `Ilha_Principal.unity`, preservando sua identidade e atualizando referências operacionais ao nome ou caminho, inclusive em ferramentas de autoria e configurações de build que apontem para ela.
- **FR-005** Verificar a hierarquia, as referências, o Console e o comportamento em Play Mode pela câmera real do jogo.

## Critérios de sucesso

- **SC-001** 100% dos objetos inventariados têm uma decisão registrada, com justificativa para cada remoção.
- **SC-002** A cena contém zero cópias de cenário ou conjuntos de testes confirmados como obsoletos. A limpeza causa zero referências quebradas ou novos erros no Console.
- **SC-003** A cena abre como `Ilha_Principal` e passa pelo teste de chegada, caminhada até o largo, HUD, escadas e interações existentes.

## Evidência inicial e premissas

- O Editor mostrou somente `Ilha_Tripo_Caminhada_011` carregada, ativa e sem alterações pendentes. A hierarquia retornou 24 raízes e 3.432 objetos.
- O inventário inicial apontou oito raízes desativadas, somando 395 objetos: `Cenario`, `Cenario_Colisao`, `Galpão do cais e píer`, `Ilha Tripo 018 - Teste`, `Blocos 019 - Implantacao`, `Terreno modular 020 - Teste`, `World Builder — DemoWorld` e `World Builder — Praia`. São candidatas, sujeitas à conferência de dependências.
- `Island023` concentra a ilha ativa. Há descendentes desativados em grupos de construções, caminhos e colisões, além de objetos de apoio em outras raízes. A decisão de remoção depende do uso de cada objeto.
- `Ilha_Principal` é o nome proposto para a cena definitiva.
