# Plano de corrida e salto

**Branch** `063-corrida-salto` · [Spec](spec.md)

## Contexto técnico

Unity 6000.3.25f1, C#, Input System e CharacterController existentes. A entrega usa `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`. O editor conectado via MCP confirmou a cena ativa.

## Constituição

A entrega segue o escopo aprovado, reutiliza os controles e o avatar da cena e avalia as ações na câmera do jogo. Registrar o resultado em Play Mode junto das tarefas.

## Execução

1. Ligar as ações Sprint e Jump à sessão da ilha. Amostrar os comandos somente durante exploração ativa e atualizar o texto de controles.
2. Aplicar velocidade de corrida e salto no controlador existente. Preservar colisões, ajuste de velocidade nas escadas e recuperação.
3. Adicionar estados de corrida e salto ao Animator do protagonista com os clipes humanoides já importados.
4. Verificar compilação, controles, animações e regressões em Play Mode.
