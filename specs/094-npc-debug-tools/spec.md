# Especificação: NPC Debug Tools

**Feature Branch**: `094-npc-debug-tools`  
**Created**: 2026-11-27  
**Status**: Fechada como implementação concluída; validação geral parcial. Os limites remanescentes estão registrados em [validation-matrix.md](evidence/validation-matrix.md) e [tasks.md](tasks.md).
**Input**: [Requisitos completos fornecidos](source-requirements.md)

## Cenários de usuário e testes

### História 1 — Inspecionar rotinas reais (P1)

Como desenvolvedor, quero abrir um monitor na Unity e comparar a atividade prevista com a atividade efetiva de cada NPC, para identificar estados, destinos, reservas, animações e falhas sem alterar código.

**Teste independente**: na cena principal, abrir o monitor fora e dentro de Play Mode; em execução, localizar os três NPCs reais e verificar que os dados apresentados correspondem às fontes de rotina, execução, navegação e Smart Objects.

**Aceite**:

1. Fora de Play Mode, o monitor informa que dados ao vivo não estão disponíveis e desabilita comandos.
2. Em Play Mode, cada NPC registrado aparece uma vez; busca, filtros, seleção e atualização preservam o NPC selecionado quando ele continua ativo.
3. A tela diferencia compromisso previsto, execução efetiva, última atividade concluída, pedido de reserva, reserva confirmada e ocupação; campos ausentes são rotulados como desconhecidos, indisponíveis ou não aplicáveis.
4. A seleção localiza o objeto correspondente na cena quando ele existe e não dispara ações de gameplay.

### História 2 — Diagnosticar e controlar a simulação (P1)

Como desenvolvedor, quero inspecionar mudanças recentes e solicitar controles de diagnóstico seguros, para reproduzir falhas sem contornar os donos reais do relógio, execução ou recursos.

**Teste independente**: observar transições correlacionadas por NPC e execução; executar comandos disponíveis e comparar o resultado informado com o estado real após o processamento.

**Aceite**:

1. O histórico contém transições relevantes com horário e correlação disponíveis, tem limite configurado e contabiliza descartes; limpar o histórico não altera o estado do jogo.
2. Cancelamento, reavaliação, controles de relógio e override temporário só ficam habilitados quando há API existente e segura; caso contrário, a ferramenta informa o motivo.
3. Um comando aceito pode permanecer pendente até o sistema confirmar o resultado. Cancelamento mantém a reserva até o cleanup real; nenhum comando libera recursos, teletransporta NPCs ou força animação diretamente.
4. Fechar a janela, trocar de cena ou encerrar Play Mode não deixa pausa, override, overlay, inscrição ou callback de diagnóstico órfão.

### História 3 — Consultar, visualizar e exportar evidências (P1)

Como validador, quero obter snapshots estruturados e arquivos JSON pequenos e estáveis, mesmo sem abrir o monitor, e visualizar destinos já calculados na Scene View, para analisar estado lógico sem depender de screenshots ou logs extensos.

**Teste independente**: consultar snapshots por API/comando documentado com a janela fechada; exportar dados globais e individuais e comparar com o estado observado na mesma sessão de Play Mode.

**Aceite**:

1. Snapshot global e individual incluem versão do schema, cena/sessão, horário, filtros e timestamp; IDs e coleções possuem ordenação estável e truncamento explícito.
2. JSON é UTF-8 válido, representa números não finitos como nulos com estado explicativo e não contém caminhos pessoais, segredos ou referências a objetos vivos.
3. Os overlays mostram apenas destinos, caminhos, regiões, pontos e estados disponíveis nos sistemas reais; desenhar não calcula caminhos nem muda o mundo.
4. Exportar, consultar e desenhar não inicia reavaliações, pathfinding, reservas, animações ou alterações de gameplay.
5. A ferramenta e seus comandos não ficam ativos em builds de produção nem publicam informação interna na interface de gameplay.

## Casos de borda

- NPC, Smart Object, slot, destino ou caminho deixa de ser válido durante uma captura ou comando.
- O snapshot muda enquanto há filtro, seleção ou exportação; dados antigos precisam ser identificados ou removidos com segurança.
- O relógio avança, pausa, acelera ou cruza meia-noite; atividades puladas não podem aparecer como fisicamente concluídas.
- Dois NPCs disputam slots; pedido, posse confirmada, ocupação física e liberação devem refletir estados distintos conforme o contrato existente.
- Histórico atinge sua capacidade, janela reabre, domínio não recarrega ou Play Mode termina.
- Animação, navegação ou decisão não fornece um detalhe consultável; o monitor deve indicar a limitação sem inferir um valor.

