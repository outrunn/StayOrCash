# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**PokerWar** is a Unity 6 (6000.0.34f1) class project using the Universal Render Pipeline (URP). This is a **2-hour game jam scope project** - keep implementations simple and focused.

**Game Concept:**
A first-person procedurally generated survival game where players race against time to find treasure chests in randomly generated natural environments. Each successful run increases difficulty with reduced time limits, creating a risk/reward mechanic where players must decide whether to cash out or push for higher scores.

**Key Technologies:**
- Unity 6 (6000.0.34f1)
- Universal Render Pipeline (URP) 17.0.3
- New Input System 1.11.2
- SimpleNaturePack for environment assets

## Game Design

### Core Gameplay Loop
1. **Start Run**: Player spawns in center of procedurally generated terrain
2. **Exploration**: Player searches for a chest hidden somewhere in the world
3. **Time Pressure**: Countdown timer ticks down (starts at configurable base time)
4. **Decision Point**: When chest is found, player can:
   - **Collect & Continue**: Take the reward, regenerate world, start new run with LESS time
   - **Cash Out**: Keep all accumulated rewards and end the session
5. **Failure State**: If time runs out, player loses all accumulated rewards

### Time Reduction Mechanic
- **Accelerating Difficulty**: Each successful run reduces available time with increasing penalties
- Example progression: Run 1 (60s) → Run 2 (55s, -5s) → Run 3 (45s, -10s) → Run 4 (30s, -15s)
- Creates escalating tension and strategic cash-out decisions

### Procedural Generation

The game uses procedural generation to create unique worlds every run, ensuring no two playthroughs are identical.

#### Terrain Generation
- **Algorithm**: Multi-octave Perlin noise for realistic, organic terrain
- **Process**:
  1. Creates Unity Terrain with configurable resolution (default 513x513 heightmap)
  2. Generates three noise layers with different frequencies:
     - Base layer (10x frequency): Creates main hills and valleys (50% weight)
     - Mid layer (5x frequency): Adds medium-sized features (30% weight)
     - Detail layer (2x frequency): Adds fine variations (20% weight)
  3. Random offset ensures different terrain each run
  4. Terrain size: Configurable (default 100x100 units, 10 units height)

#### Terrain Ground Material & Details

**Configuration System:**
All terrain textures and grass/flower details are configured via the `WorldGenerationConfig` ScriptableObject. This allows easy tweaking without code changes.

**Ground Texture Configuration:**
- **groundTexture**: Main terrain diffuse texture (e.g., Grass01_BigUV from ALP_Assets)
- **groundNormalMap**: Optional normal map for terrain surface detail
- Applied via Unity's TerrainLayer system during world generation
- Tiling is automatically calculated based on world size

**Grass & Flower Detail System:**
Instead of spawning 3D prefabs, the system uses Unity's terrain detail renderer with billboard textures for performance:

- **Grass Details** (Full Coverage Mode):
  - **grassDetailTextures[]**: Array of grass texture variants (e.g., grass01.tga, grass02.tga from ALP_Assets)
  - **grassDensity**: Base density per square meter (default: 32 for lush coverage)
  - **grassNoiseScale**: Perlin noise frequency for blending (default: 5, lower = smoother blending between grass types)
  - **grassThreshold**: Must be < 0.2 for full coverage mode (default: 0.05)
  - **Coverage**: 100% of terrain (except spawn area) always has grass, noise controls density variation
  - **Blending**: Multiple grass textures mix naturally across terrain using offset noise patterns

- **Flower Details** (Accent Overlay Mode):
  - **flowerDetailTextures[]**: Array of flower texture variants (e.g., grassFlower01-10.tga from ALP_Assets)
  - **flowerDensity**: Base density per square meter (default: 12)
  - **flowerNoiseScale**: Perlin noise frequency for distribution (default: 15, higher = smaller patches)
  - **flowerThreshold**: Must be > 0.2 for accent mode (default: 0.6, spawns in ~40% of terrain)
  - **Coverage**: Appears as patches overlaying grass base layer

