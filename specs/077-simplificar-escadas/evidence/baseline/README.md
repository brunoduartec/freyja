# Baseline da L3

Unity 6000.3.25f1, Play Mode via Unity Pipeline, cena `Ilha_Principal`. O protagonista real iniciou no terreno antes do lance. O percurso adaptou o procedimento de `Tools/Island023/Record.cs`: comandos direcionais via `SetMoveInput` durante o ciclo do jogo e leitura da RenderTexture da câmera. A sessão permaneceu ativa com a captura de entrada liberada, mantendo a contagem de recuperações. Os colisores, a pose de escada e o controlador estavam na configuração original.

| Trecho | Resultado |
|---|---|
| Base e subida | Do terreno até a varanda, de X local −5,10 m a −0,25 m. Apoiado em todas as amostras de subida. |
| Pausa no meio e na varanda | Coordenadas estáveis a 0,1 mm de resolução durante 0,80 s e 0,65 s, respectivamente. |
| Descida | Retorno ao terreno em X local −4,95 m, apoiado ao fim. O controlador indicou ausência de apoio em 100 das 4.934 amostras desse trecho. |
| Recuperação e Console | Zero recuperações. Após sair de Play Mode, zero erros, um aviso e sem falha de compilação. |

As 11.027 observações foram coletadas durante um ciclo de 11,72 s. O [CSV](percurso.csv) contém 241 amostras, com aproximadamente 20 Hz e os extremos de cada trecho. A contagem de amostras mede sinal de apoio nessa execução e não serve como métrica de CPU.

Sequência da câmera de produção em `Mundo_480x270`. [Base](base.png) · [Subida](subida.png) · [Pausa](pausa.png) · [Transição superior](transicao-superior.png) · [Varanda](varanda.png) · [Descida](descida.png) · [Retorno](retorno.png)

Em uma segunda sessão, Teresa Faria partiu do ponto de navegação no pé da L3 e usou `IslandWaypointWalker.TryMoveTo` para ir à varanda e voltar. Os dois comandos foram aceitos e o agente completou ambos os percursos, terminando a cerca de 0,14 m de cada ponto, dentro da tolerância aplicada pelo walker. A ida ganhou 2,65 m de altura e a volta desceu 2,55 m.

A baseline do jogador cobre um percurso longitudinal com caminhada e pausas. Corrida, entrada diagonal, limites laterais e perfil de CPU ainda requerem ensaios próprios.
