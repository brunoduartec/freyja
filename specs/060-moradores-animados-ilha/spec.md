# Moradores animados nos seus lugares na ilha

**Criada em** 2026-11-19  
**Branch** `060-moradores-animados-ilha`  
**Estado** Implementada e verificada em 2026-11-19. [Relatório da entrega](report.md).

## Cenários e testes

### Encontrar os dez moradores na vila

Ao explorar a ilha, o jogador deve encontrar os dez personagens de A1, A2 e A3 em locais relacionados às suas funções no GDD, com animação visível de repouso e gestos leves.

**Teste independente** Percorrer a vila em Play Mode e observar os dez moradores pela câmera real de jogo.

1. Cada personagem aparece uma única vez, com figurino e rosto reconhecíveis e animação em reprodução.
2. Os pés se apoiam no piso, o tamanho é coerente com a idade e o corpo apresenta deformação adequada nas poses utilizadas.
3. O jogador consegue passar pelos moradores, portas e escadas usados no percurso.

### Casos de borda

- Distribuir os três moradores ligados ao salão em pontos diferentes, mantendo o acesso e a presença de Teresa legíveis.
- Posicionar Ivo ao lado do trecho de escada escolhido, com espaço de circulação.
- Conferir o vestido e a gravidez de Helena, as saias das idosas e a adaptação das animações à estatura de Tomás e Duda.

## Requisitos

- **FR-001** Integrar os dez modelos aprovados de A1–A3, disponíveis em `TripoAi/<personagem>/` na raiz principal, na cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`.
- **FR-002** Usar o posicionamento inicial proposto na tabela abaixo. Ajustar os pontos exatos pelo piso, pela visibilidade e pela circulação na cena.
- **FR-003** Aplicar animações de repouso e gestos leves da biblioteca existente, com os pés apoiados e orientação adequada ao local. Variar os clipes e seu início para evitar que o grupo se mova sincronizado.
- **FR-004** Manter a leitura dos rostos, roupas e detalhes dos modelos aprovados e o limite de 30.000 triângulos por personagem.
- **FR-005** Conferir os dez personagens em Play Mode e registrar capturas, posições finais e resultado da circulação.

## Posicionamento proposto

As atividades abaixo seguem o GDD. Os pontos e a reunião de personagens são propostas desta entrega para o momento inicial.

| Morador | Local inicial | Relação com a atividade |
| --- | --- | --- |
| Joaquim Neves | Área externa de A1, na encosta destinada às hortas | Cuida das mudas |
| Mauro Neves | Próximo a Joaquim, com espaço entre ambos | Mantém as hortas |
| Duda Neves | Próxima à área de separação da colheita junto à A1 | Ajuda a separar verduras |
| Elisa Neves | Ao lado da venda, fora da passagem de Bento | Abastece a venda com conservas |
| Amélia Faria | Lateral da entrada do salão comunitário | Cuida dos arquivos e das chaves |
| Sara Faria | Área externa do salão, voltada ao espaço de encontro | Dá aulas |
| Luana Batista | Junto a Sara, em posição distinta | Auxilia nas aulas e nos materiais |
| Tomás Faria | Área livre do largo, próxima ao salão | Brinca no largo |
| Ivo Batista | Junto à escada de acesso à A3 | Conserta os degraus e caminhos |
| Helena Batista | Área externa da casa A3 | Costura e prepara a chegada do bebê |

## Critérios de sucesso

- **SC-001** Dez instâncias dos dez moradores propostos estão presentes na cena, com suas animações em reprodução.
- **SC-002** As dez posições têm apoio no piso e escala coerente, verificadas pela câmera de jogo.
- **SC-003** O percurso do cais até o largo, o salão e as áreas de A1 e A3 continua transitável, com as entradas acessíveis.
- **SC-004** As evidências registram a posição e a animação de cada personagem, incluindo os detalhes de Ivo e Helena.

## Premissas e dependências

- O pedido se refere aos dez modelos recém-produzidos de A1–A3.
- A situação inicial é diurna. As posições concretizam uma distribuição visual do elenco, sem definir horários novos no GDD.
- A biblioteca humanoide já instalada fornece as animações desta etapa. A integração segue o fluxo dos personagens da entrega 045.
- A inspeção de 2026-11-19 confirmou o projeto `Freyja/Freyja`, Unity 6000.3.25f1, a cena principal salva e os seis personagens anteriores presentes.
- A área junto à A1 é a proposta para a atividade das hortas nesta distribuição. A aprovação desta spec confirma os pontos propostos para o elenco.
