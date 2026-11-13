# Implementation Plan: MVP web de pistas — spec 009

**Branch**: `main` | **Feature**: `009-sistema-misterio` | **Date**: 2026-11-13 | **Spec**: [spec.md](spec.md)
**Input**: recorte web explicitamente solicitado pelo usuário, mantendo a spec 009. Esta entrega contém planejamento e tarefas, sem implementação.

## Summary

Produzir primeiro uma amostra de autoria e um navegador simples para testar se as pistas e falas são compreensíveis. Três fichas clicáveis — Artur, Teresa e Ivo — sobre o mapa existente, diálogo em sequência, pontos examináveis e lista de evidências sempre disponível. Seis a nove evidências sustentam uma conclusão: a partida de Ivo é imposta pela vila e ligada ao nascimento, não uma viagem voluntária. A confirmação dessa redação e dos fatos que a sustentam acontece na autoria, antes do código de interação.

O conteúdo será separado da apresentação e usado tanto pela aplicação quanto pelo dossiê, mapa E/OU e relatório de possibilidades. As falas são escritas, não geradas em execução. A validação examina sequências reais de ações e não somente a presença abstrata de nós num grafo.

A evolução é: conteúdo e contratos → estrutura da investigação → interface de teste → caminhos e hipóteses → cenários de perda → avaliação. Não implementar Unity, novos modelos ou a campanha completa.

## Technical Context

**Language/Version**: JavaScript com módulos ES (`.mjs`); Node.js 22.12.0 disponível para testes e ferramentas. HTML e CSS para a interface.
**Primary Dependencies**: APIs nativas do navegador e biblioteca padrão do Node; sem framework, instalação de pacotes ou backend nesta amostra.
**Storage**: um `case.json` de autoria e estado de sessão em memória. Dossiê e mapa são exportações desse conteúdo; reinício limpa a sessão.
**Testing**: `node:test` e `node:assert/strict` para regras, validador e conteúdo; verificação no navegador para interação e apresentação.
**Target Platform**: navegador desktop; conferir também adaptação em 390×844. Servidor HTTP local para testes.
**Project Type**: aplicação web estática de validação narrativa, fora de `Freyja/Assets/`.
**Performance Goals**: interações locais sem espera perceptível; relatório do pequeno caso em até cinco segundos na máquina de trabalho, a medir na implementação.
**Constraints**: reusar imagens; não introduzir serviços, geração de falas, backend ou dependências no projeto Unity. O painel mostra apenas o conhecimento do jogador.
**Scale/Scope**: três personagens interativos, seis a nove evidências, uma conclusão principal, no máximo quatro marcadores de conversa e sete configurações de perdas de fontes.

## Constitution Check

| Princípio | Aplicação | Resultado |
| --- | --- | --- |
| GDD guarda decisões; spec delimita entrega | Usar o GDD como cânone; separar amostra e regras futuras | Atende |
| Unity/Blender para produção do jogo | MVP web solicitado explicitamente como ferramenta prévia de validação; projeto Unity permanece intacto | Atende ao escopo autorizado |
| Direção visual legível | Reusar mapa e rostos; conferir identificação, recorte e leitura nas resoluções de teste | Atende |
| Fatos, depoimentos e hipóteses distintos | Conteúdo, regras e painel preservam atribuição e origem informacional | Atende |
| Conclusão de tarefas com evidência | Relatório de testes, caminhos, capturas e revisão do autor acompanham a futura entrega | Atende |

Reavaliação após o desenho: nenhuma alteração de engine, pipeline ou dependência do jogo; nenhuma exceção adicional necessária.

## Project Structure

### Documentation (this feature)

```text
specs/009-sistema-misterio/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── assets.md
├── contracts/web-mvp.md
├── quickstart.md
├── tasks.md
└── checklists/requirements.md
```

### Source Code (repository root; estrutura futura, ainda não criada)

```text
Tools/MysteryWeb/
├── index.html
├── styles.css
├── README.md
├── data/case.json
├── assets/map.png
├── assets/characters-sheet.png
├── src/app.mjs
├── src/case-loader.mjs
├── src/investigation.mjs
├── src/validation.mjs
├── src/views/map.mjs
├── src/views/dialogue.mjs
├── src/views/evidence.mjs
├── scripts/validate.mjs
├── scripts/export-dossier.mjs
└── tests/*.test.mjs
Source/Narrative/mvp-arrival/
├── dossier.md
├── clue-map.mmd
└── validation-report.md
```

