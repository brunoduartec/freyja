# Tarefas — World Builder
## Preparação
- [x] T001 Auditar ambiente e registrar aprovação em specs/021-world-builder/.
- [ ] T002 Criar módulos isolados em Freyja/Assets/_Project/WorldBuilder/ e backup em Artifacts/WorldBuilder/.
## Fundação
- [ ] T003 Criar testes geométricos em WorldBuilder/Tests/EditMode/.
- [ ] T004 Implementar dados e geometria em WorldBuilder/Runtime/.
## US1 — montar trecho
- [ ] T005 [US1] Implementar terreno, fundações, vias e conflitos em WorldBuilder/Editor/WorldGenerator.cs.
- [ ] T006 [US1] Criar painel e handles com prévia e Undo em WorldBuilder/Editor/WorldBuilderWindow.cs.
- [ ] T007 [US1] Criar demo e alternância preservando cena em WorldBuilder/Editor/WorldDemo.cs.
## US2 — substituir modelos
- [ ] T008 [US2] Implementar normalizador offline e testes em WorldBuilder/Editor/ExternalModelNormalizer.cs e Tests/EditMode/.
## US3 — regenerar
- [ ] T009 [US3] Validar IDs, Undo, objetos manuais e reconstruções em Tests/EditMode/.
## Entrega
- [ ] T010 Executar EditMode, PlayMode, inspeção de cena e capturas em Artifacts/WorldBuilder/.
- [ ] T011 Documentar uso e limitações em Docs/WorldBuilder/, sincronizar artefatos e conferir fontes.

Dependências: T001→T002→T003→T004→T005→T006→T007→T008→T009→T010→T011. Pesquisa somente leitura pode ocorrer em paralelo; mutações do editor sequenciais. Validar US1 gerando trecho, US2 trocando casa, US3 repetindo rebuild. Não expandir para ilha inteira.
