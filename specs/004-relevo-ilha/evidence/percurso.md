# Encerramento da validação — Spec 004

A spec foi encerrada a pedido do usuário. O relevo será refeito em outra spec para corresponder à referência, permitindo caminhada apenas onde a inclinação for menor que 45°. Essa nova regra não foi aplicada retroativamente ao modelo 004.

## Resultados registrados

- O percurso guiado em Play Mode visitou 49 dos 54 pontos, em 496,9 segundos, com zero recuperações inesperadas. Foi interrompido pelo detector de falta de progresso no ponto `north_clearance_return`, a 6,30 m do alvo. Portanto, o circuito completo não foi aprovado. Registro: [percurso](percurso-route-20261113-000947.json).
- A análise de todas as 7.290 faces mediu inclinação máxima de 32,696°, sem faces acima de 35°. Após remover a trilha antiga sobreposta ao cais, a grade encontrou um componente com 87.783 células válidas. Há exclusões na borda e em volumes ocupados; a amostragem não comprova todos os pontos infinitos da superfície. Veja [caminhabilidade.md](caminhabilidade.md).
- A trilha antiga `Inicio_do_caminho.001` foi desativada apenas na cena 004. Ela ficava até 1,88 m acima do novo terreno. O caminho novo permanece visível e a passagem pelo cais foi percorrida durante o teste.
- O novo visual Male_Casual foi aplicado somente ao jogador da cena 004, mantendo o controlador e referências de câmera e sessão. Veja [personagem.md](personagem.md).
- A visibilidade da casinha desde o largo foi verificada geometricamente no Blender: seis linhas de visão bloqueadas pelo terreno. A experiência visual completa e os controles físicos não foram aprovados pelo usuário.

## Imagens

- [Vista aérea estática Blender](vista-aerea-004.png) e [vista superior estática](vista-topdown-004.png).
- [Personagem na cena](unity-player004-front.png), [junção após correção](unity-cais-path-after.png) e [captura durante percurso](unity-percurso-004.png).

## Pendências mantidas explícitas

O travamento do percurso, a repetição completa dos controles, queda na água e revisão final de câmera/console não foram encerrados com aprovação. A pedido do usuário, não haverá mais retrabalho do relevo nesta spec. A comparação final dos hashes está em [preservacao-final.json](preservacao-final.json).
