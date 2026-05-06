---
title: "Decorators — Materials, Lights & Decals"
subtitle: "Unity Animation Mini-Series — Lab 5 of 5"
topic_code: t05_decorators_materials_particles
description: "A 30-minute follow-along lab adding emissive materials, flickering lights, and URP decal projectors to the corridor. Combines authored animation, runtime scripting, and visual polish from previous labs into one decorated scene."
created: 2026-05-02
last_updated: 2026-05-05
version: 1.0
status: published
authors: ["Games Development Teaching Team"]
tags: [unity, unity-6.3-lts, urp, materials, decals, light-flicker, animation-curve, year1, follow-along-lab]
difficulty_tier: Foundational
unity_version: "6.4 LTS"
project_template: "3D (URP) Core"
duration_minutes: 30
previous_topic: t04_input_animator_control
prerequisites:
  - Labs 1–4 completed
  - You have your Blender `EnergyOrb.fbx` (Lab 1) and `Character.fbx` (Labs 2 and 4)
  - Lab 5 uses content from `REPO_LINK/Lab05_Starter/` (continuation of Lab 4 with corridor geometry, three pre-cropped decal PNGs, URP Decal Renderer Feature pre-enabled, Bloom pre-configured)
---

# Decorators — Materials, Lights & Decals
> **Prerequisites:**
> - Labs 1–4 completed.
> - Your Blender `EnergyOrb.fbx` and `Character.fbx` are imported (the starter includes them).
> - You have cloned the labs repository (`REPO_LINK`). Open `REPO_LINK/Lab05_Starter/` as a Unity project — corridor geometry pre-built, URP Decal Renderer Feature pre-enabled in the URP Renderer asset, Bloom pre-configured in the scene's Volume, three decal textures pre-cropped with alpha channels in `Assets/Decals/`.

---

## **What you'll learn**

| Skill Type | You will be able to… |
| :-- | :-- |
| **Conceptual Understanding** | Explain the role of materials, lights, and decals as *decorators* — non-essential systems that amplify the readability and feel of underlying mechanics without changing them. |
| **Editor & Tool Fluency** | Create and edit URP emissive materials, configure URP Decal Projector components, and tune real-time Light intensity from script. |
| **Code Implementation** | Read, configure, and tinker with two complete scripts that drive a material's emission and a light's intensity from runtime values. |
| **Design Skills** | Choose subtle, readable visual effects rather than overwhelming ones — the difference between *polish* and *noise*. |

---

## **Why this matters**
Mechanics make a game *work*. Decorators make a game *feel good*. The same character control from Lab 4 can feel sluggish or snappy, dull or alive — entirely depending on the materials, lights, and decals layered on top.

This is your polish pass. It's also where you learn that great game feel is rarely about a single dramatic effect; it's about a dozen tiny ones, each tied to a specific gameplay state, working together. A glowing orb doesn't just look pretty — it draws the eye through the scene. A flickering corridor light doesn't just add atmosphere — it tells the player something is *off* about this place. A scuff mark on the wall doesn't just decorate the geometry — it tells a story about what happened here before the player arrived.

You'll add three categories of decorator in this lab, each driven by a different technique:
- **Emissive material** on the orb — driven by a script reading an `AnimationCurve` (your Lab 1/3 callback).
- **Flickering corridor lights** — driven by a script reading another `AnimationCurve`, the same pattern applied to a different output target.
- **Static decals on the corridor walls** — no script, just URP's Decal Projector component dressed appropriately.

The lab also pays off the implicit narrative arc: the character walks down the corridor and reaches the chamber at the end, where a static silhouette waits in the dark. We won't say more about that — that's for Lab 6 take-home.

---

## **How this builds on previous content**
**From Lab 1 you have:**
- The bouncing `EnergyOrb` with its `Bounce.anim` clip.
- An `AnimationCurve` you authored in the curve editor.

**From Lab 4 you have:**
- A character that responds to keyboard input via an Animator `Speed` Float parameter.
- A working corridor scene with the character ready to walk.

