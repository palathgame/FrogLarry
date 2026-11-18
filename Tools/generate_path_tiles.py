# Generates a seamless 4x4-cell "trodden path" texture: dirt blended into grass with
# grass patches and leaf tufts, sliced into 16 tiles (PathTile_<row>_<col>.png).
import random, sys
from PIL import Image, ImageFilter, ImageChops
random.seed(int(sys.argv[2]) if len(sys.argv) > 2 else 7)
SAND_MIN, SAND_MAX = float(sys.argv[3]) if len(sys.argv) > 3 else 0.25, float(sys.argv[4]) if len(sys.argv) > 4 else 0.85
TUFTS = int(sys.argv[5]) if len(sys.argv) > 5 else 70
out = sys.argv[1]
src = Image.open('F:/FrogLarryhomecoming/Assets/Graphic/GameObject/Tileset Grounds.jpg').convert('RGB')
T = 256; N = 4; S = T * N
def tile(i):
    r, c = divmod(i, 8); return src.crop((c*T, r*T, (c+1)*T, (r+1)*T))
def tiled(img):
    big = Image.new('RGB', (S, S))
    for y in range(N):
        for x in range(N): big.paste(img, (x*T, y*T))
    return big
grass = tiled(tile(13))
sand = Image.new('RGB', (S, S))
for y in range(N):
    for x in range(N): sand.paste(tile(random.choice([24, 25, 26, 27])), (x*T, y*T))
def noise(cells):
    g = Image.new('L', (cells, cells))
    g.putdata([random.randint(0, 255) for _ in range(cells*cells)])
    t = Image.new('L', (cells*3, cells*3))
    for y in range(3):
        for x in range(3): t.paste(g, (x*cells, y*cells))
    t = t.resize((S*3, S*3), Image.BICUBIC)
    return t.crop((S, S, 2*S, 2*S))
n = Image.blend(noise(9), noise(19), 0.4)
n = Image.blend(n, noise(37), 0.25)
lo, hi = int(SAND_MIN*255), int(SAND_MAX*255)
def curve(v):
    t = max(0.0, min(1.0, (v - 80) / 100.0)); t = t*t*(3-2*t)
    return int(lo + (hi - lo) * t)
mask = n.point(curve)
path = Image.composite(sand, grass, mask)
# leaf tufts: dark leafy clusters cut from tile 16, scattered more where the path is grassier
tuftsrc = tile(16).convert('RGBA'); px = tuftsrc.load()
for y in range(T):
    for x in range(T):
        r, g, b, _ = px[x, y]
        lum = 0.3*r + 0.59*g + 0.11*b
        a = max(0, min(1, (165 - lum) / 35.0))
        px[x, y] = (r, g, b, int(a*255))
crops = [tuftsrc.crop(b) for b in [(20,20,120,120), (110,60,220,170), (40,140,140,240), (150,150,250,250)]]
canvas = path.convert('RGBA')
placed = 0
while placed < TUFTS:
    x, y = random.randrange(S), random.randrange(S)
    if mask.getpixel((x, y)) < (lo+hi)//2 and random.random() < 0.6: continue
    c = random.choice(crops)
    sc = random.uniform(0.5, 1.1)
    c2 = c.resize((int(c.width*sc), int(c.height*sc))).rotate(random.uniform(0, 360), expand=True)
    alpha = c2.getchannel('A').point(lambda a: int(a*0.85)); c2.putalpha(alpha)
    for dx in (-S, 0, S):           # wrap around so the texture stays seamless
        for dy in (-S, 0, S):
            canvas.alpha_composite(c2, (x+dx - c2.width//2, y+dy - c2.height//2)) if 0 <= x+dx < S+c2.width and 0 <= y+dy < S+c2.height else None
    placed += 1
canvas = canvas.convert('RGB')
canvas.save(out + '/_full.png')
for r in range(N):
    for c in range(N):
        canvas.crop((c*T, r*T, (c+1)*T, (r+1)*T)).save(f'{out}/PathTile_{r}_{c}.png')
