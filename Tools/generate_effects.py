# Generates the bump sound, the result star chime (synthesised, no third-party audio) and the dizzy star sprite.
import math, random, struct, wave
from PIL import Image, ImageDraw
random.seed(3)
# --- Bump.wav: soft "thump" (falling sine) + short leaf rustle (filtered noise)
RATE = 44100; N = int(RATE * 0.32)
samples = []; lp = 0.0
for i in range(N):
    t = i / RATE
    f = 70 + 120 * math.exp(-t * 30)                    # pitch falls 190 -> 70 Hz
    thump = math.sin(2 * math.pi * f * t) * math.exp(-t * 18)
    noise = random.uniform(-1, 1); lp += (noise - lp) * 0.25  # low-passed noise = leaves
    rustle = lp * math.exp(-t * 14) * (1 - math.exp(-t * 400)) * 0.9
    samples.append(max(-1, min(1, 0.75 * thump + rustle)))
peak = max(abs(s) for s in samples)
with wave.open('Assets/Sounds/Bump.wav', 'wb') as w:
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE)
    w.writeframes(b''.join(struct.pack('<h', int(s / peak * 0.8 * 32767)) for s in samples))
# --- DizzyStar.png: yellow cartoon star with a brown outline
S = 4; W = 64 * S; c = W / 2
pts = []
for k in range(10):
    r = (W * 0.46) if k % 2 == 0 else (W * 0.2)
    a = math.radians(-90 + k * 36)
    pts.append((c + r * math.cos(a), c + r * math.sin(a)))
im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
d.polygon(pts, fill=(255, 214, 51, 255), outline=(92, 54, 20, 255), width=4 * S)
im.resize((64, 64), Image.LANCZOS).save('Assets/Graphic/Effects/DizzyStar.png')
# --- Resources/Sounds/StarDing.wav: short bright chime for each star on the result screen
# (ResultStarsEffect plays it with a rising pitch per star). Bell-like partials, fast attack, soft decay.
N = int(RATE * 0.55)
samples = []
for i in range(N):
    t = i / RATE
    env = (1 - math.exp(-t * 900)) * math.exp(-t * 7)
    tone = (math.sin(2 * math.pi * 1046.5 * t) + 0.5 * math.sin(2 * math.pi * 2093 * t) * math.exp(-t * 6)
            + 0.25 * math.sin(2 * math.pi * 3136 * t) * math.exp(-t * 12))
    samples.append(tone * env)
peak = max(abs(s) for s in samples)
with wave.open('Assets/Resources/Sounds/StarDing.wav', 'wb') as w:
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE)
    w.writeframes(b''.join(struct.pack('<h', int(s / peak * 0.7 * 32767)) for s in samples))
