# Decisão técnica

A primeira solução usa a distribuição existente, as malhas locais e `RenderMeshInstanced`. O caminho Built-in será verificado com GPU instancing no Forward Base e medidas de luz adicional. A resolução interna favorece geometria opaca simples. O custo de envio, de luzes e de sombras será medido. Um caminho compute e indirect acrescenta buffers e passes e só se justifica diante de gargalo medido.

Endless Meadow fica como referência de vento, máscaras e LOD. A integração direta é incompatível com o pipeline do Freyja. Evidências do código e da licença estão na [auditoria](auditoria.md).
