"""Generate wall-frame border strips for indoor cabin rooms.

Produces 12 px-wide strips (horizontal and vertical) plus corner and
door-end pieces.  The cross-section looks like a hollow wall: dark brown
wood on the outer and inner faces with a black cavity in the centre.

All images are saved to ``src/DogDays.Game/Content/Tilesets/``.
"""

from __future__ import annotations

import pathlib

try:
    from PIL import Image
except ImportError as exc:
    raise SystemExit(
        "Pillow is required.  Install it in the repo virtual environment "
        "before running this script."
    ) from exc


# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

TILE = 32          # game tile size
BORDER = 12        # wall-frame thickness in pixels

BASE_DIR = pathlib.Path(__file__).resolve().parents[2]
OUTPUT_DIR = BASE_DIR / "src" / "DogDays.Game" / "Content" / "Tilesets"

# Warm-brown plank palette — hollow wall cross-section
WOOD_OUTER = (100, 70, 40)       # outer face (away from room)
WOOD_OUTER_LIGHT = (120, 88, 52) # outer face highlight
WOOD_INNER = (115, 82, 48)       # inner face (room-facing)
WOOD_INNER_LIGHT = (140, 108, 68)# inner face highlight edge
CAVITY = (8, 6, 4)               # near-black hollow centre
BLACK = (0, 0, 0, 255)


def clamp(v: int) -> int:
    return max(0, min(255, v))


