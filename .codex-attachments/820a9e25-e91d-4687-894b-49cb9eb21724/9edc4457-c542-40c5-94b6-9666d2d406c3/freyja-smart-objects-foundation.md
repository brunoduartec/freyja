# Freyja — Smart Objects Foundation

## Missão

Implemente a primeira fatia vertical do sistema de objetos interativos de Freyja, em Unity. O objetivo é disponibilizar objetos de cenário com pontos de interação configuráveis, descoberta por identificadores e atividades, reserva exclusiva por slot e ferramentas de diagnóstico. Esta feature precede o executor de atividades e o scheduler de NPCs. Não implemente agendas nesta etapa.

Feature sugerida para Spec Kit: `smart-objects-foundation`.

## 1. Inspeção e fluxo de trabalho

- Leia AGENTS.md, instruções do repositório e documentos relevantes do Spec Kit. Use os agentes Game Designer, Implementador e Validador já configurados, se disponíveis; caso não estejam, não invente execução por agentes: registre a limitação e siga o fluxo existente.
- Inspecione apenas os diretórios relevantes: arquitetura de NPCs, navegação, World Builder, prefabs de cenário, assets low poly, ferramentas de editor, cenas e testes existentes.
- Verifique a versão da Unity, pacotes instalados, configuração de NavMesh e convenções do projeto. Reaproveite sistemas e abstrações já existentes.
- Produza especificação, plano e tasks no fluxo Spec Kit; prossiga até implementação e validação. Não finalize somente com documentos.
- Não altere arquivos não relacionados, não descarte mudanças do usuário e não introduza dependências comerciais. Não gere modelos via Tripo ou serviços pagos. Use MCP Unity quando disponível e útil, evitando consultas repetidas e dumps extensos.

## 2. Escopo funcional

### 2.1 Modelo de Smart Object

Implemente um componente reutilizável de cena/prefab (nomes abaixo são sugestões; adapte às convenções existentes):

- `SmartObject`: identificador estável de instância, tags de atividade, região ou agrupamento opcional e lista de slots.
- `InteractionSlot`: ID estável dentro do objeto, atividades suportadas, Transform de aproximação, Transform de pose final (posição e rotação), disponibilidade configurável.
- `SmartObjectRegistry`: descoberta dos objetos ativos e busca por ID, região e atividade; seleção ordenada por distância e desempate determinístico por ID.
- `ReservationHandle`: identidade do objeto/slot, proprietário e token de reserva único; impede que uma reserva antiga libere uma reserva nova.

Não utilize coordenadas fixas para referenciar destinos em futuros arquivos de rotina. IDs de instância precisam permanecer estáveis após mover o objeto, salvar a cena ou reabrir o projeto. Instâncias duplicadas de um mesmo prefab devem receber IDs distintos; detecte IDs vazios ou duplicados sem sobrescrever referências silenciosamente. Documente como gerar/corrigir IDs de instâncias e como o World Builder pode preservá-los. IDs locais de slot podem se repetir entre objetos diferentes.

Tags sugeridas: `sit`, `collect_water`, `fish`, `watch_sea`. Use a representação mais simples compatível com o projeto. Um slot pode oferecer várias atividades, mas admite apenas um proprietário por vez. Capacidade de um objeto é determinada pelo número de slots habilitados compatíveis, não por um contador independente.

### 2.2 Reserva e ocupação

Modele os estados `Available`, `Reserved`, `Occupied`, `Disabled` (ou equivalentes). O estado de runtime não deve ser gravado acidentalmente no prefab.

