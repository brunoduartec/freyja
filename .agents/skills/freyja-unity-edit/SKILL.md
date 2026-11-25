---
name: freyja-unity-edit
description: Editar e validar scripts C#, cenas, prefabs ou interações no Unity do Freyja após aprovação da spec.
---

# Edição e validação no Unity do Freyja

1. Consulte a spec da entrega. Use o MCP global do Unity e confirme o caminho do projeto e a cena ativa antes de editar. As instruções em `Tools/MCP/README.md` ajudam a identificar e reconectar a instância. Para consultas determinísticas em que a CLI for mais direta, use `unity command --query <termo> --project-path "$PWD/Freyja"` a partir da raiz. Prefira comandos estruturados do Editor.
2. Use a cena principal indicada em `AGENTS.md`. Prefira APIs do Editor para cenas, assets e prefabs. Reutilize prefabs, preserve referências e GUIDs e salve apenas os artefatos alterados. Preserve o sistema de input, câmera e UI. Identifique placeholders previstos e preserve assets finais fora da mudança.
3. Mantenha classes pequenas, nomes em inglês e namespaces coerentes. Prefira referências privadas com `[SerializeField]` e valide dependências obrigatórias. Separe estado da sessão das definições compartilhadas. Use ScriptableObjects quando fizer sentido, IDs de investigação estáveis e eventos ou chamadas explícitas para evitar buscas e alocações em `Update`. Siga o plano antes de introduzir persistência ou abstrações.
4. Após mudanças de código, verifique compilação e Console. Para cenas e interações, teste o fluxo afetado em Play Mode. Use Edit Mode para regras determinísticas e Play Mode para comportamentos do Editor quando o risco justificar. Em mudanças de investigação, teste ordens de descoberta, repetição de diálogos e transições pertinentes. Quando a entrega inicial de Ivo estiver em escopo, teste as duas ordens que liberam a pergunta.
5. Agrupe as mudanças antes da validação final. Capture a câmera real quando a legibilidade de rostos, pistas, interações ou silhuetas low poly com pixels visíveis estiver em jogo. Faça build quando empacotamento ou plataforma estiver no escopo e registre plataforma e resultado. Traga o primeiro erro relevante e um resumo de testes. Preserve o log original para diagnóstico.
