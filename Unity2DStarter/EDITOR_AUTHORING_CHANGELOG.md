# Editor Authoring Conversion Changelog

This document records the Unity authoring conversion and follow-up fixes prepared after commit `d69a7a0`. The goal is to make campaign development happen through scenes, prefabs, Animator assets, and Inspector values while leaving runtime code responsible for behavior.

## Authoring model

- Campaign scenes are saved Unity scenes that can be opened, inspected, and changed without entering Play Mode.
- Reusable gameplay objects are prefab assets with serialized Inspector fields.
- `GameplayRig` contains the shared player, camera, HUD, gameplay menus, event system, and persistent gameplay services.
- The player is a nested instance of `Assets/Prefabs/Characters/Player.prefab`; gameplay tuning should be changed on that source prefab.
- Wrath Mode can continue to create procedural encounters. Campaign and `World1` content should remain hand-authored.

## Scene organization

Scenes are grouped by purpose:

- `Assets/Scenes/Menu/MainMenu.unity`
- `Assets/Scenes/Campaign/home_day_1.unity`
- `Assets/Scenes/Campaign/outside_1.unity`
- `Assets/Scenes/Campaign/boss_fight_scenario.unity`
- `Assets/Scenes/Modes/SurvivalTesting.unity`
- `Assets/Scenes/World/World1.unity`

`EditorBuildSettings.asset` uses the reorganized menu, campaign, and mode paths. `World1` remains outside the build list until its playable route is ready.

The campaign scenes contain editable level content, spawn markers, lighting, backgrounds, NPCs, interactables, enemies, and camera boundaries. Scene-specific objects remain outside `GameplayRig`.

## Prefabs and assets

Reusable objects are organized under `Assets/Prefabs`:

- `Characters` for the player and future playable characters.
- `Enemies` for campaign enemy and boss instances.
- `Environment`, `Terrain`, and `SceneKits` for world-building pieces.
- `Interactables` and `NPCs` for dialogue, doors, vendors, and checkpoints.
- `Projectiles`, `VFX`, and `UI` for reusable effects and interface elements.

`Assets/Resources/Prefabs/GameplayRig.prefab` intentionally remains in `Resources`. `SystemsBootstrap` loads it through the stable `Prefabs/GameplayRig` resource path when a gameplay scene is opened directly.

Unity `.meta` files moved with their assets, preserving GUID references between scenes, nested prefabs, scripts, materials, and sprites.

## Player editing

`Assets/Prefabs/Characters/Player.prefab` is the authoritative source for player movement, dash, health, mana, and combat tuning.

The previous setup had multiple competing value sources:

- Player component values stored on the prefab.
- Player definition assets copied over those values during `Awake`.
- Additional gameplay overrides on the Player nested inside `GameplayRig`.

Those runtime copies and nested gameplay overrides were removed. Values edited on the Player prefab now remain unchanged when Play Mode begins. Existing intended overrides were applied to the source prefab:

- Dash distance: `6`
- Glide gravity scale: `1.1`
- Circle shield duration: `3`

Spell definition assets still provide authored projectile and effect prefab references. They no longer silently replace numeric Player component tuning.

The Player shown below Unity's `DontDestroyOnLoad` heading during Play Mode is the live instance moved there by `GameplayRig`. Permanent changes belong on the source Player prefab before entering Play Mode.

## Canvas UI

Gameplay menus and HUD are authored Canvas prefabs instead of an interface assembled at runtime. The current assets retain the illustrated style, button colors, readable text sizing, and compact lower-screen dialogue layout.

The authored UI covers:

- Main menu and difficulty selection.
- Settings and confirmation panels.
- HUD health and mana presentation.
- Pause and game-over menus.
- Save and load slots.
- Save vendor flow.
- NPC dialogue and interaction prompts.

Dialogue text contains only the dialogue content. The dedicated right-side label displays `E - continue`.

## Runtime flow fixes

### Returning to Main Menu

The gameplay rig is torn down before loading `MainMenu`. This prevents the persistent `LevelManager` from deleting the menu's authored camera and then removing its own camera, which previously produced Unity's `Display 1 / No cameras rendering` message.

Both the game-over flow and the pause menu use the corrected teardown order.

### Loading after death

Loading a save restores a playable state by closing the active modal, restoring `Time.timeScale`, resuming the audio listener, and applying living player stats. This prevents the game from remaining frozen after loading from the death screen.

### NPC dialogue

Ending an NPC conversation now waits for the interaction key to be released. The E press that closes the final line can no longer reach `PlayerInteract` in the same frame and immediately reopen the conversation.

The duplicate `E - continue` text was removed from the dialogue message. The authored right-side hint remains in `GameplayMenus.prefab`.

## Code and folder organization

Runtime scripts are grouped under:

- `Assets/Scripts/Animation`
- `Assets/Scripts/Combat`
- `Assets/Scripts/Core`
- `Assets/Scripts/Data`
- `Assets/Scripts/Enemies`
- `Assets/Scripts/Player`
- `Assets/Scripts/Rendering`
- `Assets/Scripts/UI`
- `Assets/Scripts/World`

Editor-only tools are grouped under `Assets/Editor/Authoring` and `Assets/Editor/Build`. Rendering settings are under `Assets/Settings`, and UI art is under `Assets/Art/UI`.

Completed one-time migration tools, prototype builders, obsolete runtime scripts, recovery scenes, and verification output were removed from Unity's active `Assets` tree. A recoverable local copy is kept in the ignored `ProjectArchive` directory and is not imported, compiled, or committed.

## Validation performed

- Unity runtime and editor assemblies compiled successfully after the reorganization.
- A fresh Roslyn compilation using Unity's generated response file passed after the Player and dialogue fixes.
- All enabled build-scene paths exist.
- No serialized `m_Script: {fileID: 0}` references were found in the active scene and prefab set.
- No duplicate active asset GUIDs were found.
- Active assets contain no references to the archived runtime-script GUIDs.
- `GameplayRig` retains its nested Player prefab link.
- No Player movement, health, mana, or combat overrides remain on `GameplayRig` or campaign scenes.
- No stale pre-reorganization scene, Player prefab, logo, or render-settings paths remain in active project configuration.

## Editing references

- `README.md` explains the current project structure and `GameplayRig`.
- `CAMPAIGN_EDITING_GUIDE.md` describes campaign scene authoring.
- `EDITOR_AUTHORING_WORKFLOW.md` describes the prefab and `World1` workflow.
- `PROFESSIONAL_BASE_TODO.md` lists the next development phases for `World1`, layers, region scenes, persistent world state, and remaining data work.
