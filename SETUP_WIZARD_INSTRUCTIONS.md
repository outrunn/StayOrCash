# Setup Wizard - Quick Instructions

**Automated setup for PokerWar using Unity Editor script**

---

## How to Run the Wizard

1. **Open Unity Editor** with the PokerWar project
2. **Make sure you're in the correct scene** (SampleScene or mainscene)
3. Go to Unity menu: **Tools → PokerWar → Run Setup Wizard**
4. Review the wizard window information
5. Click **"Run Setup"** button
6. Wait for completion (usually < 5 seconds)
7. Review the detailed report

---

## What the Wizard Automates

The wizard handles 95% of the MASTER_SETUP_GUIDE.md automatically:

### ✅ Configuration Assets
- Creates `Assets/DefaultGameConfig.asset` with proper values:
  - Base Time: 60 seconds
  - First Reduction: 5 seconds
  - Reduction Increment: 5 seconds
  - Minimum Time: 10 seconds
  - Scoring formulas configured

- Creates `Assets/DefaultWorldConfig.asset` with proper values:
  - World Size: 100×100 units
  - Tree/Bush/Rock counts configured
  - Chest spawn distance: 20-40 units

### ✅ GameManager Setup
- Creates `GameManager` GameObject in scene
- Adds three components:
  - `GameManager` (PokerWar.Managers)
  - `TimeManager` (PokerWar.Systems)
  - `ScoreManager` (PokerWar.Systems)
  - `PersistentDataManager` (PokerWar.Systems)
- Wires all internal references automatically
- Assigns GameConfig to all systems

### ✅ WorldGenerator Setup
- Creates `WorldGenerator` GameObject in scene
- Adds `ProceduralWorldGenerator` component
- Assigns WorldGenerationConfig
- Finds and assigns Chest prefab (if exists)

### ✅ Reference Wiring
- Links GameManager → WorldGenerator
- Links GameManager → TimeManager
- Links GameManager → ScoreManager
- Finds and assigns Player prefab (if exists)
- Finds and assigns Chest prefab (if exists)

### ✅ UI Setup
- Instantiates GameCanvas prefab into scene
- Verifies GameUI component is present

### ✅ Scene Configuration
- Removes Main Camera (Player creates its own)
- Adds Directional Light if missing
- Configures light color and angle

### ✅ Validation
- Checks all critical references
- Reports any missing assignments
- Lists remaining manual steps

---

## Remaining Manual Steps (2 minutes)

After the wizard completes, you need to do these final steps:

### 1. Auto-Assign SimpleNaturePack Prefabs (30 seconds)

**Why manual?** This requires the custom editor inspector to be active.

**Steps:**
1. In Hierarchy, select `WorldGenerator`
2. Look in Inspector for `ProceduralWorldGenerator` component
3. Click the **"Auto-Assign SimpleNaturePack Prefabs"** button
4. Wait for console message: "Successfully assigned X prefabs"

**What this does:** Populates the tree, bush, rock, grass, flower, and mushroom arrays with assets from SimpleNaturePack.

### 2. (Optional) Assign Skybox Material (30 seconds)

**Why manual?** This requires user preference for which skybox to use.

**Steps:**
1. Top menu: **Window → Rendering → Lighting**
2. Click **Environment** tab
3. **Skybox Material**: Drag a skybox material from Project window
   - Unity includes default skyboxes
   - Search "skybox" in Project to find available options
4. Click **Generate Lighting** at bottom (optional, improves visuals)

**Why recommended:** Makes the world look much better and provides ambient lighting.

### 3. Save the Scene (5 seconds)

**Steps:**
1. **File → Save Scene** (or press Ctrl/Cmd+S)
2. If prompted for name, keep current or choose new name

### 4. Test the Game (immediate)

**Steps:**
1. Press **Play** ▶ (or Ctrl/Cmd+P)
2. You should see:
   - Procedurally generated terrain with nature objects
   - Player spawns at center in first-person view
   - UI shows timer (60:00), score (0), run (1)
   - Can move with WASD, look with mouse
3. Find the rotating golden chest
4. Press **E** to collect when prompt appears
5. World regenerates with new layout and less time

---

## Troubleshooting the Wizard

### "Setup Wizard" menu item doesn't appear

**Causes:**
- Script compilation error
- Script not in Editor folder

**Fix:**
1. Check Unity Console for red errors
2. Verify file is at: `Assets/Scripts/Editor/SetupWizard.cs`
3. Restart Unity if needed

### Wizard shows errors/warnings in report

**Read the report carefully** - it tells you exactly what's missing:

**Common Issues:**

**"Player prefab not found"**
- Check that `Assets/Prefabs/Player.prefab` exists
- If missing, you need to create the Player prefab manually
- See MASTER_SETUP_GUIDE.md Step 2 for details

**"Chest prefab not found"**
- Check that `Assets/Prefabs/Chest.prefab` exists
- If missing, you need to create the Chest prefab manually
- See MASTER_SETUP_GUIDE.md Step 1 for details

