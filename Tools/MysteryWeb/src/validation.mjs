import { applyAction, availableActions, canPropose, createSession } from './investigation.mjs';

function canonicalState(state) {
  const sorted = (items = []) => [...items].sort();
  return JSON.stringify({
    knownEvidence: sorted(state.knownEvidence),
    aliveSources: sorted(state.aliveSources),
    markers: sorted(state.markers),
    dialogue: state.dialogue ? [state.dialogue.interactionId, state.dialogue.lineIndex] : null,
    completed: Boolean(state.completed),
  });
}

function reconstructPath(key, predecessors) {
  const path = [];
  let cursor = key;
  while (predecessors.get(cursor)?.previous !== null) {
    const entry = predecessors.get(cursor);
    path.push(entry.action);
    cursor = entry.previous;
  }
  return path.reverse();
}

function originIds(data, evidenceIds) {
  return [...new Set(evidenceIds.map((id) => data.evidence.find((item) => item.id === id)?.origin_id).filter(Boolean))].sort();
}

function blockedRequirements(data, states) {
  const reachable = [...states];
  return data.hypotheses.flatMap((hypothesis) => {
    const alternatives = hypothesis.alternatives.map((alternative) => {
      const candidates = reachable.map((state) => {
        const matched = alternative.evidenceIds.filter((id) => state.knownEvidence.includes(id));
        const missingEvidence = alternative.evidenceIds.filter((id) => !state.knownEvidence.includes(id));
        const knownOrigins = originIds(data, matched);
        return {
          hypothesisId: hypothesis.id,
          missingEvidence,
          missingOrigins: Math.max(0, alternative.minOrigins - knownOrigins.length),
          knownContradictions: (hypothesis.contradictions ?? []).filter((id) => state.knownEvidence.includes(id)),
          matchedCount: matched.length,
        };
      });
      return candidates.sort((a, b) => a.missingEvidence.length - b.missingEvidence.length || a.missingOrigins - b.missingOrigins || a.knownContradictions.length - b.knownContradictions.length || b.matchedCount - a.matchedCount)[0]
        ?? { hypothesisId: hypothesis.id, missingEvidence: [...alternative.evidenceIds], missingOrigins: alternative.minOrigins, knownContradictions: [] };
    });
    return alternatives.filter((item) => item.missingEvidence.length || item.missingOrigins || item.knownContradictions.length)
      .map((item) => `${item.hypothesisId}: evidências pendentes [${item.missingEvidence.join(', ') || 'nenhuma'}], origens independentes pendentes ${item.missingOrigins}${item.knownContradictions.length ? `, contradições [${item.knownContradictions.join(', ')}]` : ''}`);
  });
}

/** Search all reachable UI states and return one concrete supported proposal, if any. */
export function findSolution(data, scenarioId = data.scenarios?.[0]?.id, options = {}) {
  const initialState = options.initialState ?? createSession(data, scenarioId);
  const initialKey = canonicalState(initialState);
  const queue = [initialState];
  const stateByKey = new Map([[initialKey, initialState]]);
  const predecessors = new Map([[initialKey, { previous: null, action: null }]]);
  let head = 0;

  while (head < queue.length) {
    const state = queue[head++];
    const key = canonicalState(state);
    const actions = availableActions(data, state);
    for (const action of actions) {
      if (action.type === 'propose' && canPropose(data, state, action.hypothesisId, action.evidenceIds)) {
        const path = reconstructPath(key, predecessors);
        const result = applyAction(data, state, action);
        if (result.ok) {
          path.push(action);
          return {
            solved: true,
            path,
            evidenceIds: [...action.evidenceIds].sort(),
            origins: originIds(data, action.evidenceIds),
            blocked: [],
            visited: stateByKey.size,
          };
        }
        continue;
      }
      const result = applyAction(data, state, action);
      if (!result.ok) continue;
      const nextKey = canonicalState(result.state);
      if (stateByKey.has(nextKey)) continue;
      stateByKey.set(nextKey, result.state);
      predecessors.set(nextKey, { previous: key, action });
      queue.push(result.state);
    }
  }

  return {
    solved: false,
    path: [],
    evidenceIds: [],
    origins: [],
    blocked: blockedRequirements(data, stateByKey.values()),
    visited: stateByKey.size,
  };
}

function combinations(values, maximum) {
  const result = [[]];
  for (const value of values) {
    const additions = result.filter((set) => set.length < maximum).map((set) => [...set, value]);
    result.push(...additions);
  }
  return result;
}

/** Validate every loss combination up to maxLosses, plus authored preparation scenarios. */
export function validateCasePaths(data, { maxLosses = 2 } = {}) {
  const baseScenarioId = data.scenarios?.[0]?.id;
  if (!baseScenarioId) throw new Error('O caso precisa declarar ao menos um cenário.');
  const sourceIds = (data.characters ?? []).map((character) => character.id).sort();
  const lossSets = combinations(sourceIds, Math.min(maxLosses, sourceIds.length));
  const scenarios = lossSets.map((lostSources) => {
    const scenario = data.scenarios.find((item) =>
      !(item.prepareActions?.length) &&
      [...(item.lostSources ?? [])].sort().join('\0') === [...lostSources].sort().join('\0'));
    const scenarioId = scenario?.id ?? baseScenarioId;
    const initial = createSession(data, scenarioId);
    let state = initial;
    if (!scenario) {
      for (const characterId of lostSources) {
        if (!state.aliveSources.includes(characterId)) continue;
        const result = applyAction(data, state, { type: 'loseSource', characterId });
        if (!result.ok) throw new Error(`Falha ao remover fonte ${characterId}: ${result.feedback}`);
        state = result.state;
      }
    }
    const result = findSolution(data, scenarioId, { initialState: state });
    return {
      scenarioId,
      title: scenario?.title ?? (lostSources.length ? `Teste de perda: ${lostSources.join(', ')}` : data.scenarios[0].title),
      opening: scenario?.opening ?? data.scenarios[0].opening,
      lostSources,
      result,
    };
  });

  const authoredScenarios = data.scenarios.slice(1).filter((scenario) => scenario.prepareActions?.length).map((scenario) => {
    const result = findSolution(data, scenario.id);
    return { scenarioId: scenario.id, title: scenario.title, opening: scenario.opening, lostSources: [...(scenario.lostSources ?? [])], prepared: true, result };
  });
  const all = [...scenarios, ...authoredScenarios];
  return {
    valid: all.every((item) => item.result.solved),
    maxLosses,
    sourceIds,
    scenarios,
    authoredScenarios,
    failures: all.filter((item) => !item.result.solved),
  };
}
