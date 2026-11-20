# Tarefas

- [x] T001 Selecionar a base com a cena e os scripts atuais, conforme plan.md.
- [x] T002 [US1] Separar intenção planar e deslocamento real em CaisPlayerController.cs.
- [x] T003 [US1] Liberar a passada lateralmente e reconstruir âncoras ao retornar em StairFootPlacement.cs.
- [x] T004 [US2] Preservar os apoios na pausa voluntária em StairFootPlacement.cs.
- [x] T005 Validar compilação e regressões em Play Mode conforme quickstart.md.

Evidência de T005 em [evidence/play-mode.md](evidence/play-mode.md). A reprodução anterior à correção permaneceu limitada à captura do relato.

## Revisão do controle longitudinal

- [x] T006 [US1] Filtrar o movimento apoiado em degrau e orientar o personagem pela velocidade filtrada em StairFootPlacement.cs e CaisPlayerController.cs.
- [x] T007 Validar os cenários revisados da spec.md nos lances L3 e B3.

Evidência da revisão em [evidence/controle-longitudinal.md](evidence/controle-longitudinal.md).

## Regressão na entrada da subida

- [x] T008 Atualizar a referência lateral dos apoios quando o corpo muda de faixa entre a aproximação e os degraus, em StairFootPlacement.cs.
- [x] T009 Verificar a subida com entrada em diagonal e direção lateral bloqueada em Play Mode.

Evidência de T009 em [evidence/alinhamento-apoios.md](evidence/alinhamento-apoios.md).
