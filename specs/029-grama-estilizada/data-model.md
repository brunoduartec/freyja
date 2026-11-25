# Modelo de dados

- **Layout** guarda uma lista de células de 8 m. Cada célula tem bounds em coordenadas do mundo e lotes vinculados aos índices das malhas.
- **Lote** guarda matrizes de instâncias com ordem determinística. Tufos mais simples permanecem no nível distante. Os de maior detalhe reduzem progressivamente e somem antes.
- **Componente de cena** referencia câmera, malhas, material e layout. Expõe modos de diagnóstico sem grama, envio integral e otimizado. Após os testes de visibilidade, buffers reutilizáveis juntam instâncias do mesmo modelo em chamadas de até 255 tufos. Contadores registram células, instâncias e chamadas solicitadas ao Unity no último quadro.
- **Authoring** usa Terrain, malhas da trilha e exclusões de acesso para recalcular o layout na cena principal.
