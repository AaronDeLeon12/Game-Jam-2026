# Editor authoring workflow

## Building World1

Open `Assets/Scenes/World/World1.unity`. Build the world from scene objects and prefab instances. Keep the scene hierarchy grouped into terrain, backgrounds, decoration, actors, interactables, triggers, lighting, and camera bounds.

Reusable content belongs under `Assets/Prefabs`:

- `Characters`: player and future character prefabs.
- `Enemies`: enemy and boss prefabs.
- `Environment`: scenery and room dressing.
- `Interactables`: doors, vendors, checkpoints, and pickups.
- `Projectiles`: player and enemy projectiles.
- `UI`: saved Canvas prefabs.
- `VFX`: temporary visual-effect prefabs.

## Player and shared gameplay

Edit `Assets/Prefabs/Characters/Player.prefab` for the player itself. The player is nested in `Assets/Resources/Prefabs/GameplayRig.prefab`; edits applied to the player prefab flow into the rig.

The GameplayRig holds persistent systems shared by gameplay scenes. Do not unpack it in ordinary room scenes. Scene-specific content, spawn points, and camera bounds stay outside the rig.

## Art and animation

Keep source artwork under `Assets/Art/Source`, game-ready sprites under `Assets/Art/Sprites` or `Assets/Art/Authored`, and Animator assets under `Assets/Animations`. Preview and edit animation clips through Unity's Animator and Animation windows.

## Runtime Resources

Do not move files out of `Assets/Resources` casually. Code loads several fonts, audio clips, definitions, legacy sprite sheets, and the GameplayRig by resource path. Ordinary scene prefabs should use `Assets/Prefabs` instead.

## Archived files

Completed migration builders, legacy prototype scripts, recovery scenes, and old verification artifacts are stored outside `Assets` under `ProjectArchive`. Unity does not compile or import that folder. It exists only as a local recovery point and can be removed later after the cleaned project has been exercised for a while.
