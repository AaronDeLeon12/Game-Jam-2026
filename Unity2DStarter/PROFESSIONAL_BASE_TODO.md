# Development Roadmap

The campaign foundation is now editor-authored: its scenes, player, HUD, menus, and major interactable objects can be selected and tuned in Unity. Keep that workflow as the project expands into `World1`.

## Next: Build the World1 Foundation

1. Block out `Assets/Scenes/World/World1.unity` with Tilemaps, collision, room boundaries, spawn points, and camera bounds.
2. Divide the large world into additive room or region scenes before it becomes difficult to load and edit. Keep persistent systems in `GameplayRig`; keep level geometry and encounters in their region scenes.
3. Define Sorting Layers for background, environment, actors, foreground, effects, and UI. Use Order in Layer only within each group.
4. Add Unity Layers for player, enemies, terrain, one-way platforms, interactables, triggers, and projectiles, then configure the Physics 2D collision matrix.
5. Build reusable environment prefabs and Tile Palettes for common platforms, doors, hazards, decorations, checkpoints, and room transitions.

## Content and Code Cleanup

1. Move remaining combat and character tuning values into focused ScriptableObjects, grouped by player abilities, enemy types, and encounters.
2. Replace remaining legacy runtime UI fallbacks after every active caller uses the authored Canvas prefabs.
3. Add persistent world-state data for opened doors, defeated bosses, collected upgrades, checkpoints, and NPC progression.
4. Split large behaviour scripts by responsibility when they become hard to inspect or test. Keep scene construction out of runtime behaviour.
5. Create Animator Controllers and reusable animation clips for important characters so transitions can be previewed and tuned in Unity.

## World Expansion Checks

- Each room can be opened and edited without entering Play Mode.
- Important objects are prefab instances with useful Inspector fields.
- Scene transitions identify destinations by stable IDs and spawn markers.
- Saving restores the correct scene, checkpoint, abilities, and persistent world changes.
- Camera bounds, sorting, collisions, and lighting are visible in the editor.
- Wrath Mode may remain procedural; campaign and World1 content stays hand-authored.

## Folder Rules

- Put playable scenes under `Assets/Scenes/Campaign`, `World`, or `Modes`.
- Put reusable objects in the matching `Assets/Prefabs` category.
- Put runtime scripts in the matching `Assets/Scripts` category.
- Put editor-only tools under `Assets/Editor/Authoring` or `Assets/Editor/Build`.
- Keep only assets loaded by `Resources.Load` inside `Assets/Resources`.
- Keep retired material in `ProjectArchive` until the reorganized project has been fully play-tested.
