# Especificação — Iluminação dos percursos da ilha

**Feature Branch**: `081-iluminacao-percursos`  
**Data**: 2026-11-25  
**Status**: Implementada; validação parcial documentada em `auditoria.md`

## Cenário P1 — Explorar a ilha à noite

O jogador percorre cais, caminhos, ruas, praça, escadas e áreas habitadas com luz suficiente para reconhecer o piso e seguir a rota, preservando trechos mais sombrios e o clima de mistério.

**Aceite**: Em horário noturno fixo, percorrer todas as rotas navegáveis pela câmera real; identificar e corrigir trechos escuros, excesso de luz, sobreposições e obstruções; validar novamente com capturas antes/depois.

## Cenário P1 — Ver postes funcionando

Cada poste existente no cenário deve ter luminária e fonte de luz visíveis e funcionais, com apoio coerente no terreno, sem bloquear a navegação e vinculados ao ciclo de dia e noite.

**Aceite**: Inspecionar todos os postes em noite e dia; confirmar luz quente à noite, apagamento diurno e ausência de sombras em tempo real desnecessárias.

## Requisitos

- **FR-001** Reutilizar os modelos e prefabs de postes e luminárias existentes e corrigir todos os postes presentes na cena.
- **FR-002** Distribuir iluminação irregularmente ao longo das rotas navegáveis; preferir luminárias de parede em fachadas e evitar simetria e sobreposição.
- **FR-003** Fazer fontes novas ou corrigidas integrarem `VillageNightLights` e `CoastalDayNight` existentes, sem refatorar o ciclo.
- **FR-004** Manter o piso navegável legível à noite e a atmosfera quente nas áreas habitadas; manter sombras intencionais navegáveis.
- **FR-005** Evitar aumento excessivo de luzes com sombras em tempo real.
- **FR-006** Validar a noite em Play Mode pela câmera real, inspecionando sistematicamente a ilha, iterando sobre falhas e registrando desempenho e capturas comparativas.

## Critérios de sucesso

- **SC-001** Todas as regiões navegáveis são percorridas e não há trechos que impeçam reconhecer e seguir o caminho.
- **SC-002** Todos os postes apresentam visual de luz à noite e ficam apagados de dia pelo ciclo existente.
- **SC-003** Capturas de antes/depois cobrem o cais, subida, núcleo habitado e rotas altas; revisão não encontra nova obstrução ou excesso de luz.
- **SC-004** O custo de desempenho e a contagem/modo de sombras das luzes são registrados contra a configuração anterior.

## Observações iniciais

- A análise e os resultados observados após a implementação estão registrados em [`auditoria.md`](auditoria.md), incluindo limitações de cobertura e capturas.
- Foram preservadas as alterações locais preexistentes de terreno/água e da entrega 080.
