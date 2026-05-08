---
title: "Blend Trees — Smooth Walk, Run & Strafe Locomotion"
subtitle: "Unity Animation Mini-Series — Supplementary Lab"
topic_code: ts_blend_trees_locomotion
description: "A 60-minute supplementary lab introducing Unity's 2D Blend Tree system. Students import a rigged humanoid from the Asset Store, download seven locomotion animations from Mixamo (idle, slow/fast forward, slow/fast strafe left/right), assemble them into a 2D Freeform Cartesian blend tree, and drive it from a commented C# script that reads the new Input System and smoothly interpolates blend parameters."
created: 2026-05-08
last_updated: 2026-05-08
version: 1.0
status: published
authors: ["Games Development Teaching Team"]
tags: [unity, unity-6.4-lts, animation, blend-tree, mixamo, humanoid, locomotion, input-system, year1, supplementary-lab]
difficulty_tier: Intermediate
unity_version: "6.4 LTS"
project_template: "3D (URP) Core"
duration_minutes: 60
previous_topic: t03_input_character_control
prerequisites:
  - Lab 4 completed — comfortable with Input System and character movement scripts
  - Comfortable creating GameObjects, attaching scripts, and assigning references in the Inspector
  - A free Mixamo account at mixamo.com (Adobe ID — free to register)
  - Uses the [repo](https://github.com/nmcguinness/Unity-Intro).
---

# Blend Trees — Smooth Walk, Run & Strafe Locomotion

> **Prerequisites:**
> - You have completed Lab 4 and understand how to read Input System actions in a MonoBehaviour.
> - You have a free Adobe / Mixamo account. If not, register now at [mixamo.com](https://www.mixamo.com) — it is free and takes under two minutes.
> - This lab sits **outside the assessed 3-hour series**. It may be completed before, between, or after the numbered labs — it has no narrative dependency on any of them.

---

## **What you'll learn**

| Skill Type | You will be able to… |
| :-- | :-- |
| **Conceptual Understanding** | Explain what a blend tree is, why it produces smoother locomotion than state machine transitions, and what "2D Freeform Cartesian" means as a blend mode. |
| **Editor & Tool Fluency** | Create an Animator Controller with float parameters, insert a 2D blend tree, position seven animation clips in 2D velocity space, and preview the blending live in the Inspector. |
| **Asset Pipeline** | Download Mixamo FBX animations with "In Place" enabled, import them into Unity as Humanoid clips, and retarget them onto any rigged humanoid character. |
| **Code Implementation** | Read and modify a `LocomotionController` script that reads Input System actions, scales raw input to velocity, applies `Vector2.SmoothDamp` for responsive-but-smooth motion, and pushes parameters to the Animator every frame. |
| **Problem-Solving** | Diagnose the four classic blend tree bugs: character T-poses (wrong avatar type), clips not blending (wrong parameter names), jittery transitions (SmoothTime too low), and character sliding (root motion/in-place mismatch). |

---

## **Why this matters**

Every third-person game — from indie platformers to AAA open-world titles — uses a locomotion blend tree as the entry point of its animation graph. The player holds an analogue stick; the blend tree smoothly interpolates between idle, walk, and run without a single hard transition. Add strafe clips and the character feels genuinely responsive in eight directions.

Before blend trees, teams wired up a state machine of discrete states (Idle → Walk → Run) connected by transition conditions. The result was a visible "click" at the walk-to-run boundary that no amount of transition time could fully hide. Blend trees replaced that click with a continuous weighted average: at 60 % walk speed the character plays 40 % of the idle animation and 60 % of the walk animation simultaneously, blended per-bone.

Understanding blend trees means understanding how **every mainstream game's locomotion system** works at the animation-graph level, regardless of engine. The Unreal equivalent is a BlendSpace; Godot uses AnimationTree with BlendSpace2D. The data model is the same.

---

## **How this builds on previous content**

**From Lab 4 you know:**
- How to read a `Move` action as a `Vector2` using the new Input System.
- How to move a character using `transform.position` and `transform.Rotate`.

**This lab replaces the direct-transform movement** with a system where:
- Script still reads input and scales it to velocity.
- Velocity values are written to Animator parameters instead of directly moving the character.
- The Animator blend tree picks and interpolates clips based on those values.
- Root motion *from the animation clip* moves the character — not `transform.position +=` in script. (You will see why this produces much more natural footstep timing.)

**After this lab**, the Year 2 Animation module will extend this pattern to include blend tree layers (upper-body aim while lower body walks), AvatarMasks, and Inverse Kinematics — all of which attach to the same Animator Controller you build today.

---

# **Core Ideas / Concepts**

> Read all four before starting the practical steps.

---

### **Core Idea 1 — A Blend Tree is a weighted average, not a switch**

A standard state machine plays one animation at a time and transitions between states when conditions are met. A blend tree continuously plays *all* of its clips simultaneously, weighted by their distance from the current parameter value. The final bone pose is the weighted sum of every clip's pose at that frame.

**Snippet explanation:**
At parameter value `(0, 0)` the Idle clip has weight 1.0 and all others 0.0. Move the parameter toward `(0, 1.5)` and the Idle weight falls while Walk Forward rises. At `(0, 0.75)` — midway — Idle and Walk Forward each contribute 0.5, so the character is visually half-idle, half-walking. This math happens automatically; you only supply the parameter values and the clip positions.

The consequence is that you can add a "jog" clip midway between walk and run without touching any code — just drag a new clip into the blend tree, position it at the correct velocity, and the interpolation adjusts automatically.

---

### **Core Idea 2 — 2D Freeform Cartesian places clips in a 2D velocity space**

Unity offers several blend tree algorithms. **2D Freeform Cartesian** is the standard choice for locomotion. Each clip sits at a 2D coordinate (X, Z) representing the velocity the clip was recorded at. The algorithm's job is: given a current 2D parameter value, compute a weight for every clip such that the weighted average of their positions equals the current parameter value exactly.

**Snippet explanation:**
This lab uses two Animator parameters: `VelocityX` (lateral speed, negative = left) and `VelocityZ` (forward speed). Clip positions:

| Clip | VelocityX | VelocityZ |
| :-- | :-- | :-- |
| Idle | 0 | 0 |
| Walk Forward | 0 | 1.5 |
| Run Forward | 0 | 4.0 |
| Walk Strafe Left | −1.2 | 0 |
| Run Strafe Left | −3.5 | 0 |
| Walk Strafe Right | 1.2 | 0 |
| Run Strafe Right | 3.5 | 0 |

When the script sets `VelocityZ = 2.75` (midway between walk and run) the blend tree assigns roughly equal weight to Walk Forward and Run Forward, producing a smooth visual blend between the two footstep rhythms.

---

### **Core Idea 3 — Mixamo animations must be downloaded with "In Place" enabled**

Mixamo's default animations include **root motion** — the root bone of the skeleton actually travels forward through space over the duration of the clip. If you import a root-motion clip into a blend tree, every clip pulls the character in a different world-space direction simultaneously, and the result is chaotic sliding.

**"In Place"** strips the root translation from the animation so the root bone stays at the origin throughout the clip. Unity's Animator then controls world-space movement via root motion settings or, in this lab, by leaving motion to the script entirely.

**Snippet explanation:**
In Mixamo, before you click Download, tick the **In Place** checkbox in the configuration panel on the right. This adds the suffix `_InPlace` to the downloaded filename and strips forward translation. For strafe clips (which move sideways in world space) the same rule applies — tick In Place, or the character drifts sideways as the animation plays.

---

### **Core Idea 4 — Animator parameters are set from script every frame**

The Animator does not automatically observe your character's world-space velocity. You must push the correct values each frame via `animator.SetFloat("ParameterName", value)`. The blend tree reads those values and computes weights — but it has no awareness of physics, movement scripts, or anything outside the Animator itself.

**Snippet explanation:**
This separation is intentional. The Animator is a pure animation system; it does not know or care how the character moves. The script knows velocity but does not know how to blend animations. The contract between them is the parameter values you push. This means you can completely replace the character controller script without touching the Animator, or swap the Animator Controller without touching the script, as long as both agree on the parameter names `VelocityX` and `VelocityZ`.

Using `Animator.StringToHash("VelocityX")` converts a parameter name to an integer ID once at startup. Passing the integer ID to `SetFloat` each frame avoids a dictionary lookup and is approximately 10× faster. For a single character it makes no measurable difference; it is the industry convention worth learning now.

---

# **Progressive Lab Steps (A → B → C → D → E)**

> Total budget: **60 minutes**.
> You will build one Animator Controller with one blend tree, drive it from one script, and end with a character that walks, runs, and strafes smoothly in response to WASD + Shift.

---

### Step A — Import a rigged humanoid character (10 min)

Open the Unity Asset Store from within the editor: **Window > Asset Store** (or press `Ctrl + 9`). The Asset Store tab opens inside Unity and redirects to your browser.

Search for **"Starter Assets — ThirdPerson"** published by Unity Technologies. This is a free, officially maintained pack that includes a fully rigged humanoid character (`PlayerArmature`) along with a pre-configured Animator Controller and Input Actions. You will discard the controller and scripts it ships with — you need only the **rig** and the **Avatar definition**.

> **Alternative:** Any humanoid `.fbx` from the Asset Store works. If you prefer a different character, confirm its Inspector shows **Rig > Animation Type: Humanoid** and that Unity created an Avatar asset (a small grey-and-blue diamond icon in the Project window next to the `.fbx`). If Animation Type shows "Generic" or "Legacy", change it to Humanoid, click Apply, and wait for Unity to reprocess the mesh.

After importing:

1. In the Project window, open `Assets/StarterAssets/ThirdPersonController/Character/` (exact path varies by pack). Locate the character prefab — typically named `PlayerArmature`.
2. Drag the prefab into the Scene hierarchy.
3. In the Inspector, confirm the `Animator` component is present on the root object and that its **Avatar** field is populated (not "None"). The Avatar is the bone-mapping definition that allows Unity to retarget Mixamo animations onto this skeleton.
4. Rename the Scene GameObject to `Character`.
5. Add a **Capsule Collider** to `Character`: centre `(0, 0.9, 0)`, radius `0.3`, height `1.8`. This gives the Physics system a simple collision shape without computing against the skinned mesh.

**Checkpoint:** A human-shaped character stands in the Scene. The Animator component shows a valid Avatar. In Play mode the character stands in T-pose (no animation assigned yet — that is correct at this stage).

---

### Step B — Download and import seven Mixamo animations (15 min)

Open [mixamo.com](https://www.mixamo.com) in your browser and log in with your Adobe ID.

**Uploading your character for retargeting (optional but recommended):**
Mixamo can retarget animations onto your specific skeleton, avoiding T-pose drift on unusual rigs. To do this: Export your character from Unity as `.fbx` (`Assets > Export Package` is not correct — instead, right-click the `.fbx` file in the Project window and choose `Show in Explorer`, then use that file). In Mixamo, click **Upload Character**, choose the `.fbx`, and let Mixamo auto-rig it. This step is optional — Mixamo's standard animations work on most Unity humanoid rigs without retargeting.

**Downloading the seven clips:**

For each animation below, search by name in Mixamo's search bar, configure as described, and click **Download** with settings: `Format: FBX for Unity (.fbx)`, `Skin: Without Skin` (no mesh — just the animation), `Frames per Second: 30`, **`In Place: ✓ checked`**.

| # | Search term | Mixamo clip name | Notes |
| :-- | :-- | :-- | :-- |
| 1 | idle | **Idle** | No movement — character breathes and shifts weight |
| 2 | walking | **Walking** | Standard pace — ~1.5 m/s in-place |
| 3 | running | **Running** | Full run — ~4.0 m/s in-place |
| 4 | strafe walk left | **Left Strafe Walk** | Slow lateral — ~1.2 m/s |
| 5 | strafe left | **Left Strafe** | Fast lateral — ~3.5 m/s |
| 6 | strafe walk right | **Right Strafe Walk** | Slow lateral — ~1.2 m/s |
| 7 | strafe right | **Right Strafe** | Fast lateral — ~3.5 m/s |

> **Tip — previewing speed:** Mixamo's preview panel lets you scrub an animation. For walk vs run forward, look at the number of foot contacts per second and the visual pace. Walk should look like a casual stroll; run should look like a full sprint. If Mixamo's defaults look wrong, adjust the **Overdrive** slider (right panel) before downloading — this scales playback speed without changing the clip's recorded data.

**Importing into Unity:**

1. In your Project window, create a folder: `Assets/Animations/Locomotion/`.
2. Drag all seven downloaded `.fbx` files into this folder.
3. For each `.fbx`, click it, go to Inspector > **Rig** tab:
   - Set **Animation Type** to `Humanoid`.
   - Set **Avatar Definition** to `Copy From Other Avatar`.
   - Set **Source** to the Avatar from your character (e.g. `PlayerArmatureAvatar`).
   - Click **Apply**.
4. For each `.fbx`, go to Inspector > **Animation** tab:
   - Confirm the clip is listed (should show one clip per file).
   - Tick **Loop Time** on all seven clips.
   - For the Idle clip also tick **Loop Pose** (this irons out a small discontinuity at the loop point that Idle clips commonly have).
   - Click **Apply**.

**Checkpoint:** Seven `.fbx` files sit in `Assets/Animations/Locomotion/`. Each shows Animation Type: Humanoid in its Rig tab. Selecting any `.fbx` and clicking the small Play button in the Inspector's Animation tab preview shows the character animating in place — no forward drift.

---

### Step C — Build the Animator Controller and 2D Blend Tree (15 min)

#### C.1 — Create the Animator Controller

In the Project window: right-click `Assets/Animations/` → **Create > Animator Controller**. Name it `LocomotionController`.

Select your `Character` GameObject in the hierarchy. In the Inspector, find the `Animator` component. Drag `LocomotionController` from the Project window onto the **Controller** field of the Animator component.

Double-click `LocomotionController` in the Project window to open the **Animator** window.

#### C.2 — Add float parameters

In the Animator window, click the **Parameters** tab (top-left).

Add two `Float` parameters:

| Name | Type | Default |
| :-- | :-- | :-- |
| `VelocityX` | Float | 0 |
| `VelocityZ` | Float | 0 |

> **Exact names matter.** The script in Step D uses `Animator.StringToHash("VelocityX")` and `Animator.StringToHash("VelocityZ")`. A typo here — including wrong capitalisation — means the script's hash won't match and the blend tree won't respond to input.

#### C.3 — Create the blend tree state

In the Animator window's graph area: right-click → **Create State > From New Blend Tree**. A new state box labelled `Blend Tree` appears (orange, because it is the default state).

Double-click the `Blend Tree` state box to enter the blend tree editor (the breadcrumb at the top of the Animator window changes to `Base Layer > Blend Tree`).

#### C.4 — Configure the blend tree

In the Inspector (with the Blend Tree node selected in the graph):

1. Change **Blend Type** to `2D Freeform Cartesian`.
2. Set **Parameter** (first dropdown) to `VelocityX`.
3. Set **Parameter** (second dropdown) to `VelocityZ`.

#### C.5 — Add the seven clips

Click the **+** button in the Motion list and choose `Add Motion Field`. Repeat until you have seven motion rows. For each row, click the small circle to the right of the motion field and select the clip from the picker, then set the **Pos X** and **Pos Y** values to match the table below:

| Motion field | Clip | Pos X (VelocityX) | Pos Y (VelocityZ) |
| :-- | :-- | :-- | :-- |
| 1 | Idle | 0 | 0 |
| 2 | Walking | 0 | 1.5 |
| 3 | Running | 0 | 4.0 |
| 4 | Left Strafe Walk | −1.2 | 0 |
| 5 | Left Strafe | −3.5 | 0 |
| 6 | Right Strafe Walk | 1.2 | 0 |
| 7 | Right Strafe | 3.5 | 0 |

> **Reading the diagram:** Unity draws a 2D scatter plot of the clip positions in the Inspector. Idle sits at the centre. Forward clips extend upward along the Y axis. Left strafes extend left along the X axis. Right strafes extend right. The coloured triangle that appears when you drag the red dot (current parameter position) shows which three clips are being interpolated and at what weights — this is the visual representation of the weighted average described in Core Idea 1.

#### C.6 — Verify in Play mode

Press **Play**. Open the **Animator** window and the **Inspector** with your `Character` selected simultaneously.

In the Animator window, manually drag the `VelocityZ` parameter slider from 0 to 4. Observe: the character smoothly transitions from Idle → Walking → Running as the value rises. Drag `VelocityX` to −3.5: the character switches to the left strafe. This confirms the blend tree is wired correctly before writing any code.

**Checkpoint:** Seven clips sit in the blend tree at the correct 2D positions. Manually dragging both parameter sliders in Play mode produces smooth, visually correct blending.

---

### Step D — Write the LocomotionController script (15 min)

In the Project window: right-click `Assets/Scripts/` → **Create > C# Script**. Name it `LocomotionController`.

Open the script and replace its entire contents with the following. **Read every comment** — each one explains a decision you will need to modify for your own projects.

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads player input every frame and pushes VelocityX / VelocityZ float
/// parameters into the Animator, driving the 2D Freeform Cartesian blend tree.
///
/// Movement speed:
///   Walking forward/back  — up to walkSpeed  m/s
///   Running  forward/back  — up to runSpeed   m/s  (hold Sprint action)
///   Strafing walking       — up to walkStrafeSpeed m/s
///   Strafing running       — up to runStrafeSpeed  m/s
///
/// Attach to the same GameObject as the Animator component.
/// Requires a Move action (Vector2) and a Sprint action (Button)
/// in your project-wide Input Actions asset.
/// </summary>
[RequireComponent(typeof(Animator))]
public class LocomotionController : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector-exposed fields
    // ---------------------------------------------------------------

    [Header("Forward / Back Speeds")]

    [Tooltip("Maximum speed (m/s) when walking forward or backward.")]
    [SerializeField] private float walkSpeed = 1.5f;

    [Tooltip("Maximum speed (m/s) when running forward or backward (Sprint held).")]
    [SerializeField] private float runSpeed = 4.0f;

    [Header("Strafe Speeds")]

    [Tooltip("Maximum lateral speed (m/s) when strafing at walking pace.")]
    [SerializeField] private float walkStrafeSpeed = 1.2f;

    [Tooltip("Maximum lateral speed (m/s) when strafing at running pace (Sprint held).")]
    [SerializeField] private float runStrafeSpeed = 3.5f;

    [Header("Blend Smoothing")]

    [Tooltip("How quickly (seconds) blend tree parameters follow input changes. "
           + "0.05 = snappy but slightly jerky. 0.15 = smooth but responsive. "
           + "0.30+ = sluggish, like running in sand.")]
    [SerializeField] private float smoothTime = 0.10f;

    // ---------------------------------------------------------------
    // Private fields — not shown in Inspector
    // ---------------------------------------------------------------

    // The Animator component on this GameObject (cached in Awake)
    private Animator animator;

    // Input System action references (looked up by name from the project asset)
    private InputAction moveAction;
    private InputAction sprintAction;

    // The velocity value currently being fed to the blend tree.
    // X = lateral (negative left, positive right)
    // Y = forward / back (positive forward, negative back)
    // Stored as a field so SmoothDamp can accumulate across frames.
    private Vector2 currentVelocity = Vector2.zero;

    // SmoothDamp writes its own internal derivative into this field each frame.
    // Must be stored as a field — do NOT declare this as a local variable.
    private Vector2 smoothDampVelocity = Vector2.zero;

    // Integer hashes of the Animator parameter names.
    // Computed once at startup — faster than a string lookup every frame.
    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        // Cache the Animator component.
        // [RequireComponent] above guarantees it will exist, so no null check needed.
        animator = GetComponent<Animator>();

        // Find the named actions from the project-wide Input Actions asset.
        // The action names must match exactly what is defined in your
        // InputActions asset (Window > Input Actions, or Project Settings > Input System).
        // Lab 4 uses "Move" and "Sprint" — adjust if your asset uses different names.
        moveAction   = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");

        // Warn early if either action is missing, so students see the problem
        // immediately rather than puzzling over why the character does not move.
        if (moveAction == null)
            Debug.LogWarning("[LocomotionController] 'Move' action not found in Input Actions asset.");
        if (sprintAction == null)
            Debug.LogWarning("[LocomotionController] 'Sprint' action not found in Input Actions asset.");
    }

    private void OnEnable()
    {
        // Enable both actions so they receive device events while this
        // script is active. Actions start disabled and must be explicitly enabled.
        moveAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        // Mirror of OnEnable — disable actions when the script is inactive.
        // Without this, the actions continue receiving device events
        // even when the character is deactivated (e.g. in a cutscene).
        moveAction?.Disable();
        sprintAction?.Disable();
    }

    private void Update()
    {
        // --- 1. Read raw input -----------------------------------------

        // ReadValue<Vector2>() returns the current state of the Move action:
        //   X axis: −1 (left/A) to +1 (right/D), or analogue stick X
        //   Y axis: −1 (back/S) to +1 (forward/W), or analogue stick Y
        // The null-conditional (?.) + null-coalescing (??) guard against the
        // action not being found in Awake — character simply stops moving.
        Vector2 rawInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

        // IsPressed() returns true while the button is held down each frame.
        bool sprinting = sprintAction?.IsPressed() ?? false;

        // --- 2. Scale raw input to target velocity ---------------------

        // Multiply the −1..+1 input range by the appropriate max speed to
        // get the velocity in m/s that the animation at this position was
        // recorded at. The blend tree positions in Step C used these exact
        // speed values (walkSpeed, runSpeed, etc.), so the parameter values
        // produced here will land exactly on the recorded clips when at full
        // input deflection, and blend between clips at partial deflection.

        float targetZ = rawInput.y * (sprinting ? runSpeed        : walkSpeed);
        float targetX = rawInput.x * (sprinting ? runStrafeSpeed  : walkStrafeSpeed);

        Vector2 targetVelocity = new Vector2(targetX, targetZ);

        // --- 3. Smooth the velocity change ------------------------------

        // SmoothDamp interpolates from currentVelocity toward targetVelocity
        // over approximately smoothTime seconds. This produces the gradual
        // acceleration / deceleration that makes movement feel weighted.
        //
        // Without this: releasing W causes an instant snap from Walk to Idle —
        // the blend tree value jumps discontinuously and the footstep cycle
        // resets abruptly. With smoothing: the value glides to zero, the walk
        // cycle finishes its current step, and Idle fades in gradually.
        currentVelocity = Vector2.SmoothDamp(
            currentVelocity,      // current value (field — persists between frames)
            targetVelocity,       // where we want to end up
            ref smoothDampVelocity, // internal derivative — must be a field, not local
            smoothTime            // approximate time to reach target
        );

        // --- 4. Push values into the Animator --------------------------

        // SetFloat with an integer hash is slightly faster than the string
        // overload because it skips an internal dictionary lookup. The hash
        // matches the parameter named "VelocityX" / "VelocityZ" exactly.
        // If the parameter names in the Animator Controller were changed,
        // update the StringToHash calls in the field declarations above.
        animator.SetFloat(VelocityXHash, currentVelocity.x);
        animator.SetFloat(VelocityZHash, currentVelocity.y);
    }
}
```

**Attach the script:**

1. Select the `Character` GameObject in the hierarchy.
2. Drag `LocomotionController.cs` from the Project window onto the Inspector, or click **Add Component** and search for `Locomotion Controller`.
3. The Inspector now shows the script's fields. Leave default values for now.

**Enter Play mode** and press W. The character should animate with a walk cycle. Hold Shift — the character transitions into a run. Press A or D — the character strafes. Release keys — the character decelerates back to Idle smoothly.

**Checkpoint:** Character plays the correct animation clip for each input combination. Transitions are smooth — no hard snapping. In the Animator window's Parameters panel, the `VelocityX` and `VelocityZ` values change in real time as you move.

---

### Step E — Tinker tasks (5 min)

Work through as many of these as time allows. Each is reversible — save the scene before starting.

| # | Task | What to observe |
| :-- | :-- | :-- |
| E.1 | Raise `Smooth Time` to `0.4` and walk in a circle. | Acceleration becomes noticeably sluggish — the animation lags behind input by almost half a second. Notice how this changes the game feel from "responsive action game" to "simulation/realistic". |
| E.2 | Drop `Smooth Time` to `0.01`. | Movement is near-instant but the footstep cycle snaps and resets abruptly on direction changes. This is the pre-smoothing behaviour. |
| E.3 | In the blend tree Inspector, drag the red dot (current parameter position) to the diagonals — e.g. `VelocityX = 1.2, VelocityZ = 1.5`. | Unity blends Walk Right + Walk Forward simultaneously. The character walks diagonally. This emergent behaviour comes from the Freeform Cartesian algorithm, not from you adding a diagonal clip. |
| E.4 | Download one additional Mixamo clip: **"Walking Backwards"** (In Place, Loop). Add it to the blend tree at `(0, −1.5)`. | The character now plays a backward walk when you press S, rather than playing Walking backwards — which looks wrong. Notice how adding one clip required zero code changes. |
| E.5 | In the Animator Controller, add a second state called `Jump` connected from `Blend Tree` via a transition on a Trigger parameter `DoJump`. Leave the clip empty for now. | Observe how a blend tree state and a regular state can coexist in the same controller layer — the blend tree handles ground locomotion; discrete states handle jumps, falls, attacks. |
| E.6 | Change the blend tree type from `2D Freeform Cartesian` to `2D Simple Directional`. | Observe the visual change in clip positions — Simple Directional normalises all positions to a unit circle, which produces different blending at partial input. Most locomotion systems prefer Freeform Cartesian because it preserves the velocity scale. |

---

## **Common bugs and fixes**

| Symptom | Likely cause | Fix |
| :-- | :-- | :-- |
| Character displays T-pose at runtime | Animator Controller not assigned to the Animator component | Drag `LocomotionController` asset onto the `Controller` field in the Animator component Inspector |
| Character displays T-pose but only for imported clips | Clip imported as Generic or Legacy, not Humanoid | Select the `.fbx` → Rig tab → Animation Type: Humanoid → Apply |
| Clips play but character does not blend (snaps between states) | Wrong parameter name in blend tree (case-sensitive mismatch) | Animator window → Parameters tab: confirm `VelocityX` and `VelocityZ` are spelled and capitalised exactly as shown. Check the StringToHash strings in the script match. |
| Character slides forward while playing animation | Animation downloaded without "In Place" checked | Re-download from Mixamo with In Place ✓ and re-import. Alternatively, in the FBX Animation tab, tick `Bake Into Pose` for Root Transform Position (XZ) |
| Console: `'Move' action not found` | Input Actions asset missing a "Move" action, or using a different name | Open Window > Input Actions. Confirm an action named exactly `Move` exists. Rename it or update the `FindAction("Move")` call in the script |
| Blend tree Inspector shows no diagram / clips | Blend tree type left as 1D (default) | Inspector → Blend Type → 2D Freeform Cartesian |
| Footstep audio (from Lab 4) fires at wrong rate | Animation Event timing was authored for a different playback speed | Re-open the animation clip in the Animation window and adjust event timing to match the clip's actual footstep beats at the speed it plays in the blend tree |

---

## **Files produced**

By the end of this lab your project contains:

```
Assets/
├── Animations/
│   └── Locomotion/
│       ├── Idle.fbx
│       ├── Walking.fbx
│       ├── Running.fbx
│       ├── Left Strafe Walk.fbx
│       ├── Left Strafe.fbx
│       ├── Right Strafe Walk.fbx
│       └── Right Strafe.fbx
├── Animations/
│   └── LocomotionController.controller
└── Scripts/
    └── LocomotionController.cs
```

---

## **Going further**

- **Diagonal clips:** Mixamo has `Walking Forward-Right` diagonal clips. Position them at `(1.2, 1.5)` etc. to improve blending quality at 45° input directions.
- **Blend tree layers:** Add a second Animator layer with an AvatarMask for the upper body. The upper body layer can play a `Reloading` or `Aiming` state while the blend tree continues driving the lower body legs — standard pattern for shooter locomotion.
- **Root motion vs script movement:** The `LocomotionController` script sets Animator parameters but does not move the character. If the Animator's **Apply Root Motion** is ticked, movement comes from the animation clip's root bone — footsteps are perfectly matched to actual displacement. Try ticking it and removing the `transform.position +=` code from Lab 4's `PlayerController`.
- **Blend tree transitions:** Add a `Falling` state outside the blend tree. Transition from `Blend Tree` to `Falling` on a `IsGrounded` bool parameter going false. This connects the blend-tree locomotion system to discrete air states — the standard controller architecture for platformers.

---

## **Further reading**

- [Unity Blend Trees — Tutorial (YouTube)](https://www.youtube.com/watch?v=_J8RPIaO2Lc) — a walkthrough covering 1D and 2D blend trees in Unity, recommended as a companion to this lab.
