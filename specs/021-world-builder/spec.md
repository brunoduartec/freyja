# Freyja World Builder

**Feature Branch**: `021-world-builder`
**Created**: 2026-11-15
**Status**: Aprovada pelo usuário em 2026-11-15
**Input**: Examinar o pacote fornecido, criar o plugin World Builder e usá-lo no Freyja.

## User Scenarios & Testing

### US1 — Montar e editar um trecho (P1)
O autor gera terreno com relevo, uma rua e três lotes; move e gira os lotes em escala métrica e confere o resultado antes de aplicar.
**Teste independente:** abrir a ferramenta, gerar a demonstração, editar um lote e desfazer a alteração.
**Aceite:** terreno de 160 × 160 m, rua de 5 m por pelo menos 40 m, três lotes de 12 × 14 m e três casas provisórias; conflitos visíveis impedem aplicação insegura.

### US2 — Trocar construções sem refazer o terreno (P2)
O autor prepara um modelo local, ajusta orientação e dimensões e substitui uma casa preservando seu lote.
**Teste independente:** substituir uma casa e conferir posição, rotação e apoio.
**Aceite:** fonte preservada, proporções sem distorção e fundação mantida; arquivo incompatível recebe mensagem clara.

### US3 — Regenerar com segurança (P2)
O autor salva, reabre e reconstrói o trecho sem perder objetos manuais.
**Teste independente:** reconstruir duas vezes e comparar quantidade, identidade e geometria.
**Aceite:** permanecem exatamente três casas, objetos manuais intactos e mesmo resultado para a mesma configuração.

### Edge Cases
Lotes girados a 45°/90°, coordenadas negativas, contato de bordas, sobreposição com rua, limite do mapa, modelo vazio ou com escala inválida, material ausente e terreno muito inclinado devem ter validação explícita.

## Requirements

- **FR-001**: Disponibilizar painel de autoria com prévia, aplicação, validação, seleção de mundo e edição de lotes; grid de 1 m ou 0,5 m e desfazer.
- **FR-002**: Separar relevo, rua, fundações e construções; nivelar apoio e suavizar transição ao terreno natural.
- **FR-003**: Manter identificadores persistentes, orientação, cota, dimensões e modelo atribuído a cada lote.
- **FR-004**: Preparar modelos locais sem modificar a fonte, com escala uniforme e base alinhada; manter construções substituíveis.
- **FR-005**: Detectar conflitos antes de aplicar; reconstruir apenas objetos gerenciados e preservar trabalho manual.
- **FR-006**: Salvar dados, documentação de uso, relatório de testes e três vistas reais da demonstração.
- **FR-007**: Trabalhar sem geração 3D externa ou novos serviços; usar modelos locais e volumes provisórios quando necessário.
- **FR-008**: Preservar a ilha atual, controles, estilo gráfico e versão do projeto. Usar inicialmente um conjunto isolado na cena principal; cena adicional somente se houver necessidade técnica documentada.

### Key Entities
Mundo: limites, relevo e configuração reproduzível. Lote: identidade, posição, rotação, área e cota. Via: trajeto e largura. Modelo preparado: fonte, dimensões e orientação. Relatório: evidências e resultados executados ou pendentes.

## Success Criteria

- **SC-001**: Demonstração contém as dimensões e quantidades da US1, com referência humana de 1,80 m.
- **SC-002**: Desvio do apoio de cada casa de até 5 cm em 16 amostras; nenhuma interseção indevida entre lotes e via.
- **SC-003**: Duas reconstruções não duplicam casas nem alteram objetos manuais; salvar e reabrir preserva configuração.
- **SC-004**: Trocar uma casa preserva identidade, posição, rotação e cota do lote.
- **SC-005**: Testes de coordenadas, encaixe, conflitos, repetibilidade e abertura têm resultados reais registrados, com vistas superior, geral e da rua inspecionáveis.

## Assumptions
O pacote contém especificações, não código instalável. Os documentos em references são referência de requisitos, subordinados às orientações do projeto. A primeira entrega é o plugin com a demonstração acima; 160 × 160 m não define a ilha definitiva. A distribuição de 12 famílias, igreja, comércio, praia, cais e casa isolada fica para uma entrega posterior avaliada pelo usuário. Reutilizar o aprendizado do teste 020 sem misturar alterações pendentes de outras branches. Não planejar nem implementar antes da aprovação desta spec, conforme AGENTS.md.
