# Recolors the frog sprite sheet (Assets/Graphic/GameObject/Fog.png) into skins.
# Only greenish pixels (body, spots) change; eyes, mouth, cheeks and outline stay.
# Usage: python Tools/generate_frog_skins.py [out_dir] [--preview file.png]
import colorsys, sys
from PIL import Image
SKINS = {  # name: (target body hue 0-360, saturation mul, brightness lift 0-1, spot hue)
    'Yellow': (52, 1.4, 0.55, 28),
    'Orange': (28, 1.1, 0.40, 50),
    'Red':    (2,  1.0, 0.30, 45),
    'Blue':   (215, 1.0, 0.25, 185),
    'Teal':   (178, 1.0, 0.25, 140),
    'Purple': (275, 0.9, 0.25, 310),
    'Pink':   (325, 0.75, 0.40, 290),
}
def recolor(img, hue, sm, lift, spot_hue):
    out = img.copy(); px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0: continue
            h, s, v = colorsys.rgb_to_hsv(r/255, g/255, b/255)
            if not (0.12 < h < 0.5 and s > 0.25): continue
            spot = v > 0.55            # the bright spots on the back
            h = (spot_hue if spot else hue) / 360.0
            s = min(1, s * sm)
            v = min(1, v + lift * (1 - v) * (0.6 if spot else 1))
            r2, g2, b2 = colorsys.hsv_to_rgb(h, s, v)
            px[x, y] = (int(r2*255), int(g2*255), int(b2*255), a)
    return out
if __name__ == '__main__':
    src = Image.open('Assets/Graphic/GameObject/Fog.png').convert('RGBA')
    if '--preview' in sys.argv:
        src = src.resize((src.width//4, src.height//4), Image.LANCZOS)
    out = sys.argv[1] if len(sys.argv) > 1 and not sys.argv[1].startswith('--') else None
    for name, p in SKINS.items():
        im = recolor(src, *p)
        if out: im.save(f'{out}/Frog{name}.png')
        else: im.save(f'_frog_{name}.png')
