# Validação independente — água 079

25/11/2026. Critérios: spec 079 e seção 10 do anexo. Reavaliei a versão híbrida em `Artifacts/water-validation/roystan-hybrid.png`, `hybrid-overview.png`, `hybrid-phase00.png`, `hybrid-phase04.png` e `hybrid-live-00.png` a `hybrid-live-03.png`, além de `docs/water/implementacao.md`. As imagens são amostras, não vídeo contínuo. Não alterei a cena nem os assets.

| Critério | Resultado | Evidência e limite |
|---|---|---|
| AC-01 | PASS | Na cena isolada em Play, água renderizada sem rosa/preto; Console atual sem erros e compilação sem falha. Objetos ativos: zero scripts ausentes e zero materiais/shaders ausentes ou não suportados; os três shaders usados reportam zero erros. Não cobre a cena principal. |
| AC-02 | FAIL | Os quatro quadros em tempo normal mostram variação de manchas suaves, mas não ondas e ondulações claramente legíveis na escala do jogo; imagens amostradas não provam continuidade. |
| AC-03 | FAIL | A visão geral mostra areia e algumas pedras submersas, mas os objetos têm contraste baixo e a vegetação não é identificável como tal; o desaparecimento gradual do fundo não está claramente demonstrado. |
| AC-04 | FAIL | As fases e imagens ao vivo mostram contorno em pedra e pilar e controle suspenso limpo; no casco o contorno é estreito, irregular e incompleto. A variação temporal satisfatória em todos os contatos não está comprovada. |
| AC-05 | BLOCKED | A sequência costeira apresenta cristas em diferentes posições; a inspeção de quadros não demonstrou dois ciclos contínuos completos nem propagação satisfatória. |
| AC-06 | FAIL | `seam-blend.png` reduz a faixa de areia exposta, mas ainda mostra mudança de tonalidade e cristas brancas destacadas em segmentos; falta teste de mudança de câmera. |
| AC-07 | BLOCKED | A faixa escura varia, mas as imagens examinadas não demonstram de modo inequívoco molhar após a passagem e secar gradualmente. |
| AC-08 | BLOCKED | A comparação com luz desligada mostra redução do padrão sobre o fundo, mas movimento contínuo, atenuação em profundidades distintas e ausência sobre todo terreno seco não foram demonstrados. |
| AC-09 | NOT RUN | Refração é opcional; não há comparação ligada/desligada. |
| AC-10 | BLOCKED | Sem quatro horários nem sequência de transição de vento. |
| AC-11 | BLOCKED | A captura posterior `Artifacts/water-validation/079-main-play.png` mostra água na câmera real com alvo `Mundo_480x270`; falta comparação antes/depois e avaliação de regressões em outras vistas. |
| AC-12 | BLOCKED | Interações pertencem à etapa posterior, após aprovação do núcleo. |
| AC-13 | BLOCKED | Sem comparação Low/Medium nem medição de GPU com água ligada e desligada. |
| AC-14 | BLOCKED | Sem regeneração repetida da costa, comparação de assets ou teste de independência da câmera. |

**Defeitos prioritários:** (1) ondas pouco legíveis (AC-02, `hybrid-live-00...03`); (2) fundo e vegetação pouco legíveis (AC-03, `hybrid-overview`); (3) espuma estreita e irregular no casco (AC-04, `roystan-hybrid`). Revalidar com sequência contínua, controles negativos e enquadramentos adequados antes de integrar à cena principal. A câmera da prova solicitava Depth e DepthNormals; isso não valida transparências ou projeção ortográfica.

O histórico do serviço de Console inclui exceções anteriores `CoastalGrass.LateUpdate`, `Assets/_Project/Scripts/Environment/CoastalGrass.cs:81`; o estado atual informa zero erros e uma advertência. Não atribuo essas exceções à água nem presumo que ainda ocorram. Nenhum teste automatizado ou profiling foi executado nesta revisão. Play Mode permaneceu ativo, preservando a sessão.

## Reavaliação direcional

Examinei `directional-overview.png` e seis quadros extraídos visualmente de `directional-rt.gif` (0, 10, 20, 30, 40 e 50). O arquivo contém 60 quadros de 480 × 270, com duração declarada de 150 ms por quadro. A abertura animada no navegador foi bloqueada pela política de URL local; portanto não avaliei a reprodução contínua nem sua suavidade.

- **AC-02 — FAIL:** as formas direcionais mudam entre quadros, mas são manchas largas e pouco definidas na escala apresentada; a leitura clara de ondas e ondulações não está demonstrada. A suavidade do movimento continua sem avaliação.
- **AC-03 — FAIL:** areia, pedras e hastes aparecem, mas o contraste das pedras e a identificação da vegetação permanecem fracos; as imagens não demonstram claramente o desaparecimento gradual do fundo em profundidade.
- **AC-04 — FAIL:** há espuma visível em pedra e pilar e os controles seco e suspenso parecem limpos; a espuma do casco continua estreita e descontínua na saída de 480 × 270. A variação temporal dos contatos não pôde ser julgada pela reprodução contínua.

