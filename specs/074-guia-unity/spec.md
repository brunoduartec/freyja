# Especificacao: Guia para navegar no projeto Unity

**Feature Branch**: `074-guia-unity`
**Criada**: 2026-11-20
**Status**: Rascunho para avaliacao
**Entrada**: Pedido para explicar onde ficam os elementos do Unity e como encontra-los sem IA.

## Cenarios de usuario e testes

### Historia 1 - Encontrar cenas e assets no Unity (Prioridade: P1)

Como pessoa que esta aprendendo o projeto, quero localizar cenas, personagens, objetos e outros assets no editor Unity para inspecionar o jogo sem depender de uma IA.

**Motivo da prioridade**: Ver e reconhecer o conteudo do projeto e a necessidade principal.

**Teste independente**: Seguir o guia no Unity e abrir a cena principal, localizar um prefab e inspecionar um objeto na Hierarchy.

**Cenarios de aceite**:

1. **Dado** que o projeto esta aberto no Unity, **quando** sigo o caminho indicado para cenas, **entao** encontro e abro a cena principal.
2. **Dado** que uma cena esta aberta, **quando** seleciono um objeto da Hierarchy, **entao** encontro seus componentes e consigo identificar os assets relacionados pelo Inspector.

### Historia 2 - Encontrar e entender os codigos (Prioridade: P2)

Como pessoa que quer aprender o projeto, quero localizar scripts e descobrir qual codigo esta associado a um objeto ou comportamento.

**Motivo da prioridade**: Permite investigar e aprender como o jogo funciona sem pedir que uma IA encontre os arquivos.

**Teste independente**: Usar os caminhos do guia para encontrar um script existente e seguir as instrucoes de busca no editor de codigo.

**Cenarios de aceite**:

1. **Dado** que sei o nome de um comportamento, **quando** uso a busca do Unity ou do editor de codigo conforme o guia, **entao** encontro o script correspondente.
2. **Dado** que selecionei um objeto com um componente de script, **quando** abro o script associado, **entao** consigo navegar entre o codigo e o objeto que o utiliza.

### Historia 3 - Explorar o projeto com seguranca (Prioridade: P3)

Como pessoa iniciante, quero distinguir arquivos editaveis, arquivos gerados e areas importantes para nao alterar o projeto por engano.

**Motivo da prioridade**: A orientacao de seguranca evita danos enquanto a pessoa explora por conta propria.

**Teste independente**: Usar a lista de cuidados do guia para identificar o que pode ser inspecionado e o que nao deve ser apagado ou editado manualmente.

**Cenarios de aceite**:

1. **Dado** que encontro uma pasta tecnica ou arquivo gerado, **quando** consulto o guia, **entao** sei se devo apenas inspecionar ou evitar alteracoes.
2. **Dado** que quero testar o jogo, **quando** sigo as orientacoes de Play Mode, **entao** sei como iniciar e parar o teste e que alteracoes feitas durante o teste nao sao necessariamente permanentes.

### Casos limite

- Se um script nao aparecer no Inspector de um objeto, o guia deve mostrar como busca-lo pelo nome e lembrar que nem todo script e anexado diretamente a um objeto.
- Se a pessoa nao encontrar um asset no caminho escrito, o guia deve sugerir a busca pelo nome no painel Project e explicar a diferenca entre o caminho no computador e o caminho relativo a `Assets/`.
- Se uma cena nao abrir ou houver erros de compilacao, o guia deve indicar onde encontrar o Console e nao sugerir apagar arquivos para resolver o problema.

## Requisitos

### Requisitos funcionais

- **FR-001**: O guia MUST explicar que a pasta do projeto Unity e `Freyja/`, dentro da raiz do repositorio, e orientar como abri-la pelo Unity Hub.
- **FR-002**: O guia MUST mapear as pastas importantes em `Freyja/Assets/_Project/`, incluindo `Scenes`, `Scripts`, `Prefabs`, `Art`, `Data`, `Input`, `Editor` e `WorldBuilder`.
- **FR-003**: O guia MUST identificar a cena principal de trabalho como `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` e distinguir cenas de teste/iteracao quando relevante.
- **FR-004**: O guia MUST dar exemplos reais de scripts e explicar como localiza-los pelo Unity e por um editor de codigo, sem IA.
- **FR-005**: O guia MUST explicar como usar Scene, Game, Hierarchy, Inspector, Project e Console para encontrar e inspecionar objetos, componentes e assets.
- **FR-006**: O guia MUST apresentar um fluxo pratico para seguir um objeto da cena ate seus componentes e arquivos correspondentes, inclusive scripts e prefabs quando aplicavel.
- **FR-007**: O guia MUST distinguir assets dentro de `Assets/` de fontes de producao fora do Unity, como `Source/Blender/`.
- **FR-008**: O guia MUST alertar para nao apagar arquivos `.meta`, nao editar conteudo gerado como `Library/` e ter cautela ao modificar cenas e prefabs.
- **FR-009**: O guia MUST estar escrito em portugues simples, com caminhos concretos e passos que um iniciante possa executar sem assistencia de IA.

## Criterios de sucesso

### Resultados mensuraveis

- **SC-001**: Uma pessoa iniciante consegue localizar e abrir a cena principal seguindo apenas o guia em ate 3 minutos.
- **SC-002**: Uma pessoa iniciante consegue localizar um script de exemplo e um prefab seguindo apenas o guia em ate 3 minutos cada.
- **SC-003**: O guia cobre pelo menos as oito pastas principais listadas em FR-002 e inclui tres exemplos de arquivos existentes no projeto.
- **SC-004**: Todas as instrucoes principais podem ser executadas dentro do Unity, Unity Hub ou editor de codigo instalado, sem servico de IA.

## Premissas

- O publico-alvo esta aprendendo Unity e pode ainda nao conhecer a diferenca entre cena, objeto, componente, prefab e asset.
- O guia sera um documento Markdown em portugues, com caminhos relativos a raiz do repositorio e instrucoes visuais passo a passo no editor.
- A primeira versao usa nomes e caminhos que existem no estado atual do repositorio; nao promete que nomes de arquivos futuros permanecerao iguais.
- O documento cobre orientacao e inspecao. Tutoriais completos de C#, Blender, Git ou modificacao de gameplay estao fora do escopo.