- Uma operação de claim valida objeto, slot, atividade, proprietário e disponibilidade e efetua a reserva de forma indivisível no contexto de execução da Unity.
- `TryReserve` deve retornar sucesso e handle válido, ou falha explícita. `TryOccupy` somente aceita o handle vigente do proprietário. `Release` valida o handle; deve ser seguro chamar repetidamente sem afetar reservas de terceiros.
- Desativar/destruir objeto ou slot invalida a reserva, notifica ou permite ao consumidor detectar a invalidação e impede novas claims enquanto indisponível. Reativação não restaura reservas antigas automaticamente.
- Se o proprietário for destruído/desativado ou cancelar o uso, libere seus recursos por um mecanismo de ciclo de vida explícito. Documente a política de limpeza; não retenha reservas indefinidamente após falhas do consumidor de teste.
- Separe configuração e estado runtime. Não use busca global de objetos por frame; registre/desregistre por lifecycle de cena e componente. Trate mudanças de cena e reinício de Play Mode sem registros estáticos obsoletos.

API ilustrativa, não obrigatória: `FindCandidates(activityId, regionId, origin)`, `TryReserve(slotId, ownerId, activityId, out handle)`, `TryOccupy(handle)`, `Release(handle)`, `IsValid(handle)`. O consumidor deve conseguir tentar o próximo candidato caso a primeira reserva falhe. Não crie um scheduler, fila sofisticada ou sistema de prioridades nesta etapa.

### 2.3 Ferramentas de autoria

- Inspector simples com lista de slots e referências de Transforms editáveis.
- Gizmos na Scene View: pontos de aproximação, pose final, setas de orientação, IDs, e cores distinguindo livre, reservado, ocupado e desabilitado em Play Mode.
- Validação de referências ausentes, IDs duplicados, ausência de atividades, slots sem pose/aproximação e pontos de aproximação fora do NavMesh quando houver superfície navegável configurada. Diferencie erro de configuração de verificação não executável.
- Painel simples no Inspector ou ferramenta de debug, mostrando objetos ativos, capacidade, estado de slot e dono da reserva. Não crie EditorWindow complexa se o Inspector e Gizmos forem suficientes.
- Nenhum objeto precisa de script especializado para cada atividade nesta etapa.

## 3. Prefabs e configuração física

### Obrigatório: banco de madeira

- Procure primeiro um banco compatível entre os assets existentes, incluindo os pacotes low poly já presentes no Freyja. Não presuma que exista um asset específico.
- Se não houver banco adequado, crie um banco low poly provisório usando geometria simples local, sem downloads pagos, sem geração Tripo e sem depender do Blender para cumprir a entrega. Identifique claramente o asset provisório e mantenha seu visual substituível.
- Crie um prefab reutilizável com dois slots `sit`, cada um com aproximação navegável, pose sentada e orientação própria. Separe visual, collider e Transforms de interação.
- Configure dois bancos próximos para exercitar reserva concorrente e fallback. Mantenha os pontos livres de obstáculos; não mova ou reconstrua grandes partes do mapa.
- Prefira cena de validação isolada ou área de teste não invasiva. Se adicionar bancos ao cenário principal, use integração pequena e intencional conforme as convenções existentes e relate os locais; não faça distribuição geral de mobiliário.

### Configuração adicional (se assets existentes estiverem disponíveis)

- Poço: um slot `collect_water` com aproximação e orientação coerentes.
- Cais: múltiplos slots compatíveis com `fish` e/ou `watch_sea`, posicionados em superfície válida.
- Essas tags representam capacidade futura, não implementação da mecânica. Se os objetos existentes não forem adequados ou não estiverem acessíveis, documente o impedimento e não crie assets definitivos adicionais. Banco e testes básicos são o escopo obrigatório.

## 4. Consumidor de demonstração

Implemente apenas um harness de teste temporário/reutilizável, não um novo sistema de IA:

1. Três NPCs existentes ou agentes de teste solicitam `sit`.
2. Dois reservam slots distintos do banco A; o terceiro encontra e reserva um slot do banco B.
3. Com a navegação existente, caminham até o ponto de aproximação. Apenas depois de chegar, o harness confirma a ocupação e utiliza a pose/orientação final para demonstrar o contrato, sem teletransporte visível durante o deslocamento. Para a prova, um alinhamento curto e controlado no destino é aceitável; não implemente IK complexo.
4. Ação de debug ou timeout explícito do harness libera as reservas. Exercite cancelamento durante o trajeto e caminho inalcançável, sem deixar slot preso.
5. Se não houver NPC ou navegação utilizável, demonstre reservas com agentes de teste e marque a validação de deslocamento como bloqueada. Não implemente um sistema paralelo de movimento só para mascarar o bloqueio.

