# Draws small cartoon leaf sprites (outlined, in the game's greens) for the path decoration.
import math
from PIL import Image, ImageDraw
S = 4; W = 128
OUT = (64, 92, 34, 255)
FILLS = [(141, 191, 69, 255), (118, 170, 58, 255), (172, 207, 92, 255), (196, 180, 96, 255)]
def leaf_poly(cx, cy, length, width, angle):
    pts = []
    for i in range(41):
        t = i / 40.0
        x = (t - 0.5) * length
        y = math.sin(math.pi * t) ** 0.8 * width / 2
        pts.append((x, y))
    pts += [(x, -y) for x, y in reversed(pts)]
    a = math.radians(angle)
    return [(cx + x*math.cos(a) - y*math.sin(a), cy + x*math.sin(a) + y*math.cos(a)) for x, y in pts]
def draw_leaf(d, cx, cy, length, width, angle, fill):
    d.polygon(leaf_poly(cx, cy, length, width, angle), fill=fill, outline=OUT, width=3*S)
    a = math.radians(angle)
    d.line([(cx - math.cos(a)*length*0.42, cy - math.sin(a)*length*0.42),
            (cx + math.cos(a)*length*0.38, cy + math.sin(a)*length*0.38)], fill=OUT, width=2*S)
def save(name, fn):
    im = Image.new('RGBA', (W*S, W*S), (0, 0, 0, 0)); d = ImageDraw.Draw(im); fn(d)
    im.resize((W, W), Image.LANCZOS).save(f'Assets/Graphic/Path/Leaves/{name}.png')
c = W*S/2
save('Leaf1', lambda d: draw_leaf(d, c, c, 100*S, 46*S, -30, FILLS[0]))
save('Leaf2', lambda d: (draw_leaf(d, c-14*S, c+6*S, 80*S, 38*S, -60, FILLS[1]), draw_leaf(d, c+16*S, c-4*S, 80*S, 38*S, 10, FILLS[2])))
save('Leaf3', lambda d: [draw_leaf(d, c + math.cos(math.radians(a))*26*S, c + math.sin(math.radians(a))*26*S, 62*S, 40*S, a, FILLS[0]) for a in (-90, 30, 150)])
save('Leaf4', lambda d: draw_leaf(d, c, c, 90*S, 40*S, 50, FILLS[3]))   # dry, yellowish leaf
def tuft(d):
    for i, a in enumerate((-35, -15, 0, 18, 38)):
        r = math.radians(a - 90); L = (70 if i % 2 == 0 else 52) * S
        x0, y0 = c + (i-2)*6*S, c + 34*S
        tip = (x0 + math.cos(r)*L, y0 + math.sin(r)*L)
        d.polygon([(x0-6*S, y0), tip, (x0+6*S, y0)], fill=FILLS[1 if i % 2 else 0], outline=OUT, width=3*S)
save('Tuft', tuft)
