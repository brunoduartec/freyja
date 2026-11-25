# Dados da escada

- **Instância de casa** contém fachada visível, guardas, pisos adjacentes e o lance.
- **Lance convertido** tem uma malha de colisão estática em coordenadas locais, com largura e extremos limitados à superfície transitável. O colisor usa a layer de sólidos do cenário. Caixas antigas desativadas mantêm dados de reversão.
- **Patamar** é um piso plano independente. Na B3 ele permite parar e mudar de direção na porta.
- **Lista de apoio procedural** referencia lances legados até que sejam validados com a rampa.
- **NavMesh** deriva da geometria física. A validação usa pontos existentes e percursos efetivos de NPC.