**Smart Distribution:**
  1. **Two-Mode System**:
     - **Full Coverage Mode** (threshold < 0.2): Guarantees base coverage everywhere, noise adds density variation (grass)
     - **Accent Mode** (threshold > 0.2): Traditional threshold-based spawning for patches (flowers)
  2. **Grass Blending**: Each grass texture uses unique Perlin noise offset, naturally mixing across terrain
  3. Avoids center spawn area (keeps player spawn clear based on minDistanceFromCenter)
  4. Configurable density and thresholds allow fine-tuning appearance
  5. Billboard rendering for performance (50+ FPS with thousands of details)
  6. Seed-based procedural placement for reproducible worlds
  7. Lower noise scale for grass (5) creates smooth blending; higher for flowers (15) creates distinct patches

**Render Settings:**
- Details render up to 150 units from camera (configurable via terrain.detailObjectDistance)
- Full density rendering (1.0 = 100% of configured density)
- Automatic LOD (Level of Detail) for optimal performance

#### Environment Object Spawning
- **Nature Assets**: Trees (5 variants), Bushes (3 variants), Rocks (5 variants), Mushrooms (optional detail prefabs)
- **Note**: Grass and flowers are handled by the terrain detail system (see above), not as spawned prefabs
- **Smart Placement**:
  1. For each object type, attempts to spawn requested count
  2. Generates random XZ position within world bounds (covers entire terrain surface)
  3. Uses `Terrain.SampleHeight()` to get accurate terrain height (more reliable than raycasting)
  4. Validates spacing (minimum distance from nearby objects)
  5. Validates center exclusion (keeps spawn area clear)
  6. Applies random rotation (0-360°) and scale variation
  7. Maximum attempts prevent infinite loops if space is limited
  8. Debug logging shows spawn distribution across X and Z axes to verify coverage

#### Chest Positioning
- **Strategy**: Spawned at controlled distance from center (20-40 units default)
  1. Generates random angle (0-360°) and distance within range
  2. Uses `Terrain.SampleHeight()` to get exact terrain height
  3. Positioned 1 unit above ground for visibility
  4. ChestInteractable script added for rotation/bobbing animation
  5. Debug logging shows exact chest position and distance from center

#### Seed System
- Each run uses a random seed (0-100,000)
- Seed initializes Unity's Random state
- Same seed = identical world (useful for debugging/sharing)
- Different seed every run = infinite variety

### Player Mechanics
- **First-Person Perspective**: Immersive exploration
- **Movement**: WASD controls using New Input System
- **Interaction**: E key to collect chest or trigger cash-out

### Menu System & Scene Flow

**Main Menu Scene** (`MainMenu.unity`):
- Displays persistent cash balance (starts at $0)
- Shows high score
- Play button loads game scene and starts new session
- Uses `MainMenuUI.cs` script

**Scene Flow**:
1. **Game Start** → MainMenu scene loads
2. **Play Button** → mainscene loads, GameManager.StartGameFromMenu() called
3. **Run Start** → World generates, "Find the chest!" prompt shows for 3 seconds
4. **Chest Found** → Cash-out panel appears with Continue/Cash Out buttons
5. **Continue** → New world generates with reduced time, gameplay continues
6. **Cash Out** → Returns to MainMenu, cash is added to persistent balance
7. **Time Up** → Returns to MainMenu, all session cash lost

**Persistent Data**:
- Cash stored using `PersistentDataManager` with PlayerPrefs
- High score tracked across sessions
- Default starting cash: $1000 (if first time playing)

### Current Implementation Status
- ✅ Procedural world generator with terrain and nature objects
- ✅ Chest spawning with interaction system
- ✅ Game manager with run/score management
- ✅ Time management with accelerating reduction
- ✅ First-person controller using Input System
- ✅ UI system (timer, score, prompts)
- ✅ Main menu with cash display and play button
- ✅ Complete gameplay loop (menu → play → find chest → cash out → menu)
- ✅ Persistent cash storage using PlayerPrefs
- ⏳ Unity Editor UI setup (Canvas, buttons, text elements)

## Development Commands

### Opening the Project
This is a Unity project - open it through Unity Hub or directly via Unity Editor:
```bash
# Unity must be launched through the Unity Hub or application
# Working directory: /Users/dsfdasv/Documents/Unity Projects/PokerWar (Class Project)
```

### Building the Project
Unity projects are built through the Unity Editor:
- **File → Build Settings** (Cmd+Shift+B / Ctrl+Shift+B)
- Select target platform and click "Build" or "Build and Run"

