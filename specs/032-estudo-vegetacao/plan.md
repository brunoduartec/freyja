# Plano de implementação — estudo de vegetação

## Resumo

Implementar a primeira avaliação na cena principal, usando a vegetação costeira existente como base. Acrescentar o coqueiro da praia e comparar duas coberturas no eixo cais–praça–igreja. As demais regiões permanecem para a próxima avaliação.

## Contexto técnico e constituição

Unity 6000.3.25f1, Built-in. Cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. Usar cópia isolada da cena e dependências atuais na branch 032. Editar pelo Unity e exportar pelo Blender. Preservar a largura jogável, vistas e interações. Produção visual sem mudanças nas regras de investigação.

## Fases

1. Preparar os modelos do Nature e reaproveitar a grama da spec 029.
2. Assentar o coqueiro na retaguarda seca da praia e compor canteiros nas bordas do eixo.
3. Salvar a variante escolhida, capturar visualmente as duas coberturas e conferir o percurso em Play Mode.

## Implantação das oito zonas de grama

O pedido aprovou o [mapa de grama](estudo-grama.md). A grama usa os modelos, o shader e o renderer da spec 029. O perfil editável registra os limites e âncoras X/Z aprovados, escolhe densidades dentro das faixas e controla a área de manchas por zona. O bake cria um layout da ilha ao lado do piloto existente. A cena principal seleciona o layout novo pelo componente existente.

A seleção determinística respeita paths, acessos, o corredor frontal da igreja, declive, camada do terreno e obstáculos. No Unity, verificar as oito zonas, a leitura da trilha e o custo de submissão comparado ao envio integral. O trabalho de composição de outras plantas segue o recorte descrito acima.
