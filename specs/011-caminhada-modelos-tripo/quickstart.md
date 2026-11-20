# Roteiro de validação — Spec 011

Executar após implementar a cena. Na raiz do repositório, abrir o projeto `Freyja/` no Unity 6000.3.25f1 e verificar a conexão ao editor conforme o [guia MCP](../../Tools/MCP/README.md). Aguardar a importação e conferir o Console. Abrir `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` e entrar em Play Mode com teclado e mouse.

| Teste | Procedimento e resultado esperado |
| --- | --- |
| SC-001 · Percurso | Confirmar o ponto inicial em solo firme e o personagem de corpo inteiro. Seguir os controles exibidos e fazer duas idas e voltas consecutivas até o marco, passando pela curva. Sem atravessar o solo ou ficar preso. |
| SC-002 · Animação e imagem | Fazer três partidas e três paradas e observar a transição entre caminhar e repousar. Conferir mochila, pernas, pixels nas silhuetas e contato dos pés no trecho de avaliação. Segurar o comando contra uma barreira e conferir que os passos param quando o avanço cessa. |
| SC-003 · Câmera e queda | Orbitar perto de uma encosta e conferir que a câmera respeita o relevo e recupera a visão ao sair. Cair pela saída prevista e confirmar o retorno em solo firme, em repouso e com enquadramento inicial. |
| FR-003 · Comandos | Em solo plano, comparar avanço axial e diagonal com o mesmo tempo de entrada. Testar Esc e perda de foco com uma tecla pressionada. Confirmar a retomada por clique. |
| SC-004 · Registro | Gravar uma ida e volta na Game View com uma parada e uma mudança de direção. Registrar cena, controles, local do marco, como reproduzir a queda, versão do editor e resultado por critério em `evidence/`. |

Após sair de Play Mode, registrar erros relevantes do Console. Caso os componentes compartilhados do cais sejam alterados, abrir `Cais_Caminhada.unity` e repetir captura, caminhada, câmera e recuperação para conferir o comportamento existente.
