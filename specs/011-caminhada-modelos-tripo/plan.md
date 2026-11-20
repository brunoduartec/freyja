# Plano de implementação — Caminhar no cenário do TripoAI

**Branch** `011-caminhada-modelos-tripo` · **Data** 13/11/2026 · [Spec 011](spec.md)

## Resumo

Montar uma cena de avaliação no Unity em que o protagonista do TripoAI caminhe por um trecho curto da ilha texturizada. Reaproveitar movimento, câmera e recuperação do cais. Produzir no Blender as animações de repouso e caminhada e usar a câmera do jogo para calibrar passada, escala e legibilidade.

## Contexto técnico

| Item | Decisão |
| --- | --- |
| Ambiente | Unity 6000.3.25f1, C# e Built-in, no projeto `Freyja/`. Blender para modelos e animações. Verificar os MCPs no início da implementação. |
| Dependências | Input System e componentes do cais já instalados. Importar FBX e texturas pelos recursos existentes do Unity. Conferir a disponibilidade da dependência local do Tripo Bridge para abrir o projeto. |
| Dados | Cena, fontes Blender, exports e configurações serializadas. Estado de controle e animação dura a sessão de Play Mode. |
| Avaliação | Unity Editor no macOS com teclado e mouse. Registrar compilação, Console, Play Mode e gravação. |
| Escopo | Um protagonista, uma cena e um percurso curto. Medir custo da colisão no trecho e manter a caminhada e a câmera responsivas na captura de avaliação. |

## Constituição

Conferida antes da pesquisa e após o desenho. A cena segue o GDD para terceira pessoa, jaqueta azul, mochila e silhuetas pixeladas. Os modelos ficam editáveis no Blender, e a integração e a validação passam pelo Unity. O roteiro define evidências para cada critério da spec. A entrega se limita à locomoção.

## Execução

1. **Preparar os modelos · FR-001, FR-004.** Confirmar o Unity em `Freyja/` e o arquivo ativo no Blender. Importar os GLBs originais pelo Blender MCP. Conferir rig, pesos, texturas, escala e eixo de frente. Manter a origem do protagonista nos pés, criar os clipes `Idle` e `Walk` no rig e salvar fontes editáveis. Exportar FBX e texturas e comparar os dois modelos no Unity com a referência.
2. **Montar o trecho · FR-001, FR-002, FR-005.** Criar `Ilha_Tripo_Caminhada_011.unity`. Selecionar um início em solo firme, um marco visível e uma mudança de direção. Ajustar a escala uniforme do terreno, preservar o relevo visível e criar colisões estáticas simplificadas para piso e obstáculos no entorno do trecho. Configurar a recuperação com limites da nova cena e verificar uma saída testável que leve à queda.
3. **Ligar controles e animação · FR-003, FR-004, FR-005.** Reusar `CaisPlayerController`, `CaisCameraRig`, `CaisSessionController` e as ações do Input System. Configurar a cápsula, o ponto de início, as máscaras de colisão, o enquadramento e o texto da nova cena. Manter os clipes no lugar, com o deslocamento a cargo do controlador. Acionar a animação pela velocidade planar efetiva após o movimento, ignorando a queda e o salto de posição da recuperação. Ajustar velocidade e cadência pela passada.
4. **Ajustar imagem e validar · FR-006, SC-001 a SC-004.** Renderizar o mundo em 480 × 270 com filtro pontual e sem antialiasing nessa cena. Manter proporção 16:9 na apresentação e o texto dos controles na resolução da tela. Executar o [roteiro](quickstart.md) e gravar a demonstração.

## Estrutura

| Caminho | Conteúdo |
| --- | --- |
| `Source/Blender/Ilha_Tripo_011/` | Fontes editáveis do cenário e do protagonista animado |
| `Freyja/Assets/_Project/Art/Ilha_Tripo_011/` | FBX, texturas, materiais, animação e ajustes de imagem |
| `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` | Cena de avaliação |
| `Freyja/Assets/_Project/Scripts/Prototype/` | Componentes existentes e integração da animação, se necessária |
| `specs/011-caminhada-modelos-tripo/evidence/` | Resultados da validação e gravação |

## Documentos de apoio

- [Pesquisa técnica](research.md)
- [Modelo de dados e estados](data-model.md)
- [Contrato de controles](contracts/player-controls.md)
- [Roteiro de validação](quickstart.md)

## Revisão de escala pelos degraus

A pedido do usuário, o cenário passou a usar escala uniforme calibrada pelos degraus originais. Medidas finais de 57,600 × 48,396 × 9,857 m, protagonista de 1,80 m e desníveis típicos de aproximadamente 0,18 m. O fator é 0,36 em relação à primeira integração. O modelo original foi restaurado e as escadas adicionais foram removidas.

A referência para reutilização está em [Source/Blender/Ilha_Tripo_011/README.md](../../Source/Blender/Ilha_Tripo_011/README.md). Esta revisão substitui as medidas anteriores de escala.