### Running Tests
Unity Test Framework tests are run through the Unity Editor:
- **Window → General → Test Runner**
- Select PlayMode or EditMode tab
- Click "Run All" or select specific tests

### Common Development Tasks
- **Play Mode**: Press the Play button in Unity Editor or use Cmd+P / Ctrl+P
- **Scene Management**: Scenes are located in `Assets/Scenes/`
- **Opening Scenes**: Double-click scene files (.unity) in the Project window

## Project Structure

### Assets Directory Layout
```
Assets/
├── Scenes/              # Game scenes
├── Scripts/             # Game code (organized by namespace)
│   ├── Core/
│   │   ├── Managers/   # GameManager.cs (orchestration)
│   │   └── Systems/    # TimeManager.cs, ScoreManager.cs (game logic)
│   ├── Data/           # GameConfig.cs, WorldGenerationConfig.cs (ScriptableObjects)
│   ├── Interfaces/     # IInteractable.cs, IWorldGenerator.cs, IGameStateObserver.cs
│   ├── Player/         # FirstPersonController.cs
│   ├── UI/             # GameUI.cs (event-driven UI)
│   └── World/
│       ├── ChestInteractable.cs
│       └── Generation/
│           ├── ProceduralWorldGenerator.cs
│           └── Editor/
│               └── ProceduralWorldGeneratorEditor.cs
├── Settings/            # URP render pipeline and quality settings
│   ├── PC_RPAsset.asset & PC_Renderer.asset (Desktop rendering)
│   ├── Mobile_RPAsset.asset & Mobile_Renderer.asset (Mobile rendering)
│   └── Volume profiles for post-processing
├── Resources/           # Runtime-loadable assets
├── Prefabs/            # Player.prefab, Chest.prefab
├── SimpleNaturePack/    # Nature assets (trees, bushes, rocks, grass, flowers, mushrooms)
│   ├── Prefabs/        # Ready-to-use prefabs for procedural spawning
│   ├── Materials/      # Nature materials
│   └── Models/         # 3D models
└── TutorialInfo/        # Unity template files (can be removed)
```

### Input System Configuration

The project uses Unity's New Input System with a preconfigured Input Actions asset at `Assets/InputSystem_Actions.inputactions`.

**Input Action Maps:**
1. **Player** - Game controls with actions:
   - Move (WASD/Arrow keys, gamepad left stick)
   - Look (Mouse delta, gamepad right stick)
   - Attack (Enter, Mouse left click, gamepad button West)
   - Interact (E key, gamepad North - hold interaction)
   - Crouch (C key, gamepad East)
   - Jump (Space, gamepad South)
   - Sprint (Left Shift, gamepad left stick press)
   - Previous/Next (1/2 keys, gamepad D-pad)

2. **UI** - Standard UI navigation (Navigate, Submit, Cancel, Point, Click, etc.)

**Supported Control Schemes:**
- Keyboard & Mouse
- Gamepad
- Touch
- Joystick
- XR (VR/AR controllers)

To use the Input System in scripts:
```csharp
using UnityEngine.InputSystem;

// Reference the input actions asset
[SerializeField] private InputActionAsset inputActions;

// Or generate C# class from the .inputactions file:
// Right-click InputSystem_Actions.inputactions → Generate C# Class
```

### Rendering Pipeline

The project uses **Universal Render Pipeline (URP)** with separate configurations for different platforms:

- **PC/Desktop**: `Assets/Settings/PC_RPAsset.asset` and `PC_Renderer.asset`
- **Mobile**: `Assets/Settings/Mobile_RPAsset.asset` and `Mobile_Renderer.asset`
- **Global Settings**: `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`
- **Post-Processing**: Volume profiles in `Assets/Settings/`

**Important**: When creating materials, use URP-compatible shaders (URP/Lit, URP/Unlit, etc.), not Built-in or HDRP shaders.

## Dependencies

Key Unity packages (from `Packages/manifest.json`):
- **com.unity.render-pipelines.universal** (17.0.3) - URP rendering
- **com.unity.inputsystem** (1.11.2) - New Input System
- **com.unity.ai.navigation** (2.0.5) - NavMesh navigation
- **com.unity.purchasing** (4.12.2) - In-app purchases
- **com.unity.ads** (4.4.2) - Unity Ads integration
- **com.unity.ugui** (2.0.0) - UI system
- **com.unity.visualscripting** (1.9.5) - Visual scripting (formerly Bolt)
- **com.unity.timeline** (1.8.7) - Timeline animation system

