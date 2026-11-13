# Medidas e integração Blender — Ilha Bruta 003

## Fonte e limites

- Fonte editável: `Source/Blender/Ilha_Bruta_003.blend`, derivada por cópia em disco da fonte salva `Source/Blender/Cais_Iteracao01.blend` (hash SHA-256 idêntico no momento da cópia: `ed6ed08907afcecdc22118ebd7e03c722f8e205b0815d95c70e72a359af6e5c1`). A instância interativa aberta não foi operada.
- FBX: `Freyja/Assets/_Project/Art/Environment/IlhaBruta/Ilha_Bruta_003.fbx`.
- Render geral estático: `specs/003-ilha-bruta/evidence/ilha_bruta_overview.png`.
- Unidade Blender: métrica, 1 BU = 1 m. Eixos de trabalho: X leste/oeste, Y sul/norte, Z altura. A costa sul e a junção do cais ficam em Y=4.
- Terreno medido pelos vértices: X=-131.7…132.4 m (264.1 m), Y=4…184 m (180 m), Z=1…21.1 m (20.1 m de relevo; abaixo do limite de referência de 24 m). A extensão nominal solicitada de ~260 × 180 m foi aproximada; X efetivo ficou 4.1 m maior.
- Âncora exata para integração do cais: margem/começo da ilha em Blender `(0, 4, 1)`; terreno conecta ao centro do último degrau na região `(0, 4.94, 1.08)`. Orientação Unity esperada: conversão FBX Y-up padrão; conferir importação e alinhar o cais pelo ponto de junção, sem importar o barco/cais do FBX.

## Coleções exportadas

- `01_Terreno_Costa`: `Terreno_Ilha_Caminhavel_260x180` (2,312 vértices, 4,396 faces) e `Costa_Escarpas_Rocha`.
- `02_Rotas_Caminhaveis`: `Rota_Principal_Cais_Largo_Baixa_Alta_Mirante`, `Rota_Leste`, `Rota_Fundos_Hortas_Ruina_Retorno`. Superfícies com largura nominal 2.8 m, malhas contínuas amostradas na mesma função de altura do relevo; topo da faixa fica 0.1 m acima do terreno para legibilidade. Foi amostrado o centro de todas as três rotas a cada 0.2 m, comparado com o retângulo de cada bloco expandido em 1.4 m: nenhum cruzamento com os 12 blocos.
- `03_Blocos_Casas`: `B1`–`B4`, `A1`–`A4`, `L1`–`L4` (12 blocos separados, sem arquitetura detalhada).
- `04_Marcos_Servicos`: largo, venda, sala comunitária, galpão, horta, ruína e mirante em formas simples.
- `Referencia_Cais_Barco_NaoExportar`: conteúdo herdado da fonte original, oculto no viewport/render e excluído da seleção de exportação. Cais e barco permanecem fora do FBX.

## Rota e distribuição (coordenadas X,Y; metros)

- Cais/entrada `(0,4)` → caminho principal → largo `(0,58)` → casas baixas B (B1=(-44,68), B2=(-39,70), B3=(-45,81), B4=(-58,88)) → casas altas A (A1=(-34,94), A2=(-30,105), A3=(-31,116), A4=(-25,122)) → subida pela crista → mirante norte `(0,174)`.
- Ramal leste: largo → `(14,67)` → `(42,82)` → setor L (L1=(64,84), L2=(65,95), L3=(65,106), L4=(65,116)) → retorno ao eixo principal em `(15,125)`.
- Circuito posterior: mirante → `(-20,166)` → hortas `(-28,146)`, `(-24,142)`, `(-21,137)`, `(-27,132)`, `(-34,139)` → ruína `(-35,130)`, `(-31,127)`, `(-27,131)` → retorno pelo oeste às casas altas/baixas e largo → cais.
- Marcos de serviço: `Servico_Venda` `(6,57)`, `Servico_Sala_Comunitaria` `(-7,58)`, `Servico_Galpao_Cais` `(8,17)`. Não fazem parte dos 12 blocos de casas.

## Materiais provisórios

`Terreno_Grama`, `Costa_Rocha`, `Caminho_Terra`, `Largo_Piso`, `Bloco_Casa_Baixa`, `Bloco_Casa_Alta`, `Bloco_Casa_Leste`, `Marcos_Servicos`, `Ruina_Pedra`, `Horta_Canteiros`; materiais Principled simples, sem texturas externas.

## Export FBX

Exportado pelo operador Blender `bpy.ops.export_scene.fbx` usando somente seleção das quatro coleções da ilha; objetos `MESH`, aplicar modificadores, conversão de unidade, `FBX_SCALE_UNITS`, frente `-Z`, cima `Y`, sem leaf bones, sem texturas incorporadas. O export não contém barco/cais, câmera nem luz. A validação final de importação, escala/eixos no Unity, colisores e caminhada em Play Mode continua a cargo das tarefas de integração Unity.
