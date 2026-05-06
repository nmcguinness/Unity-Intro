---
title: "PBR Materials — Building a Sci-Fi Crate"
subtitle: "Unity Animation Mini-Series — Supplementary Lab"
topic_code: ts_pbr_materials
description: "A 45-minute supplementary lab walking students through Unity URP's six core material properties — Base Colour, Normal, Metallic, Smoothness, Emission (HDR + Bloom), and Opacity — by progressively dressing a single sci-fi crate."
created: 2026-05-05
last_updated: 2026-05-05
version: 1.0
status: published
authors: ["Games Development Teaching Team"]
tags: [unity, unity-6.3-lts, urp, pbr, materials, normal-map, emission, transparency, year1, supplementary-lab]
difficulty_tier: Foundational
unity_version: "6.4 LTS"
project_template: "3D (URP) Core"
duration_minutes: 45
previous_topic: t05_decorators_materials_particles
prerequisites:
  - Recommended (not required) — Labs 1–5 completed, especially Lab 5's introduction to emissive materials
  - Comfortable creating Materials in the Project window and assigning them to GameObjects
  - Lab S uses content from `REPO_LINK/LabS_Starter/` (sci-fi crate model + curated PBR texture set pre-imported, URP Bloom enabled in the scene's Volume)
---

# PBR Materials — Building a Sci-Fi Crate
> **Prerequisites:**
> - You can create a Material asset, assign it to a GameObject, and edit its properties in the Inspector.
> - You have cloned the labs repository (`REPO_LINK`). Open `REPO_LINK/LabS_Starter/` as a Unity project — sci-fi crate model and PBR texture sets pre-imported, URP Bloom enabled in the scene's Volume.
> - This lab sits **outside the assessed 3-hour series**. You may complete it before, between, or after the numbered labs — it has no narrative dependency on any of them.

---

## **What you'll learn**

| Skill Type | You will be able to… |
| :-- | :-- |
| **Conceptual Understanding** | Explain the role of each of the six core PBR material properties and why modern game art separates them into distinct texture maps. |
| **Editor & Tool Fluency** | Create URP Lit materials, plug texture maps into the correct slots, switch surface types between Opaque and Transparent, and use HDR colour intensity for emission. |
| **Design Skills** | Choose appropriate property values to communicate material identity (metal vs plastic, polished vs scuffed, glowing vs inert, opaque vs translucent). |
| **Problem-Solving** | Diagnose the four classic material bugs: pink shaders, flat-looking surfaces, wrong-colour metals, and invisible transparency. |

---

## **Why this matters**
Animation makes a game move. **Materials make it look like something.** A sphere with a flat red colour reads as a primitive; the same sphere with the right combination of base colour, normal map, metallic value, smoothness, and emission reads as a battered chrome power core under flickering corridor lights. The geometry is identical in both cases — the difference is entirely material.

Modern real-time rendering — in Unity, Unreal, Godot, and every major engine — uses a system called **Physically Based Rendering (PBR)**. PBR breaks "what a surface looks like" into a small set of independent properties, each authored separately, then combined at render time according to physics-based lighting equations. The system was developed across multiple studios in the 2010s and standardised across engines around 2014–2015. Once you understand the properties, you can build a believable material for almost any surface you encounter — wood, metal, plastic, glass, fabric, skin, anything.

This lab takes you from a blank material to a full sci-fi crate with all six properties working together. By the end you'll have built a complete PBR material from scratch, encountered each property's pitfalls, and understood why production game art is structured the way it is.

---

## **How this builds on previous content**
**From Lab 5 you've met:**
- The URP Lit shader.
- The Emission property (briefly — you ticked it on and tinted it).
- Bloom as a post-processing effect that amplifies bright pixels.

**This lab goes deeper:**
- The same Lit shader exposes **six** properties you should master, not just one.
- Emission has more depth than Lab 5 covered — HDR colour intensity, Bloom interaction, and how to use it for non-glowing accents.
- A new property — **Opacity** — opens up glass, holograms, and fade-out effects.
- A new property — **Normal Map** — adds 3D-looking surface detail without changing geometry.

**This lab feeds into nothing further in the numbered series**, but the skills carry directly into:
- Year 2 Materials & Shaders module (where you'll write your own shaders in HLSL — the same module the lab format you're reading was first developed in).
- Year 2 Lighting module (where surface properties interact with light models).
- Any 3D project you build for the rest of your degree.

---

# **Core Ideas / Concepts**

> Each property is introduced briefly here. Each gets a full step in the lab below.

---

### **Core Idea 1 — PBR is the "art protocol" of modern rendering**

Every modern engine agrees on the same six-ish material properties. Author them once in Substance Painter, Blender, or Photoshop, and the same textures work in Unity, Unreal, and Godot. The agreement is informal but extremely strong — there's no PBR standards body, but if you produce a "PBR-ready" texture pack, every modern engine will accept it.

**Snippet explanation:**
This is why you see "PBR-ready" labelled on every asset store pack. The protocol is the contract; the renderer is the implementation. Today you become fluent in the contract — meaning you can pick up textures from any source (Quixel Megascans, ambientCG, Substance Source, your own bake) and plug them into Unity correctly without reading any documentation specific to where they came from.

---

### **Core Idea 2 — Each property is *independent***

Base Colour doesn't know whether a surface is metal. Smoothness doesn't know what colour the surface is. Each map is authored separately and combined at render time according to lighting physics.

**Snippet explanation:**
This independence is what makes PBR so powerful. You can swap a brass crate for a steel crate by changing *one* texture (Base Colour) without re-authoring anything else. You can age a crate by changing *one* texture (Smoothness — scuffs reduce reflectivity) without altering its colour. Production teams use this to ship hundreds of asset variants from a single template — a city's worth of buildings might share normal maps and roughness maps, with only base colour varying per building.

---

### **Core Idea 3 — "Roughness" and "Smoothness" are the same idea, inverted**

Unreal Engine and Blender call this property **Roughness** (0 = mirror, 1 = matte). Unity URP calls it **Smoothness** (0 = matte, 1 = mirror). Same physics, opposite slider.

**Snippet explanation:**
When you read tutorials online, mentally invert the value if the tutorial is from a different engine. This trips up *everyone* at first. You'll see this in action in Step 4. The reason for the difference is largely historical — early Unity used Smoothness in the legacy Standard shader, and changing the convention now would break every existing project. So the inconsistency persists.

---

### **Core Idea 4 — Emission isn't lighting — it's "ignore lighting"**

A material with emission glows the same colour regardless of how dark the scene is. Emission tells the renderer "this pixel always reads as this colour and intensity, ignoring shadows and lights cast on the surface."

**Snippet explanation:**
That's why emission combined with **Bloom** is the standard sci-fi look — a screen, an LED, or a power core looks bright even in shadow, then Bloom bleeds the brightness outwards into a halo. You'll do this in Step 5 with HDR intensity values, going beyond Lab 5's introduction. The technique works equally well for non-sci-fi cases: stained glass windows, glowing eyes, lava, candle flames, neon signs, hot metal.

---

### **Core Idea 5 — Transparency requires a surface-type switch**

The Lit shader's Alpha slider does *nothing* on an Opaque material. To make a material see-through, you must first change its `Surface Type` from Opaque to Transparent. This is the #1 beginner trap with opacity.

**Snippet explanation:**
This separation exists because rendering transparent surfaces is significantly more expensive than opaque ones — the engine handles them in a separate pass with depth sorting and alpha blending. Forcing you to opt in keeps performance predictable. If alpha "just worked" on every material, every project would accidentally render expensive transparency on materials that didn't need it. The two-step opt-in (change surface type, *then* adjust alpha) is friction by design.

---

# **Progressive Lab Steps (1 → 2 → 3 → 4 → 5 → 6)**

> Total budget: **45 minutes**.
> The starter scene contains: a sci-fi crate (`Crate.fbx`) on a neutral grey floor, a directional light at a flattering angle, a small set of PBR texture maps in `Assets/Textures/Crate/`, and a URP post-processing Volume with Bloom enabled (intensity 1.0, threshold 1.0).
> **You will build one Material asset progressively across Steps 1–5**, then a *second* material for the glass viewport in Step 6. Save your scene between steps and observe the visual change after each — the cumulative payoff is the lesson.

---

### Step 1 — Base Colour (5 min)

Open the starter project at `REPO_LINK/LabS_Starter/` in Unity 6.3 LTS. The scene `Assets/Scenes/MaterialsLab.unity` should open automatically; if not, open it manually. The crate sits on the floor in front of the camera, currently showing the default grey material.

In the Project window, navigate to `Assets/Materials/`. Right-click → `Create > Material`. Name the new asset `SciFiCrate.mat`.

Confirm in the Inspector that `Shader` is `Universal Render Pipeline/Lit` (the default for new materials in a URP project — you shouldn't need to change it). The Inspector now shows several sections: **Surface Options**, **Surface Inputs**, **Detail Inputs**, **Advanced Options**.

In the **Surface Inputs** section, click the small circle next to `Base Map`. The texture browser appears. Select `Crate_BaseColor.png` from the texture browser. Click out of the picker.

Drag `SciFiCrate.mat` from the Project window onto the crate in the Scene view. The crate's surface immediately changes to show the painted-on colours of the texture — panel lines, scuff marks, decals.

**Observe:** the crate now has visible colour detail — but the surface is still completely flat and matte. Looking at it from different angles produces no highlights, no reflections, nothing changing. The surface reads more like a 2D printout pasted onto a box than a 3D object.

**Checkpoint:** Crate has visible colour detail. Move around in Scene view to confirm — the surface looks identically flat from every angle.

---

### Step 2 — Normal Map (7 min)

Normal maps are the single most magical PBR property. They store *fake surface direction* per pixel — telling the renderer "this pixel faces up-and-left even though the geometry is flat" — and the result is dramatic, geometry-free surface detail.

In the same Material's Inspector, locate the `Normal Map` slot.

Click the small circle next to it, select `Crate_Normal.png`.

**Important:** Unity may show a yellow warning box that reads *"This texture is not marked as a normal map."* with a `Fix Now` button. Click `Fix Now`. This tells Unity to interpret the image's RGB channels as surface direction vectors (XYZ encoded into RGB), not as colour. If you don't click Fix Now, the texture is treated as colour data and produces wrong results — the bumps point in nonsense directions.

**Observe:** the crate's panel seams now catch light. Bolts cast tiny shadows. The surface looks *raised* even though the geometry is unchanged. Move the camera around — the highlights move with you, exactly as they would on a real bumpy surface.

**Tinker:** drag the **Normal Map intensity slider** (just to the right of the texture slot) up to `2` and down to `0`. Notice how the *same* texture produces dramatically different surface depth. At `0` the surface is flat again; at `2` the bumps look exaggerated. Most production materials use values between `0.5` and `1.5` — pushing higher tends to look fake, like a relief print rather than a 3D surface.

**Checkpoint:** Crate looks 3D — surface details respond to your scene's directional light. Walk around it in Scene view to confirm the highlights track properly.

---

### Step 3 — Metallic (5 min)

Metallic tells the renderer whether the surface is conductive (metal) or dielectric (everything else — plastic, wood, fabric, skin). This binary distinction has profound consequences for how light interacts with the surface.

Locate the `Metallic Map` slot. For this lab, we'll use a *constant* value, not a map.

Drag the **Metallic slider** from `0` (default — non-metal, dielectric) to `1` (full metal, conductor).

**Observe:** the crate now reflects its surroundings as if it were polished steel. But it also looks weirdly grey — because metals don't have their own diffuse colour. **The Base Colour you set in Step 1 now controls the *tint* of the reflection rather than the surface colour.** This is physically correct: metal mirrors only show the colour of what they're reflecting, tinted slightly by the metal's species (gold tints toward yellow, copper toward red, steel toward neutral).

This isn't quite what we want for a sci-fi crate, which should have metal panels but also painted decals that *aren't* metal. Drag Metallic back to around `0.6` — partially metallic.

**Tinker:** swing the slider 0 → 1 → 0 a few times to feel the difference. Notice that mid-values (0.3, 0.5, 0.7) look unrealistic for *real* materials — they read as "uncertain" rather than "this metal-painted thing." That's because mid-values are a physics impossibility — real materials are either conductors or insulators with no in-between.

> **Why mid-Metallic looks fake:** in the real world, materials are either conductors (metal, ~Metallic 1) or insulators (everything else, ~Metallic 0). Mid-values exist only as artistic compromises. Production assets often use a *Metallic Map* (a texture, where some pixels are 0 and others are 1) so different parts of the same mesh can be metal *or* not metal — never partly both. The painted decal pixels would have Metallic = 0; the bare metal panel pixels would have Metallic = 1; the texture authors paint the boundary cleanly.

**Checkpoint:** Crate has a metallic sheen that responds to your view angle. The Base Colour now visibly tints the reflections.

---

### Step 4 — Smoothness (Roughness inverted) (5 min)

If Metallic answers "what kind of material is this?" then Smoothness answers "how polished is it?" The two work independently — you can have polished plastic, polished metal, scuffed plastic, scuffed metal, all distinct surface identities.

Locate the **Smoothness slider** (just below Metallic in the Inspector).

Drag Smoothness from `0.5` (default — somewhat polished) up to `1.0`.

**Observe:** the crate's reflections sharpen into a near-mirror finish. Highlights become pinpricks. The crate looks freshly polished.

Now drag Smoothness down to `0.0`.

**Observe:** reflections blur into a soft sheen. Highlights spread across panels. The crate looks scuffed and weathered.

For a battered sci-fi crate, set Smoothness to about `0.3` — mostly worn with a hint of polish where the metal shows through.

**Tinker:** at Smoothness `1.0`, lower Metallic to `0` and notice that the crate now looks like *polished plastic* rather than *polished metal*. Smoothness ≠ metallicness; they're orthogonal axes of "what is this surface?" Try every combination of high/low Metallic and high/low Smoothness — you'll see four distinct surface identities emerge.

**Checkpoint:** Crate has a believable aged-metal finish at Smoothness ~0.3.

> **Vocabulary watch:** Unreal Engine and Blender call this slider **Roughness**, with the values inverted (Roughness 0 = mirror, 1 = matte). It's the same physical concept on an opposite scale. When you read tutorials online, mentally flip if the tutorial is from a different engine. There's no technical reason for the inconsistency — it's a historical accident — but it persists because changing it now would break every existing Unity project.

---

### Step 5 — Emission (HDR + Bloom) (10 min)

Now for the fun one — the property that makes sci-fi look sci-fi.

In the Material's Inspector, scroll down to the **Emission** section. Tick the checkbox to enable it.

The Emission Map / Color row appears. Click the colour swatch next to it. The colour picker opens.

**Critical:** at the top of the colour picker, ensure you're in **HDR** mode (Unity's URP Lit material exposes this by default for the Emission slot). HDR mode adds an **Intensity** slider above the colour wheel — this is what unlocks bloom-compatible emission.

Set the colour to a saturated cyan (R: 0, G: 1, B: 1) and set **Intensity** to `2`.

Now plug in the texture: click the small circle next to `Emission Map`, select `Crate_Emissive.png`. (This texture is mostly black with bright shapes where the crate's screens and indicator lights should be — it acts as a *mask*, telling Unity which pixels glow and which stay dark. The Emission Color you just set tints those bright shapes.)

**Observe:** small areas of the crate — a status screen, a row of indicator LEDs, a scrolling info ticker — now glow cyan. With Bloom enabled in the scene's Volume (already configured), the edges of the glow bleed into the surrounding pixels in a soft halo.

**Tinker:** raise Intensity to `5`, then `10`. Notice that beyond a certain intensity the glow looks "blown out" and stops feeling believable — the screens become so bright they read as light sources rather than illuminated displays. Pull back to `2`–`3` for clean sci-fi UI panels that read as "active" without being overwhelming.

**Tinker:** disable the Volume in the scene Hierarchy temporarily (untick its checkbox in the Inspector). The glow remains *bright* but no longer *blooms outwards* — the halo disappears, the screens read as flat-bright. Re-enable the Volume. **The combination of HDR Emission + Bloom is what produces the sci-fi screen look.** Either alone is significantly less effective.

**Checkpoint:** Crate has glowing UI elements that bleed light into their surroundings.

> **What HDR means here:** standard colour goes from 0 (black) to 1 (full intensity white). HDR (High Dynamic Range) allows values *above 1* — a value of 5 is "five times brighter than white." Real screens can't display values above 1 — your monitor's brightest pixel maxes out at 1.0, no matter how hard the engine pushes. But Bloom uses the over-1 information to decide *how much to bleed*. Without HDR, emission can only ever be at most "white" — and "white" doesn't bloom convincingly. The over-1 values are essentially a hint to the post-processing system about how aggressive the bloom should be.

---

### Step 6 — Opacity (Transparency) (10 min)

For this last step, you'll create a *second* material — a glass viewport on the crate's status screen. Transparency genuinely requires a separate material because it changes the surface type, which we don't want to apply to the entire crate.

In `Assets/Materials/`, right-click → `Create > Material`. Name it `CrateGlass.mat`.

Confirm `Shader` is `Universal Render Pipeline/Lit`.

**The critical step:** scroll to the top of the Inspector, find the **Surface Options** section. The first dropdown is `Surface Type`. Change it from `Opaque` to `Transparent`. The Inspector immediately reorganises — new fields appear, some old ones disappear, and the **Blending Mode** dropdown becomes available.

Now the **Base Map** colour swatch's **alpha (A) channel** is meaningful. Click the swatch and:
- Set the colour to a tinted blue: R: 0.7, G: 0.85, B: 1.0
- Drag the alpha (A) slider down to about `0.3` — meaning the material is 30% opaque.

In Surface Inputs, set `Smoothness` to `0.95` (highly polished, like glass).

Find the small `GlassPanel` child object inside the crate in the Hierarchy (pre-placed in the starter — expand the `Crate` GameObject to find it). Drag `CrateGlass.mat` onto it.

**Observe:** you can see *into* the crate through the glass panel. The panel itself catches highlights from the directional light. Move the camera — the highlights track, and what's visible behind the glass changes with your view angle.

**Tinker:** drag the alpha slider from `0.05` (almost invisible) to `1.0` (looks opaque again). Notice that even at alpha `1.0` on a Transparent material, it still renders differently from an Opaque material — sorting and depth behaviour change subtly. Production teams use Opaque materials whenever possible because they're cheaper to render.

**Tinker:** switch the Surface Type back to `Opaque` while alpha is still `0.3`. **Nothing visible changes** — Opaque ignores alpha entirely. This confirms the trap the Pitfall section warns about.

**Checkpoint:** Crate has a tinted glass panel revealing what's behind it.

*(2-minute buffer for save and Tinker Tasks below)*

---

# **Tinker Tasks**

> Quick experiments to deepen understanding. Try at least four before leaving.

| Try this | Notice |
| :-- | :-- |
| On `SciFiCrate.mat`, swap the Normal Map for a *Base Colour* texture by mistake | Crate looks weird and discoloured — normal maps must use blue-toned data (the typical purple-blue you see on normal map previews), not arbitrary RGB |
| Set Smoothness to `1` and Metallic to `0` | The crate looks like *polished plastic* — proves the two sliders are orthogonal |
| Set Emission Intensity to `0` while keeping the Emission Map plugged in | Glow vanishes entirely — Intensity is the master gain on the whole emission system, multiplying the texture's pixel values |
| On `CrateGlass.mat`, set Smoothness to `0.0` | Glass becomes frosted/etched — useful for shower doors, sci-fi privacy panels, ice |
| On `SciFiCrate.mat`, plug a **completely white** image into the Normal Map slot | Surface flattens — confirms the normal map is *displacement information*, not just colour. White means "no displacement, every pixel faces forward" |
| Disable Bloom in the scene Volume entirely | Emission survives but stops being magical — proves Bloom is half the visual recipe, not optional polish |
| Set Metallic to 1 and change the Skybox in the Lighting window | The crate's reflections completely change — confirms metals reflect the *environment*, not their own diffuse colour |

---

# **Useful Editor Tricks**

| Trick | Why it helps |
| :-- | :-- |
| Right-click a material → `Create > Variant` | Creates a child material that inherits properties from the parent — change the parent, all variants update. Production teams use this for asset family hierarchies. |
| Drag a Material onto a folder of meshes | Bulk-assigns the material to every mesh — useful for prototyping a whole environment with one base material |
| Inspector debug mode (three-dot menu top-right → Debug) | Shows raw shader property values (like `_EmissionColor` HDR intensity), useful when the editor-friendly values lie about what's happening |
| `Window > Rendering > Lighting > Environment` → Set ambient intensity to 0 | Stronger emission contrast during testing — useful for dialling in HDR values |
| Click a texture in the Inspector → `Texture Type` dropdown | Switches between Default, Normal Map, Sprite, etc. Fixes wrong-import-mode bugs in two clicks (the Step 2 pitfall) |

---

# **Debugging & Pitfalls**

| Mistake | Why it happens | Fix |
| :-- | :-- | :-- |
| Material renders pink | Shader is Built-in Standard, not URP Lit | Inspector → top of Material → set Shader to `Universal Render Pipeline/Lit` |
| Normal Map looks wrong (faint blue tint, weird highlights, or surface looks like coloured noise) | Texture not marked as a normal map on import | Click the texture in Project window → Inspector → set `Texture Type` to `Normal map`, click Apply. Or click "Fix Now" in the material's warning box |
| Metal looks grey instead of coloured | Metals reflect their *environment*, not their own diffuse colour | Either accept this (it's physically correct) or use a coloured Skybox / lights so the reflections are coloured |
| Emission set but doesn't glow | Intensity at default (RGB-mode) below 1, or Bloom disabled | Switch colour picker to HDR mode; raise Intensity above 1; confirm Volume + Bloom in scene |
| Alpha slider does nothing | Surface Type is `Opaque` | Top of Material Inspector → Surface Type → `Transparent` |
| Transparent object renders behind opaque objects | Render queue / sorting issue with stacked transparency | Usually fine for single transparent objects — for stacked transparent objects (multiple panes of glass), you may need to manually adjust `Render Queue` in the material's Advanced Options |
| Crate looks pixelated up close | Texture import set to a low max size | Click texture → Inspector → set `Max Size` to `2048` or `4096`, click Apply (note: large textures cost VRAM, balance accordingly) |
| Smoothness affects only some pixels | Using a Smoothness *map* with mostly-black pixels (= mostly rough) | Either author the map differently, or use the *constant* slider with no map plugged in |
| Material variant doesn't update when parent changes | Variant has overridden the property locally | Inspector → right-click the property → `Revert to Inherited` |
| Glass material looks completely opaque even after Surface Type change | Alpha still at 1.0 | Click the Base Map colour swatch → drag the A slider down |

---

# **Reflective Questions**

- You added one property at a time. Which single property added the most visual upgrade for you? Which added the least? Why?
- Smoothness and Metallic are independent sliders. Describe a real-world material that is **high Smoothness, low Metallic**. Now describe one that is **low Smoothness, high Metallic**.
- Emission lets a material "ignore lighting." Name three game elements that *should* ignore lighting (UI, screens, etc.) and three that *must not* (a wooden crate's base colour, a character's skin, etc.).
- Switching Surface Type from Opaque to Transparent feels like a small toggle, but it changes how the GPU renders the object significantly. Why might Unity force you to opt in rather than just "make alpha always work"?
- If you saw a Roughness slider in an Unreal tutorial set to `0.7`, what Smoothness value would you set in Unity to match? Explain in your own words why the answer is `0.3`.
- Production materials often use *texture maps* for Metallic, Smoothness, and Emission rather than constant slider values. Why? When would the constant value be the right choice?

---

# **Software Development Parallel**
PBR's "independent properties combined at render time" is structurally identical to **CSS in web development**: each property (`color`, `font-weight`, `border-radius`, `opacity`) is authored independently, and the browser's render engine combines them into the final pixel. Both systems deliberately separate *what* a surface looks like from *how* it's drawn — a separation of concerns that keeps the system manageable as it grows. The same architectural lesson applies to **component-based UI frameworks** (React, SwiftUI), where appearance properties are independent of behaviour properties, which are independent of layout properties. Once you've internalised the separation in PBR, it transfers everywhere.

---

# **Stretch Task (optional, take-home)**
Apply the same six-property workflow to **a second asset** — try the corridor wall section provided in `Assets/Models/CorridorWall.fbx`. Use the wall PBR texture set in `Assets/Textures/CorridorWall/`. The wall has different visual character — flatter, less metallic, more weathered — so you'll find different "right" values for each property than you did on the crate.

If you want to push further: author your *own* normal map by photographing a textured surface (brick, fabric, metal panel) and converting it via the free web tool **NormalMap-Online** (search "normal map online generator") or the free Windows tool **Materialize**. The conversion algorithm fakes 3D from 2D — it can't reproduce a true scanned normal map, but it gets surprisingly close for organic surfaces.

If you want to push *further* still: investigate **Ambient Occlusion** maps and **Height/Parallax** maps. Both are real PBR properties this lab didn't cover. AO maps darken crevices to fake contact shadow — the URP Lit shader has a slot for it. Height/Parallax maps push the visual displacement effect further than Normal maps can, with real performance cost. The starter project includes example textures for both if you want to experiment.

---

## Files produced by end of lab
- `LabS_PBRMaterials/` Unity project (from starter)
- `Assets/Materials/SciFiCrate.mat`
- `Assets/Materials/CrateGlass.mat`
- `Assets/Scenes/MaterialsLab.unity` (from starter, with crate and glass panel materials assigned)

---

## Lesson Context

```yaml
previous_lesson:
  topic_code: t05_decorators_materials_particles
  domain_emphasis: Games

this_lesson:
  topic_code: ts_pbr_materials
  primary_domain_emphasis: Games
  difficulty_tier: Foundational
  feeds_into: null
  status: Supplementary — outside the assessed 3-hour series. May be completed before, between, or after the numbered labs.
```
