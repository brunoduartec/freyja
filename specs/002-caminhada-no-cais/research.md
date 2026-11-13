# Pesquisa — Caminhar pelo cais

**Data:** 12/11/2026. Fontes: spec, GDD/constituição, pacotes e arquivos locais; revisão delegada somente leitura. Nenhum editor foi modificado.

## Controlador e terreno
**Decisão:** CharacterController com gravidade explícita e parâmetros ajustáveis.
**Justificativa:** atende caminhada e degraus sem simulação dinâmica do jogador. Move não aplica gravidade automaticamente; step offset e skin width afetam passagem e travamentos.
**Alternativas:** Rigidbody exigiria resposta física não pedida; pacotes completos de personagem ampliariam dependências.
**Fontes:** [Move — Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/charactercontroller/move), [CharacterController — Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/physics-section/physics-overview/character-control-section/class-character-controller).

## Input e cursor
**Decisão:** reutilizar referências às ações do asset existente; limitar Move e consumir Look como delta, sem nova multiplicação por tempo. A captura do cursor depende de clique; Esc e perda de foco interrompem o controle.
**Justificativa:** o projeto já usa exclusivamente Input System (activeInputHandler=1). O asset contém Player/Move e Look, UI/Cancel e Click. O sistema/editor pode mudar o lock; não assumir foco como autorização para recapturar.
**Alternativas:** misturar Input Manager antigo ou reativar automaticamente ao ganhar foco contrariaria previsibilidade e o FR-007.
**Fontes:** [Cursor.lockState — Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/cursor/lockstate), [Pointer — Input System 1.20](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.Pointer.html); asset local Assets/InputSystem_Actions.inputactions.

## Câmera
**Decisão:** rig pequeno em perspectiva com SphereCast, proteção do near clip e masks explícitos; sem Cinemachine novo.
**Justificativa:** raycast único não protege as bordas da câmera. Validar quinas, encosta, rotação perto de caixas e recuperação da distância após obstrução.
**Alternativas:** câmera fixa não cumpre o controle por mouse; atravessar obstáculos e só ocultar o jogador não cumpre SC-002.

## Cenário e colisão
**Decisão:** criar cena separada; completar somente proxies pertinentes à caminhada e câmera.
**Justificativa:** Tools/Unity/CreateCaisReview.cs adicionou colisões às tábuas/degraus e três superfícies. A evidência anterior confirma 123 renderers e dois raycasts positivos, mas não certifica caixas, pedras, barco ou navegação.
**Alternativas:** alterar o prefab-base afetaria a revisão aprovada; supor que raycasts de piso provam percurso seria insuficiente.

## Figura e recuperação
**Decisão:** figura provisória de 1,8 m produzida no Blender via MCP, sem animação final. Spawn validado; limite de queda/bounds e reset de velocidades/câmera. Estado não estático.
**Justificativa:** permite testar escala, mantém a constituição de produção visual e evita carregar posição entre partidas.
**Alternativas:** personagem final, natação, save/load ou checkpoint dinâmico ampliariam o recorte.
**Fonte:** [Domain Reload — Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/domain-reloading).

## Resultado
Sem esclarecimentos bloqueantes. Valores de velocidade, câmera, spawn e controlador são ajustes reversíveis a verificar na implementação. O teste observado e a aceitação do usuário continuam futuros.
