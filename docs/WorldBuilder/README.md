# Freyja World Builder — primeira versão

Abra **Tools > Freyja > World Builder** no Unity. O plugin está em `Assets/_Project/WorldBuilder`. A demonstração e seu backup se referem à cena histórica `Ilha_Tripo_Caminhada_011`, disponível no histórico do Git. A cena oficial é `Assets/_Project/Scenes/Ilha_Principal.unity`.

## Montar e editar
1. Clique **Criar / abrir demonstração** e **Ativar demonstração na cena**. Isso preserva o estado dos ambientes anteriores.
2. Escolha um lote. Ajuste centro, tamanho, yaw (rotação), footprint (área da construção), prefab e cota. Para cota automática, desmarque Manual Height.
3. No Scene View, selecione o ponto do lote e use as alças para mover e girar. O movimento encaixa no grid; 1 m e 0,5 m são suportados. A altura é editada na cota do painel.
4. A prévia mostra volumes e orientações. Se houver conflito, os avisos bloqueiam Aplicar. Ajustar dados não regenera o terreno a cada movimento.
5. Clique **Aplicar / reconstruir gerenciados**. O relevo volta à base da seed antes de nivelar as fundações. Undo desfaz a operação agrupada.
6. **Restaurar cenário anterior** recupera ambientes, posição do personagem, câmera e ponto de início. Alterne fora de Play Mode.

## Preparar e trocar uma casa
Selecione um FBX ou prefab já existente em **Modelo / FBX**, informe área máxima em metros e a correção de eixos. Clique **Criar prefab normalizado** e salve com outro nome. A fonte permanece intacta; o wrapper mantém escala raiz 1, proporções uniformes e base centralizada. O descritor salvo registra dimensões, orientação, escala e avisos.

Atribua o novo prefab ao lote e aplique. A identidade, posição e cota do lote permanecem. O footprint deve comportar os limites visuais, inclusive beirais nesta versão conservadora. Frente padrão +Z. Revise manualmente modelos que tragam piso decorativo ou base irregular. Não há geração de modelos por serviços externos.

## Demonstração
Terreno de 160 × 160 m, altura de referência de 32 m, origem (-80,-4,-80), resolução 257; seed 1138. Três lotes de 12 × 14 m, rua de 5 × 56 m, três casas provisórias vinculadas a prefabs, acessos independentes e rochas locais. O protagonista existente mede 1,80 m. A dimensão da demo não determina o tamanho final da ilha.

## Validação
**Validar e capturar** salva JSON, Markdown e três imagens reais em `Artifacts/WorldBuilder/<data-hora>/`, na raiz do repositório. O relatório distingue medidas automáticas de revisão visual e não afirma build/testes que não executou.

No Test Runner, execute os assemblies **Freyja.WorldBuilder.Tests** (EditMode) e **Freyja.WorldBuilder.PlayTests** (PlayMode). O método `Freyja.WorldBuilder.Tests.ResultRecorder.Run(false/true)` executa o mesmo fluxo e salva os XMLs em `Artifacts/WorldBuilder/`. As capturas não são uma comparação automática com baseline.

## Limitações
- Ferramenta de autoria inicial, sem ilha completa, distribuição das famílias, igreja, comércio ou geração de bairros.
- Vias retas por segmento; interseções e blends conflitantes são recusados. Relevo procedural parametrizado, sem pincel de escultura próprio; editar o Terrain manualmente será sobrescrito no próximo rebuild.
- Prévia por contornos/volumes; não há renderização transparente do modelo completo.
- Modelos com base irregular exigem revisão; não há remoção automática de pisos, retopologia ou importador GLB novo.
- O FBX real testado é uma rocha local; as casas da demo são proxies. A normalização não garante qualidade artística de qualquer casa futura.
- Arquivos gerados pertencem à ferramenta. Alterações em posições dos wrappers gerenciados são reaplicadas a partir dos dados; objetos manuais são preservados.
- Build de distribuição fora desta entrega.

External 3D generation jobs: 0
