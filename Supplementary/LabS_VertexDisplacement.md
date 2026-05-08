---
title: "Vertex Displacement — Time Animation, Mouse Ripples & Ocean Waves"
subtitle: "Unity Animation Mini-Series — Supplementary Lab"
topic_code: ts_vertex_displacement_shadergraph
description: "A 60-minute supplementary lab introducing vertex displacement in Unity 6's Shader Graph. Students build three progressive shaders: a time-driven normal-pulse on a sphere, a mouse-position-driven ripple on a plane (with a companion C# script passing world-space cursor position to the material), and a sine-wave ocean surface driven by spatial frequency and time."
created: 2026-05-07
last_updated: 2026-05-07
version: 1.0
status: published
authors: ["Games Development Teaching Team"]
tags: [unity, unity-6.3-lts, urp, shadergraph, vertex-displacement, procedural, mouse-input, ocean-wave, year1, supplementary-lab]
difficulty_tier: Advanced
unity_version: "6.4 LTS"
project_template: "3D (URP) Core"
duration_minutes: 60
previous_topic: t05_decorators_materials_particles
prerequisites:
  - Labs 1–5 recommended — especially Lab 5's introduction to materials and shaders
  - Comfortable creating Materials and attaching Scripts to GameObjects
  - Uses the [repo](https://github.com/nmcguinness/Unity-Intro).
---

# Vertex Displacement — Time Animation, Mouse Ripples & Ocean Waves
> **Prerequisites:**
> - You can create a Material in the Project window, change its Shader, and assign it to a GameObject.
> - You have cloned the labs repository. Open the project in Unity 6.4 LTS — URP is configured, Shader Graph is installed.
> - This lab sits **outside the assessed 3-hour series**. It may be completed before, between, or after the numbered labs — it has no narrative dependency on any of them.

---

## **What you'll learn**

| Skill Type | You will be able to… |
| :-- | :-- |
| **Conceptual Understanding** | Explain the difference between the Vertex and Fragment stages of a shader, and why vertex displacement must happen in the Vertex stage before rasterisation. |
| **Editor & Tool Fluency** | Create a URP Shader Graph, add properties to the Blackboard, wire nodes in the Vertex context, and connect a C# script to a shader property via `Material.SetVector`. |
| **Code Implementation** | Read, configure, and tinker with a `MouseRippleController` script that ray-casts the cursor into the scene and passes the world-space hit point to a shader property every frame. |
| **Design Skills** | Tune wave amplitude, frequency, and speed to produce visually convincing results; understand how small changes to each parameter feel to a player. |
| **Problem-Solving** | Diagnose the three classic vertex displacement bugs: no displacement visible (wrong stage or wrong output port), geometry stretching (incorrect space), and jitter at the mesh boundary (space mismatch). |

---

## **Why this matters**
Every technique in this series so far has decorated the surface of geometry — materials, emission, decals, lights. None of them changed the actual *shape* of the mesh. Vertex displacement changes that.

Moving vertices in a shader is how studios produce water surfaces, breathing foliage, character skin deformation under impact, flag cloth simulation, heat distortion rising from asphalt, and the subtle "alive" wobble of sci-fi energy shields — all at GPU speed, with no CPU physics simulation. The geometry is authored once in a DCC tool; the shader decides where each vertex actually sits, every frame.

The techniques here are also the entry point to **procedural mesh animation** — a distinct paradigm from keyframed clips (Labs 1, 2) and ragdoll physics. Procedural mesh animation is fully parameterised: you expose sliders for frequency, amplitude, speed, and your art director can tune the ocean without touching a line of code. That clean separation — authors control *parameters*, code controls *execution* — is what makes shader-driven effects ship successfully under deadline.

You'll build three demonstrations in ascending complexity:
1. **Time Pulse** — the simplest vertex animation, pushing vertices along their normals on a sine wave.
2. **Mouse Ripple** — displacing only the vertices nearest to the cursor, driven by a C# script injecting the cursor's world position into the shader every frame.
3. **Ocean Wave** — a sine wave sweeping across a plane in one spatial direction while time advances, producing a simple but convincing water surface.

---

## **How this builds on previous content**
**From Lab 5 you know:**
- That materials have a Shader — the maths that computes how a surface looks.
- That a material property (like Emission HDR colour) can be changed from script.

**This lab goes deeper:**
- Instead of changing surface *colour*, you change surface *position* — vertex displacement is a property written to the Vertex stage output, not the Fragment stage.
- The pattern `material.SetFloat("_MyProp", value)` from Lab 5 extends to `material.SetVector("_MouseWorldPos", pos)` here — the mechanism is identical, only the data type differs.

**After this lab,** the Year 2 Materials & Shaders module will revisit these techniques with HLSL and custom shader files, giving you full control over the GPU pipeline. This lab builds the intuition those lectures assume.

---

# **Core Ideas / Concepts**

> Read all four before starting the practical steps.

---

### **Core Idea 1 — The Vertex Stage runs before the Fragment Stage**

A shader executes in two sequential stages. The **Vertex Stage** runs once per vertex — it decides *where* each corner of each triangle ends up in screen space. The **Fragment Stage** runs once per pixel — it decides *what colour* each screen pixel shows. Vertex displacement means writing a modified position to the Vertex Stage output. By the time the Fragment Stage runs, the geometry has already been moved; fragments are shaded on the displaced surface.

**Snippet explanation:**
In Shader Graph, the **Master Stack** shows two contexts: `Vertex` and `Fragment`. The `Vertex` context has three output ports: `Position` (Object space), `Normal` (Object space), and `Tangent`. Everything in this lab goes to the `Position` port. Nodes wired to Fragment outputs (like `Base Color` and `Alpha`) have no effect on vertex position — you cannot displace geometry from the Fragment stage.

---

### **Core Idea 2 — Displacement must be expressed in the correct coordinate space**

ShaderGraph nodes produce and consume values in specific spaces (Object, World, View). The `Vertex Position` output expects **Object space**. If you compute your displacement in World space, you must convert back to Object space using a `Transform` node before connecting to the output — otherwise the geometry will distort or skip when the GameObject is moved, rotated, or scaled.

**Snippet explanation:**
Step 2 (Time Pulse) works entirely in Object space — no conversion needed. Steps 3 and 4 receive world-space inputs (mouse world position, world-scale wave frequency) and therefore end with a `Transform` (World → Object, Type: Position) node before the output port. Forgetting this node is the #1 source of "geometry looks fine in place but explodes when I move the object" bugs.

---

### **Core Idea 3 — Normal-direction displacement preserves shape; Y-axis displacement is simpler but less flexible**

If you push every vertex by a scalar amount in its own *normal direction*, the mesh inflates and deflates like a balloon — a sphere becomes a slightly bigger sphere, a cube grows slightly in all directions at once. This is appropriate for organic "breathing" effects.

If you push every vertex by a scalar amount in world *Y only*, you get the behaviour of lifting and lowering points on a flat surface — appropriate for water and terrain waves. Both are vertex displacement; they differ only in which vector you multiply the scalar by.

**Snippet explanation:**
Step 2 uses `Normal Vector × scalar` — normal-direction. Steps 3 and 4 use `Combine(0, scalar, 0)` — Y-direction only. Switching between them is a single node change. Many complex shaders combine both: waves displace in Y, while foam particles along wave crests use normal displacement.

---

### **Core Idea 4 — A shader property is a per-material value you can write from C# every frame**

`material.SetFloat`, `material.SetVector`, `material.SetColor` write values into the GPU's constant buffer for that material. The shader reads them each frame. If you write a new value from `Update()`, the shader sees the new value on the next rendered frame — essentially a one-way data channel from CPU game logic into GPU rendering math.

**Snippet explanation:**
This is how the mouse ripple works: `Physics.Raycast` finds the cursor's world position on the plane, then `material.SetVector("_MouseWorldPos", hit.point)` writes it into the shader. The shader reads `_MouseWorldPos` as a Vector3 property, computes distances from each vertex to that point, and uses the distances to drive displacement. No intermediate `MonoBehaviour` callbacks, no intermediate `ScriptableObject` — a direct CPU-to-shader value injection each frame.

---

# **Progressive Lab Steps (1 → 2 → 3 → 4)**

> Total budget: **60 minutes**. Save your scene after each step.
> This lab does **not** walk you through every click in Unity's Shader Graph editor — it describes which nodes to place, which settings to apply, and which ports to connect. Getting comfortable navigating the graph editor (right-click → Create Node → search by name) is part of the skill being built.

---

### Step 1 — Create the test scene and understand the Vertex Stage (5 min)

Open the Unity project. Create a new scene (`File > New Scene`) or open `Assets/Scenes/LabS_VertexDisplacement.unity` if provided in the starter.

Create three GameObjects to use as test meshes — you'll assign one shader to each:

1. In the Hierarchy: right-click → `3D Object > Sphere`. Name it `PulsingOrb`. Set scale `(1, 1, 1)`.
2. Right-click → `3D Object > Plane`. Name it `RipplePlane`. Set scale `(2, 1, 2)`. Add Component → `Mesh Collider` (required for Step 3's raycast). Leave all default settings.
3. Right-click → `3D Object > Plane`. Name it `OceanPlane`. Set scale `(3, 1, 3)`. Move it to `(6, 0, 0)` so the three objects don't overlap.

Separate them in world space so they're visible simultaneously in the Scene view.

**Understand the Vertex Stage in Shader Graph:**

In the Project window, right-click in `Assets/Shaders/` (create the folder if needed) → `Create > Shader Graph > URP > Lit Shader Graph`. Name it `TestVertexStage.shadergraph`. Double-click to open the Shader Graph editor.

The editor shows a **Master Stack** on the right side — two blocks labelled `Vertex` and `Fragment`. These are the two shader stages. Click the `Vertex` block to expand it. The three output ports are:

- **Position** (Object space) — where this vertex sits. Default: the mesh's authored position.
- **Normal** (Object space) — the vertex's normal direction. Default: the mesh's authored normal.
- **Tangent** (Object space) — for normal map calculations. You won't use this today.

Any node you connect to `Position` **moves the vertex**. Close this test graph without saving — it was just an orientation exercise.

**Checkpoint:** You can locate the Master Stack in Shader Graph and identify the Vertex/Fragment contexts and the Position output port.

---

### Step 2 — Time Pulse: displacement along normals (12 min)

This shader makes a mesh inflate and deflate rhythmically along its surface normals using a sine wave driven by time. You'll apply it to the `PulsingOrb` sphere.

**2.1 — Create the shader.**

In `Assets/Shaders/`, right-click → `Create > Shader Graph > URP > Lit Shader Graph`. Name it `TimeDisplace.shadergraph`. Double-click to open it.

**2.2 — Add a property.**

Open the **Blackboard** (top-left panel in the Shader Graph editor — click the expand icon if it's collapsed). Click `+` → `Float`. Configure the new property:

- **Name:** `_DisplaceAmount`
- **Reference:** `_DisplaceAmount` (auto-populated)
- **Default:** `0.1`

This property will appear as an editable slider in the Material Inspector, letting you tune the effect without reopening the graph.

**2.3 — Build the node graph.**

Right-click in the empty graph area to open the node search. Add the following nodes and make the connections listed. Work left to right — sources on the left, the Vertex Position output on the right.

**Node 1 — Time**
- Add: `Time`
- You will use the **Sine Time** output port (bottom output, already a −1 to +1 oscillating float — no separate Sine node needed).

**Node 2 — Multiply (scalar)**
- Add: `Multiply`
- Connect `Time → Sine Time` → `Multiply.A`
- Drag `_DisplaceAmount` from the Blackboard into the graph → creates a property node
- Connect `_DisplaceAmount` property → `Multiply.B`
- *Output: a float oscillating between −0.1 and +0.1 at the shader's time rate.*

**Node 3 — Normal Vector**
- Add: `Normal Vector`
- In the node's dropdown, set **Space** to `Object`
- *Output: the normalised surface normal at this vertex, in Object space.*

**Node 4 — Multiply (vector scale)**
- Add a second: `Multiply`
- Connect `Normal Vector.Out` → second `Multiply.A`
- Connect first `Multiply.Out` (the scalar) → second `Multiply.B`
- *Output: the normal vector scaled by the displacement amount — a Vector3 pointing outward by at most ±0.1 units.*

**Node 5 — Position**
- Add: `Position`
- Set **Space** to `Object`
- *Output: this vertex's position in Object space — the baseline we'll displace from.*

**Node 6 — Add**
- Add: `Add`
- Connect `Position.Out` → `Add.A`
- Connect second `Multiply.Out` → `Add.B`
- *Output: the displaced position — original position nudged outward (or inward) along the normal.*

**Final connection:**
- Connect `Add.Out` → `Vertex` context → **Position** port in the Master Stack.

Click **Save Asset** (Ctrl+S in the graph editor).

**2.4 — Apply and observe.**

In the Project window, right-click `Assets/Materials/` → `Create > Material`. Name it `TimeDisplace.mat`. In the Inspector, set the **Shader** dropdown to `Shader Graphs/TimeDisplace`.

Drag `TimeDisplace.mat` onto `PulsingOrb` in the Hierarchy.

Press **Play**. The sphere breathes — swelling outward and contracting inward once per second. The normals along the surface all point outward, so each vertex moves radially; the sphere inflates and deflates symmetrically.

**Tinker (in the Inspector during Play):**

- Raise `_DisplaceAmount` to `0.5` — the sphere deforms dramatically, vertices visibly separating at the equator. This is the mesh's low vertex count becoming apparent — vertex displacement only looks smooth when there are enough vertices to represent the wave.
- Lower to `0.02` — barely perceptible surface shimmer.
- Try applying the same material to a subdivided sphere from a 3D package — the breathing looks organic at higher poly count.

**Checkpoint:** The sphere inflates and deflates in Play mode. The `_DisplaceAmount` slider in the Material Inspector changes the intensity without stopping Play.

---

### Step 3 — Mouse Ripple: cursor-driven displacement on a plane (18 min)

This shader pushes vertices upward on a flat plane based on proximity to the cursor's world position. Where the cursor hovers, vertices rise; farther away, the effect falls off smoothly. A companion C# script raycasts the cursor and passes the hit point to the shader as a Vector3 property each frame.

**3.1 — Create the shader.**

In `Assets/Shaders/`, create a new `URP > Lit Shader Graph`. Name it `MouseRipple.shadergraph`. Open it.

**3.2 — Add three properties to the Blackboard.**

| Name | Reference | Type | Default |
| :-- | :-- | :-- | :-- |
| `Mouse World Position` | `_MouseWorldPos` | Vector3 | (0, 0, 0) |
| `Ripple Radius` | `_RippleRadius` | Float | 4.0 |
| `Ripple Height` | `_RippleHeight` | Float | 0.5 |

**3.3 — Build the node graph.**

This graph works in **World space** — the mouse position is a world coordinate, and the plane's vertices have world-space positions. At the end you'll convert back to Object space using a `Transform` node.

**Node 1 — Position (vertex world position)**
- Add: `Position`
- Set **Space** to `World`
- *Output: this vertex's current world-space position.*

**Node 2 — _MouseWorldPos property**
- Drag `_MouseWorldPos` from the Blackboard into the graph.

**Node 3 — Subtract (offset vector)**
- Add: `Subtract`
- Connect `Position.Out` → `Subtract.A`
- Connect `_MouseWorldPos` → `Subtract.B`
- *Output: a Vector3 pointing from the mouse position to this vertex — the offset between them.*

**Node 4 — Split (extract X and Z)**
- Add: `Split`
- Connect `Subtract.Out` → `Split.In`
- *This node outputs R, G, B, A — which correspond to X, Y, Z, W of the input vector.*

**Node 5 — Combine (2D horizontal offset)**
- Add: `Combine`
- Connect `Split.R` (= X component) → `Combine.R`
- Connect `Split.B` (= Z component) → `Combine.G`
- Leave `Combine.B` and `Combine.A` unconnected (they default to 0)
- Set the output type to `Vector2` using the Combine node's output port dropdown
- *Output: a Vector2 containing only the XZ offset — height difference between vertex and mouse is ignored.*

**Node 6 — Length (horizontal distance)**
- Add: `Length`
- Connect `Combine.Out` → `Length.In`
- *Output: a single float — how far this vertex is from the mouse cursor in the XZ plane.*

**Node 7 — Smoothstep (smooth falloff)**
- Add: `Smoothstep`
- Connect `Length.Out` → `Smoothstep.In`
- Connect `_RippleRadius` property → `Smoothstep.Edge2`
- Leave `Smoothstep.Edge1` at its default `0`
- *Output: 0.0 when the vertex is at the cursor (distance = 0), 1.0 when at the radius edge — a smooth ramp outward.*

**Node 8 — One Minus (invert falloff)**
- Add: `One Minus`
- Connect `Smoothstep.Out` → `One Minus.In`
- *Output: 1.0 at the cursor, 0.0 at the radius edge — the falloff now peaks at the mouse position.*

**Node 9 — Multiply (height displacement)**
- Add: `Multiply`
- Connect `One Minus.Out` → `Multiply.A`
- Connect `_RippleHeight` property → `Multiply.B`
- *Output: a scalar Y displacement — highest at the cursor, zero at the edge.*

**Node 10 — Combine (world-space Y-only displacement)**
- Add a second: `Combine`
- Leave `Combine.R` (X) unconnected — defaults to 0
- Connect `Multiply.Out` → `Combine.G` (Y)
- Leave `Combine.B` (Z) unconnected — defaults to 0
- Set output type to `Vector3`
- *Output: Vector3(0, height, 0) — a displacement vector that only moves vertices upward.*

**Node 11 — Add (apply displacement)**
- Add: `Add`
- Connect `Position.Out` (from Node 1 — the world position) → `Add.A`
- Connect second `Combine.Out` (the displacement) → `Add.B`
- *Output: the new world-space position with the ripple applied.*

**Node 12 — Transform (convert back to Object space)**
- Add: `Transform`
- Set **Input Space** to `World`
- Set **Output Space** to `Object`
- Set **Type** to `Position`
- Connect `Add.Out` → `Transform.In`
- *Output: the displaced position expressed in Object space — what the Vertex Position port expects.*

**Final connection:**
- Connect `Transform.Out` → `Vertex` context → **Position** port in the Master Stack.

Save the asset.

**3.4 — Create the companion script.**

The shader's `_MouseWorldPos` property is currently (0, 0, 0). Without this script, the ripple just creates a permanent bump at the world origin. The script raycasts the cursor each frame and writes the hit point into the material.

In `Assets/Scripts/`, create a new MonoBehaviour script called `MouseRippleController.cs`. Replace its contents with:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycasts the mouse cursor against the scene each frame and passes the
/// world-space hit point to a shader property (_MouseWorldPos) on this
/// GameObject's material. The shader uses the position to drive vertex
/// displacement centred on the cursor.
///
/// REQUIRES: a Collider on this GameObject (MeshCollider for a Plane),
///           and a material using the MouseRipple shader.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class MouseRippleController : MonoBehaviour
{
    private static readonly int MouseWorldPosID = Shader.PropertyToID("_MouseWorldPos");

    // .material creates a per-instance copy so we don't modify the shared asset on disk.
    private Material materialInstance;

    private void Start()
    {
        materialInstance = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (Mouse.current == null || Camera.main == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Hit the plane — pass the exact world-space contact point.
            materialInstance.SetVector(MouseWorldPosID, hit.point);
        }
        else
        {
            // Mouse is off the mesh — project the ray onto the y = 0 plane as a fallback
            // so the ripple doesn't snap to (0,0,0) when the cursor leaves the mesh edge.
            if (Mathf.Abs(ray.direction.y) > 0.001f)
            {
                float t = -ray.origin.y / ray.direction.y;
                if (t > 0f)
                {
                    Vector3 worldPos = ray.origin + ray.direction * t;
                    materialInstance.SetVector(MouseWorldPosID, worldPos);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Destroy the material instance we created — otherwise it leaks into the Editor.
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
```

**3.5 — Apply and wire.**

1. Create a new Material `Assets/Materials/MouseRipple.mat`. Set its Shader to `Shader Graphs/MouseRipple`.
2. Drag `MouseRipple.mat` onto `RipplePlane` in the Hierarchy.
3. Select `RipplePlane`. Confirm it has both a **Mesh Renderer** and a **Mesh Collider** component. If the Mesh Collider is missing, Add Component → Mesh Collider.
4. Add the `MouseRippleController` script to `RipplePlane` (Add Component → search `MouseRippleController`).

**3.6 — Test.**

Press Play. Hover the cursor over the `RipplePlane` in the Game view. A raised bump follows the cursor across the surface. Moving the cursor quickly sweeps the bump; holding still keeps it centred in one position.

Adjust in the Material Inspector (still in Play mode):
- **Ripple Radius** `2` vs `8` — small focused bump vs wide gentle hill.
- **Ripple Height** `0.05` vs `1.5` — subtle shimmer vs dramatic spike.

Notice that the ripple effect is only as smooth as the plane's vertex density. Unity's default `Plane` primitive has a 10×10 vertex grid — the bump has visible faceting. This is expected; the same shader on a 100×100 grid would appear smooth.

**Checkpoint:** The bump follows the cursor in Play mode. Moving the mouse off the edge of the plane gracefully holds the last valid position (the fallback projection in the script). The Material Inspector sliders change the ripple shape in real time.

---

### Step 4 — Ocean Wave: time-driven spatial sine wave (20 min)

This shader creates a continuous wave sweeping across a flat plane by evaluating a sine function over both world-space X position and time. Vertices farther along the wave's direction are at different phases of the cycle — this is the key insight that produces travelling waves rather than uniform up-and-down bobbing. A height-based colour gradient in the Fragment stage makes peaks appear lighter and troughs darker.

**4.1 — Create the shader.**

In `Assets/Shaders/`, create a new `URP > Lit Shader Graph`. Name it `OceanWave.shadergraph`. Open it.

**4.2 — Add properties.**

| Name | Reference | Type | Default |
| :-- | :-- | :-- | :-- |
| `Wave Amplitude` | `_WaveAmplitude` | Float | 0.4 |
| `Wave Frequency` | `_WaveFrequency` | Float | 1.5 |
| `Wave Speed` | `_WaveSpeed` | Float | 2.0 |
| `Shallow Colour` | `_ShallowColour` | Color | (0.35, 0.65, 0.75, 1) — light teal |
| `Deep Colour` | `_DeepColour` | Color | (0.05, 0.15, 0.35, 1) — deep navy |

**4.3 — Build the Vertex stage node graph.**

**Node 1 — Position (vertex world position)**
- Add: `Position`, Space: `World`

**Node 2 — Split (extract X component)**
- Add: `Split`
- Connect `Position.Out` → `Split.In`
- You will use the `Split.R` output (= X component of world position)
- *X gives each vertex a different starting phase along the wave direction.*

**Node 3 — _WaveFrequency property**
- Drag from Blackboard.

**Node 4 — Multiply (spatial frequency)**
- Add: `Multiply`
- Connect `Split.R` → `Multiply.A`
- Connect `_WaveFrequency` → `Multiply.B`
- *Output: higher frequency = more wave crests per world unit.*

**Node 5 — Time**
- Add: `Time`
- Use the `Time` output (the top port — raw elapsed seconds, not pre-wrapped into Sine).

**Node 6 — _WaveSpeed property**
- Drag from Blackboard.

**Node 7 — Multiply (temporal phase)**
- Add: `Multiply`
- Connect `Time.Time` → `Multiply.A`
- Connect `_WaveSpeed` → `Multiply.B`
- *Output: time advancing scaled by wave speed — this shifts the wave pattern forward over time.*

**Node 8 — Add (full wave phase)**
- Add: `Add`
- Connect Node 4 `Multiply.Out` (spatial) → `Add.A`
- Connect Node 7 `Multiply.Out` (temporal) → `Add.B`
- *Output: `x * frequency + time * speed` — the complete input to the sine function.*

**Node 9 — Sine**
- Add: `Sine`
- Connect `Add.Out` → `Sine.In`
- *Output: wave value oscillating between −1 and +1. Each vertex samples a different point on the sine curve because each has a different X position.*

**Node 10 — _WaveAmplitude property**
- Drag from Blackboard.

**Node 11 — Multiply (scale to amplitude)**
- Add: `Multiply`
- Connect `Sine.Out` → `Multiply.A`
- Connect `_WaveAmplitude` → `Multiply.B`
- *Output: Y displacement ranging from −amplitude to +amplitude.*

**Node 12 — Combine (Y-only displacement vector)**
- Add: `Combine`, output type `Vector3`
- Leave `Combine.R` (X) unconnected — defaults to 0
- Connect `Multiply.Out` → `Combine.G` (Y)
- Leave `Combine.B` (Z) unconnected — defaults to 0
- *Output: Vector3(0, waveY, 0).*

**Node 13 — Add (apply to world position)**
- Add: `Add`
- Connect `Position.Out` (Node 1's world position) → `Add.A`
- Connect `Combine.Out` (the wave offset) → `Add.B`
- *Output: new world-space position with wave height applied.*

**Node 14 — Transform (World → Object)**
- Add: `Transform`
- Input Space: `World`, Output Space: `Object`, Type: `Position`
- Connect `Add.Out` → `Transform.In`

**Final Vertex connection:**
- Connect `Transform.Out` → `Vertex` context → **Position** port.

**4.4 — Add a height-based colour gradient in the Fragment stage.**

The Fragment stage runs after vertices are displaced — the `Position` node in the Fragment stage reads the *interpolated* world position of each screen fragment, which already reflects the vertex displacement. This lets you drive colour from wave height.

In the **Fragment context** of the Master Stack, you'll wire to the `Base Color` port.

**Fragment Node 1 — Position (fragment world position)**
- Add: `Position`, Space: `World`
- *This is the world position of this fragment — Y is the wave height at this point.*

**Fragment Node 2 — Split**
- Add: `Split`
- Connect `Position.Out` → `Split.In`
- Use `Split.G` (= Y component — the wave height).

**Fragment Node 3 — _WaveAmplitude property**
- Drag from Blackboard.

**Fragment Node 4 — Divide (normalise height to 0–1)**
- Add: `Divide`
- Connect `Split.G` → `Divide.A`
- Connect `_WaveAmplitude` → `Divide.B`
- *Output: height / amplitude — maps the ±amplitude range toward ±1.*

**Fragment Node 5 — Remap (map −1/+1 to 0/1)**
- Add: `Remap`
- Connect `Divide.Out` → `Remap.In`
- Set `Remap.In Min` to `−1`, `Remap.In Max` to `1`
- Set `Remap.Out Min` to `0`, `Remap.Out Max` to `1`
- *Output: 0 at trough, 0.5 at rest, 1 at peak — a clean 0–1 range for colour interpolation.*

**Fragment Node 6 — Saturate (clamp)**
- Add: `Saturate`
- Connect `Remap.Out` → `Saturate.In`
- *Prevents values outside 0–1 if amplitude tuning pushes beyond the remap range.*

**Fragment Node 7 — _DeepColour and _ShallowColour properties**
- Drag both from the Blackboard.

**Fragment Node 8 — Lerp (colour by height)**
- Add: `Lerp`
- Connect `_DeepColour` → `Lerp.A`
- Connect `_ShallowColour` → `Lerp.B`
- Connect `Saturate.Out` → `Lerp.T`
- *Output: deep colour at 0 (trough), shallow colour at 1 (peak).*

**Final Fragment connection:**
- Connect `Lerp.Out` → `Fragment` context → **Base Color** port.

Save the asset.

**4.5 — Apply and observe.**

Create `Assets/Materials/OceanWave.mat`. Set its Shader to `Shader Graphs/OceanWave`. Drag it onto `OceanPlane`.

Press Play. The plane should show blue-toned waves travelling in one direction (along the world X axis), with peaks shaded lighter and troughs darker.

Adjust in Play mode:
- **Wave Amplitude** `0.1` → `1.0` — from gentle swell to stormy sea.
- **Wave Frequency** `0.5` → `4.0` — from long gentle swells to choppy chop.
- **Wave Speed** `0.5` → `6.0` — from calm tide to fast-moving current.
- **Shallow Colour** and **Deep Colour** — try white/dark-teal for Arctic, green/deep-brown for swamp.

**Checkpoint:** Waves travel across the plane. Peaks are lighter, troughs darker. The three property sliders each control a distinct visual characteristic independently.

---

### Step 5 — Reflect (5 min)

With all three shaders running simultaneously, observe what they have in common and where they diverge:

- All three displace vertices — none change fragment colour as their primary effect (though Step 4 uses colour as a *secondary* effect layered on top).
- Steps 2 uses Object space throughout (no Transform node). Steps 3 and 4 use World space and require the Transform node at the end — without it, moving the plane in the scene would break the displacement entirely.
- Step 2 reads only `Time`. Steps 3 and 4 each read one additional input that comes from *outside the graph*: Step 3 from a C# script via `SetVector`, Step 4 from nothing — it's entirely self-contained in time and position.
- The pattern `Position → [compute displacement] → Add(Position, displacement) → Transform → Vertex Position` is identical across Steps 3 and 4. It's the standard vertex displacement template you'll reuse in any future shader.

Stop Play. Save the scene.

**Checkpoint:** All three shaders are running. You can describe the structure of the vertex displacement pattern and explain why the `Transform` node appears in Steps 3 and 4 but not Step 2.

---

# **Tinker Tasks**

> Pick at least four. These produce visible results quickly and build intuition faster than reading.

| Try this | Notice |
| :-- | :-- |
| On `TimeDisplace.shadergraph`, add a second `Multiply` node to scale by `Cosine Time` instead of `Sine Time` for alternate pulses | The orb still breathes but the timing phase shifts — combine Sine Time and Cosine Time on two separate axes for an elliptical wobble |
| On `TimeDisplace.shadergraph`, replace the `Normal Vector` node with a `Combine(0, 1, 0)` — a fixed upward Vector3 | The sphere no longer inflates uniformly — it bobs up and down instead of breathing. Confirms the normal direction is what makes inflation look radial |
| On `MouseRipple.shadergraph`, change `Smoothstep` Edge1 from `0` to `1.5` | A flat zone of maximum displacement appears at the cursor centre — the smoothstep's lower plateau becomes visible. Good for a flat-topped pressure wave |
| On `MouseRipple.shadergraph`, multiply the displacement by `Sine(Time × speed)` before combining | The ripple pulses in and out at the cursor position — combines spatial and temporal displacement in the same graph |
| On `OceanWave.shadergraph`, add a second wave using `Split.B` (Z world position) instead of `Split.R` (X), with different frequency and amplitude, then `Add` both displacements before the final `Combine` | Waves travel in two directions simultaneously — the intersection of the two sine waves creates interference patterns that look far more realistic than a single wave |
| On `OceanWave.shadergraph`, multiply wave amplitude by a `Gradient Noise` node driven by world XZ position | Wave height varies spatially — some areas are calm, others choppy. Approximates the uneven texture of real ocean |
| On `OceanWave.shadergraph`, also output a modified `Normal` in the Vertex stage: compute the wave's derivative analytically (`WaveFrequency × WaveAmplitude × Cosine(phase)`) and build a new normal from it | Lighting reacts correctly to the wave surface — without this, the original flat normals cause incorrect highlights on the displaced geometry |
| Duplicate `RipplePlane` and apply `OceanWave.mat` to it — then place both planes at the same position | The two displacement effects stack — the ocean wave creates the base undulation and the cursor ripple adds a second, interactive layer on top. No shader modification needed; this is material-level composition |

---

# **Useful Editor Tricks**

| Trick | Why it helps |
| :-- | :-- |
| Right-click any node → `Open Documentation` | Opens Unity's official node reference with the exact mathematical formula the node implements — useful when `Smoothstep` or `Remap` behaves unexpectedly |
| In the Shader Graph editor, hold `Alt` and drag to pan; scroll wheel to zoom | Navigation shortcuts — the default graph area is larger than the viewport |
| Right-click an edge (connection line) → `Delete` | Removes a single connection without deleting either node |
| Select multiple nodes → `Ctrl + G` | Groups them — right-click the group to name it (e.g. "Wave Phase Calc"). Groups don't affect shader output but make large graphs readable |
| Press `Space` in the graph editor | Opens the node search at the mouse position — faster than right-clicking |
| `Shader Graph > Graph Inspector` → **Preview** tab | Shows a live sphere preview of the shader as you build it — vertex displacement is visible in the preview in real time |
| With `_MouseWorldPos` selected in the Blackboard, tick **Exposed** | The property appears in the Material Inspector — useful for manually testing values without the script running |
| On any node → right-click → `Preview` | Shows a per-node output preview inline — invaluable for debugging which node is producing unexpected values |

---

# **Debugging & Pitfalls**

| Mistake | Why it happens | Fix |
| :-- | :-- | :-- |
| No visible displacement — geometry unchanged | Nodes are wired to the Fragment context (`Base Color` port) instead of the Vertex context (`Position` port) | Disconnect from Base Color; connect the final `Add.Out` to the **Vertex** block's **Position** port in the Master Stack |
| Geometry explodes / stretches when the GameObject moves | Missing `Transform` (World → Object) node — output is world-space position fed to an Object-space port | Add a `Transform` node: Input Space: World, Output Space: Object, Type: Position; connect it before the Vertex Position port |
| Displacement appears correct at origin but wrong everywhere else | Same as above — Object space ≠ World space when the object is not at (0,0,0) | Same fix: `Transform` node before output |
| Mouse ripple does nothing | `_MouseWorldPos` property is never written — script not attached, or tag/layer blocked the raycast | Confirm `MouseRippleController` is on `RipplePlane`; confirm the Plane has a Mesh Collider; check Console for null errors |
| Mouse ripple snaps to (0,0,0) when cursor leaves the plane | `OnDestroy` of the material instance clears the value | Already handled by the script's y=0 plane fallback — if you see this, confirm the fallback branch in `Update` isn't bypassed |
| Wave doesn't move — vertices displace but stay static | Connecting to `Sine Time` (a pre-wrapped value) instead of building the `Time → Multiply → Add → Sine` chain — `Sine Time` has speed locked to Unity's internal timer frequency | Use the raw `Time` output and the full `Multiply → Add → Sine` chain as described in Step 4 |
| Wave looks correct but lighting is wrong (no highlights on crests) | Vertex normals are not updated to match the displaced geometry — the Vertex Normal output still reads the flat mesh normals | In the Vertex stage, also output a displaced normal: the wave's analytical derivative is `WaveFrequency × WaveAmplitude × Cos(phase)` — build a Normalize(Cross(tangent, displaced_tangent)) for the normal. Alternatively, enable `Smooth Normals` on the mesh import if available |
| Shader Graph won't compile — errors in Console | A property Reference name (`_MouseWorldPos`) doesn't match the string passed to `SetVector` in C# | Exact string match required — case-sensitive. Open the Blackboard, check the **Reference** field (not the display Name) |
| `Mouse.current` is null at runtime | Input System package not installed, or the Input System action asset was deleted | Window → Package Manager → confirm `Input System` is installed. Alternatively replace `Mouse.current.position.ReadValue()` with `(Vector2)Input.mousePosition` if using Legacy Input |

---

# **Reflective Questions**

- The `Transform` (World → Object) node appears in Steps 3 and 4 but not Step 2. Explain in your own words *why* it is necessary in those two steps and *why* it isn't needed in Step 2.
- Step 3 passes a world-space position from C# to the shader every frame using `material.SetVector`. The `EmissivePulse.cs` script from Lab 5 passes a colour using `material.SetColor`. What do these two communication patterns have in common? What does this suggest about how Unity's CPU-to-GPU data channel works generally?
- The ocean wave uses `Position.X` as the spatial input to the sine function. What would happen if you replaced `Position.X` with `Position.Z`? What if you used `(Position.X + Position.Z) × 0.707` (a 45° diagonal)? What real ocean phenomenon does using multiple spatial directions together approximate?
- Vertex displacement only looks smooth when there are enough vertices in the mesh. The default Unity Plane has 121 vertices (11×11 grid). How does this constraint affect your approach to real-time ocean water in a production game? Research the term "vertex density" in the context of GPU tessellation.
- The `_WaveAmplitude` property is used in both the Vertex stage (to scale displacement) and the Fragment stage (to normalise height for colour). Is reusing the same property across stages good design, or would separating them give more control? Argue both sides.
- You can produce similar visual results by animating a Mesh from C# each frame (`Mesh.vertices = ...`). What are the performance trade-offs between doing vertex displacement on the CPU (C# Mesh API) vs the GPU (Shader Graph)? When might the CPU approach be preferable?

---

# **Software Development Parallel**
The ShaderGraph node network you built today is a **dataflow graph** — data flows from sources (Time, Position, Properties) through transformations (Multiply, Add, Sine) to outputs (Vertex Position, Base Color). No loops, no mutable state, no side effects. This is the same model used by **functional reactive programming** (FRP) frameworks like RxJS, Elm, or Haskell's FRP libraries. It's also the same model as audio DSP graphs (Max/MSP, Pure Data), visual effects graphs (Houdini, Nuke), and neural network computation graphs (TensorFlow, PyTorch). Recognising the pattern across domains matters: once you've built intuition for dataflow in ShaderGraph, you've built intuition for a programming model that appears in sound, film, AI, and systems programming — all at once. The node is just a function. The wire is just a value being passed.

---

# **Stretch Tasks (optional, take-home)**

**Stretch 1: Gerstner Waves.**
Real ocean surfaces use **Gerstner waves** instead of pure sine waves. In a Gerstner wave, vertices move in a circular orbit — displaced in both Y and in the wave's travel direction (XZ) — which produces the characteristic peaked crests and flat troughs of ocean waves. Implement a Gerstner wave in your `OceanWave.shadergraph`: the X displacement is `−Amplitude × Sin(phase)` and the Z (or X, depending on travel direction) displacement is `Amplitude × Cos(phase)`. Compare the result to the sine-only version.

**Stretch 2: Ripple decay over time.**
The current mouse ripple is always at full height while the cursor is over the plane. Modify `MouseRippleController.cs` to record a timestamp each time the cursor moves significantly, then have the shader fade the ripple out over ~1 second using `material.SetFloat("_RippleAge", age)`. In the graph, multiply the final displacement by `1 − Saturate(_RippleAge / _FadeDuration)`. The ripple now appears at full height when the cursor arrives and decays as it sits still.

**Stretch 3: Multiple ripple centres.**
A single `_MouseWorldPos` produces one bump. Extend the shader to support three independent mouse positions using three Vector3 properties (`_Pos1`, `_Pos2`, `_Pos3`). Add all three falloff-weighted displacements before the final `Combine`. In the C# script, cycle which `_Pos` gets the current cursor position each frame on a timer, so the three bumps decay independently at different phases.

---

## Files produced by end of lab
- `Assets/Shaders/TimeDisplace.shadergraph`
- `Assets/Shaders/MouseRipple.shadergraph`
- `Assets/Shaders/OceanWave.shadergraph`
- `Assets/Materials/TimeDisplace.mat`
- `Assets/Materials/MouseRipple.mat`
- `Assets/Materials/OceanWave.mat`
- `Assets/Scripts/MouseRippleController.cs`
- `Assets/Scenes/LabS_VertexDisplacement.unity`

---

## Lesson Context

```yaml
previous_lesson:
  topic_code: t05_decorators_materials_particles
  domain_emphasis: Games

this_lesson:
  topic_code: ts_vertex_displacement_shadergraph
  primary_domain_emphasis: Games
  difficulty_tier: Advanced
  feeds_into: null
  status: Supplementary — outside the assessed 3-hour series. May be completed before, between, or after the numbered labs.
```
