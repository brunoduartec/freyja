# Estudo preliminar do mar de Freyja

O GDD descreve uma ilha ensolarada cercada de água turquesa, com o píer protegido na enseada sul. A superfície deve ajudar a distinguir o abrigo do cais e a abertura do mar. O contraste perto de personagens e pistas precisa ser baixo o suficiente para que água e espuma permaneçam em segundo plano.

Confirmei a cena 011 ativa via MCP. O arquivo da cena referencia três materiais de mar com cores turquesa e alpha 1, e a câmera de jogo tem distância de recorte de 150 m. A primeira captura deve identificar quais superfícies aparecem e se a água some antes da linha do horizonte. A inspeção ainda é documental, sem teste em Play Mode.

| Proposta | Como aparece na câmera | Ponto a verificar |
| --- | --- | --- |
| **A · Água opaca com ondulações animadas** | Uma superfície turquesa com faixas largas em duas direções. Perto da costa, um tom mais claro e espuma localizada sugerem águas rasas. | É a primeira aposta para preservar a leitura dos pixels. O movimento pode parecer uma textura deslizando se as camadas tiverem ritmo e direção parecidos. |
| **B · Água com ondulação geométrica** | Ondas baixas deformam discretamente a superfície, com cristas mais claras em mar aberto. A enseada mantém menor amplitude. A soma de senos dá cristas suaves. Gerstner é uma opção para cristas mais definidas se a câmera justificar. | O volume precisa aparecer na altura do píer sem cortar pilares ou revelar interseções com rochas. A contribuição depende da densidade da malha. [Mark Finch, GPU Gems](https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-1-effective-water-simulation-physical-models) |

Para a espuma, começaria com **uma máscara da costa e dos pilares**, reutilizada nas duas propostas. A máscara dá controle sobre onde a espuma aparece. Se o contato ficar pouco convincente, vale comparar com espuma por profundidade da câmera. No Built-in, a textura de profundidade tem regras de inclusão ligadas à fila de renderização e ao passe de sombra, então o resultado depende dos materiais presentes na cena. [Manual do Unity sobre profundidade da câmera](https://docs.unity3d.com/6000.3/Documentation/Manual/SL-CameraDepthTexture.html)

| Trecho | Comportamento a observar |
| --- | --- |
| **Sob o píer** | Ondulações baixas. Arcos curtos de espuma junto aos pilares, com pausas entre eles. |
| **Costão** | Grupos de ondas chegam em sequência, desenham trechos irregulares de espuma e se dissipam. |
| **Mar aberto** | Ondas mais largas, turquesa mais fechado e perda gradual do detalhe fino na distância. |

Minha primeira aposta é **A com espuma localizada**. A comparação com B deve mostrar se o volume das ondas é perceptível na câmera a ponto de justificar seu custo. O retorno ao mesmo píer, reservado no GDD aos moradores sem passagem autorizada, pode ser estudado depois como comportamento narrativo da travessia.
