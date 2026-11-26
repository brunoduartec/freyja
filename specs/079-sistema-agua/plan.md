# Plano: água estilizada e costa

**Branch**: 079-sistema-agua · **Spec**: spec.md · **Data**: 2026-11-25

## Resumo e contexto técnico

Unity 6000.3.25f1, C#/shader Built-in, cor linear, projeto `Freyja/`; sem pacote URP no manifesto e sem pipeline customizado em GraphicsSettings. Existe `Assets/_Project/Art/Mar049/`, vento e iluminação em `Scripts/Environment/`, Render Texture 480×270 e cena principal `Assets/_Project/Scenes/Ilha_Principal.unity`. Confirmar overrides, topografia, câmera efetiva e baseline no Editor. Preservar pacote, pipeline e GUIDs. Plataformas e orçamento de GPU devem ser medidos, não presumidos.

## Ordem e gates

1. Diagnóstico real da cena, câmera, profundidade, ambiente, baseline e licença/compatibilidade; registrar ADR. Avaliar candidato isoladamente se tecnicamente viável sem migração.
2. Prova isolada em `Assets/_Project/` com praia curva, fundo e câmera equivalente; validar saída final e transparência.
3. Núcleo com superfície subdividida, profundidade, iluminação, ondulações e contato; gate AC-01–04.
4. Costa ancorada ao mundo, avanço/recuo, areia molhada e geração não destrutiva; gate AC-05–07/14.
5. Cáusticas, vento, dia/noite, qualidade e refração opcional; gate AC-08–10/13.
6. Só integrar na cena principal quando o núcleo isolado passar; confirmar câmera e Render Texture (AC-11). Interações após núcleo aprovado (AC-12).
7. QA independente, correções, recaptura, comparação de performance e documentação. Sem evidência, não declarar PASS.

## Estrutura e riscos

Arte e scripts próprios em `Freyja/Assets/_Project/`; testes e cena isolada no mesmo projeto; `docs/water/` para diagnóstico, ADR, relatório e guia; `artifacts/water-validation/` para capturas locais. Adaptação exata dependerá do diagnóstico dos sistemas existentes. Riscos: profundidade Built-in na câmera RT, geometria submersa ausente, aliasing a 480×270, costa complexa e custo de transparência. Não alterar cenário principal até o gate.

## Constituição

Aprovado: respeitar GDD, direção low poly e pixelada, trabalhar e validar no Unity MCP, registrar fatos e evidências. Nenhuma alteração narrativa. Reavaliar após design e implementação; não migrar pipeline.
