# Modelo de dados — Grama tripla

- **Raiz-base**: posição/matriz estável no layout costeiro ou no layout de campo. Campos relevantes: posição, índice de malha e célula de região. Preserva a origem da vegetação.
- **Raiz suplementar**: posição nova única validada na superfície da ilha, com rotação, escala e índice de malha. Pertence a uma região do layout e é atribuída a um perfil de vegetação pelo renderer. Sua geração depende de geometria, seed, ruído e ocupação espacial.
- **Campo de density/noise**: amostrador determinístico em coordenadas de mundo com tamanho de manchas, contraste e offset; modula aceitação das raízes suplementares para manter cobertura agrupada e contínua entre limites de região.
- **Máscara de superfície**: conjunto de posições-base que não podem renderizar por sobrepor caminho, acesso ou outra superfície inválida. Recalculada quando terreno e acessos mudam.
- **Exclusão dinâmica**: volume local que oculta raízes na área atingida; desligar/mover o volume restaura apenas raízes elegíveis existentes.
- **Perfil de qualidade**: regra de seleção Baixa/Média/Alta sobre raízes base e suplementares. Alta define a meta de contagem; qualidades menores usam subconjuntos estáveis e menos apresentação.
- **Submissão LOD**: matrizes desenhadas por perfil, região, malha e estágio de LOD, com batches limitados pelo renderer. Métricas do benchmark distinguem matrizes cacheadas, raízes únicas e submissões duplicadas por transição.
