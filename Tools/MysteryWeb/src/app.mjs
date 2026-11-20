import { applyAction, availableActions, createSession } from './investigation.mjs';
import { loadCase } from './case-loader.mjs';
import { renderMap, bindMap } from './views/map.mjs';
import { renderDialogue, renderTopics, bindDialogue } from './views/dialogue.mjs';
import { renderEvidence, renderExaminedLines, bindEvidence } from './views/evidence.mjs';

const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, char => ({
  '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
}[char]));
const root = document.querySelector('#app');
let data;
let state;
let selectedCharacterId = null;
let selectedEvidenceIds = [];
let selectedHypothesisId = '';
let examinedInteraction = null;
let feedback = '';
let notice = '';
let lossCharacterId = '';
let nextFocus = null;

function resetSession(scenarioId) {
  state = createSession(data, scenarioId);
  selectedCharacterId = null;
  selectedEvidenceIds = [];
  selectedHypothesisId = '';
  examinedInteraction = null;
  lossCharacterId = '';
  feedback = '';
  notice = '';
  render();
}

function act(action) {
  const previousDialogue = state.dialogue;
  const result = applyAction(data, state, action);
  state = result.state;
  feedback = result.feedback || '';
  if (action.type === 'examine' && result.ok) {
    examinedInteraction = data.interactions.find(item => item.id === action.interactionId) ?? null;
    notice = 'As observações deste local foram registradas no caderno.';
  }
  if (action.type === 'advance' && result.ok) {
    examinedInteraction = null;
    notice = '';
  }
  if (result.ok && previousDialogue && !state.dialogue && ['advance','closeDialogue'].includes(action.type)) {
    const personId = data.interactions.find(item => item.id === previousDialogue.interactionId)?.characterId;
    selectedCharacterId = personId ?? null;
    if (personId) nextFocus = `[data-character="${personId}"]`;
  }
  if (action.type === 'talk' || action.type === 'present') {
    selectedCharacterId = null;
    examinedInteraction = null;
    notice = '';
    nextFocus = '[data-advance]';
  }
  if (action.type === 'propose') {
    if (result.ok) notice = result.feedback;
    selectedEvidenceIds = [];
  }
  render();
}

function hypothesisPanel() {
  if (state.knownEvidence.length < 2 && !state.completed) {
    return `<section class="panel hypothesis-panel"><p class="eyebrow">Síntese</p><h2>Compare os relatos</h2><p class="muted">Registre pelo menos duas evidências antes de avaliar explicações.</p></section>`;
  }
  const options = data.hypotheses.map(item => `<option value="${escapeHtml(item.id)}" ${item.id === selectedHypothesisId ? 'selected' : ''}>${escapeHtml(item.text)}</option>`).join('');
  const picked = data.evidence.filter(item => selectedEvidenceIds.includes(item.id));
  const solved = state.completed ? data.hypotheses.find(item => item.id === state.acceptedHypothesisId) : null;
  return `<section class="panel hypothesis-panel" aria-labelledby="hypothesis-heading">
    <p class="eyebrow">Sua leitura</p><h2 id="hypothesis-heading">Propor uma explicação</h2>
    <label class="field-label" for="hypothesis-select">Qual explicação combina com as evidências?</label>
    <select id="hypothesis-select" ${state.completed ? 'disabled' : ''}><option value="">Escolha uma explicação</option>${options}</select>
    <p class="muted proposal-hint">Marque as evidências que sustentam sua leitura e proponha quando estiver pronto.</p>
    <div class="selected-evidence">${picked.length ? picked.map(item => `<span class="evidence-chip">${escapeHtml(item.title)}</span>`).join('') : '<span class="muted">Nenhuma evidência selecionada.</span>'}</div>
    <button class="primary-button propose-button" data-propose ${!selectedHypothesisId || !selectedEvidenceIds.length || state.completed ? 'disabled' : ''}>Propor explicação</button>
    ${solved ? `<div class="conclusion-note"><p class="eyebrow">Conclusão registrada</p><p>${escapeHtml(solved.conclusion)}</p></div><p class="completion-note">Investigação concluída. Reinicie para percorrer outro cenário.</p>` : ''}
  </section>`;
}

