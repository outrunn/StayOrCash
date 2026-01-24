# StayOrCash - Master Setup Guide

**Complete guide to get StayOrCash playable from scratch**

Unity 6 (6000.0.34f1) | Universal Render Pipeline | 2-Hour Game Jam Project

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start (15 Minutes)](#quick-start-15-minutes)
3. [Detailed Setup Steps](#detailed-setup-steps)
4. [Testing & Verification](#testing--verification)
5. [Troubleshooting](#troubleshooting)
6. [Quick Reference](#quick-reference)
7. [Understanding the Architecture](#understanding-the-architecture)
8. [Next Steps](#next-steps)

---

## Prerequisites

Before starting, ensure you have:

- ✅ **Unity 6 (6000.0.34f1)** installed via Unity Hub
- ✅ **Project opened** in Unity Editor
- ✅ **TextMeshPro** installed (Unity will prompt on first use)
- ✅ **All scripts compiled** without errors (check Console)

**If scripts don't compile**: Check the Console window for errors and resolve them before proceeding.

---

## Quick Start (15 Minutes)

For experienced Unity users who want to get running fast:

### 1. Create Config Assets (2 min)
```
Assets → Create → StayOrCash → Game Config → Name: "DefaultGameConfig"
Assets → Create → StayOrCash → World Generation Config → Name: "DefaultWorldConfig"
```

### 2. Setup GameManager (5 min)
- Create empty GameObject named "GameManager"
- Add components: `GameManager`, `TimeManager`, `ScoreManager`
- Assign all config and system references in Inspector
- Assign Player prefab from `Assets/Prefabs/Player.prefab`

### 3. Setup WorldGenerator (3 min)
- Create empty GameObject named "WorldGenerator"
- Add `ProceduralWorldGenerator` component
- Assign `DefaultWorldConfig`
- Click "Auto-Assign SimpleNaturePack Prefabs" button
- Assign Chest prefab from `Assets/Prefabs/Chest.prefab`
- Link WorldGenerator to GameManager

### 4. Setup UI (2 min)
- Drag `Assets/Prefabs/UI/GameCanvas.prefab` into scene

### 5. Configure Scene (2 min)
- Delete default Main Camera (Player creates its own)
- Add Directional Light if needed
- Assign Skybox: Window → Rendering → Lighting → Skybox Material

### 6. Test (1 min)
- Press Play ▶
- Move with WASD, look with Mouse, find chest, press E

**Done!** Skip to [Testing & Verification](#testing--verification) to confirm everything works.

---

## Detailed Setup Steps

Follow these steps if you're new to Unity or want detailed explanations.

---

### Step 1: Create Configuration Assets

The refactored architecture uses **ScriptableObject** assets to store game settings. This allows you to tweak values without changing code.

#### 1.1 Create Game Config

1. In the **Project** window, navigate to `Assets/`
2. Right-click → **Create → StayOrCash → Game Config**
3. Name it `DefaultGameConfig`
4. Select the asset and set values in **Inspector**:
   - **Base Time**: `60` (starting time in seconds for Run 1)
   - **First Reduction**: `5` (time penalty from Run 1 to Run 2)
   - **Reduction Increment**: `5` (how much penalty increases each run)
   - **Minimum Time**: `10` (lowest possible time limit)
   - **Time Score Multiplier**: `10` (score = time remaining × this)
   - **Run Score Bonus**: `100` (bonus score per run number)
   - **Player Spawn Delay**: `0.1` (delay after world gen)
   - **Game Over Restart Delay**: `3`
   - **Cash Out Restart Delay**: `3`

**What this controls**: Time limits per run and scoring formulas.

#### 1.2 Create World Generation Config

1. In the **Project** window, navigate to `Assets/`
2. Right-click → **Create → StayOrCash → World Generation Config**
3. Name it `DefaultWorldConfig`
4. Select the asset and review settings in **Inspector**:
   - **World Size**: `100` (100×100 units)
   - **Max Terrain Height**: `10` (vertical terrain scale)
   - **Noise Scale**: `0.05` (larger = smoother terrain)
   - **Tree Count**: `50`
   - **Bush Count**: `30`
   - **Rock Count**: `40`
   - **Detail Count**: `80` (grass/flowers/mushrooms)
   - **Min Object Spacing**: `2` (prevents clustering)
   - **Center Exclusion Radius**: `10` (clear spawn area)
   - **Chest Min/Max Distance**: `20` to `40` (from center)
   - **Scale Variation**: `0.3` (random size variation)

**What this controls**: Procedural world generation parameters.

---

### Step 2: Set Up GameManager GameObject

The GameManager orchestrates all game systems using a modular architecture.

#### 2.1 Create the GameObject

1. In **Hierarchy**, right-click → **Create Empty**
2. Name it `GameManager`
3. Reset Transform: Inspector → Transform → Right-click → **Reset**

#### 2.2 Add Components

Add these three components in order (Add Component button):

1. **GameManager** (`StayOrCash.Managers.GameManager`)
2. **TimeManager** (`StayOrCash.Systems.TimeManager`)
3. **ScoreManager** (`StayOrCash.Systems.ScoreManager`)

**Note**: If you see multiple options, select the ones with the namespace shown in parentheses.

#### 2.3 Configure GameManager Component

Select the GameManager GameObject and configure the **GameManager** component:

**Configuration:**
- **Game Config**: Drag `DefaultGameConfig` asset from Project window

**System References:**
- **Time Manager**: Drag the `TimeManager` component (same GameObject)
- **Score Manager**: Drag the `ScoreManager` component (same GameObject)
- **World Generator**: Leave empty for now (we'll set this in Step 3)
- **Data Manager**: Leave empty (optional persistent data system)

**Player:**
- **Player Prefab**: Drag `Assets/Prefabs/Player.prefab`

**Menu Settings:**
- **Menu Scene Name**: `MainMenu` (if you have a main menu scene)

#### 2.4 Configure TimeManager Component

Still on the GameManager GameObject, configure the **TimeManager** component:

- **Game Config**: Drag `DefaultGameConfig` asset

#### 2.5 Configure ScoreManager Component

Still on the GameManager GameObject, configure the **ScoreManager** component:

- **Game Config**: Drag `DefaultGameConfig` asset

**Result**: Your GameManager should now have 3 components, all configured with the GameConfig asset.

---

### Step 3: Set Up WorldGenerator GameObject

The WorldGenerator creates procedural terrain and spawns objects.

#### 3.1 Create the GameObject

1. In **Hierarchy**, right-click → **Create Empty**
2. Name it `WorldGenerator`
3. Reset Transform

#### 3.2 Add Component

1. Click **Add Component**
2. Search for `ProceduralWorldGenerator`
3. Select **ProceduralWorldGenerator** (`StayOrCash.World`)

#### 3.3 Configure WorldGenerator

**Config:**
- **Config**: Drag `DefaultWorldConfig` asset from Project window

**Prefabs:**
- **Chest Prefab**: Drag `Assets/Prefabs/Chest.prefab`

**SimpleNaturePack Prefabs (Auto-Assign):**
1. Click the **"Auto-Assign SimpleNaturePack Prefabs"** button in the Inspector
2. Wait for confirmation message in Console
3. All prefab arrays should now be populated

**What Auto-Assign does**: Automatically finds and assigns all trees, bushes, rocks, grass, flowers, and mushrooms from the SimpleNaturePack asset folder.

**If Auto-Assign button doesn't appear**: Make sure the `ProceduralWorldGeneratorEditor.cs` script exists at `Assets/Scripts/World/Generation/Editor/`.

#### 3.4 Link to GameManager

1. Select **GameManager** GameObject in Hierarchy
2. Find the **GameManager** component in Inspector
3. Drag the **WorldGenerator** GameObject into the **World Generator** field

**Result**: GameManager and WorldGenerator are now connected.

---

### Step 4: Set Up UI

The UI is pre-configured and ready to use via a prefab.

#### 4.1 Add UI to Scene

1. In **Project** window, navigate to `Assets/Prefabs/UI/`
2. Drag **GameCanvas.prefab** into the **Hierarchy**
3. Done! The UI is fully configured.

**What's included**:
- Canvas with proper screen scaling
- Timer display (top-left, color-coded by urgency)
- Score display (top-right, gold color)
- Run counter (top-right, below score)
- Interaction prompt (bottom-center, shows when near chest)
- GameUI component (auto-subscribes to game events)

#### 4.2 Verify UI References (Optional)

Select **GameCanvas** in Hierarchy and check the **GameUI** component:

- Timer Text, Score Text, Run Text, Prompt Text should all be assigned
- If any are missing, drag the corresponding TextMeshPro child object

---

### Step 5: Configure Scene Settings

Make the world visible and playable.

#### 5.1 Remove Default Camera

1. Find **Main Camera** in Hierarchy (if it exists)
2. Right-click → **Delete**

**Why**: The FirstPersonController creates its own camera automatically. Having two cameras causes issues.

#### 5.2 Add Lighting

Check if you have a **Directional Light** in the scene:

1. If missing: Right-click Hierarchy → **Light → Directional Light**
2. Name it `Sun`
3. Set Transform → Rotation: `(50, -30, 0)` (sunlight angle)
4. Set Light component:
   - **Color**: Slightly yellow `(255, 244, 214)`
   - **Intensity**: `1`
   - **Mode**: Realtime

#### 5.3 Add Skybox (Optional but Recommended)

1. Top menu: **Window → Rendering → Lighting**
2. Click **Environment** tab
3. **Skybox Material**: Drag a skybox from Project (Unity has defaults)
   - Try: `Assets/Skybox` (if you have one)
   - Or use Unity's built-in: Search "skybox" in Project window
4. Click **Generate Lighting** at bottom (wait for baking to finish)

**Why**: Makes the world look better and provides ambient lighting.

#### 5.4 Set Scene Starting State

1. Save the scene: **File → Save Scene** (Ctrl/Cmd+S)
2. Name it appropriately (e.g., `GameScene` or keep `SampleScene`)

---

### Step 6: Final Checklist

Before testing, verify this checklist:

#### Configuration Assets
- [ ] `DefaultGameConfig.asset` exists in Assets folder
- [ ] `DefaultWorldConfig.asset` exists in Assets folder
- [ ] Both assets have values set (not empty)

#### GameManager GameObject
- [ ] Has `GameManager` component
- [ ] Has `TimeManager` component
- [ ] Has `ScoreManager` component
- [ ] All three components reference `DefaultGameConfig`
- [ ] GameManager references TimeManager and ScoreManager
- [ ] GameManager references WorldGenerator GameObject
- [ ] GameManager references Player prefab

#### WorldGenerator GameObject
- [ ] Has `ProceduralWorldGenerator` component
- [ ] References `DefaultWorldConfig`
- [ ] References Chest prefab
- [ ] Tree, bush, rock, grass arrays are populated (Auto-Assign worked)

#### UI
- [ ] GameCanvas prefab is in scene
- [ ] GameUI component has all text references assigned

#### Scene
- [ ] No Main Camera (Player creates its own)
- [ ] Has Directional Light
- [ ] Has Skybox assigned (optional but recommended)
- [ ] Scene is saved

---

## Testing & Verification

Time to test your setup!

### Initial Test

1. **Press Play** ▶ in Unity Editor (or press Ctrl/Cmd+P)

2. **What should happen**:
   - World generates (terrain with trees, bushes, rocks, grass)
   - Player spawns at center (first-person view)
   - UI appears (Timer: 60:00, Score: 0, Run: 1)
   - Cursor is locked (invisible)
   - Console shows: "World generated with seed: XXXXX"

3. **Try moving**:
   - **WASD** or **Arrow Keys**: Move
   - **Mouse**: Look around
   - **Space**: Jump
   - **Left Shift**: Sprint (hold while moving)

4. **Find the chest**:
   - Look around for a rotating golden cube
   - It spawns 20-40 units from center
   - Should be visible among the nature objects

5. **Interact with chest**:
   - Walk close to it (within 3 meters)
   - Prompt appears: "Press E to collect chest"
   - **Press E**
   - Chest disappears, score increases
   - World regenerates with new layout
   - Timer now shows less time (55 seconds for Run 2)

6. **Continue playing**:
   - Find the next chest before time runs out
   - Each run reduces time further: Run 3 = 45s, Run 4 = 30s, etc.
   - If time runs out: Game over, lose all score, restart

### Console Output

Check the **Console** window for these messages (no errors):

```
World generated with seed: 12345
Terrain created at position (0, 0, 0)
Spawned 50 trees
Spawned 30 bushes
Spawned 40 rocks
Spawned 80 details
Chest spawned at (X, Y, Z)
Player spawned at center
TimeManager: Starting timer for Run 1 with 60 seconds
```

### Performance Check

- **Frame Rate**: Should be 60+ FPS on modern hardware
- **Generation Time**: World should generate in < 1 second
- **No Stuttering**: Movement should be smooth

If performance is bad, reduce object counts in `DefaultWorldConfig`.

---

## Troubleshooting

### Problem: "Game doesn't start / nothing happens"

**Possible Causes**:
1. GameManager not in scene
2. GameManager missing references
3. Script compilation errors

**Fix**:
1. Check Console for red errors
2. Verify GameManager exists in Hierarchy
3. Select GameManager, check Inspector for missing references (shows as "None")
4. Reassign any missing references

---

### Problem: "No terrain appears / black screen"

**Possible Causes**:
1. WorldGenerator not configured
2. No lighting in scene
3. Camera issues

**Fix**:
1. Select WorldGenerator, verify `DefaultWorldConfig` is assigned
2. Add Directional Light to scene
3. Check Console for "World generated" message
4. If player is spawning but terrain is invisible, check Terrain Layer (Scene view)

---

### Problem: "Player falls through terrain"

**Possible Causes**:
1. Player spawns before terrain finishes generating
2. Terrain collider missing

**Fix**:
1. Already handled by code (0.1s spawn delay)
2. If still happens, increase `playerSpawnDelay` in GameConfig to `0.5`
3. Check that generated terrain has TerrainCollider component (it should auto-add)

---

### Problem: "Can't move / camera doesn't work"

**Possible Causes**:
1. Player prefab missing FirstPersonController
2. Input System not configured
3. Cursor not locked

**Fix**:
1. Open Player prefab, check for `FirstPersonController` component
2. Verify `CharacterController` component exists on Player
3. Check Console for input-related errors
4. Input Actions should auto-create, but verify `InputSystem_Actions.inputactions` exists

---

### Problem: "Press E does nothing / can't collect chest"

**Possible Causes**:
1. Chest missing ChestInteractable component
2. Chest missing Collider
3. Player reference not set

**Fix**:
1. Open Chest prefab in Project window
2. Verify `ChestInteractable` component exists
3. Verify BoxCollider or SphereCollider exists
4. Collider must NOT be "Is Trigger"
5. Set Interaction Range to `3` in ChestInteractable
6. Play again (GameManager auto-assigns player reference)

---

### Problem: "UI doesn't show / shows 00:00"

**Possible Causes**:
1. GameUI not subscribed to events
2. GameManager not running
3. TextMeshPro not imported

**Fix**:
1. Check that GameCanvas exists in scene
2. Select GameCanvas, verify GameUI component exists
3. Verify text components are assigned in GameUI
4. If TextMeshPro prompts import, do it and regenerate UI
5. Check Console for GameManager start messages

---

### Problem: "SimpleNaturePack objects missing / pink objects"

**Possible Causes**:
1. Auto-Assign didn't work
2. Materials using wrong shader
3. SimpleNaturePack not in correct folder

**Fix**:
1. Click "Auto-Assign SimpleNaturePack Prefabs" button again
2. Check Console for assignment confirmation
3. If materials are pink: Select prefab → Material → Shader → Change to "Standard"
4. Verify SimpleNaturePack folder exists at `Assets/SimpleNaturePack/Prefabs/`

---

### Problem: "Can walk through chest"

**Expected Behavior**: Chest should have collision.

**Fix**:
1. Open Chest prefab
2. Add Component → Physics → Box Collider
3. Make sure "Is Trigger" is **unchecked**
4. Adjust collider size to match chest visual

---

### Problem: "Compilation errors"

**Fix**:
1. Check Console for red error messages
2. Common issues:
   - Missing namespace imports: Add `using StayOrCash.X;`
   - Wrong namespace: Verify script is in correct folder
   - Mismatched component references: Update Inspector references
3. If unsure, check `CLAUDE.md` for architecture details

---

## Quick Reference

### Controls

| Action | Input |
|--------|-------|
| Move | WASD / Arrow Keys |
| Look | Mouse |
| Jump | Space |
| Sprint | Left Shift (hold) |
| Interact | E |
| Pause/Unlock Cursor | Escape |

### Game Mechanics

- **Run 1**: 60 seconds
- **Run 2**: 55 seconds (-5s)
- **Run 3**: 45 seconds (-10s)
- **Run 4**: 30 seconds (-15s)
- **Minimum**: 10 seconds

**Scoring Formula**: `(Time Remaining × 10) + (Run Number × 100)`

Example: Collect chest on Run 3 with 20 seconds left = (20 × 10) + (3 × 100) = 500 points

### File Locations

```
Assets/
├── Prefabs/
│   ├── Player.prefab              [First-person player]
│   ├── Chest.prefab               [Collectible treasure]
│   └── UI/
│       └── GameCanvas.prefab      [HUD UI]
├── Scripts/
│   ├── Core/
│   │   ├── Managers/GameManager.cs
│   │   └── Systems/TimeManager.cs, ScoreManager.cs
│   ├── Data/GameConfig.cs, WorldGenerationConfig.cs
│   ├── Player/FirstPersonController.cs
│   ├── UI/GameUI.cs
│   └── World/
│       ├── ChestInteractable.cs
│       └── Generation/ProceduralWorldGenerator.cs
├── Scenes/
│   ├── SampleScene.unity
│   └── mainscene.unity
└── SimpleNaturePack/              [Nature assets]
```

### Console Commands (for debugging)

Open Console: Window → General → Console (Ctrl+Shift+C)

**Useful filters**:
- Errors only: Click red icon
- Warnings only: Click yellow icon
- Clear: Click "Clear" button

---

## Understanding the Architecture

This project uses a **modular, event-driven architecture**.

### Key Components

**GameManager** (Orchestrator)
- Coordinates all systems
- Manages game state machine
- Singleton pattern: Access via `GameManager.Instance`

**TimeManager** (System)
- Counts down timer
- Calculates time limits per run
- Fires events: `OnTimeUpdate`, `OnTimeUp`

**ScoreManager** (System)
- Tracks score and run number
- Calculates scores based on formulas
- Fires events: `OnScoreChanged`, `OnRunChanged`

**ProceduralWorldGenerator** (System)
- Generates terrain using Perlin noise
- Spawns nature objects with smart placement
- Spawns chest at controlled distance

**FirstPersonController** (Player)
- Handles movement, camera, and input
- Interacts with `IInteractable` objects

**GameUI** (UI)
- Subscribes to system events
- Updates displays automatically
- No polling, event-driven only

### Events Flow Example

```
Player presses E
  ↓
FirstPersonController detects IInteractable
  ↓
ChestInteractable.Interact() called
  ↓
GameManager.OnChestCollected()
  ↓
ScoreManager.AddChestScore() → fires OnScoreChanged event
  ↓
TimeManager.PauseTimer()
  ↓
GameUI.HandleScoreChanged() → updates UI
  ↓
Player decides: Continue or Cash Out
```

### Configuration Assets (ScriptableObjects)

**Why use ScriptableObjects?**
- Change game balance without editing code
- Create multiple configs (Easy/Hard modes)
- Shareable between scenes
- Visible in Inspector

**GameConfig**: Controls time, scoring, delays
**WorldGenerationConfig**: Controls world size, object counts, spawning

### Namespaces

All scripts use C# namespaces for organization:

- `StayOrCash.Managers` - GameManager
- `StayOrCash.Systems` - TimeManager, ScoreManager
- `StayOrCash.Data` - Config ScriptableObjects
- `StayOrCash.Interfaces` - IInteractable, IWorldGenerator, etc.
- `StayOrCash.Player` - FirstPersonController
- `StayOrCash.UI` - GameUI
- `StayOrCash.World` - ChestInteractable, ProceduralWorldGenerator

**Benefits**: Clean organization, prevents naming conflicts, professional structure

---

## Next Steps

Congratulations! You have a playable procedural treasure hunt game.

### Immediate Polish (30 min each)

1. **Improve Chest Visual**
   - Replace cube with 3D model
   - Add glowing material (URP Emission)
   - Add particle effect on collection

2. **Add Audio**
   - Footstep sounds (random pitch variation)
   - Chest collection sound
   - Timer warning beep (< 10 seconds)
   - Background music

3. **Enhanced UI**
   - Add timer progress bar
   - Add cash-out panel with buttons
   - Add game over screen with "Try Again"
   - Add final score screen with stats

4. **Visual Polish**
   - Better skybox (Asset Store has free ones)
   - Post-processing (Bloom, Color Grading, Vignette)
   - Fog for atmosphere
   - Better terrain texturing

### Feature Additions (1-2 hours each)

1. **Cash-Out System**
   - UI panel: "Continue" vs "Cash Out" buttons
   - Display: Current score, next run time, risk/reward
   - End game on cash-out with final score screen

2. **Main Menu**
   - New scene: `MainMenu.unity`
   - Buttons: Play, Options, Quit
   - Scene transition with loading screen

3. **Save System**
   - High score persistence (PlayerPrefs)
   - Best run tracking
   - Total chests collected

4. **Difficulty Modes**
   - Create multiple GameConfig assets
   - Easy: More time, less reduction
   - Hard: Less time, more reduction
   - Menu to select difficulty

5. **Chest Indicator**
   - Directional arrow pointing to chest
   - Distance meter
   - Optional: Minimap

### Advanced Features (2+ hours each)

1. **Multiple Biomes**
   - Create different WorldGenerationConfigs
   - Desert, forest, snow themes
   - Different object sets per biome

2. **Power-Ups** (implement IInteractable)
   - Speed boost (temporary sprint increase)
   - Time freeze (pause timer for 5s)
   - Chest radar (show direction)

3. **Obstacles**
   - Water (slows movement)
   - Cliffs (require jumping)
   - Dense forests (reduced visibility)

4. **Achievement System** (implement IGameStateObserver)
   - First chest collected
   - Reach Run 5
   - Collect chest with < 5s remaining
   - UI popup notifications

### Testing & Optimization

1. **Performance Profiling**
   - Window → Analysis → Profiler
   - Check CPU/GPU usage
   - Optimize object counts if needed

2. **Build Testing**
   - File → Build Settings
   - Select platform (PC, Mac, Linux)
   - Build and test outside editor

3. **Playtesting**
   - Have others play your game
   - Gather feedback on difficulty
   - Adjust GameConfig based on data

---

## Additional Resources

### Documentation

- **CLAUDE.md**: Comprehensive project context and architecture
- **REFACTORING_GUIDE.md**: Deep dive into modular architecture
- **TROUBLESHOOTING.md**: Detailed debugging information

### Unity Learning

- [Unity Manual](https://docs.unity3d.com/Manual/index.html)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

### Asset Packs (Free on Asset Store)

- Better skyboxes
- Nature sounds
- UI sprite packs
- Particle effects

---

## Summary

You've just set up a complete procedurally generated survival game with:

✅ Modular, event-driven architecture
✅ Configurable game balance (ScriptableObjects)
✅ Procedural world generation (Perlin noise terrain)
✅ Smart object placement (spacing, exclusion zones)
✅ First-person player controller (WASD, mouse look, jumping)
✅ Timer system with accelerating difficulty
✅ Score system with risk/reward mechanics
✅ Interaction system (collect chests)
✅ Dynamic UI (event-driven updates)
✅ Professional code structure (namespaces, interfaces, events)

**Total Setup Time**: 15-30 minutes depending on experience

**Result**: Fully playable game at runtime!

---

## Need Help?

1. **Check Console**: Most issues show error messages there
2. **Review Troubleshooting**: Common problems have solutions above
3. **Read CLAUDE.md**: Comprehensive project documentation
4. **Unity Forums**: Community help for Unity-specific questions
5. **C# Documentation**: For programming questions

---

**Good luck with your class project!** 🎮

This architecture will serve you well in future Unity projects. You've learned:
- Modular system design
- Event-driven programming
- ScriptableObject patterns
- Unity best practices
- Professional code organization

Keep building and learning!
