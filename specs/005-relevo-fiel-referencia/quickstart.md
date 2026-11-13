# Guia de validação — Spec 005

Este guia descreve a evidência esperada. Não executa testes. A primeira etapa termina na avaliação visual da maquete; aguarde esse retorno antes de prosseguir à integração final.

## Pré-requisitos

- Projeto Unity em `Freyja/`, editor 6000.3.25f1 e Built-in conforme configuração atual.
- Blender 5.2.2 LTS e Unity conectados pelos MCPs globais; reconfirmar versão, projeto, cena e alterações não salvas antes de operar.
- Conjunto de seis referências mandatórias: `Referencias/ilha-vista-topdown-004.png`, `Referencias/modelagem-vista-aerea.png` e os quatro perfis em `Referencias/ilha-vistas-laterais/`.

## Checkpoint visual — primeira maquete

1. Salvar a fonte editável independente `Source/Blender/Ilha_Relevo_005.blend`.
2. Renderizar vistas reais da maquete de cima, perspectiva e lados norte, sul, leste e oeste, alinhadas a cada imagem correspondente.
3. Comparar todas as seis imagens com as vistas correspondentes, lado a lado e por sobreposição; registrar divergências entre referências e entre malha e imagens.
4. Registrar as inferências da face posterior, escala e diferenças remanescentes em `evidence/comparacao.md`.
5. Apresentar a maquete para avaliação do usuário. A etapa seguinte requer esse retorno explícito.

## Validação depois da avaliação

1. Exportar `Ilha_Relevo_005.fbx`, integrar `IlhaRelevo005.prefab` na cena isolada `Ilha_Relevo_005.unity` e confirmar a junção com cais e spawn.
2. Na superfície de colisão importada, medir os ângulos de todos os triângulos e separar `<45°`, `45–70°`, `70–80°` e `>80°`. Registrar degenerações, gaps, limites do scan e método.
3. Analisar apoio e conectividade considerando raio 0,3 m, altura 1,8 m e step 0,25 m, após confirmar esses valores na cena. Explicitar que a grade/capsule cast é amostragem.
4. Em Play Mode, percorrer cais → largo → setores B/A/L → horta/ruína → recanto e retornar. Testar também áreas livres fora dos caminhos, rampas e bordas de paredões; confirmar que faces íngremes barram o jogador.
5. Testar câmera, colisões do cais, queda na água e recuperação. Anotar falha por falha, distinguir controle físico de entrada sintética e registrar capturas.
6. Verificar console, referências, assets, hashes da linha de base e estado de salvamento. Não declarar aprovação visual sem avaliação explícita.

## Evidências esperadas

- `evidence/comparacao.md` e capturas aérea, sobreposta e de perfil.
- Registro de ambiente e hashes.
- Relatório de inclinação/topologia e configuração da análise.
- Registro do percurso Play Mode com cobertura, tempos, falhas e limitações.
- Capturas Unity na cena e câmera do jogador; estado final de preservação dos ativos 003/004.
