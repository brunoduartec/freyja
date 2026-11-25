# Decisões técnicas

- Skybox procedural em direção de mundo para manter horizonte, disco solar e deslocamento das nuvens coerentes ao girar a câmera. A renderização existente fornece a baixa resolução; as nuvens usam volumes estilizados com bordas definidas.
- Reutilizar a luz Coastal Sun e o ambiente de três cores. O disco e a luz usam a mesma direção solar. A iluminação ambiente azul dá leitura ao caminho à noite.
- Usar a API do Unity via CLI com projeto explícito, pois múltiplos editores impedem a resolução automática pelo MCP global.