O Editor permanecia em Play, com compilação sem falha, zero erros atuais e uma advertência. Não foram alterados assets; desempenho e câmera real permanecem sem validação.

## Prova costeira e cáusticas

Examinei `wash-revised.png`, `wash-caustics-day.png`, `wash-caustics-no-sun.png` e quadros 0, 20, 40, 60, 80 e 100 de `wash-two-cycles.gif` (110 quadros, 480 × 270, 150 ms declarados). As posições e a largura da faixa mudam, mas não observei a reprodução contínua: a política do navegador bloqueou anteriormente a abertura de arquivos locais. A comparação sem sol altera a iluminação inteira e não equivale ao ciclo completo de noite.

O shader `PrototypeSand.shader:37` fixa a equação da margem à praia de teste; a crista, a cobertura e a umidade alteram a cor da areia (`:43`–`:52`), com alcance baseado apenas na fase atual e anterior, sem histórico de secagem. As cáusticas são limitadas pela altura mundial e profundidade em relação a zero (`:54`–`:63`), uma hipótese específica do nível do mar da prova; somente a malha com esse material recebe o efeito. Outras topografias, níveis de água, objetos submersos e movimento de câmera continuam sem validação. O Editor permanecia em Play, sem falha de compilação e com zero erros atuais e uma advertência. Não alterei assets.

## Reavaliação da emenda e comparação controlada

Examinei `seam-blend.png`, `controlled-comparison.png` e quadros 0, 20, 40, 60, 80 e 100 de `blended-two-cycles.gif` (110 quadros de 480 × 270, 150 ms declarados). Não observei a reprodução contínua. **AC-05 permanece BLOCKED:** os quadros mostram posições diferentes da crista, mas não comprovam visualmente dois ciclos completos e suaves. **AC-06 permanece FAIL:** a abertura anterior diminuiu, porém a mudança de cor e segmentos de crista destacados continuam visíveis; a independência da câmera não foi testada.

**AC-07 permanece BLOCKED:** as fases fixas de avanço, recuo e secagem indicam mudança na faixa, mas não demonstram claramente secagem gradual em um ponto após a passagem. **AC-08 permanece BLOCKED:** a comparação controlada mostra padrão mais fraco com profundidade substituída por 2,5 m do que por 0,25 m; são valores sintéticos aplicados ao fator, não fundos reais em profundidades diferentes. A comparação com luz desligada anterior também não é noite completa.

Na versão então avaliada de `WaterPrototype.shader`, o fade multiplicava a opacidade final, incluindo a espuma de contato. A correção posterior é avaliada abaixo. A fórmula da margem continua fixa à cena de prova. Console então: zero erros, uma advertência e nenhuma falha de compilação; não houve profiling nem alterações de assets nessa revisão.

## Correção do fade de contato

Em `WaterPrototype.shader:98`, o fade agora multiplica somente a opacidade base; a espuma entra separadamente no máximo da opacidade. Nas imagens `shore-contact-fixed.png` e `shore-contact-phase04.png`, a pedra identificada na documentação como controle dentro da faixa apresenta um contorno claro nas duas fases. Isso confirma que o fade não apaga totalmente esse contato nas condições capturadas; o contorno é estreito e as duas imagens não demonstram todos os ângulos, objetos ou movimento contínuo.

O defeito específico de aplicação do fade à contribuição de espuma foi corrigido no código e o caso da pedra tem evidência visual parcial. **AC-04 permanece FAIL**, pois não há nova evidência suficiente para o casco e a variação temporal exigida. **AC-05 e AC-07–08 permanecem BLOCKED; AC-06 permanece FAIL** pelos limites já registrados. Console atual: zero erros, uma advertência e compilação sem falha. Nenhum asset foi alterado nesta revisão.

## Verificação direta da integração provisória — 25/11/2026

Abri uma instância Unity separada, explicitamente apontada para `/.utmp/079-sistema-agua/Freyja`, versão `6000.3.25f1`; o workspace principal permaneceu em outro Editor e não foi alterado. A cena ativa foi `Assets/_Project/Scenes/Ilha_Principal.unity` e ficou limpa após o teste. No objeto `/Island023/Mar`, o `MeshRenderer` referencia `Assets/_Project/Art/Water079/IslandWaterMain.mat` (GUID `53e4b6c9d6fad44b381db82251819141`); o material usa `Freyja/Island Water 079` e referencia `Mar049/CoastDistance`. A câmera `Cais Gameplay Camera` mantém como alvo `Ilha_Tripo_011/Mundo_480x270.renderTexture` (480×270).

