# Plano — Pedras da encosta

**Branch** `034-pedras-encosta` · **Spec** [Pedras da encosta](spec.md)

## Resumo

Montar um piloto editável na encosta oeste da cena principal. Reagrupar as rochas do Kit PS1 022 em uma formação costeira alta e um afloramento baixo, com faces em tons quentes e escalas variadas.

## Contexto técnico

Unity 6000.3.25f1, Built-in. Usar APIs do editor pelo Unity Pipeline para ajustar instâncias, criar os materiais locais e salvar a cena. A primeira composição usa as malhas existentes. Se o encaixe ou a silhueta falhar na avaliação visual, revisar as formas no Blender. Validar na cena principal com capturas pareadas e caminhada em Play Mode pelo controlador existente.

## Constituição

Escopo limitado ao trecho aprovado, com dados editáveis e evidência visual no enquadramento jogável. Regras, cenas paralelas e ferramentas permanentes de runtime ficam fora do escopo. A solução respeita a direção low poly e usa o editor para edição e validação.

## Estrutura

- `Freyja/Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity` contém as instâncias do piloto, sob `Island023`.
- `Freyja/Assets/_Project/Art/Encosta034/` contém materiais específicos do trecho.
- `Artifacts/Encosta034/` contém capturas e evidências de validação.
