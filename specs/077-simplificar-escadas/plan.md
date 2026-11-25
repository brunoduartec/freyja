# Plano — Simplificar as escadas jogáveis

**Branch** `077-simplificar-escadas` · [Spec](spec.md)

## Contexto técnico

Unity 6000.3.25f1, Built-in e Unity Pipeline. Trabalhar na cena `Ilha_Principal`, com `CharacterController.Move`, RenderTexture de produção e NavMesh derivado de colisores. Base de comparação em [evidence/baseline](evidence/baseline/README.md).

## Sequência

1. Salvar uma cópia da cena local fora dos assets. Na L3, criar uma cunha estática com `MeshCollider` na layer de colisão existente, alinhada em espaço local ao chão e à varanda. Manter as caixas dos degraus desativadas individualmente para reversão, bem como guardas e demais apoios. Remover somente a associação da L3 da lista de lances do IK. A primeira prova conserva controlador, Animator e câmera.
2. Executar a matriz funcional e capturar a RenderTexture pelo harness do projeto. Se aparecer uma falha, reproduzir e medir antes de intervir. Revisar o defeito perceptível na câmera real com os agentes. O resultado funcional e visual condiciona a expansão.
3. Após o aceite, repetir a configuração na B3. Reaproveitar a autoria e a navegação existentes, preservar o patamar e validar cópia rotacionada. Conferir outros candidatos antes de migrar.
4. Verificar regressões e registrar evidências, reversão, exceções de autoria e dados comparáveis de desempenho quando disponíveis.

## Constituição

A cena é editada e testada pelo Unity. A câmera e o material visual reais entram no aceite. A entrega registra evidências por cenário e preserva as decisões das specs anteriores, salvo a simplificação proposta e aprovada. Não há mudança de narrativa, dependências ou contrato externo.
