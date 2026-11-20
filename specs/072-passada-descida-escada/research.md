# Diagnóstico e decisões

- A descida atual restringe o deslocamento ao eixo do lance e usa a caminhada genérica. Só a subida possui limite de velocidade, contatos por degrau e fase sincronizada com a posição.
- A passada deve avançar pelo deslocamento real para evitar continuar animando quando o personagem para ou encontra uma barreira. O IK pode usar a geometria calibrada para ambos os sentidos.
- Reduzir a velocidade isoladamente deixa os pés seguindo a cadência da caminhada genérica. Amostrar a subida ao contrário pode servir de base, mas os contatos, a orientação do corpo e a transferência de peso precisam ser avaliados na descida.
- O collider Patamar da B3 serve como referência visual de contato. A restrição de movimento deve terminar ao alcançar esse patamar.
- A descida precisa de contatos de entrada no topo e de saída no pé do lance. A orientação dos joelhos e calçados acompanha o eixo descendente, e a folga da ponta e do calcanhar deve ser verificada durante o balanço. O solver precisa conservar a fase durante perdas breves de contato entre degraus e liberar o apoio ao saltar.
