# Space Shooter Mobile

A vertical-scrolling arcade space shooter for mobile, built with **Unity 6** and **C#**.
Drag your ship around the screen, destroy meteors and enemy ships, and beat your best score.

![Unity](https://img.shields.io/badge/Unity-6000.3-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-.NET-purple?logo=csharp)
![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS-green)

## Gameplay

- **Move:** touch and drag anywhere on the screen. The ship follows your finger with the offset kept, so your thumb never covers it.
- **Shoot:** the ship fires lasers automatically.
- **Score:** a meteor is worth **10** points and an enemy ship is worth **25**.
- **Game over:** touching a meteor or an enemy ship ends the run. If your score beats the record, it is saved as the new high score.

## Features

| Feature | Details |
| --- | --- |
| Touch controls | Enhanced Touch API from the new Input System, with movement clamped to a safe play area |
| Enemies | Meteors with random size, speed and spin, plus zig-zagging enemy ships with more health |
| Spawning | Spawn positions are calculated from the camera viewport, so they work on any screen ratio |
| Scoring | Live HUD shows the current score and the best score |
| High score | Saved between sessions with `PlayerPrefs` |
| Menus | Main menu (Start / Quit, best score) and a Game Over screen (Restart / Menu) |
| Background | Multi-layer parallax scrolling space background |
| UI scaling | Canvas scales from a 1080×1920 reference resolution for portrait phones |

## Project structure

```
Assets/
├── Prefabs/            LaserBullet, MeteorOne (3 variants), EnemyShip
├── Scenes/
│   ├── MainMenu.unity  Start screen (build index 0)
│   └── SampleScene.unity  Gameplay (build index 1)
├── Scripts/
│   ├── PlayerControls.cs   Touch drag movement and screen bounds
│   ├── PlayerShooting.cs   Auto-fire on a fixed interval
│   ├── LaserBullet.cs      Projectile movement and hit damage
│   ├── Parallax.cs         Endless scrolling background layers
│   ├── GameManager.cs      Score, high score, game over, restart
│   ├── MainMenu.cs         Main menu buttons and best score
│   ├── EnemySc/
│   │   ├── Enemy.cs        Base class: health, damage, score reward
│   │   ├── Meteor.cs       Falling, rotating meteor
│   │   ├── EnemyShip.cs    Zig-zag enemy ship
│   │   └── MeteorSpawner.cs  Timed random spawner (used for both enemy types)
│   └── Editor/
│       └── SpaceShooterSetup.cs  One-click scene/UI/prefab generator
└── Sprites/            Backgrounds, player, enemies, UI, fonts
```

### Architecture notes

- **Inheritance for enemies:** `Enemy` holds health, damage handling and the score reward. `Meteor` and `EnemyShip` override `DeathSequence()` and only add their own movement.
- **Singleton `GameManager`:** enemies report kills and player hits through `GameManager.Instance`, so they don't need references set up in the Inspector.
- **Editor tooling:** `Tools > Space Shooter > Setup Game` builds the menu scene, the HUD and Game Over UI, the enemy ship prefab and the build settings from code. Running it again replaces what it generated before.

## Getting started

1. Install **Unity 6000.3.7f1** (or a newer 6000.3 version) with Android and/or iOS build support.
2. Clone the repository:
   ```bash
   git clone https://github.com/furkancirdi/Space-Shooter-Mobile.git
   ```
3. Open the folder with Unity Hub.
4. Open `Assets/Scenes/MainMenu.unity` and press **Play**.
   - For touch input in the editor, use **Window > General > Device Simulator** or enable touch simulation.

## Building for mobile

1. **File > Build Profiles** and choose **Android** or **iOS**.
2. Make sure `MainMenu` is first in the scene list and `SampleScene` is second.
3. Build and run on a device.

## Tech stack

- Unity 6 (6000.3), Universal Render Pipeline (2D)
- C#
- Unity Input System (Enhanced Touch)
- Unity UI (uGUI)

## Roadmap

- Power-ups (shield, heal, extra projectiles). The sprites are already in the project.
- Enemy ships that shoot back, and a boss
- Explosion effects and sound
- Difficulty that increases over time

## Author

**Muhammed Furkan Cirdi** · [GitHub](https://github.com/furkancirdi)