## Requisitos

### Requisitos funcionais

- **FR-001**: Disponibilizar uma janela de diagnóstico pela Unity, preferencialmente no menu `Freyja > NPC > Debug Monitor`, com estado vazio explícito fora de Play Mode.
- **FR-002**: Exibir por NPC identidade, compromisso previsto, atividade efetiva, estado e etapa, destino, navegação, animação, reserva/ocupação, próximas mudanças conhecidas e motivo real de falha/fallback quando disponível.
- **FR-003**: Distinguir dados observados de desconhecidos, indisponíveis, não aplicáveis e desatualizados; não tratar fallback legítimo como falha grave.
- **FR-004**: Permitir busca, filtros, seleção/localização, histórico limitado, cópia de resumo e exportação JSON global ou individual.
- **FR-005**: Disponibilizar consultas estruturadas reutilizáveis por harnesses e ferramentas sem exigir a janela aberta, sem criar serviço HTTP, socket ou servidor externo.
- **FR-006**: Oferecer controles de relógio, cancelamento, reavaliação e override temporário somente onde contratos existentes os suportarem com segurança; informar resultado, pendência ou motivo de indisponibilidade.
- **FR-007**: Visualizar dados de navegação, regiões e Smart Objects na Scene View por desenho passivo e com escopo selecionável.
- **FR-008**: Reutilizar os sistemas existentes de relógio, agenda, host, executor, navegação, animação e reservas; não introduzir autoridades paralelas para esses domínios.
- **FR-009**: Isolar a ferramenta para desenvolvimento/editor, limitar polling e histórico, e remover inscrições e estado temporário ao encerrar sua sessão.
- **FR-010**: Validar os fluxos aplicáveis na cena principal com os três NPCs reais, reportando cada caso como PASS, FAIL, BLOCKED ou NOT RUN, com evidência correspondente.
- **FR-011**: Documentar o menu, os comandos realmente disponíveis, os limites dos dados e exemplos de JSON provenientes de uma execução real.
- **FR-012**: Não alterar autoria de rotinas, adicionar atividades profissionais, funcionalidades narrativas, novo scheduler, relógio, executor, navegação ou sistema de reservas.

### Entidades principais

- **Snapshot de diagnóstico**: cópia imutável de uma captura do contexto global ou de um NPC, com indicação de origem, horário, disponibilidade e recência dos campos.
- **Evento de diagnóstico**: transição significativa correlacionada com NPC e execução quando esses identificadores existirem; sujeito a limite e contagem de descarte.
- **Comando de diagnóstico**: solicitação explícita encaminhada ao sistema proprietário, com origem, resultado e estado pendente quando aplicável.
- **Slot de interação**: recurso do Smart Object com estado e proprietário reportados pela fonte autoritativa.

## Critérios de sucesso

- **SC-001**: Na cena de referência, o monitor identifica uma vez cada um dos três NPCs e permite localizar e inspecionar qualquer um deles.
- **SC-002**: Os estados previstos, efetivos e de recursos exibidos correspondem às fontes reais em idle, deslocamento e disputa/ocupação de assentos; toda lacuna é rotulada em vez de inferida.
- **SC-003**: Snapshot global e individual podem ser obtidos sem abrir a janela e exportados como JSON válido que corresponde ao instante de captura.
- **SC-004**: Consultas, atualizações e overlays não alteram o estado da simulação; comandos suportados respeitam o ciclo real de execução e cleanup.
- **SC-005**: Reabrir a janela e iniciar/encerrar Play Mode três vezes não deixa callbacks, overlays, pausas ou overrides órfãos.
- **SC-006**: A matriz de validação do anexo registra evidência real para cada caso executado e limitações explícitas para recursos não suportados ou não verificáveis.

## Premissas

- O usuário do monitor é desenvolvedor/validador trabalhando no Editor; o monitor não é parte da interface do jogador.
- A cena de referência é `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` e contém os três NPCs de integração relatados.
- As fontes reais de tempo, resolução, execução, navegação, animação e reservas serão confirmadas antes do planejamento; dados ausentes não serão fabricados.
- Overrides são condicionais à existência de um mecanismo compatível e podem ser registrados como não suportados sem criar um sistema novo.
- A validação anterior é parcial: a integração foi considerada operacional para esta etapa, mas a pose sentada e alguns casos de falha/lifecycle permanecem sem aceite físico completo.
