"""Generate four bedroom wallpaper tile variants for indoor cabin rooms.

Saves 32×32 PNGs to src/DogDays.Game/Content/Tilesets/ and patches
CabinInterior.tsx to append the four new tile entries (idempotent).

Variants
--------
  botanical  – small muted flowers on warm cream-rose
  stripe     – horizontal ticking stripes in dusty blue + cream
  stars      – tiny stars on deep navy
  solid      – textured dusty-lavender solid

Run from the repo root:
    python tooling/sprites/generate_bedroom_wallpapers.py
"""

from __future__ import annotations

import pathlib
import re

try:
    from PIL import Image
except ImportError as exc:
    raise SystemExit(
        "Pillow is required. Install it in the repo virtual environment "
        "before running this script."
    ) from exc

# ---------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------

BASE_DIR = pathlib.Path(__file__).resolve().parents[2]
OUTPUT_DIR = BASE_DIR / "src" / "DogDays.Game" / "Content" / "Tilesets"
TSX_PATH = OUTPUT_DIR / "CabinInterior.tsx"

TILE = 32

# ---------------------------------------------------------------------------
# Shared helpers
# ---------------------------------------------------------------------------

def clamp(v: int) -> int:
    return max(0, min(255, v))


def _noise(x: int, y: int, seed: int = 0) -> int:
    """Deterministic pseudo-noise in the range [-7, 7].  No stdlib random needed."""
    return ((x * 73856093 ^ y * 19349663 ^ seed * 83492791) & 0xFFFF) % 15 - 7


# ---------------------------------------------------------------------------
# Wallpaper 1 – Botanical
# Small 5-pixel cross flowers + stem + leaf on warm cream-rose background.
# Two flowers per tile at (8, 7) and (24, 23) — offset by exactly 16px on
# both axes so the pattern tiles seamlessly on a 32px grid.
# ---------------------------------------------------------------------------

_BG_BOTANICAL = (240, 220, 208)
_PETAL         = (200, 125, 115)
_CENTER        = (245, 225, 195)
_STEM          = (110, 125,  80)
_LEAF          = (130, 145,  95)

_FLOWER_POSITIONS = [(8, 7), (24, 23)]


def _draw_flower(pixels, cx: int, cy: int) -> None:
    """Place a tiny flower, wrapping at tile boundaries for edge safety."""
    def put(x: int, y: int, color: tuple) -> None:
        pixels[x % TILE, y % TILE] = color + (255,)

    # 4-petal cross
    put(cx,     cy,     _CENTER)
    put(cx - 1, cy,     _PETAL)
    put(cx + 1, cy,     _PETAL)
    put(cx,     cy - 1, _PETAL)
    put(cx,     cy + 1, _PETAL)
    # 2-pixel stem
    put(cx,     cy + 2, _STEM)
    put(cx,     cy + 3, _STEM)
    # Small side leaf
    put(cx + 1, cy + 3, _LEAF)


def make_botanical() -> Image.Image:
    img = Image.new("RGBA", (TILE, TILE))
    pixels = img.load()
    for y in range(TILE):
        for x in range(TILE):
            n = _noise(x, y, seed=1) // 2
            pixels[x, y] = (
                clamp(_BG_BOTANICAL[0] + n),
                clamp(_BG_BOTANICAL[1] + n),
                clamp(_BG_BOTANICAL[2] + n),
                255,
            )
    for cx, cy in _FLOWER_POSITIONS:
        _draw_flower(pixels, cx, cy)
    return img


# ---------------------------------------------------------------------------
# Wallpaper 2 – Ticking stripe (horizontal)
# 1 dusty-blue stripe every 5 rows on a warm cream background.
# ---------------------------------------------------------------------------

_BG_STRIPE     = (238, 232, 218)
_STRIPE_COLOR  = (115, 140, 165)
_STRIPE_PERIOD = 5


