# Comparação do relevo — revisão 02 (2026-11-13)

Estado: maquete revisada e renderizada; aguardando avaliação visual. Não foi declarada idêntica, nem integrada ao Unity nesta revisão.

## Regra de precedência

`Referencias/modelagem-vista-aerea.png` é o oráculo, por instrução explícita do usuário nesta revisão. A topdown e as quatro laterais continuam sendo comparadas, mas cedem em conflitos. Em particular, a abertura para a praça sugerida na vista norte não foi adotada quando interromperia o maciço posterior da aérea. A casa isolada e a face posterior usam informações complementares onde o oráculo não mostra a superfície.

## Alterações e evidências

- A malha anterior extraía areia de telhados, caminhos e praça e transformava esses pixels em depressões. A nova superfície utiliza contorno, cristas e cotas explícitos; não usa cor da imagem como altitude.
- Removido o achatamento global dos picos em 24 m. Maciço contínuo, coroas irregulares, paredões e contrafortes modelados com patamares posteriores.
- Costa triangulada a partir de um contorno contínuo; eliminadas bordas em degraus da grade e a saia vertical única. Praia limitada à enseada sul.
- Patamares residenciais e da horta reorganizados; praça circular, caminhos em fitas de superfície e subida sinuosa com suporte de terreno. A navegabilidade é provisória e não aprovada.
- Reutilizados blocos de casas, ruína, horta e cais da 004 na cópia 005. Telhados provisórios ajudam a leitura de escala; arquitetura permanece placeholder.
- Sete capturas produzidas: aérea, superior inclinada, planta vertical e norte/sul/leste/oeste. Perfis usam a mesma escala e elevação relativa da câmera.

[Comparação lado a lado das seis referências](revisao-02/comparacao.html).

## Limites da comparação

A correspondência está melhor que na maquete anterior, mas não é exata: distribuição fina das cristas, recortes costeiros, largura dos patamares, proporções da enseada e posições locais ainda requerem avaliação visual. Não há medidas 3D nas ilustrações que determinem univocamente a câmera ou a superfície oculta. Nenhum percentual de semelhança foi inferido. Vegetação, texturas, arquitetura e objetos detalhados não foram reconstruídos nesta revisão de relevo.

Os perfis laterais expõem as superfícies não visíveis no oráculo; são uma reconstrução orientada pelas referências complementares, sem alegação de identidade geométrica certificada.

## Verificações

MCP interativo indisponível em localhost:9876; edição, render e reabertura executados pelo MCP global Blender em segundo plano. A fonte 005 foi salva, reaberta e inspecionada. Métricas e verificações em `revisao-02/geometry.json`, `revisao-02/reopen-check.json` e `revisao-02/preservation-check.json`. Originais 004 e referências preservados por SHA256. Nenhum export ou teste Unity executado nesta revisão.

A presença de faces com 70–80° comprova inclinações na geometria, não colisão ou bloqueio de caminhada. Esses comportamentos seguem pendentes na fase US2.

## Artefatos

- Fonte atual: `Source/Blender/Ilha_Relevo_005.blend`.
- Backup anterior à revisão: `Source/Blender/Ilha_Relevo_005_pre_revisao02.blend`.
- Receita executável no Blender MCP: `Tools/Blender/revise_relief_005.py`. Executar sobre o backup anterior à revisão, não sobre o resultado, pois reposiciona elementos reutilizados.
- Capturas anteriores preservadas em `evidence/`; novas capturas em `evidence/revisao-02/`.

## Abertura para avaliação

Ao final, o Blender foi aberto com `Ilha_Relevo_005.blend`. O MCP interativo passou a responder e confirmou o arquivo correto. A janela 3D foi conferida por captura, com câmera aérea ativa, prévia de materiais e enquadramento ampliado. A fonte foi salva novamente com essa apresentação.

## Revisão 03 — segunda rodada solicitada em 2026-11-13

