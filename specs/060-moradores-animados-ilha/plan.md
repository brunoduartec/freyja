# Integração dos moradores na cena 011

## Contexto técnico e constituição

Unity 6000.3.25f1, Built-in, cena principal confirmada salva e fora de Play Mode. Fontes em TripoAi e Source/Blender/Moradores053 na raiz principal. A branch desta entrega é 060-moradores-animados-ilha. As operações no editor usam a conexão Unity do projeto principal. A constituição e os limites de escopo foram conferidos.

## Produção

Preparar exports com o MCP do Blender, corrigindo o eixo frontal e preservando esqueleto, pesos, texturas e contagem. Guardar as fontes em Source/Blender/Moradores060 e os exports em Assets/_Project/Art/Moradores060. Configurar os importadores Humanoid, conferir os avatares, criar materiais Standard com cores originais e animadores próprios com clipes de repouso e gestos da biblioteca instalada. Os estados terão fases e velocidades discretamente variadas.

Criar prefabs identificados por morador com colisão, apoio no piso e escala proporcional à idade. Instanciar uma vez cada prefab no grupo Moradores060 da cena, distribuindo pelos pontos aprovados. As posições exatas dependem do relevo e das passagens.

## Verificação

Salvar os assets e a cena. Conferir os dez avatares e as contagens, entrar em Play Mode e observar a reprodução e deformação. Validar portas e a circulação entre o cais, largo, salão, A1 e A3 com o controlador do jogador. Registrar posições, capturas e Console em evidence/. Reaproveitar os instrumentos de percurso existentes quando aplicável.
