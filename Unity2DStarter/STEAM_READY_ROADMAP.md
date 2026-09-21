# Steam-Ready Roadmap

## Phase 1 - Build Stability
- Keep `main` playable.
- Use release tags such as `v0.1.0-beta`.
- Build Windows x64 from the editor menu instead of hand-copying files.
- Keep a known issues list for every shared beta.

## Phase 2 - Unity Project Cleanup
- Replace runtime-created gameplay objects with prefabs.
- Move player, enemies, projectiles, spell effects, doors, save vendor, and terrain chunks into prefab assets.
- Replace runtime sprite slicing with Sprite Editor slicing and Animator Controllers.
- Keep character pivots grounded and spell pivots centered.
- Move menus, HUD, dialogue, save/load, pause, and death screens from `OnGUI` to Canvas.

## Phase 3 - Data And Tuning
- Move spell, enemy, and difficulty numbers into ScriptableObjects.
- Cache or serialize asset references instead of loading them repeatedly from `Resources`.
- Add object pooling for projectiles, hitboxes, shield effects, enemy shots, and Wrath Mode spawns.
- Generate Wrath Mode chunks over multiple frames.

## Phase 4 - Saves And QA
- Keep saves versioned.
- Save stable scene object IDs for defeated enemies and story flags.
- Add playtime, checkpoint/save-machine ID, and future inventory/story data.
- Add smoke tests for New Game, Load Game, Wrath Mode, vendor save, death menu, and boss fight.

## Phase 5 - Steam Prep
- Prepare capsule art, app icon, screenshots, trailer clips, short description, and credits/license notes.
- Add resolution, fullscreen/windowed, music volume, SFX volume, and control settings.
- Plan Steam Cloud around Unity `Application.persistentDataPath`.
- Later, connect action counters to Steam stats and achievements.
