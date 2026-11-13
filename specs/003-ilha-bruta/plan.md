# Plano — 003 · Ilha bruta caminhável

**Data:** 12/11/2026 · **Spec:** [spec.md](spec.md)  
**Identificador:** 003-ilha-bruta; a pasta não possui repositório Git, portanto nenhuma branch foi criada.  
**Estado:** planejamento solicitado; implementação não iniciada.

## Resumo

Construir no Blender uma maquete editável da ilha inteira, com relevo, costa, caminhos e 12 blocos de casas. Integrar uma cópia na Unity e reutilizar o jogador para percorrer o mapa. A entrega termina com a ilha aberta para avaliação: sem casas detalhadas, moradores ou sistemas narrativos.

## Contexto técnico

| Item | Decisão |
| --- | --- |
| Ferramentas | Blender 5.2.2 LTS e Unity 6000.3.25f1 Built-in, ambos via MCP |
| Linguagem | C# existente somente se necessário para integração; sem novas dependências |
| Pacotes | Pipeline 1.0.0, Input System 1.20.0 e uGUI 2.0.0 existentes |
| Escala | 1 unidade Unity = 1 metro; referências aproximadas: 260 × 180 m e 24 m de altura |
| Validação | Editor macOS, teclado/mouse e Play Mode; teste manual do circuito e recuperação |
| Desempenho | Verificar fluidez no editor; medir e registrar condições se houver problema |
| Persistência | Sem save/load; preservar arquivos e cenas anteriores |

## Constituição: antes e depois do desenho

| Princípio | Verificação |
| --- | --- |
| GDD como fonte | Atendido: topologia, bairros e medidas são referências revisáveis; sem novos fatos narrativos |
| Unity e Blender via MCP | Atendido: modelar, integrar e validar nos editores |
| Low poly e legibilidade | Atendido: relevo facetado legível; blocos provisórios, sem antecipar acabamento |
| Investigação e cânone | Atendido: nenhum diálogo ou pista nesta entrega |
| Evidências recuperáveis | Atendido: registrar fonte, export, cena, capturas e percurso |

**Resultado:** gates aprovados antes da pesquisa e reavaliados após o desenho; sem exceções ou dúvidas técnicas bloqueantes.

## Estrutura planejada

Os caminhos novos abaixo são destinos, ainda não são artefatos implementados.

~~~text
Source/Blender/Ilha_Bruta_003.blend               # fonte nova
Freyja/Assets/_Project/
├── Art/Environment/IlhaBruta/                   # FBX e materiais provisórios
├── Prefabs/IlhaBruta.prefab                     # cenário e volumes
└── Scenes/Ilha_Bruta_003.unity                  # cena jogável nova
specs/003-ilha-bruta/
├── spec.md, plan.md, research.md, data-model.md, quickstart.md
├── contracts/                                   # navegação e integração
├── checklists/requirements.md
└── evidence/                                    # registros futuros
~~~

A fonte Cais_Iteracao01.blend, a cena Cais_Caminhada.unity, seus prefabs e controles permanecem preservados. Abrir e inspecionar a cena ativa antes de mudar o editor; não sobrescrever alterações não salvas.

## Desenho

