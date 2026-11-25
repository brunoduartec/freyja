# Resultado da consolidação

A base foi o projeto aberto no Unity da pasta principal com a cena 011 na revisão 049. As branches e worktrees examinados partiam de 878aff6. As contribuições de produção estavam nos diretórios de trabalho. A integração compara arquivos e GUIDs e incorpora componentes à cena pelas APIs do editor.

| Estudos | Resultado na consolidação |
| --- | --- |
| 001–010 | Cenas e especificações anteriores preservadas. Incluídas as revisões de investigação, a ferramenta narrativa e o material de moradores do diretório principal. |
| 011–019 | Caminhada, galpão, implantação da ilha e referências preservados. Incluídas as fontes exclusivas do galpão. A cena 011 atual continua a entrada de revisão no editor. |
| 020–023 | Preservados terreno modular, World Builder, kit PS1, ferramentas, demonstrações e fontes editáveis. A cena 011 conserva o terreno e os edifícios da versão jogável posterior. |
| 024 | Worktree do baú ainda no commit-base, sem especificação ou entrega local identificada. |
| 025–028 | Edifícios e praça presentes na ilha atual. Modelo e fonte do lagarto rosa incorporados ao acervo de produção. O estudo 028 contém proposta de peças; não há módulos prontos próprios dessa pasta. |
| 029–030 | Mantidos o sistema e a fonte da grama, a implantação compacta e as rotas da ilha. |
| 031 e 049 | A cena usa a ondulação leve 049 com sua amplitude de 0,065 m. A integração recupera a transição de cor e a espuma do estudo 031 com máscara recalculada sobre o terreno atual. A água acompanha a iluminação dia/noite. As variantes A/B originais estão preservadas como fontes de estudo. |
| 032 | Incorporados o coqueiro, a cobertura base e o complemento opcional desligado como na cena do estudo. Dezoito instâncias tiveram a altura corrigida pelo relevo atual. Preservado o layout de grama do projeto principal, já adaptado à ilha posterior. |
| 033, 035 e 038 | Incorporados o céu e o ciclo de luz atualizados e as 16 fontes da revisão 038, incluindo doze casas. A câmera atual passou a exibir o céu do estudo. |
| 034, 036 e 037 | Mantidos o relevo vigente, as camadas de terreno e o tratamento da encosta. |
| 039–045 | Preservados câmera de exploração, pavimento, meios-fios, seis personagens e rotinas da cena 049. |
| 046–048 | Incorporadas as referências e os dois GLBs finais com escala verificada na entrega 047. A atracação 048 é uma especificação de trabalho futuro. |

## Verificações

No editor Unity 6000.3.25f1, os 22 percursos foram concluídos nos dois sentidos com zero recuperação e zero amostra de oclusão do tronco. Às 12h estavam acesas 0 de 16 fontes da vila e à meia-noite, 16 de 16. A inspeção encontrou seis animadores ativos, zero scripts ou prefabs ausentes, referências de fontes e vidros válidas e todos os GUIDs externos diretamente citados pela cena resolvidos. O shader do mar passou na compilação. O Console reportou zero erros na verificação final. [Percursos](evidence/percurso.txt) · [Integridade](evidence/integridade.txt) · [Comparativo](evidence/comparacao.jpg)

A comparação SHA-256 entre assets dos worktrees e a base consolidada está em [conteúdo Unity](evidence/conteudo-unity.tsv). Não foram detectados GUIDs duplicados na árvore de Assets nem divergências de GUID entre arquivos históricos e consolidados de mesmo caminho. Os códigos que variaram por revisão estão em [variantes de código](evidence/variantes-de-codigo.tsv).

O Tripo Bridge continua na versão 1.0.14, com a mesma cópia local agora incluída em Tools/MCP/Packages e resolvida pelo Unity com caminho relativo. A fonte Blender 018 foi recomprimida sem perda; o SHA-256 do conteúdo descomprimido foi conferido antes de sua inclusão. Os worktrees originais foram preservados.
