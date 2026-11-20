# Plano: direção de renderização pixelada

**Branch**: `014-renderer-pixelado` · **Spec**: [spec.md](spec.md) · **Data**: 2026-11-14

## Resumo e contexto

Investigar referências e comparar três combinações de resolução e texturas; capturar a cena real com uma proposta e documentar como reproduzi-la. O projeto usa Unity 6000.3.25f1, Built-in e cena `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. A plataforma final e metas de desempenho estão em aberto; não serão presumidas. A recomendação inicial é testar renderização em resolução reduzida com ampliação sem interpolação, mantendo interface legível em resolução de saída. A resolução exata depende da avaliação visual.

## Constituição

O GDD permanece fonte de decisões; a proposta exige avaliação. Usar Unity MCP, confirmar projeto e cena; validar silhuetas, personagens e pistas; documentar evidências e limitações. Preservar assets e alterações existentes. Nenhuma migração de pipeline. Verificação após o desenho: as mesmas regras se aplicam.

## Execução

1. Registrar pesquisa e fontes em `research.md`; separar observação de afirmações técnicas comprovadas.
2. Inspecionar cena e câmera no Unity; comparar resoluções candidatas com o mesmo enquadramento e capturar uma imagem escolhida em `specs/014-renderer-pixelado/`.
3. Avaliar legibilidade, movimento e saídas em duas resoluções ou proporções, ou documentar limitações reais.
4. Documentar configurações reproduzíveis, filtros, escala, interface e critérios de assets em `quickstart.md`; manter configurações como proposta até aprovação.
5. Registrar evidência e tarefas realizadas em `tasks.md`. Contratos externos não se aplicam; modelo simples dos artefatos em `data-model.md`.
