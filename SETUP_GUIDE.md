# StayOrCash - Setup Guide

This guide will help you set up the procedural generation game in Unity.

## Prerequisites
- Unity 6 (6000.0.34f1) installed
- Project opened in Unity Editor

## Step-by-Step Setup

### 1. Create the Chest Prefab

1. In the Hierarchy, create a new **Cube** (Right-click → 3D Object → Cube)
2. Name it "Chest"
3. Scale it to make it look like a chest: `(1, 0.6, 0.8)`
4. Change the color:
   - Create a new Material in `Assets/` folder (Right-click → Create → Material)
   - Name it "ChestMaterial"
   - Change the Albedo color to gold/yellow
   - Assign it to the Cube's Mesh Renderer
5. Add the `ChestInteractable` script component
6. Drag the "Chest" GameObject from Hierarchy to `Assets/Prefabs/` folder to create a prefab
7. Delete the Chest from the Hierarchy

### 2. Create the Player Prefab

1. In the Hierarchy, create an empty GameObject
2. Name it "Player"
3. Add a **Character Controller** component
   - Set Radius: 0.5
   - Set Height: 2
   - Set Center: (0, 1, 0)
4. Add the `FirstPersonController` script
5. The script will automatically create a camera
6. Drag the "Player" GameObject to `Assets/Prefabs/` folder
7. Delete the Player from the Hierarchy

### 3. Set up the Main Scene

1. Open `Assets/Scenes/SampleScene.unity` (or create a new scene)
2. Delete the default Main Camera (the FirstPersonController creates its own)

#### Add the GameManager

1. Create an empty GameObject, name it "GameManager"
2. Add the `GameManager` script component
3. In the Inspector:
   - **World Generator**: Will assign in next step
   - **Player Prefab**: Drag the Player prefab from `Assets/Prefabs/`
   - **Base Time**: 60 (starting time in seconds)
   - **First Reduction**: 5 (first time penalty)
   - **Reduction Increment**: 5 (how much the penalty increases each run)

#### Add the World Generator

1. Create an empty GameObject, name it "WorldGenerator"
2. Add the `ProceduralWorldGenerator` script component
3. Click the **"Auto-Assign SimpleNaturePack Prefabs"** button in the Inspector
   - This will automatically populate all the tree, bush, rock, grass, etc. arrays
4. In the Inspector:
   - **Chest Prefab**: Drag the Chest prefab from `Assets/Prefabs/`
   - **World Size**: 100 (adjust for larger/smaller worlds)
   - **Tree Count**: 50
   - **Bush Count**: 30
   - **Rock Count**: 40
   - **Detail Count**: 80
   - **Chest Min Distance**: 20
   - **Chest Max Distance**: 40

5. Go back to **GameManager** and assign:
   - **World Generator**: Drag the "WorldGenerator" GameObject

### 4. Set up the UI (Optional but Recommended)

1. Create a Canvas (Right-click in Hierarchy → UI → Canvas)
2. Set Canvas Scaler to "Scale With Screen Size" (Reference: 1920x1080)

#### Timer Display
1. Right-click Canvas → UI → Text - TextMeshPro
2. Name it "TimerText"
3. Position: Top-left corner
4. Anchor: Top-left
5. Text: "Time: 00:00"
6. Font Size: 36
7. Color: White

#### Score Display
1. Duplicate TimerText, name it "ScoreText"
2. Position below TimerText
3. Text: "Score: 0"

#### Run Counter
1. Duplicate TimerText, name it "RunText"
2. Position below ScoreText
3. Text: "Run: 1"

#### Interaction Prompt
1. Right-click Canvas → UI → Text - TextMeshPro
2. Name it "PromptText"
3. Anchor: Bottom-center
4. Position: Center-bottom of screen
5. Text: "Press E to collect chest"
6. Font Size: 24
7. Alignment: Center
8. Disable this GameObject initially

#### Create GameUI Manager
1. Create an empty GameObject under Canvas, name it "GameUI"
2. Add the `GameUI` script
3. Assign all the text objects:
   - **Timer Text**: TimerText
   - **Score Text**: ScoreText
   - **Run Text**: RunText
   - **Prompt Text**: PromptText

### 5. Configure Input System

The project already has `InputSystem_Actions.inputactions` configured with:
- **Move**: WASD / Arrow Keys / Gamepad Left Stick
- **Look**: Mouse Delta / Gamepad Right Stick
- **Jump**: Space / Gamepad South Button
- **Sprint**: Left Shift / Gamepad Left Stick Press
- **Interact**: E Key / Gamepad North Button

If you need to modify controls:
1. Double-click `Assets/InputSystem_Actions.inputactions`
2. Edit the bindings in the Input Actions window

### 6. Test the Game

1. Click Play in Unity Editor
2. You should spawn in the center of a procedurally generated world
3. Use **WASD** to move, **Mouse** to look, **Space** to jump, **Left Shift** to sprint
4. Find the glowing rotating chest
5. When near the chest, press **E** to collect it
6. The world will regenerate with less time
7. The game ends when time runs out

## Troubleshooting

### No terrain appears
- Check that the WorldGenerator has the ProceduralWorldGenerator script attached
- Make sure GameManager is calling StartNewGame() on Start

### Player falls through terrain
- Wait 0.1 seconds after generating terrain before spawning player (this is already handled in code)
- Make sure the terrain has a TerrainCollider component

### Can't interact with chest
- Make sure the Chest prefab has the ChestInteractable script
- Check that the chest's interaction range is set (default: 3 units)
- Ensure Input System is enabled in Project Settings

### Camera not working
- The FirstPersonController automatically creates a camera
- Delete any Main Camera in the scene
- Make sure Cursor is locked (Cursor.lockState = Locked)

### SimpleNaturePack prefabs not loading
- Click the "Auto-Assign SimpleNaturePack Prefabs" button in WorldGenerator Inspector
- If that fails, manually drag prefabs from `Assets/SimpleNaturePack/Prefabs/`

## Next Steps

### Enhance the Chest Visual
- Replace the cube with a 3D chest model
- Add a glow effect using URP's emission
- Add particle effects when collected

### Add Audio
- Footstep sounds
- Chest collection sound
- Timer tick sound when < 10 seconds
- Background music

### Improve UI
- Add cash-out panel with buttons
- Add game over screen
- Add main menu
- Add visual timer bar

### Polish
- Add skybox
- Add lighting
- Add post-processing effects
- Add chest indicator (directional arrow or distance meter)

## File Structure Reference

```
Assets/
├── Scenes/
│   └── SampleScene.unity (main game scene)
├── Scripts/
│   ├── Core/
│   │   └── GameManager.cs
│   ├── Player/
│   │   └── FirstPersonController.cs
│   ├── World/
│   │   ├── ProceduralWorldGenerator.cs
│   │   └── ChestInteractable.cs
│   ├── UI/
│   │   └── GameUI.cs
│   └── Editor/
│       └── ProceduralWorldGeneratorEditor.cs
├── Prefabs/
│   ├── Player.prefab
│   └── Chest.prefab
└── SimpleNaturePack/
    └── Prefabs/ (auto-assigned by editor script)
```

## Game Design Summary

- **Run 1**: 60 seconds
- **Run 2**: 55 seconds (-5s)
- **Run 3**: 45 seconds (-10s)
- **Run 4**: 30 seconds (-15s)
- **Minimum**: 10 seconds

Scoring: `(Time Remaining × 10) + (Run Number × 100)`

Good luck with your 2-hour game jam project!
