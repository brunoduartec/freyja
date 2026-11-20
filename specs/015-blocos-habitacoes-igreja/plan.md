# Plano: blocos das habitações e igreja

**Branch**: `015-blocos-habitacoes-igreja` | **Data**: 2026-11-14 | **Spec**: [spec.md](spec.md)

## Resumo

Na cena principal existente, implantar 12 volumes residenciais e um de igreja, identificados e substituíveis, usando a escala humana e a topografia atuais. Validar o encaixe andando com a câmera do jogo e apresentar vistas e medidas. As posições e dimensões exatas dependem da inspeção do terreno e são propostas para avaliação, não decisões já verificadas.

## Contexto técnico

- Unity 6000.3.25f1, Built-in; cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`.
- Ilha atual ~57,6 × 48,4 m; personagem 1,80 m. O plano do GDD prevê área substancialmente maior.
- Unity MCP para inspeção, montagem, salvamento e Play Mode; Blender MCP se a preparação de geometria própria for necessária. Confirmar projeto e cena ativos antes de modificar. A disponibilidade real dos editores ainda não foi verificada nesta fase.
- Blocos simples individuais, sem código novo previsto. Persistência na cena; metadados preservados.
- Não há meta de FPS ou plataforma nova nesta entrega; validação visual e de navegação, sem pressupor build.

## Verificação constitucional

Antes e depois do desenho: aprovado. A proposta preserva o GDD, assinala a igreja e o encaixe como provisórios, utiliza os editores exigidos e prevê evidências reproduzíveis. Não cria nova regra narrativa nem amplia o escopo.

## Abordagem

1. Confirmar conexão MCP, projeto/cena e estado atual; inspecionar terreno, percursos, colisão, escala e edifícios já existentes. Preservar alterações e assets do usuário.
2. Identificar zonas baixas, altas e leste, usando o GDD como referência relativa; levantar superfícies efetivamente disponíveis. Considerar 5 × 6 m, 7 × 8 m e 6 × 8 m como pontos de partida; alturas e igreja serão propostas com base no personagem e no espaço.
3. Distribuir B1–B4, A1–A4 e L1–L4, depois igreja, com frentes voltadas para acessos viáveis; registrar divergências entre mapa e terreno. Manter cada volume separado e com referência na base e indicador visível da frente. Usar diferenciação visual simples entre setores e igreja, sem criar arte final.
4. Conferir encontro com terreno e colisões, acesso às frentes, escadas e percurso; ajustar apenas os blocos. Evitar obstruir caminhos. Se não houver espaço, manter o problema visível e documentado, sem encolher o personagem ou alterar a ilha.
5. Registrar a tabela de implantação conforme [data-model.md](data-model.md), salvar a cena e produzir vistas gerais e imagens da câmera jogável. Executar o roteiro de [quickstart.md](quickstart.md), relatar conflitos e deixar a implantação para avaliação.

## Estrutura

- `specs/015-blocos-habitacoes-igreja/`: plano, pesquisa, modelo de dados, roteiro e evidências futuras.
- `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`: cena existente a alterar na implementação.
- Assets próprios adicionais, apenas se necessários, em `Freyja/Assets/_Project/`; fontes Blender editáveis fora de `Assets/`.

Não há contrato externo para esta entrega interna. Nenhum novo sistema, interior ou modelo final está previsto.
