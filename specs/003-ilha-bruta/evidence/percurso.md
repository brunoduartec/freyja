# Validação da ilha bruta — 12/11/2026

Unity 6000.3.25f1; cena `Assets/_Project/Scenes/Ilha_Bruta_003.unity`; execução em Play Mode. O [guia de avaliação](../quickstart.md) explica como experimentar a cena.

## Percurso e colisões

- Um percurso guiado por entrada sintética percorreu continuamente 186 pontos: cais, largo, setores B, L e A, mirante, retorno pelos fundos, com aproximações à horta e à ruína. Resultado: 232,95 s, 698,99 m horizontais, zero recuperações e zero quadros com colisão lateral. Registro: [percurso principal](percurso-route-20261112-213054.json) e [configuração](validation-config-route.json).
- O terreno e os 12 blocos têm colisores; as três fitas de caminho são visuais. O mar de 600 × 600 m não tem collider. Há quatro blocos por bairro.
- Num segundo teste, o personagem foi guiado intencionalmente para fora do cais. Retornou ao spawn; o roteiro continuou tentando alcançar a água e registrou cinco recuperações antes de parar por falta de progresso. A indicação `stuck` nesse teste é esperada para esse alvo no mar, não é resultado do circuito principal. Registro: [queda](percurso-route-20261112-213502.json) e [configuração](validation-config-queda.json).
- Após remover os costões antigos sobrepostos ao mar e preencher uma junta de 3 cm no cais, a travessia entre o spawn e a ilha foi repetida: concluída em 5,37 s, sem recuperação. Registro: [travessia após ajuste](percurso-route-20261112-214352.json).
- O limite de recuperação tem centro `(0, 11,05, -94)` e semiextensões `(148,31, 50,05, 110)` na Unity; contém os limites do terreno e o spawn. A câmera tem alcance de 344,1 m.

## Controles e limites da validação

- Eventos sintéticos do Input System confirmaram movimento W por 2,4 m em 0,8 s, rotação da câmera, liberação pelo Esc, imobilização sem captura e retomada por clique. Não houve recuperação durante o teste. Registro: [controles sintéticos](synthetic-input-20261112-214407.json).
- A automação de interface retornou `noWindowsAvailable` ao tentar clicar na janela da Unity. Portanto não houve teste com teclado e mouse físicos, e as capturas não substituem a avaliação do usuário. A câmera foi observada nas capturas, mas não foi avaliada manualmente em todos os obstáculos.
- O caminho visual exige desvios curtos para chegar aos canteiros e à ruína. Distâncias e pontos usados estão na configuração do percurso e em [medidas.md](medidas.md).

## Imagens e preservação

- [Vista geral Blender](ilha_bruta_overview.png): imagem estática da maquete, sem o cais exportado.
- [Vista final do cais na Unity](unity-cais-final.png): captura estática em Play Mode depois da correção local da junta e remoção dos costões sobrepostos.
- [Vista do cais na Unity](unity-cais.png): captura inicial antes da remoção dos costões antigos; mostra a integração em Play Mode e não representa a correção visual final.
- [Durante o percurso](unity-percurso.png): captura em Play Mode durante o circuito automatizado, antes da correção local da junta do cais.
- Os hashes finais da fonte salva do cais, cena 002, dois prefabs compartilhados, manifest e versão do editor coincidem com [source-hashes.json](source-hashes.json). A cena 003 foi reaberta limpa, com prefab, 13 colisores e peça da junta presentes; a fonte Blender foi reaberta durante a exportação. O console contém dois erros de comandos de ferramenta não reconhecidos (`get` e `play`), e não erros do jogo observados nos testes; não foram apagados para preservar o histórico.
