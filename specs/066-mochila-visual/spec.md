# Especificação — Mochila do protagonista

**Branch** `066-mochila-visual`  
**Data** 2026-11-19  
**Status** Aguardando avaliação  
**Base** Inventário e objetos coletáveis da spec 065.

## Cenário e teste

### P1 — Abrir a mochila para conferir os objetos

Durante a exploração, o jogador pressiona **I** e vê a mochila do protagonista aberta, em destaque, com a ilha escurecida ao fundo. A abertura deixa visíveis o forro e o compartimento principal. O jogador reconhece os objetos pelas silhuetas e confere nome e quantidade pelas etiquetas. **I**, **Esc** ou o botão de fechar devolvem o controle.

1. Em uma sessão nova, abrir a mochila mostra o interior vazio e o texto “Nenhum item guardado”.
2. Após coletar dois carretéis e uma boia, abrir a mochila mostra dois tipos distinguíveis, com nome e quantidade corretos.
3. Fechar, coletar outro objeto disponível e reabrir atualiza o conteúdo. Com a mochila aberta, a exploração fica suspensa.

## Requisitos

- **FR-001** A interface deve representar a mochila já usada pelo protagonista. A silhueta, a cor externa e os fechos servem de referência para o desenho original do interior aberto.
- **FR-002** Apresentar a mochila em vista de cima levemente inclinada, com lona texturizada, costuras visíveis e a direção low poly pixelada de Freyja. Usar a ilha escurecida como fundo.
- **FR-003** Representar cada tipo guardado com uma imagem reconhecível do objeto e uma etiqueta legível com nome e quantidade. Os objetos devem parecer apoiados no forro.
- **FR-004** Exibir estado vazio e acomodar mais objetos com rolagem quando necessário. A área ilustrada não representa limite de capacidade.
- **FR-005** Preservar os controles de coleta, consulta e retorno à exploração da spec 065.

### Entidades

- **Mochila aberta** Representação visual do inventário da sessão.
- **Objeto guardado** Tipo de item com imagem, nome e quantidade.

## Critérios de sucesso

- **SC-001** Entregar duas capturas da Game View a 1280 × 720, uma da mochila vazia e outra com a boia ×1 e o carretel ×2.
- **SC-002** Em Play Mode a 1280 × 720 e 1920 × 1080, a silhueta da mochila, as duas imagens dos objetos, os nomes, as quantidades e a opção de fechar ficam visíveis sem sobreposição.
- **SC-003** Verificar em Play Mode o estado vazio, a atualização após a coleta, a reabertura com o conteúdo da sessão e o retorno ao jogo por I e Esc.

## Premissas

- A primeira versão cobre apresentação e consulta do conteúdo. Animação do protagonista, uso de objetos e missões ficam para escopos próprios.
- Paleta inicial proposta: exterior verde-oliva a partir da mochila atual, forro claro e etiquetas em papel cru.

## Referências visuais

- **Zniw Adventure** — A mochila aberta vira a moldura do inventário. É a referência mais próxima para a relação entre a silhueta da bolsa e os objetos guardados. [Manual, p. 20](https://cdn.cloudflare.steamstatic.com/steam/apps/904750/manuals/manualEn.pdf#page=20)
- **Sons of the Forest** — A vista de cima e a organização sobre tecido dão a sensação de consultar objetos físicos. Para Freyja, a proposta usa o forro da própria mochila como superfície. [Capturas da mochila em jogo](https://sonsoftheforest.wiki.gg/wiki/Backpack)
- **Resident Evil 4** — A maleta é um exemplo de como dar a cada objeto uma silhueta distinta e um lugar fácil de encontrar. [Manual ilustrado da Capcom](https://static.capcom.com/manuals/re4/RE4_PS2_DMNL_EN.pdf)
