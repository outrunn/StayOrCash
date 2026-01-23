# Refactoring Guide - PokerWar

This document explains the modular refactoring of the PokerWar project.

## ✨ What Changed

The project has been refactored from a monolithic structure into a **modular, event-driven architecture** with clear separation of concerns, namespaces, and configuration data.

### Key Improvements

1. **Namespaces**: All scripts now use proper C# namespaces
2. **Interfaces**: Common behaviors defined through interfaces
3. **ScriptableObjects**: Configuration moved to data assets
4. **Event System**: Loose coupling through C# events
5. **Single Responsibility**: Each class has one clear purpose
6. **Modularity**: Systems can be tested and modified independently

## 📁 New Folder Structure

```
Assets/Scripts/
├── Core/
│   ├── Managers/
│   │   └── GameManager.cs           [Orchestrates all systems]
│   └── Systems/
│       ├── TimeManager.cs           [Countdown timer logic]
│       └── ScoreManager.cs          [Score tracking logic]
│
├── Data/
│   ├── GameConfig.cs                [Game settings ScriptableObject]
│   └── WorldGenerationConfig.cs    [World gen settings ScriptableObject]
│
├── Interfaces/
│   ├── IInteractable.cs             [Interface for interactive objects]
│   ├── IWorldGenerator.cs           [Interface for world generation]
│   └── IGameStateObserver.cs        [Interface for game event observers]
│
├── Player/
│   └── FirstPersonController.cs     [Player movement & input]
│
├── UI/
│   └── GameUI.cs                    [UI controller with event subscribers]
│
└── World/
    ├── ChestInteractable.cs         [Chest behavior, implements IInteractable]
    └── Generation/
        ├── ProceduralWorldGenerator.cs  [World gen, implements IWorldGenerator]
        └── Editor/
            └── ProceduralWorldGeneratorEditor.cs  [Custom inspector]
```

## 🔧 Architecture Breakdown

### 1. Managers (Orchestration Layer)

**GameManager** (`PokerWar.Managers.GameManager`)
- Singleton pattern for global access
- Orchestrates TimeManager, ScoreManager, and WorldGenerator
- Manages game state machine (Idle, Playing, ChestCollected, TimeUp, etc.)
- Fires events when game state changes
- Does NOT handle specific logic (delegated to systems)

**Why**: Single source of truth for game flow, but doesn't do the heavy lifting

### 2. Systems (Logic Layer)

**TimeManager** (`PokerWar.Systems.TimeManager`)
- Counts down time each run
- Calculates time limits based on GameConfig
- Fires events: `OnTimeUpdate`, `OnTimeUp`
- Provides methods: `StartTimer()`, `PauseTimer()`, `ResumeTimer()`, `AddTime()`

**ScoreManager** (`PokerWar.Systems.ScoreManager`)
- Tracks current run, total score, run score
- Calculates score based on GameConfig formulas
- Fires events: `OnScoreChanged`, `OnRunChanged`
- Provides methods: `StartNewRun()`, `AddChestScore()`, `ResetScore()`

**Why**: Each system is independently testable and reusable

### 3. Data (Configuration Layer)

**GameConfig** (`PokerWar.Data.GameConfig`)
- ScriptableObject asset created in Unity Editor
- Stores: base time, time reductions, score multipliers, delays
- Methods: `CalculateTimeLimit(runNumber)`, `CalculateRunScore(timeRemaining, runNumber)`
- Created via: `Assets -> Create -> PokerWar -> Game Config`

**WorldGenerationConfig** (`PokerWar.Data.WorldGenerationConfig`)
- ScriptableObject for world generation settings
- Stores: world size, terrain height, noise scale, object counts, scale ranges
- Created via: `Assets -> Create -> PokerWar -> World Generation Config`

**Why**: Settings can be tweaked in editor without code changes, multiple configs can exist for different difficulty levels

### 4. Interfaces (Contract Layer)

**IInteractable**
```csharp
bool IsInRange();
void Interact();
string GetInteractionPrompt();
```
- Implemented by: `ChestInteractable`
- Future: Doors, NPCs, power-ups, etc.

**IWorldGenerator**
```csharp
void GenerateWorld(int seed);
void ClearWorld();
Vector3 GetCenterSpawnPosition();
Vector3 GetRandomSpawnPosition();
```
- Implemented by: `ProceduralWorldGenerator`
- Future: Different generation algorithms, biome systems

**IGameStateObserver**
```csharp
void OnRunStarted(int runNumber, float timeLimit);
void OnChestCollected(int score);
void OnTimeUp();
void OnGameOver(int finalScore);
```
- Future: Achievement system, analytics, cutscenes

**Why**: Other classes can work with interfaces without knowing implementation details

### 5. Event System

**How Events Work**:
1. System fires event: `timeManager.OnTimeUp?.Invoke()`
2. Subscribers get notified: `GameUI.HandleTimeUp()`
3. No direct dependencies between systems

**Event Flow Example**:
```
Player presses E
→ FirstPersonController.OnInteract()
→ ChestInteractable.Interact()
→ GameManager.OnChestCollected()
→ ScoreManager.AddChestScore() [fires OnScoreChanged event]
→ TimeManager.PauseTimer()
→ GameManager.OnGameStateChanged?.Invoke(GameState.ChestCollected)
→ GameUI.HandleGameStateChanged() [shows cash-out panel]
```

**Why**: Loose coupling means changing UI doesn't break game logic

## 🔄 Migration Path

### Old vs New

#### Old GameManager (Monolithic)
```csharp
public class GameManager : MonoBehaviour
{
    private int currentRun;
    private int totalScore;
    private float currentTime;

    void Update() {
        // Timer logic here
        currentTime -= Time.deltaTime;
        // Score calculation here
        // UI updates here
    }
}
```