O usuário considerou a revisão 02 parcialmente melhor e solicitou nova rodada. A aérea continua sendo o oráculo.

- Substituídas cristas fragmentadas por uma coroa ampla, com ondulações baixas, paredão dominante e três contrafortes inferiores.
- Reduzidos os patamares contínuos da vila; criadas plataformas locais para grupos de casas e ruína, preservando os três níveis da horta.
- Ampliada a faixa de areia na enseada. Reenquadrada a câmera aérea e ajustada a orientação do cais, mantendo os componentes originais.
- Acrescentados marcos volumétricos provisórios de torre da igreja, poço e armazém para facilitar a comparação espacial. Não representam arquitetura final.
- Sete capturas regeneradas em `revisao-03/`. [Comparação das seis referências](revisao-03/comparacao.html). A planta vertical é uma captura adicional.

Backup da revisão 02: `Source/Blender/Ilha_Relevo_005_pre_revisao03.blend`. Receita: `Tools/Blender/revise_relief_005_r03.py`, executada sobre `Ilha_Relevo_005_pre_revisao02.blend` para evitar acumular transformações.

Limites: a fidelidade visual ainda requer avaliação, especialmente recortes locais, cotas e proporções entre praia, vila e maciço. As mudanças são na maquete Blender; não houve export, validação de colisões ou teste de navegação Unity. As referências e os quinze originais auditados foram preservados por SHA256.

## Revisão 04 — 2026-11-13

Rodada solicitada pelo usuário após a revisão 03. Referência prioritária mantida: `modelagem-vista-aerea.png`.

A reconstrução passou a usar pontos e contornos traçados na aérea: crista, borda frontal do maciço, costa, praça, ruína, três níveis da horta e plataformas residenciais. As posições foram convertidas da imagem para o espaço 3D usando a mesma câmera aérea da revisão 03 e cotas estimadas; não são medidas reais deduzidas da imagem. Isso corrige a vila avançada em direção à praia e a falta de extensão do maciço na revisão anterior.

A costa e a enseada foram refeitas com o contorno da aérea. A parte posterior, oculta no oráculo, foi reconstruída com patamares e faces íngremes, orientada pelas vistas complementares. Essa porção continua inferida; a casa isolada foi posicionada em um patamar posterior. Nenhuma das imagens adicionais autoriza contradizer uma superfície visível na aérea.

Os blocos reutilizados receberam suporte de terreno; igreja, casas e cais continuam marcos provisórios de escala. A receita gera diretamente a fonte 005 pelo MCP Blender. Não houve exportação ou modificação no Unity. A navegabilidade e a passagem entre todos os patamares ainda precisam de validação na fase seguinte.

[Comparação: referência, revisão 03 e revisão 04](revisao-04/comparacao.html). Sete capturas em `revisao-04/`; câmera superior e perfis reenquadrados para mostrar a ilha inteira após o ajuste do contorno. A câmera aérea de calibração permanece fixa.

Dados auditáveis: `revisao-04/oracle-landmarks.json`, `geometry.json`, `reopen-check.json` e `preservation-check.json`. Os quinze originais auditados foram preservados por SHA256. A presença de faces entre 70–80° não certifica bloqueio de caminhada.

Artefatos: `Source/Blender/Ilha_Relevo_005.blend`; backup da revisão 03 em `Source/Blender/Ilha_Relevo_005_pre_revisao04.blend`; receita `Tools/Blender/revise_relief_005_r04.py`. Executar a receita sobre `Ilha_Relevo_005_pre_revisao02.blend`, não sobre o resultado, para não acumular transformações dos proxies.

Limites: ainda há diferenças de recorte fino, formas locais e largura de patamares. Arquitetura, materiais e vegetação não são finais. A reconstrução e os contornos traçados aproximam a composição do oráculo; não demonstram identidade visual ou geometria única. Aprovação visual permanece pendente.
