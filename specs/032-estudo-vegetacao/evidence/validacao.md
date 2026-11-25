# Primeira avaliação de vegetação

## Cena e paleta

A cena principal foi salva no worktree da branch `032-estudo-vegetacao`, sobre uma cópia da cena local em uso em 18/11/2026. A raiz `Vegetação 032` contém o coqueiro, a cobertura base com 88 elementos e um complemento com 59 elementos que pode ser ativado no Inspector. As duas variantes partilham a grama costeira de fundo. O complemento ficou desligado na cena salva.

Modelo da praia `PalmTree_4.blend`, com cocos e tronco curvo. Preserva-se o original em `Low Poly World/downloads/nature`, junto a `Bush_1.blend` e `Flowers.blend`. Os FBX e os materiais adaptados estão em `Freyja/Assets/ThirdParty/Nature032`. O coqueiro tem aproximadamente 3,92 m de altura e foi assentado em (27,071 m, 2,249 m, −28,294 m), na borda seca da praia.

## Evidências

- [Praia com coqueiro](praia.png) · [Vista a partir da praia](praia-retaguarda.png).
- [Comparação no cais, praça e igreja](comparacao.png). Imagens renderizadas a 480 × 270 pelo Unity, com parâmetros da câmera do jogo e enquadramento fixo entre variantes.
- [Amostras de deslocamento na praça](amostras-movimento.png). As moitas se mantêm legíveis na curta sequência de câmera. Há variação de poucos pixels nas pétalas e nas lâminas finas.
- [Play Mode](play-mode.json). O controlador do personagem percorreu os 321 pontos da rota conhecida do píer à igreja com as duas variantes. Em ambas terminou no chão, aproximadamente 23 cm da referência na porta da igreja. Nenhum objeto da vegetação adiciona colisão.
- O Unity terminou a compilação sem falhas. O Console registrou uma falha de autenticação na busca de pacotes e erros de indexação do Quick Search. A caminhada não registrou erro do controlador ou da vegetação.

A avaliação compara dois níveis de composição em pontos do eixo. As metas de cobertura da tabela são referências para distribuição por região. Uma medição integral da área plantável fica para a etapa de implantação regional.