## Architecture Notes

### Modular Architecture Overview

The project uses a **modular, event-driven architecture** with clear separation of concerns. All scripts use C# namespaces for organization.

**Core Principles**:
- **Single Responsibility**: Each class has one focused purpose
- **Event-Driven**: Systems communicate through C# events (loose coupling)
- **Configuration-Based**: Settings stored in ScriptableObject assets
- **Interface-Driven**: Common behaviors defined through interfaces
- **Testable**: Systems can be tested independently

### Namespace Organization

```
PokerWar.Managers    - Orchestration layer (GameManager)
PokerWar.Systems     - Game logic systems (TimeManager, ScoreManager)
PokerWar.Data        - Configuration assets (GameConfig, WorldGenerationConfig)
PokerWar.Interfaces  - Contracts (IInteractable, IWorldGenerator, IGameStateObserver)
PokerWar.Player      - Player-related code (FirstPersonController)
PokerWar.UI          - User interface (GameUI)
PokerWar.World       - World and interactables (ProceduralWorldGenerator, ChestInteractable)
PokerWar.Editor      - Editor tools (Custom inspectors)
```

### Implemented Systems

#### **GameManager** (`PokerWar.Managers.GameManager`)
*Orchestrates all game systems*
- Singleton pattern for global access
- Manages game state machine (Idle, Starting, Playing, ChestCollected, TimeUp, CashedOut)
- Coordinates TimeManager, ScoreManager, and WorldGenerator
- Fires `OnGameStateChanged` event
- **Does NOT** contain game logic (delegates to systems)

#### **TimeManager** (`PokerWar.Systems.TimeManager`)
*Handles countdown timer with accelerating difficulty*
- Counts down time each run
- Calculates time limits based on `GameConfig`
- Events: `OnTimeUpdate(float)`, `OnTimeUp()`
- Methods: `StartTimer()`, `PauseTimer()`, `ResumeTimer()`, `AddTime()`
- Independent system - no GameManager dependency

#### **ScoreManager** (`PokerWar.Systems.ScoreManager`)
*Tracks score and run progression*
- Manages current run number, total score, run score
- Calculates scores using `GameConfig` formulas
- Events: `OnScoreChanged(int, int)`, `OnRunChanged(int)`
- Methods: `StartNewRun()`, `AddChestScore()`, `ResetScore()`
- Returns `ScoreStats` struct for UI display

#### **ProceduralWorldGenerator** (`PokerWar.World.ProceduralWorldGenerator`)
*Generates terrain and spawns objects*
- Implements `IWorldGenerator` interface
- Uses `WorldGenerationConfig` for all settings
- Multi-octave Perlin noise terrain generation
- Smart object placement with spacing validation
- Chest spawning at controlled distances
- Editor tool for auto-assigning SimpleNaturePack prefabs

#### **ChestInteractable** (`PokerWar.World.ChestInteractable`)
*Collectible treasure chest*
- Implements `IInteractable` interface
- Visual feedback: rotation + bobbing animation
- Gizmos show interaction range in editor
- Notifies GameManager when collected

#### **FirstPersonController** (`PokerWar.Player.FirstPersonController`)
*Player movement and interaction*
- WASD movement, mouse look, jumping, sprinting
- Uses Unity's New Input System
- Interacts with `IInteractable` objects
- Auto-creates camera if not assigned
- Fallback input actions if asset not found

#### **GameUI** (`PokerWar.UI.GameUI`)
*Event-driven UI controller*
- Subscribes to TimeManager, ScoreManager, GameManager events
- Updates timer, score, run displays automatically
- Shows interaction prompts for nearby `IInteractable` objects
- Manages cash-out panel and game over screen
- Color-coded timer (white → yellow → red)

### Configuration Assets (ScriptableObjects)

#### **GameConfig** (`PokerWar.Data.GameConfig`)
*Game balance and timing settings*
- Create via: `Assets → Create → PokerWar → Game Config`
- Settings: base time, time reductions, score multipliers, delays
- Methods: `CalculateTimeLimit(runNumber)`, `CalculateRunScore(timeRemaining, runNumber)`
- Allows tweaking without code changes

