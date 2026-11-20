const esc = value => String(value ?? '').replace(/[&<>"']/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));

export function renderEvidence(data, state, selected = []) {
  const known = data.evidence.filter(item => state.knownEvidence.includes(item.id));
  const rows = known.map(item => {
    const source = item.source?.label ?? 'Origem não identificada';
    return `<label class="evidence-card${selected.includes(item.id) ? ' is-selected' : ''}"><input type="checkbox" name="evidence-selection" value="${esc(item.id)}" ${selected.includes(item.id) ? 'checked' : ''}><span class="checkmark" aria-hidden="true"></span><span class="evidence-body"><span class="evidence-title">${esc(item.title)}</span><span class="evidence-meta">${item.nature === 'observation' ? 'Observação' : 'Depoimento'} · ${esc(source)}</span><span class="evidence-description">${esc(item.description)}</span></span></label>`;
  }).join('');
  return `<section class="panel evidence-panel" aria-labelledby="evidence-heading"><div class="panel-heading"><div><p class="eyebrow">Registro</p><h2 id="evidence-heading">Evidências <span class="count">${known.length}</span></h2></div></div><div class="evidence-list">${rows || '<p class="empty-state">Ainda não há evidências. Explore o mapa e converse com os moradores.</p>'}</div></section>`;
}

export function renderExaminedLines(data, interaction) {
  if (!interaction) return '';
  return `<section class="examined-lines" aria-live="polite"><p class="eyebrow">Anotações do local</p><h3>${esc(interaction.title)}</h3>${interaction.lines.map(line => `<p>${esc(line.text)}</p>`).join('')}</section>`;
}

export function bindEvidence(root, onSelection) {
  root.querySelectorAll('[name="evidence-selection"]').forEach(input => input.addEventListener('change', () => {
    onSelection([...root.querySelectorAll('[name="evidence-selection"]:checked')].map(item => item.value));
  }));
}
