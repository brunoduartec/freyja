# Validação rápida

1. Abrir `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity` no Unity 6000.3.25f1 e confirmar cena salva/importada.
2. Usar a câmera normal do jogador no trecho praia seca/entrada da vila e capturar estado anterior e posterior em horário claro e noturno, mesma pose/FOV/resolução.
3. Entrar em Play Mode, percorrer o trecho, testar o acesso a elementos interativos e observar personagem, portas e caminho com vegetação visível.
4. Inspecionar grupos regionais e verificar ausência de colliders decorativos, instâncias em água/areia molhada, trilha, escada, rota ou interação.
5. Após o piloto passar, revisar praia, vila/caminhos, praça, encosta, lavoura e ruínas existentes; capturar câmera real de dia e noite em cada setor.
6. Comparar custo de renderização e contagem no mesmo Editor, equipamento, câmera e qualidade antes/depois. Conferir Console usando cursor do início da validação; registrar erros/avisos novos.
7. Salvar imagens e relatório em `specs/083-vegetacao-ilha/evidence/` e `validacao.md`.

Resultado esperado: grupos low-poly variados e intencionais; percurso/interações legíveis; nenhuma instância em superfície excluída; sem novo erro de compilação ou regressão significativa sem explicação.
