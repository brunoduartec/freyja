# Plano de implementação — Spec 004: Relevo da ilha

**Spec:** [spec.md](spec.md) · **Referência:** [modelagem-vista-aerea.png](../../Referencias/modelagem-vista-aerea.png) · **Estado:** planejamento; sem implementação iniciada.

**Guia complementar para modelagem:** [ilha-vista-topdown-004.png](../../Referencias/ilha-vista-topdown-004.png), gerado a partir da referência aérea. É uma interpretação visual, não uma planta medida; o lado de trás do monte e a casinha foram inferidos. A spec continua prevalecendo: todo o terreno caminhável e casas do povoado representadas por blocos nesta iteração.

## Resumo

Reorganizar a maquete existente em uma ilha mais compacta, com enseada, povoado escalonado e um monte dominante ao fundo. Toda a superfície terrestre deverá ser acessível caminhando, inclusive as encostas e o topo. Atrás do monte haverá um recanto com casinha simples, fora da vista do largo. Manter a caminhada, os 12 blocos, os principais marcos e a recuperação da 003. A referência guia proporções e composição; a face posterior, não mostrada, será uma interpretação para avaliação.

## Contexto técnico

- Unity 6000.3.25f1, renderização Built-in, Input System 1.20.0; Blender 5.2.2 LTS. Usar os MCPs globais disponíveis de Blender e Unity na implementação.
- A ilha 003 mede aproximadamente 264 × 180 m no Blender, com elevação de 1 a 21,1 m. Seu cais referencia `(0, 4, 1)` em coordenadas Blender; confirmar alinhamento real na importação.
- O jogador 003 usa `CharacterController` com `slopeLimit` de 60°, `stepOffset` de 0,25 m, raio de 0,3 m e velocidade de 3 m/s. Preservar esses parâmetros, entrada e câmera da 003; não alterar a velocidade para compensar distância.
- Alvo de construção: inclinação do terreno preferencialmente até 30° e em nenhum ponto caminhável superior a 35°, medido sobre os triângulos de colisão após a importação. Essa margem abaixo de 60° reduz risco de bloqueios em costuras e discretização. Transições devem ser suaves, sem degraus além do limite do jogador. O monte pode parecer mais íngreme por silhueta, facetas e cores, sem paredes terrestres inacessíveis.
- As dimensões finais serão medidas após uma primeira maquete compacta: precisam ser menores que a 003 nas duas dimensões horizontais, mantendo espaço para os 12 blocos, caminhos, monte e área posterior. Não inferir metros exatos da imagem sem escala.
- Escopo exclui interiores, NPC, diálogos, missões e casas finais do povoado. A casinha isolada será um volume simples com telhado, sem interior.

## Verificação da constituição

| Princípio | Antes e depois do desenho |
| --- | --- |
| Spec e GDD orientam a entrega | Atendido: a spec define escopo; o isolamento é proposta narrativa, sem criar fatos ou diálogos. |
| Unity e Blender via MCP | Atendido: edição, integração e verificações serão feitas pelos MCPs globais quando disponíveis. |
| 3D low poly e legibilidade | Atendido: relevo facetado e comparação aérea e na câmera de jogo. |
| Cânone e pistas | Atendido: nenhum morador, depoimento ou pista será implementado. |
| Evidências por tarefa | Atendido: guardar medições, capturas, resultados de navegação e preservação. |

## Estrutura e preservação

```text
specs/004-relevo-ilha/
  spec.md, plan.md, research.md, data-model.md, quickstart.md
  contracts/navegacao.md, contracts/integracao.md
  evidence/                         # criar na implementação
Source/Blender/Ilha_Relevo_004.blend
Freyja/Assets/_Project/Art/Environment/IlhaRelevo/Ilha_Relevo_004.fbx
Freyja/Assets/_Project/Prefabs/IlhaRelevo.prefab
Freyja/Assets/_Project/Scenes/Ilha_Relevo_004.unity
```

Criar fonte, exportação, prefab e cena 004 separados. Partir da cena 003 sem salvar ou aplicar alterações aos prefabs e cenas anteriores. Antes de operar qualquer editor, inspecionar mudanças não salvas e preservá-las; registrar hashes dos arquivos anteriores e verificar de novo ao final. No Blender, se a sessão aberta estiver com alterações do usuário, trabalhar em cópia separada via MCP sem descartá-las. As rotas podem ser redesenhadas na 004; não introduzir colisores nos caminhos visuais que atrapalhem o relevo.

## Fases de execução previstas

1. **Preparar e medir:** confirmar estado dos editores e versões, salvar referências e capturas da 003, medir os limites atuais e criar cópias 004 sem modificar o que já existe.
2. **Relevo e composição:** modelar costa, enseada, terraços e monte em superfície contínua, compacta e sem superfícies terrestres inacessíveis; reposicionar os 12 blocos e os marcos; reservar a casinha isolada atrás do monte e rotas legíveis.
3. **Integração:** importar o modelo e preparar a cena 004 com colisões, mar sem collider, encaixe com cais, câmera, spawn e bounds calculados para o novo terreno; preservar controles e parâmetros do jogador.
4. **Verificação abrangente:** medir inclinação em todos os triângulos de colisão e continuidade/conectividade em uma amostragem densa para o corpo do jogador; executar trajetos reais por múltiplas direções, topo, costa, povoado e recanto; testar colisão, câmera, mar e recuperação. A inspeção geométrica é cobertura estrutural, e percursos amostrados não provam literalmente infinitos pontos: documentar resolução, regiões não cobertas e problemas.
5. **Apresentação:** salvar vistas comparáveis e registro com medidas, cobertura, limitações e hashes; abrir a cena para avaliação do usuário. Parar para avaliação sem marcar aprovação visual como recebida.

## Riscos e decisões

- A referência sugere paredões, enquanto a spec exige todas as superfícies terrestres caminháveis: privilegiar a caminhada, sugerindo rocha com materiais e formas moderadas. Não ocultar superfícies intransponíveis como se fossem acessíveis.
- A imagem não mostra a parte de trás: posição e aparência da casinha são uma proposta, não um fato da referência.
- Controles sintéticos e capturas não equivalem a uma avaliação manual com teclado e mouse; registrar separadamente. Caso um MCP ou interação real esteja indisponível, registrar a limitação sem declarar o teste realizado.

## Complexidade

Sem novas dependências ou alteração de versão prevista. O requisito de toda a superfície caminhável exige análise geométrica e testes distribuídos além de um único caminho, mas não exige um novo sistema de navegação do jogo.
