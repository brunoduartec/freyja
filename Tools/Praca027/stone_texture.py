"""Paint a seamless 64 px coastal flagstone texture with a seeded palette."""
import math
import random
import sys
from pathlib import Path
from PIL import Image

SIZE = 64
GRID = 6
STEP = SIZE / GRID
PALETTE = [(189, 129, 91), (204, 146, 108), (210, 151, 109), (193, 134, 98),
           (213, 157, 120), (197, 139, 102), (203, 142, 110), (185, 125, 88)]
JOINT = (218, 176, 138)

def wrapped(value):
    return (value + SIZE / 2) % SIZE - SIZE / 2

def make_texture():
    rng = random.Random(270619)
    stones = [(STEP * (x + .5 + rng.uniform(-.43, .43)),
               STEP * (z + .5 + rng.uniform(-.43, .43)), rng.choice(PALETTE),
               rng.randrange(1, 12)) for z in range(GRID) for x in range(GRID)]
    image = Image.new('RGB', (SIZE, SIZE))
    for z in range(SIZE):
        for x in range(SIZE):
            # Periodic perturbation softens the corners without breaking tile seams.
            wx = x + .65 * math.sin(z / SIZE * math.tau * 5 + x / SIZE * math.tau * 3)
            wz = z + .53 * math.cos(x / SIZE * math.tau * 4 - z / SIZE * math.tau * 3)
            proximity = []
            for sx, sz, color, grain in stones:
                dx, dz = wrapped(wx - sx), wrapped(wz - sz)
                proximity.append((dx * dx + dz * dz, dx, dz, color, grain))
            nearest, next_stone = sorted(proximity, key=lambda n: n[0])[:2]
            d, dx, dz, color, grain = nearest
            separation = math.hypot(dx - next_stone[1], dz - next_stone[2])
            edge = (next_stone[0] - d) / max(.001, 2 * separation)
            noise = rng.choice((-3, -2, 0, 0, 1, 2))
            if edge < .49:
                color = JOINT
            elif edge < 1.02:
                noise += -11 if dz > 0 or dx > 3 else 8
            elif (x * 13 + z * 5 + grain * 7) % 37 == 0:
                noise -= 6
            else:
                noise += round(math.sin((dx + dz) * .9 + grain) * 1.8)
            image.putpixel((x, z), tuple(max(0, min(255, channel + noise)) for channel in color))
    return image

if __name__ == '__main__':
    make_texture().save(Path(sys.argv[1]))
