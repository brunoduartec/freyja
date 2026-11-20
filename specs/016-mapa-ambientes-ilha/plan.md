# Plano: novo mapa e ambientes da ilha

**Branch:** `016-mapa-ambientes-ilha` · **Spec:** [spec.md](spec.md)

## Objetivo

Desenvolver proposta de planta geral legível, em escala e revisável, a partir do inventário de moradores, atividades e espaços. Entregar artefato visual e documentação para avaliação antes de gerar terreno 3D.

## Contexto e limites

Fontes: GDD, plano anterior e escala da ilha Tripo em `Source/Blender/Ilha_Tripo_011/README.md`. As 12 casas e 30 moradores permanecem; igreja e lojas adicionais são propostas. A ilha Tripo atual mede 57,6 × 48,4 m e suas escadas foram a referência; dimensões de um novo mapa serão propostas, não herdadas automaticamente. Planta 2D, sem modificação de Unity ou Blender, sem build ou Play Mode nesta etapa.

## Constituição

Aprovado: preservar fatos do GDD, sinalizar propostas, entregar evidência visual e documentos rastreáveis, sem inventar decisões narrativas. Rever ao concluir.

## Método

1. Extrair as fichas dos 30 moradores, ligando cada um a casa, atividade e local estabelecido ou proposto; identificar necessidades compartilhadas, pendências e interiores previstos.
2. Inventariar ambientes de moradia, economia, circulação, comunidade e investigação, distinguindo fatos e propostas e relacionando-os às pessoas.
3. Definir escala e desenho em planta, com coordenadas coerentes, dimensões humanas, costas, relevo, acessos, 12 casas, comércio, igreja e destinos de investigação. Registrar larguras e distâncias propostas; a caminhada ainda dependerá de teste no jogo.
4. Gerar planta visual editável e imagem para avaliação; conferir a contagem, acessos, legibilidade, escala e consistência com o inventário. Não afirmar geometria 3D nem navegação verificadas.

## Artefatos

`inventario.md`, `mapa.svg`, `mapa.png`, `research.md`, `data-model.md`, `quickstart.md` e `tasks.md` nesta pasta. Sem interfaces externas; contratos não se aplicam. O GDD e a cena existente não serão alterados nesta proposta.