**Lab 5 layers visuals onto both:**
- The orb gets an emissive material that pulses on each impact, driven by an `AnimationCurve`.
- The corridor's overhead lights get a flicker effect, driven by a *second* `AnimationCurve` evaluated at a different frequency.
- Three decals are placed on the corridor walls and floor: a hazard chevron near the entrance, a warning symbol near the chamber door, and a scuff/scorch mark partway along.

This is the **most important lesson of the series**: decorators *attach to* mechanics, they don't replace them. The Animator from Lab 2, the script from Lab 4, and the curves from Lab 1 all keep working untouched. You're decorating, not rewriting. The same `AnimationCurve` capability appears in three different lab contexts (timeline keyframes, transform driving, material/light driving) — that consistency is what makes Unity's API feel "designed" rather than "assembled."

---

# **Core Ideas / Concepts**

> Each idea is introduced briefly here and revisited concretely in the lab steps. Read these once before starting.

---

### **Core Idea 1 — Materials describe how a surface responds to light**

A material in URP combines a **shader** (the maths) with **properties** (colour, smoothness, emission, etc.). The shader is fixed; you only edit properties.

**Snippet explanation:**
For this lab, you'll use the URP **Lit** shader (the default for opaque objects), and adjust two properties to make the orb glow: Base Map colour and Emission. Emission is the magic one — it makes a material self-light regardless of scene lighting, and combined with **Bloom** (a post-processing effect already enabled in your Volume), it bleeds outwards into a satisfying sci-fi glow.

---

### **Core Idea 2 — Emission requires HDR intensity to bloom convincingly**

Standard colour values run from 0 (black) to 1 (full intensity white). HDR (High Dynamic Range) allows values *above* 1 — essentially "brighter than white." Real screens can't display values above 1, but Bloom uses the over-1 information to decide *how much* to bleed the brightness outwards.

**Snippet explanation:**
When you set an emission colour in Unity, the colour picker has an HDR mode that exposes an `Intensity` slider. Values above 1 produce convincing bloom; values at or below 1 produce flat colour with no halo. You'll see this in Step A.

---

### **Core Idea 3 — Decals are projected materials, not painted textures**

Painting decals onto wall textures works but is inflexible — every variation requires a new texture. URP's Decal Projector projects a small material (a PNG with alpha) onto any geometry inside its bounding box, at runtime. Move the decal, rotate it, change which texture it projects — all without modifying the underlying wall texture.

**Snippet explanation:**
A Decal Projector is just a GameObject with a special component that points at a decal-shader material. The bounding box defines what gets the decal applied; the material defines what gets drawn. URP ships with `Shader Graphs/Decal` for this purpose — you don't author the shader, you just use it. We've pre-enabled the Decal Renderer Feature in the starter project's URP Renderer asset, since adding it requires editing the URP Renderer pipeline asset, which is not something a beginner should fight with mid-lab.

---

### **Core Idea 4 — Driving decorators from gameplay state is a one-line bridge**

```csharp
emission.rateOverTime = animator.GetFloat("Speed") * 10f;
materialInstance.SetColor(EmissionColorID, baseColor * curveValue);
light.intensity = baseIntensity * flickerCurve.Evaluate(t);
```

**Snippet explanation:**
Every decorator script in real games follows the same shape: read a value (from an Animator parameter, a curve, a clock, or another GameObject), do a small calculation, write the result to a renderer/light/particle property. The two scripts you'll write today both follow this exact pattern. Once you've internalised the shape, you can decorate anything: cameras shake on damage, post-processing saturates on adrenaline, audio pitches up when fleeing. **One source of truth, multiple consumers.**

---

# **Progressive Lab Steps (A → B → C → D → E)**

> Total budget: **30 minutes**.
> The starter scene contains: your `EnergyOrb` bouncing in the antechamber, your `Character` controlled by `PlayerController.cs` from Lab 4, a corridor with two overhead `Light` GameObjects, three corridor walls and floor sections, and a sealed chamber door at the far end. URP Bloom is configured. The Decal Renderer Feature is added to the URP Renderer asset.
> **You will not write code from scratch.** Both scripts below are provided complete.

