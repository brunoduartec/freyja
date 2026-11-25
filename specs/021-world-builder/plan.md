# Plano — World Builder 021
Branch: 021-world-builder. Spec aprovada em 2026-11-15.

## Resumo e contexto técnico
Plugin local C# no Unity 6000.3.25f1, Built-in, macOS. Sem novas dependências. Dados persistidos em ScriptableObjects, terreno exclusivo, instâncias de prefabs identificadas. NUnit/Test Framework existentes para testes geométricos; harness do Editor para integração, captura e relatório. Medir tempo de geração e validação.

## Constituição e preservação
Preservar narrativa e 30 moradores; demo não altera cânone nem determina ilha definitiva. Primeiro trecho avaliável com três casas. Trabalhar na cena principal com root isolado, estados anteriores preservados e alternância explícita. Coordenadas globais da demo centradas em zero exigem desativar somente apresentação antiga, restaurável. Testes não modificam conteúdo original. Nenhuma geração externa.

## Estrutura
Assets/_Project/WorldBuilder/Runtime: dados, identidade e geometria pura.
Assets/_Project/WorldBuilder/Editor: janela, geração, normalizador, validação/captura e integração.
Assets/_Project/WorldBuilder/Tests: EditMode e PlayMode.
Assets/_Project/WorldBuilder/Demo: dados e prefabs locais.
Docs/WorldBuilder: guia e registro.
Artifacts/WorldBuilder: relatórios e imagens.
Autoria nesta worktree; instalar apenas pasta própria no projeto Unity aberto, preservando GUIDs. Guardar backup da cena antes da integração. Sincronizar assets gerados de volta para a branch.

## Fases
1. Dados, conversões, SAT e testes.
2. Terreno determinístico com stamps orientados e estrada independente; conflitos bloqueiam Apply.
3. Janela com grid, handles, Undo, prévia e reconstrução por ID que preserva objetos manuais.
4. Normalizador offline de modelos locais com wrapper e diagnóstico.
5. Demo, testes reais, capturas e documentação.

## Decisões
Padding do apoio deve cobrir interpolação de heightmap 0,625 m. Contato de bordas não é overlap; blends conflitantes bloqueiam aplicação. Undo agrupa geração e dados. Rebuild mantém wrappers estáveis e troca apenas visual gerenciado. Fontes permanecem intactas. Sem atualização de versão/pipeline. Constituição verificada antes/depois do desenho: conforme.
