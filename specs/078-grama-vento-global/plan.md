# Plano — Grama e vento global

Unity 6000.3.25f1, Built-in Forward, Metal, câmera de produção e terrain compacto. [Spec](spec.md) · [Pesquisa](research.md)

1. Validar um piloto isolado com a grama local, luz principal, luz pontual, ambiente, sombra e vento. Usar o material existente como comparação.
2. Preservar as matrizes e malhas editáveis da ilha. Adicionar perfis, qualidade, seleção determinística, LOD por distância e enquadramento, e exclusões regionais. Manter leitura dos layouts antigos.
3. Integrar em `Ilha_Principal` após o piloto. Revisar câmera, percurso, exclusões e ciclo ambiental com as ferramentas do projeto.
4. Medir com a mesma pose e relógio por condição. Capturar o dia, a noite, o vento e os zooms através da saída do jogo. Validar regressões e corrigir os problemas encontrados.

A implementação segue a Constituição e mantém dependências, motor, pipeline, dados de terreno e sistemas de gameplay.
