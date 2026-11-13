# Implementation Plan: UV e textura da ilha do TripoAI

**Branch**: sem Git neste diretório | **Date**: 2026-11-13 | **Spec**: [spec.md](spec.md)

## Summary

Preservar a malha importada, criar UVs únicos e produzir textura de cor com materiais de grama, pedra, terra e areia. Usar a referência aprovada e máscaras de localização para acompanhar os caminhos do relevo. Entregar a primeira versão no Blender, sem integrar no Unity.

## Technical Context

- Blender 5.2.2 LTS, via MCP global; Python/bpy para operações em lote.
- Entrada: TripoAi/island.glb, 290.322 vértices / 580.632 triângulos, sem UV.
- UV: Smart UV Project, inspecionar e validar áreas, limites e cobertura. Não alterar vértices ou faces.
- Texturas-fonte: PNGs de 128 ou 256 pixels por material, amostragem Closest. Máscara de distribuição para caminhos e praia.
- Textura final: atlas Base Color 2048×2048, bake sem luz; resolução acima de 256 justificada por reunir a ilha inteira e suas ilhas UV. Sem normal map ou luz pintada permanente.
- Materiais foscos; render de apresentação com iluminação suave e câmeras geral e de detalhe.
- Validação: hash da geometria e original, UVs válidos sem áreas degeneradas, inspeção de sobreposição, reabertura do blend e arquivos de imagem.
- Sem novas dependências no Unity, APIs externas, alteração de escala ou otimização de malha.

## Constitution Check

Aprovado antes e depois do desenho: fonte editável preservada; MCP Blender usado; sem mudança narrativa; bitmaps com pixels legíveis; caminhos de evidência registrados. Não há validação na câmera Unity porque integração está fora desta entrega. A malha densa é mantida por requisito, sem alegar que já atende ao orçamento low poly do jogo.

## Project Structure

### Documentation (this feature)

spec.md, plan.md, research.md, data-model.md, quickstart.md, tasks.md e validation.md.

### Source Code (repository root)

Source/Blender/Ilha_Tripo_007/
- Ilha_Tripo_Texturizada.blend
- textures/ (fontes, máscara e atlas)
- previews/ (geral, superior, detalhes e layout UV)
- scripts/ (produção e validação)
- validation.json e README.md

## Execution

1. Registrar hashes e preparar cópia de trabalho.
2. Inspecionar vista superior; delimitar superfícies e caminhos.
3. Criar UVs, PNGs e material-fonte; prévia e correção visual.
4. Gravar a cor em atlas UV e conectar o material final.
5. Salvar, reabrir, validar e apresentar a primeira versão.

## Complexity Tracking

Sem violações. Smart Project escolhido para a malha triangulada existente; retopologia e integração no jogo excluídas.
