# Cave Explorer

**Cave Explorer** is a procedurally generated 3D cave exploration game developed in Unity. Players navigate underground caves, avoid enemies, cross lakes, activate crystal checkpoints, and attempt to reach the exit.

## Features

### Procedural Cave Generation
- Uses **Cellular Automata** to generate and smooth randomized cave layouts.
- Flood Fill identifies disconnected regions, which are connected through procedurally carved tunnels.
- Entrance and exit are placed on cave borders, with the exit chosen as the geometrically furthest border position.
- Breadth-First Search (BFS) calculates shortest-path distances from the entrance.

### Underground Lakes
- Lakes are generated through randomized frontier-based growth.
- Generation favors open spaces and avoids flooding narrow corridors.
- Water tiles apply a movement speed penalty.

### Enemies
- Enemies spawn randomly, with spawn probability increasing with distance from the entrance.
- Enemies detect, chase, and attack the player within configurable ranges.
- Enemy distributions regenerate after death when starting a new cave.

### Procedural Crystals & Checkpoints
- Crystals are generated using **L-Systems** and 3D turtle graphics.
- Each crystal has a deterministic random seed, producing varied branching structures.
- Crystals spawn throughout the cave, avoiding lakes and restricted areas.
- Players can activate crystals as checkpoints. The active checkpoint changes from pink to yellow.
- Death respawns the player at the latest activated checkpoint or the entrance.

### Player Movement
- First-person camera and mouse controls.
- Physics-inspired movement with acceleration, friction, jumping, air control, and momentum preservation.
- Supports bunny hopping.
- Movement speed decreases when crossing lakes.

### Game Flow
- **Objective:** Reach the red exit tile while surviving enemy encounters.
- **Death:** Press `Space` to respawn or `N` to generate a new cave.
- **Victory:** Reaching the exit displays a victory screen. Press `N` to play again.
- Generating a new cave resets checkpoints and replaces the previous level.

## Controls

| Input | Action |
|---|---|
| `W A S D` | Move |
| Mouse | Look around |
| `Space` | Jump / Respawn after death |
| `N` | Generate a new cave after death or victory |
| Checkpoint interaction | Activate a crystal checkpoint |

## Technical Highlights

- **Engine:** Unity (C#)
- **Cave generation:** Cellular Automata, Flood Fill, BFS
- **Lake generation:** Randomized frontier expansion
- **Crystals:** L-System rewriting and turtle graphics
- **Enemy AI:** Distance-based spawning and proximity-based chasing
- **Player controller:** Unity CharacterController with custom movement physics

## Possible Improvements

- Obstacle avoidance or pathfinding for enemies.
- Improved spawn placement and collision clearance.
- Checkpoint interaction prompts.
- Death and respawn transitions.
- Better cave lighting, lake materials, and entrance/exit markers.
- Gameplay balancing for enemies, crystals, and movement.
