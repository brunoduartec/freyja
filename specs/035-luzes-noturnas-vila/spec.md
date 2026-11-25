# Especificação das luzes noturnas da vila

**Branch** — `035-luzes-noturnas-vila`  
**Data** — 2026-11-18  
**Estado** — Aprovada para implementação

## Cenários e testes

### P1 — Ver as casas acenderem ao entardecer

Da câmera do jogo, o jogador vê janelas e entradas de casas ganharem um brilho âmbar conforme o sol baixa. Alguns pontos acendem antes de outros, com uma transição suave. Pelo menos três fachadas do trecho entre o cais e a praça devem mostrar a mudança. As luzes permanecem acesas durante a noite e se apagam gradualmente ao amanhecer.

### P2 — Caminhar pela vila à noite

Ao sair do cais à noite, o jogador reconhece a entrada do percurso e a chegada à praça pela luz das fachadas e por luminárias nesses dois pontos de passagem. A luz alcança trechos próximos do piso, dos degraus e das paredes. A iluminação localizada mantém o contraste quente com a noite azul e permite distinguir áreas escuras entre os pontos de luz.

### Casos de borda

- Ao passar da meia-noite para o dia seguinte, as luzes permanecem contínuas.
- Ao saltar para um horário no modo de revisão, todas as luzes representam imediatamente a hora selecionada.
- A mesma fachada conserva sua sequência de acendimento a cada volta do ciclo.

## Requisitos funcionais

- **RF-001** — Sincronizar as luzes da vila com o ciclo de dia e noite já implementado na cena principal `Ilha_Tripo_Caminhada_011`.
- **RF-002** — Mostrar o brilho nas janelas e nas entradas e iluminar superfícies próximas em pontos selecionados do trecho cais–praça.
- **RF-003** — Fazer as luzes entrarem progressivamente entre 17h30 e 18h30, permanecerem acesas até 5h30 e apagarem gradualmente até 6h30. Os horários são uma proposta para este estudo.
- **RF-004** — Cobrir nesta primeira entrega pelo menos três fachadas de casas e dois pontos de passagem, um no cais e outro na praça.

## Critérios de sucesso

- **CS-001** — Um vídeo na câmera do jogo mostra janelas e luminárias passando de apagadas para acesas ao entardecer. A revisão do ciclo confirma a continuidade à meia-noite e o apagar ao amanhecer.
- **CS-002** — Duas capturas do mesmo enquadramento, de dia e de noite, mostram a mudança nas fachadas e a incidência de luz no entorno. Em caminhada noturna, é possível reconhecer a saída do cais e a entrada da praça.

## Premissas

- A base visual é o estudo de iluminação da branch `033-estudo-iluminacao`. A primeira entrega usa o trecho cais–praça para avaliar a densidade e a intensidade antes de estender a iluminação pela vila.
- O acendimento é visual e segue o horário. Rotinas dos moradores e estados de ocupação ficam para outra etapa.
