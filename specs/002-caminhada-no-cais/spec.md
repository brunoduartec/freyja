# Spec 002 — Caminhar pelo cais

**Criada:** 12/11/2026 · **Estado:** implementada e validada tecnicamente; aguardando avaliação do usuário.
**Origem:** próxima entrega curta, após a revisão visual do cais na Unity.

## Cenários do usuário e testes

### História 1 — Percorrer o cenário em terceira pessoa (P1)

Quero controlar um personagem provisório do desembarque até o início do caminho para avaliar escala, movimento, câmera e passagem pelos degraus.

**Valor:** transformar a cena estática em um pequeno trecho jogável, antes de adicionar sistemas narrativos.
**Teste independente:** abrir a cena de teste e percorrer cais → degraus → início do caminho → cais.

**Aceitação:**
1. Ao iniciar, o personagem aparece sobre o cais, em posição segura, visível pela câmera.
2. Com WASD, consigo fazer o percurso de ida e volta sem atravessar o piso, ficar preso nas junções ou precisar saltar.
3. Com o mouse, consigo observar o entorno; a câmera acompanha o personagem e evita atravessar o cenário.
4. Ao reiniciar a cena, começo novamente no desembarque.

### Casos de borda
- Ao encostar em obstáculos, o personagem não os atravessa.
- Ao sair do cais e cair fora da área transitável, retorna ao ponto inicial; não há natação nesta entrega.
- Ao perder foco da janela, o personagem deixa de receber comandos; o usuário consegue liberar o cursor com Esc e retomar o controle explicitamente.

## Requisitos

- **FR-001:** disponibilizar uma cena jogável separada, preservando a cena de revisão visual existente.
- **FR-002:** representar o jogador por uma figura provisória de escala humana, sem exigir modelo ou animações finais.
- **FR-003:** permitir caminhada com WASD em relação à orientação da câmera; movimento diagonal não deve ser mais rápido.
- **FR-004:** oferecer câmera em terceira pessoa controlada pelo mouse, com personagem visível em percurso aberto e tratamento de obstáculos entre câmera e personagem.
- **FR-005:** permitir atravessar as junções e os degraus do trecho sem salto e respeitar as superfícies sólidas do cenário.
- **FR-006:** recuperar o personagem após queda e iniciar cada nova partida sem carregar posição anterior.
- **FR-007:** exibir os controles básicos e permitir liberar/retomar o cursor sem movimento involuntário.

## Critérios de sucesso

- **SC-001:** completar duas idas e voltas consecutivas pelo percurso, sem ficar preso, atravessar o chão ou precisar saltar.
- **SC-002:** ao aproximar a câmera dos obstáculos do trecho, recuperar a visão do personagem sem a câmera atravessar superfícies sólidas.
- **SC-003:** após uma queda e após reiniciar a partida, reaparecer no ponto seguro do desembarque.
- **SC-004:** o usuário consegue jogar o trecho e avaliar movimento, câmera e proporções com os controles exibidos, sem instruções externas.

## Premissas e limites

Teclado e mouse; usar o cais e o barco já criados, com ajustes pontuais de passagem se necessários. A figura provisória serve para testar escala e não aprova o visual do protagonista. Terceira pessoa continua sendo uma proposta a avaliar.

**Fora do escopo:** novos cenários, personagens finais, salto, corrida, natação, diálogos, caderno, pistas, save/load e acabamento visual PS1.

**Entrega:** [cena jogável](../../Freyja/Assets/_Project/Scenes/Cais_Caminhada.unity), [guia de controles](quickstart.md), [prévia](evidence/cais-caminhada-final.png) e [validação](evidence/validacao-us1.md). As [tarefas](tasks.md) foram executadas; aprovação da experiência pelo usuário permanece pendente. Não ampliar o escopo antes dessa avaliação.
