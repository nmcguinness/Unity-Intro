---
title: "Procedural Head Tracking — The Watcher in the Chamber"
subtitle: "Unity Animation Mini-Series — Lab 6 (Take-Home, Optional)"
topic_code: t06_head_tracking_aim_constraint
description: "A take-home lab that adds procedural head tracking to the character from Labs 2 and 4 using Unity's Animation Rigging package. The character's head turns to follow a target you move through the scene, with rotation limits matching real human cervical range."
created: 2026-05-05
last_updated: 2026-05-05
version: 1.0
status: published
authors: ["Games Development Teaching Team"]
tags: [unity, unity-6.3-lts, animation-rigging, multi-aim-constraint, procedural-animation, take-home, year1]
difficulty_tier: Advanced
unity_version: "6.4 LTS"
project_template: "3D (URP) Core"
duration_minutes: 75
previous_topic: t05_decorators_materials_particles
prerequisites:
  - Labs 1–5 completed
  - Comfortable installing packages from Unity's Package Manager
  - Comfortable reading and modifying short C# scripts
  - Lab 6 uses content from `REPO_LINK/Lab06_TakeHome/` (continuation of Lab 5 with chamber expanded, target-sphere prefab pre-configured, character rig pre-verified for head bone orientation)
---

# Procedural Head Tracking — The Watcher in the Chamber
> **Prerequisites:**
> - You have completed Labs 1–5.
> - You have your Blender `Character.fbx` and the `Lab05_Chamber.unity` scene as your starting state, or you can use the provided Lab 6 starter (recommended — it has the rig pre-verified).
> - You have cloned the labs repository (`REPO_LINK`). Open `REPO_LINK/Lab06_TakeHome/` as a Unity project.
> - This lab is **take-home and optional**. There is no time limit. Aim for 60–90 minutes; if it takes longer, you're learning more.

---

## **What you'll learn**

| Skill Type | You will be able to… |
| :-- | :-- |
| **Conceptual Understanding** | Explain what a constraint-based animation rig is, how it composes *on top of* an existing Animator, and why this approach scales better than scripted bone manipulation. |
| **Editor & Tool Fluency** | Install the Animation Rigging package, set up a Rig Builder + Rig hierarchy on a character, and configure a Multi-Aim Constraint with appropriate aim axis, up axis, and rotation limits. |
| **Code Implementation** | Read, configure, and tinker with a complete `TargetMover` script that translates a GameObject through the scene from keyboard input. |
| **Design Skills** | Choose realistic rotation limits that respect human cervical range, and tune constraint weight to balance authored animation with procedural override. |
| **Problem-Solving** | Diagnose the three classic Animation Rigging issues: head turned to wrong axis, head not turning at all, and head twisting at extreme angles. |

---

## **Why this matters**
The labs so far have given you two animation paradigms:
- **Authored animation** (Labs 1, 2, 5) — keyframed clips that play back identically every time.
- **Procedural animation** (Labs 3, 4) — code-driven motion that responds to runtime values.

Real games combine these. The character's body walks via an authored Walk clip — but its *head turns to look at threats, allies, and points of interest* via procedural overrides on top of the clip. This is sometimes called **layered animation** or **constraint-based rigging**, and it's how every modern third-person character is built. The Last of Us, Spider-Man, Uncharted, Hellblade — all of them play authored clips on the body while procedural systems handle the head, eyes, hands, and feet.

You'll add this layer to your character today. As they walk into the chamber from Lab 5, their head will track the spider you've been wondering about — turning to follow it as the player moves the target around. The character's body keeps walking via the Lab 2 Animator. The head responds to the world.

The result feels alive in a way no authored clip can match. A character whose head reacts to *what's actually in the scene* communicates awareness, intention, and attention — all the cues humans read as "this thing has a mind." That perception of mind is conferred entirely by *reaction*. The thing being looked at can be completely static — as the spider will be — and still feel present, because someone is *looking* at it.

