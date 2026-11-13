# Modelo espacial — Spec 005

Esta entrega não introduz persistência nem entidades narrativas novas. O modelo é o conjunto espacial da ilha e os elementos herdados usados para avaliar composição e movimento.

| Entidade | Campos relevantes | Relações e regras |
| --- | --- | --- |
| Superfície de terreno | malha, posição, altura, normal, inclinação, material, papel visual | Forma a costa, praia, patamares, maciço, cristas e faces rochosas. A inclinação é medida relativamente à horizontal sobre a superfície de colisão integrada. |
| Região percorrível | polígono/área, inclinações, apoio do jogador, conexões e estado de obstrução | Área terrestre livre abaixo de 45° deve ser acessível. Água, pés de construção e faces íngremes são excluídos da caminhada. Regiões obrigatórias conectam cais, largo, setores, horta/ruína e recanto. |
| Caminho | faixa visual, início/fim, superfície de suporte, conexão | Segue os terraços compatíveis com a referência. A apresentação visual do caminho não deve divergir do apoio físico. |
| Marco herdado | ID, posição, dimensões, tipo, origem | Cais, barco, largo, 12 volumes em setores B/A/L, hortas, ruína e casa do recanto. Pode ser reposicionado; mantém sua identidade funcional. |
| Configuração de exploração | cena, jogador, controlador, câmera, spawn, recuperação | Continuação da 004. Preserva controles e movimento; bounds e pontos de integração podem ser ajustados por instância da cena 005. |
| Evidência de comparação | vista, ângulo, escala de apresentação, imagem de origem, observações | Permite relacionar as seis características visíveis da referência com a forma criada; vistas devem ter orientação e enquadramento declarados. |

## Invariantes de validação

- Classificar triângulos do terreno por faixa `<45°`, `45–70°`, `70–80°` e `>80°`; registrar inclinações máximas e exemplos de faces íngremes intencionais.
- Exigir acesso conectado aos destinos funcionais por superfícies utilizáveis e caminhada contínua; o teste em grade é uma aproximação, não prova todos os pontos.
- Testar as faces de 70–80° como obstáculos no controlador em Play Mode.
- Preservar cenas e ativos anteriores, distinguindo elementos herdados, reposicionados e recriados.
- Aprovação visual, análise geométrica e percurso físico são estados de evidência separados.
