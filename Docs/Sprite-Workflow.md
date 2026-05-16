# Sprite Workflow

## Recommended Tools

Start simple:

- Aseprite: best paid option for pixel art.
- LibreSprite: free Aseprite-like option.
- Krita: free option for painted or hand-drawn 2D art.
- Piskel: simple browser-based pixel art tool.

## Folder Structure

- `Assets/Art/Source`: editable source files, such as `.aseprite`, `.kra`, or layered files.
- `Assets/Art/Sprites`: exported character and object PNG files.
- `Assets/Art/Tiles`: exported tile PNG files or tilesets.
- `Assets/Art/Animations`: exported sprite sheets or frame sequences.

## Beginner Rules

- Use PNG for sprites.
- Use transparent backgrounds for characters and objects.
- Start at `32x32` or `64x64` pixels for pixel art.
- Keep one consistent pixel size for the game at first.
- Name files clearly, like `player_idle.png`, `player_walk_down_0.png`, or `coin_spin_sheet.png`.
- In Unity, set pixel art sprites to `Filter Mode: Point`.
- Set `Pixels Per Unit` consistently. This template uses `32`.

## Good First Sprite Set

For the next step, make only these:

- `player_idle_down.png`
- `player_walk_down_0.png`
- `player_walk_down_1.png`
- `wall_block.png`
- `floor_tile.png`
- `coin.png`

That is enough to build movement, collisions, collectibles, and a tiny first room.

## Our Process

1. Decide the art style.
2. Make temporary placeholder sprites.
3. Build gameplay with placeholders.
4. Replace placeholders with nicer art later.
5. Only animate once movement and collisions feel good.

Gameplay should become fun before the art becomes detailed.