This is why the lab is the series capstone. Once you've built it, you've encountered every major piece of Unity's animation toolkit: clips, controllers, parameters, scripts, materials, lights, decals, and now the Animation Rigging package. You're equipped to tackle any character animation problem that comes up in Year 2 and beyond.

---

## **How this builds on previous content**
**From Lab 2 you have:**
- A character with a Generic rig and `Idle` / `Walk` clips.

**From Lab 4 you have:**
- The character moving through space under input control, with the Animator's `Speed` parameter driving state transitions.

**From Lab 5 you have:**
- A decorated chamber scene with a static spider silhouette at the far end.

**Lab 6 layers procedural head tracking onto all of this:**
- The Animation Rigging package adds a constraint system that runs *after* the Animator, so it can override specific bones while leaving others untouched.
- A Multi-Aim Constraint applied to the head bone makes the head rotate to face a target Transform.
- Rotation limits ensure the head doesn't turn 270° to stare at something behind the character (which would be horrifying in a different way to what we want).
- A small script lets you move the target around the chamber to feel the system working.

The character's body keeps walking, idling, and responding to input exactly as before. The head layer is genuinely additive — comment out the Rig Builder component and everything from Labs 2–4 still works. **Decoration via composition, not modification** — the same lesson from Lab 5, applied to the character itself.

---

# **Core Ideas / Concepts**

> Each idea is introduced in some depth here because Lab 6 is take-home and you have time to read carefully. Read all four before starting the practical steps.

---

### **Core Idea 1 — Animation Rigging is a constraint system layered on top of the Animator**

Unity's Animator plays keyframed clips and blends between them. The Animator outputs a complete pose every frame: every bone has a position and rotation. Normally that pose goes straight to the rendered character.