A animação real de sentar, o ciclo sentar/levantar, o executor de tarefas e o scheduler são features posteriores. A presença de um marcador ou agente em pé na pose final não comprova uma atividade `sit` completa. Nesta etapa a tag `sit` indica compatibilidade do slot e a demonstração valida apenas infraestrutura, aproximação e pose de referência. Se uma animação existente puder ser usada sem criar novos sistemas, pode demonstrá-la opcionalmente, separando o resultado dos critérios obrigatórios.

## 5. Testes e critérios de aceite

Implemente testes Edit Mode para o núcleo e Play Mode para integração conforme a infraestrutura do repositório. Execute-os, não apenas escreva-os.

Obrigatórios:

- Registro e remoção de objetos ativos; consulta por ID e atividade, exclusão de objetos desabilitados.
- IDs únicos e estáveis para dois objetos instanciados do mesmo prefab; validação de ID duplicado.
- Dois proprietários podem reservar dois slots distintos do mesmo banco; disputa por um mesmo slot tem um único vencedor.
- Terceiro proprietário encontra outro banco, com seleção determinística quando houver empate.
- Slot ocupado/reservado não é oferecido como livre; atividade incompatível e slot desabilitado são rejeitados.
- Handle de outro proprietário ou antigo não ocupa nem libera uma reserva atual. Liberação repetida não corrompe estado.
- Cancelamento, desativação/destruição do objeto e do proprietário limpam/invalidate reservas corretamente. Reativação e reinício de Play Mode não reutilizam handles inválidos.
- Prefab de banco com dois slots, pose e aproximação configuráveis; duplicação e edição pelo Inspector sem escrever código C#.
- Teste de navegação no cenário de demonstração, quando os pré-requisitos estiverem disponíveis: agentes chegam aos pontos de aproximação corretos e liberam recursos em caso de caminho inválido.
- Compilação sem novos erros; não introduzir warnings recorrentes nem regressões nos testes relevantes.

Validação visual: abra Play Mode pela Unity quando disponível; observe os três agentes, os dois bancos, os estados dos slots e o fallback. Capture evidências (prints do cenário e Gizmos/Inspector, ou logs estruturados de teste quando não houver acesso visual; logs não substituem uma validação visual bloqueada). Registre comandos executados, resultados, falhas e limitações reais. Não declare testes aprovados sem execução.

## 6. Fora de escopo

- Agendas JSON, integração ao relógio, templates de profissão e editor de rotinas.
- Executor genérico de atividades, animação completa de sentar/levantar, IK, ferramentas de trabalho, pesca, agricultura ou diálogos.
- Eventos narrativos, possessão, histórico, persistência de reservas em save game.
- GOAP, Behavior Trees, ECS/DOTS, navegação nova, economia ou simulação off-screen.
- Distribuição em massa de objetos, reformulação visual da ilha, assets gerados por serviços pagos.

Mantenha interfaces simples e extensíveis para o futuro executor de atividades. Não implemente funcionalidades futuras por antecipação.

## 7. Entrega final

Apresente: resumo da arquitetura realmente implementada; arquivos e prefabs alterados; localização da cena de demonstração e instruções de execução; resultados dos testes e evidências; blockers e decisões divergentes desta spec; passos para transformar um novo prefab em Smart Object pelo Inspector, adicionar slots, configurar atividades e verificar IDs; lacunas de animação ou navegação para a próxima etapa.

Use respostas e relatórios compactos. Game Designer valida a interação e legibilidade espacial; Implementador entrega a fatia vertical; Validador executa critérios e aponta regressões. Corrija problemas dentro do escopo antes de finalizar. Não repita a leitura integral do projeto em cada agente.
