# Pesquisa — Ilha bruta caminhável

**Data:** 12/11/2026. Fontes: GDD, spec, constituição, plano local de mapa e implementação da 002. Análise somente de leitura.

## Topologia e escala
**Decisão:** uma ilha de referência aproximada de 260 × 180 m e até 24 m, cais ao sul, largo no interior e mirante ao norte; 12 blocos divididos entre três bairros.
**Justificativa:** corresponde à spec e ao GDD. O plano local sugere caminho principal de 60–80 m e 2,5–3 m de largura, subidas graduais e retorno pelos fundos.
**Alternativa:** impor dimensões como medidas finais antes de caminhar impede calibrar a experiência. Formato, distância e tempo devem ser revisados no teste.
**Referências:** [GDD](../../GDD.md) e [plano de mapa](../../../Low%20Poly%20World/docs/PLANO_ILHA_E_VILAREJO.md). O segundo é orientação de projeto sujeita ao GDD e à avaliação atual.

## Cena e modelo
**Decisão:** nova fonte Blender, FBX/prefab próprios e cena Unity separada; manter cais e jogador já validados.
**Justificativa:** permite retrabalhar a maquete sem quebrar a cena anterior. Desativar sobreposições apenas na nova cena.
**Alternativas:** sobrescrever a cena anterior ou editar o prefab compartilhado dificultaria comparação e recuperação.

## Limites, colisões e câmera
**Decisão:** ampliar os bounds por instância, manter recuperação após queda e usar colisão em relevo/blocos compatível com o controlador existente. Rever mar e distância de visão na nova cena.
**Justificativa:** o limite serializado atual de 28 × 20 × 30 m foi feito para o cais e impediria explorar 260 × 180 m; o mar anterior tem extensão limitada e a câmera foi configurada para distância de visão de 150 m.
**Alternativas:** desativar recuperação ou colocar piso invisível no mar prejudicaria os controles existentes. Usar apenas raycasts de piso não comprova percurso contínuo.

## Conteúdo e testes
**Decisão:** 12 volumes identificados, sem casas finais; três serviços podem ter volumes distintos. Testar ida e retorno pelos pontos principais, colisões, câmera e recuperação em Play Mode.
**Justificativa:** permite avaliar implantação e escala antes da arte final. A validação da 002 não cobre esta ilha.
**Alternativas:** moradores, interiores, diálogos e detalhamento ampliariam o escopo. Automação é opcional e não substitui caminhada real.

**Resultado:** sem dúvidas técnicas bloqueantes. Posição exata dos lotes, medidas finais e tempos permanecem ajustes de maquete sujeitos à avaliação do usuário.
