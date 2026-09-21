# Tales of Ivory Moss

Unity 2D action-adventure project built around editor-authored scenes, reusable prefabs, Animator assets, and Inspector-driven tuning.

## Start here

Open `Assets/Scenes/Menu/MainMenu.unity` to run the normal game flow. Campaign scenes are in `Assets/Scenes/Campaign`; Wrath Mode is in `Assets/Scenes/Modes`; the larger hand-built world begins in `Assets/Scenes/World/World1.unity`.

## Main project folders

- `Assets/Scenes`: scenes grouped into Menu, Campaign, Modes, and World.
- `Assets/Prefabs`: reusable Characters, Enemies, Environment, Interactables, Projectiles, UI, and VFX.
- `Assets/Scripts`: runtime code grouped by responsibility.
- `Assets/Animations`: Animator Controllers and animation clips.
- `Assets/Art`: imported and authored visual assets.
- `Assets/Resources`: assets intentionally loaded by resource path at runtime.
- `Assets/Editor`: build and authoring utilities only.

## GameplayRig

`Assets/Resources/Prefabs/GameplayRig.prefab` is the persistent gameplay foundation. Campaign scenes contain one instance, and it survives scene changes so the same player state continues between rooms.

It contains:

- `Player`: movement, combat, health/mana, interaction, colliders, and animation.
- `Main Camera`: follows the persistent player and uses scene camera bounds.
- `GameHUD`: health, mana, equipped spell, death menu, and load slots.
- `GameplayMenus`: pause, settings, save/load, confirmation, dialogue, and interaction prompts.
- `EventSystem`: sends keyboard and pointer input to Canvas controls.
- Root controllers: bootstrap references, level transitions, and pause behavior.

The rig belongs under `Resources/Prefabs` because `SystemsBootstrap` can load it by the stable path `Prefabs/GameplayRig` when a gameplay scene is opened directly. The editable player source prefab is under `Assets/Prefabs/Characters/Player.prefab`.

Edit movement, dash, health, mana, and combat tuning on `Assets/Prefabs/Characters/Player.prefab`. The nested Player in `GameplayRig` inherits those values and has no separate gameplay tuning overrides. During Play Mode Unity moves the same rig into `DontDestroyOnLoad`; that runtime object is only a live preview, so permanent changes should be made on the Player prefab before entering Play Mode.

## Editing rules

- Place level geometry, NPCs, doors, enemies, and camera bounds directly in scenes.
- Make reusable objects prefabs and tune their serialized values in the Inspector.
- Keep runtime construction for Wrath Mode, temporary combat effects, and other genuinely dynamic objects.
- Preserve scene names used by transitions even if scenes move between folders.

See `CAMPAIGN_EDITING_GUIDE.md` for the detailed campaign workflow, `EDITOR_AUTHORING_WORKFLOW.md` for the project-wide authoring rules, and `EDITOR_AUTHORING_CHANGELOG.md` for the complete conversion and validation record.
