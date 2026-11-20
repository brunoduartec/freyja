# Escala dos modelos TripoAI — Freyja

Referência ajustada a pedido do usuário em 13/11/2026. A altura dos degraus originais determina a escala do cenário. O protagonista mede 1,80 m.

## Medidas para reutilização

| Item | Valor |
| --- | --- |
| Ilha no Unity, largura × profundidade × altura | 57,600 × 48,396 × 9,857 m |
| Redução uniforme em relação à primeira integração de 160 m | 0,36 em todos os eixos |
| Transform local de Cenario e Cenario_Colisao na cena 011 | (5760, 5760, 5760) |
| Desníveis típicos entre patamares dos degraus | aproximadamente 0,18 m |
| Altura do protagonista | 1,80 m |
| Step Offset da cápsula | 0,25 m |

A amostragem da escada na malha original mostrou patamares sucessivos próximos de 11,96, 12,49, 12,96, 13,47 e 13,89 m na escala antiga. Diferenças de 0,42 a 0,53 m passam a aproximadamente 0,15 a 0,19 m com o fator 0,36. As superfícies do TripoAI são irregulares e os valores representam patamares típicos.

O fator de Transform 5760 depende do FBX e do importador atuais. Ao reexportar ou importar de outro modo, calibrar pelas dimensões medidas no Unity. Para o GLB original com largura de uma unidade, a largura final de referência é 57,6 m. Aplicar o mesmo fator nos três eixos e à colisão.

## Fontes e decisão

- Fonte original do cenário em `TripoAi/cenario/stylized island terrain 3d model.glb`.
- `Cenario.blend` contém a malha original restaurada e sua colisão simplificada. A escala final está aplicada nas instâncias da cena Unity.
- Exports em `Freyja/Assets/_Project/Art/Ilha_Tripo_011/`.
- Cena em `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity`.
- As escadas adicionais da tentativa anterior foram removidas. Os degraus usados são os do modelo original.
- A referência de 260 × 180 m do GDD corresponde ao planejamento geral. A instrução mais recente usa os degraus como parâmetro deste modelo. A versão de escala por eixos para atingir 260 × 180 × 24 m foi substituída pela calibração uniforme descrita acima.

## Verificação

No Play Mode, com movimento aplicado ao controlador via MCP, o personagem subiu as duas escadas com velocidade de 3 m/s ao final e retornou ao spawn. A cena foi salva em modo de edição. A inspeção manual da sensação de caminhada permanece disponível para avaliação.

[Registro do teste](../../../specs/011-caminhada-modelos-tripo/evidence/escala-degraus-teste.txt) · [Personagem na escada](../../../specs/011-caminhada-modelos-tripo/evidence/escada-escala-pelos-degraus.png) · [Vista geral](../../../specs/011-caminhada-modelos-tripo/evidence/ilha-escala-pelos-degraus.png)
