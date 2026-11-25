# Implementação 021

Spec aprovada em 15/11/2026. Referências: ambos os documentos do pacote enviado, preservados em specs/021-world-builder/references.

Ambiente auditado: Unity 6000.3.25f1, Built-in, Test Framework 1.6.0 e NUnit 2.0.5 existentes. Sem mudança de versão, pipeline ou dependências. O bridge Tripo já existia no manifest; esta entrega não o usa nem adiciona integração externa.

Adaptação: a spec aprovada pede conjunto isolado na cena principal, portanto a demo usa a cena Ilha_Tripo_Caminhada_011, com ocultação reversível dos ambientes anteriores e backup antes da integração. A proposta de cena nova do pacote foi substituída por essa regra do projeto. A escala da demo não é decisão para a ilha inteira.

Fontes na worktree 021; instalação do plugin na pasta própria do Unity aberto. Nenhuma mudança de outra entrega será incluída em commit. Evidências em Artifacts/WorldBuilder.

External 3D generation jobs: 0
