# Especificação: direção de renderização low poly pixelada

**Branch**: `014-renderer-pixelado`  
**Criada**: 2026-11-14  
**Estado**: Rascunho para aprovação

## Cenários e validação

### 1. Escolher a aparência (P1)

Como responsável pela direção de Freyja, quero comparar referências e técnicas e avaliar uma imagem da cena do jogo, para escolher uma aparência low poly com pixels visíveis que preserve a identidade da ilha e a leitura da investigação.

**Teste independente**: examinar o estudo e a imagem na proporção de apresentação, verificando cenário, personagem e um detalhe relevante para investigação.

**Aceitação**:

1. O estudo compara alternativas com referências identificadas, benefícios, limitações e justificativa para Freyja, distinguindo fatos das fontes de conclusões próprias.
2. A imagem mostra a proposta na câmera da cena principal existente, com personagem e detalhe de cenário ou pista reconhecíveis.
3. A avaliação registra legibilidade e possíveis cintilações ou distorções em movimento e diferentes tamanhos de tela, com evidência ou limitação explícita.

### 2. Reproduzir a direção (P2)

Como responsável por futuras cenas e assets, quero uma definição prática da técnica escolhida, para produzir imagens coerentes com a referência aprovada.

**Teste independente**: seguir as instruções documentadas e verificar se seus parâmetros, contexto e passos permitem reproduzir a imagem entregue.

**Aceitação**: a definição identifica configuração, tratamento de texturas e apresentação, orientações para assets, passos de reprodução, verificação e limitações conhecidas; decisões ainda não aprovadas são identificadas como propostas.

### Casos de atenção

- Detalhes pequenos, rostos e pistas podem desaparecer na resolução escolhida; registrar compromissos e ajustes necessários.
- Movimento pode causar cintilação ou perda de estabilidade invisível em imagem estática.
- Proporções e resoluções de saída diferentes podem produzir pixels irregulares, recorte ou faixas; documentar o comportamento escolhido.
- Interface e texto precisam permanecer legíveis; registrar seu tratamento sem exigir reformulação completa da interface.

## Requisitos

- **FR-001**: Pesquisar jogos e técnicas relevantes, com fontes verificáveis e indicação de quais características são observadas e quais técnicas são confirmadas pelas fontes.
- **FR-002**: Comparar pelo menos três abordagens ou combinações relevantes, incluindo resolução de renderização e tratamento de texturas, e recomendar uma com justificativa e concessões.
- **FR-003**: Produzir ao menos uma imagem renderizada da cena principal existente com a proposta, identificando câmera, configurações e resolução de captura.
- **FR-004**: Preservar a direção visual do GDD e avaliar a leitura de silhueta, personagem e detalhe de investigação no enquadramento real.
- **FR-005**: Registrar como reproduzir a aparência em futuras cenas e assets, incluindo parâmetros, apresentação em telas de proporções distintas, interface e cuidados com estabilidade em movimento.
- **FR-006**: Apresentar evidência de avaliação em movimento e de saída em pelo menos duas resoluções ou proporções; caso não seja possível, registrar explicitamente a limitação e não declarar validação concluída.
- **FR-007**: Manter a escolha como proposta até avaliação do usuário; não alterar editor, pipeline ou dependências fora do escopo necessário.

## Critérios de sucesso

- **SC-001**: O estudo compara pelo menos três alternativas e identifica fontes e motivo da recomendação.
- **SC-002**: Uma imagem da cena do jogo permite ao usuário avaliar pixels nas silhuetas, personagem e detalhe relevante de investigação.
- **SC-003**: As instruções registram parâmetros e passos necessários para reproduzir a imagem e aplicar a direção em cenas futuras.
- **SC-004**: A avaliação cobre movimento, legibilidade e ao menos duas saídas distintas, ou identifica claramente o que não pôde ser verificado.

## Premissas e limites

- A cena principal é `Ilha_Tripo_Caminhada_011`; serão preservados os trabalhos existentes e usados assets disponíveis.
- O projeto usa Unity 6000.3.25f1 e Built-in. A escolha final de renderização está em aberto no GDD; esta entrega propõe uma decisão, sujeita à avaliação visual.
- A imagem é evidência produzida na cena do jogo; referências externas servem ao estudo, não substituem o render da cena.
- A entrega não prevê criar uma sequência narrativa, refazer os assets ou migrar o pipeline.