**"GameCanvas prefab not found"**
- Check that `Assets/Prefabs/UI/GameCanvas.prefab` exists
- If missing, generate it: **Tools → Generate Game UI → Generate HUD UI**
- Or create UI manually per UI_SETUP_GUIDE.md

### Wizard completes but game doesn't work

**Most common cause:** SimpleNaturePack prefabs not assigned

**Fix:**
1. Select WorldGenerator in Hierarchy
2. Click "Auto-Assign SimpleNaturePack Prefabs" button
3. Check Console for success message
4. Try playing again

**Other causes:**
- Check Console for runtime errors
- Verify all validation items in wizard report passed
- See TROUBLESHOOTING.md for detailed debugging

---

## Wizard Report Interpretation

The wizard shows a detailed report with symbols:

- **✓** = Success, action completed
- **⚠** = Warning, something needs attention (but not critical)
- **❌** = Error, something failed (wizard will explain)

**Example Report:**
```
=== POKERWAR SETUP WIZARD ===

STEP 1: Creating Configuration Assets
--------------------------------------
✓ Created GameConfig at: Assets/DefaultGameConfig.asset
  - Base Time: 60s
  - First Reduction: 5s
  - Reduction Increment: 5s
✓ Created WorldGenerationConfig at: Assets/DefaultWorldConfig.asset
  - World Size: 100x100
  - Tree Count: 50
  - Chest Distance: 20-40 units

STEP 2: Setting up GameManager
--------------------------------------
✓ Created GameManager GameObject
✓ Added GameManager component
✓ Added TimeManager component
✓ Added ScoreManager component
✓ Assigned GameConfig to GameManager
✓ Assigned GameConfig to TimeManager
✓ Assigned GameConfig to ScoreManager

... (continues for all steps)

===========================================
SETUP COMPLETE!
===========================================

REMAINING MANUAL STEPS:
1. Select 'WorldGenerator' in Hierarchy
2. Click 'Auto-Assign SimpleNaturePack Prefabs' button in Inspector
3. (Optional) Assign Skybox: Window → Rendering → Lighting → Skybox Material
4. Save the scene: File → Save Scene (Ctrl/Cmd+S)
5. Press Play to test!
```

---

## Re-Running the Wizard

You can run the wizard multiple times safely:

**What happens:**
- Existing assets are **reused** (not overwritten)
- Existing GameObjects are **updated** (not duplicated)
- References are **re-wired** (fixes broken links)
- Scene is **validated** again

**When to re-run:**
- References got disconnected
- Want to reset to clean state
- Troubleshooting setup issues
- Switched scenes and need setup in new scene

**How to reset completely:**
1. Delete GameManager and WorldGenerator from Hierarchy
2. Delete DefaultGameConfig.asset and DefaultWorldConfig.asset
3. Delete GameCanvas from Hierarchy
4. Run wizard again

---

## Comparison: Manual vs Wizard Setup

| Task | Manual Time | Wizard Time |
|------|-------------|-------------|
| Create config assets | 5 min | Instant |
| Setup GameManager | 5 min | Instant |
| Setup WorldGenerator | 3 min | Instant |
| Wire all references | 5 min | Instant |
| Add UI Canvas | 2 min | Instant |
| Configure scene | 2 min | Instant |
| SimpleNaturePack assign | 1 min | 30 sec (manual) |
| Skybox (optional) | 1 min | 30 sec (manual) |
| Save scene | 5 sec | 5 sec (manual) |
| **TOTAL** | **23-24 min** | **~2 min** |

**Time saved: ~20 minutes!**

---

## What's Next After Setup

Once setup is complete and tested:

### Immediate
1. Play the game and verify everything works
2. Check Console for any errors during gameplay
3. Collect a chest and verify world regenerates

### Short Term (Optional Polish)
- Replace cube chest with 3D model
- Add glowing material to chest
- Add footstep sounds
- Add chest collection sound effect
- Improve terrain texturing

### Medium Term (Features)
- Implement cash-out UI panel with buttons
- Add game over screen
- Add main menu scene
- Add save system for high scores

See **MASTER_SETUP_GUIDE.md** "Next Steps" section for detailed enhancement ideas.

---

## Support

If you encounter issues:

1. **Check the wizard report** - it's very detailed
2. **Check Unity Console** - errors will appear there
3. **Review TROUBLESHOOTING.md** - covers common issues
4. **Review MASTER_SETUP_GUIDE.md** - comprehensive manual setup guide
5. **Check CLAUDE.md** - project architecture documentation

---

## Summary

**The Setup Wizard automates 95% of the setup process:**

✅ Creates configuration assets
✅ Sets up all GameObjects and components
✅ Wires all references automatically
✅ Configures scene lighting
✅ Validates everything

**You only need to:**
1. Click "Auto-Assign SimpleNaturePack Prefabs" button (30 seconds)
2. (Optional) Assign skybox material (30 seconds)
3. Save scene (5 seconds)
4. Press Play!

**Total time from wizard to playable: ~2 minutes!**

Enjoy your procedurally generated treasure hunt game! 🎮
