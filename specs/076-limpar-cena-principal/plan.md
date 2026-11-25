# Plano — Limpeza da cena principal

## Contexto técnico

Unity 6000.3.25f1, Built-in, projeto `Freyja/`. A entrega altera a cena existente e as referências editoriais ao nome antigo. O Unity MCP fornece a hierarquia, a serialização e a verificação jogável.

## Constituição

A spec foi aprovada. O Unity é a fonte da hierarquia e dos testes. A câmera real verifica a continuidade visual. As decisões por objeto e as verificações ficam junto à spec.

## Execução

1. Inventariar a árvore e referências serializadas. Cruzar candidatos com ferramentas de autoria, specs anteriores e substitutos ativos.
2. Remover instâncias confirmadas pelo Editor, preservar as dependências do nível e salvar. Renomear o asset preservando o GUID.
3. Atualizar caminhos operacionais e validar referências, Console, câmera, chegada, navegação e interação.
