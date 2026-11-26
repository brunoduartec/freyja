# ADR — candidato Uber e pipeline

Status: decisão preliminar, prova de conceito local aberta.

O [Uber Stylized Water](https://github.com/MatrixRex/Uber-Stylized-Water) declara licença MIT e suporte a Unity 6 / URP, mas explicita que Built-in não é suportado. HEAD consultado: `1fdd16c6312611fef7fb248dee86cdd3fd456a1e`. O Freyja usa Built-in; não instalar URP nem migrar sem autorização. Assim, a prova direta funcional na cena isolada exigiria alteração de pipeline proibida e não foi realizada. Nenhum código do projeto externo foi importado. A prova local usa shader Built-in e está em avaliação, sem decisão final de arquitetura ou evidência de custo comparativo. Pinterest indicado no anexo não pôde ser acessado; não se inferiu seu conteúdo.
