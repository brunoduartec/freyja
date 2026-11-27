# Auditoria de iluminação — Ilha Principal

**Data:** 2026-11-25  
**Cena:** `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`  
**Validação:** Unity Editor em Play Mode, horário manual 22h, câmera de gameplay.

## Diagnóstico e correções

- `VillageNightLights` já controlava 16 fontes da vila, praça e cais. Cinco postes `Poste 00`, `03`, `07`, `10` e `13` na escadaria/fachada L3 não tinham luminária nem `Light`.
- Foram reutilizadas peças visuais do `Lampião da praça` como cabeças compactas nos cinco postes. Cada novo ponto é quente (RGB 1, 0.62, 0.32), alcance 2,15, intensidade noturna 0,72, modo `ForceVertex` e sem sombras em tempo real. O deslocamento foi colocado para o lado visível do caminho/escada.
- Três objetos adicionais `Lampião da praça -2`, `-2 (1)` e `-2 (2)` tinham pontos de luz zerados e estavam fora do controlador. Foram integrados ao `VillageNightLights`, com intensidade noturna 1,55, alcance 4,8, cor quente, `ForceVertex`, sem sombras e atraso escalonado.
- Total controlado passou de 16 para 24 fontes. Verificação em Play Mode confirmou os oito pontos corrigidos acesos às 22h e desligados às 12h. A integração foi feita no componente existente; nenhum script do ciclo foi alterado.
- A inspeção visual encontrou os focos quentes legíveis no cais, rota baixa, praça e escada L3. Vegetação periférica e trechos sob copas seguem escuros, mas ainda legíveis nas capturas. Não foi adicionada luz aos caminhos onde as fontes já existentes bastavam.

## Capturas

Comparações pareadas (mesmo enquadramento por par):

- Praça antes/depois: [antes](../../Artifacts/lighting/final-before-plaza.png) · [depois](../../Artifacts/lighting/final-after-plaza.png)
- Escada L3 antes/depois: [antes](../../Artifacts/lighting/before-l3.png) · [depois](../../Artifacts/lighting/final-after-l3.png)

Cobertura noturna representativa pela câmera de gameplay:

- [Cais](../../Artifacts/lighting/final-cais.png)
- [Rota baixa](../../Artifacts/lighting/final-lower-route.png)
- [Conector alto](../../Artifacts/lighting/final-upper-connector.png)

## Desempenho observado

Amostras da mesma sessão/cena com os oito pontos novos ativos e inativos:

| Métrica | Novos pontos inativos | Ativos | Diferença observada |
|---|---:|---:|---:|
| Draw calls | 1091 | 1091 | 0 |
| Batches | 64 | 64 | 0 |
| SetPass | 570 | 613 | +43 |
| Triângulos | 1.839.661 | 1.839.661 | 0 |
| CPU frame | 3,58 ms | 3,40 ms | amostra variável |
| GPU | 2,03 ms | 2,75 ms | +0,72 ms nesta amostra |
| Main thread | 2,28 ms | 2,38 ms | +0,10 ms |

As oito luzes são sem sombras em tempo real; os cinco novos pontos da escadaria usam `ForceVertex`. Tempos são amostras pontuais e variam por quadro. Draw calls, batches e geometria não aumentaram; SetPass subiu 43.

## Limitações e aceite

A travessia contínua de todas as rotas não foi concluída: o comando de teclas do runtime não manteve estado entre chamadas no Editor selecionado. Foram inspecionados enquadramentos representativos, e a tentativa a pé confirmou colisão normal com o galpão do píer ao seguir reto; a rota contorna o prédio. Não se declara cobertura sistemática integral do mapa.

Durante as capturas noturnas, a água apareceu magenta após recarga de domínio/Play Mode; não foi alterada nesta entrega e precisa ser investigada separadamente. Isso limita a comparação visual global da costa.

- **SC-001:** parcial — imagens representativas legíveis; travessia contínua integral não concluída.
- **SC-002:** atendido para os oito pontos corrigidos; ciclo dia/noite confirmado.
- **SC-003:** parcial — pares da praça e escada, mais três rotas representativas; sem cobertura integral.
- **SC-004:** atendido — contagens, tempos amostrados e sombras registrados acima.

`git diff --check` reporta quatro linhas de espaço em branco já presentes no arquivo serializado da cena; não foram removidas para evitar mudanças colaterais na cena.
