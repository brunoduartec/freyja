# Pesquisa e decisão preliminar

- GraphicsSettings serializa `m_CustomRenderPipeline: {fileID: 0}`; manifesto não inclui URP; documentação `Tools/MCP/README.md` registra Built-in. Confirmar configuração efetiva no Editor.
- [Uber Stylized Water](https://github.com/MatrixRex/Uber-Stylized-Water) informa licença MIT, Unity 6 e suporte a URP, explicitamente não Built-in. Importação funcional sem instalar URP ou migrar não é compatível; avaliar viabilidade isolada e documentar o limite antes da decisão definitiva. Não copiar código.
- Já existem `Art/Mar049/ShelteredSea.shader`, malha, distância costeira e material, além de vento global e dia/noite; inspecionar para reaproveitar o que funciona.
- Link Pinterest não abriu na ferramenta de pesquisa; referência não observada. O GDD prevê ilha 260×180 m enquanto anexo menciona cerca de 160×160 m: medir a cena.
- Custo incremental GPU desejável de aproximadamente 2 ms é apenas referência, não resultado medido. Profiling comparável depende do suporte do ambiente.