**Structure Decision**: o JSON dentro da ferramenta é a fonte estruturada do piloto. As exportações legíveis não são cópias editadas separadamente. A aplicação testa decisões; não é um segundo projeto Unity. Código futuro fica restrito à pasta nova e as fontes visuais existentes são preservadas.

## Desenho da solução

1. **Autoria antes da interface.** Escrever as três fichas, a verdade do caso, as crenças e os assuntos disponíveis. Luana aparece no texto de abertura. Escolher seis a nove evidências, com duas ou mais origens independentes por conjunto de sustentação e caminhos alternativos após perdas. Atribuições visuais provisórias ficam identificadas no dossiê.
2. **Uma máquina de estado pequena.** Estado contém evidências conhecidas, personagens vivos/disponíveis, marcadores de conversa, hipóteses propostas e histórico de ações. A ação só aplica efeitos se suas condições forem atendidas. Repetir aquisição é idempotente; morte remove conversa futura, não a memória já registrada.
3. **Interface simples.** Mapa com pontos e fichas circulares; conversa ocupa área própria; painel de evidências permanece visível. Perguntas e falas vêm dos dados. O usuário escolhe hipótese e evidências; a interface não revela automaticamente a conclusão.
4. **Rastreabilidade.** Evidência tem uma origem informacional atômica; duas falas que repetem o mesmo relato apontam para a mesma origem. Documento comprova o registro de uma afirmação, não sua veracidade. As combinações aceitas são escritas e revisadas, não deduzidas pela quantidade de itens.
5. **Validador por busca em largura.** Explorar estados alcançáveis pelas mesmas ações usadas na interface, guardando predecessores. Para cada configuração de fontes, produzir percurso de aquisição e sustentação da conclusão ou listar requisitos bloqueados. Descartar estados repetidos e não somar ramos incompatíveis. Ciclos sem evidência inicial não criam conhecimento.
6. **Perdas controladas.** Validar zero, uma e duas testemunhas indisponíveis entre as três do piloto, mais um percurso que obtém o relato antes da perda. São configurações de teste de fontes; não simulam sorteios, autores de homicídio ou o relógio da campanha. Um cenário insolúvel proposital deve fazer o validador falhar.

## Cobertura e limites da prova

A amostra deve provar rastreabilidade, efeito das escolhas de pergunta, ordem de descoberta, sustentação de hipóteses e continuidade após perda de fontes. Não prova a viabilidade dos três dias em tempo real, o equilíbrio das mortes, a festa ou a robustez dos 30 moradores. A validação humana ainda precisa conferir se as combinações são convincentes e se a história interessa; um caminho computável não garante boa escrita.

O mapa visual é referência existente, não uma alteração do layout final da ilha. Círculos são máscaras da folha original, sem redesenho ou raster novo. A correspondência de Ivo é provisória no piloto; ver [assets.md](assets.md).

## Validação prevista

- Conteúdo: IDs únicos, referências existentes, fontes, condições, ligações de retrato e pelo menos dois percursos válidos.
- Regras: E/OU, repetição, origem compartilhada, documento falso, hipóteses não usadas como prova de si mesmas, condições de conversa e memória após morte.
- Possibilidades: sete configurações, percurso concreto por configuração, cenário negativo detectado e relatório sem lacunas ocultas.
- Navegador: clicar nas fichas e pontos; percorrer falas; consultar pistas durante conversa; propor explicação; reiniciar; operar por teclado. Capturas em 1366×768, 1920×1080 e 390×844.
- Revisão: avaliar o motivo inicial para investigar e pedir justificativa da conclusão com duas fontes. Registrar feedback antes de ampliar o elenco.

## Complexity Tracking

| Escolha | Necessidade | Alternativa dispensada |
| --- | --- | --- |
| Busca por estados no pequeno caso | Respeitar condições de aquisição e escolhas incompatíveis | Contar nós/pistas sem provar um percurso |
| Web estática com assets existentes | Pedido do usuário para testar falas e pistas rapidamente | Framework, backend ou cena Unity para esta validação |

**Estado:** apenas plano. Os comandos e caminhos de aplicação descritos nos demais artefatos são previstos para a implementação; não foram executados nem criados nesta etapa.
