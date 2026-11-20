const esc = value => String(value ?? '').replace(/[&<>"']/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));

export function renderMap(data, state, actions) {
  const mapPath = data.assets?.find(asset => /map\.png$/i.test(asset.path))?.path ?? 'assets/map.png';
  const people = data.characters.map(character => {
    const {x, y} = character.position;
    const lost = !state.aliveSources.includes(character.id);
    return `<button class="map-person${lost ? ' is-lost' : ''}" style="--x:${x * 100}%;--y:${y * 100}%" data-character="${esc(character.id)}" ${lost ? 'disabled' : ''} aria-label="${esc(character.name)}${lost ? ', indisponível neste cenário' : ''}">
      <span class="portrait"><img src="assets/characters-sheet.png" alt="" style="--image-left:${-character.portrait.rect.x / character.portrait.rect.width * 100}%;--image-width:${100 / character.portrait.rect.width}%;--image-shift-y:${-character.portrait.rect.y * 100}%"></span>
      <span class="map-person-name">${esc(character.name)}</span>
    </button>`;
  }).join('');
  const points = data.interactions.filter(item => item.point).map(item => {
    const available = actions.some(action => action.interactionId === item.id);
    return `<button class="map-point${available ? '' : ' is-quiet'}" style="--x:${item.point.x * 100}%;--y:${item.point.y * 100}%" data-interaction="${esc(item.id)}" ${available ? '' : 'disabled'} aria-label="Examinar: ${esc(item.title)}"><span aria-hidden="true">⌖</span><span class="map-point-label">${esc(item.point.label ?? item.title)}</span></button>`;
  }).join('');
  return `<section class="panel map-panel" aria-labelledby="map-heading"><div class="panel-heading"><div><p class="eyebrow">Caderno de campo</p><h2 id="map-heading">A ilha</h2></div><span class="map-hint">Escolha um morador ou ponto</span></div><div class="map-frame"><img class="island-map" src="${esc(mapPath)}" alt="Mapa ilustrado da ilha, com casas, largo, enseada e cais">${points}${people}</div><div class="map-legend"><span><i class="legend-dot"></i> disponível</span><span><i class="legend-dot muted"></i> fonte indisponível</span></div></section>`;
}

export function bindMap(root, data, actions, onAction, onCharacter) {
  root.querySelectorAll('.map-point[data-interaction]').forEach(button => button.addEventListener('click', () => {
    const action = actions.find(item => item.interactionId === button.dataset.interaction);
    if (action) onAction(action);
  }));
  root.querySelectorAll('[data-character]').forEach(button => button.addEventListener('click', () => onCharacter(button.dataset.character)));
}
