# Unity 2D Beginner Workflow

## Goal

Start with a tiny playable project: one player, side-view platformer movement, solid platforms, gravity, and Space-to-jump.

## First concepts to know

- A `Scene` is a level or screen.
- A `GameObject` is anything in the scene: player, camera, wall, enemy, item.
- A `Component` gives behavior to a GameObject.
- A `Script` is a component you write in C#.
- `Rigidbody2D` lets Unity move an object through the 2D physics system.
- `Collider2D` gives an object a physical shape for collisions.
- `Update` runs every rendered frame.
- `FixedUpdate` runs on the physics timer and is best for Rigidbody movement.

## Clean project structure

- `Assets/Scenes`: Unity scenes.
- `Assets/Scripts`: C# scripts.
- `Assets/Art`: sprites, textures, and visual assets.
- `Assets/Editor`: Unity editor automation scripts.
- `Packages`: package list.
- `ProjectSettings`: Unity project settings.

## Step-by-step first session

1. Open Unity Hub.
2. Add the `Unity2DStarter` folder as an existing project.
3. Open it with Unity `6000.4.6f1`.
4. Wait for import to finish.
5. Open `Assets/Scenes/StarterScene.unity`.
6. Press Play.
7. Test movement with A/D or left/right arrow keys.
8. Jump with Space.
8. Select the Player object.
9. In the Inspector, change `Move Speed` on `PlayerMovement2D`.
10. Press Play again and feel the difference.

## Your first safe experiments

- Change the player color in `StarterSceneBuilder.cs`.
- Change `moveSpeed` in the Player inspector.
- Resize the `Play Area`.
- Add a new sprite for a wall.
- Add a `BoxCollider2D` to the wall.

## Recommended learning order

1. Move a player.
2. Add walls and collision.
3. Add collectibles.
4. Add a score UI.
5. Add enemies.
6. Add a start menu.
7. Add sound.
8. Build a small level.

## Questions to answer before the next build step

- Should movement be keyboard-only, controller, or both?
- Should the player collide with walls?
- Should the camera follow freely or stay inside level bounds?
- What is the first tiny objective: collect coins, avoid enemies, reach an exit, survive a timer, or explore?