def _wood_pixel(base: tuple[int, int, int], x: int, y: int, seed: int) -> tuple[int, int, int]:
    """Add subtle grain variation to a base colour."""
    grain = ((x * 7 + y * 3 + seed * 11) % 13) - 6
    return (
        clamp(base[0] + grain),
        clamp(base[1] + grain // 2),
        clamp(base[2] + grain // 3),
    )


# ---------------------------------------------------------------------------
# Gradient helpers — define how colour transitions across the 12 px depth
# ---------------------------------------------------------------------------

def _colour_at_depth(depth: int, max_depth: int) -> tuple[int, int, int]:
    """Return an RGB colour for a given depth (0 = outer edge, max_depth-1 = inner/room edge).

    Hollow-wall cross section:
        0..2         → outer wood face (dark brown, slight highlight at 2)
        3..max-4     → black cavity
        max-3..max-1 → inner wood face (brown, lightest at room edge)
    """
    def _lerp(a: tuple[int, int, int], b: tuple[int, int, int], t: float) -> tuple[int, int, int]:
        return (
            int(a[0] + t * (b[0] - a[0])),
            int(a[1] + t * (b[1] - a[1])),
            int(a[2] + t * (b[2] - a[2])),
        )

    outer_thickness = 3
    inner_thickness = 3

    if depth < outer_thickness:
        # Outer wood face: darkest at edge, slightly lighter toward cavity
        t = depth / (outer_thickness - 1)
        return _lerp(WOOD_OUTER, WOOD_OUTER_LIGHT, t)
    elif depth >= max_depth - inner_thickness:
        # Inner wood face: darker toward cavity, lightest at room edge
        t = (depth - (max_depth - inner_thickness)) / (inner_thickness - 1)
        return _lerp(WOOD_INNER, WOOD_INNER_LIGHT, t)
    else:
        # Black cavity
        return CAVITY


# ---------------------------------------------------------------------------
# Straight strip generators
# ---------------------------------------------------------------------------

def make_horizontal_strip(side: str) -> Image.Image:
    """Return a TILE×BORDER strip.

    *side* is ``"top"`` or ``"bottom"``.
    For ``"top"`` the outer (black) edge is at ``y=0``.
    For ``"bottom"`` the outer edge is at ``y=BORDER-1``.
    """
    img = Image.new("RGBA", (TILE, BORDER))
    px = img.load()
    for y in range(BORDER):
        depth = y if side == "top" else (BORDER - 1 - y)
        for x in range(TILE):
            base = _colour_at_depth(depth, BORDER)
            r, g, b = _wood_pixel(base, x, y, seed=42)
            px[x, y] = (r, g, b, 255)
    return img


def make_vertical_strip(side: str) -> Image.Image:
    """Return a BORDER×TILE strip.

    *side* is ``"left"`` or ``"right"``.
    """
    img = Image.new("RGBA", (BORDER, TILE))
    px = img.load()
    for x in range(BORDER):
        depth = x if side == "left" else (BORDER - 1 - x)
        for y in range(TILE):
            base = _colour_at_depth(depth, BORDER)
            r, g, b = _wood_pixel(base, x, y, seed=57)
            px[x, y] = (r, g, b, 255)
    return img


# ---------------------------------------------------------------------------
# Corner generators
# ---------------------------------------------------------------------------

def make_corner(h_side: str, v_side: str) -> Image.Image:
    """Return a BORDER×BORDER corner piece.

    *h_side*: ``"left"`` or ``"right"``
    *v_side*: ``"top"`` or ``"bottom"``

    The corner blends the two gradients together using the minimum depth
    of the horizontal and vertical distances to the outer edge.
    """
    img = Image.new("RGBA", (BORDER, BORDER))
    px = img.load()
    for y in range(BORDER):
        for x in range(BORDER):
            dx = x if h_side == "left" else (BORDER - 1 - x)
            dy = y if v_side == "top" else (BORDER - 1 - y)
            depth = min(dx, dy)
            base = _colour_at_depth(depth, BORDER)
            r, g, b = _wood_pixel(base, x, y, seed=73)
            px[x, y] = (r, g, b, 255)
    return img


# ---------------------------------------------------------------------------
# Door-end caps
# ---------------------------------------------------------------------------

def make_door_end_horizontal(end: str) -> Image.Image:
    """Return a BORDER×BORDER cap placed where the bottom strip terminates at a doorway.

    *end*: ``"left"`` means the door-opening is to the right (frame ends on left side).
    ``"right"`` means the door-opening is to the left.
    """
    img = Image.new("RGBA", (BORDER, BORDER))
    px = img.load()
    for y in range(BORDER):
        dy = BORDER - 1 - y  # bottom strip: outer at bottom
        for x in range(BORDER):
            dx = x if end == "right" else (BORDER - 1 - x)
            depth = min(dx, dy)
            base = _colour_at_depth(depth, BORDER)
            r, g, b = _wood_pixel(base, x, y, seed=88)
            px[x, y] = (r, g, b, 255)
    return img


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

OUTPUTS: list[tuple[str, callable]] = [
    # Straight strips
    ("cabin-frame-top.png",    lambda: make_horizontal_strip("top")),
    ("cabin-frame-bottom.png", lambda: make_horizontal_strip("bottom")),
    ("cabin-frame-left.png",   lambda: make_vertical_strip("left")),
    ("cabin-frame-right.png",  lambda: make_vertical_strip("right")),
    # Corners
    ("cabin-frame-corner-tl.png", lambda: make_corner("left", "top")),
    ("cabin-frame-corner-tr.png", lambda: make_corner("right", "top")),
    ("cabin-frame-corner-bl.png", lambda: make_corner("left", "bottom")),
    ("cabin-frame-corner-br.png", lambda: make_corner("right", "bottom")),
    # Door-end caps (bottom wall only for now)
    ("cabin-frame-door-end-left.png",  lambda: make_door_end_horizontal("left")),
    ("cabin-frame-door-end-right.png", lambda: make_door_end_horizontal("right")),
]


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, factory in OUTPUTS:
        path = OUTPUT_DIR / name
        factory().save(path, format="PNG")
        print(f"  wrote {path.relative_to(BASE_DIR)}")
    print(f"\nDone — {len(OUTPUTS)} wall-frame assets generated.")


if __name__ == "__main__":
    main()