Animation Rigging inserts itself *between* the Animator and the renderer. It takes the pose the Animator produced, applies a chain of **constraints** to it (override the head's rotation, reach the hand toward this target, plant the foot on the ground), and then sends the *modified* pose to the renderer. The Animator doesn't know this is happening; the constraint chain doesn't know what clips the Animator played. Both systems are independent.

**Snippet explanation:**
This is a form of the **decorator pattern** at the rendering level. The constraints "decorate" the Animator's output without altering the Animator. You can disable any constraint at runtime (set its weight to 0) and the Animator's output passes through untouched. The architecture lets you author body animation *traditionally* (in Blender, baked into clips) while handling reactive animation (head tracking, foot IK, hand placement) *procedurally*. Best of both worlds.

---

### **Core Idea 2 — A Rig is a hierarchy of constraints with one entry point**

Animation Rigging requires three pieces working together:

1. **Rig Builder** — a component on the character's root GameObject (next to the Animator). It registers Rigs and runs them after the Animator.
2. **Rig** — a GameObject (and component) that contains constraints. A character can have one or several Rigs (e.g., a "Combat Rig" with weapon-aim constraints and a separate "Locomotion Rig" with foot IK), each enabled or disabled independently.
3. **Constraints** — components that perform specific procedural overrides. Multi-Aim, Two-Bone IK, Chain IK, Multi-Position, and several others.

**Snippet explanation:**
Today you'll create the simplest possible setup: one Rig Builder, one Rig, one Multi-Aim Constraint on the head bone. The hierarchy looks like:

```
Character (has Animator + Rig Builder)
├── (skeleton hierarchy) ...
└── HeadTrackingRig (has Rig component)
    └── HeadAim (has Multi-Aim Constraint, with Constrained Object set to the head bone)
```

The `HeadTrackingRig` GameObject sits *next to* the skeleton, not inside it. This is a convention from the Animation Rigging documentation — control rigs live alongside skeletons, not within them.

---

### **Core Idea 3 — A Multi-Aim Constraint rotates a bone to face a target**

The Multi-Aim Constraint takes:
- A **Constrained Object** (the bone to rotate, e.g., the head).
- One or more **Source Objects** (the targets to look at — multiple weighted targets are supported, hence the "Multi" prefix).
- An **Aim Axis** (which local axis of the constrained object should point at the target — typically `+Z` for "forward").
- An **Up Axis** and **World Up** settings (which axis stays vertical, to prevent the head rolling sideways).
- **Min Limit** and **Max Limit** values for each constrained axis (the human-range constraints we want).

**Snippet explanation:**
Today you'll use a single source object (the target sphere). The Aim Axis must match how your character's head bone was modelled in Blender — for the standard Blender humanoid orientation, this is typically `+Z`. We've pre-verified this in the starter project; for your own characters in the future, you'll need to check.

The Min/Max Limits are the magic that keeps the head movement humanly plausible. Without them, the head would happily rotate 270° backwards. With them, you get the natural "looks toward, but not impossibly" behaviour you see in real games.

---

### **Core Idea 4 — Constraint Weight blends between authored and procedural**

Every constraint has a **Weight** slider from 0 to 1. At weight 0, the constraint is disabled and the Animator's output passes through. At weight 1, the constraint fully overrides the Animator's output for the affected bone. At weight 0.5, the bone is interpolated halfway between the two.

**Snippet explanation:**
This is your knob for blending between authored and procedural animation. A weight of 0.7 on the head Multi-Aim Constraint means "70% look at target, 30% follow the authored clip's head movement." This produces beautifully natural results because the authored animation's subtle head movements (idle breathing motion, walk-cycle bob) survive — the head tracks the target *while still being part of the body's overall motion*. Setting weight to 1.0 gives a "stiff" look-at where the head is fully locked to the target; lower values produce more organic results. We'll use 1.0 for the lab and you can experiment with lower values in the Tinker Tasks.

---

# **Progressive Lab Steps (A → B → C → D → E → F)**

> Take your time. There's no clock. Save your scene at the end of each step.

---

### Step A — Open the starter & install the Animation Rigging package (10 min)

Open the starter project at `REPO_LINK/Lab06_TakeHome/` in Unity 6.3 LTS. The scene `Assets/Scenes/Lab06_Chamber.unity` should open. It's similar to your Lab 5 final state, but the chamber is opened up so you can walk around inside it, and a static `Spider` GameObject sits at the far end.

The Animation Rigging package needs to be installed manually (it's not in the default Unity 6 set):

1. Open `Window > Package Manager`.
2. In the top-left dropdown, ensure `Packages: Unity Registry` is selected.
3. Search for **"Animation Rigging"** in the search bar.
4. Click `Install`.
5. Wait for installation to complete (~30 seconds). Close the Package Manager.

You'll see a few new menu items appear under `GameObject > Animation Rigging` and a new component category in the Add Component menu.

**Checkpoint:** Animation Rigging package shows as "installed" in Package Manager. Returning to the scene, the character is still standing in the chamber and the spider is still at the far end. No errors in the console.

---

### Step B — Add the Rig Builder and Rig hierarchy (15 min)

Now we'll set up the constraint hierarchy on the character. Take this slowly — each step adds one piece, and they all need to be in the right relationship to each other.

**B.1 — Add the Rig Builder.**
Select the `Character` GameObject in the Hierarchy (the root of the FBX, the one with the Animator). In the Inspector, click `Add Component` → search for `Rig Builder` → add it.

The Rig Builder component appears with one field: a **Rig Layers** list. It's empty for now.

**B.2 — Create the Rig GameObject.**
In the Hierarchy, right-click the `Character` GameObject → `Create Empty` (this creates a child of `Character`). Rename the new empty `HeadTrackingRig`.

Confirm in the Hierarchy that `HeadTrackingRig` is a *direct child* of `Character`, at the *same level* as the skeleton root (likely named `Hips` or `Armature`). It should NOT be inside the skeleton hierarchy.

With `HeadTrackingRig` selected, click `Add Component` → search for `Rig` → add it (the simple `Rig` component, not "Rig Builder").

**B.3 — Register the Rig with the Rig Builder.**
Select the `Character` GameObject again. In the Rig Builder component:
1. Click the `+` next to `Rig Layers` to add a new entry.
2. Drag the `HeadTrackingRig` GameObject from the Hierarchy into the new entry's `Rig` field.
3. Confirm `Active` is ticked next to it.

**B.4 — Create the target sphere.**
The character needs something to look at. In the Hierarchy, right-click → `3D Object > Sphere`. Rename it `LookTarget`. Set its scale to `(0.2, 0.2, 0.2)` so it's a small visible marker. Position it about `(2, 1.5, 2)` — somewhere in front of the character, at roughly head height.

Drag a bright material onto it from `Assets/Materials/` (the starter includes `LookTarget.mat`, a saturated red unlit material). The bright sphere will be visible in Game view as you move it around.

**Checkpoint:** Hierarchy contains `Character > HeadTrackingRig` (with Rig component) and a separate `LookTarget` sphere visible in the scene. The Character's Rig Builder has one Rig Layer pointing at `HeadTrackingRig`.

---

### Step C — Add and configure the Multi-Aim Constraint (15 min)

Now the procedural rotation logic. This is the most fiddly step in the lab — read each sub-step carefully.

**C.1 — Find the head bone.**
Expand the `Character`'s skeleton in the Hierarchy. Drill down through the bone names — typical paths are `Hips > Spine > Spine1 > Spine2 > Neck > Head`. The exact names depend on how your Blender lecturer named the rig; you're looking for the bone that visibly corresponds to the head/neck region. You might need to click bones one by one in the Scene view to find the right one.

Make a note of the head bone's exact GameObject name. You'll need it.

**C.2 — Create the constraint GameObject.**
In the Hierarchy, right-click `HeadTrackingRig` (the Rig GameObject you made in Step B.2) → `Create Empty`. Rename the new child `HeadAim`.

With `HeadAim` selected, click `Add Component` → search for `Multi-Aim Constraint` → add it.

**C.3 — Wire up the constraint.**
The Multi-Aim Constraint component has many fields. Configure them precisely:

- **Weight:** `1.0` (full override; we'll experiment with lower values later).
- **Constrained Object:** drag the head bone from the Hierarchy into this slot. The component now knows which bone to rotate.
- **Source Objects:** click the `+` to add an entry. Drag `LookTarget` (the sphere from Step B.4) into the `Transform` field. Set the weight column next to it to `1.0`.
- **Aim Axis:** `Z` (positive Z-axis — assumes Blender authored the head bone with Z+ as forward; this is the standard for Blender humanoid rigs).
- **Up Axis:** `Y` (positive Y-axis — the head's up direction).
- **World Up Type:** `Scene Up` (uses the scene's global Y-up to keep the head from rolling).
- **Maintain Rotation Offset:** *ticked* — preserves the head's authored rotation as a baseline rather than snapping to "look directly at target with no offset".
- **Constrained Axes:** all three (`X`, `Y`, `Z`) ticked — full rotation freedom subject to the limits we set next.
- **Min Limit:** `(-50, -60, -30)` — pitch min, yaw min, roll min in degrees.
- **Max Limit:** `(60, 60, 30)` — pitch max, yaw max, roll max in degrees.

The Min/Max values use the **comfortable human cervical range**:
- **Pitch (X axis): -50° to +60°** — the head can look down ~50° and up ~60° from its rest position before straining.
- **Yaw (Y axis): ±60°** — the head can rotate left/right by 60° before the shoulders need to follow.
- **Roll (Z axis): ±30°** — the head can tilt sideways by 30° before looking unnatural.

Beyond these ranges, real humans rotate their shoulders or whole upper body. We're not implementing that here, so the head simply stops at the limit. This produces the naturalistic "tries to look but can't quite reach" behaviour you see in good games.

**C.4 — Test it.**
Press Play. The character idles. Move the `LookTarget` sphere in the Scene view (or click on it and drag its Transform position handles in the Inspector). The character's head should rotate to follow it.

**Checkpoint:** Moving `LookTarget` makes the character's head turn. The head stops at the configured limits — you can prove this by moving the sphere directly behind the character; the head turns as far as it can but doesn't snap around.

If the head doesn't move, or moves to an unexpected axis, see Pitfalls below — it's almost certainly an Aim Axis or Constrained Object misconfiguration.

---

### Step D — Add the TargetMover script (15 min)

Moving the target by dragging it in the Inspector proves the constraint works, but a player should be able to move it with input. Time for a small script.

In `Assets/Scripts/`, create a new MonoBehaviour script called `TargetMover.cs`. Open it and replace its contents with:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves a target GameObject through the scene from keyboard input.
/// Reads the project-wide "Look" action (Vector2) from Unity 6's default input actions
/// and translates the target on the world XZ plane (with optional vertical movement).
///
/// REQUIRES:
///  - Unity 6's default project-wide actions (specifically the "Look" action).
///
/// Attach to: the LookTarget GameObject in the Lab06_Chamber scene.
/// </summary>
public class TargetMover : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Tooltip("Which input action to read. 'Look' is the default project-wide action bound to mouse delta and gamepad right stick. You can change this to 'Move' if you want WASD-style target control.")]
    private string actionName = "Look";

    [Header("Movement")]
    [SerializeField, Tooltip("How fast the target moves in world units per second.")]
    private float moveSpeed = 3f;

    [SerializeField, Tooltip("Multiplier on the input. Mouse delta values are typically much smaller than gamepad stick values, so you may need to tune this.")]
    private float inputSensitivity = 0.05f;

    [Header("Vertical Control")]
    [SerializeField, Tooltip("Hold this key to raise the target vertically. Default: E.")]
    private Key raiseKey = Key.E;

    [SerializeField, Tooltip("Hold this key to lower the target vertically. Default: Q.")]
    private Key lowerKey = Key.Q;

    [SerializeField, Tooltip("Vertical movement speed when Raise/Lower is held.")]
    private float verticalSpeed = 2f;

    [Header("Bounds")]
    [SerializeField, Tooltip("Optional: limit how far from origin the target can travel. Set to 0 to disable.")]
    private float maxDistanceFromOrigin = 10f;

    [SerializeField, Tooltip("Optional: minimum vertical position. Prevents the target dropping below the floor.")]
    private float minHeight = 0.2f;

    // Cached reference to the input action.
    private InputAction lookAction;

    private void Awake()
    {
        // Find the named action in Unity 6's project-wide input actions.
        lookAction = InputSystem.actions.FindAction(actionName);
    }

    private void Update()
    {
        // Read the input. "Look" returns mouse delta (small frame-by-frame values)
        // or gamepad right-stick position (-1 to +1). We treat both the same way.
        Vector2 input = lookAction.ReadValue<Vector2>();

        // Compute horizontal movement (XZ plane).
        // Multiplied by inputSensitivity to normalise mouse-vs-gamepad scales,
        // then by moveSpeed and Time.deltaTime for frame-rate-independent motion.
        Vector3 horizontalDelta = new Vector3(
            input.x * inputSensitivity,
            0f,
            input.y * inputSensitivity
        ) * moveSpeed * Time.deltaTime;

        // Compute vertical movement from the raise/lower keys.
        float verticalDelta = 0f;
        if (Keyboard.current[raiseKey].isPressed)
        {
            verticalDelta += verticalSpeed * Time.deltaTime;
        }
        if (Keyboard.current[lowerKey].isPressed)
        {
            verticalDelta -= verticalSpeed * Time.deltaTime;
        }

        // Apply movement to the target.
        Vector3 newPosition = transform.position + horizontalDelta + Vector3.up * verticalDelta;

        // Enforce bounds: clamp distance from origin (XZ only) and minimum height.
        if (maxDistanceFromOrigin > 0f)
        {
            Vector2 horizontal = new Vector2(newPosition.x, newPosition.z);
            if (horizontal.magnitude > maxDistanceFromOrigin)
            {
                horizontal = horizontal.normalized * maxDistanceFromOrigin;
                newPosition.x = horizontal.x;
                newPosition.z = horizontal.y;
            }
        }
        newPosition.y = Mathf.Max(newPosition.y, minHeight);

        transform.position = newPosition;
    }
}
```

Save and wait for compilation. Drag `TargetMover.cs` onto the `LookTarget` GameObject in the Hierarchy.

Press Play. Now:

- Move the **mouse** — the target slides horizontally through the scene, and the character's head follows.
- Hold **Q** — the target lowers; the character looks down at it.
- Hold **E** — the target raises; the character looks up at it.

If your mouse moves the target too fast, lower the `Input Sensitivity` field. If too slow, raise it. Mouse delta values are typically tiny per frame so the default `0.05` is conservative.

**Checkpoint:** Moving the mouse produces visible head tracking on the character. The target stays within the chamber bounds and doesn't fall below the floor.

---

### Step E — Walk to the spider, watch the head turn (10 min)

This is the payoff. Press Play. Walk the character (using `WASD` and `Shift`) from the corridor entrance, down the corridor, through the door, into the chamber. The whole way, your mouse continues to move the `LookTarget`.

Move the target to where the spider sits at the far end of the chamber. As the character walks toward it, the head turns to follow the spider. The character is *aware* of what's in the chamber — and the player feels that awareness through the head tracking, even though the spider itself does nothing.

Walk in a circle around the spider (still moving the target to it as you go). The head tracks continuously, hitting the rotation limits at extreme angles and stopping there. The character's body keeps walking via the Lab 2 Animator and the Lab 4 `PlayerController` script — completely untouched. The head layer is purely additive.

**Checkpoint:** The character can walk and look at the same time. The head reaches its rotation limits at extreme angles and stops there rather than snapping around. The whole scene feels alive — far more so than Lab 5's static silhouette.

*(Take a moment here. This is what 6 labs of progressively layered animation produce. Authored body, procedural head, decorated environment, all working together.)*

---

### Step F — Tune the feel (10 min)

The default constraint values produce a believable result, but tuning unlocks character-specific personality. Try these tweaks during Play mode:

**Constraint Weight (HeadAim's Weight slider).**
Lower it from 1.0 to 0.7. The head still tracks but the original walk-cycle head-bob comes through more strongly. The motion feels less robotic, more like the head is drawn to the target rather than locked onto it. Try 0.5 — even more organic. Try 0.3 — the tracking becomes a subtle lean rather than a turn.

**Limit values.**
Raise `Max Limit` Y to 90° (extending yaw to maximum human range). The head tries to look further behind — but if you push the target *too* far behind, the head still stops at the maximum and reads as "trying but unable" rather than impossible 270° rotation.

Lower `Min Limit` X to -90° (extending pitch-down beyond comfortable). When the target is on the floor near the character's feet, the head bows down sharply — useful for character reactions to dropped objects, but unnatural for general world awareness.

Set both Min and Max Y to ±15°. The head barely turns at all — the character looks *almost* at things but never directly. Useful for shy or distracted character personalities.

**Maintain Rotation Offset.**
Untick it. The head rotation no longer respects the original authored rotation — it snaps directly toward the target's literal direction, ignoring the natural head tilt the walk clip provides. The character looks slightly off-balance. Re-tick it and notice the difference.

**Checkpoint:** You've felt the difference between weight 1.0 / 0.7 / 0.5, and you understand what the limits are doing. You can articulate when each tuning choice would be appropriate for a different character personality.

---

# **Tinker Tasks**

> These go beyond the main lab. Pick at least three.

| Try this | Notice |
| :-- | :-- |
| Add a *second* Source Object to the Multi-Aim Constraint pointing at the corridor entrance, with weight 0.3, while LookTarget has weight 1.0 | The head averages between both points — a soft awareness of "behind me" while attending to "ahead" |
| Make the LookTarget invisible (disable its MeshRenderer) | The tracking still works — the constraint reads the Transform, not the visible mesh. Useful for production where you wouldn't want a debug sphere visible |
| Animate the Constraint's Weight property over time using `AnimationCurve` (callback to Lab 5) | Smooth fade-in / fade-out of head tracking — the system handles the blend automatically. Useful for cinematic moments where the character "becomes aware" of something |
| Change `Aim Axis` to `-Z` | The head rotates 180° wrong — the back of the head points at the target. Demonstrates exactly why Aim Axis must match the rig's authored forward direction |
| Untick `Constrained Axes > Z` (roll) | The head can no longer tilt sideways to look at extreme angles — a more "stiff" tracking that's actually closer to how military characters look at things |
| Move the LookTarget to inside the character's body | The head tries to look "into itself" — the limits prevent grotesque rotation but the result is uncomfortable. Confirms the limits are doing real work |

---

# **Useful Editor Tricks**

| Trick | Why it helps |
| :-- | :-- |
| Animation Rigging adds a small icon overlay on bones with constraints in Scene view | Lets you see at a glance which bones are constrained without inspecting components |
| Right-click a Rig component → `Bake to Animation Clip` | Bakes the constraint's procedural animation back into a regular `.anim` file — useful for shipping animations that don't need runtime constraint evaluation |
| Pause Play mode and scrub the Constraint Weight slider | See instantaneously how different weight values blend authored and procedural — much faster than playing repeatedly |
| In the Animator window, set `Speed` parameter to 0 while in Play mode | Stops the body's walk cycle but the head still tracks — proves the layers are independent |

---

# **Debugging & Pitfalls**

| Mistake | Why it happens | Fix |
| :-- | :-- | :-- |
| Head doesn't turn at all | Rig Builder doesn't reference the Rig, OR the Rig's `Active` toggle is off, OR the Multi-Aim Constraint's weight is 0 | Check all three. The Rig must be in the Rig Builder's `Rig Layers` list and active. The constraint's weight must be > 0. |
| Head turns to wrong axis (looks sideways or backwards) | Aim Axis doesn't match the head bone's forward direction | Try each of `X`, `-X`, `Y`, `-Y`, `Z`, `-Z` until the head looks correctly. For standard Blender rigs, `Z` is correct |
| Head twists weirdly when target is directly above or below | Up Axis configuration is wrong | Set Up Axis to `Y` and World Up Type to `Scene Up`. If still weird, try `World Up Type: Object Up` with `World Up Object` set to the character's root |
| Character's whole body rotates instead of just the head | Multi-Aim Constraint's Constrained Object is set to a higher-up bone (like Hips or Spine) | Drag the *Head* bone specifically into the Constrained Object slot, not a parent bone |
| Animation Rigging menu doesn't appear | Package not installed | Window → Package Manager → search Animation Rigging → Install |
| Constraint shows error: "Animator is not valid" | Rig Builder is on a GameObject that doesn't have an Animator | Move the Rig Builder to the Character root (which has the Animator). The Rig itself can be a child |
| Head tracking works in Editor but not in Build | Animation Rigging requires "Auto Setup from Target Skeleton" or an Animator with a valid Avatar | Ensure your Generic rig's Avatar Definition is set to "Create From This Model" in the FBX import |
| Mouse doesn't move the target | `InputSystem.actions.FindAction("Look")` returned null | Check Project Settings → Input System Package — confirm the project-wide actions asset exists with a Look action. Same fix as Lab 4's similar issue |
| Target falls below the floor | `minHeight` is too low or has been disabled | The default `minHeight = 0.2` should keep the target above a `y = 0` floor; raise if your floor is higher |
| Head jitters or flips at extreme angles | Min/Max limits create unreachable target positions causing the constraint solver to oscillate | Widen the limits slightly, or lower the constraint weight |

---

# **Reflective Questions**

- The character's body is animated by the Lab 2 Animator. The character's head is animated by Lab 6's Multi-Aim Constraint. They're independent systems. Why is this independence valuable? Can you imagine wanting them dependent in some scenarios?
- The spider in the chamber doesn't animate. It doesn't need to. The character's head tracks it, and that *reaction* makes the static silhouette feel present. What does this tell you about the source of perceived life in animated characters?
- You set Min/Max limits using comfortable human cervical range. What happens to the perceived character when you exceed those ranges? What about when you constrain *tighter*?
- Constraint Weight blends between authored and procedural. At what scenarios would you want weight 0.3 vs 0.7 vs 1.0? Can you imagine animating the weight itself over time?
- This lab uses *one* constraint on one bone. Real games stack constraints — head aim, eye aim, hand aim, foot IK, balance correction, weapon aim, all on one character simultaneously. What does that scaling imply about how Animation Rigging is architected?
- Lab 1 was about authoring keyframes. Lab 6 is about *reacting to runtime values*. Trace the journey across all six labs. Which lab was the turning point, in your opinion?

---

# **Software Development Parallel**
The constraint stack in Animation Rigging — multiple constraints applied in order, each one taking the previous output and modifying it — is structurally identical to a **middleware pipeline** in web frameworks (Express, Django, ASP.NET). Each middleware in the chain receives the request, modifies it, and passes it on; each constraint receives the pose, modifies it, and passes it on. The same architecture also appears in **image processing pipelines** (filters applied in sequence) and **shader passes** (each pass writes to a buffer the next pass reads). Recognising the pattern across domains makes each new instance easier to learn — the *idea* is more important than any particular instance.

---

# **Stretch Tasks (optional, beyond the lab)**

- **Add eye tracking.** Eyes have their own bones in many character rigs. Add a *second* Multi-Aim Constraint to each eye bone, with the same target as the head. The eyes will lead the head slightly — a hugely natural detail. Use higher weights on eyes (1.0) and lower on head (0.6) for a "the eyes notice first, the head follows" effect.
- **Make the spider react.** Once the head tracking is working, add a small script to the spider that detects when the character's head is looking at it (calculate the angle between `character.head.forward` and `(spider.position - character.head.position).normalized`). When the angle is small (<20°), trigger a subtle scale-pulse on the spider using DOTween. Now the spider reacts to *being looked at*. The chamber suddenly feels very different.
- **Add a chain to involve the upper body.** Add a *Chain IK Constraint* to the spine, pointing at the same target with weight 0.2. The character's torso now subtly leans toward what they're looking at — production-quality character behaviour.

These stretch tasks are open-ended and not walked through. They're where the curriculum hands off to your own curiosity.

---

## Files produced by end of lab
- `Lab06_HeadTracking/` Unity project (from starter)
- `Assets/Scripts/TargetMover.cs`
- `Assets/Scenes/Lab06_Chamber.unity` (modified with Rig Builder + Rig + Multi-Aim Constraint + LookTarget added)

---

## Lesson Context

```yaml
previous_lesson:
  topic_code: t05_decorators_materials_particles
  domain_emphasis: Games

this_lesson:
  topic_code: t06_head_tracking_aim_constraint
  primary_domain_emphasis: Games
  difficulty_tier: Advanced
  feeds_into: null
  status: Take-home, optional. Caps the Unity Animation Mini-Series.
```
