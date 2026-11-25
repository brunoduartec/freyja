# Validação — Ilha Principal

## Inventário e autoria

- Unity 6000.3.25f1, projeto `Freyja/`. Uma cena carregada antes e depois, aberta em Edit Mode sem alterações pendentes.
- 3.432 objetos em 24 raízes antes e 2.363 objetos em 14 raízes depois. O [inventário](inventario.tsv) registra a decisão dos 3.432 objetos e os motivos. Foram removidos 1.069 objetos, incluindo 395 dos oito conjuntos antigos.
- O GUID `653b506f34b104a8383faa4a5c094893` e o conteúdo do `.meta` foram preservados. A cena oficial está habilitada na lista de cenas do build.
- Os 2.363 objetos mantidos conservaram IDs e os mesmos caminhos, estados, componentes, prefabs e posições no inventário. As 1.299 referências internas serializadas entre objetos mantidos são idênticas. Nenhuma referência de um objeto mantido apontava para um objeto retirado. Zero scripts ausentes ou referências quebradas nas duas inspeções no Editor.
- A validação independente dos arquivos verificou 4.113 seções YAML antes e 2.983 depois, com zero referências locais quebradas. As 23 seções retidas que mudaram pertencem à hierarquia ou à serialização de overrides de prefabs.
- As ferramentas de vegetação, diálogo, waypoints e autoria da ilha foram atualizadas. As rotinas de preview 020 e World Builder continuam vinculadas à cena histórica, assim como seus snapshots. O antigo spawn desses snapshots já tinha sido substituído pelo spawn da ilha atual.

## Play Mode

- Chegada inicializou em `(-33.54, 1.77, -61.18)` com a câmera de jogo e o HUD. A [captura da chegada](evidence/chegada.png) é idêntica byte a byte à captura anterior, SHA-256 `8b9264085d60af78047ae2dc9198165ffa5c964edb36000929993a3934245479`.
- O controlador do jogador percorreu 79,12 m com colisões ativas, atingiu 202 de 202 pontos e terminou no chão no [largo](evidence/largo.png), com zero recuperações. O percurso anterior chegava à posição ocupada por Nuno. O teste contornou o morador pela esquerda.
- Na [escada L3](evidence/escada-l3.png), o controlador foi do primeiro degrau até a varanda, ganhou 2,71 m de altura e terminou apoiado, com zero recuperações. A navegação registrou `PathComplete`.
- Com eventos de teclado no jogo, `E` coletou a boia de pesca e o inventário registrou quantidade 1. Um novo `E` abriu o [diálogo com Nuno](evidence/dialogo-nuno.png) e `Esc` fechou o painel e devolveu o controle.
- Compilação concluída sem falha. O Console registrou zero erros durante a validação jogável. Durante a recarga anterior, o servidor do Unity Pipeline registrou dois erros transitórios de conexão com `ObjectDisposedException`, recuperados no próprio Editor. Permaneceram avisos do pacote Unity AI sobre acesso à API de conta.
- Ao sair de Play Mode, o Editor permaneceu com apenas `Ilha_Principal` aberta, ativa e salva, e zero erros no Console.
