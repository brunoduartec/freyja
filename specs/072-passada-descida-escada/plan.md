# Plano técnico — passada na descida

1. Usar a implementação atual da 071 como base e gravar o comportamento original na L3. A branch 072 usa a base da entrega 071 e os arquivos já presentes no workspace principal. As alterações locais do painel de debug foram preservadas.
2. Em `StairFootPlacement.cs`, reconhecer movimento no sentido descendente e limitar sua velocidade dentro do lance. Calibrar a cadência pela distância real entre degraus. Preservar o bloqueio lateral e a saída livre nos patamares.
3. Reutilizar a resolução de apoios e a alternância dos pés, percorrendo os degraus na ordem descendente e orientando a postura para baixo. Manter a âncora do pé de apoio fixa até a transferência de peso e suavizar a entrada e a saída da pose. A pausa congela a fase válida. A inversão para subida reconstrói o passo a partir da posição atual.
4. Em `PlayerWalkAnimation.cs`, sincronizar a pose base de descida com o progresso físico no lance. Experimentar a passada existente amostrada no sentido inverso como ponto de partida e aprovar pelo resultado combinado com o IK na câmera do jogo.
5. Verificar compilação e Console e executar o [roteiro em Play Mode](quickstart.md) via MCP Unity 6000.3.25f1 na cena Ilha_Tripo_Caminhada_011. Registrar a comparação visual e os resultados por lance.

A mudança mantém as colisões e os modelos existentes. O teste visual é obrigatório para avaliar contato de pé e qualidade da transição.
