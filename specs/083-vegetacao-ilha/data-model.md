# Modelo de dados — Vegetação regional

Não há modelo persistente novo. A cena serializa GameObjects decorativos agrupados sob um pai de região.

| Entidade | Campos / invariantes | Relações |
|---|---|---|
| Grupo regional | Nome identificando setor/categoria; ativo e filho da raiz de ambiente existente; sem comportamento próprio | Agrupa instâncias vegetais locais |
| Instância vegetal | Referência ao asset compartilhado, Transform (posição/rotação/escala), sem collider decorativo; base alinhada ao solo; árvore vertical | Pertence a um grupo regional |
| Área de exclusão | Espaço observável de caminho, água/faixa molhada, escada, porta, interação, lavoura, rota NPC ou pista | Não deve conter instâncias vegetais |

As instâncias não são entidades de gameplay nem coletáveis. Não armazenar estado em runtime.