A captura da Game View em Play Mode está em `Artifacts/water-validation/079-main-play.png` (960×540). Nela, água e ondas são visíveis no píer, com espuma junto à borda e ao casco; a composição pixelada e os controles do jogo também aparecem. Isso comprova renderização da integração na câmera/saída real sob essa condição, mas não é comparação antes/depois nem prova ausência de regressão em outras vistas. Por isso, AC-11 continua **BLOCKED** para o critério completo. Esta evidência não altera os resultados independentes do núcleo: AC-02/03/04/06 continuam FAIL; AC-05/07/08 continuam BLOCKED; AC-09 NOT RUN; AC-10/12/13/14 continuam BLOCKED.

No início da sessão, o Editor registrava compilação concluída e zero erros. Durante a inicialização, o Console registrou um erro interno da indexação do Unity Search (`SearchDatabase.EnumerateAll` → `SearchInit.IndexationOnStartup`, `ArgumentOutOfRangeException`), sem stack de shader/água; também havia warnings. Não atribuo esse erro ao sistema de água. A captura foi feita durante Play Mode; depois, a cena continuava limpa e o Editor isolado ficou parado. Nenhuma alteração foi salva na cena nesta verificação. Não houve profiling, comparação ligada/desligada, sequência contínua de ciclos, validação de transparências ou variação de horário/qualidade.

## Vento global e costa na cena real — 25/11/2026

`Water079/IslandWater.shader` consome `_FreyjaWind` e `_FreyjaWindGust`, já publicados por `GlobalWindController` na cena principal. O vento modula a amplitude, sem alterar as direções fixas nem a frequência/fase das ondas e ripples. Não houve mudança no controlador, na cena, no material do terreno ou em GUIDs.

No Editor isolado, `GlobalWindController` estava ativo com preset Breeze. `Shader.GetGlobalVector` leu Breeze `(x=.80, y=.60, strength=.55)` / rajada `.50` e, após a troca temporária em Play Mode, Strong `(x=.80, y=.60, strength=1)` / rajada `.85`; o valor retornou a Breeze antes de parar. `material list_shaders` confirmou `Freyja/Island Water 079` como suportado. `079-main-wind-play.png` e `079-main-wind-strong.png` são capturas de render nos dois estados, mas o tempo de dia/noite também avançou entre elas; não as trato como comparação visual controlada nem como medição da transição suave. O Editor terminou com `compiling=false`, `compilationFailed=false` e a cena limpa.

A costa real usa `Mar049/CoastDistance` no material da água; o shader calcula `shoreFoam` com essa textura. Cinco capturas `079-coast-00.png` a `079-coast-04.png`, espaçadas por aproximadamente 1,8 s mais o tempo de captura, abrangem mais de dois períodos teóricos dessa fase. A faixa fina no limite píer/água aparece, porém a câmera de jogo não mostra com clareza dois ciclos de avanço e recuo na praia. Testei temporariamente alargar a crista no shader; a captura `079-main-coast-wide.png` não mostrou melhoria legível, então o ajuste foi revertido. AC-05 segue BLOCKED e não há alteração de areia/terreno.

As cáusticas de `IslandWater.shader` reconstruem a posição do fundo a partir da profundidade da câmera e somam cor ao próprio fragmento transparente da água; esse shader não escreve cáusticas sobre a areia opaca. A cena não tem um receiver de decal seletivo para esta costa. Sem um passe de câmera/decal isolado ou uma alteração do material do terreno (fora do escopo), projetar cáusticas sobre a praia real não é seguro. O teste adicional não mediu custo: depois das reimportações, o Console totalizou cinco ocorrências da exceção interna `UnityEditor.Search.SearchDatabase` descrita acima, sem falha de compilação ou stack de água.

## Regressão da fase do vento — 26/11/2026

Na revisão, removi a rotação de direção e o multiplicador de vento aplicado a `WaterTime()`. As coordenadas/direções espaciais e a fase/frequência temporal agora ficam fixas. `WindAmplitudeScale()` usa `1 + saturate(strength) * (.12 + .04 * sin(_FreyjaWind.w) * saturate(gust))`; o controlador integra `_FreyjaWind.w` continuamente, e seus presets blendam força/rajada. Sem controlador, os globals zero deixam a escala em 1. A direção configurada no controlador ainda não orienta as ondas; a integração atual limita-se à amplitude para preservar continuidade.