#### **WorldGenerationConfig** (`PokerWar.Data.WorldGenerationConfig`)
*World generation parameters*
- Create via: `Assets → Create → PokerWar → World Generation Config`
- Settings: world size, terrain height, noise scale, object counts, spawn ranges
- Supports multiple configs for different difficulty levels

### Interfaces

#### **IInteractable**
```csharp
bool IsInRange();
void Interact();
string GetInteractionPrompt();
```
- Implemented by: `ChestInteractable`
- Future use: Doors, NPCs, power-ups, terminals

#### **IWorldGenerator**
```csharp
void GenerateWorld(int seed);
void ClearWorld();
Vector3 GetCenterSpawnPosition();
Vector3 GetRandomSpawnPosition();
```
- Implemented by: `ProceduralWorldGenerator`
- Future use: Different generation algorithms, biome systems

#### **IGameStateObserver**
```csharp
void OnRunStarted(int runNumber, float timeLimit);
void OnChestCollected(int score);
void OnTimeUp();
void OnGameOver(int finalScore);
```
- Future use: Achievement system, analytics, cutscenes

### Event Flow Example

```
Player presses E
→ FirstPersonController.OnInteract()
→ IInteractable.Interact()
→ ChestInteractable.Collect()
→ GameManager.OnChestCollected()
→ ScoreManager.AddChestScore() → fires OnScoreChanged event
→ TimeManager.PauseTimer()
→ GameManager changes state → fires OnGameStateChanged event
→ GameUI.HandleGameStateChanged() → shows cash-out panel
```

### When Adding New Features

**Follow this pattern**:
1. Create interface if needed (`IMyFeature`)
2. Create ScriptableObject config if settings required
3. Implement system with single responsibility
4. Add C# events for state changes
5. Subscribe to events in observers (UI, etc.)
6. Use proper namespace (`PokerWar.MyFeature`)
7. Add XML documentation (`/// <summary>`)

**Best Practices**:
- Access systems via `GameManager.Instance.TimeManager` (not `FindObjectOfType`)
- Use event subscriptions instead of polling in `Update()`
- Store settings in ScriptableObjects, not hardcoded values
- Implement interfaces for extensibility
- Keep scripts under 300 lines (split if larger)
- Use `#region` to organize code sections

### Common Unity Pitfalls (CRITICAL - Read Before Coding!)

**GetComponent Limitations**:
- ❌ **NEVER** use `GetComponent<IInterfaceType>()` - Unity doesn't support this reliably
- ❌ **NEVER** use `GetComponent<ScriptableObject>()` - ScriptableObjects are NOT components
- ✅ **ALWAYS** use `GetComponent<ConcreteMonoBehaviourClass>()`
- Examples:
  ```csharp
  // WRONG - interfaces cause ArgumentException
  IInteractable interactable = chest.GetComponent<IInteractable>();
  // CORRECT - use concrete class
  IInteractable interactable = chest.GetComponent<ChestInteractable>();

  // WRONG - ScriptableObjects cause ArgumentException
  GameConfig config = gameManager.GetComponent<GameConfig>();
  // CORRECT - access via public property
  GameConfig config = GameManager.Instance.GameConfig;
  ```
- GetComponent only works with MonoBehaviour/Component types - interfaces are compile-time only, ScriptableObjects are assets not components

**ScriptableObject Asset References**:
- When assigning texture/material references in ScriptableObject assets (.asset files), **always verify GUIDs**
- Incorrect GUID = wrong asset loaded at runtime (even if it appears correct in Inspector)
- For this project:
  - ✅ Ground texture: `Grass01_BigUV.png` (GUID: `38bb15d6b378aff46b461e248592d1da`)
  - ✅ Location: `Assets/ALP_Assets/GrassFlowersFREE/Textures/Ground/Grass01_BigUV.png`
  - ✅ Grass detail textures: `Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass01.tga`, `grass02.tga`
  - ✅ Flower detail textures: `Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower01-10.tga`

