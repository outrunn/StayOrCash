# UI Setup Guide

## Automatic UI Generation (Recommended)

### Step 1: Generate the UI Prefab

1. **Open Unity Editor**
2. **Go to top menu:** `Tools → Generate Game UI`
3. **Click the button:** `Generate HUD UI`
4. **Wait for confirmation dialog**

The script will automatically create:
- Canvas with proper scaling settings
- Timer display (top-left)
- Score display (top-right, gold color)
- Run number display (top-right, below score)
- Interaction prompt (bottom-center, hidden by default)
- All components properly anchored for different screen sizes
- TextMeshPro components with outlines for readability

**Output:** `Assets/Prefabs/UI/GameCanvas.prefab`

### Step 2: Add to Scene

1. **Locate the prefab** in Project window: `Assets/Prefabs/UI/GameCanvas.prefab`
2. **Drag it into your scene** Hierarchy
3. **Done!** The UI is ready to use

### Step 3: Verify Setup

The prefab already has:
- ✅ `GameUI` component attached
- ✅ All text references auto-assigned
- ✅ Proper canvas settings

You don't need to configure anything!

---

## What the UI Shows

### HUD Elements (Always Visible)

**Timer (Top-Left)**
- Format: `Time: MM:SS`
- Color changes based on remaining time:
  - White: > 20 seconds
  - Yellow: 10-20 seconds
  - Red: < 10 seconds (warning!)

**Score (Top-Right)**
- Format: `Score: XXXX`
- Gold color to stand out
- Updates automatically as you collect chests

**Run Number (Top-Right, Below Score)**
- Format: `Run: X`
- Shows which run you're on
- Helps track difficulty progression

**Interaction Prompt (Bottom-Center)**
- Format: `Press E to collect chest`
- Only appears when near a chest (within 3 meters)
- Light yellow color with bold text
- Automatically hides when you move away

---

## How the UI Works (Technical)

### GameUI.cs Script Behavior

The `GameUI` component on the Canvas does the following automatically:

1. **Every frame** (`Update()`):
   - Reads current time from `GameManager.Instance.CurrentTime`
   - Formats and displays timer
   - Changes timer color based on urgency
   - Updates score from `GameManager.Instance.TotalScore`
   - Updates run number from `GameManager.Instance.CurrentRun`
   - Checks if player is near chest and shows/hides prompt

2. **No manual updates needed!** Everything is automatic.

### Connection to Game Systems

```
GameManager (Singleton)
    ↓
    Provides: CurrentTime, TotalScore, CurrentRun, IsGameActive
    ↓
GameUI.Update() reads these values
    ↓
Updates text displays
```

---

## Customization (Optional)

After generation, you can customize the UI:

### Change Colors

1. Select `GameCanvas` prefab in Project window
2. Open in Prefab Mode (double-click)
3. Select any text element (Timer, Score, etc.)
4. In Inspector → TextMeshPro component → Color
5. Change as desired
6. Save prefab (auto-saves)

### Change Fonts

1. Import a custom font into Unity
2. Create a TextMeshPro Font Asset: `Assets → Create → TextMeshPro → Font Asset`
3. Select text element in prefab
4. Assign new font in TextMeshPro → Font Asset field

### Reposition Elements

1. Open prefab in Prefab Mode
2. Select element to move
3. Adjust `Rect Transform → Anchored Position` in Inspector
4. Or use the Rect Tool (T key) to drag in Scene view

### Add Backgrounds/Panels

1. Right-click in prefab Hierarchy → `UI → Image`
2. Position behind text elements
3. Adjust color and transparency
4. Makes text more readable against busy backgrounds

---

## Testing the UI

### In Editor Play Mode

1. **Enter Play Mode**
2. **Check that you see:**
   - Timer counting down
   - Score showing (starts at 0)
   - Run number (starts at 1)

3. **Find and approach chest:**
   - When within ~3 meters, prompt should appear
   - "Press E to collect chest"

4. **Press E to collect:**
   - Prompt disappears
   - Score increases
   - Game pauses for cash-out decision (to be implemented)

### Common Issues

**"UI doesn't appear"**
- Make sure `GameCanvas` is in the scene Hierarchy
- Check that Canvas → Render Mode is "Screen Space - Overlay"
- Verify Canvas component is enabled

**"Timer shows 00:00"**
- GameManager might not be running
- Check Console for errors
- Verify GameManager exists in scene

**"Prompt never shows"**
- Chest might not have ChestInteractable component
- Check interaction range (default 3m - might be too small)
- See TROUBLESHOOTING.md for chest setup

---

## Future UI Screens (To Be Generated)

The UI Generator can be extended to create:

### Cash-Out Screen (Coming Next)
- Appears after collecting chest
- Shows current score
- Two buttons: "Continue" or "Cash Out"
- Displays next run info (time limit reduction)

### Game Over Screen
- Appears when time runs out
- Shows "Time's Up!" message
- Displays runs completed
- "Try Again" button

### Final Score Screen
- Appears when player cashes out
- Shows final score
- Displays statistics (runs completed, time survived)
- "Play Again" button

---

## Advanced: Manual UI Creation

If you want to create UI manually instead of using the generator:

### Required Components

**Canvas:**
- Render Mode: Screen Space - Overlay
- Canvas Scaler: Scale with Screen Size
- Reference Resolution: 1920x1080
- Match: 0.5 (balance between width and height)

**Text Elements (use TextMeshPro):**
- Timer: TMP Text component
- Score: TMP Text component
- Run: TMP Text component
- Prompt: TMP Text component

**GameUI Script:**
- Attach to Canvas
- Drag text components to corresponding fields in Inspector

### Anchoring Guide

- **Top-Left** (Timer): Anchor (0, 1), Pivot (0, 1)
- **Top-Right** (Score): Anchor (1, 1), Pivot (1, 1)
- **Bottom-Center** (Prompt): Anchor (0.5, 0), Pivot (0.5, 0)

---

## Troubleshooting

### "Tools → Generate Game UI" menu doesn't exist

**Cause:** Script compilation error or not in Editor folder

**Fix:**
1. Check Console for compilation errors
2. Verify file is at: `Assets/Scripts/UI/Editor/UIGenerator.cs`
3. File must have `#if UNITY_EDITOR` at top
4. Restart Unity if needed

### Generated UI looks wrong

**Cause:** Missing TextMeshPro package

**Fix:**
1. Window → TextMeshPro → Import TMP Essential Resources
2. Regenerate the UI

### Can't customize prefab

**Fix:**
1. Select prefab in Project window (not scene)
2. Click "Open Prefab" button in Inspector
3. OR double-click prefab in Project window

---

## Quick Reference

| Element | Location | Color | Updates |
|---------|----------|-------|---------|
| Timer | Top-Left | White/Yellow/Red | Every frame |
| Score | Top-Right | Gold | On chest collect |
| Run | Top-Right Below Score | White | On new run |
| Prompt | Bottom-Center | Light Yellow | When near chest |

**Interaction Range:** 3 meters (configurable in ChestInteractable)

**Timer Warning Thresholds:**
- Red: < 10 seconds
- Yellow: 10-20 seconds
- White: > 20 seconds