function render() {
  if (!state) return;
  const actions = availableActions(data, state);
  const activeScenario = data.scenarios.find(item => item.id === state.scenarioId);
  const lostPeople = data.characters.filter(person => !state.aliveSources.includes(person.id));
  const sameLosses = lostPeople.length === activeScenario.lostSources.length
    && lostPeople.every(person => activeScenario.lostSources.includes(person.id));
  const subtitle = !sameLosses
    ? `Revisão de fontes. Sem novas conversas com: ${lostPeople.map(person => person.name).join(', ')}. Os relatos já registrados permanecem no caderno.`
    : activeScenario.id === data.scenarios[0].id ? data.introduction : activeScenario.opening;
  const evidenceScrollTop = root.querySelector('.evidence-list')?.scrollTop ?? 0;
  const focused = document.activeElement;
  const focus = nextFocus ? { selector: nextFocus }
    : focused?.matches?.('[data-advance]') ? { selector: '[data-advance]' }
      : focused?.matches?.('[data-close-dialogue]') ? { selector: '[data-close-dialogue]' }
        : focused?.matches?.('[data-reset]') ? { selector: '[data-reset]' }
          : focused?.matches?.('[data-lose]') ? { selector: '[data-lose]' }
            : focused?.id ? { selector: `#${focused.id}` }
              : focused?.matches?.('[data-character]') ? { character: focused.dataset.character }
                : focused?.matches?.('[name="evidence-selection"]') ? { evidence: focused.value }
                  : focused?.matches?.('[data-interaction]') ? { interaction: focused.dataset.interaction } : null;
  nextFocus = null;
  const peopleOptions = data.characters.filter(person => state.aliveSources.includes(person.id))
    .map(person => `<option value="${escapeHtml(person.id)}" ${lossCharacterId === person.id ? 'selected' : ''}>${escapeHtml(person.name)}</option>`).join('');

  root.innerHTML = `<header class="masthead"><div><p class="eyebrow">Freyja · investigação narrativa</p><h1>${escapeHtml(data.title)}</h1><p class="subtitle">${escapeHtml(subtitle)}</p></div><div class="case-seal" aria-hidden="true">FM</div></header>
    <section class="scenario-bar" aria-label="Cenário de revisão"><div class="scenario-copy"><span class="eyebrow">Percurso de revisão</span><span class="scenario-title">${escapeHtml(activeScenario?.title)}</span><span class="scenario-opening">${escapeHtml(notice)}</span></div><label class="scenario-field"><span>Trocar cenário</span><select id="scenario-select">${data.scenarios.map(item => `<option value="${escapeHtml(item.id)}" ${item.id === state.scenarioId ? 'selected' : ''}>${escapeHtml(item.title)}</option>`).join('')}</select></label><div class="loss-test"><label for="loss-select">Teste de indisponibilidade</label><div class="loss-controls"><select id="loss-select"><option value="">Escolha uma fonte</option>${peopleOptions}</select><button class="small-button" data-lose ${!lossCharacterId ? 'disabled' : ''}>Simular perda</button></div></div></section>
    ${feedback ? `<p class="feedback" role="status">${escapeHtml(feedback)}</p>` : ''}
    <div class="workspace"><div class="left-column">${renderMap(data, state, actions)}${renderExaminedLines(data, examinedInteraction)}</div><div class="middle-column">${renderDialogue(data, state, actions, selectedCharacterId)}${renderTopics(data, state, actions, selectedCharacterId)}</div><aside class="right-column">${renderEvidence(data, state, selectedEvidenceIds)}${hypothesisPanel()}</aside></div>
    <footer class="page-footer"><span>Fatos observados, depoimentos e hipóteses ficam registrados separadamente.</span><button class="text-button" data-reset>Reiniciar cenário</button></footer>`;

  bindMap(root, data, actions, act, characterId => {
    selectedCharacterId = selectedCharacterId === characterId ? null : characterId;
    render();
  });
  bindDialogue(root, actions, act, () => act({ type: 'advance' }), () => act({ type: 'closeDialogue' }));
  bindEvidence(root, ids => { selectedEvidenceIds = ids; render(); });
  root.querySelectorAll('[data-character]').forEach(button => button.classList.toggle('is-selected', button.dataset.character === selectedCharacterId));
  root.querySelector('.evidence-list').scrollTop = evidenceScrollTop;
  if (focus?.selector) root.querySelector(focus.selector)?.focus({ preventScroll: true });
  if (focus?.character) [...root.querySelectorAll('[data-character]')].find(button => button.dataset.character === focus.character)?.focus({ preventScroll: true });
  if (focus?.evidence) [...root.querySelectorAll('[name="evidence-selection"]')].find(input => input.value === focus.evidence)?.focus({ preventScroll: true });
  if (focus?.interaction) [...root.querySelectorAll('[data-interaction]')].find(button => button.dataset.interaction === focus.interaction)?.focus({ preventScroll: true });

  root.querySelector('#scenario-select')?.addEventListener('change', event => resetSession(event.target.value));
  root.querySelector('#loss-select')?.addEventListener('change', event => {
    lossCharacterId = event.target.value;
    root.querySelector('[data-lose]').disabled = !lossCharacterId;
  });
  root.querySelector('[data-lose]')?.addEventListener('click', () => {
    if (!lossCharacterId) return;
    const person = data.characters.find(item => item.id === lossCharacterId);
    const result = applyAction(data, state, { type: 'loseSource', characterId: lossCharacterId });
    state = result.state;
    feedback = result.feedback || '';
    notice = `${person?.name ?? 'Fonte'} ficou indisponível para novas conversas neste percurso.`;
    selectedCharacterId = null;
    lossCharacterId = '';
    render();
  });
  root.querySelector('[data-reset]')?.addEventListener('click', () => resetSession(state.scenarioId));
  root.querySelector('#hypothesis-select')?.addEventListener('change', event => {
    selectedHypothesisId = event.target.value;
    render();
  });
  root.querySelector('[data-propose]')?.addEventListener('click', () => {
    if (!selectedHypothesisId || !selectedEvidenceIds.length) return;
    act({ type: 'propose', hypothesisId: selectedHypothesisId, evidenceIds: selectedEvidenceIds });
  });
}

try {
  data = await loadCase('data/case.json');
  resetSession(data.scenarios[0].id);
} catch (error) {
  root.innerHTML = `<section class="load-error" role="alert"><p class="eyebrow">Caderno indisponível</p><h1>Não foi possível abrir a investigação.</h1><p>${escapeHtml(error?.message ?? 'Falha ao carregar os dados do caso.')}</p><p>Verifique se os arquivos do caso e seus assets estão disponíveis e tente novamente.</p></section>`;
}
