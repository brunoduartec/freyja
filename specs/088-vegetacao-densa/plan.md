# Implementation Plan: Vegetação regional da ilha

**Branch**: `088-vegetacao-densa` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

## Summary

Compor vegetação decorativa na `Ilha_Principal` usando modelos e materiais já presentes. Fazer primeiro o trecho praia seca/entrada da vila; comparar na câmera e em Play Mode de dia e à noite, corrigir qualquer obstrução ou incoerência, depois compor os outros setores existentes. Manter a grama 080, vento, iluminação e ciclo atuais.

## Contexto técnico

- **Engine/plataforma**: Unity 6000.3.25f1, Built-in, cena principal do projeto.
- **Assets relevantes**: `ThirdParty/Nature032` (CoastalShrub, CoastalFlowers e materiais), `Art/KitPS1_022` (Tree_Cypress, Tree_Pine) e `Art/Grass029` (CoastalGrass e tufos autorais 078). Itens adicionais entram somente após inspeção de compatibilidade.
- **Persistência**: hierarquia da cena principal e prefabs/meta existentes; não há sistema procedural novo necessário.
- **Implementação escolhida**: instâncias manuais agrupadas por região, com variações pequenas de rotação/escala e transforms registrados na cena. Não criar ferramenta/sistema de distribuição novo. Reutilizar materiais compartilhados.
- **Validação**: Unity Editor/Play Mode; câmera real do jogador em resoluções do projeto, pontos do piloto e dos setores expandidos, tomadas diurna/noturna; percurso do piloto; Console e comparação observável de contagem/renderização.
- **Performance**: evitar colliders e materiais individuais, limitar densidade por leitura e medir a cena sob câmera representativa no mesmo Editor/configuração.
- **Restrições**: sem alterações de dependência, terreno, câmera, shader de água, ciclo, mecânica, ou assets/modelos externos.

## Portão constitucional

- Reutiliza infraestrutura e ativos do projeto: aprovado.
- Preserva identidade low-poly, personagens e pistas: validado visualmente antes da expansão.
- Evidência de cena, verificação de Console e limitações registradas: requerido.
- Complexidade/dependências novas: nenhuma.

## Organização de arquivos

- Cena jogável: `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`
- Instâncias/organização: filhos regionais na própria cena, seguindo hierarquia existente.
- Evidências e documentação: `specs/088-vegetacao-densa/evidence/` e `validacao.md`.

## Fases

1. Inventário visual dos assets e baseline de câmera da área piloto.
2. Posicionamento do piloto praia-vila e primeira revisão visual dia/noite.
3. Correções do piloto e verificação de fluxo/performance; expansão regional somente após piloto aprovado nos critérios da spec.
4. Revisão dos setores, evidências, Console, integração na main e relato de limitações.
