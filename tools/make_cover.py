#!/usr/bin/env python3
"""Draws the 630 by 500 itch.io cover from the game's own font sheet (DINOBYTE, unmodified)."""
import sys
from PIL import Image

SHEET = "assets/dinobyte/dbyte_1x.png"
OUT = sys.argv[1] if len(sys.argv) > 1 else "assets/cover.png"
BG = (11, 11, 18)
INK = {
    "beige": (172, 172, 145),
    "white": (255, 255, 255),
    "yellow": (255, 215, 95),
    "green": (95, 215, 95),
    "red": (255, 95, 95),
    "gray": (128, 128, 128),
    "cyan": (95, 215, 215),
    "magenta": (215, 95, 215),
}

sheet = Image.open(SHEET).convert("RGBA")
W, H = 630, 500
img = Image.new("RGB", (W, H), BG)

CP437_FIX = {"ü": 0x81, "ä": 0x84, "ë": 0x89, "ö": 0x94, "Ö": 0x99, "Ü": 0x9A}


def glyph(code, color, scale):
    cx, cy = (code % 16) * 6, (code // 16) * 8
    cell = sheet.crop((cx, cy, cx + 6, cy + 8))
    out = Image.new("RGBA", (6, 8), (0, 0, 0, 0))
    px = cell.load()
    o = out.load()
    for y in range(8):
        for x in range(6):
            if px[x, y][3] > 0:
                o[x, y] = color + (255,)
    return out.resize((6 * scale, 8 * scale), Image.NEAREST)


def text_width(s, scale):
    return len(s) * 6 * scale


def put(x, y, s, color, scale):
    for ch in s:
        code = ch if isinstance(ch, int) else CP437_FIX.get(ch, ord(ch))
        g = glyph(code, INK[color], scale)
        img.paste(g, (x, y), g)
        x += 6 * scale


def centered(y, s, color, scale):
    put((W - text_width(s, scale)) // 2, y, s, color, scale)


# Title
centered(28, "GWEN'S", "yellow", 8)
centered(100, "PEREGRINATION II", "yellow", 6)

# A road with a walker, snax and a finish line, between verges of flowers and bushes.
S = 5
cols = W // (6 * S)
left = (W - cols * 6 * S) // 2
scene_y = 178
verge_top = [0x05, 0x0F, 0x20, 0x05, 0x20, 0x0F, 0x05, 0x20]
verge_bottom = [0x0F, 0x20, 0x05, 0x05, 0x20, 0x0F, 0x20, 0x05]
for i in range(cols):
    put(left + i * 6 * S, scene_y, [verge_top[i % len(verge_top)]], "green", S)
    put(left + i * 6 * S, scene_y + 8 * S, [0xB0], "gray", S)
    put(left + i * 6 * S, scene_y + 16 * S, [verge_bottom[i % len(verge_bottom)]], "green", S)
road_y = scene_y + 8 * S
put(left + 3 * 6 * S, road_y, [0x02], "white", S)  # Gwen
put(left + 9 * 6 * S, road_y, [0x0F], "yellow", S)  # snax
put(left + 15 * 6 * S, road_y, [0x04], "cyan", S)  # a jool
put(left + (cols - 3) * 6 * S, road_y, [0x10], "red", S)  # the way on

# Tagline and credits
centered(372, "2000 miles. Snax.", "white", 3)
centered(404, "A learner's permit.", "white", 3)
centered(448, "TheGrumpyGameDev", "beige", 2)
centered(472, "Nonbinary Game Jam 2026", "gray", 2)

img.save(OUT)
print("wrote", OUT, img.size)
