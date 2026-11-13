# Contrato Blender → Unity

- **Fonte nova:** Source/Blender/Ilha_Bruta_003.blend, com relevo, rotas, casas e marcos em coleções identificáveis.
- **Export:** FBX em Freyja/Assets/_Project/Art/Environment/IlhaBruta/; materiais simples compatíveis com Built-in.
- **Destino:** prefab IlhaBruta e cena nova Ilha_Bruta_003.unity; não substituir cenas, prefabs ou arquivos .meta existentes.

Usar metros e conversão de eixos consistente. Conferir escala, orientação, pivôs e faces do relevo no Unity, pois faces invertidas podem desaparecer. Blocos B1–B4, A1–A4 e L1–L4 devem poder ser contados separadamente. Verificar colisões de terreno e blocos, costa e encaixe com o cais; não criar piso invisível sobre o mar.

Reutilizar o personagem e os controles da 002. Atualizar bounds e distância de visão apenas na nova cena. Registrar fonte, export, cena, dimensões e evidência visual depois da integração.
