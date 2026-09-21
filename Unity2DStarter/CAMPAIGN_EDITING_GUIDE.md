# Editing the campaign in Unity

Open the campaign scenes under `Assets/Scenes/Campaign` in Edit Mode. The player rig, scenery, collision, NPCs, doors, lights and encounters are saved scene objects. Campaign controllers no longer construct the world at startup. `Assets/Scenes/Menu/MainMenu.unity` contains the saved menu Canvas. The larger hand-built world belongs under `Assets/Scenes/World`, starting with `World1.unity`.

## Player

Open `Assets/Prefabs/Characters/Player.prefab` in Prefab Mode. Edit its collider, physics material, component settings, and the `Player Visual` transform. The nested `Animated Sprite` is controlled by clips; scale or offset its parent to change the character without fighting animation curves.

The player is nested inside `Assets/Resources/Prefabs/GameplayRig.prefab`, which is placed in every campaign scene. The rig persists across level transitions to preserve health, mana and action counters. Apply shared changes to the player prefab. A scene-only player override applies when starting a fresh run in that scene, not when an existing player travels into it.

Tune health/mana in `Assets/Resources/Definitions/PlayerVitals.asset`, movement in `PlayerMovement.asset`, and shared combat behavior in `PlayerCombat.asset`. Individual `Square`, `Triangle`, `Circle`, and `Knife` assets own spell costs/cooldowns/damage and effect prefab references. Assigned definitions take precedence over the legacy fallback values on components.

Open `Assets/Animations/Authored/Player.controller` in Animator. Its named states are selected by the existing movement/combat behavior. Edit the referenced clips in the Animation window; retain state names when changing art. Walk, jump, crouch, crouch walk, teleport, spell and death have saved sprite clips. Enemy/NPC controllers and clips are in the same folder.

## Campaign layout and encounters

Move objects directly in the Hierarchy and Scene view, then save the scene. Prefabs are under `Assets/Prefabs/Enemies`, `Environment`, `Interactables`, `Projectiles` and `UI`. Drag them into a scene to add content. Original floor/wall artwork and pre-existing mantis/unicorn placements were retained.

`CampaignDayContent` parents reference existing objects and expose the first/last active day. Runtime activation uses the actual day; day content can also be inspected by enabling its authored parent in the Hierarchy.

The campaign starts with no day-gated enemies on day 1, then mantis on day 2, unicorn on day 3, and fairy on day 4. Existing placements are reused. Day 5 routes to the boss scene. Scene enemy save IDs are serialized so moving those existing objects does not silently change their identity. Give newly duplicated enemies a distinct ID in `EnemySaveTracker`.

Floor and wall colliders are saved. Their context-menu **Apply** commands are optional authoring tools; they no longer resize colliders automatically during Play. Moving/resizing a floor sprite does not automatically resize its collider: edit the collider or explicitly apply the floor tool.

## Canvas UI

Edit `Assets/Prefabs/UI/MainMenu.prefab`, `GameplayMenus.prefab`, and `GameHUD.prefab`. Buttons, labels, bars, dialogue, confirmation, save/load and death panels are actual RectTransforms. Select inactive panels in Prefab Mode and enable them temporarily to preview their layout. Retain serialized controller references when replacing controls.

The gameplay rig contains one EventSystem. Main menu, pause, settings, dialogue, doors, vendor save UI and the HUD use the saved Canvases in campaign play. Older IMGUI helpers remain for legacy test content but are bypassed by the campaign's Canvas controllers.

## Art and tuning

`Assets/Art/Authored` contains saved, transparent animation frames and UI assets baked from the existing artwork. They use point filtering, no mipmaps, uncompressed alpha and no runtime CPU-read requirement. Imported source sprite sheets remain available for procedural legacy content. The importer now applies defaults only to new assets, so reimporting no longer destroys artist-selected slicing and pixels-per-unit values.

Enemy and difficulty tuning assets are under `Assets/Resources/Definitions`. Prefabs reference the enemy data they consume; `CampaignMantis` preserves the placed mantis's settings. The three difficulty assets are loaded by `DifficultyRules`.

Wrath Mode retains procedural chunk generation. It shares the editable player rig and can still use its existing procedural enemy behaviors.

## Archived migration material

The one-time conversion builders and their verification output are no longer part of `Assets`, so Unity does not import or compile them. A recoverable local copy is stored under `ProjectArchive`. Normal game development should use the saved scenes, prefabs, Animator Controllers, and Inspector fields directly.

If Unity has one of these scenes open while files are refreshed, reload it from disk to see the saved hierarchy before making further edits.
