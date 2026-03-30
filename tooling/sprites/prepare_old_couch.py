"""
Prepares old-couch.png for use as a game prop:
  - Source is already RGBA — just trims transparent border pixels.
  - Resizes to exactly 96 px tall (3 tiles), preserving aspect ratio.
  - Saves as old-couch.png in the game's Content/Sprites folder.
"""
from PIL import Image

SRC = r"C:\Users\Luc\git\RiverRats\tooling\sprites\old-couch.png"
DST = r"C:\Users\Luc\git\RiverRats\src\RiverRats.Game\Content\Sprites\old-couch.png"

TARGET_HEIGHT = 80  # 2.5 tiles × 32 px


def main() -> None:
    img = Image.open(SRC).convert("RGBA")
    print(f"Source size: {img.size[0]}x{img.size[1]}")

    bbox = img.getbbox()
    if bbox is None:
        raise ValueError("Image is fully transparent.")
    img = img.crop(bbox)
    print(f"Trimmed size: {img.size[0]}x{img.size[1]}")

    tw, th = img.size
    scale = TARGET_HEIGHT / th
    new_w = max(1, round(tw * scale))
    img = img.resize((new_w, TARGET_HEIGHT), Image.NEAREST)
    print(f"Final size: {img.size[0]}x{img.size[1]}")

    img.save(DST)
    print(f"Saved to: {DST}")


if __name__ == "__main__":
    main()
