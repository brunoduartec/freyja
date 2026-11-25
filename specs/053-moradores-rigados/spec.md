# Modelos dos 10 moradores com rig

**Criada em** 2026-11-19  
**Branch** `053-moradores-rigados`  
**Estado** Dez modelos gerados, rigados e verificados em 2026-11-19. Relatório em TripoAi/moradores-A1-A2-A3.md na raiz principal.

## Cenários e testes

### Usar os moradores como personagens articulados

A equipe deve encontrar os 10 moradores de `Referencias/Poses-Moradores/` como modelos 3D texturizados e articulados, em pastas individuais junto dos modelos existentes em `TripoAi/`.

**Teste independente** Abrir cada modelo, verificar sua correspondência visual com a referência aprovada e movimentar braços e pernas pelo esqueleto.

1. Cada uma das 10 imagens corresponde a um personagem com o mesmo nome no destino.
2. O rosto, cabelo, figurino, faixa etária e detalhes identificadores seguem a imagem individual, mantendo a diversidade do elenco e a direção low poly.
3. O modelo possui esqueleto humanoide e pesos de deformação. Braços e pernas podem ser movimentados para conferir a vinculação da malha ao rig.
4. Cada modelo final tem no máximo 30.000 triângulos, contando todas as malhas do personagem.

### Casos de borda

- Se a checagem de compatibilidade reprovar uma malha, registrar o personagem e a causa antes de propor uma correção.
- Conferir saias, aventais, cabelo, mãos, a barriga de Helena e a tira azul de Ivo durante a inspeção.
- Se o orçamento disponível impedir o lote completo, registrar o ponto de retomada. Uma nova execução deve reutilizar tarefas concluídas.

## Requisitos

- **FR-001** Gerar os 10 personagens com o Tripo a partir das imagens atuais das subpastas A1, A2 e A3 de `/Users/bruno.duartec/Documents/Projects/Freyja/Referencias/Poses-Moradores/`.
- **FR-002** Entregar cada personagem em `/Users/bruno.duartec/Documents/Projects/Freyja/TripoAi/<nome-sobrenome>/`, com modelo texturizado, rig humanoide, prévia e registro da geração.
- **FR-003** Entregar o modelo texturizado em GLB e a versão com rig em FBX compatível com esqueleto humanoide Mixamo.
- **FR-004** Conferir o limite de 30.000 triângulos no modelo final e registrar a contagem, a existência do esqueleto e a vinculação da malha.
- **FR-005** Conferir visualmente cada personagem e demonstrar a articulação de ao menos braços e pernas, com evidência da inspeção.
- **FR-006** Registrar custos efetivos por tarefa, total consumido e estado de cada personagem para retomada.

## Critérios de sucesso

- **SC-001** As 10 referências têm seus respectivos modelos texturizados e com rig nos destinos individuais.
- **SC-002** Os 10 modelos finais respeitam o teto de 30.000 triângulos.
- **SC-003** Os 10 personagens passam pela inspeção visual e pela conferência de articulação, ou têm sua pendência identificada até a correção.

## Inventário

| Casa | Personagens |
| --- | --- |
| A1 | Duda, Elisa, Joaquim e Mauro Neves |
| A2 | Amélia, Sara e Tomás Faria |
| A3 | Helena, Ivo e Luana Batista |

## Premissas e dependências

- O usuário restringiu a geração aos 10 moradores de A1, A2 e A3. Cada pasta tem respectivamente quatro, três e três personagens.
- O limite solicitado é tratado como 30.000 triângulos por personagem, uma medida verificável também após triangulação.
- O lote autorizado contém dez personagens. A primeira geração será inspecionada para conferir o processo antes das demais.
- O saldo de API verificado em 2026-11-19 é de 630 créditos. O lote estimado custa 550 créditos, dentro do saldo disponível. O consumo real será registrado por tarefa.
- A entrega cobre os arquivos de modelos e a verificação do rig. A integração na cena Unity será definida em outra entrega.
