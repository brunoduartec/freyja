# Pesquisa e decisões — Spec 005

## Referência visual

- **Decisão:** tratar a imagem top-down, a perspectiva aérea e os quatro perfis norte/sul/leste/oeste como conjunto mandatário de exatidão, cada qual na sua vista.
- **Razão:** a top-down fixa disposição e caminhos; os perfis não existiam e foram criados como propostas para orientar as alturas. A captura da 004 mostra um platô arredondado, diferente das colinas e falésias desejadas.
- **Alternativas rejeitadas:** reduzir qualquer imagem do conjunto a inspiração ou referência de estilo; o usuário determinou que todas orientem a exatidão.

As imagens laterais geradas integram o conjunto de referências mandatórias conforme o pedido do usuário; cada perfil deve corresponder à sua direção. Qualquer conflito entre vistas deve ser apontado e resolvido com o usuário antes de alterar o relevo correspondente.
- **Interpretação do usuário:** algumas faces das colinas devem alcançar aproximadamente 70–80°. Medir e registrar as inclinações reais na geometria criada; a imagem é ilustrada e sua projeção não é instrumento de medição.

## Regra de caminhada

- **Decisão:** superfícies livres com inclinação inferior a 45° são candidatas obrigatórias a acesso; faces a partir de 45° podem bloquear a caminhada. Preservar faces de 70–80° como paredes rochosas. Acessibilidade efetiva depende ainda do controlador, colisões, folgas e conectividade.
- **Razão:** essa regra substitui explicitamente a meta anterior de até 35° e acesso a toda a superfície da 004. O controlador 004 está registrado com limite de 60°, step de 0,25 m, raio de 0,3 m e altura de 1,8 m, sujeito a inspeção na cena 005.
- **Alternativas rejeitadas:** aplainar todas as colinas abaixo de 35°, o que elimina a forma de paredão pedida; alterar o limite do controlador para produzir aparência de sucesso, o que muda a sensação de caminhada e não prova conectividade.

## Reaproveitamento e validação

- **Decisão:** aproveitar individualmente elementos compatíveis da fonte Blender, cena, export e prefab 004, incluindo personagem, cais, barco, volumes, materiais, trilhas e comportamento de recuperação. Criar ativos isolados para a 005. Reutilizar `Tools/Unity/AnalyzeIlhaRelevoWalkability.cs` somente após conferir faixa de inclinação, obstáculos, amostragem e limitações.
- **Razão:** a 004 oferece uma base navegável e registrou geometria útil, mas seu percurso em Play Mode parou em 49/54 pontos e suas verificações não substituem o movimento físico do controlador nem inspecionam a malha cozida do PhysX.
- **Alternativa rejeitada:** tratar resultados 004 como evidência de conformidade 005; cenário, geometria e regra de caminhada mudaram.

## Incerteza

A imagem não fixa escala ou geometria oculta. Dimensões e face posterior serão propostas de modelagem, identificadas como inferências e submetidas à avaliação visual; não há clarificações que bloqueiem o planejamento.

## Prioridade das referências — decisão de 2026-11-13

Por instrução explícita do usuário, `Referencias/modelagem-vista-aerea.png` é o oráculo. Em qualquer divergência de relevo, proporção, altura, silhueta ou disposição dos patamares, essa imagem prevalece sobre a topdown e as quatro vistas laterais. As demais imagens continuam obrigatórias para comparação, mas não podem impor uma forma incompatível com o oráculo. A vista norte não autoriza abrir um vale que interrompa o maciço contínuo da aérea. A aprovação de exatidão depende da comparação visual da maquete; contagem de faces íngremes não prova semelhança.