**ScriptableObject Config Requirements (CRITICAL)**:
- **ALL managers require their config assigned** or they fail silently with errors
- **GameManager**: Needs `GameConfig` assigned (used for timing/scoring calculations)
- **TimeManager**: Needs `GameConfig` assigned (calculates time limits per run)
- **ScoreManager**: Needs `GameConfig` assigned (calculates scores based on time/run)
- **ProceduralWorldGenerator**: Needs `WorldGenerationConfig` assigned (terrain/object generation)
- **Symptom**: System reports "GameConfig not assigned!" or similar in Console
- **Fix**: Assign `DefaultGameConfig.asset` or `DefaultWorldGenerationConfig.asset` in Inspector
- Check for these errors IMMEDIATELY when adding new managers/systems to scene

**Player Tag Assignment**:
- Player prefab/GameObject **MUST** have tag "Player" for chest interaction to work
- `ChestInteractable.IsInRange()` uses `GameObject.FindGameObjectWithTag("Player")`
- Tag is auto-set in `FirstPersonController.Awake()` but verify in Inspector

**Unity UI RectTransform Requirement**:
- ❌ **NEVER** use `Transform` (Component ID: `!u!4`) for Canvas UI elements
- ✅ **ALWAYS** use `RectTransform` (Component ID: `!u!224`) for Canvas UI elements
- **Symptom**: If UI elements appear centered/positioned incorrectly, check if parent uses Transform instead of RectTransform
- **Fix**: Change `--- !u!4 &fileID` to `--- !u!224 &fileID` and add anchor/pivot properties:
  ```yaml
  m_AnchorMin: {x: 0, y: 0}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: 0, y: 0}
  m_Pivot: {x: 0.5, y: 0.5}
  ```
- All UI GameObjects under a Canvas **must** use RectTransform, not Transform

**Unity UI EventSystem Requirement**:
- **EventSystem is REQUIRED** for Unity UI to process button clicks and interactions
- **Symptoms if missing**:
  - Buttons don't respond to hover/click
  - UI elements are visible but not interactive
  - No errors in Console
- **Fix**: Add EventSystem GameObject to scene with two components:
  1. `EventSystem` component (GUID: `76c392e42b5098c458856cdf6eceffca`)
  2. `StandaloneInputModule` component (GUID: `4f231c4fb786f3946a6b90b886c48677`)
- Canvas also needs `GraphicRaycaster` component (GUID: `dc42784cf147c0c48a680349fa168899`) - usually auto-added
- **Only ONE EventSystem per scene** - Unity will warn if multiple exist

**FirstPersonController and UI Interaction**:
- When showing UI menus (cash-out panel, game over, etc.), **MUST disable FirstPersonController**
- Otherwise camera continues responding to mouse movement while menu is open
- Pattern:
  ```csharp
  // When showing menu
  playerController.enabled = false;
  Cursor.lockState = CursorLockMode.None;
  Cursor.visible = true;

  // When hiding menu
  playerController.enabled = true;
  Cursor.lockState = CursorLockMode.Locked;
  Cursor.visible = false;
  ```
- Find player dynamically: `GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>()`

### Platform Considerations
The project is configured for multi-platform development:
- Desktop/PC uses higher quality rendering settings
- Mobile uses optimized rendering with reduced features
- When testing performance, switch platform build target to verify optimization

## Best Practices for This Project

- **Input Handling**: Always use the New Input System API; legacy Input Manager should be avoided
- **Shaders**: Use URP shader graph or URP shader variants only
- **Scene References**: Avoid cross-scene references; use ScriptableObjects or singletons for shared data
- **Git**: Unity projects should use Unity's `.gitignore` (Library/, Temp/, obj/ directories should never be committed)
- **Asset Workflow**: Import settings for textures/models should be configured before use (consider mobile texture compression)

## Development Guidelines

### Scope Management (2-Hour Game Jam)
- **Keep it simple**: Prioritize core gameplay over polish
- **No feature creep**: Stick to the core loop (explore → find chest → decide)
- **Reuse existing assets**: SimpleNaturePack provides all visual needs
- **Rapid iteration**: Use editor tools for quick testing

### Implementation Priorities
1. **Core Loop First**: Get the basic gameplay working (spawn, find chest, timer)
2. **Polish Later**: Visual effects, sound, and juice are secondary
3. **Test Early**: Use Play Mode testing frequently
4. **Simple UI**: Text-based UI is sufficient for a game jam scope

### Code Quality for Learning
- This is a class project - code should be readable and well-commented
- Prefer clarity over optimization
- Use Unity best practices but don't over-engineer