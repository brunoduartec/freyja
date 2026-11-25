# Decisões técnicas

- **Rampa estática local.** O visual já está separado. A cunha precisa só de faces superiores e laterais de uma malha de colisão simples. As caixas inclinadas deixariam a extremidade superior ou inferior exposta ou exigiriam enterrá-las além da área do lance.
- **IK por lance.** O caminho atual depende da lista serializada de suportes, consultada tanto pelo controlador quanto pela animação. Retirar apenas a L3 preserva o comportamento da B3 durante a prova. O sinal legado de pose também influencia a câmera, então o enquadramento é parte do aceite.
- **Grounding.** A adesão atual é −2 m/s. O ensaio deve medir a descida a pé e em corrida, além de distinguir perda momentânea de apoio, queda legítima e regressão. Alteração localizada depende dos resultados.
- **Autoria e navegação.** Ambas as casas usam o prefab genérico House, com fachada e suportes como dados locais da cena. Não aplicar overrides à origem compartilhada. O bake existente usa colisores físicos e o walker também detecta obstáculos por cápsula. Validar trajeto efetivo e preservar pontos da ilha.
