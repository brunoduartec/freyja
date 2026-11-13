# Ambiente inicial — Spec 005

Registro em 2026-11-12 antes das alterações.

- Unity MCP conectado ao projeto `Freyja/`; uma instância Main Editor. Cena ativa: `Assets/_Project/Scenes/Ilha_Relevo_004.unity`, limpa (`isDirty=false`), 13 roots carregados. O editor estava em Play Mode (`isPlaying=true`). A cópia 005 foi criada via `AssetDatabase.CopyAsset`; GUIDs diferentes confirmados e o ativo original permaneceu ativo/limpo. Não foi possível abrir a cópia aditiva para inspecionar a hierarquia enquanto Play Mode permanece ativo; a cópia é byte a byte da cena 004, portanto preserva as referências serializadas.
- Blender MCP interativo indisponível (`localhost:9876` não responde). A integração Blender MCP de execução em background está disponível e abriu a fonte 004 em modo de leitura sem salvar; será usada para gerar a cópia 005. Não há alterações de uma sessão Blender conectada a proteger.
- Blender instalado em `/opt/homebrew/bin/blender`; Unity MCP disponível. A configuração do projeto permanece a registrada em `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` e `Packages/packages-lock.json`.
- Fonte 004 inspecionada em modo de leitura: scene `Cais_Iteracao01`, 67 objetos; terreno atual `Terrain_004_Walkable_Island` com dimensões aproximadas 175 × 176 × 31,7 m. A organização e posição relativas à nova planta serão reavaliadas.
- Conjunto de exatidão: `Referencias/ilha-vista-topdown-004.png`, `modelagem-vista-aerea.png` e perfis norte, sul, leste e oeste em `Referencias/ilha-vistas-laterais/`.
- Hashes SHA-256 da base de produção e do conjunto de referências em [`source-hashes.json`](source-hashes.json).

## Resultado do primeiro incremento

- Como o MCP Blender interativo não conectou, a fonte independente foi criada e editada pelo executor Blender MCP em background, que abre o `.blend` especificado sem tocar na sessão interativa. A 004 permaneceu preservada por hash.
- `Source/Blender/Ilha_Relevo_005.blend` agora contém a malha `Terrain_005_ReferenceRelief`, os placeholders reposicionados e câmeras próprias. Arquivo reaberto pelo executor para consultar malha e medidas.
- Renderizados, a partir da fonte, `topdown-match-005.png`, `planta-005.png`, `aerea-005.png` e os quatro perfis cardeais em `specs/005-relevo-fiel-referencia/evidence/`.
- A cópia Unity `Assets/_Project/Scenes/Ilha_Relevo_005.unity` tem GUID próprio e continua fechada. A cena 004 segue ativa e limpa em Play Mode; não foi encerrado o Play Mode nem importado o novo relevo nesta etapa.
- A primeira versão permanece como maquete para avaliação. A análise de inclinações reportada em `comparacao.md` cobre a malha Blender e não representa verificação de colisores ou caminhada Unity.
