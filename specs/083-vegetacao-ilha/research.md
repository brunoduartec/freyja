# Pesquisa e decisões — Vegetação regional

## Decisões

- **Cena única**: `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`, confirmada pelo `AGENTS.md` e Editor conectado na porta 7800.
- **Ativos candidatos existentes**: `ThirdParty/Nature032/CoastalShrub.fbx`, `CoastalFlowers.fbx`, seus materiais (folhagem oliva/escura; pétala creme, amarela e coral suave); `Art/KitPS1_022/Tree_Cypress.fbx`, `Tree_Pine.fbx`; `Art/Grass029/CoastalGrass.fbx` e tufos 078 autorais. As variantes e o uso exato devem ser confirmados no Editor antes do placement.
- **Authoring manual**: poucos grupos manuais na cena, sem distribuição global procedural. Dá controle de composição e mantém a entrega idempotente sem acrescentar subsistema.
- **Integração**: vegetação decorativa estática no vento caso o shader/modelo não responda ao vento global; não alterar shaders ou sistema global. Materiais existentes usam iluminação corrente da cena.
- **Composição**: piloto praia seca/vila; depois vila/jardins e caminhos, praça, encosta, bordas agrícolas e ruínas somente nas áreas presentes e seguras. Evitar palmeira por padrão.
- **Performance**: compartilhar meshes/materiais, sem colliders decorativos; limitar objetos pelo quadro e registrar contagem/custo observável antes/depois.

## Evidência e limitações

- Inventário de arquivo mostrou FBX e materiais, sem prefabs na pasta `Nature032`.
- Unity CLI atual disponível em versão 1.0.0; informou 1.1.0. Atualização não executada porque requer aprovação explícita segundo instrução do CLI.
- `unity status --project-path Freyja --port 7800` reportou Editor 6000.3.25f1 pronto. Há também outro Editor no projeto temporário 079, portanto comandos precisam ser direcionados ao projeto/caminho certo.
- Consulta inicial do Console indicou 1 erro/26 avisos na conexão (1599 erros/387 avisos históricos agregados), sem entradas retornadas no snapshot. Fazer leitura incremental após a mudança e distinguir histórico de novos erros.

## Alternativas descartadas

- Poisson/dispersão procedural em toda a ilha: aumenta risco de invadir gameplay e não é necessário para composição manual.
- Novas árvores costeiras, vegetação animada ou prefabs adicionais: ativos atuais bastam para o primeiro ensaio; não ampliar escopo.
