const assert = (test, message) => { if (!test) throw new Error(message); };
const list = value => value ?? [];
const idPattern = /^[a-z][a-z0-9_-]*$/;

export function validateCase(data) {
  assert(data && typeof data === 'object', 'Caso inválido');
  assert(idPattern.test(data.id) && Number.isInteger(data.version) && data.version > 0, 'Identificação do caso inválida');
  for (const key of ['title','introduction','question']) assert(typeof data[key] === 'string' && data[key].length > 0, `Texto do caso ausente: ${key}`);
  const groups = ['characters', 'origins', 'claims', 'evidence', 'interactions', 'hypotheses', 'scenarios'];
  const ids = {};
  for (const group of groups) {
    assert(Array.isArray(data[group]), `${group}: lista obrigatória`);
    ids[group] = new Set();
    for (const item of data[group]) {
      assert(idPattern.test(item.id), `${group}: ID inválido ${item.id}`);
      assert(!ids[group].has(item.id), `${group}: ID duplicado ${item.id}`);
      ids[group].add(item.id);
    }
  }
  assert(data.characters.length === 3, 'O piloto exige três personagens');
  assert(data.scenarios.length > 0, 'Cenários ausentes');
  assert(data.evidence.length >= 6 && data.evidence.length <= 9, 'O piloto exige seis a nove evidências');
  assert(Array.isArray(data.markers) && data.markers.length <= 4 && new Set(data.markers).size === data.markers.length, 'Marcadores inválidos');
  const ref = (group, id) => assert(ids[group].has(id), `${group}: referência inválida ${id}`);
  const refs = (group, values) => { assert(Array.isArray(values), `${group}: referências devem ser lista`); values.forEach(id => ref(group, id)); };
  const markerRefs = values => { assert(Array.isArray(values), 'Marcadores devem ser lista'); values.forEach(id => assert(data.markers.includes(id), `Marcador inválido ${id}`)); };
  const position = (point, label) => {
    assert(point && Number.isFinite(point.x) && Number.isFinite(point.y) && point.x >= 0 && point.x <= 1 && point.y >= 0 && point.y <= 1, `Coordenada inválida ${label}`);
  };
  const condition = (value = {}) => {
    assert(value && typeof value === 'object' && !Array.isArray(value), 'Condição inválida');
    for (const key of Object.keys(value)) assert(['allEvidence', 'anyEvidence', 'alive', 'markers', 'notMarkers'].includes(key), `Condição desconhecida ${key}`);
    for (const key of ['allEvidence', 'anyEvidence']) if (value[key]) refs('evidence', value[key]);
    if (value.anyEvidence) assert(value.anyEvidence.length > 0, 'Regra OU vazia');
    if (value.alive) refs('characters', value.alive);
    for (const key of ['markers', 'notMarkers']) if (value[key]) markerRefs(value[key]);
  };
  for (const c of data.characters) {
    assert(typeof c.name === 'string' && c.name.length, `Nome ausente ${c.id}`);
    position(c.position, c.id);
    const rect = c.portrait?.rect;
    assert(rect && ['x','y','width','height'].every(k => Number.isFinite(rect[k])), `Retrato inválido ${c.id}`);
    assert(rect.x >= 0 && rect.y >= 0 && rect.width > 0 && rect.height > 0 && rect.x + rect.width <= 1 && rect.y + rect.height <= 1, `Retângulo inválido ${c.id}`);
  }
  for (const claim of data.claims) assert(typeof claim.true === 'boolean' && typeof claim.text === 'string', `Afirmação inválida ${claim.id}`);
  for (const e of data.evidence) {
    assert(['observation', 'testimony'].includes(e.nature), `Natureza inválida ${e.id}`);
    ref('origins', e.origin_id);
    refs('claims', e.claimIds);
    assert(e.claimIds.length > 0, `Evidência sem afirmação ${e.id}`);
    assert(e.source && ['person','document','place','object'].includes(e.source.kind) && typeof e.source.id === 'string' && e.source.label, `Fonte inválida ${e.id}`);
    if (e.source.kind === 'person') ref('characters', e.source.id);
    assert(e.nature !== 'testimony' || e.source.kind === 'person', `Depoimento sem pessoa ${e.id}`);
    assert(e.nature !== 'observation' || e.source.kind !== 'person', `Observação mal atribuída ${e.id}`);
  }
  for (const interaction of data.interactions) {
    assert(Boolean(interaction.characterId) !== Boolean(interaction.point), `Interação precisa de pessoa ou ponto ${interaction.id}`);
    if (interaction.characterId) ref('characters', interaction.characterId);
    if (interaction.point) position(interaction.point, interaction.id);
    if (interaction.responseToEvidenceId) ref('evidence', interaction.responseToEvidenceId);
    condition(interaction.condition);
    assert(Array.isArray(interaction.lines) && interaction.lines.length > 0, `Falas vazias ${interaction.id}`);
    for (const line of interaction.lines) {
      assert(typeof line.text === 'string' && line.text.length > 0 && typeof line.speaker === 'string', `Fala inválida ${interaction.id}`);
      const effects = line.effects ?? {};
      Object.keys(effects).forEach(key => assert(['evidence','setMarkers','clearMarkers'].includes(key), `Efeito desconhecido ${key}`));
      refs('evidence', list(effects.evidence));
      markerRefs(list(effects.setMarkers)); markerRefs(list(effects.clearMarkers));
      for (const eid of list(effects.evidence)) {
        const e = data.evidence.find(item => item.id === eid);
        if (e.nature === 'testimony') assert(e.source.id === interaction.characterId, `Fonte mal atribuída ${eid} em ${interaction.id}`);
        else assert(interaction.point || interaction.characterId, `Observação sem acesso ${eid}`);
      }
    }
  }
  const grantedEvidence = new Set(data.interactions.flatMap(i => i.lines.flatMap(line => line.effects?.evidence ?? [])));
  for (const e of data.evidence) assert(grantedEvidence.has(e.id), `Evidência sem aquisição: ${e.id}`);
  for (const h of data.hypotheses) {
    assert(Array.isArray(h.alternatives), `Alternativas ausentes ${h.id}`);
    refs('evidence', list(h.contradictions));
    for (const alt of h.alternatives) {
      refs('evidence', alt.evidenceIds);
      assert(alt.evidenceIds.length >= 2 && new Set(alt.evidenceIds).size === alt.evidenceIds.length, `Regra vazia ou repetida ${h.id}`);
      assert(Number.isInteger(alt.minOrigins) && alt.minOrigins >= 2, `Origens insuficientes ${h.id}`);
      const origins = new Set(alt.evidenceIds.map(id => data.evidence.find(e => e.id === id).origin_id));
      assert(origins.size >= alt.minOrigins, `Origens compartilhadas em ${h.id}`);
    }
  }
  assert(data.hypotheses.some(h => h.alternatives.length), 'Nenhuma hipótese sustentada');
  for (const scenario of data.scenarios) {
    refs('characters', scenario.lostSources);
    assert(Array.isArray(scenario.prepareActions), `Preparação inválida ${scenario.id}`);
    assert(typeof scenario.opening === 'string' && scenario.opening.length > 0, `Abertura ausente ${scenario.id}`);
    for (const action of scenario.prepareActions) {
      assert(['talk','present','advance','examine'].includes(action.type), `Ação prévia inválida ${scenario.id}`);
      if (action.interactionId) ref('interactions', action.interactionId);
      if (action.evidenceId) ref('evidence', action.evidenceId);
    }
  }
  const assetIds = new Set();
  for (const asset of list(data.assets)) {
    assert(!assetIds.has(asset.id), `Asset duplicado ${asset.id}`);
    assetIds.add(asset.id);
    assert(typeof asset.path === 'string' && /^assets\/[a-zA-Z0-9._/-]+$/.test(asset.path) && !asset.path.includes('..'), `Caminho de asset inválido ${asset.id}`);
  }
  for (const c of data.characters) assert(assetIds.has(c.portrait.asset), `Asset do retrato não existe: ${c.id}`);
  return data;
}

export async function loadCase(url = 'data/case.json') {
  const response = await fetch(url);
  if (!response.ok) throw new Error(`Não foi possível carregar o caso (${response.status})`);
  return validateCase(await response.json());
}
