const esc = value => String(value ?? '').replace(/[&<>"']/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));

export function renderDialogue(data, state, actions, selectedCharacterId = null) {
  if (!state.dialogue) {
    const person = data.characters.find(item => item.id === selectedCharacterId && state.aliveSources.includes(item.id));
    return `<section class="panel conversation-panel" aria-labelledby="conversation-heading"><p class="eyebrow">Conversas</p><h2 id="conversation-heading">${person ? esc(person.name) : 'Quem pode contar algo?'}</h2><p class="muted">${person ? 'Escolha o assunto abaixo.' : 'Selecione um morador no mapa para ver os assuntos disponíveis.'}</p></section>`;
  }
  const interaction = data.interactions.find(item => item.id === state.dialogue.interactionId);
  if (!interaction) return '';
  const person = data.characters.find(item => item.id === interaction.characterId);
  const line = interaction.lines[state.dialogue.lineIndex];
  return `<section class="panel conversation-panel is-speaking" aria-labelledby="conversation-heading"><div class="panel-heading"><div><p class="eyebrow">${person ? 'Conversa' : 'Assunto'}</p><h2 id="conversation-heading">${esc(person?.name ?? interaction.title)}</h2></div><button class="icon-button" data-close-dialogue aria-label="Fechar conversa">×</button></div>
    <div class="dialogue-copy" aria-live="polite">${line ? `<p class="speaker">${esc(line.speaker)}</p><p class="dialogue-line">${esc(line.text)}</p>` : `<p class="dialogue-prompt">${esc(interaction.title)}</p><p class="muted">A conversa começa quando você avançar.</p>`}</div>
    <button class="primary-button" data-advance>${state.dialogue.lineIndex >= interaction.lines.length - 1 ? 'Concluir conversa' : line ? 'Continuar' : 'Ouvir'}</button>
  </section>`;
}

export function renderTopics(data, state, actions, selectedCharacterId = null) {
  if (state.dialogue) return '';
  const people = selectedCharacterId ? data.characters.filter(item => item.id === selectedCharacterId) : data.characters;
  const groups = [];
  for (const person of people) {
    if (!state.aliveSources.includes(person.id)) continue;
    const topics = data.interactions.filter(item => item.characterId === person.id && actions.some(action => action.interactionId === item.id));
    if (!topics.length) continue;
    const rows = topics.map(item => {
      const response = item.responseToEvidenceId ? data.evidence.find(e => e.id === item.responseToEvidenceId) : null;
      return `<div class="topic-row"><div><strong>${esc(item.title)}</strong>${response ? `<span class="topic-requires">Responder a: ${esc(response.title)}</span>` : ''}</div><button class="small-button" data-interaction="${esc(item.id)}" ${response ? 'data-present' : 'data-talk'}>${response ? 'Apresentar' : 'Conversar'}</button></div>`;
    });
    groups.push(`<section class="person-topics"><h3>${esc(person.name)}</h3>${rows.join('')}</section>`);
  }
  return `<section class="topic-list" aria-label="Assuntos disponíveis">${groups.length ? groups.join('') : '<p class="muted small-note">Nenhum novo assunto disponível por enquanto.</p>'}</section>`;
}

export function bindDialogue(root, actions, onAction, onAdvance, onClose) {
  root.querySelector('[data-advance]')?.addEventListener('click', onAdvance);
  root.querySelector('[data-close-dialogue]')?.addEventListener('click', onClose);
  root.querySelectorAll('[data-talk], [data-present]').forEach(button => button.addEventListener('click', () => {
    const action = actions.find(item => item.interactionId === button.dataset.interaction);
    if (action) onAction(action);
  }));
}
