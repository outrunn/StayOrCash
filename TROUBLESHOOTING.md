# Troubleshooting Guide

## Issues Fixed in Code

### 1. ✅ Terrain Material Issue
**Problem:** Terrain appeared pink/without material
**Fix:** Added automatic material creation in `ProceduralWorldGenerator.cs`
- Creates a default green Standard shader material at runtime
- Assigns it to terrain automatically

### 2. ✅ Chest Interaction Not Working
**Problem:** Pressing 'E' near chest did nothing
**Fix:** Added comprehensive debug logging to track down the issue

---

## Common Setup Issues & Solutions

### Issue: "E key does nothing when near chest"

**Possible Causes:**

1. **GameManager and WorldGenerator not connected**
   - GameManager needs ProceduralWorldGenerator component attached
   - Check Inspector: Select GameManager → should see ProceduralWorldGenerator component

2. **Chest needs a Collider**
   - Your chest prefab MUST have a collider (BoxCollider, SphereCollider, etc.)
   - Without it, the chest exists but has no physical presence

3. **Player reference not set**
   - The chest needs to know where the player is
   - This happens automatically in `GameManager.SpawnPlayer()`
   - Check console for "Player reference is null!" warnings

**How to Fix:**

```
Unity Editor Setup:
1. Select GameManager in Hierarchy
2. In Inspector, verify these components exist:
   - GameManager script
   - ProceduralWorldGenerator script
   - WorldInitializer script (optional, helps with auto-generation)

3. Select your chest prefab (in Project window)
4. In Inspector, add a collider if missing:
   - Add Component → Physics → Box Collider (or Sphere Collider)
   - Make sure it's NOT set to "Is Trigger"

5. Play the game and check the Console for debug messages
```

### Issue: "Objects have no materials (pink/magenta)"

**Cause:** Prefabs are missing material assignments

**Fix:**
1. **For SimpleNaturePack prefabs:**
   - These should have materials already
   - If they're pink, the materials might be using the wrong shader
   - Select a pink prefab → Inspector → Material → Shader → Change to "Standard"

2. **For Terrain:**
   - Now handled automatically by the code
   - If still pink, check console for errors

3. **For Chest:**
   - Your chest prefab needs a material
   - Create a simple material: Assets → Create → Material
   - Drag it onto the chest prefab's Renderer component

### Issue: "Can walk through chest"

**This is expected!** The chest collision is working correctly. The chest has a collider that stops you from walking through it.

If you CAN walk through it:
- Chest prefab needs a Collider component (see above)
- Collider must NOT be set to "Is Trigger"

---

## Debug Tools Added

### 1. Console Logging
When you press 'E', you'll see messages like:
```
E key pressed - attempting interaction!
Player in range! Distance: 2.5m (max: 3m)
Chest collected!
```

If something's wrong, you'll see:
```
GameManager.Instance is null!
// OR
ProceduralWorldGenerator not found on GameManager!
// OR
Player is NOT in range of chest.
```

### 2. Visual Gizmos (Scene View Only)
- **Yellow wireframe sphere** around chest = interaction range (3 meters)
- **Green line** = player in range
- **Red line** = player out of range

To see these: Make sure "Gizmos" button is enabled in Scene view (top right)

### 3. Setup Verifier Script
Attach `SetupVerifier` to your GameManager to get automatic diagnostics:
```
=== GAME SETUP VERIFICATION ===
✅ GameManager found
✅ ProceduralWorldGenerator attached to GameManager
✅ FirstPersonController found in scene
...
```

---

## Checklist Before Testing

- [ ] GameManager GameObject exists in scene
- [ ] GameManager has ProceduralWorldGenerator component
- [ ] Chest prefab assigned in ProceduralWorldGenerator (Inspector)
- [ ] Chest prefab has a Collider component
- [ ] Chest prefab has a material/renderer
- [ ] Player prefab assigned in GameManager
- [ ] Console window is open to see debug messages
- [ ] Gizmos enabled in Scene view

---

## Testing Steps

1. **Enter Play Mode**
   - Watch console for "Terrain created..." and "Chest spawned..." messages
   - If you don't see these, world generation didn't run

2. **Find the Chest**
   - Chest spawns 20-40 units from center
   - Rotates and bobs up/down for visibility
   - Look for yellow gizmo sphere in Scene view

3. **Approach Chest**
   - Walk toward it with WASD
   - In Scene view, watch the line between player and chest turn green when in range

4. **Press E**
   - Console should show: "E key pressed - attempting interaction!"
   - Then: "Player in range! Distance: X.XXm"
   - Then: "Chest collected!"
   - Chest should disappear

5. **If Nothing Happens**
   - Check console for error messages
   - Use the messages to identify the issue
   - See "Common Setup Issues" above

---

## Quick Reference: Interaction Range

- Default interaction range: **3 meters**
- To change: Select chest in scene → Inspector → ChestInteractable → Interaction Range
- Yellow gizmo shows the exact range

---

## Still Having Issues?

Run the Setup Verifier:
1. Select GameManager in Hierarchy
2. Right-click on SetupVerifier component → "Verify Setup"
3. Check console for diagnostic results
4. Fix any ❌ or ⚠️ issues listed
