"""Three tileable 64 px surfaces at a shared scale, from flagstone to packed earth."""
import math
import random
from pathlib import Path
import sys
from PIL import Image

SIZE = 64
STEP = SIZE / 6
STONES = [(184, 139, 105), (196, 151, 115), (203, 155, 113), (190, 144, 108),
          (200, 151, 116), (187, 139, 104), (193, 148, 115), (181, 135, 102)]
JOINT = (207, 171, 135)
EARTH = (181, 145, 108)


def wrap(x):
    return (x + SIZE / 2) % SIZE - SIZE / 2


def shade(rgb, amount):
    return tuple(round(max(0, min(255, c + amount))) for c in rgb)


def make_surfaces(folder):
    rng = random.Random(270619)
    cells = [(STEP * (x + .5 + rng.uniform(-.43, .43)),
              STEP * (z + .5 + rng.uniform(-.43, .43)), rng.choice(STONES),
              rng.randrange(1, 12)) for z in range(6) for x in range(6)]
    sparse = random.Random(300707).sample(range(len(cells)), 14)
    packed = Image.new('RGB', (SIZE, SIZE))
    paved = Image.new('RGB', (SIZE, SIZE))
    worn = Image.new('RGB', (SIZE, SIZE))
    for z in range(SIZE):
        for x in range(SIZE):
            u, v = x * math.tau / SIZE, z * math.tau / SIZE
            wx = x + .65 * math.sin(v * 5 + u * 3)
            wz = z + .53 * math.cos(u * 4 - v * 3)
            ranked = []
            for i, (sx, sz, color, grain) in enumerate(cells):
                dx, dz = wrap(wx - sx), wrap(wz - sz)
                ranked.append((dx * dx + dz * dz, dx, dz, color, grain, i))
            first, second = sorted(ranked, key=lambda cell: cell[0])[:2]
            distance, dx, dz, stone, grain, index = first
            boundary = (second[0] - distance) / max(.001, 2 * math.hypot(dx - second[1], dz - second[2]))
            scatter = rng.choice((-3, -2, -1, 0, 0, 0, 1, 2, 3))
            mottling = round(3.1 * math.sin(u * 3 + v + .6) * math.cos(v * 4 - u * 2) +
                              2.4 * math.sin(u * 5 - v * 3) + 1.0 * math.sin(u * 2 + v * 7))
            earth = shade(EARTH, scatter + mottling + (5 if (x * 3 + z * 13) % 97 == 0 else 0))
            if boundary < .43:
                tile = shade(JOINT, scatter)
            elif boundary < .94:
                tile = shade(stone, scatter + (-8 if dx + dz > .7 else 5))
            else:
                tile = shade(stone, scatter + 2 * math.sin((dx + dz) * .85 + grain))
            exposed = earth
            if index in sparse and boundary >= .63:
                exposed = shade(tile, -3 if boundary < 1 else -1)
            packed.putpixel((x,z), earth)
            paved.putpixel((x,z), tile)
            worn.putpixel((x,z), exposed)
    folder.mkdir(parents=True, exist_ok=True)
    for name, image in [('TerraBatida', packed), ('LajesDaPraca', paved), ('LajesNaTerra', worn)]:
        image.save(folder / (name + '.png'))


if __name__ == '__main__':
    make_surfaces(Path(sys.argv[1]))
