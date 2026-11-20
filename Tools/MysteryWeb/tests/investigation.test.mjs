import test from 'node:test';
import assert from 'node:assert/strict';
import {fixture} from './fixtures.mjs';
import {createSession,applyAction,availableActions,canPropose,meetsCondition} from '../src/investigation.mjs';
const perform=(data,state,action)=>{const result=applyAction(data,state,action);assert.equal(result.ok,true,result.feedback);return result.state;};
const conversation=(data,state,id)=>{
  state=perform(data,state,{type:'talk',interactionId:id});
  while(state.dialogue) state=perform(data,state,{type:'advance'});
  return state;
};
test('E/OU respeita os requisitos de cada alternativa, sem aceitar mera contagem',()=>{
  const data=fixture(), state=createSession(data);
  assert.equal(meetsCondition(state,{anyEvidence:['record','report']}),false);
  state.knownEvidence=['record']; assert.equal(meetsCondition(state,{anyEvidence:['record','report']}),true);
  assert.equal(meetsCondition(state,{allEvidence:['record','report']}),false);
  assert.equal(canPropose(data,state,'imposed',['record']),false);
  state.knownEvidence.push('alternative'); assert.equal(canPropose(data,state,'imposed',state.knownEvidence),true);
});
test('falas só concedem evidência ao alcançar sua posição; apresentação exige item conhecido',()=>{
  const data=fixture(); let state=createSession(data);
  assert.equal(applyAction(data,state,{type:'present',interactionId:'ask_record',evidenceId:'record'}).ok,false);
  state=perform(data,state,{type:'talk',interactionId:'hear_ivo'});
  assert.deepEqual(state.knownEvidence,[]);
  state=perform(data,state,{type:'advance'}); assert.deepEqual(state.knownEvidence,[]);
  state=perform(data,state,{type:'advance'}); assert.deepEqual(state.knownEvidence,['report']);
});
test('conversa→objeto e objeto→conversa preservam a base factual; repetição é idempotente',()=>{
  const data=fixture();
  let a=conversation(data,createSession(data),'hear_ivo');
  a=perform(data,a,{type:'examine',interactionId:'inspect'});
  let b=perform(data,createSession(data),{type:'examine',interactionId:'inspect'});
  b=conversation(data,b,'hear_ivo');
  assert.deepEqual([...a.knownEvidence].sort(),[...b.knownEvidence].sort());
  const count=b.history.filter(a=>a.type==='acquire').length;
  b=conversation(data,b,'hear_ivo');b=perform(data,b,{type:'examine',interactionId:'inspect'});
  assert.equal(b.history.filter(a=>a.type==='acquire').length,count);
  assert.equal(b.completed,false);
  b=perform(data,b,{type:'propose',hypothesisId:'imposed',evidenceIds:['report','record']}); assert.equal(b.completed,true);
});
test('boatos repetidos, documento falso e hipótese sem base não viram prova de si mesmos',()=>{
  const data=fixture(), state=createSession(data);
  state.knownEvidence=['rumor_a','rumor_b','false_record'];
  data.hypotheses[0].alternatives=[{evidenceIds:['rumor_a','rumor_b'],minOrigins:2}];
  assert.equal(canPropose(data,state,'imposed',state.knownEvidence),false);
  const rejected=applyAction(data,state,{type:'propose',hypothesisId:'voluntary',evidenceIds:['false_record']});
  assert.equal(rejected.ok,false); assert.deepEqual(rejected.state.knownEvidence,state.knownEvidence);
  assert.equal(rejected.state.completed,false);
  assert.equal(canPropose(data,state,'imposed',['report','record']),false);
});
test('escolha fecha um assunto; alternativa permanece acessível e reinício limpa tudo',()=>{
  const data=fixture(); let state=conversation(data,createSession(data),'accuse');
  assert.equal(availableActions(data,state).some(a=>a.interactionId==='trust'),false);
  state=conversation(data,state,'hear_ivo');state=perform(data,state,{type:'examine',interactionId:'inspect'});
  assert.equal(canPropose(data,state,'imposed',state.knownEvidence),true);
  const clean=createSession(data);assert.deepEqual(clean.knownEvidence,[]);assert.deepEqual(clean.markers,[]);assert.deepEqual(clean.history,[]);
});
