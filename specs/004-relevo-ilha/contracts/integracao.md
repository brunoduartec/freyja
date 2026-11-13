# Contrato de integração e preservação

- Usar uma fonte Blender, FBX, prefab e cena 004 distintos; preservar fontes, cenas, prefabs, pacotes e versões anteriores. Registrar hashes antes e depois.
- Operar Blender e Unity via MCP global quando disponíveis. Inspecionar e preservar alterações não salvas de sessões existentes antes de abrir ou salvar arquivos.
- O FBX deve manter escala em metros, eixo e âncora coerentes com o cais; verificar visualmente e por colisão a junção após importação.
- O collider corresponde à superfície terrestre caminhável; o mar é visual e sem collider. Blocos e casinha têm colisões sem isolar trechos de terreno. Não adicionar chão invisível sobre a água.
- Reutilizar personagem, câmera e controles da 003. Ajustar apenas na cena 004 o mar visual, spawn, alcance da câmera e bounds de recuperação para conter toda a nova ilha e o cais.
- A casinha simples fica atrás do monte, sem linha de visão desde o largo; verificar com a câmera do jogador e uma vista aérea. Não criar NPC ou interior.
