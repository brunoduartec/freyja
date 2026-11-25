# Kit de cenário PS1 — Freyja

**Feature Branch**: `022-kit-cenario-ps1`
**Created**: 2026-11-17
**Status**: Aprovada pelo usuário em 2026-11-17
**Input**: Prosseguir com materiais de baixíssima resolução, ruas, areia, praia e posicionamento de pedras e árvores pelo usuário e pela IA.

## User Scenarios & Testing

### US1 — Pintar o terreno (P1)
O autor escolhe grama, terra, areia de caminho ou areia de praia e pinta uma região sem perder a pintura ao ajustar construções.
**Teste independente:** pintar uma faixa, salvar/reabrir e reconstruir os lotes; comparar a pintura antes/depois.
**Aceite:** quatro superfícies distinguíveis, repetíveis e misturáveis, preservadas após reconstrução de lotes. Reset da pintura deve ser ação separada e explícita.

### US2 — Montar caminhos (P1)
O autor configura largura e acabamento do caminho e conecta a praia à casa no relevo.
**Teste independente:** trocar o acabamento entre terra, pedra e tijolo e percorrer o trecho com o protagonista existente.
**Aceite:** rua independente do terreno, sem buracos, obstrução ou trechos visualmente suspensos no percurso demonstrado.

### US3 — Colocar pedras e árvores (P2)
O autor escolhe um objeto da biblioteca, posiciona, gira, redimensiona e remove/desfaz pelo editor. A IA utiliza os mesmos assets.
**Teste independente:** colocar uma pedra e uma árvore, ajustar ambas, reconstruir lotes e salvar/reabrir.
**Aceite:** objetos e transformações preservados; desfazer funciona. Biblioteca inicial com três pedras e duas árvores de silhuetas diferentes, reutilizando modelos locais adequados.

### Edge Cases
Escala incoerente entre texturas, repetição visível, mistura excessivamente borrada, árvores enterradas ou flutuando, colisão bloqueando o percurso e reconstrução apagando pintura/objetos devem ser conferidos.

## Requirements

- **FR-001**: Usar PNGs de 32×32 ou 64×64 pixels, sem filtragem suavizante; não aumentar resolução para disfarçar baixa definição. Avaliar legibilidade e cintilação na câmera real.
- **FR-002**: Entregar materiais de grama, terra, areia de caminho, areia de praia, pedra e tijolo, além dos mapas necessários às árvores. Compartilhar texturas quando apropriado.
- **FR-003**: Reutilizar os pincéis e transformações nativos do editor; fornecer biblioteca acessível de materiais e objetos, sem criar ferramenta genérica redundante.
- **FR-004**: Separar pintura manual, geração de relevo e reconstrução de lotes. Preservar pintura e objetos manuais em reconstruções normais; documentar limites da escultura manual.
- **FR-005**: Produzir um trecho avaliável com praia baixa, caminho subindo até uma casa provisória, pedras, árvores e referência humana de 1,80 m. Manter relevo legível e acesso transitável.
- **FR-006**: Manter os fontes editáveis, materiais e objetos reutilizáveis. Produção de modelos no Blender e integração/validação no Unity existentes, respeitando as convenções do projeto.
- **FR-007**: Preservar cenas, arte, controles e pipeline existentes. Não gerar modelos por serviços externos. Não definir nesta etapa a ilha inteira ou sua distribuição definitiva.
- **FR-008**: Concluir/verificar as pendências pertinentes do World Builder 021 antes de depender dele; não repetir testes aprovados sem alteração ou evidência que justifique.

### Key Entities
Material: textura, resolução e escala de repetição. Objeto de biblioteca: modelo, pivô, dimensões e colisão. Trecho de avaliação: terreno, pintura, caminho, casa e objetos. Evidência: capturas, percurso e verificações de preservação.

## Success Criteria

- **SC-001**: Todas as texturas novas têm dimensão máxima de 64 pixels por eixo e pixels legíveis na câmera de jogo; entregar inventário de resoluções.
- **SC-002**: As seis superfícies podem ser atribuídas aos elementos pertinentes; pintura do terreno e materiais de ruas acessíveis pelo editor.
- **SC-003**: Biblioteca inicial possui três pedras e duas árvores utilizáveis; inserir, mover, girar e desfazer são demonstrados.
- **SC-004**: Uma reconstrução de lotes mantém a pintura e os objetos manuais; salvar/reabrir mantém o resultado.
- **SC-005**: O protagonista percorre praia–casa sem cair ou ficar bloqueado; capturas geral, de superfície e da câmera de jogo permitem avaliar escala e aparência.

## Assumptions
Direção aprovada: 3D low poly, baixíssima resolução e pixels visíveis, evocando PS1. Não adicionar automaticamente efeitos de tremulação geométrica, distorção de textura ou sujeira pesada. Resoluções pedidas pelo usuário prevalecem sobre padrões de skills. Modelagem detalhada de casas, igreja e ilha completa fica fora deste teste. Sem novas dependências salvo necessidade comprovada. A regra do AGENTS.md exige avaliação explícita desta spec antes de plano e implementação.
