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
duration_minutes: 90
previous_topic: t05_decorators_materials_particles
prerequisites:
  - Labs 1–5 completed
  - Comfortable installing packages from Unity's Package Manager
  - Comfortable reading and modifying short C# scripts
  - Uses the [repo](https://github.com/nmcguinness/Unity-Intro).
---

# Procedural Head Tracking — The Watcher in the Chamber
> **Prerequisites:**
> - You have completed Labs 1–5.
> - You have your Blender `Character.fbx` and the `Lab05_Chamber.unity` scene as your starting state, or you can use the provided Lab 6 starter (recommended — it has the rig pre-verified).
> - You have cloned the labs [repo](https://github.com/nmcguinness/Unity-Intro) to your machine.
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
Today you'll create: one Rig Builder, one Rig, and two stacked Multi-Aim Constraints on the head bone — one for yaw (left/right) and one for pitch (up/down). The hierarchy looks like:

```
Character (has Animator + Rig Builder)
├── (skeleton hierarchy) ...
└── HeadTrackingRig (has Rig component)
    ├── HeadYaw   (has Multi-Aim Constraint — Y axis only, ±60°)
    └── HeadPitch (has Multi-Aim Constraint — X axis only, −50° to +60°)
```

The `HeadTrackingRig` GameObject sits *next to* the skeleton, not inside it. This is a convention from the Animation Rigging documentation — control rigs live alongside skeletons, not within them.

---

### **Core Idea 3 — A Multi-Aim Constraint rotates a bone to face a target**

The Multi-Aim Constraint takes:
- A **Constrained Object** (the bone to rotate, e.g., the head).
- One or more **Source Objects** (the targets to look at — multiple weighted targets are supported, hence the "Multi" prefix).
- An **Aim Axis** (which local axis of the constrained object should point at the target — typically `+Z` for "forward").
- An **Up Axis** and **World Up** settings (which axis stays vertical, to prevent the head rolling sideways).
- A single **Min Limit** and **Max Limit** scalar — one pair per constraint, not per axis. To get independent limits for pitch and yaw, you stack two constraints: one that only constrains the Y axis (yaw), and one that only constrains the X axis (pitch). Each enforces its own Min/Max independently.

**Snippet explanation:**
Today you'll use a single source object (the target sphere). The Aim Axis must match how your character's head bone was modelled in Blender — Step C.5 walks you through finding the right value for your specific rig. The Min/Max limits keep head rotation humanly plausible: without them, the head rotates 270° backwards. Two stacked constraints give you the independent per-axis limits a single constraint cannot provide.

---

### **Core Idea 4 — Constraint Weight blends between authored and procedural**

Every constraint has a **Weight** slider from 0 to 1. At weight 0, the constraint is disabled and the Animator's output passes through. At weight 1, the constraint fully overrides the Animator's output for the affected bone. At weight 0.5, the bone is interpolated halfway between the two.

**Snippet explanation:**
This is your knob for blending between authored and procedural animation. A weight of 0.7 on the head Multi-Aim Constraint means "70% look at target, 30% follow the authored clip's head movement." This produces beautifully natural results because the authored animation's subtle head movements (idle breathing motion, walk-cycle bob) survive — the head tracks the target *while still being part of the body's overall motion*. Setting weight to 1.0 gives a "stiff" look-at where the head is fully locked to the target; lower values produce more organic results. We'll use 1.0 for the lab and you can experiment with lower values in the Tinker Tasks.

---

# **Progressive Lab Steps (A → B → C → D → E → F → G)**

> Take your time. There's no clock. Save your scene at the end of each step.

---

### Step A — Open the starter & install the Animation Rigging package (10 min)

Open the starter scene `Assets/Scenes/Lab06_Chamber.unity`. It's similar to your Lab 5 final state, but the chamber is opened up so you can walk around inside it, and a static `Spider` GameObject sits at the far end.

The Animation Rigging package needs to be installed manually (it's not in the default Unity 6 set):

1. Open `Window > Package Management > Package Manager`.
2. In the top-left dropdown, ensure `Packages: Unity Registry` is selected.
3. Search for **"Animation Rigging"** in the search bar.
4. Click `Install`.
5. Wait for installation to complete (~30 seconds). Close the Package Manager.

You'll see a few new menu items appear under `Animation Rigging` in the main menu and a new component category in the Add Component menu.

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

### Step C — Add and configure the Multi-Aim Constraints (15 min)

Now the procedural rotation logic. This is the most fiddly step in the lab — read each sub-step carefully before touching the Inspector.

Two important facts about the Multi-Aim Constraint that will save you frustration:

1. **Min Limit and Max Limit are single scalars, not per-axis values.** You cannot set different limits for pitch and yaw on one constraint. To get independent limits — e.g. ±60° yaw but only −50°/+60° pitch — you stack **two constraints** on the same bone: one that only modifies the Y axis (yaw), and a second that only modifies the X axis (pitch). The Z axis (roll) stays unconstrained on both; human heads don't usefully roll sideways.

2. **World Up Type: Scene Up causes the head to tilt toward a shoulder** when the head bone's local axes don't align with world Y — which is the case for most imported rigs. The fix is **World Up Type: Object Up**, with the character root as the World Up Object. This grounds the up-reference to the character's own skeleton instead of the world.

---

**C.1 — Find the head bone.**

Expand the `Character`'s skeleton in the Hierarchy. Drill down through the bone chain — typical paths are `Hips > Spine > Spine1 > Spine2 > Neck > Head`. The exact names depend on how your Blender lecturer named the rig. If the path isn't obvious, click bones one by one in Scene view and watch which one highlights at the character's head level.

Make a note of the head bone's exact GameObject name. You'll need it twice.

---

**C.2 — Create two constraint GameObjects.**

In the Hierarchy, right-click `HeadTrackingRig` → `Create Empty`. Rename it `HeadYaw`.
Right-click `HeadTrackingRig` again → `Create Empty`. Rename it `HeadPitch`.

`HeadYaw` should appear **above** `HeadPitch` in the Hierarchy (drag to reorder if needed). Unity executes Rig constraints top-to-bottom, so yaw applies first and pitch layers on top.

With `HeadYaw` selected: `Add Component` → `Multi-Aim Constraint`.
With `HeadPitch` selected: `Add Component` → `Multi-Aim Constraint`.

---

**C.3 — Configure HeadYaw (left/right rotation, ±60°).**

Select `HeadYaw`. In the Multi-Aim Constraint Inspector, set every field as follows:

| Field | Value |
| :-- | :-- |
| **Weight** | `1` |
| **Constrained Object** | drag the head bone from the Hierarchy |
| **Source Objects** | click `+` → drag `LookTarget` → set the weight column to `1` |
| **Aim Axis** | `Z` *(verify in C.5 — may differ for your rig)* |
| **Up Axis** | `Y` |
| **World Up Type** | **Object Up** |
| **World Up Object** | drag the `Character` root GameObject (the one with the Animator) |
| **Maintain Offset** | **ticked** |
| **Constrained Axes** | X ☐ **Y ☑** Z ☐ |
| **Min Limit** | `-60` |
| **Max Limit** | `60` |

Unchecking X and Z means this constraint **only rotates the head around its Y axis** — pure left/right yaw. It will not affect pitch or roll at all.

`Object Up` anchors the up-reference to the character's own skeleton. When the character tilts slightly on uneven ground or when the head bone's local Y isn't perfectly vertical, the head stays upright relative to the body instead of rolling toward a shoulder.

---

**C.4 — Configure HeadPitch (up/down rotation, −50° to +60°).**

Select `HeadPitch`. Configure identically to HeadYaw **except** for the constrained axis and limits:

| Field | Value |
| :-- | :-- |
| **Weight** | `1` |
| **Constrained Object** | the same head bone |
| **Source Objects** | `+` → `LookTarget` → weight `1` |
| **Aim Axis** | `Z` *(same as HeadYaw — match whatever you set in C.5)* |
| **Up Axis** | `Y` |
| **World Up Type** | **Object Up** |
| **World Up Object** | the `Character` root |
| **Maintain Offset** | **ticked** |
| **Constrained Axes** | **X ☑** Y ☐ Z ☐ |
| **Min Limit** | `-50` |
| **Max Limit** | `60` |

Unchecking Y and Z means this constraint **only rotates the head around its X axis** — pure pitch. The asymmetric limits (−50° down, +60° up) reflect real human cervical range: looking up toward the ceiling is slightly easier than looking down at your feet.

> **Why Z stays unchecked on both:** roll (tilting the head toward a shoulder) isn't a useful degree of freedom for world-awareness head tracking, and leaving it free tends to produce unnatural corkscrew motion when the target moves to extreme positions. Both constraints leave Z unchecked, so the head never rolls.

---

**C.5 — Find the correct Aim Axis for your rig.**

The Aim Axis tells the constraint which local axis of the head bone points out the face. This depends on how the rig was built in Blender and is **not always Z** — verify it now rather than debugging later.

**Quick test:**

1. Press **Play**.
2. In the Inspector, temporarily set `HeadYaw`'s **Constrained Axes** to X ☑ Y ☑ Z ☑ (all three) so you can see the full rotation.
3. Position `LookTarget` **directly in front of the character** at face height, about 2 units away.
4. The head should point squarely at the target with no sideways tilt.

If the head tilts, rolls, or points the wrong direction, work through the Aim Axis options in this order until the head faces correctly:

| Try | Typical cause |
| :-- | :-- |
| `Z` | Standard Blender humanoid (Rigify, most manually-rigged chars) |
| `-Z` | Rig exported with Z flipped — face points backward |
| `Y` | Bone's length axis is forward — common in some auto-riggers |
| `-Y` | Same but flipped |
| `X` or `-X` | Unusual orientations; try last |

Once the head faces the target cleanly, **set the same Aim Axis on both `HeadYaw` and `HeadPitch`**, then restore `HeadYaw`'s Constrained Axes back to Y-only (X ☐ Y ☑ Z ☐).

---

**C.6 — Test the full setup.**

Stop Play, verify both constraints are configured, then Press Play again.

Move the `LookTarget` sphere:

- **Left and right** — head yaws to follow, stops at ±60° when the target moves behind the character.
- **Up and down** — head pitches to follow, stops at −50° (floor-level) and +60° (above head).
- **Directly behind the character** — the head turns as far as 60° and then stops. It does not snap around 270°.

The head should stay upright at all times — no tilting toward a shoulder. If you still see tilt, confirm `World Up Type` is **Object Up** on both constraints and that `World Up Object` is the `Character` root (not `HeadTrackingRig`, not the head bone itself).

**Checkpoint:** Moving `LookTarget` produces clean yaw and pitch head tracking. The head stops at the configured limits. No shoulder-tilt at any target position. The character's body continues its idle animation untouched.

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

### Step E — Proximity-based look activation with a trigger zone (12 min)

Right now the head constraints are always active at weight 1 — the character looks at the target from anywhere in the scene. That's not how awareness works in games. In this step you'll place a trigger zone near the Spider; when the character walks into it, head tracking switches on, and when they leave, it switches off. This is the **awareness radius** pattern used in virtually every third-person character: NPCs don't react to things they haven't walked close enough to notice.

---

**E.1 — Create the trigger zone GameObject.**

In the Hierarchy, right-click on an empty area → `Create Empty`. Name it `LookZone`. Position it at the Spider's location — use the Spider's Transform position in the Inspector as a reference, then set LookZone's Position to the same world coordinates.

With `LookZone` selected, `Add Component` → `Sphere Collider`. Configure it:

- **Is Trigger**: **ticked**
- **Center**: `(0, 0, 0)`
- **Radius**: `5` (world units — the character must walk within 5 metres of the Spider for tracking to activate; you'll tune this in E.6)

Add a second component: `Add Component` → `Rigidbody`. Configure it:

- **Use Gravity**: unticked
- **Is Kinematic**: **ticked**
- Expand **Constraints** → freeze all six (Position X, Y, Z and Rotation X, Y, Z)

> **Why a Rigidbody?** Unity's `OnTriggerEnter` / `OnTriggerExit` messages only fire when at least one of the two overlapping objects has a Rigidbody. Without it the physics engine ignores the overlap entirely and your script never gets called. A kinematic, gravity-free, fully-frozen Rigidbody satisfies that requirement without any visible effect on the scene.

---

**E.2 — Tag the Character.**

Select the `Character` root GameObject. At the very top of the Inspector, click the **Tag** dropdown → `Player`.

If `Player` is not in the list: click **Add Tag...** → click `+` → type `Player` → press Enter → navigate back to the Character and set the tag.

The `LookAtZone` script will use this tag to distinguish the character from any other colliders that might wander into the zone (other GameObjects, the Spider itself, etc.).

---

**E.3 — Add a Collider to the Character.**

For the trigger to detect the Character, the Character needs a Collider. Select the `Character` root. `Add Component` → `Capsule Collider`. Configure it to roughly match the character's body:

| Field | Value |
| :-- | :-- |
| Center | `(0, 0.9, 0)` |
| Radius | `0.3` |
| Height | `1.8` |
| Direction | `Y-Axis` |
| Is Trigger | unticked |

Do **not** add a Rigidbody here — the `PlayerController` script already handles movement via `transform.position`. Adding a dynamic Rigidbody would conflict with that. The Capsule Collider alone is enough for trigger detection because `LookZone` already supplies the required Rigidbody.

---

**E.4 — Create LookAtZone.cs.**

In `Assets/Scripts/`, create a new MonoBehaviour script called `LookAtZone.cs`. Replace its contents with:

```csharp
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Enables the head-tracking constraints when a tagged object enters this
/// trigger zone and disables them on exit.
///
/// Place this near a point of interest (e.g. the Spider). Head tracking is
/// off by default and only activates when the character is close enough.
///
/// REQUIRES: a SphereCollider (Is Trigger = true) and a Kinematic Rigidbody
///           on the same GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LookAtZone : MonoBehaviour
{
    [SerializeField, Tooltip("Multi-Aim Constraint on the HeadYaw GameObject.")]
    private MultiAimConstraint headYawConstraint;

    [SerializeField, Tooltip("Multi-Aim Constraint on the HeadPitch GameObject.")]
    private MultiAimConstraint headPitchConstraint;

    [SerializeField, Tooltip("Tag on the Character root. Must match exactly — Unity tags are case-sensitive.")]
    private string characterTag = "Player";

    [SerializeField, Range(0f, 1f), Tooltip("Weight applied to both constraints when the character is inside the zone.")]
    private float activeWeight = 1f;

    [SerializeField, Range(0f, 1f), Tooltip("Weight applied to both constraints when the character is outside the zone.")]
    private float inactiveWeight = 0f;

    private void Start()
    {
        // Head tracking off at startup — the character hasn't entered the zone yet.
        SetWeight(inactiveWeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(characterTag)) return;
        SetWeight(activeWeight);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(characterTag)) return;
        SetWeight(inactiveWeight);
    }

    private void SetWeight(float weight)
    {
        headYawConstraint.weight = weight;
        headPitchConstraint.weight = weight;
    }
}
```

Save and wait for compilation. Drag `LookAtZone.cs` onto the `LookZone` GameObject.

---

**E.5 — Wire the Inspector references.**

Select `LookZone`. In the Inspector, find the **Look At Zone (Script)** component:

- **Head Yaw Constraint**: drag the `HeadYaw` GameObject from `Character > HeadTrackingRig > HeadYaw` into the slot. Unity resolves the `MultiAimConstraint` component automatically from the GameObject.
- **Head Pitch Constraint**: drag `HeadPitch` from `Character > HeadTrackingRig > HeadPitch` the same way.
- **Character Tag**: leave as `Player` (or change to match whatever tag you set in E.2).
- **Active Weight**: `1` — full tracking when inside the zone.
- **Inactive Weight**: `0` — tracking fully off outside the zone.

---

**E.6 — Test and tune.**

Press Play. Walk the character **away** from the Spider. Confirm the head is no longer tracking — the constraints are at weight 0, so the head follows the Animator's authored pose.

Now walk the character toward the Spider. As you cross the zone boundary:

- The green SphereCollider gizmo in Scene view shows the exact radius (visible while `LookZone` is selected).
- `OnTriggerEnter` fires → both constraints jump to weight 1 → the head turns toward `LookTarget`.
- If `LookTarget` is positioned near the Spider, the head looks directly at it.

Walk back out. `OnTriggerExit` fires → constraints return to 0 → head tracking stops.

To adjust the awareness radius, stop Play and change the **Sphere Collider Radius** on `LookZone` in the Inspector. A radius of `3` creates a tight "only when very close" trigger; `8` creates a wide approach corridor.

**Checkpoint:** Head tracking is off when the character is outside the zone, activates at the zone boundary, and deactivates when the character leaves. The zone radius is visible as a green sphere in Scene view. The character's body animation is unaffected throughout.

---

### Step F — Walk to the spider, watch the head turn (10 min)

This is the payoff. Press Play. Walk the character (using `WASD` and `Shift`) from the corridor entrance, down the corridor, through the door, into the chamber. The whole way, your mouse continues to move the `LookTarget`.

Move the target to where the spider sits at the far end of the chamber. As the character walks toward it, the head turns to follow the spider. The character is *aware* of what's in the chamber — and the player feels that awareness through the head tracking, even though the spider itself does nothing.

Walk in a circle around the spider (still moving the target to it as you go). The head tracks continuously, hitting the rotation limits at extreme angles and stopping there. The character's body keeps walking via the Lab 2 Animator and the Lab 4 `PlayerController` script — completely untouched. The head layer is purely additive.

**Checkpoint:** The character can walk and look at the same time. The head reaches its rotation limits at extreme angles and stops there rather than snapping around. The whole scene feels alive — far more so than Lab 5's static silhouette.

*(Take a moment here. This is what 6 labs of progressively layered animation produce. Authored body, procedural head, decorated environment, all working together.)*

---

### Step G — Tune the feel (10 min)

The default constraint values produce a believable result, but tuning unlocks character-specific personality. Try these tweaks during Play mode:

**Constraint Weight (HeadYaw and HeadPitch weight sliders).**
Lower both from 1.0 to 0.7. The head still tracks but the original walk-cycle head-bob comes through more strongly. The motion feels less robotic, more like the head is drawn to the target rather than locked onto it. Try 0.5 — even more organic. Try 0.3 — the tracking becomes a subtle lean rather than a turn. Try setting HeadYaw to 0.8 and HeadPitch to 0.5 — the head turns more confidently than it tilts, which is how attentive humans actually look at things at a distance.

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
| Set `LookZone` Sphere Collider Radius to `1` then `15` | At radius 1 the character must nearly touch the Spider; at 15 the head starts tracking from the corridor entrance — the radius defines the "noticing distance" |
| Set **Active Weight** on `LookAtZone` to `0.4` instead of `1` | Tracking activates subtly on enter — the head turns partway, as if the character notices something in peripheral vision rather than snapping to full attention |
| Add a second `LookZone` near the corridor entrance pointing at a different `LookTarget` | Two independent awareness zones — the character's head switches attention between points as they walk through the scene |
| Replace the instant `SetWeight` call with a `Coroutine` that lerps weight over 0.5s | The on/off transition becomes a smooth fade rather than a snap — much more natural for slow realisation vs sudden alert |

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
| Head turns to wrong axis (looks sideways or backwards) | Aim Axis doesn't match the head bone's forward direction | Follow the C.5 diagnostic: set all three Constrained Axes temporarily, move `LookTarget` in front of the character, try `Z → -Z → Y → -Y → X → -X` until the face points at the target. Set the same Aim Axis on both `HeadYaw` and `HeadPitch`. |
| Head tilts toward the shoulder when target moves up or down | `World Up Type` is `Scene Up`, which uses world Y — if the head bone's local Y isn't vertical this causes a roll | Set `World Up Type` to **Object Up** on **both** constraints and drag the `Character` root into the `World Up Object` slot. This grounds the up-reference to the skeleton, not the world. |
| Character's whole body rotates instead of just the head | Multi-Aim Constraint's Constrained Object is set to a higher-up bone (like Hips or Spine) | Drag the *Head* bone specifically into the Constrained Object slot, not a parent bone |
| Animation Rigging menu doesn't appear | Package not installed | Window → Package Manager → search Animation Rigging → Install |
| Constraint shows error: "Animator is not valid" | Rig Builder is on a GameObject that doesn't have an Animator | Move the Rig Builder to the Character root (which has the Animator). The Rig itself can be a child |
| Head tracking works in Editor but not in Build | Animation Rigging requires "Auto Setup from Target Skeleton" or an Animator with a valid Avatar | Ensure your Generic rig's Avatar Definition is set to "Create From This Model" in the FBX import |
| `OnTriggerEnter` never fires when character enters the zone | Missing Rigidbody on `LookZone`, OR `Character` has no Collider, OR tag mismatch | Confirm: (1) `LookZone` has a Rigidbody set to Is Kinematic = true; (2) `Character` root has a Capsule Collider with Is Trigger unticked; (3) Character's Tag matches `characterTag` exactly — tags are case-sensitive |
| Constraints enable on entry but never disable — head stays locked on | `OnTriggerExit` didn't fire — usually because the character teleported out of the zone (large `transform.position +=` delta in one frame) or the zone was destroyed | For large movement steps, add an `Update` distance fallback: check `Vector3.Distance(character.position, transform.position) > radius` and call `SetWeight(inactiveWeight)` if true |
| Head tracking activates before the character reaches the zone visually | Capsule Collider on Character is too large — the outer radius reaches the SphereCollider before the character's visible body does | Reduce the Capsule Collider's Radius on the Character (try `0.25`) or increase the Sphere Collider's Centre Y to match the character's torso height |
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
- `Assets/Scripts/TargetMover.cs`
- `Assets/Scripts/LookAtZone.cs`
- `Assets/Scenes/Lab06_Chamber.unity` (modified: Rig Builder, HeadTrackingRig with HeadYaw + HeadPitch constraints, LookTarget sphere, LookZone trigger)

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