No Editor isolado, apliquei `_PreviewTime = 10000` apenas à instância runtime do material, sem salvar o asset. Uma amostra contínua a cada atualização durante a troca Strong subiu de força `.651` a `.999` em `1.20 s`; na volta a Breeze, amostras a cada `0.20 s` desceram de `.929` para `.551` em `1.20 s`. A fase global avançou monotonicamente nas amostras (Strong: `134.143` → `137.433`; Breeze: `309.242` → `311.477`). `079-wind-long-breeze.png`, `079-wind-long-strong.png` e `079-wind-long-breeze-return.png` mostram a Game View no início, no preset Strong e no retorno; não observei deslocamento abrupto da água. As imagens são referências, não vídeo contínuo.

Após o teste, Play Mode foi encerrado; `Ilha_Principal` permaneceu limpa, `compiling=false`, `compilationFailed=false` e `Freyja/Island Water 079` permaneceu `isSupported=true`. As exceções restantes no Console continuam sendo da inicialização do Unity Search, sem stack de água. O teste verificou uma fase alta e as duas transições de preset, mas não mede custo nem valida todos os horários/qualidades.

## Espuma sobre a praia real — 26/11/2026

`Mar049/CoastDistance` está vazio (amostras nos cantos e centro retornam branco), então não fornece uma máscara útil da praia. Para investigar uma alternativa localizada, criei `TerrainShoreFoamOverlay` e o shader `Freyja/Beach Sand Foam 079`. O componente lê a camada `BeachSand` e a altura do `TerrainData`, gera malha nos quadrantes arenosos dentro do alcance do nível d'água e usa alpha por vértice; não altera `TerrainData`, o material do terreno ou as texturas. A malha experimental chegou a 24.815 quadrantes. A fase temporal é periódica (4,76 s), a variação espacial é limitada a ±0,12 e a opacidade padrão é 0,55.

Em Play Mode do Editor isolado, Unity `6000.3.25f1`, cinco capturas `079-integrated-beach-00.png` a `079-integrated-beach-04.png` (960×540) mostram movimento por mais de dois períodos. A revisão independente rejeitou a aparência: as cristas viram várias faixas brancas largas através da praia, chegando perto das casas, em vez de uma espuma costeira estreita. Desativei e depois removi a instância de `TerrainShoreFoamOverlay` da cena principal. Os scripts, shader, material e capturas experimentais foram preservados; a cena de produção não usa o overlay. O filho residual apareceu somente em Play Mode quando a instância ainda existia: após parar, o Editor mostrava zero filhos; o YAML da cena não continha `Coastal Foam Overlay` nem `Generated BeachSand Foam Overlay`. Portanto, era gerado durante execução pelo ciclo de vida do componente, não serializado na cena nem cache visível no Editor; o callback exato que o gerou não foi isolado. Após remover a instância, Play Mode confirmou `componentCount=0` e `generatedOverlayObjects=0`; `079-overlay-removed-main.png` registra o enquadramento real sem as faixas sobre o terreno. O componente continua experimental e fora da cena.

Não há escurecimento/secagem da areia molhada (`FR-005`), e a máscara experimental não passou no gate visual de forma. Não foi validada a costa curva completa, controles negativos, quatro horários, níveis de qualidade ou custo de GPU. `SC-002` permanece incompleto; T005/T006 e a spec não estão aprovadas. O Console não apontou falha de compilação nem erro de shader; persiste uma exceção interna do Unity Search (`SearchDatabase.EnumerateAll` / `IndexationOnStartup`) sem stack de água.

## Cor noturna da água — 26/11/2026

Reproduzi a iluminação pela API da cena (`CoastalDayNight.SetHour`) em 18h e 21h, na cena real e saída 960×540 da câmera que usa `Mundo_480x270`. O comparativo pareado durante o protótipo está em `079-main-night21-before-clean.png` e `079-main-night21-after-clean.png`; as duas capturas mostram água azul-escura e céu com tom púrpura/rosado. Não consegui reproduzir água rosa nessa câmera específica. Para reduzir aquecimento de cor em outros ângulos, `IslandWater.shader` transita a água para um azul mais escuro quando a elevação da luz cai abaixo de `.18`. As cáusticas multiplicam a mesma atenuação diurna e ficam zeradas quando a luz direta está abaixo do horizonte/intensidade noturna. `079-overlay-removed-main.png` foi capturada depois de retirar o overlay costeiro; mostra a cena final sem as faixas rejeitadas.

No teste, travei o relógio em 21h e não salvei estado temporário. O shader permaneceu suportado pelo Editor, sem erro de compilação ou stack de água; `IslandWaterMain.mat` e seus valores não foram alterados. A sequência não demonstra cáusticas projetadas sobre terreno opaco nem identifica a percepção em todas as câmeras, e não mede desempenho. O Console isolado segue contendo exceção interna de Unity Search (`ArgumentOutOfRangeException` em `SearchDatabase.EnumerateAll` durante `IndexationOnStartup`), registrada antes das capturas e sem relação com o shader.
