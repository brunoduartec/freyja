# Plano — Iluminação dos percursos

1. Preservar a cena atual e capturar a câmera de gameplay à noite em rotas representativas.
2. Auditar objetos `Poste *`, fontes de luz e controlador `VillageNightLights`; registrar posições, alcances e sombras.
3. Reutilizar a luminária da praça para iluminar os cinco postes da escada L3 e registrar suas luzes no controlador existente.
4. Ajustar somente sobreposições ou pontos comprovadamente escuros observados durante a travessia noturna.
5. Capturar novamente os mesmos enquadramentos, percorrer rotas, conferir estado diurno e medir desempenho e modos de sombra.
6. Documentar resultados, imagens, limitações e verificações na pasta da spec.

## Restrições

- Não criar modelos, não introduzir sistema novo e não alterar scripts de ciclo dia-noite.
- Preservar as alterações locais preexistentes de terreno e água na branch base.
- As alterações persistentes de iluminação são restritas à cena principal e à documentação desta entrega.
