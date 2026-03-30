from __future__ import annotations

import pathlib
import random
import struct
import zlib


TILE_SIZE = 32
OUTPUT_NAMES = [
    "cabin-wood-floor-1.png",
    "cabin-wood-floor-2.png",
    "cabin-wood-floor-3.png",
    "cabin-wood-floor-4.png",
]
BASE_DIR = pathlib.Path(__file__).resolve().parents[2]
OUTPUT_DIR = BASE_DIR / "src" / "RiverRats.Game" / "Content" / "Tilesets"


def clamp(value: int) -> int:
    return max(0, min(255, value))


def make_tile(seed: int) -> bytes:
    rng = random.Random(seed)
    rows = []
    plank_widths = [8, 9, 7, 8]
    plank_offsets = [rng.randint(-3, 3) for _ in plank_widths]
    seam_rows_by_plank = []

    plank_edges = []
    cursor = 0
    for plank_index, width in enumerate(plank_widths):
        plank_edges.append((cursor, min(TILE_SIZE, cursor + width)))
        seam_count = 1 + ((seed + plank_index) % 2)
        candidate_rows = list(range(7 + ((seed + plank_index) % 3), TILE_SIZE - 6, 8))
        rng.shuffle(candidate_rows)
        seam_rows = sorted(candidate_rows[:seam_count])
        seam_rows_by_plank.append(seam_rows)
        cursor += width

    for y in range(TILE_SIZE):
        row = bytearray([0])
        for x in range(TILE_SIZE):
            plank_index = 0
            for index, (left, right) in enumerate(plank_edges):
                if left <= x < right:
                    plank_index = index
                    break

            base_r = 154 + plank_offsets[plank_index] + plank_index
            base_g = 124 + (plank_offsets[plank_index] // 2)
            base_b = 79 + (plank_index // 3)

            grain = ((y * (6 + plank_index)) + x + (seed * 9)) % 13
            grain_adjust = grain - 7

            r = base_r + grain_adjust
            g = base_g + (grain_adjust // 3)
            b = base_b + (grain_adjust // 5)

            if any(abs(x - edge[0]) <= 0 or abs(x - edge[1] + 1) <= 0 for edge in plank_edges[1:]):
                r -= 10
                g -= 8
                b -= 6

            for seam_y in seam_rows_by_plank[plank_index]:
                if abs(y - seam_y) <= 0:
                    r -= 14
                    g -= 11
                    b -= 8
                elif abs(y - seam_y) == 1:
                    r -= 5
                    g -= 4
                    b -= 3

            if (y + (seed * 3) + (plank_index * 5)) % 9 in (0, 1):
                r += 3
                g += 2
                b += 1

            if y in (0, TILE_SIZE - 1):
                r -= 4
                g -= 3
                b -= 2

            row.extend((clamp(r), clamp(g), clamp(b), 255))
        rows.append(bytes(row))

    return b"".join(rows)


def png_chunk(chunk_type: bytes, data: bytes) -> bytes:
    return (
        struct.pack(">I", len(data))
        + chunk_type
        + data
        + struct.pack(">I", zlib.crc32(chunk_type + data) & 0xFFFFFFFF)
    )


def write_png(path: pathlib.Path, rgba_data: bytes) -> None:
    ihdr = struct.pack(">IIBBBBB", TILE_SIZE, TILE_SIZE, 8, 6, 0, 0, 0)
    compressed = zlib.compress(rgba_data, level=9)
    png_bytes = b"\x89PNG\r\n\x1a\n"
    png_bytes += png_chunk(b"IHDR", ihdr)
    png_bytes += png_chunk(b"IDAT", compressed)
    png_bytes += png_chunk(b"IEND", b"")
    path.write_bytes(png_bytes)


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for seed, file_name in enumerate(OUTPUT_NAMES, start=1):
        write_png(OUTPUT_DIR / file_name, make_tile(seed))


if __name__ == "__main__":
    main()