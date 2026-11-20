# Tarefas — implantação 019

- [x] T001 Preparar plano aprovado.
- [x] T002 Inspecionar cena e referência; capturar terreno sem blocos em planta.
- [x] T003 Identificar e medir os 13 lotes marcados no chão.
- [x] T004 Ajustar os 17 conjuntos; ocupar todos os lotes, respeitando suas extensões e eixos.
- [x] T005 Registrar dimensões, posições, frentes e alturas em `evidence/implantacao.md` e `evidence/implantacao-medida.json`.
- [x] T006 Inspecionar vista geral, planta e vistas próximas do centro, igreja e cais.
- [x] T007 Sobrepor as bases reais do Unity ao terreno; conferir aplicação das medidas e ausência de interpenetrações entre construções.
- [x] T008 Validar percurso cais–centro em Play Mode e Console; salvar cena principal.

## Evidência vigente — revisão pelos limites do terreno

- Vista geral: `evidence/implantacao-lotes.png`.
- Limites do solo e ajustes retangulares: `evidence/lotes-medidos.png`.
- Bases reais dos 17 conjuntos sobre o terreno: `evidence/planta-validada.png`.
- Planta renderizada: `evidence/planta-blocos.png`.
- Vistas próximas: `evidence/detalhe-centro.png`, `evidence/detalhe-igreja.png`, `evidence/detalhe-cais.png`.
- Personagem no centro: `evidence/centro-jogo-lotes.png`.
- Play Mode: `evidence/teste-rota-status.txt`, percurso concluído em 14,52 s; resultado atualizado em 2026-11-15 00:56 UTC.
- Console atual sem erros, sem falha de compilação.

A revisão vigente substitui as implantações anteriores rejeitadas. A correspondência numérica entre os retângulos medidos e suas aplicações em Unity está em `evidence/validacao-geometrica.json`; esse erro não mede fidelidade à imagem artística. Quatro locais não têm marcação de solo e permanecem estimados. As alturas também são estimadas; o terreno e a imagem com construções não são projeções idênticas. O percurso cais–centro foi testado, mas não todos os acessos individuais.