---

### Step A — Create an emissive material for the orb (5 min)

Open the starter project at `REPO_LINK/Lab05_Starter/` in Unity 6.3 LTS. The scene `Assets/Scenes/Lab05_Chamber.unity` should open automatically; if not, open it manually. The scene shows the orb on the antechamber floor and the character standing at the corridor entrance.

In the Project window, navigate to `Assets/Materials/`. Right-click → `Create > Material`. Name the new asset `OrbGlow.mat`.

With `OrbGlow` selected, configure in the Inspector:

- Confirm `Shader` is `Universal Render Pipeline/Lit` (this is the default for new materials in a URP project — leave it alone if so).
- Click the colour swatch next to **Base Map**. Set the colour to a saturated cyan (R: 0, G: 200, B: 255 in standard mode is a good starting point). Close the colour picker.
- Tick the **Emission** checkbox. This enables the emission keyword on the material.
- Click the colour swatch next to `Emission`. The colour picker opens.
- **Important:** at the top of the colour picker, find the **Intensity** slider (visible only when the colour is in HDR mode — Unity's URP Lit material exposes this by default for the Emission slot). Set the colour to bright cyan (e.g., R: 0, G: 1, B: 1) and the **Intensity** to `2`.

Drag `OrbGlow.mat` from the Project window onto the `EnergyOrb` GameObject in the Hierarchy. The orb's surface in the Scene view immediately changes to glowing cyan.

Press Play briefly. The orb bounces and glows steadily. With Bloom enabled in the scene's Volume (already configured for you), the bright pixels bleed outwards into a soft halo. The glow is constant — Step B will make it pulse.

**Checkpoint:** Orb glows steadily with cyan emission and visible bloom. Stop Play.

---

### Step B — Pulse the emission on impact (7 min)

The orb glows constantly, but it would feel more alive if it pulsed on each bounce — bright at impact, dim at the apex.

In `Assets/Scripts/`, right-click → `Create > MonoBehaviour Script`. Name it `EmissivePulse.cs`. Open it and replace its contents with:

```csharp
using UnityEngine;

/// <summary>
/// Pulses a Renderer's emission colour intensity over time, following an AnimationCurve.
/// Drives a *material property* using the same curve technique you used for transforms in Lab 3.
/// Attach to: the EnergyOrb GameObject in the Lab05_Chamber scene.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class EmissivePulse : MonoBehaviour
{
    // The Lit shader exposes its emission colour under the property name "_EmissionColor".
    // We cache the property ID for fast lookup (faster than re-hashing the string each frame).
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    [SerializeField, Tooltip("Author this curve so the pulse spikes at the moment of impact (around t=0.5 if the bounce loops over 1 second). X = normalised time (0–1), Y = brightness multiplier (0–1).")]
    private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Tooltip("How long one pulse cycle takes, in seconds. Match this to your bounce clip's length (typically 1s if your bounce uses 60 frames at 60 fps).")]
    private float duration = 1f;

    [SerializeField, Tooltip("Base hue of the emission. The curve will multiply this colour's intensity each frame.")]
    private Color baseEmission = Color.cyan;

    [SerializeField, Tooltip("Maximum intensity multiplier at the curve's peak. Pushed >1 for HDR bloom.")]
    private float maxIntensity = 4f;

    // We need a *unique* material instance for this object so we don't overwrite the asset
    // on disk and so multiple orbs could pulse independently.
    private Material materialInstance;

    // Time accumulator that loops within [0, duration].
    private float timer;

    private void Start()
    {
        // .material (singular) returns a per-instance copy. Editing it does not affect
        // the shared material asset. .sharedMaterial would, which is rarely what you want.
        materialInstance = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        // 1. Advance the timer and wrap it within [0, duration].
        timer = (timer + Time.deltaTime) % duration;

        // 2. Compute a normalised time t in [0, 1] for curve evaluation.
        float t = timer / duration;

        // 3. Evaluate the curve. The output is multiplied by maxIntensity to control glow strength.
        float intensity = pulseCurve.Evaluate(t) * maxIntensity;

        // 4. Push the new emission colour into the material instance.
        //    Multiplying a Color by a float scales each RGB channel by that amount.
        materialInstance.SetColor(EmissionColorID, baseEmission * intensity);
    }

    // Material instances created with .material are not garbage collected automatically.
    // We destroy our copy when this component is removed to avoid a memory leak across scene reloads.
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
```

Save and wait for compilation. Drag `EmissivePulse.cs` onto the `EnergyOrb` GameObject.

In the Inspector for `EnergyOrb`, find the new `Emissive Pulse (Script)` component. Click the `Pulse Curve` field to open the curve editor. Author a curve that:

- Starts low at `t=0` (apex — minimum glow)
- Stays low until about `t=0.45`
- Spikes sharply at `t=0.5` (impact moment — maximum glow)
- Drops back down by `t=0.55`
- Stays low until `t=1` (apex again)

The curve should look like a single spike around the middle. Right-click keyframes for tangent options if needed.

Set `Duration` to match your bounce clip's length — `1.0` if you used the standard 60-frame loop from Lab 1.

Press Play. The orb now pulses bright on each impact, dims at the apex.

**Checkpoint:** Orb glows brightly when it hits the floor, dims while airborne. The `AnimationCurve` from Lab 1 is now driving a *material* property. Stop Play.

---

### Step C — Make the corridor lights flicker (7 min)

The corridor has two overhead lights pre-placed in the starter scene (`CorridorLight_01` and `CorridorLight_02`, children of the `Corridor` GameObject). They glow steadily — fine, but uninteresting. A flicker effect transforms the corridor's atmosphere.

In `Assets/Scripts/`, create a new MonoBehaviour script called `CorridorLightFlicker.cs`. Replace its contents with:

```csharp
using UnityEngine;

/// <summary>
/// Drives a Light's intensity from an AnimationCurve, producing a flicker effect.
/// Same curve-evaluation technique as EmissivePulse but applied to a different
/// output target (Light.intensity rather than Material.emission).
/// Attach to: each CorridorLight in the Lab05_Chamber scene.
/// </summary>
[RequireComponent(typeof(Light))]
public class CorridorLightFlicker : MonoBehaviour
{
    [SerializeField, Tooltip("Author a flicker pattern here. X = normalised time (0–1), Y = intensity multiplier (typically 0–1.5). Sharp dips create an authentic 'failing fluorescent' feel.")]
    private AnimationCurve flickerCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

    [SerializeField, Tooltip("How long one flicker cycle takes, in seconds. Short values (0.5–2s) feel like an electrical fault; long values (5–10s) feel atmospheric.")]
    private float duration = 1.5f;

    [SerializeField, Tooltip("The light's resting intensity, used as a baseline. The curve multiplies this value each frame.")]
    private float baseIntensity = 1f;

    [SerializeField, Tooltip("Random offset added to the timer at Start. Prevents multiple lights flickering in lockstep, which looks fake.")]
    private bool randomiseStartOffset = true;

    private Light lightComponent;
    private float timer;

    private void Start()
    {
        lightComponent = GetComponent<Light>();

        // Stagger the start time across lights so they flicker out of sync.
        if (randomiseStartOffset)
        {
            timer = Random.Range(0f, duration);
        }
    }

    private void Update()
    {
        // 1. Advance the timer and wrap within [0, duration]. Same pattern as EmissivePulse.
        timer = (timer + Time.deltaTime) % duration;

        // 2. Compute normalised time t.
        float t = timer / duration;

        // 3. Evaluate the flicker curve and apply to the light's intensity.
        //    Multiplying baseIntensity by a curve value of 0.0 fully extinguishes the light;
        //    a curve value of 1.0 returns it to baseline; values >1 make it briefly brighter.
        lightComponent.intensity = baseIntensity * flickerCurve.Evaluate(t);
    }
}
```

Save. Wait for compilation. Drag `CorridorLightFlicker.cs` onto **both** `CorridorLight_01` and `CorridorLight_02`.

For each light, click the `Flicker Curve` field and author a flicker pattern. A good starting flicker:

- Most of the curve sits at `1.0` (steady on)
- One sharp dip to `0.1` around `t=0.3` (a brief blackout)
- Back to `1.0` quickly after
- Optional: a small dip to `0.7` around `t=0.7` (a stutter)

The key insight: most of the time the light is steady; the *interruptions* are what create the flicker character. A continuously wavering light just looks broken in a different way.

Set `Base Intensity` to `1.0` (or whatever the lights' resting intensity was — check their Light component's Intensity field before authoring).

Press Play and look down the corridor. Each light flickers independently because the `randomiseStartOffset` field gives each a different timer start. The corridor now has atmosphere.

**Checkpoint:** Corridor lights flicker independently, with sharp dips rather than a continuous waver. Stop Play.

---

### Step D — Add three decals to the corridor (8 min)

Decals add static environmental storytelling — without them, the corridor walls look pristine. With them, the place feels lived-in.

The starter project has three decal textures pre-cropped with alpha channels in `Assets/Decals/Textures/`:
- `Decal_HazardChevron.png` — yellow/black warning stripe
- `Decal_BiohazardSymbol.png` — a biohazard warning symbol
- `Decal_ScorchMark.png` — a black scuff/scorch mark

You'll create a decal material for each (the material wraps the texture in the URP decal shader), then place a Decal Projector to project each onto the corridor.

**Create the materials:**

In `Assets/Materials/`, right-click → `Create > Material`. Name the asset `DecalHazard.mat`.

With `DecalHazard` selected, in the Inspector:
- Click the **Shader** dropdown at the top. Navigate to `Shader Graphs > Decal`. Select it.
- The Inspector now shows decal-specific properties. Click the small circle next to **Base Map** and select `Decal_HazardChevron`.
- Leave **Normal Map** empty (these decals are flat, not surface-detailed).
- Confirm `Surface Type` (in the material's options) is set up correctly — the Decal shader handles this internally.

Repeat to create `DecalBiohazard.mat` (using `Decal_BiohazardSymbol`) and `DecalScorch.mat` (using `Decal_ScorchMark`).

**Create the decal projectors:**

In the Hierarchy, right-click → `Rendering > URP Decal Projector`. A new GameObject called `Decal Projector` appears in the scene. Rename it `Decal_HazardEntry`.

With `Decal_HazardEntry` selected:
- In the Inspector, find the `Decal Projector` component's `Material` field. Drag `DecalHazard.mat` from the Project window into the slot.
- A yellow rectangular projection appears in the Scene view, with a white arrow showing the projection direction (downwards by default).
- Position the projector near the corridor entrance, at floor level. Use the **Move** tool (W) to position it, **Rotate** tool (E) to point the projection arrow downwards onto the floor.
- Adjust the **Size** fields in the Inspector (`Width`, `Height`, `Projection Depth`) to make the chevron readable but not overwhelming. Try `Width: 2`, `Height: 1`, `Projection Depth: 1`.
- Confirm the chevron appears on the corridor floor when the projection arrow points down.

Repeat to create:
- `Decal_Biohazard` — projected onto the chamber door at the corridor's end. Rotate the projection arrow to point at the door (horizontally).
- `Decal_Scorch` — projected onto a corridor wall midway down. Rotate the projection arrow to point at the wall (horizontally).

You can scale and reposition each projector freely in the Scene view — the projection updates in real time. **Each Decal Projector will only project its decal on geometry within its rectangular bounding box.**

**Checkpoint:** Three decals visible in the scene — hazard chevron on the floor at the corridor entrance, biohazard symbol on the chamber door, scorch mark on a corridor wall. Press Play and walk the character through the corridor; the decals stay in place as static environmental detail.

---

### Step E — Walk the corridor & reflect (3 min)

Press Play. Walk the character (using `PlayerController` from Lab 4) from the antechamber, past the bouncing-pulsing orb, down the flickering corridor with its hazard markings, all the way to the chamber door at the end.

When the character reaches the chamber, look into the dark space beyond — there's a static silhouette there. Don't approach it. We'll let that question hang.

Stop Play. Reflect:

- The orb's pulse, the lights' flicker, and the corridor decals were all added today *without changing* any of the Lab 1–4 work. Lab 1's bounce clip plays. Lab 4's `PlayerController` script runs. Nothing was modified or rewired. The decoration sits *on top of* the mechanics.
- The orb pulse and the light flicker share a script *shape*: cache a renderer/light reference, evaluate an `AnimationCurve` over a looping timer, write the result to a property. Two scripts, one pattern.
- The decals required no script at all — pure editor-side decoration with materials and projectors.

**Checkpoint:** All decorations active, character can walk the full corridor, scene reads as a coherent environment with atmosphere.

*(2-minute buffer for save and Tinker Tasks below)*

---

# **Tinker Tasks**

> Quick experiments. Try at least three before leaving the lab.

| Try this | Notice |
| :-- | :-- |
| Change `OrbGlow`'s emission colour to electric red | The same animation now feels like a charge core / danger signal — colour carries *meaning*, not just decoration |
| Set `EmissivePulse.maxIntensity` to `0.2` | Pulse becomes barely visible — emission needs to push past 1 to bloom convincingly |
| Disable the URP Volume in the scene Hierarchy temporarily | Emission survives but stops blooming — confirms Bloom is half the visual recipe |
| Set both lights' flicker curves to a steady `1.0` value across the whole curve | Lights stop flickering — proves the curve, not the script, encodes the flicker pattern |
| Tick `Use Rendering Layers` on a Decal Projector and exclude the chamber door from `Receive decals` | The biohazard symbol vanishes from the door — useful when you want decals to skip specific objects |
| Move a decal projector while the game is running | Decal moves with it in real time — projectors aren't "baked," they're recalculated each frame |

---

# **Useful Editor Tricks**

| Trick | Why it helps |
| :-- | :-- |
| `Window > Rendering > Lighting > Environment` | Dim the ambient light to make emission and decals more dramatic |
| Right-click material → `Select Dependencies` | Finds every GameObject using a material — useful when polish multiplies across a scene |
| Inspector debug mode (three-dot menu → Debug) | Lets you see "real" values like `_EmissionColor` HDR intensity, not just the editor-friendly version |
| With a Decal Projector selected, drag the on-screen handles | Resizes the projector visually rather than via numeric fields — much faster for tuning |
| `Frame Selected` (F key in Scene view) on a Decal Projector | Centres the camera on the projector's bounding box, useful when projectors get lost in big scenes |

---

# **Debugging & Pitfalls**

| Mistake | Why it happens | Fix |
| :-- | :-- | :-- |
| Material renders pink | Shader is Built-in Standard, not URP Lit | Inspector → top of Material → set Shader to `Universal Render Pipeline/Lit` |
| Emission set but doesn't glow | Intensity ≤ 1 with no Bloom | Raise Intensity above 1 in the HDR colour picker; confirm the URP Volume is active in the scene and Bloom is enabled |
| `materialInstance.SetColor` does nothing | `Emission` keyword not enabled at material level | Tick the `Emission` checkbox in the material once (already in Step A), even though the script drives the colour value |
| Performance drops noticeably with many orbs | Each `.material` access creates a new instance | For 1–2 orbs this is fine. For dozens, switch to `MaterialPropertyBlock` (out of scope here, flag for later) |
| Decal Projector shows the bounding box but no visible decal | Material isn't using `Shader Graphs/Decal` shader | Material's Shader dropdown → `Shader Graphs/Decal`. The standard URP/Lit shader can't be projected |
| Decal appears on the floor but not the walls (or vice versa) | Projection arrow points the wrong way | Rotate the Decal Projector so the white arrow points *into* the surface you want decorated |
| Decal vanishes when far from the camera | Decal Projector's Draw Distance is too low | Inspector → Decal Projector → raise `Draw Distance` |
| Lights flicker in lockstep | `randomiseStartOffset` is false on both, or both have identical curves and durations | Tick `Randomise Start Offset` on both lights (already default in script) |
| Decal Projector option missing from the Hierarchy right-click menu | Decal Renderer Feature isn't added to the URP Renderer | Already configured in starter; if missing, select the URP Renderer asset → Add Renderer Feature → Decal |
| Orb pulse is offset from impact (bright at apex, dim at impact) | Pulse Curve's spike is at the wrong `t` value | Re-author the curve so the spike sits at `t=0.5` (or wherever your bounce clip's impact occurs in normalised time) |

---

# **Reflective Questions**

- You drove the same `Speed` parameter from Labs 2, 4, and earlier today. List every system that *now* consumes that one value across all your labs. What does this tell you about clean game architecture?
- The `AnimationCurve` first appeared in Lab 1 (timeline keyframes), then Lab 3 (procedural transform motion), and now Lab 5 (material emission *and* light intensity). What single Unity capability unifies all four uses? Why is it powerful that the *same* type can drive transforms, materials, and lights?
- Compare the scene with decorators on vs off (mentally; the starter is decorated). Which decorator contributes most to the "feel" — emission, flicker, or decals? Which contributes least?
- A common beginner mistake is *too much* polish — every action explodes with effects. Why is restraint the harder skill?
- Both scripts in this lab use `[RequireComponent]`. What does that prevent at edit time? When would `[RequireComponent]` be wrong (i.e., when do you *want* a script to be attachable to anything)?
- The decals required no script. Why is "no script" sometimes the right answer? When *would* you want to drive a decal from script?

---

# **Software Development Parallel**
The "decorator" pattern in this lab — adding visual layers that observe and react to underlying state without modifying it — mirrors the **Decorator design pattern** in OOP (the same Year 2 module that taught you Interfaces). The character's Animator is the core component; the emission script, flicker script, and decal projectors are decorators that *enhance* without altering the Animator's responsibilities. Same idea, different domain. The lesson: *decoration is composition, not modification*. When you find yourself wanting to add visual flair, your first instinct should be a separate component or projector, not an edit to the existing system.

---

# **Stretch Task (optional, take-home)**
Add a fourth decorator: a **footstep dust** particle system that emits more particles as the character moves faster. The pattern is identical to `EmissivePulse` and `CorridorLightFlicker` — read a runtime value (this time `animator.GetFloat("Speed")` from Lab 4), apply a small calculation, write the result to a particle system property (`emission.rateOverTime`).

*Hint:* attach a Particle System to the `Character` at foot level. Configure its Emission Rate over Time to `0` (the script will drive it). Write a `FootstepEmitter.cs` that reads the character's Animator's `Speed` parameter and assigns `emission.rateOverTime = speed * 10f`. The cleanest implementation closely mirrors `CorridorLightFlicker.cs` — same shape, different output target.

If you want to push further: investigate **Light Cookies** (URP supports projecting a texture through a light, like a stained-glass window). A subtle "warning sign" cookie projected through one of the corridor lights would land the sci-fi atmosphere even harder than the current decals.

---

## Files produced by end of lab
- `Lab05_Decorators/` Unity project (from starter)
- `Assets/Materials/OrbGlow.mat`
- `Assets/Materials/DecalHazard.mat`, `DecalBiohazard.mat`, `DecalScorch.mat`
- `Assets/Scripts/EmissivePulse.cs`
- `Assets/Scripts/CorridorLightFlicker.cs`
- `Assets/Scenes/Lab05_Chamber.unity` (from starter, with three Decal Projectors added)

---

## Lesson Context

```yaml
previous_lesson:
  topic_code: t04_input_animator_control
  domain_emphasis: Games

this_lesson:
  topic_code: t05_decorators_materials_particles
  primary_domain_emphasis: Games
  difficulty_tier: Foundational
  feeds_into: null
```
