# Validação

Abra `Ilha_Tripo_Caminhada_011`. Compare a ilha e os setores nas capturas em `Artifacts/Relevo037/`. Em Play Mode, percorra cais, praça, igreja, trilha alta, casa isolada e costa leste. Confira as rotas existentes dos lotes na proximidade das mudanças.

## Entrega

- Vista aérea · [antes](../../Artifacts/Relevo037/aerea-antes.png) e [depois](../../Artifacts/Relevo037/aerea-depois.png).
- Costa oeste e enseada · [antes](../../Artifacts/Relevo037/oeste-antes.png) e [depois](../../Artifacts/Relevo037/oeste-depois.png).
- Norte e leste · [antes](../../Artifacts/Relevo037/nordeste-antes.png) e [depois](../../Artifacts/Relevo037/nordeste-depois.png).
- Câmera do jogador no cais · [antes](../../Artifacts/Relevo037/jogo-cais-antes.png) e [depois](../../Artifacts/Relevo037/jogo-cais-depois.png). No leste · [antes](../../Artifacts/Relevo037/jogo-leste-antes.png) e [depois](../../Artifacts/Relevo037/jogo-leste-depois.png). Na crista · [antes](../../Artifacts/Relevo037/jogo-crista-antes.png) e [depois](../../Artifacts/Relevo037/jogo-crista-depois.png).
- Quatro seções · [perfis](../../Artifacts/Relevo037/perfis-antes-depois.png), [amostras](../../Artifacts/Relevo037/perfis.csv) e [verificação de cotas e pintura](../../Artifacts/Relevo037/terreno.txt).
- Play Mode · [22 trajetos concluídos com o controlador do jogo](../../Artifacts/Relevo037/percurso.txt), incluindo cinco ligações e 17 acessos aos edifícios. O roteiro da costa leste acompanha o centro da rua na aproximação final à junção da trilha alta.

O Terrain e o TerrainCollider da cena principal apontam para `TerrenoIlha.asset`. A primeira curva mantém o perfil do piloto como ponto de partida. A praia e os pisos foram comparados com o terreno anterior. Os prefabs das pedras estão preservados como instâncias desativadas. As árvores e os tufos afetados foram reassentados.
