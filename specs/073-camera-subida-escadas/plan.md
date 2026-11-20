# Plano técnico — enquadramento na subida

1. Trabalhar sobre a cena jogável usada no diagnóstico, com as entregas 071 e 072 preservadas. Na L3, reproduzir a subida pelo centro e pelas laterais e registrar o enquadramento, o menor afastamento e qual sonda reduziu a distância.
2. Em `CaisCameraRig.cs`, manter a órbita atual enquanto ela comportar uma vista utilizável. Quando a distância segura ficar curta demais, tentar primeiro uma vista centralizada, sem o deslocamento de ombro. Se necessário, testar uma posição curta e elevada que mantenha o tronco visível.
3. Validar cada posição alternativa com o volume da câmera e uma linha de visão até o tronco. Orientar a câmera para o alvo quando usar uma posição alternativa. Preservar o ângulo solicitado pelo jogador e voltar à órbita normal com suavização após liberar o obstáculo. Usar a última posição segura apenas enquanto ela manter o personagem no quadro e passar nas verificações de colisão e visibilidade.
4. Validar a compilação, o Console e o [roteiro em Play Mode](quickstart.md). Comparar o antes e depois no mesmo percurso e verificar a B3 e o patamar.

## Ambiente e constituição

Unity 6000.3.25f1, Built-in, cena `Ilha_Tripo_Caminhada_011`, validação via MCP. O ajuste pertence ao rig de câmera. A movimentação, a animação e a geometria permanecem conforme as entregas anteriores. A branch 073 foi criada da main local em worktree separado. A implementação precisa usar a cena atual do workspace principal para reproduzir o cenário observado.

O diagnóstico isolou a sonda do tronco contra os colliders do corrimão na L3. Verificar primeiro se centralizar a vista resolve os três percursos. Preservar a direção usada pelo controle de movimento quando a rotação visual variar. Usar a mesma resolução no reposicionamento inicial e durante a exploração.
