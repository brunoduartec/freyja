# Diagnóstico e decisões

- **Observação.** A distância da câmera ao pivô caiu a 0 m em três subidas na L3. Uma reprodução instrumentada pelo centro isolou a sonda na altura do tronco. Ela atingiu os colliders do corrimão chamados `Guarda 057 01` a `Guarda 057 14` e reduziu a distância prevista. A sonda do pivô permaneceu livre nesse percurso.
- **Decisão.** Tentar a vista centralizada quando o deslocamento de ombro coloca a sonda na linha do corrimão. Uma alternativa curta e elevada fica condicionada à necessidade observada nos testes.
- **Alternativas avaliadas.** Impor distância mínima à força pode atravessar o corrimão ou a fachada. Remover o corrimão da máscara de colisão pode deixar a câmera atravessá-lo em outros ângulos. A vista centralizada mantém as consultas de segurança.
- **Controle.** A caminhada usa a orientação do rig. Preservar o eixo solicitado pelo jogador quando a vista alternativa precisar apontar para o tronco. Aplicar a mesma regra na atualização e no reposicionamento inicial da câmera, revalidando a posição segura após reinicializações.
