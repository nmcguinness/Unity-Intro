---
title: "Input & Character Control"
subtitle: "Unity Animation Mini-Series — Lab 4 of 5"
topic_code: t04_input_animator_control
description: "A 40-minute follow-along lab combining the Animator from Lab 2 with C# scripting from Lab 3, driving character locomotion from keyboard input via Unity 6's project-wide Input System actions. Includes a complete, runnable, beginner-commented PlayerController script and a FootstepAudio script wired to Animation Events."
created: 2026-05-02
last_updated: 2026-05-05
version: 1.0
status: published
authors: ["Games Development Teaching Team"]
tags: [unity, unity-6.3-lts, input-system, animator, character-control, blender, year1, follow-along-lab]
difficulty_tier: Foundational
unity_version: "6.4 LTS"
project_template: "3D (URP) Core"
duration_minutes: 40
previous_topic: t03_procedural_animation
prerequisites:
  - Labs 1–3 completed
  - You can attach a script to a GameObject and reference Animator parameters
  - Uses the [repo](https://github.com/nmcguinness/Unity-Intro).
---

# Input & Character Control
> **Prerequisites:**
> - Labs 1–3 completed.
> - You have your `Character.fbx` (Blender, with `Idle` and `Walk` clips).
> - You have cloned the labs [repo](https://github.com/nmcguinness/Unity-Intro) to your machine.
> - Open the Unity project — Lab 2's Animator is pre-built (and prepared for the Bool to Float upgrade you'll do in Step B), the character is already in the scene, and Unity 6's default project-wide input actions are active (no manual setup needed).

---

## **What you'll learn**

| Skill Type | You will be able to… |
| :-- | :-- |
| **Conceptual Understanding** | Explain how a script reads input, computes a value, and writes it to an Animator parameter to drive state transitions. |
| **Editor & Tool Fluency** | Locate and use Unity 6's project-wide `InputSystem_Actions` asset, recognise the default `Move` and `Sprint` actions, and access them from script. |
| **Code Implementation** | Read, configure, and tinker with a complete `PlayerController` script that reads input, drives the Animator, and moves the character through space. |
| **Problem-Solving** | Diagnose the four classic locomotion bugs: character won't move, character moves but doesn't animate, character animates but doesn't move, and character moves *too fast* (skating feet). |

---

## **Why this matters**
Up until now, your animations have played either on loop (Labs 1 and 3) or by you toggling parameters by hand (Lab 2). Real games are *interactive* — the player presses a key and the character responds. This lab is the moment your character stops being a puppet on autopilot and starts responding to you.

You'll wire **input → script → Animator parameter → state transition → visible animation** end-to-end. This pipeline is the heart of every action game ever shipped in Unity. Once you've built it for one Bool/Float parameter, you can extend it indefinitely: the same pattern handles jumping, attacking, crouching, blocking, and every other player verb your game might support.

This lab also showcases a recent improvement in Unity 6: **project-wide Input System actions**. In older versions of Unity, students had to author an Input Actions asset manually, drag references into Inspector fields, manage Enable/Disable lifecycles, and learn five other concepts before reading their first input value. Unity 6 ships with sensible defaults already in place — `Move`, `Sprint`, `Jump`, `Attack`, and a dozen others, all bound to keyboard, mouse, and gamepad. You can read them from anywhere with one line of code. We'll take advantage of that.

---

## **How this builds on previous content**
**From Lab 2 you have:**
- An Animator Controller with `Idle` and `Walk` states.
- A `IsWalking` Bool parameter that *you toggled by hand* in the Animator window during Play mode.

**From Lab 3 you have:**
- Comfort reading `[SerializeField]`, `Update()`, and cached references in `Awake()`/`Start()`.
- The `startPos` cache pattern for stable per-frame motion.

**Lab 4 connects them — and upgrades the controller:**
- **Step B replaces the `IsWalking` Bool with a `Speed` Float.** This is a deliberate teaching moment: you'll see *why* Float beats Bool for locomotion the moment you try to scale walk-clip playback speed with the character's actual movement speed. You can't multiply by a Bool.
- Your script reads keyboard input via Unity 6's project-wide actions (`Move` for direction, `Sprint` for the run modifier).
- It computes a `currentSpeed` value (continuous, like Lab 3's procedural motion).
- It writes that value into the Animator's `Speed` parameter, which drives the transitions wired in Lab 2.
- It also moves the character through space and rotates it to face the movement direction, so animation and locomotion stay in sync.

**This sets up Lab 5**, where you'll add materials and particles that *react* to the same `Speed` parameter — the orb's emission pulses and the corridor lights flicker, all driven by data the controller already exposes.

---

# **Core Ideas / Concepts**

> Each idea is introduced briefly here and revisited concretely in the lab steps. Read these once before starting.

---

### **Core Idea 1 — Input is a value you read every frame**

Unity's Input System exposes input as **Actions** (e.g. "Move", "Sprint", "Jump"). In a script, you read the current value of an Action each frame and react to it. The Action doesn't care whether the value came from a keyboard, gamepad, or touchscreen — that abstraction is the whole point of the Input System.

**Snippet explanation:**
The default `Move` action is a `Vector2`, bound to `WASD`, the arrow keys, and the gamepad left stick. `ReadValue<Vector2>()` returns `(0, 0)` when nothing is pressed, `(0, 1)` when forward, `(-1, 0)` when left, etc. The `Sprint` action is a button, bound to Left Shift and the gamepad right shoulder. `IsPressed()` returns `true` while the button is held. Both actions exist in Unity 6 by default — you don't author them.

---

### **Core Idea 2 — Animator parameters are the bridge between code and state**

```csharp
animator.SetFloat("Speed", currentSpeed);
```

**Snippet explanation:**
This single line is the entire bridge. The Animator was wired in Lab 2 — once you upgrade it from Bool to Float in Step B, you just need to tell it what `Speed` is each frame. All the transitions (Idle ↔ Walk) fire automatically. The Animator doesn't know who set the value or why; it only cares that the value exists.

---

### **Core Idea 3 — Animation and locomotion are *separate concerns***

The Animator decides *which clip plays*. The script decides *where the character is*. They must agree, but they're independent systems.

**Snippet explanation:**
A common bug is animating a character running while it stays glued to one spot — or moving the character while it idles. The provided script handles both pieces. You'll tune them so they feel synchronised: when the character moves at `2 units/sec`, the walk clip plays at its authored speed; when sprinting at `5 units/sec`, the clip plays faster so the legs match. This is the "skating feet" problem and one easy parameter is all that prevents it.

---

### **Core Idea 4 — Float beats Bool when you need a *spectrum* of values**

A Bool can be *either* true or false. A Float can be *anywhere* between two extremes. Locomotion benefits from a Float because:

- You can scale the walk clip's playback speed with the Float, avoiding "leg-skating" entirely.
- You can add a `Run` state in future without rewiring anything — just add a transition with condition `Speed > 4`.
- Lab 5's particle system reads the Float to scale dust emission. The same Float drives multiple consumers without modification.

The cost of upgrading from Bool to Float is one parameter type change in the Animator and one method call in script (`SetFloat` instead of `SetBool`). Tiny investment, large payoff.

---

# **Progressive Lab Steps (A → B → C → D → E → F)**

> Total budget: **40 minutes**.
> **You will not write code from scratch.** The script below is provided complete.

---

### Step A — Open the starter and verify Unity 6 input actions (4 min)

Open the project from the repo in Unity 6.4 LTS. The scene `Assets/Scenes/Lab04_Corridor.unity` should open automatically; if not, open it manually.

Check the scene:
- The Hierarchy contains your `Character` (the Blender FBX from Lab 2) standing on a `CorridorFloor` plane.
- The `Character` has a Lab 2-style Animator Controller assigned, with `Idle` and `Walk` states already authored.
- A `Main Camera` is positioned to view the character from a slight angle.

Verify Unity 6's project-wide actions are configured (they should be; this is the default in Unity 6):

1. Open `Edit > Project Settings`. In the left column, find **Input System Package**. Click it.
2. The right pane should show the Input Actions Editor with two action maps: `Player` and `UI`. Inside `Player`, you should see entries including `Move`, `Look`, `Jump`, `Sprint`, `Attack`, `Interact`, `Crouch`.
3. Click `Move` — its bindings include `WASD`, the gamepad left stick, and arrow keys. Click `Sprint` — its bindings include Left Shift and the gamepad right shoulder.
4. Close the Project Settings window.

If those default actions aren't present, the project-wide actions asset has been deleted. Recover by clicking **Create and assign a default project-wide Action Asset** in Project Settings → Input System Package. Unity will recreate the defaults.

Check the `Character`'s Animator component in the Inspector:
- The Animator Controller field references `CharacterController.controller`.
- `Apply Root Motion` is **unticked** (your in-place walk clip means the script handles all locomotion).

Press Play. The character idles in place. Press W or move the gamepad stick — the character will *not yet* respond. That's expected. There's no script yet.

**Checkpoint:** Scene plays, character idles, default actions visible in Project Settings. No movement yet.

---

### Step B — Upgrade the Animator from Bool to Float (5 min)

The Lab 2 Animator used a Bool parameter `IsWalking` because Lab 2 only needed binary state. Locomotion driven from input is a *continuous* problem — how *fast* you're moving, not just *whether* you're moving. So we'll upgrade.

Open `Assets/Animators/CharacterController.controller` (in the Project window) by double-clicking it. The Animator window opens.

In the **Parameters** tab (top-left, next to **Layers**):
1. Click `+` → `Float` → name the new parameter `Speed`. Make sure it's spelled exactly `Speed` (capital S) — Unity is case-sensitive about parameter names.
2. Right-click the existing `IsWalking` Bool parameter → `Delete`. Confirm if Unity asks.

Now update the transitions to use the new parameter:

1. Click the `Idle → Walk` transition arrow (the one going from Idle to Walk). In the Inspector:
   - Under **Conditions**, click the existing condition (`IsWalking` `true`, now showing as missing/red because we deleted the Bool).
   - Click `-` to remove it.
   - Click `+`. The new condition automatically uses the only available parameter: `Speed`. Set the comparison to `Greater` and the value to `0.1`.
2. Click the `Walk → Idle` transition arrow. In the Inspector:
   - Remove the existing condition.
   - Add a new condition: `Speed` `Less` `0.1`.

Both transitions still have `Has Exit Time` unticked from Lab 2 — confirm this hasn't changed, because we still want instant response to input.

**Checkpoint:** Animator has one Float parameter `Speed`, two transitions both conditioned on it, no Bool, `Has Exit Time` off on both transitions.

---

### Step C — Add the PlayerController script (12 min)

In the Project window, right-click `Assets/Scripts/` (create the folder if it doesn't exist) → `Create > MonoBehaviour Script`. Name it `PlayerController.cs`. Open it in your IDE.

**Replace the entire file contents** with the code below. Read the comments — they explain every section.

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Drives a character from keyboard/gamepad input.
/// Reads Unity 6's project-wide "Move" (Vector2) and "Sprint" (button) actions,
/// computes a speed value, writes it to the Animator's "Speed" parameter,
/// and moves the character through space.
///
/// Control scheme:
///  W / S  — move forward / backward along the character's facing direction.
///  A / D  — rotate (yaw) the character left / right.
///  This lets you complete a full 360° turn on the spot or while walking.
///
/// REQUIRES:
///  - An Animator on the same GameObject with a Float parameter called "Speed".
///  - Unity 6's default project-wide actions, which contain "Move" and "Sprint".
///
/// Attach to: the Character GameObject in the Lab04_Corridor scene.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // ----- Inspector fields -------------------------------------------------

    [Header("Locomotion")]
    [SerializeField, Tooltip("World-space movement speed when walking, in units per second.")]
    private float walkSpeed = 2f;

    [SerializeField, Tooltip("World-space movement speed when sprinting, in units per second.")]
    private float runSpeed = 5f;

    [SerializeField, Tooltip("How fast `currentSpeed` ramps toward target. Higher = snappier, lower = floatier.")]
    private float acceleration = 8f;

    [SerializeField, Tooltip("Rotation speed in degrees per second when pressing A or D.")]
    private float turnSpeed = 180f;

    [Header("Animation Sync")]
    [SerializeField, Tooltip("The world-units-per-second the walk clip was authored for. Used to scale clip playback speed so the legs match the body's velocity. Ask your Blender lecturer if unsure — 2 is a common default.")]
    private float walkClipBaselineSpeed = 2f;

    // ----- Cached references -----------------------------------------------

    private Animator animator;

    // The two project-wide actions we'll read every frame.
    // Cached in Awake() so we look them up once, not every frame.
    private InputAction moveAction;
    private InputAction sprintAction;

    // ----- Runtime state ---------------------------------------------------

    // The smoothed speed value we feed the Animator each frame.
    // Smoothed (rather than raw) so the character ramps up/down naturally
    // instead of snapping between speeds.
    private float currentSpeed;

    // ----- Unity lifecycle methods -----------------------------------------

    private void Awake()
    {
        // Cache the Animator reference once. Calling GetComponent every frame is wasteful.
        animator = GetComponent<Animator>();

        // Look up the project-wide actions by name.
        // InputSystem.actions is auto-populated from the project-wide Action Asset
        // (the default in Unity 6 — no Inspector field needed, no Enable/Disable lifecycle).
        // FindAction returns the InputAction we can read every frame.
        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    private void Update()
    {
        // 1. Read inputs.
        //    moveInput.x = A/D (rotation), moveInput.y = W/S (forward/back).
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool wantsToSprint = sprintAction.IsPressed();

        float horizontal = moveInput.x; // A/D — yaw rotation
        float vertical   = moveInput.y; // W/S — forward/back translation

        // 2. Compute the *target* speed for this frame.
        //    Speed is based only on forward/back input so that pressing A or D
        //    on the spot rotates the character without triggering the Walk animation.
        float targetSpeed = Mathf.Abs(vertical) * (wantsToSprint ? runSpeed : walkSpeed);

        // 3. Smoothly ramp currentSpeed toward targetSpeed.
        //    Mathf.Lerp here gives natural acceleration instead of a snap from 0 to full speed.
        //    Multiplying by Time.deltaTime makes the ramp frame-rate independent.
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // 4. Push the speed into the Animator. The Lab 2 transitions will respond:
        //    - Speed > 0.1 → Walk
        //    - Speed < 0.1 → Idle
        animator.SetFloat("Speed", currentSpeed);

        // 5. Scale the walk clip's playback speed so the legs match the body's velocity.
        //    Without this, the character "skates" — feet stationary while the body slides.
        //    We treat walkClipBaselineSpeed as "the world-speed the clip was authored for"
        //    and scale linearly from there. The Mathf.Max prevents the clip from playing
        //    too slowly at low speeds (which looks worse than playing at normal speed).
        if (currentSpeed > 0.1f)
        {
            animator.speed = Mathf.Max(0.5f, currentSpeed / walkClipBaselineSpeed);
        }
        else
        {
            animator.speed = 1f;
        }

        // 6. Rotate: A/D yaw the character around its up axis.
        //    turnSpeed is in degrees per second, so a full 360° turn takes
        //    (360 / turnSpeed) seconds — about 2 seconds at the default 180.
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);
        }

        // 7. Translate: W/S move the character along its *current* facing direction.
        //    Using transform.forward (not world +Z) means the character always moves
        //    in the direction it is facing, regardless of how much it has rotated.
        if (Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 moveDir = transform.forward * Mathf.Sign(vertical);
            transform.position += moveDir * currentSpeed * Time.deltaTime;
        }
    }
}
```

Save the file. Wait for compilation.

Drag `PlayerController.cs` from the Project window onto the `Character` GameObject in the Hierarchy. With `Character` selected, look at the Inspector — there's now a `Player Controller (Script)` component with all six tuneable fields visible.

**Checkpoint:** Press Play. Press `W` — character walks forward and animates. Hold `Shift + W` — character runs, clip plays faster. Press `A` or `D` — character rotates on the spot without triggering the Walk animation. Hold `W` then steer with `A`/`D` — character walks in a curve and can complete a full 360° turn. Release — character returns to idle.

---

### Step D — Tune the feel (5 min)

The default values in the script are reasonable but probably not perfect for your Blender character. Adjust during Play mode:

- **`walkSpeed`** and **`runSpeed`**: how fast the character translates through the world. If movement feels too slow, raise; if it feels rushed, lower. Try `walkSpeed = 1.5` and `runSpeed = 4` for a more deliberate feel; try `3` and `7` for a snappier action-game feel.
- **`acceleration`**: how quickly the character ramps up from stopped to walking. `8` is responsive but not instant. `2` feels heavy and momentum-driven. `30` feels arcade-like and snappy.
- **`turnSpeed`**: rotation speed in **degrees per second** when pressing A or D. `180` completes a half-turn in one second — responsive but not instant. `90` feels heavy and deliberate (a full circle takes 4 seconds). `360` snaps to any direction almost instantly.
- **`walkClipBaselineSpeed`**: the most important tuning value for synchronising legs and body. If the character's legs cycle *faster* than the body's apparent movement speed, raise this value. If they cycle *slower*, lower it. Typical values are between `1.5` and `3`. The right value depends on how your Blender lecturer authored the walk cycle — ask if unsure.

The visible test for `walkClipBaselineSpeed`: at normal walking speed, the character's foot should appear to be planted on the floor as the body moves over it (no foot-sliding). If feet slide forwards, the clip plays too slow; if they slide backwards, the clip plays too fast.

**Checkpoint:** Believable locomotion at both walk and run, with no obvious foot-sliding.

---

### Step E — Add footstep audio via Animation Events (8 min)

Right now the character walks in silence. In this step you'll create a `FootstepAudio` script that holds a pool of audio clips and exposes a method called `OnFootstep`. You'll then wire that method to an **Animation Event** on the walk clip — Unity will call `OnFootstep` automatically at the exact frame each foot strikes the floor.

#### Part 1 — Create the script

In the Project window, right-click `Assets/Scripts/` → `Create > MonoBehaviour Script`. Name it `FootstepAudio.cs`. Open it and replace the entire contents with the code below.

```csharp
using UnityEngine;

/// <summary>
/// Plays a random clip from a pool each time OnFootstep() is called.
/// OnFootstep is invoked by an Animation Event placed on the walk clip —
/// Unity calls it at the exact frame you choose, so audio stays in sync
/// with the foot striking the ground without any manual timing code.
///
/// REQUIRES: an AudioSource on the same GameObject (auto-added via RequireComponent).
/// Attach to: the Character GameObject alongside PlayerController.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [SerializeField, Tooltip("Pool of footstep clips to pick from at random. Add 2–4 variations to avoid a robotic repeating sound.")]
    private AudioClip[] footstepClips;

    [SerializeField, Range(0f, 1f), Tooltip("Playback volume for each footstep hit.")]
    private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Called by the Animation Event on the walk clip at each footfall frame.
    // Must be public so the Animator can find it by name at runtime.
    public void OnFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        // Pick a random clip so consecutive steps don't sound identical.
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        // PlayOneShot lets overlapping calls play simultaneously — important when
        // the character walks quickly or sprints and events fire close together.
        audioSource.PlayOneShot(clip, volume);
    }
}
```

Save and wait for compilation. Drag `FootstepAudio.cs` onto the `Character` GameObject in the Hierarchy — Unity will automatically add an `AudioSource` component alongside it (enforced by `[RequireComponent]`).

#### Part 2 — Assign footstep audio clips

You need at least one `AudioClip`. Import your footstep audio files (`.wav` or `.mp3`) into `Assets/Audio/` by dragging them from Windows Explorer into the Project window.

1. Select the `Character` GameObject. In the Inspector, find the **Foot Step Audio (Script)** component.
2. Set **Footstep Clips** size to the number of clips you imported, then drag each clip into a slot.
3. Leave **Volume** at `0.5` for now — you'll tune it in a moment.

> **Tip:** 2–4 slightly different clips (same surface, slightly different pitch or room tone) make footsteps feel far more natural than a single clip looping.

#### Part 3 — Wire the Animation Event

Animation Events live on the clip itself. Because `Character.fbx` is imported, you add events through the FBX importer's **Animation** tab.

1. In the Project window, select your `Character.fbx`.
2. In the Inspector, click the **Animation** tab.
3. Under **Clips**, select your **Walk** clip.
4. Scroll down to the **Events** section — a mini timeline appears.
5. Scrub the preview (bottom of Inspector) to find the frame where the **left foot** is flattest on the floor.
6. Click `+` to add an event at that frame. In the **Function** field, type exactly `OnFootstep` (capital O and F — Unity is case-sensitive).
7. Repeat for the **right foot** footfall frame.
8. Click **Apply** at the bottom of the Inspector.

**How it works:** When the walk clip plays, the Animator reaches those event frames and calls `OnFootstep()` by name on every `MonoBehaviour` on the same GameObject. Your `FootstepAudio` component receives the call and plays a random clip from the pool.

Press Play. Walk the character — you should hear a footstep on each footfall. Sprint — the sounds speed up automatically because `animator.speed` (set in `PlayerController`) also scales how fast Animation Events fire.

**Checkpoint:** A footstep sound plays on each footfall. Two events fire per walk cycle (left foot, right foot). Sprinting doubles the tempo of the sounds.

---

### Step F — Compare with Lab 2 (2 min)

In the Animator window during Play mode, watch the `Speed` parameter scrub up and down with your input. Recall Lab 2: you toggled `IsWalking` by hand in this same window. Now the script does it automatically — and as a *spectrum* rather than a binary.

Notice also that the `Speed` parameter ramps smoothly between values rather than jumping — that's the `Mathf.Lerp` smoothing in your script doing its work. If you replaced `Mathf.Lerp` with direct assignment, the parameter would snap between 0 and 2 (or 5) instantly, and the character would feel "twitchy."

**Checkpoint:** You can articulate why the Float upgrade was worth doing, and you can identify which line of the script controls the smoothing.

*(2-minute buffer for save and Tinker Tasks below)*

---

# **Tinker Tasks**

> Quick experiments. Try at least three before leaving the lab.

| Try this | Notice |
| :-- | :-- |
| Set `acceleration` to `1` | Character takes ages to ramp up — feels heavy and sluggish, like running through water |
| Set `acceleration` to `100` | Snappy, almost instantaneous response — feels arcade-y, common in fast-paced action games |
| Comment out the `animator.speed = ...` lines (Step 5 in the script) | Legs no longer match running speed — character "skates" when sprinting. Confirms what `walkClipBaselineSpeed` does. |
| Set `turnSpeed` to `45` | Character turns very slowly — one full circle takes 8 seconds. Feels like steering a boat |
| Set `turnSpeed` to `360` | Instant snap to any direction. Feels arcade-y. Notice there is no easing at all |
| Replace the `Mathf.Lerp(currentSpeed, targetSpeed, ...)` with `currentSpeed = targetSpeed;` | No smoothing — character snaps between speeds. Confirms why we Lerp. |
| In Project Settings, swap the `Move` action's WASD binding for arrow keys only | Same code still works — proves the binding/action separation in action |
| Reduce **Footstep Clips** to a single clip | Notice the robotic "tick tick tick" repetition — confirms why a pool of variations matters |
| Set footstep **Volume** to `1.0` then `0.1` | Find where audio supports movement without dominating it |
| Remove one of the two Animation Events (keep left foot only) | Every other step is silent — makes the asymmetry obvious and shows how events map to specific frames |

---

# **Useful Snippets (Guards & Helpers)**

### `[RequireComponent]` enforces dependencies

```csharp
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour { ... }
```

**Why?**
Unity will refuse to add `PlayerController` to a GameObject without an `Animator` — and refuse to remove the Animator while `PlayerController` is attached. Catches setup mistakes at edit time, not runtime. Defensive coding without runtime cost.

### Guard dead-zone input before acting

```csharp
if (Mathf.Abs(horizontal) > 0.01f)
    transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);

