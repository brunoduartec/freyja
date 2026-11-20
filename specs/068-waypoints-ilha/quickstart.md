# Validação no Unity

1. Abra Ilha_Tripo_Caminhada_011 e selecione Waypoints da ilha para visualizar os pontos na cena.
2. Em Play Mode, selecione um NPC equipado com IslandWaypointWalker. Atribua um destino da rede em Destination e use Caminhar até destino no menu de contexto do componente.
3. Repita com outro NPC. Durante a caminhada, atribua outro destino e execute o menu de novo.
4. Valide píer, largo, acessos das casas, leste, igreja, colina e escadarias. Teste também um caminho bloqueado e confira que o NPC para antes da barreira.

## Cobertura da cena

| Grupo | Pontos | Referência visual |
| --- | ---: | --- |
| Destinos principais | 12 | Amarelo |
| Acessos às edificações | 17 | Laranja |
| Distribuição pelo terreno | 340 | Ciano |
| Área isolada na encosta nordeste | 1 | Rosa |

Veja [o mapa de cobertura](waypoints-ilha.png). Os 369 destinos da rede principal têm caminho completo a partir do Largo. O ponto isolado fica em (45,36 m, 1,54 m, 35,19 m), abaixo da encosta nordeste. Um percurso solicitado da crista até esse ponto foi recusado em Play Mode. As fachadas fechadas são atendidas pelo lado externo. A área acessível da banca da Venda e a varanda de L3 têm destinos próprios.

Quinze NPCs de Elenco 045 e Moradores060 receberam o componente de navegação. Use o campo Destination e o menu de contexto para uma chamada manual ou o método TryMoveTo para uma rotina do personagem. Bento usa a rotina existente da banca.

## Evidências em Play Mode

- Nuno caminhou até o convés, recebeu um novo destino durante o retorno e chegou ao Largo a 0,134 m do waypoint. Depois caminhou até a crista e voltou ao waypoint do convés.
- Teresa caminhou do pé da escada até a varanda L3 e parou a 0,136 m do destino. Também chegou à praia na rua leste.
- Amiga de coral chegou ao ramal da casa isolada. Artur chegou ao acesso B3 e Elisa à banca da Venda.
- Um bloqueio temporário no trajeto do Largo à Venda fez Nuno parar 0,24 m antes da face do obstáculo.
- A checagem de caminhos em Play Mode aprovou os 369 pontos conectados. Os deslocamentos acima foram executados com NPCs reais.
- A compilação e o Console ficaram sem erros após a última correção. O Console registrou um aviso do serviço de conta do Unity AI Toolkit.
- Os quatro destinos principais restantes foram percorridos. NPCs chegaram ao patamar da Igreja, à subida da rua leste, à saída do píer e à curva do cais. Todos os 12 destinos principais foram alcançados em caminhada.
