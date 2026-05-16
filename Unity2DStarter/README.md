# Unity2DStarter

A small Unity 2D starter project for learning top-down player movement.

## What opens first

When you open this folder in Unity, the editor script in `Assets/Editor/StarterSceneBuilder.cs` automatically creates:

- `Assets/Scenes/StarterScene.unity`
- a blue `Player` object
- a dark `Play Area`
- a `Main Camera` that follows the player
- keyboard movement using WASD or arrow keys

## How to play

1. Open Unity Hub.
2. Choose `Add` or `Open`.
3. Select this folder: `Unity2DStarter`.
4. Wait for Unity to import packages.
5. Open `Assets/Scenes/StarterScene.unity` if it is not already open.
6. Press the Play button.
7. Move left/right with A/D or arrow keys.
8. Jump with Space.
9. Cast a short spell projectile with right click.

## Main files

- `Assets/Scripts/PlayerMovement2D.cs`: player movement.
- `Assets/Scripts/CameraFollow2D.cs`: smooth camera follow.
- `Assets/Scripts/PlayerStats.cs`: health, mana, spell cost, and mana regeneration.
- `Assets/Scripts/PlayerCombat.cs`: right-click spell casting.
- `Assets/Scripts/EnemyDummy.cs`: three-hit respawning dummy enemy.
- `Assets/Scripts/GameHud.cs`: placeholder HP, mana, and game-over overlay.
- `Assets/Scripts/GameTemplateBootstrap.cs`: keeps the scene wired as the gameplay template.
- `Assets/Editor/StarterSceneBuilder.cs`: creates the starter scene.

## Art folders

- `Assets/Art/Source`: editable art files.
- `Assets/Art/Sprites`: exported character/object sprites.
- `Assets/Art/Tiles`: tiles and tilesets.
- `Assets/Art/Animations`: sprite sheets or animation frames.
