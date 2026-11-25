# Validação do piloto

Abrir Ilha_Tripo_Caminhada_011 com a integração de moradores 060. Se a cena ainda precisar ser configurada, usar Freyja > Diálogos > Instalar piloto e salvar. Iniciar Play Mode e clicar para capturar o controle.

1. Falar com Ivo antes de ouvir depoimentos. Conferir seu assunto inicial e sair.
2. Falar com Nuno e Luana. Voltar a Ivo e conferir a pergunta nova. Repetir um assunto e conferir o caderno.
3. Reiniciar Play Mode, ouvir Luana antes de Nuno e voltar a Ivo. Verificar o mesmo resultado.

## Resultado em 2026-11-19

- Cena principal salva com a instância de diálogo e os dez moradores de A1–A3 da entrega 060. Ivo e Luana usam seus prefabs existentes. Em B2, Nuno possui referência 2D; foi usado um figurante identificado no cais.
- Play Mode via Unity MCP verificou o alcance, o bloqueio com zero e um depoimento, a liberação nas duas ordens e o caderno com fontes sem duplicatas. O roteiro usou reposicionamento e chamada dos métodos de seleção.
- Em um segundo Play Mode, entradas sintéticas pelo Input System confirmaram E para abordar Nuno, Esc para encerrar e devolver a captura, e Tab para abrir o caderno. O clique físico nas opções e a caminhada completa entre os três não fizeram parte da verificação.
- [Captura do painel de Ivo](dialogo-ivo.png).

A cena integrada está no projeto Unity aberto na raiz principal. Esta branch guarda o código, a evidência e o instalador reproduzível para a cena com a entrega 060.

## Nuno integrado ao elenco

Nuno Pires substituiu o figurante na área de abastecimento do cais, junto ao armazém, em (-32,50; 1,87; -41,50). O prefab NunoPires tem rig Mixamo de 65 ossos, 28.856 triângulos, 16.829 vértices com peso e 1,76 m de altura na pose de importação. A fonte editável está em Source/Blender/Moradores060/NunoPires.blend e o rig bruto em TripoAi/nuno-pires.

Play Mode confirmou avatar válido, reprodução de repouso, ausência de sobreposição da cápsula, tecla E junto ao modelo e registro do depoimento do próximo barco com a fonte Nuno Pires. O teste aproximou o jogador e enviou E pelo Input System. Ivo mantém sua associação própria. [Nuno no cais](nuno-no-cais.png) e [conversa](nuno-dialogo.png).

Tripo consumiu 30 créditos na geração (6b9d886c-ccd6-4a64-9a5a-00554fd16508), 0 na checagem (75c10255-2258-4da0-907a-b766308d3597) e 25 no rig (8ac19a18-d929-4047-b94c-6b9d7a123d61). Saldo final de 25, com 0 bloqueados.
