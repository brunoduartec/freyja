# Freyja

Jogo 3D de investigação narrativa em uma vila insular. O projeto é desenvolvido em iterações curtas, com decisões em [GDD.md](GDD.md) e regras de trabalho em [AGENTS.md](AGENTS.md).

## Projeto e fontes

- Projeto Unity: `Freyja/` — editor **6000.3.25f1**, Built-in.
- Fontes Blender: `Source/Blender/`; arquivo inicial: `Source/Blender/Freyja.blend`.
- Referências: `Referencias/`; modelos de origem: `TripoAi/`.
- Integrações e validação: `Tools/MCP/README.md`.
- Especificações e acompanhamento: [specs/README.md](specs/README.md).

## MVP de pistas

A [spec 009](specs/009-sistema-misterio/spec.md) tem [plano](specs/009-sistema-misterio/plan.md) e [30 tarefas](specs/009-sistema-misterio/tasks.md) para um protótipo web com mapa existente, rostos clicáveis, diálogos e painel permanente de pistas. A implementação ainda não foi iniciada.

A ferramenta validará uma pequena investigação e caminhos após perda de testemunhas antes da produção da experiência em Unity. O dossiê e o mapa de pistas serão exportados do mesmo conteúdo da ferramenta.

## Versionamento

Este repositório preserva `Assets` com seus `.meta`, `Packages`, `ProjectSettings`, fontes e documentação. Caches Unity, builds, dados locais de IDE e credenciais são ignorados. Evidências de validações antigas mantêm caminhos e nomes históricos; a migração de nome está descrita em [docs/project-rename.md](docs/project-rename.md).