def make_stripe() -> Image.Image:
    img = Image.new("RGBA", (TILE, TILE))
    pixels = img.load()
    for y in range(TILE):
        is_stripe = (y % _STRIPE_PERIOD == 0)
        base = _STRIPE_COLOR if is_stripe else _BG_STRIPE
        for x in range(TILE):
            n = _noise(x, y, seed=2) // 3
            pixels[x, y] = (
                clamp(base[0] + n),
                clamp(base[1] + n),
                clamp(base[2] + n),
                255,
            )
    return img


# ---------------------------------------------------------------------------
# Wallpaper 3 – Stars on deep navy
# Bright and dim 4-point micro-stars at fixed positions that tile at 32px.
# ---------------------------------------------------------------------------

_BG_STARS      = (28,  32,  66)
_STAR_BRIGHT   = (230, 220, 170)
_STAR_DIM      = (160, 155, 130)

# Positions chosen so all tip offsets stay within 1..30 — no wrap needed.
_STARS_BRIGHT_POS = [(6, 5), (21, 3), (13, 17), (29, 21), (3, 27)]
_STARS_DIM_POS    = [(10, 12), (26, 8), (7, 24), (22, 29), (16, 6), (30, 15)]


def _draw_star(pixels, cx: int, cy: int, color: tuple) -> None:
    """Draw a 5-pixel diamond star (center + 4 dim tips)."""
    def put(x: int, y: int, c: tuple) -> None:
        pixels[x % TILE, y % TILE] = c + (255,)

    dim = (clamp(color[0] - 55), clamp(color[1] - 55), clamp(color[2] - 55))
    put(cx,     cy,     color)
    put(cx - 1, cy,     dim)
    put(cx + 1, cy,     dim)
    put(cx,     cy - 1, dim)
    put(cx,     cy + 1, dim)


def make_stars() -> Image.Image:
    img = Image.new("RGBA", (TILE, TILE))
    pixels = img.load()
    for y in range(TILE):
        for x in range(TILE):
            n = _noise(x, y, seed=3) // 4
            pixels[x, y] = (
                clamp(_BG_STARS[0] + n),
                clamp(_BG_STARS[1] + n),
                clamp(_BG_STARS[2] + n),
                255,
            )
    for cx, cy in _STARS_BRIGHT_POS:
        _draw_star(pixels, cx, cy, _STAR_BRIGHT)
    for cx, cy in _STARS_DIM_POS:
        _draw_star(pixels, cx, cy, _STAR_DIM)
    return img


# ---------------------------------------------------------------------------
# Wallpaper 4 – Textured solid (dusty lavender)
# Plain colour with deterministic grain — no hard pattern.
# ---------------------------------------------------------------------------

_BASE_SOLID = (182, 162, 208)


def make_solid() -> Image.Image:
    img = Image.new("RGBA", (TILE, TILE))
    pixels = img.load()
    for y in range(TILE):
        for x in range(TILE):
            n = _noise(x, y, seed=4)
            pixels[x, y] = (
                clamp(_BASE_SOLID[0] + n),
                clamp(_BASE_SOLID[1] + n),
                clamp(_BASE_SOLID[2] + n),
                255,
            )
    return img


MGCB_PATH = BASE_DIR / "src" / "DogDays.Game" / "Content" / "Content.mgcb"

# ---------------------------------------------------------------------------
# Content.mgcb patcher — keeps the pipeline in sync with the TSX
# ---------------------------------------------------------------------------

def _mgcb_entry(relative_path: str) -> str:
    """Return the Content.mgcb stanza for a Tileset PNG."""
    return (
        f"\n#begin {relative_path}\n"
        "/importer:TextureImporter\n"
        "/processor:TextureProcessor\n"
        "/processorParam:ColorKeyColor=255,0,255,255\n"
        "/processorParam:ColorKeyEnabled=True\n"
        "/processorParam:GenerateMipmaps=False\n"
        "/processorParam:PremultiplyAlpha=True\n"
        "/processorParam:ResizeToPowerOfTwo=False\n"
        "/processorParam:MakeSquare=False\n"
        "/processorParam:TextureFormat=Color\n"
        f"/build:{relative_path}"
    )


