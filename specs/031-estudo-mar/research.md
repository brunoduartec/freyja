# Decisões técnicas

- Mar ativo na cena 011 fica sob Island023, com superfície de 600 m na cota zero. A malha original serve de referência. A opção B precisa de mais vértices para ondas com escala local.
- Máscara em coordenadas de mundo dá estabilidade na câmera pixelada. A costa vem do cruzamento do terreno com a cota zero. Estacas e rochas têm contatos locais registrados na mesma máscara.
- A e B usam a mesma paleta, espuma e vista distante. B soma ondas de senos com menor amplitude na enseada. A comparação isola a contribuição da geometria.
- O recorte de 150 m deve ser avaliado na vista elevada. Nas variantes, a cor pode se aproximar do fundo perto desse limite.
