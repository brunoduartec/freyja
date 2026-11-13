# Backlog — recorte narrativo adiado

Preservado em 12/11/2026 após o pedido de iterações curtas. Este conteúdo não é o aceite da iteração visual atual.

# Feature 001 — Chegada e marca de Ivo

**Estado:** proposta pronta para revisão.

## Objetivo

Testar se o jogador consegue seguir uma contradição por conta própria, do desembarque à descoberta do motivo da partida de Ivo.

## Histórias e aceitação

1. **Desembarcar e se orientar (P1).** Dado o início da cena no cais, o jogador encontra Artur e chega ao largo a pé. A conversa estabelece que os três amigos chegaram sem aviso e que Artur quer que partam.
2. **Cruzar pistas (P1).** O jogador pode conversar com Nuno sobre a próxima viagem e com Luana sobre Helena em qualquer ordem. Cada pista registra seu conteúdo e sua fonte. Com as duas pistas, o jogador pode perguntar a Ivo por que a partida depende do nascimento.
3. **Entender a marca (P1).** O jogador vê a tira azul na manga de Ivo e ouve que ele foi escolhido para partir antes do nascimento, deixando a família na ilha. A conclusão fica registrada no caderno.

## Requisitos

- FR-01: Usar o cais, o largo e os degraus da encosta como área do protótipo.
- FR-02: Permitir caminhar, iniciar e encerrar diálogos e consultar o caderno.
- FR-03: Liberar a pergunta de dedução quando o jogador reunir as duas pistas, em qualquer ordem.
- FR-04: Manter o estado das pistas ao reabrir o caderno e ao repetir diálogos, sem entradas duplicadas.
- FR-05: Durante este trecho, o jogador pode explorar no próprio ritmo.

**Recorte:** quatro moradores em cena (Artur, Nuno, Luana e Ivo), com os dois amigos representados na chegada. A revelação da bruxa, a escolha de partir cedo e a simulação do elenco inteiro são candidatas a entregas posteriores.

**Critério de sucesso:** numa partida de teste, a pessoa encontra o largo e consegue explicar por que voltaria a falar com Ivo usando as pistas que coletou. Registrar o percurso, a interpretação do jogador e os pontos de bloqueio.

## Tarefas anteriores (histórico)

# Tarefas — Chegada e marca de Ivo

- [x] T001 — Preparar Unity MCP e verificar Blender MCP. Registrar versões, pastas e resultado da conexão.
- [ ] T002 — Criar o trecho do cais ao largo e aos degraus no Blender e importar na Unity. Depende de T001.
- [ ] T003 — Fazer a cena abrir no desembarque e permitir caminhar pelo percurso. Depende de T002.
- [ ] T004 — Escrever e inserir os diálogos de Artur, Nuno, Luana e Ivo. Depende de T001.
- [ ] T005 — Registrar as duas pistas no caderno e habilitar a pergunta a Ivo. Depende de T003 e T004.
- [ ] T006 — Jogar pelas duas ordens, repetir conversas e verificar o critério de sucesso. Depende de T005.

## Evidências de execução

Preencher com tarefa, resultado e caminho do arquivo ou da captura. Registrar bloqueios junto à tarefa afetada.

- **T001 — 12/11/2026:** Unity `6000.3.25f1` em `Freyja/`, template 3D Built-in e pacote `com.unity.pipeline` `1.0.0`. Editor em estado `ready`; leitura via MCP confirmou o projeto correto, `Assets/Scenes/SampleScene.unity`, sem compilação ou atualização em andamento. Blender `5.2.2 LTS` com add-on oficial habilitado e bridge local `localhost:9876`; leitura via MCP confirmou `Source/Blender/Freyja.blend`. Caminho previsto de exportação: `Freyja/Assets/_Project/Art/`. Evidência: `Tools/MCP/connection-status.json`. Abertura, diagnóstico e verificador reproduzível: `Tools/MCP/README.md`.
