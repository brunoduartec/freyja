# Especificação — Grama e vento global

**Branch** `078-grama-vento-global`
**Data** 2026-11-25
**Status** Implementada e validada em 2026-11-25

## Cenários e testes

### P1 — Percorrer a vila e o litoral

O jogador vê vegetação abundante e irregular nos campos, encostas, jardins e transições costeiras. Caminhos, escadas, portas, personagens, pistas e cultivos continuam legíveis.

**Teste independente** — Percorrer área construída, campo, praia e região de cultivo quando disponível. Comparar a câmera de jogo em distância normal e nos extremos de zoom existentes. Conferir que as silhuetas pixeladas preservam leitura, que a densidade varia e que a vegetação não invade áreas excluídas.

### P1 — Ver a grama reagir ao ambiente

O sol, a sombra e as luminárias mudam o aspecto da grama ao longo do dia. Ondas de vento cruzam o terreno, com as raízes fixas e resposta gradual a mudanças ambientais.

**Teste independente** — Fixar a câmera e o relógio real às 06h, 12h, 17h e 22h. Comparar sol e sombra, contraluz e poste ligado/desligado. Em calmaria, brisa, vento forte e mudança de direção, capturar sequências que incluam uma divisa entre regiões.

### P2 — Ajustar a cobertura com custo previsível

A equipe configura os perfis de grama baixa, selvagem e costeira, a qualidade e o vento. A mesma configuração reproduz a distribuição. Alterações em exclusões afetam apenas as regiões atingidas e a remoção restaura os tufos elegíveis.

**Teste independente** — Repetir a geração com a mesma seed e com seed diferente. Adicionar, mover e remover uma exclusão. Comparar três densidades com a grama desligada, a versão atual e o sistema novo.

## Requisitos

- **FR-001** Aplicar instancing, LOD, iluminação dinâmica e vento global a todos os tufos 3D dos três perfis configuráveis na cena jogável `Ilha_Principal`. Preservar a câmera, a saída pixelada, a paleta, os percursos e as interações. Variar posição, altura, largura, inclinação, cor e rotação de forma determinística.
- **FR-002** Responder ao ciclo de luz real, luz ambiente, luminárias locais, sombras de cenário e fog. Usar gradiente de raiz a ponta, normais coerentes e contraluz cuja intensidade siga a visibilidade e a energia da fonte. Permitir testar o custo das sombras próprias.
- **FR-003** Usar um campo de vento global em espaço de mundo, compartilhável por outros materiais, com presets e ajuste em execução. Manter as raízes fixas, limitar curvatura e deslocamento e respeitar a pausa do jogo. Direção nula e calmaria devem produzir comportamento definido. O ciclo de vida da cena deve impedir controladores concorrentes e estado residual.
- **FR-004** Distribuir sobre superfícies válidas conforme altura, inclinação, tipos de solo e máscaras existentes. Excluir água, áreas de praia não autorizadas, rochas, vias, praça, cais, construções, acessos e cultivos. Preservar jardins e áreas livres. Reutilizar as integrações existentes para exclusão dinâmica, ou oferecer um ponto de extensão testável onde o fluxo de gameplay estiver ausente.
- **FR-005** Usar desenho instanciado, com níveis de qualidade, descarte de regiões fora da câmera e redução de detalhe estável durante movimento e zoom. Incluir a amplitude de vento nos limites de visibilidade. As folhas são decorativas e agrupadas para renderização.
- **FR-006** Avaliar primeiro Endless Meadow, incluindo licença e compatibilidade, e testar isoladamente quando compatível com o projeto. Comparar sua arquitetura com o instancing atual e uma alternativa mínima. Preservar Unity, pipeline, dependências, referências e modificações locais.
- **FR-007** A qualidade Alta deve aumentar consideravelmente a presença de grama, com prioridade para campos e encostas. A comparação usa a versão com 49.885 tufos anterior a este ajuste. Variar a densidade por um campo de ruído em coordenadas da ilha, com manchas contínuas entre regiões. Manter as exclusões e o orçamento de renderização.
- **FR-008** Fornecer controles de autoria, guia de uso, decisão técnica, cena de ensaio, testes e benchmark reproduzíveis. Usar o harness existente para capturas pela câmera jogável. Registrar limitações, falhas e retestes com evidências independentes.

## Critérios de sucesso

- **SC-001** As capturas dos quatro horários no mesmo enquadramento mostram resposta à luz, sombras, contraluz sem brilho artificial noturno e efeito local de uma luminária. As sequências dos quatro cenários de vento mostram raízes fixas, transições suaves e continuidade regional.
- **SC-002** Observações dos três enquadramentos de zoom mostram grama presente e legível nas áreas elegíveis. A travessia em ida e volta mantém continuidade nos limites de visibilidade. As capturas de campo, casa, caminho e praia confirmam os três perfis e as exclusões. Cultivos entram na matriz onde o fluxo existir.
- **SC-003** A repetição da mesma seed produz a mesma distribuição. Alterar a seed produz outra distribuição válida. Inserir, mover e remover uma exclusão preserva as regiões não afetadas e restaura a cobertura original nas áreas liberadas. Testes cobrem inclinação, altura, costuras, visibilidade sob rajada, estado de vento e recarga.
- **SC-004** Em Metal no M4 Pro, registrar no mínimo 300 quadros ou 10 s por cenário estável após aquecimento, com câmera, horário, resolução, qualidade e limite de quadros registrados. Medir FPS, p50/p95, CPU principal/renderização, custo incremental, lotes, instâncias, triângulos, memória, GC e latência de atualização regional quando disponíveis. Medir GPU quando disponível. Incluir três densidades, os extremos de zoom, campo, área construída, noite e sombras próprias ligadas/desligadas. Metas de referência: 60 FPS quando a base permitir, até 2 ms incrementais de GPU e 0 B/quadro de GC atribuíveis à grama em regime estável.
- **SC-005** Play Mode e regressões pertinentes passam, com zero novos erros de Console e shader. Relatório final classifica cada critério como PASS, FAIL, BLOCKED ou NOT RUN, com capturas, medições e reteste das falhas corrigidas.

## Premissas

- A distribuição e as referências das specs 029 e 032 são candidatas a reaproveitamento. A cena atual e os assets que ela referencia definem a base geométrica e os limites de integração. A auditoria inicial registra a diferença entre dimensões do World Builder e da ilha atual.
- Os intervalos iniciais de altura são 0,12–0,30 m para grama baixa, 0,30–0,65 m para selvagem e 0,20–0,50 m para costeira. A escala final depende da leitura na câmera real.
- As densidades de ensaio começam em 8, 24 e 48 tufos por m² apto. A qualidade de referência será decidida pela validação visual e medições.
- A mecânica de reação da grama ao personagem fica para uma entrega futura.

[Auditoria inicial](auditoria.md)
