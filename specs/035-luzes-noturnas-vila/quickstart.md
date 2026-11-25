# Revisão no Unity

Abra `Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` no projeto da branch 035 e inicie Play Mode. No objeto `Coastal Sun`, desmarque `Advance Clock` e ajuste `Hour`. Compare 12, 17,5, 18, 18,5, 0, 5,5 e 6,5. Caminhe do cais à praça à noite para conferir as piscinas de luz e o contraste do piso. As fontes são editáveis sob `Luzes noturnas da vila`.

## Validação executada

- Compilação no Unity 6000.3.25f1 concluída com zero erros e avisos. O Console registrou mensagens do Package Manager e da indexação interna do editor durante a sessão.
- A cena contém cinco fachadas iluminadas e dois lampiões. A [comparação entre dia e noite](evidencias/comparativo.png) mostra a incidência de luz nas fachadas, no cais e no piso da praça. A captura [sem as luzes locais](evidencias/praca-noite-sem-luzes.png) permite comparar a contribuição das luminárias à meia-noite.
- O [vídeo do entardecer](evidencias/acendimento-entardecer.mp4) registra o ciclo rodando em Play Mode, da praça, com as sete luzes entrando gradualmente. A duração do ciclo foi ajustada temporariamente para a gravação.
- Às 17h30, nenhuma luz emitia. Às 17h45, quatro fontes já emitiam. Às 18h30, todas estavam na intensidade noturna, que se manteve na passagem da meia-noite e até 5h30. Às 6h30, todas estavam apagadas. Ao voltar para 18h, o conjunto retomou as mesmas intensidades.
- A caminhada noturna percorreu os 11 pontos de verificação do cais até o centro da praça, cerca de 70 m, sem recuperação. Os dois lampiões ficam visíveis nos pontos de passagem.
