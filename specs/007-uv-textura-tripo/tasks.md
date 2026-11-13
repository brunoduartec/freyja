# Tasks: UV e textura da ilha do TripoAI

## Phase 1 — Setup
- [x] T001 Confirmar sessão, registrar hashes e preparar Source/Blender/Ilha_Tripo_007/validation.json.

## Phase 2 — Base
- [x] T002 Inspecionar relevo superior e registrar câmeras e fonte em Source/Blender/Ilha_Tripo_007/Ilha_Tripo_Texturizada.blend.

## Phase 3 — US1: ilha texturizada (P1)
**Objetivo**: mesma ilha, com UVs e acabamento conforme referência.
**Teste independente**: abrir a fonte e conferir vista aérea, chão, costa e encostas.
- [x] T003 [US1] Criar e validar UVs únicos; gravar layout em Source/Blender/Ilha_Tripo_007/previews/UV_Layout.png.
- [x] T004 [US1] Criar bitmaps de materiais e máscara de distribuição em Source/Blender/Ilha_Tripo_007/textures/.
- [x] T005 [US1] Aplicar composição e revisar caminhos, grama, rocha e praia em Source/Blender/Ilha_Tripo_007/Ilha_Tripo_Texturizada.blend.
- [x] T006 [US1] Fazer bake do atlas Base Color e configurar material final em Source/Blender/Ilha_Tripo_007/textures/Island_BaseColor.png.
- [x] T007 [US1] Renderizar e revisar geral/superior/detalhes em Source/Blender/Ilha_Tripo_007/previews/.

## Phase 4 — Entrega
- [x] T008 Salvar, reabrir e validar fonte, geometria, UVs e imagens em Source/Blender/Ilha_Tripo_007/validation.json.
- [x] T009 Documentar artefatos e limitações em Source/Blender/Ilha_Tripo_007/README.md e specs/007-uv-textura-tripo/validation.md.

## Dependências e execução
T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008 → T009.
Uma única história; sem tarefas paralelas no editor compartilhado. A primeira versão é o MVP completo desta spec; parar para avaliação ao entregar.

## Evidências de conclusão

- T001: baseline.json e validation.json registram hashes, contagens e transformação.
- T002: fonte salva e reaberta, câmera Camera_General; Inspection_Top.png.
- T003: Island_UV, uv_check.json e previews/UV_Layout.png.
- T004–T005: seis PNGs em textures/ e material Terrain_Source_Editable preservado.
- T006: Island_BaseColor.png, material Island_Terrain_Atlas, padding.json e texel_check.json.
- T007: cinco prévias finais inspecionadas; comparação com Source_Path_Detail.png.
- T008: validation.json confirma reabertura, seis imagens empacotadas/carregadas e geometria idêntica.
- T009: README.md na pasta de produção e validation.md nesta spec.