1. **Maquete editável:** criar fonte própria no Blender via MCP. Definir e identificar coleção de relevo/costa, rotas, 12 blocos de casas e marcos. O terreno é uma ilha, não apenas um recorte retangular; usar faces simples e escala humana.
2. **Mapa:** ancorar o cais existente na enseada sul. O caminho principal liga cais e largo; a rota pelos fundos liga hortas, ruína e casas altas; a crista norte leva ao mirante. Casas baixas, altas e do leste têm quatro blocos cada. Venda e sala comunitária no largo e galpão no cais podem ser volumes separados, identificados como serviços e não contados entre as casas.
3. **Caminhabilidade:** planejar uma rota contínua entre cais, largo, três áreas e mirante, com retorno pela rota dos fundos. Começar com caminho principal de 2,5–3 m e subidas graduais; usar patamares onde necessário. Medir inclinação, largura e colisão com o personagem, ajustando o blockout sem transformar costões em áreas obrigatoriamente escaláveis.
4. **Integração:** exportar FBX para a pasta própria; validar escala, eixos, pivôs, normais e materiais. Criar cena nova a partir da caminhada aprovada. Reutilizar cais, personagem e controles; desativar apenas na nova cena os volumes antigos de terreno/mar que se sobrepuserem à ilha. Não aplicar essas mudanças aos prefabs compartilhados.
5. **Colisões e limites:** gerar colisão para o relevo e os blocos, com máscaras compatíveis com CaisSolid/CaisPlayer. Não colocar collider sobre o mar. Garantir junção terreno–cais, spawn livre e piso contínuo. Ajustar na nova cena os limites de recuperação da sessão para conter toda a ilha e a altura do mirante; os limites antigos eram dimensionados para o cais.
6. **Câmera e água:** ampliar o mar visual sem collider e conferir a distância de visão da câmera na nova cena, sem alterar a câmera compartilhada da 002. Verificar visão e colisão em encostas, blocos e costa. Uma queda no mar retorna ao spawn.

Medidas de partida são hipóteses de teste, não geometria aprovada nem justificativa para mudar os controles da 002. Os arquivos compartilhados mantêm GUIDs e referências.

## Ordem de implementação

| Etapa | Resultado verificável |
| --- | --- |
| 1. Base e preservação | Conferir projeto/cena ativa e registrar referências ou hashes das cenas e prefabs existentes |
| 2. Ilha e circulação | Fonte Blender com costa, elevação, cais conectado, caminhos, hortas, ruína e mirante |
| 3. Implantação | 12 blocos B1–B4, A1–A4, L1–L4, em três grupos distinguíveis; serviços, se usados, separados |
| 4. Importação e cena | FBX e prefab próprios; cena nova com jogador existente, colliders, mar e bounds ajustados |
| 5. Percurso e entrega | Circuito completo, água/queda, câmera e controles verificados; capturas de conjunto e evidência |

## Cobertura

| Requisito | Verificação |
| --- | --- |
| FR-001/002 | Abrir fonte e cena; medir extensão e altura, anotando ajustes em relação às referências |
| FR-003 | Identificar cais, largo, rota dos fundos, hortas, ruína e mirante no modelo e no percurso |
| FR-004 | Contar 4 + 4 + 4 blocos; distinguir serviços e não contar como casas |
| FR-005 | Caminhar pelas rotas e retornar ao cais sem atravessar terreno ou ficar preso |
| FR-006 | Testar WASD/mouse/Esc, queda e recuperação; conferir preservação da cena anterior |
| FR-007 | Reabrir o arquivo Blender e a cena Unity entregues |

No percurso registrar distância e tempo reais: o GDD sugere 60–80 m do cais ao largo, cerca de um minuto até lá e 3–4 minutos pelos pontos principais. Essas são metas exploratórias, não resultados nem critérios rígidos desta spec. Se distância, velocidade e tempo entrarem em conflito, registrar para avaliação em vez de alterar a velocidade da caminhada sem justificativa.

## Validação e entrega

Testar o circuito em Play Mode, o encontro dos colliders nas bordas e rampas, o litoral e o mirante. Sair da área caminhável e confirmar recuperação; verificar também que não há recuperação indevida perto das extremidades da ilha. Inspecionar câmera e distinguir os três bairros. Registrar trajetos, tempos, dimensões reais, falhas e capturas em evidence/; verificar reabertura das duas fontes.

Automação é opcional; Play Mode e percurso real são obrigatórios. Não há resultado de execução neste planejamento. As [tarefas](tasks.md) foram geradas por solicitação do usuário; modelagem e implementação não iniciadas.

[Pesquisa](research.md) · [Modelo](data-model.md) · [Navegação](contracts/navegacao.md) · [Integração](contracts/integracao.md) · [Guia de validação](quickstart.md)
