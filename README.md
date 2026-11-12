# Space Shooter Mobile

A portrait arcade shooter for phones, made in Unity 6 with C#. You drag the ship around with one finger, it shoots on its own, and you try to beat your best score.

This was my first Unity project. I started it to learn how touch input, prefabs and spawning work on mobile.

## How it plays

Touch anywhere and drag. The ship keeps its distance from your finger, so your thumb doesn't cover it. Lasers fire automatically.

A meteor gives 10 points and an enemy ship gives 25. Enemy ships move in a zig-zag and take more hits. If anything touches you the run is over, and the score is saved if it is a new record.

There is a main menu (start, quit, best score) and a game over screen (restart, back to menu).

## What I wrote and what I got help with

The first part I wrote myself in February and March 2026, one small step per commit: touch movement, keeping the ship inside the screen, the meteor, the spawner and the laser.

In November I came back to it and added the rest with an AI coding assistant: scoring and the saved high score, both menus, the enemy ship, and an editor script that sets up the scenes and UI. I decided what to add and how it should behave, and the assistant wrote most of that code.

## Code

Everything is under `Assets/Scripts`.

- `PlayerControls.cs` handles touch drag and screen limits
- `PlayerShooting.cs` and `LaserBullet.cs` handle firing and hits
- `EnemySc/Enemy.cs` is the base class with health and score value. `Meteor.cs` and `EnemyShip.cs` only add their own movement.
- `EnemySc/MeteorSpawner.cs` spawns both enemy types. Positions come from the camera, so it works on different screen ratios.
- `GameManager.cs` keeps the score, the high score (PlayerPrefs) and the game over flow
- `Parallax.cs` scrolls the background layers
- `Editor/SpaceShooterSetup.cs` adds a menu item, Tools > Space Shooter > Setup Game, that rebuilds the menu scene, HUD and enemy ship prefab

## Running it

Open the folder with Unity 6000.3.7f1, load `Assets/Scenes/MainMenu.unity` and press Play. Touch needs the Device Simulator (Window > General > Device Simulator).

For a phone build, `MainMenu` has to be first in the scene list and `SampleScene` second.

## Not done yet

- Power-ups (the sprites are in the project, nothing uses them yet)
- Enemies that shoot back, and a boss
- Explosions and sound
- Difficulty that goes up over time
