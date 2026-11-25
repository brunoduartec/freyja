# Especificação — Simplificar as escadas jogáveis

**Branch** `077-simplificar-escadas`  
**Data** 2026-11-25  
**Status** Implementada e validada em 2026-11-25

## Cenários

### P1 — Cruzar a escada piloto da L3

O jogador sobe e desce a escada da L3 na cena principal com os controles e as animações de locomoção do jogo. Os degraus continuam visíveis e uma rampa invisível conduz o personagem entre o terreno e a varanda.

**Teste independente** — Em Play Mode, partir do terreno, chegar à varanda e retornar. Repetir correndo, em diagonal e junto às bordas. Parar na base, no meio e no topo, retomar e inverter o sentido. Tentar entrar pelas laterais protegidas. Conferir o quadro real do jogo durante o percurso.

### P2 — Aplicar a solução às outras escadas compatíveis

Depois do aceite funcional e visual da L3, validar a configuração na escada curta da casa B3 e nos demais lances compatíveis que o inventário confirmar. Na B3, a rampa deve atravessar os quatro degraus até o patamar da porta. Verificar a transição de entrada e a chegada ao patamar plano, que deve continuar permitindo parar e mudar de direção.

**Teste independente** — Percorrer cada lance nos dois sentidos e testar junções, patamar, limites laterais e uma cópia rotacionada. Conferir uma rota de NPC pela escada quando houver rota equivalente na versão anterior.

## Requisitos

- **FR-001** Preservar geometria visual, materiais, orientação, largura caminhável, barreiras laterais e ligação com os pisos. Usar colisão contínua e invisível por lance. Permitir inspecionar seus limites no editor.
- **FR-002** Retirar reversivelmente a influência do Foot IK nos lances convertidos, preservando usos independentes. A primeira comparação usa a caminhada e a corrida existentes. Só considerar animação específica ou ajuste do controlador diante de necessidade demonstrada no piloto.
- **FR-003** Manter os controles e o enquadramento de produção. Verificar terreno plano e inclinado, soleiras, salto, queda, bordas e recuperação. Uma superfície transitável deve permanecer apoiada na descida normal.
- **FR-004** Reutilizar a autoria existente do World Builder quando aplicável. Uma nova configuração deve ser localizada, repetível e preservar referências. Se faltar ferramenta equivalente e o piloto demonstrar necessidade, a geração de rampa deve validar dimensões, inclinação, pisos, layer e conflitos, oferecer Undo e produzir o mesmo resultado ao ser repetida.
- **FR-005** Usar o harness, o jogador, a câmera e a RenderTexture de produção em Play Mode. Registrar trajetórias e sequências visuais comparáveis à versão atual, revisar com os agentes do projeto e checar compilação e Console. Registrar custo de CPU se houver uma medição comparável.
- **FR-006** Conferir o bake com o sistema de navegação existente, testar ida e volta de NPC nas rotas aplicáveis e registrar exceções de geometria ou navegação. Preservar a configuração global de física e dependências, salvo ajuste mínimo demonstrado e compatível com o escopo.

## Critérios de sucesso

- **SC-001** Na L3, a matriz de testes cobre as duas direções, duas velocidades, três paradas, inversão, aproximação diagonal, bordas, tentativas laterais e junções. As travessias previstas terminam no piso correto sem travamento, deriva, perda indevida de apoio ou recuperação.
- **SC-002** Sequências da câmera de produção cobrem subida, descida, parada e transições. A revisão independente avalia flutuação, clipping, oscilação, deslizamento dos pés e enquadramento. A expansão depende do aceite visual e funcional do piloto.
- **SC-003** Cada escada inventariada tem uma decisão registrada. Cada lance migrado completa ida e volta, mantém seus limites e conserva patamares e rotas de NPC aplicáveis. A configuração de uma cópia rotacionada mantém o alinhamento.
- **SC-004** A compilação passa, o Console tem zero novos erros e a regressão cobre terreno, soleira, salto, queda e borda. A entrega registra evidências, limitações, configuração de novas escadas e procedimento de reversão.

## Diagnóstico e premissas

- A L3 é o piloto por ter um lance único e um percurso já documentado. No Editor, a instância tem rotação de 15°, escala unitária e 14 caixas de piso. A faixa local mede 3,92 m por 1,42 m e tem cerca de 2,87 m de desnível entre o terreno e a varanda.
- A casa B3 tem rotação de 20° e quatro degraus com 36,5 cm de profundidade e 19 cm de desnível entre pisos sucessivos. O patamar da porta tem colisor plano separado. É a segunda escada a validar e representa o caso de lance curto.
- O protagonista usa `CharacterController.Move`, sem root motion. A cápsula mede 1,80 m de altura e 0,30 m de raio, com inclinação máxima de 60°, Step Offset de 0,40 m e Skin Width de 0,04 m.
- A solução atual combina filtragem de movimento, pose procedural e fase de animação. O enquadramento de escada consulta o sinal da pose. A migração precisa verificar esse contrato junto com a navegação, cujo bake usa colisores físicos.
- A cena de referência é `Ilha_Principal` no Unity 6000.3.25f1. A inspeção confirmou a câmera `Cais Gameplay Camera` usando `Mundo_480x270`.
- O [percurso original da L3](evidence/baseline/README.md) confirma ida até a varanda, retorno ao terreno e estabilidade nas pausas. Ele fornece quadros e dados de apoio para comparação com o piloto.
