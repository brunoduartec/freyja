# Migração do nome para Freyja — 13/11/2026

Título e repositório definidos pelo usuário: Freyja e `https://github.com/brunoduartec/freyja.git`.

| Antes | Agora |
| --- | --- |
| Pasta local `Ilha Misterio/` | `Freyja/` |
| Projeto Unity `IlhaMisterio/` | `Freyja/` dentro da raiz do repositório |
| `Source/Blender/IlhaMisterio.blend` | `Source/Blender/Freyja.blend` |
| Namespace `IlhaMisterio.Prototype` | `Freyja.Prototype` |
| Produto Unity `IlhaMisterio` | `Freyja` |

Os scripts mantêm atributos `MovedFrom` para reconhecer os tipos serializados sob o namespace antigo. Os GUIDs existentes devem permanecer intactos. Cenas e prefabs serão salvos pelas APIs do editor na validação da migração, sem edição manual de seu YAML.

Registros JSON de evidência de entregas anteriores não foram reescritos: seus nomes e caminhos correspondem ao momento da execução. Os hashes históricos não são apresentados como validações posteriores à migração.

A antiga pasta local é um link simbólico de compatibilidade para a nova pasta real, fora do conteúdo do repositório. Ele mantém esta sessão e integrações que usavam o caminho anterior funcionais, sem duplicar o projeto.

Nenhuma tarefa do MVP web foi implementada durante esta migração. A validação e sincronização são registradas separadamente quando concluídas.

## Verificação executada

Unity 6000.3.25f1 executado em batchmode: compilação bem-sucedida, cinco cenas e quatro prefabs abertos e salvos pelas APIs do editor, zero scripts ausentes. Os identificadores serializados agora usam Freyja.Prototype. Os 80 GUIDs originais, a versão do editor, os manifests/locks de pacotes e os bytes do Blender inicial foram preservados. O helper temporário de migração foi removido.

Relatório: [Freyja_Rename.validation.json](../Source/Unity/Freyja_Rename.validation.json). Não foi executado Play Mode nem build de plataforma nesta migração. O MVP web continua apenas planejado, com 30 tarefas pendentes.
