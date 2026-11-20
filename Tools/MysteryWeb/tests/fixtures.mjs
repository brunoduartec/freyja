export function fixture() {
  const chars = ['artur','teresa','ivo'];
  return {
    id:'fixture',version:1,title:'Caso de teste',introduction:'Chegada',question:'Por quê?',
    characters:chars.map(id=>({id,name:id,position:{x:.5,y:.5},portrait:{asset:'sheet',rect:{x:0,y:0,width:.1,height:.1}}})),
    origins:[{id:'origin_a'},{id:'origin_b'},{id:'rumor'},{id:'document'}],
    claims:[{id:'claim_a',text:'Uma ordem foi dada',true:true},{id:'claim_false',text:'A partida é voluntária',true:false}],
    evidence:[
      {id:'report',origin_id:'origin_a',source:{kind:'person',id:'ivo',label:'Ivo'},nature:'testimony',claimIds:['claim_a']},
      {id:'record',origin_id:'origin_b',source:{kind:'document',id:'ledger',label:'Caderno'},nature:'observation',claimIds:['claim_a']},
      {id:'rumor_a',origin_id:'rumor',source:{kind:'person',id:'artur',label:'Artur'},nature:'testimony',claimIds:['claim_false']},
      {id:'rumor_b',origin_id:'rumor',source:{kind:'person',id:'teresa',label:'Teresa'},nature:'testimony',claimIds:['claim_false']},
      {id:'false_record',origin_id:'document',source:{kind:'document',id:'notice',label:'Aviso'},nature:'observation',claimIds:['claim_false']},
      {id:'alternative',origin_id:'origin_a',source:{kind:'person',id:'teresa',label:'Teresa'},nature:'testimony',claimIds:['claim_a']}
    ],markers:['trust','accused'],
    interactions:[
      {id:'hear_ivo',title:'Ouvir Ivo',characterId:'ivo',condition:{},lines:[{speaker:'Ivo',text:'Não quero partir.'},{speaker:'Ivo',text:'A vila me escolheu.',effects:{evidence:['report']}}]},
      {id:'inspect',title:'Examinar',point:{x:.2,y:.3},condition:{},lines:[{speaker:'Caderno',text:'Uma partida ordenada.',effects:{evidence:['record','false_record']}}]},
      {id:'trust',title:'Ouvir com cuidado',characterId:'teresa',condition:{notMarkers:['accused']},lines:[{speaker:'Teresa',text:'Eu vi a escolha.',effects:{evidence:['alternative'],setMarkers:['trust']}}]},
      {id:'accuse',title:'Acusar',characterId:'teresa',condition:{notMarkers:['trust']},lines:[{speaker:'Teresa',text:'Não vou tratar disso.',effects:{setMarkers:['accused']}}]},
      {id:'rumor_artur',title:'Ouvir versão pública',characterId:'artur',condition:{},lines:[{speaker:'Artur',text:'Ouvi o boato.',effects:{evidence:['rumor_a']}}]},
      {id:'rumor_teresa',title:'Ouvir boato',characterId:'teresa',condition:{},lines:[{speaker:'Teresa',text:'Repetiram isso para mim.',effects:{evidence:['rumor_b']}}]},
      {id:'ask_record',title:'Apresentar registro',characterId:'ivo',responseToEvidenceId:'record',condition:{},lines:[{speaker:'Ivo',text:'Esse é o registro.',effects:{evidence:['report']}}]}
    ],
    hypotheses:[{id:'imposed',text:'Foi imposto',alternatives:[{evidenceIds:['report','record'],minOrigins:2},{evidenceIds:['alternative','record'],minOrigins:2}],contradictions:[],conclusion:'Fim da amostra'},
      {id:'voluntary',text:'Foi voluntário',alternatives:[],contradictions:['report']}],
    scenarios:[{id:'arrival',title:'Chegada',opening:'Chegada',prepareActions:[],lostSources:[]}],assets:[{id:'sheet',path:'assets/characters-sheet.png'}]
  };
}
