import test from 'node:test';
import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import {validateCase} from '../src/case-loader.mjs';
import {fixture} from './fixtures.mjs';

test('caso entregue é estruturalmente válido', async()=>{
  const data = JSON.parse(await readFile(new URL('../data/case.json',import.meta.url),'utf8'));
  assert.equal(validateCase(data),data);
});
test('referência inexistente e ID duplicado falham com contexto',()=>{
  let data=fixture(); data.interactions[0].lines[0].effects={evidence:['missing']};
  assert.throws(()=>validateCase(data),/referência inválida missing/);
  data=fixture(); data.evidence[1].id='report'; assert.throws(()=>validateCase(data),/duplicado/);
});
test('origem compartilhada não satisfaz independência e regra vazia falha',()=>{
  let data=fixture(); data.hypotheses[0].alternatives=[{evidenceIds:['rumor_a','rumor_b'],minOrigins:2}];
  assert.throws(()=>validateCase(data),/Origens compartilhadas/);
  data=fixture(); data.hypotheses[0].alternatives=[{evidenceIds:[],minOrigins:2}];
  assert.throws(()=>validateCase(data),/Regra vazia/);
});
test('depoimento não pode ser concedido em fala de outra pessoa',()=>{
  const data=fixture(); data.interactions[0].characterId='artur';
  assert.throws(()=>validateCase(data),/Fonte mal atribuída/);
});
test('gramática desconhecida, retrato fora da folha e caminho externo são rejeitados',()=>{
  let data=fixture(); data.interactions[0].condition={expression:'eval(1)'};
  assert.throws(()=>validateCase(data),/Condição desconhecida/);
  data=fixture(); data.characters[0].portrait.rect.x=.99;
  assert.throws(()=>validateCase(data),/Retângulo inválido/);
  data=fixture(); data.assets=[{id:'map',path:'../private.png'}];
  assert.throws(()=>validateCase(data),/Caminho de asset inválido/);
});
test('documento falso é permitido como afirmação registrada, sem mudar a verdade autoral',()=>{
  const data=fixture(); validateCase(data);
  assert.equal(data.claims.find(c=>c.id==='claim_false').true,false);
  assert.equal(data.evidence.find(e=>e.id==='false_record').nature,'observation');
});
test('evidência autoral não pode ficar sem ação de aquisição',()=>{
  const data=fixture(); data.interactions=data.interactions.filter(i=>i.id!=='rumor_artur');
  assert.throws(()=>validateCase(data),/Evidência sem aquisição: rumor_a/);
});
