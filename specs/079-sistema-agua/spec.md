# Especificação: água estilizada e costa

**Branch**: `079-sistema-agua`  
**Criada**: 2026-11-25  
**Estado**: aprovada pelo usuário em 2026-11-25  
**Fonte**: `freyja-water-system-codex.md`, seções 1–13, requisitos e critérios AC-01 a AC-14.

## Cenários e testes

### P1 — Mar e praia vivos

Na câmera do jogo, o jogador vê mar calmo com ondas e ondulações, fundo identificável nas áreas rasas e transição gradual ao profundo. Frentes de espuma percorrem a costa curva, avançam sobre a areia e recuam; a areia escurece e seca gradualmente. Pedras, pilares e cascos em contato recebem espuma localizada, sem espuma em objetos suspensos ou secos.

**Teste independente**: executar cena representativa, observar dois ciclos completos e comparar rasos, profundos, contatos e controles negativos na saída final da câmera.

### P2 — Ambiente e integração

A água acompanha iluminação e vento existentes, preserva a apresentação pixelada e se integra à praia e ao píer sem prejudicar a leitura do jogo. Cáusticas permanecem no fundo submerso e diminuem à noite; refração é opcional e deve evitar halos.

**Teste independente**: comparar quatro horários, variação suave do vento e saída real do jogo, incluindo Render Texture, antes e depois da integração.

### P3 — Configuração e interações

O criador ajusta aparência e qualidade, regenera a costa sem destruir autoria e utiliza perfis de mar calmo e agitado. Após aprovação do núcleo, personagens e barcos produzem ondulações ou rastros leves na água e os efeitos cessam ao sair.

**Teste independente**: regenerar a costa duas vezes e comparar resultados; executar interações em movimento, parado e fora d’água; comparar níveis de qualidade.

### Casos limite

- Ausência de fundo ou profundidade válida; objetos transparentes que não escrevem profundidade; geometria seca ou suspensa.
- Praias íngremes, baías e junções, mudança de câmera ou de nível do mar; cobertura não deve vazar em construções.
- Noite, transições de vento, resolução final pixelada, limites da tela e qualidade baixa.
- Falta de medição de GPU ou de acesso ao editor deve resultar em limitação registrada, nunca aprovação presumida.

## Requisitos

- **FR-001**: A superfície deve mover-se continuamente com ondas suaves e detalhes orgânicos configuráveis, coerentes no espaço e legíveis na saída real.
- **FR-002**: Água rasa deve permitir identificar areia, pedras e vegetação submersas; cor e visibilidade devem mudar suavemente com profundidade, inclusive com fundo real e fallback para ausência de geometria.
- **FR-003**: Espuma de contato animada e ajustável deve acompanhar objetos opacos que tocam a água, evitando falsos positivos visíveis em objetos secos ou suspensos.
- **FR-004**: Frentes costeiras ancoradas à margem devem acompanhar curvas, avançar sobre areia e recuar, com crista branca orgânica e sem emendas, vazamentos ou camada azul artificial.
- **FR-005**: A faixa alcançada deve escurecer ao molhar e retornar gradualmente à aparência seca, acompanhando a costa.
- **FR-006**: Cáusticas animadas devem ocorrer apenas sobre fundo submerso e atenuar com profundidade e falta de luz solar; refração opcional deve ter alternativa desligada e evitar halos.
- **FR-007**: Água deve responder à luz, ambiente e vento existentes com transições suaves, preservando a direção artística e a câmera de jogo.
- **FR-008**: A configuração deve oferecer perfis calmo e ventoso, controles compreensíveis e níveis Low, Medium e High; Low preserva os efeitos essenciais e Medium é o padrão.
- **FR-009**: A preparação da costa deve ser determinística, editável e não destrutiva; a cena isolada deve reproduzir praia curva, diferentes profundidades, contatos e controles negativos.
- **FR-010**: Após validação isolada, integrar à cena principal preservando câmera e apresentação. Implementar interações leves de personagens e barcos após aprovação do núcleo, sem adicionar natação, flutuação ou navegação.
- **FR-011**: Registrar diagnóstico, decisão de reaproveitamento e licenças, uso, limitações, testes, evidências visuais, defeitos corrigidos e custo medido quando possível; preservar arte e alterações preexistentes.
- **FR-012**: Avaliar primeiro o candidato Uber Stylized Water em ambiente isolado e documentar compatibilidade e decisão; não migrar o pipeline nem contratar dependências pagas.

## Critérios de sucesso

- **SC-001**: AC-01 a AC-11 e AC-13 a AC-14 do documento de origem são avaliados individualmente com evidências reais; AC-12 é avaliado para interações da etapa posterior. Cada resultado é PASS, FAIL, BLOCKED ou NOT RUN.
- **SC-002**: Uma sequência abrange ao menos dois ciclos completos de avanço e recuo costeiro; evidencia também a umidade antes, durante e depois, sem substituir revisão visual por detecção automática de pixels.
- **SC-003**: Quatro horários, ao menos dois níveis de qualidade e a saída final da câmera real são comparados; a cena principal só recebe o sistema após o núcleo isolado passar nos gates correspondentes.
- **SC-004**: O custo com água ligada e desligada é comparado sob as mesmas condições quando a medição estiver disponível; limitações e métricas não medidas são registradas sem alegar desempenho alcançado.
- **SC-005**: Compilação, referências, comportamento e ausência de erros relevantes são verificados no Editor; imagens e sequências são examinadas e falhas corrigidas e revalidadas.

## Premissas e limites

- O Unity observado é 6000.3.25f1; documentação e manifesto apontam Built-in, sem pacote URP. Confirmar no Editor e adaptar sem migração.
- O GDD apresenta ilha planejada de 260 × 180 m; os 160 × 160 m citados no anexo são hipótese de World Builder e devem ser confrontados com a cena real.
- Valores, paleta e orçamento de 2 ms do documento são pontos de partida, não medições ou critérios automaticamente aprovados.
- A referência Pinterest e ferramentas externas devem ser verificadas; indisponibilidade e limitações de transparência/depth devem ser documentadas.
- A cena de teste não entra automaticamente no fluxo de produção. Nenhuma captura isolada ou compilação prova movimento e qualidade.
