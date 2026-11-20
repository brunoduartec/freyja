export function meetsCondition(state, condition = {}) {
  return (condition.allEvidence ?? []).every(id => state.knownEvidence.includes(id))
    && (!condition.anyEvidence || condition.anyEvidence.some(id => state.knownEvidence.includes(id)))
    && (condition.alive ?? []).every(id => state.aliveSources.includes(id))
    && (condition.markers ?? []).every(id => state.markers.includes(id))
    && (condition.notMarkers ?? []).every(id => !state.markers.includes(id));
}

export function isInteractionAvailable(data, state, interaction) {
  return Boolean(interaction) && !state.completed
    && (!interaction.characterId || state.aliveSources.includes(interaction.characterId))
    && (!interaction.responseToEvidenceId || state.knownEvidence.includes(interaction.responseToEvidenceId))
    && meetsCondition(state, interaction.condition);
}

export function canPropose(data, state, hypothesisId, evidenceIds = []) {
  const hypothesis = data.hypotheses.find(h => h.id === hypothesisId);
  if (!hypothesis || !Array.isArray(evidenceIds) || !evidenceIds.every(id => state.knownEvidence.includes(id))) return false;
  if ((hypothesis.contradictions ?? []).some(id => state.knownEvidence.includes(id))) return false;
  return hypothesis.alternatives.some(alt => alt.evidenceIds.every(id => evidenceIds.includes(id))
    && new Set(alt.evidenceIds.map(id => data.evidence.find(e => e.id === id)?.origin_id)).size >= alt.minOrigins);
}

function applyEffects(state, effects = {}, interaction) {
  for (const id of effects.evidence ?? []) {
    if (!state.knownEvidence.includes(id)) {
      state.knownEvidence.push(id);
      state.history.push({type:'acquire', evidenceId:id, interactionId:interaction.id, sequence:state.history.length + 1});
    }
  }
  state.markers = state.markers.filter(id => !(effects.clearMarkers ?? []).includes(id));
  for (const id of effects.setMarkers ?? []) if (!state.markers.includes(id)) state.markers.push(id);
}

export function applyAction(data, current, action) {
  const state = structuredClone(current);
  const fail = feedback => ({state:current, ok:false, feedback});
  if (!action || typeof action.type !== 'string') return fail('Ação inválida.');
  if (action.type === 'loseSource') {
    if (!data.characters.some(c => c.id === action.characterId)) return fail('Fonte desconhecida.');
    state.aliveSources = state.aliveSources.filter(id => id !== action.characterId);
    const active = data.interactions.find(i => i.id === state.dialogue?.interactionId);
    if (active?.characterId === action.characterId) state.dialogue = null;
  } else if (state.completed) return fail('A amostra terminou. Reinicie para investigar novamente.');
  else if (action.type === 'closeDialogue') state.dialogue = null;
  else if (action.type === 'advance') {
    if (!state.dialogue) return fail('Nenhuma conversa em andamento.');
    const interaction = data.interactions.find(i => i.id === state.dialogue.interactionId);
    if (interaction.characterId && !state.aliveSources.includes(interaction.characterId)) return fail('Esta fonte está indisponível.');
    const next = state.dialogue.lineIndex + 1;
    if (next >= interaction.lines.length) state.dialogue = null;
    else {
      state.dialogue.lineIndex = next;
      applyEffects(state, interaction.lines[next].effects, interaction);
    }
  } else if (['talk','present','examine'].includes(action.type)) {
    const interaction = data.interactions.find(i => i.id === action.interactionId);
    if (state.dialogue || !isInteractionAvailable(data,state,interaction)) return fail('Este assunto ainda não está disponível.');
    if (action.type === 'examine') {
      if (!interaction.point) return fail('Este ponto não pode ser examinado.');
      interaction.lines.forEach(line => applyEffects(state,line.effects,interaction));
    } else {
      if (!interaction.characterId) return fail('Interlocutor inválido.');
      if (interaction.responseToEvidenceId) {
        if (action.type !== 'present' || action.evidenceId !== interaction.responseToEvidenceId || !state.knownEvidence.includes(action.evidenceId)) return fail('Apresente uma evidência conhecida.');
      } else if (action.type === 'present') return fail('Não há resposta prevista para essa evidência.');
      state.dialogue = {interactionId:interaction.id,lineIndex:-1};
    }
  } else if (action.type === 'propose') {
    if (state.dialogue) return fail('Conclua a conversa antes de propor uma explicação.');
    const accepted = canPropose(data,state,action.hypothesisId,action.evidenceIds);
    state.proposals.push({hypothesisId:action.hypothesisId,evidenceIds:[...(action.evidenceIds ?? [])],accepted});
    state.history.push({...action,accepted,sequence:state.history.length + 1});
    if (!accepted) return {state,ok:false,feedback:'A explicação ainda não se sustenta nas fontes escolhidas. Compare o que cada uma realmente afirma.'};
    state.completed = true;
    state.acceptedHypothesisId = action.hypothesisId;
    return {state,ok:true,feedback:'Explicação sustentada. Fim desta amostra de investigação.'};
  } else return fail('Ação desconhecida.');
  state.history.push({...action,sequence:state.history.length + 1});
  return {state,ok:true,feedback:''};
}

export function createSession(data, scenarioId = data.scenarios[0].id) {
  const scenario = data.scenarios.find(s => s.id === scenarioId);
  if (!scenario) throw new Error(`Cenário desconhecido: ${scenarioId}`);
  let state = {scenarioId,knownEvidence:[],aliveSources:data.characters.map(c=>c.id),markers:[],dialogue:null,history:[],proposals:[],completed:false};
  for (const action of scenario.prepareActions) {
    const result = applyAction(data,state,action);
    if (!result.ok) throw new Error(`Preparação inválida em ${scenarioId}: ${result.feedback}`);
    state = result.state;
  }
  if (state.dialogue) throw new Error(`Preparação deixa conversa aberta: ${scenarioId}`);
  for (const characterId of scenario.lostSources) state = applyAction(data,state,{type:'loseSource',characterId}).state;
  return state;
}

export function availableActions(data, state) {
  if (state.completed) return [];
  if (state.dialogue) return [{type:'advance'}, {type:'closeDialogue'}];
  const actions = data.interactions.filter(i => isInteractionAvailable(data,state,i)).map(i => i.point
    ? {type:'examine',interactionId:i.id}
    : i.responseToEvidenceId ? {type:'present',interactionId:i.id,evidenceId:i.responseToEvidenceId}
      : {type:'talk',interactionId:i.id});
  for (const h of data.hypotheses) for (const alt of h.alternatives) {
    if (canPropose(data,state,h.id,alt.evidenceIds)) actions.push({type:'propose',hypothesisId:h.id,evidenceIds:alt.evidenceIds});
  }
  return actions;
}