#### New GameManager (Modular)
```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ScoreManager scoreManager;

    // No Update() - systems handle their own logic
    // GameManager just coordinates
}
```

### Setting Up Refactored Project

#### 1. Create Configuration Assets

**GameConfig**:
1. Right-click in Project window
2. `Create -> PokerWar -> Game Config`
3. Name it `DefaultGameConfig`
4. Set values in Inspector:
   - Base Time: 60
   - First Reduction: 5
   - Reduction Increment: 5
   - Minimum Time: 10
   - Time Score Multiplier: 10
   - Run Score Bonus: 100

**WorldGenerationConfig**:
1. Right-click in Project window
2. `Create -> PokerWar -> World Generation Config`
3. Name it `DefaultWorldConfig`
4. Set values (or use defaults)

#### 2. Update GameManager GameObject

1. Find/Create `GameManager` GameObject in scene
2. Add components:
   - `GameManager` (PokerWar.Managers)
   - `TimeManager` (PokerWar.Systems)
   - `ScoreManager` (PokerWar.Systems)

3. Assign references on `GameManager`:
   - Game Config: Drag `DefaultGameConfig` asset
   - Time Manager: Drag `TimeManager` component (same GameObject)
   - Score Manager: Drag `ScoreManager` component (same GameObject)
   - World Generator: Drag `WorldGenerator` GameObject
   - Player Prefab: Drag Player prefab

4. Assign configs on `TimeManager`:
   - Game Config: Drag `DefaultGameConfig` asset

5. Assign configs on `ScoreManager`:
   - Game Config: Drag `DefaultGameConfig` asset

#### 3. Update WorldGenerator GameObject

1. Find/Create `WorldGenerator` GameObject
2. Remove old `ProceduralWorldGenerator` component (if exists)
3. Add NEW `ProceduralWorldGenerator` (PokerWar.World)
4. Assign references:
   - Config: Drag `DefaultWorldConfig` asset
   - Chest Prefab: Drag chest prefab
   - Click "Auto-Assign SimpleNaturePack Prefabs" button

#### 4. Update Player Prefab

1. Open Player prefab
2. Remove old `FirstPersonController` (if exists)
3. Add NEW `FirstPersonController` (PokerWar.Player)
4. Ensure `CharacterController` component exists
5. Player should auto-create camera

#### 5. Update Chest Prefab

1. Open Chest prefab
2. Remove old `ChestInteractable` (if exists)
3. Add NEW `ChestInteractable` (PokerWar.World)
4. Set interaction range: 3
5. Set interaction prompt: "Press E to collect chest"

#### 6. Update UI

1. Find Canvas GameObject
2. GameUI component should auto-update (same namespace)
3. Ensure all Text elements are assigned
4. GameUI will automatically subscribe to events

## 🎯 Benefits

### For Development
- **Easier Testing**: Test TimeManager without GameManager
- **Faster Iteration**: Change time formula in GameConfig asset
- **Clear Ownership**: Bug in timer? Check TimeManager
- **Reusability**: Use ScoreManager in different game modes

### For Collaboration
- **Less Merge Conflicts**: Different systems in different files
- **Clear Contracts**: Interfaces define expectations
- **Documentation**: Namespaces group related code

### For Performance
- **Event-Driven**: UI only updates when needed
- **No Polling**: TimeManager fires events instead of UI checking every frame
- **Modular Loading**: Could load systems asynchronously

### For Future Features
- **Easy to Extend**: Implement IInteractable for new objects
- **Swappable Systems**: Different WorldGenerator implementations
- **Multiple Configs**: Easy mode, hard mode, speedrun mode

## 📋 Checklist

When adding new features, follow this pattern:

- [ ] Create interface if needed (`IMySystem`)
- [ ] Create ScriptableObject config if needed (`MySystemConfig`)
- [ ] Implement system with single responsibility
- [ ] Add events for state changes
- [ ] Subscribe to events in relevant observers (UI, etc.)
- [ ] Use namespaces (`PokerWar.MyFeature`)
- [ ] Document with XML comments (`/// <summary>`)

## 🚨 Common Pitfalls

### ❌ Don't Do This
```csharp
// Accessing systems directly
TimeManager timeManager = FindObjectOfType<TimeManager>();
```

### ✅ Do This Instead
```csharp
// Use GameManager singleton
TimeManager timeManager = GameManager.Instance.TimeManager;
```

### ❌ Don't Do This
```csharp
// Hardcoded values
float baseTime = 60f;
```

### ✅ Do This Instead
```csharp
// Use ScriptableObject config
float baseTime = gameConfig.baseTime;
```

### ❌ Don't Do This
```csharp
// Polling in Update
void Update() {
    if (timeManager.CurrentTime <= 0) {
        HandleTimeUp();
    }
}
```

### ✅ Do This Instead
```csharp
// Event subscription
void Start() {
    timeManager.OnTimeUp += HandleTimeUp;
}
```

## 🔮 Future Enhancements

With this architecture, you can easily add:

1. **Save System**: ScoreManager data → JSON → PlayerPrefs
2. **Achievement System**: Implement IGameStateObserver
3. **Multiple Biomes**: Create new WorldGenerationConfig assets
4. **Power-Ups**: Implement IInteractable interface
5. **Difficulty Modes**: Create different GameConfig assets
6. **Multiplayer**: Events can broadcast to network
7. **Analytics**: Observer pattern logs events
8. **Modding Support**: Expose interfaces for plugins

## 📚 Further Reading

- [C# Events and Delegates](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- [Unity ScriptableObjects](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Observer Pattern](https://refactoring.guru/design-patterns/observer)

---

**Remember**: This refactoring maintains 100% functionality while improving maintainability. All gameplay works exactly as before, just cleaner under the hood!
