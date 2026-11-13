# Modelo de dados — MVP web de pistas

**Estado:** contrato de autoria e implementação futura; não há dataset nem aplicação criados nesta etapa.

## Conteúdo do caso

Um `case.json` contém os registros abaixo. IDs são estáveis, em inglês e independentes do texto exibido. A redação é em português.

| Registro | Campos mínimos | Validação |
| --- | --- | --- |
| Caso | `id`, título, introdução, pergunta, conclusão-alvo, versão | Uma conclusão principal; escopo do piloto explícito |
| Personagem | `id`, nome, perfil, vínculos, rotina, conhecimento, crenças, omissões, localização, retrato, tópicos | Três interativos; conhecimento com origem; atributos da campanha fora do piloto não são simulados |
| Retrato | asset, retângulo do rosto normalizado, estado de associação visual | Coordenadas dentro da imagem; nome legível; associação provisória identificada |
| Evidência | `id`, texto, natureza, fonte imediata, `origin_id`, afirmações expressas, condições de aquisição | Seis a nove; origem informacional não se confunde com quem repetiu a frase |
| Afirmação | `id`, texto, verdade autoral, quem a conhece ou acredita | Verdade autoral não é impressa antes da descoberta |
| Interação | `id`, personagem ou ponto, condições, falas, opções, efeitos | Efeitos limitados a conceder evidência e alterar marcadores conhecidos |
| Hipótese | `id`, texto, alternativas de sustentação, contradições | Não concede evidência; requer escolha explícita do jogador |
| Cenário | `id`, título de teste, fontes disponíveis, ações prévias válidas, perdas de fonte | Distinguir estado inicial vazio e conhecimento obtido antes da perda |
| Asset | `id`, caminho web, fonte original, descrição e associações | Aplicação não carrega imagens por caminhos externos à sua raiz |

Natureza da evidência: `observation` ou `testimony`. Objeto, documento, lugar e pessoa são categorias de fonte. Uma inscrição observada não transforma o que está escrito em verdade. Hipóteses ficam separadas das evidências.

## Regras E/OU

Cada alternativa de sustentação contém uma lista de requisitos que devem ser satisfeitos juntos. A lista de alternativas representa OU. Exemplo conceitual: `(A E B) OU (C E D)`. Além de conhecer os requisitos, verificar as origens independentes e as contradições relevantes.

No piloto, cada evidência é atômica e possui um `origin_id`. Dois repetidores do mesmo boato compartilham esse ID. Informações compostas devem ser divididas em itens atômicos; não contar a união de duas repetições como confirmação independente. O autor revisa se as origens são realmente distintas e se os fatos sustentam a hipótese.

Condições de disponibilidade usam somente uma gramática fixa: evidências conhecidas, personagem vivo/disponível, marcadores presentes/ausentes. Não executar expressões arbitrárias vindas do JSON. Marcadores ficam limitados ao necessário para as perguntas do caso, no máximo quatro.

## Estado da sessão

- Conjunto de evidências adquiridas e registro de aquisição, com fonte e sequência.
- Conjunto de personagens vivos/disponíveis.
- Marcadores de conversa que afetam opções futuras.
- Personagem, tópico e posição de fala atuais.
- Hipóteses propostas, evidências selecionadas e histórico de ações.

Dados autorais e estado mutável permanecem separados. A mesma função aplica as ações na interface, nos testes e na busca do validador. O diálogo em andamento não é parte do estado lógico da busca quando não afeta disponibilidade.

## Transições

| Ação | Condição | Efeito |
| --- | --- | --- |
| Conversar/avançar | Fonte disponível e assunto liberado | Exibir fala; aplicar efeitos no ponto definido, sem antecipar evidência |
| Examinar | Ponto acessível e requisitos satisfeitos | Registrar a observação uma única vez |
| Apresentar evidência | Item conhecido e interlocutor disponível | Liberar resposta ou marcador previsto na autoria |
| Propor explicação | Hipótese e itens escolhidos | Avaliar sustentação; não revelar itens ainda desconhecidos |
| Perda de fonte | Configuração de teste autorizada | Bloquear novas falas, preservar evidências e histórico |
| Reiniciar | Cenário selecionado | Limpar estado e aplicar apenas a preparação válida desse cenário |

## Relatório de possibilidades

Para cada configuração de zero, uma ou duas perdas, guardar: fontes removidas, evidências conhecidas inicialmente, conclusão alcançável, sequência de ações, origens usadas e requisitos bloqueados. A preparação de um cenário com relato anterior à morte deve ser reexecutada e validada, não inserida como conhecimento sem procedência.

A busca não mede tempo real, não decide quem é possuído e não valida a festa. O relatório deve declarar esses limites. O dossiê e o diagrama são exportados do mesmo JSON, com IDs e nomes correspondentes.
