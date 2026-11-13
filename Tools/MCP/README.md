# Conexões MCP do projeto

Configuração e leitura dos dois editores verificadas em **12/11/2026**. A evidência completa está em [`connection-status.json`](connection-status.json); ela registra uma verificação pontual, não monitora os processos continuamente.

| Item | Configuração |
| --- | --- |
| Raiz Unity | `Freyja/` |
| Editor Unity | `6000.3.25f1`, instalado pelo Hub |
| Template / renderização | `com.unity.template.3d` / Built-in |
| Integração Unity | `com.unity.pipeline` `1.0.0` |
| Servidor MCP Unity | `~/.unity/bin/unity mcp` (`1.0.0-beta.12`) |
| Blender | `5.2.2 LTS` |
| Servidor MCP Blender | `blender-mcp` `1.0.2` |
| Add-on Blender | `bl_ext.user_default.mcp`, início automático, `localhost:9876` |
| Fonte Blender atual | `Source/Blender/Cais_Iteracao01.blend` (fonte inicial `Freyja.blend` preservada) |
| Destino previsto para exports | `Freyja/Assets/_Project/Art/` |

O add-on foi instalado a partir do [repositório oficial do Blender Lab](https://projects.blender.org/lab/blender_mcp), diretório `addon/blender_mcp_addon/`. Está instalado no usuário desta máquina, não embutido no jogo. O `.blend` inicial mantém os objetos padrão; ainda não é o cenário da primeira entrega.

## Abrir e reconectar no macOS

Na raiz do repositório, execute:

```sh
./Tools/MCP/open_editors.command
```

O comando abre a versão registrada em `ProjectVersion.txt` e o arquivo Blender deste projeto. Usa `--online-mode` para permitir a comunicação do add-on; o bridge escuta somente em loopback. Aguarde o Unity terminar a importação. Para evitar selecionar outro arquivo, mantenha apenas uma instância do Blender com o bridge ativo.

Os servidores `unity` e `blender` já estão registrados globalmente em `~/.codex/config.toml`. A configuração global foi preservada. Para selecionar explicitamente este Unity ao iniciar um cliente MCP, use:

```sh
"$HOME/.unity/bin/unity" mcp --project-path "$PWD/Freyja"
```

Esse comando inicia um servidor **stdio**, destinado a um cliente MCP. Não é um teste de conexão isolado. Se as ferramentas Unity não aparecerem na sessão do agente, reconecte o servidor ou reabra a sessão com o editor já aberto; nesta configuração elas são descobertas a partir do editor disponível. O acesso também foi validado por um cliente MCP Python.

## Verificar a conexão real

Com os dois editores abertos, na raiz do repositório:

```sh
uv run --with 'mcp>=1.2,<2' python Tools/MCP/check_connections.py
```

Nesta máquina também é possível usar o ambiente Python do servidor já instalado:

```sh
"$HOME/.local/share/uv/tools/blender-mcp/bin/python" Tools/MCP/check_connections.py
```

O verificador inicializa os dois MCPs, lista suas ferramentas e faz leituras reais: `eval` no Unity e `execute_blender_code` no Blender. Confere os caminhos do projeto antes de salvar `connection-status.json`. As leituras não alteram as cenas. Falha ou indisponibilidade encerra o comando com erro.

Para diagnóstico rápido do Unity:

```sh
"$HOME/.unity/bin/unity" status --project-path "$PWD/Freyja" --format json
```

O resultado esperado é `state: ready`. Se não conectar, confirme que o projeto está aberto, que `com.unity.pipeline` foi carregado e que não há erros de compilação ou Safe Mode. No Blender, confirme o add-on MCP habilitado, início automático e porta `9876`. Outro processo ocupando a porta pode direcionar o cliente ao arquivo errado; o verificador detecta a divergência de caminho.

A criação foi local, sem vinculação a uma organização Unity Cloud. Serviços de nuvem não são necessários para essas conexões MCP.


## Migração para Freyja

O projeto Unity agora está em `Freyja/` e os scripts de abertura/verificação foram atualizados. O relatório `connection-status.json` é histórico e mantém o nome usado em sua execução; não constitui um teste novo da conexão. A migração de nome foi verificada no editor em batchmode e está documentada em `Source/Unity/Freyja_Rename.validation.json` na raiz do repositório. A conexão MCP não foi revalidada nesta etapa.
