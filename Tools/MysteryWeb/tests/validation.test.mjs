import test from 'node:test';
import assert from 'node:assert/strict';
import { applyAction, createSession } from '../src/investigation.mjs';
import { findSolution, validateCasePaths } from '../src/validation.mjs';

const ev = (id, origin_id = id) => ({ id, title: id, description: id, nature: 'testimony', source: { kind: 'character', id: 'witness', label: 'Witness' }, origin_id, claimIds: [] });
const line = (evidence) => [{ speaker: 'Witness', text: 'Record', effects: { evidence: [evidence] } }];

function caseData({ interactions = [], evidence = [], alternatives = [], scenarios = [{ id: 'arrival', title: 'Arrival', opening: '', prepareActions: [], lostSources: [] }] } = {}) {
  return {
    id: 'test-case', title: 'Test', version: 1,
    characters: [{ id: 'witness', name: 'Witness' }, { id: 'other-a', name: 'Other A' }, { id: 'other-b', name: 'Other B' }],
    evidence,
    hypotheses: [{ id: 'departure', text: 'The departure was imposed.', alternatives, contradictions: [], conclusion: 'Supported.' }],
    interactions, scenarios,
  };
}

const interaction = (id, evidenceId, condition = {}) => ({ id, title: id, characterId: 'witness', condition, lines: line(evidenceId) });
const alternative = (evidenceIds, minOrigins = evidenceIds.length) => ({ evidenceIds, minOrigins });

test('reports a cycle with no seed evidence as unsolved', () => {
  const data = caseData({
    evidence: [ev('a'), ev('b')],
    interactions: [interaction('get-a', 'a', { allEvidence: ['b'] }), interaction('get-b', 'b', { allEvidence: ['a'] })],
    alternatives: [alternative(['a', 'b'], 2)],
  });
  const result = findSolution(data, 'arrival');
  assert.equal(result.solved, false);
  assert.ok(result.blocked.some((item) => item.includes('a') || item.includes('b')));
  assert.equal(validateCasePaths(data).valid, false);
});

test('does not combine evidence from mutually exclusive routes', () => {
  const data = caseData({
    evidence: [ev('left'), ev('right')],
    interactions: [
      { id: 'choose-left', title: 'Left', point: { x: 0, y: 0, label: 'Left' }, condition: { notMarkers: ['right-route'] }, lines: [{ speaker: 'N', text: 'Left route', effects: { evidence: ['left'], setMarkers: ['left-route'] } }] },
      { id: 'choose-right', title: 'Right', point: { x: 1, y: 0, label: 'Right' }, condition: { notMarkers: ['left-route'] }, lines: [{ speaker: 'N', text: 'Right route', effects: { evidence: ['right'], setMarkers: ['right-route'] } }] },
    ],
    alternatives: [alternative(['left', 'right'], 2)],
  });
  assert.equal(findSolution(data, 'arrival').solved, false);
});

test('finds an explicitly unsolvable case and reports blocked requirements', () => {
  const data = caseData({ evidence: [ev('unavailable')], alternatives: [alternative(['unavailable'], 1)] });
  const result = findSolution(data, 'arrival');
  assert.equal(result.solved, false);
  assert.ok(result.blocked.length > 0);
});

test('loss before acquisition blocks a source; loss after acquisition preserves its evidence', () => {
  const data = caseData({
    evidence: [ev('memory')],
    interactions: [interaction('witness-story', 'memory')],
    alternatives: [alternative(['memory'], 1)],
  });
  const initial = createSession(data, 'arrival');
  const before = applyAction(data, initial, { type: 'loseSource', characterId: 'witness' }).state;
  assert.equal(findSolution(data, 'arrival', { initialState: before }).solved, false);

  const talk = applyAction(data, initial, { type: 'talk', interactionId: 'witness-story' }).state;
  const acquired = applyAction(data, talk, { type: 'advance', interactionId: 'witness-story' }).state;
  const after = applyAction(data, acquired, { type: 'loseSource', characterId: 'witness' }).state;
  assert.ok(after.knownEvidence.includes('memory'));
  assert.equal(findSolution(data, 'arrival', { initialState: after }).solved, true);
});

test('accepts either independently sourced support alternative', () => {
  const data = caseData({
    evidence: [ev('a', 'origin-a'), ev('b', 'origin-b'), ev('c', 'origin-c'), ev('d', 'origin-d')],
    interactions: [interaction('one', 'a'), interaction('two', 'b'), interaction('three', 'c'), interaction('four', 'd')],
    alternatives: [alternative(['a', 'b'], 2), alternative(['c', 'd'], 2)],
  });
  const result = findSolution(data, 'arrival');
  assert.equal(result.solved, true);
  assert.ok([['a', 'b'], ['c', 'd']].some((ids) => ids.every((id) => result.evidenceIds.includes(id))));
  assert.equal(result.origins.length, 2);
});

test('validates all seven zero, one, and two source loss configurations', () => {
  const characters = ['one', 'two', 'three'].map((id) => ({ id, name: id }));
  const evidence = characters.map((character) => ev(`e-${character.id}`, `origin-${character.id}`));
  const interactions = characters.map((character) => ({
    id: `talk-${character.id}`, title: character.id, characterId: character.id, condition: {},
    lines: [{ speaker: character.id, text: 'Statement', effects: { evidence: [`e-${character.id}`] } }],
  }));
  const data = caseData({
    evidence,
    interactions,
    alternatives: evidence.map((item) => alternative([item.id], 1)),
  });
  data.characters = characters;
  const lossSets = [[], ['one'], ['two'], ['three'], ['one', 'two'], ['one', 'three'], ['two', 'three']];
  data.scenarios = lossSets.map((lostSources) => ({
    id: `loss-${lostSources.join('-') || 'none'}`,
    title: lostSources.length ? `Unavailable: ${lostSources.join(', ')}` : 'Arrival',
    opening: lostSources.length ? `Unavailable ${lostSources.join(', ')}` : 'Everyone is available.',
    prepareActions: [],
    lostSources,
  }));
  const result = validateCasePaths(data, { maxLosses: 2 });
  assert.equal(result.scenarios.length, 7);
  assert.equal(result.valid, true);
  for (const scenario of result.scenarios) {
    const key = [...scenario.lostSources].sort().join(',');
    const expected = data.scenarios.find((item) => [...item.lostSources].sort().join(',') === key);
    assert.equal(scenario.scenarioId, expected.id);
    assert.equal(scenario.title, expected.title);
    assert.equal(scenario.opening, expected.opening);
  }
});

test('validates a prepared memory scenario after the source is lost', () => {
  const data = caseData({
    evidence: [ev('memory')],
    interactions: [interaction('witness-story', 'memory')],
    alternatives: [alternative(['memory'], 1)],
    scenarios: [
      { id: 'arrival', title: 'Arrival', opening: '', prepareActions: [], lostSources: [] },
      { id: 'memory-review', title: 'Memory after loss', opening: '', prepareActions: [
        { type: 'talk', interactionId: 'witness-story' }, { type: 'advance' }, { type: 'advance' },
      ], lostSources: ['witness'] },
    ],
  });
  const state = createSession(data, 'memory-review');
  assert.deepEqual(state.knownEvidence, ['memory']);
  assert.equal(state.aliveSources.includes('witness'), false);
  assert.equal(findSolution(data, 'memory-review').solved, true);
});
