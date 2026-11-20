# Relatório de validação narrativa

Fonte: `ivo-departure-case` em `Tools/MysteryWeb/data/case.json`. Busca por estados alcançáveis usando as mesmas ações da interface.

---

## Configurações de perda

### 1. Chegada sem perdas (`arrival-no-losses`) — fontes perdidas: nenhuma

**Abertura do cenário:** Luana pede ajuda no cais; todos os interlocutores estão disponíveis.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 172  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

### 2. Artur indisponível (`loss-artur`) — fontes perdidas: artur

**Abertura do cenário:** O caderno permanece examinável; Artur não está disponível para novas conversas.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 91  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

### 3. Ivo indisponível (`loss-ivo`) — fontes perdidas: ivo

**Abertura do cenário:** A tira, o caderno e os degraus permanecem; Ivo não oferece falas atuais.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 138  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

### 4. Artur e Ivo indisponíveis (`loss-artur-ivo`) — fontes perdidas: artur, ivo

**Abertura do cenário:** Teresa pode relatar o que ouviu; o caderno, a marca e os degraus continuam acessíveis.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 57  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

### 5. Teresa indisponível (`loss-teresa`) — fontes perdidas: teresa

**Abertura do cenário:** Luana e o cais ainda podem ser investigados; Teresa não oferece falas atuais.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 138  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

### 6. Artur e Teresa indisponíveis (`loss-artur-teresa`) — fontes perdidas: artur, teresa

**Abertura do cenário:** Documentos, a marca e o caminho da encosta continuam acessíveis.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 57  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

### 7. Teresa e Ivo indisponíveis (`loss-teresa-ivo`) — fontes perdidas: ivo, teresa

**Abertura do cenário:** Artur e os vestígios do cais e da encosta continuam disponíveis.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 104  
**Evidências usadas:** ev-departure-notice, ev-ivo-note  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-hillside-step → examine:inspect-departure-notice → examine:inspect-ivo-note → propose:hypothesis-imposed-departure (ev-ivo-note, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

---

## Memória obtida antes da perda

### 2. Relato de Ivo registrado antes da perda (`memory-ivo-before-loss`) — fontes perdidas: ivo

**Abertura do cenário:** O caderno do jogador preserva o depoimento obtido enquanto Ivo estava vivo; a cena não reencena a chegada nem permite novas perguntas a ele.  
**Resultado:** SOLUÇÃO encontrada  
**Estados visitados:** 31  
**Evidências usadas:** ev-departure-notice, ev-ivo-selection  
**Origens independentes:** ivo-personal-account, public-departure-notice  
**Percurso:** examine:inspect-departure-notice → propose:hypothesis-imposed-departure (ev-ivo-selection, ev-departure-notice)

**Requisitos bloqueados:**
  - nenhum

---

## Resultado

- Caso válido em todas as configurações: **sim**
- Percurso de memória anterior válido: **sim**
- Tempo medido: **10.1 ms** (limite: 5.000 ms; dentro do limite)
- Configurações de perda examinadas: **7**

A busca verifica alcançabilidade e rastreabilidade de evidências. Não simula o relógio de campanha, a causa das mortes ou a qualidade narrativa; esses pontos exigem revisão humana.
