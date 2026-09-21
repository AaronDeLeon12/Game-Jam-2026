# Tales of Ivory Moss Asset Bible

This is the source of truth for art and audio requests. Add new assets here before importing or generating them.

## Import Defaults

- Character sprites: `Sprite (2D and UI)`, `Multiple` when animated, pivot at feet/bottom center, `Pixels Per Unit = 32`, no mipmaps, point filter for pixel art or bilinear for painted art.
- Spell/VFX sprites: pivot center, transparent background, consistent canvas size per animation cycle, no text from source sheets inside the crop.
- UI frames: transparent PNG, 9-slice ready when possible, no baked white/cream background unless it is part of the design.
- Backgrounds: large painted layers, no colliders on the visual object, separate collision/terrain object in scene.
- Terrain: tileable visual pieces plus separate physics prefabs for floor/platform/wall.
- Audio: music loops normalized separately from SFX; all clips named by use, not by download filename.

## Sorting Layers Proposal

- Background
- TerrainBack
- Terrain
- NPC
- Enemy
- Player
- VFX
- UIWorld

## Missing Asset Checklist

| Area | Asset | Current State | Target | Status |
| --- | --- | --- | --- | --- |
| Player | idle/walk/jump/crouch/death/spell attack | mixed imported sheets | sliced Animator Controller frames | Needs cleanup |
| Player | teleport VFX | runtime animation | prefabbed VFX with appear/disappear anchors | Needs prefab |
| Enemies | mantis | current working enemy | prefab + animator + tuned collider | In progress |
| Enemies | fairy | frame files | prefab + Animator Controller | Needs cleanup |
| Enemies | unicorn shooter | frame files | prefab + attack VFX prefab | Needs cleanup |
| Boss | boss body/attack/projectile | runtime resources | boss prefab + attack states | Needs conversion |
| NPC | vendor | runtime-created in outside scene | placed prefab in scene | Needs scene authoring |
| NPC | deer/cat/other dialogue NPCs | mixed | dialogue prefab variants | Needs catalog |
| UI | HUD bars | Canvas foundation started | fully prefabbed HUD with fill/flash/images | In progress |
| UI | menus | IMGUI | Canvas prefabs | Needs conversion |
| Backgrounds | main menu | imported image | final menu art + logo layout | Placeholder |
| Backgrounds | house interior/exterior | imported images | layered scene backgrounds | Needs scene kits |
| Terrain | outside floor/platforms | mixed runtime/scene | tileable terrain prefabs | Needs art |
| VFX | square/triangle/shield/knife | runtime-created | prefabbed VFX and hitboxes | Needs conversion |
| Audio | music/SFX | imported resources | audio event table + mixer groups | Needs mixer |

## Naming Rules

- Sprites: `character_action_01`, `enemy_mantis_walk_01`, `vfx_square_cast_01`.
- Prefabs: `PF_Player`, `PF_Enemy_Mantis`, `PF_UI_GameHud`, `PF_VFX_SquareProjectile`.
- ScriptableObjects: `SO_Spell_Square`, `SO_Enemy_Mantis`, `SO_Difficulty_Normal`.