def _patch_mgcb(filenames: list[str]) -> None:
    """Append missing Tileset PNG entries to Content.mgcb (idempotent)."""
    content = MGCB_PATH.read_text(encoding="utf-8")
    added = 0
    for filename in filenames:
        rel = f"Tilesets/{filename}"
        if f"/build:{rel}" in content:
            print(f"  [skip] {rel} already in Content.mgcb")
            continue
        content += _mgcb_entry(rel) + "\n"
        print(f"  [add]  {rel} → Content.mgcb")
        added += 1
    if added:
        MGCB_PATH.write_text(content, encoding="utf-8")


# ---------------------------------------------------------------------------
# Ordered list of (filename, terrainType, generator_fn)
# ---------------------------------------------------------------------------

_VARIANTS: list[tuple[str, str, object]] = [
    ("cabin-wall-wallpaper-botanical.png", "CabinWallpaper", make_botanical),
    ("cabin-wall-wallpaper-stripe.png",    "CabinWallpaper", make_stripe),
    ("cabin-wall-wallpaper-stars.png",     "CabinWallpaper", make_stars),
    ("cabin-wall-wallpaper-solid.png",     "CabinWallpaper", make_solid),
]


# ---------------------------------------------------------------------------
# TSX patcher — string-based to preserve the original Tiled formatting
# ---------------------------------------------------------------------------

def _patch_tsx(to_add: list[tuple[str, str]], next_id: int) -> None:
    """Append missing tile entries and bump tilecount in CabinInterior.tsx."""
    content = TSX_PATH.read_text(encoding="utf-8")

    added = 0
    for filename, terrain_type in to_add:
        # Idempotency guard — skip if the source file is already referenced
        if f'source="{filename}"' in content:
            print(f"  [skip] {filename} already registered")
            continue

        tile_block = (
            f'  <tile id="{next_id}">\n'
            f'    <properties>\n'
            f'      <property name="blocked" value="true"/>\n'
            f'      <property name="terrainType" value="{terrain_type}"/>\n'
            f'    </properties>\n'
            f'    <image source="{filename}" width="32" height="32"/>\n'
            f'  </tile>\n'
        )
        # Insert immediately before the closing </tileset> tag
        content = content.replace("</tileset>", tile_block + "</tileset>")
        print(f"  [add]  id={next_id}  {filename}")
        next_id += 1
        added += 1

    if added > 0:
        # Bump tilecount by the number of newly added entries
        def _bump(m: re.Match) -> str:
            return f'tilecount="{int(m.group(1)) + added}"'
        content = re.sub(r'tilecount="(\d+)"', _bump, content)

    TSX_PATH.write_text(content, encoding="utf-8")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    # Determine the current highest tile ID from the TSX so we start correctly.
    tsx_content = TSX_PATH.read_text(encoding="utf-8")
    existing_ids = [int(m) for m in re.findall(r'<tile id="(\d+)"', tsx_content)]
    next_id = max(existing_ids) + 1 if existing_ids else 0

    # Generate images
    to_add: list[tuple[str, str]] = []
    for filename, terrain_type, generator_fn in _VARIANTS:
        out_path = OUTPUT_DIR / filename
        img = generator_fn()
        img.save(out_path)
        print(f"[ok] {out_path.name}")
        to_add.append((filename, terrain_type))

    # Patch the tileset
    print("\nPatching CabinInterior.tsx …")
    _patch_tsx(to_add, next_id)

    # Register in the Content Pipeline
    print("\nPatching Content.mgcb …")
    _patch_mgcb([f for f, _ in to_add])

    print("Done.")


if __name__ == "__main__":
    main()
