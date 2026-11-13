# Validação — UV e textura do TripoAI

**Data**: 2026-11-13. **Resultado**: primeira versão entregue para avaliação visual.

## Executado

- Blender 5.2.2 LTS controlado pelo MCP global; arquivo final salvo e reaberto na sessão interativa.
- Fonte: Source/Blender/Ilha_Tripo_007/Ilha_Tripo_Texturizada.blend.
- GLB original, topologia, posições dos vértices e transformação preservados, verificados por SHA-256/arrays.
- 290.322 vértices e 580.632 triângulos; nenhuma redução de malha.
- 580.632 faces mapeadas em Island_UV; coordenadas finitas no intervalo 0–1; zero faces com área UV nula.
- Operador nativo uv.select_overlap do Blender: zero faces/arestas/vértices selecionados por sobreposição.
- Seis imagens carregadas, empacotadas e também existentes nos caminhos externos relativos.
- Atlas 2048×2048 com amostragem Closest; quatro padrões de material e máscara de distribuição preservados.
- Cinco renders finais: geral, superior, caminhos, falésia e praia. Inspeção visual efetuada nos PNGs.

## Correções durante a revisão

A grama foi aquecida e clareada. Máscaras de rocha e praia foram corrigidas para preservar os rochedos e restringir areia ao piso da enseada. A comparação com o material-fonte revelou pontos pretos no bake em pequenos fragmentos UV. A extensão das cores vizinhas preencheu os texels vazios: amostragem no centro de todas as faces passou de 2.700 faces com texel preto para zero. Os renders finais foram refeitos após a correção e o blend reaberto.

## Evidências

Ver Source/Blender/Ilha_Tripo_007/validation.json, baseline.json, uv_check.json, padding.json e texel_check.json, além dos PNGs em previews/ e do README.md.

## Limitações e pendências

- Malha densa original mantida. O UV automático contém muitos fragmentos pequenos; não foi feita retopologia para pintura manual ou otimização de jogo.
- A textura segue a referência, acompanhando a geometria do TripoAI. Irregularidades e depressões de caminhos/degraus do modelo não foram remodeladas.
- Sem integração Unity, compilação no editor, Play Mode ou build; fora do escopo aprovado.
- Avaliação estética do usuário pendente. Parar nesta versão antes de ampliar o escopo.