if (Mathf.Abs(vertical) > 0.01f)
    transform.position += transform.forward * Mathf.Sign(vertical) * currentSpeed * Time.deltaTime;
```

**Why?**
At rest, gamepad sticks rarely return exactly `(0, 0)` — they drift slightly within a small dead zone. The `> 0.01f` threshold discards that noise so the character stays still. Each axis is checked independently: you can rotate without translating (A/D on the spot) or translate without rotating (W/S straight ahead).

---

# **Debugging & Pitfalls**

| Mistake | Why it happens | Fix |
| :-- | :-- | :-- |
| Character animates but doesn't move | Animator's `Apply Root Motion` is on, OR the `transform.position +=` line is missing | Untick `Apply Root Motion` on the Animator component (in-place clip + script-driven movement); confirm the script's `direction.sqrMagnitude > 0.01f` block runs |
| Character moves but doesn't animate | `Speed` parameter name doesn't match (case-sensitive), or the parameter doesn't exist on the Animator | In the Animator, the parameter must be exactly `Speed` (capital S). If you skipped Step B, the upgrade isn't done. |
| `NullReferenceException` on `moveAction.ReadValue<Vector2>()` | `InputSystem.actions.FindAction("Move")` returned null because the project-wide actions were deleted | Project Settings → Input System Package → Create and assign a default project-wide Action Asset |
| Character moves too fast / "skates" / legs lag behind body | `walkClipBaselineSpeed` doesn't match your Blender clip's authored speed | Tune `walkClipBaselineSpeed` in the Inspector — raise it if legs cycle too fast, lower it if too slow |
| Input doesn't respond at all | Active Input Handling set to "Old (Input Manager)" only | Project Settings → Player → Active Input Handling must be `Input System Package` or `Both`. Unity 6 defaults to `Both`, but a project upgraded from older Unity may have `Old` selected |
| Character drifts sideways instead of turning | Using an old version of the script that mapped A/D to world-space strafe | Replace with the current script — A/D call `transform.Rotate`, W/S translate along `transform.forward` |
| Compilation error: "InputSystem does not contain a definition for 'actions'" | The Input System package isn't installed, or its version is too old | Window → Package Manager → search "Input System" → install or update. Unity 6 ships with this; only happens in projects upgraded from very old Unity versions |
| Character drifts forward during Idle | Walk clip has root motion baked in (not in-place) | Re-export from Blender as in-place; or in the FBX's Animation tab, set `Root Motion Node` to `<None>` and set `Bake Into Pose` for X/Y/Z |
| Sprint doesn't change speed | `Sprint` action's binding doesn't match what you're pressing | Project Settings → Input System Package → click Sprint → confirm bindings include Left Shift on keyboard |

---

# **Reflective Questions**

- The `Speed` parameter is set every frame by your script. What did you do *manually* in Lab 2 that the script now does automatically?
- Why does smoothing (`Mathf.Lerp`) on `currentSpeed` make the character feel more believable than raw input?
- Animation and locomotion are independent. Can you imagine a game where they're *intentionally* desynchronised? (Hint: cinematic stumbles, getting hit, ice physics.)
- Lab 2 used a Bool. Lab 4 upgraded to a Float. Why did the Float make the rest of the script easier to write? List at least two specific places in the code that would have been harder with a Bool.
- The Input System uses Action References (named "Move", "Sprint") instead of polling specific keys (`if (Input.GetKey(KeyCode.W))`). What advantage does that give you when you later want to support gamepad? When you want to let players rebind controls?
- Project-wide actions in Unity 6 mean you write *less* setup code than the old InputActionReference approach. What's the trade-off? (Hint: where do the bindings live, and what happens if a different scene needs different bindings?)

---

# **Software Development Parallel**
The pattern of "read input → compute value → write to system" is the **observer / data-flow pattern** common in reactive UI frameworks (React, SwiftUI, Jetpack Compose). Your `Update()` loop is the simplest possible reactive system: input changes → state recomputes → view (Animator + transform) updates. Modern UI frameworks formalise this pattern with hooks, observables, or signals; Unity's `Update()` does it with a tick. The *idea* is the same: derive view from state, recompute when state changes.

---

# **Stretch Task (optional, take-home)**
Add a `Jump` Trigger to the Animator (Lab 2's stretch task), then bind it to the spacebar via the existing `Jump` action in Unity 6's project-wide actions (already present, no setup needed).

In `PlayerController.cs`:
1. Add a private field: `private InputAction jumpAction;`
2. In `Awake()`: `jumpAction = InputSystem.actions.FindAction("Jump");`
3. In `Update()`: `if (jumpAction.WasPressedThisFrame()) animator.SetTrigger("Jump");`

Note the use of `WasPressedThisFrame()` instead of `IsPressed()` — Triggers should fire once per press, not continuously while held. This is the API distinction you'll meet repeatedly in Unity scripting.

---

## Files produced by end of lab
- `Lab04_CharacterControl/` Unity project (from starter)
- `Assets/Models/Character.fbx` (from Lab 2's Blender import, walk clip has two footstep Animation Events added in Step E)
- `Assets/Animators/CharacterController.controller` (upgraded from Bool to Float in Step B)
- `Assets/Scripts/PlayerController.cs`
- `Assets/Scripts/FootstepAudio.cs`
- `Assets/Audio/` (your footstep clips)
- `Assets/Scenes/Lab04_Corridor.unity` (from starter)

---

## Lesson Context

```yaml
previous_lesson:
  topic_code: t03_procedural_animation
  domain_emphasis: Balanced

this_lesson:
  topic_code: t04_input_animator_control
  primary_domain_emphasis: Games
  difficulty_tier: Foundational
  feeds_into: t05_decorators_materials_particles
```
