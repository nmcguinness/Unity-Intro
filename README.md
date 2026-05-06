# Unity Animation Labs 

This repository contains a series of practical Unity labs that take you from your first keyframe to a character whose head turns to track threats in a procedurally-decorated environment. Across the labs you'll build a small interactive scene, scripted in C#, dressed with materials and decals, and capped with constraint-based animation.

The labs are designed to be worked through in order. They share a quietly-connected scene that emerges as you progress — but each lab is self-contained, so you can also dip into individual topics if you're catching up or revisiting a concept.

---

## Quick start

```bash
# 1. Clone the repository (one time)
git clone <REPO_LINK> unity-animation-labs
cd unity-animation-labs

# 2. Each lab has its own starter project folder.
#    Open the appropriate folder in Unity Hub when starting a lab:
#    - Lab 1: Lab01_Starter/
#    - Lab 2: Lab02_Starter/
#    - ...etc.

# 3. Open the corresponding lab handout from Labs/ in your favourite Markdown viewer
#    (VS Code, Typora, GitHub web, or just a browser).
```

You only need to clone the repo **once**. Each lab uses a different starter folder inside the cloned directory.

---

## Before you start

You'll need:

- **Unity Hub** installed.
- **Unity 6.3 LTS Editor** installed via Unity Hub. The labs will not run reliably on older versions.
- A Blender-authored **`IcoSphere.fbx`** (used in Lab 1) and **`Character.fbx` with `Idle` and `Walk` clips** (used in Labs 2, 4, 5, 6). These come from your Blender module — speak to your Blender lecturer if you don't yet have them. Lab 5 and 6 starters include backup versions of both for students who miss the Blender prerequisite, but the labs read better when you use your own work.
- A working understanding of **basic C#** (variables, methods, classes, `[SerializeField]`) — needed from Lab 3 onwards. You don't need to write code from scratch in any lab; complete commented scripts are provided.

If you've never opened the Unity Editor before, do that first. Create an empty 3D (URP) Core project, click around, learn where the Hierarchy, Inspector, Scene view, and Game view live. None of the labs will spend time teaching the editor's basic UI.

---

## The labs at a glance

| # | Lab | Description | Level | Key concepts | File |
| :-- | :-- | :-- | :-- | :-- | :-- |
| **1** | Animating a Bouncing Ball | Author your first Animation Clip — a looping bounce on a Blender-imported ball, with curve shaping and squash & stretch. The smallest possible self-contained animation exercise. | Beginner | keyframes, curves, tangents, loop time, squash & stretch | [Lab 1](Labs/Lab01_BouncingBall.md) |
| **2** | Animating a Character | Build an Animator Controller with two states (Idle and Walk) on your Blender-rigged character. Drive transitions with a Bool parameter. | Beginner | Animator Controller, state machines, parameters, transitions, Generic rigs, `Has Exit Time` | [Lab 2](Labs/Lab02_AnimatingACharacter.md) |
| **3** | Procedural Animation in Code | Write three small scripts that drive motion via trigonometry, `AnimationCurve`, and DOTween. See three approaches to procedural motion side by side. | Beginner+ | `Mathf.Sin`, `AnimationCurve`, easing functions, DOTween, fluent builder pattern | [Lab 3](Labs/Lab03_ProceduralAnimation.md) |
| **4** | Input & Character Control | Combine the Animator from Lab 2 with a script that reads input via Unity 6's project-wide actions. Upgrade the Animator's Bool to a Float and drive locomotion end-to-end. | Beginner+ | Input System, project-wide actions, `SetFloat`, `[RequireComponent]`, Lerp smoothing, locomotion | [Lab 4](Labs/Lab04_InputAndCharacterControl.md) |
| **5** | Decorators — Materials, Lights & Decals | Add an emissive pulse to the ball, a flicker effect to corridor lights, and three URP decals to the environment. Decoration via composition, not modification. | Beginner+ | URP emissive materials, HDR + Bloom, Light intensity, Decal Projectors, decoration pattern | [Lab 5](Labs/Lab05_DecoratorsMaterialsAndDecals.md) |
| **6** | Procedural Head Tracking *(take-home)* | The capstone. Install the Animation Rigging package and add a Multi-Aim Constraint to the character's head, with realistic human cervical limits. Move a target through the scene; the head follows. | Advanced | Animation Rigging, Rig Builder, Multi-Aim Constraint, rotation limits, layered animation, constraint weights | [Lab 6](Labs/Lab06_HeadTracking.md) |

---

## Recommended order

The numbered labs are intended to be worked through **in order** — they share assets and the scripts in each lab build on the previous one's setup. Lab 6 is take-home and optional, recommended after Lab 5. Lab S can be slotted in at any point but lands best after Lab 5, where you've already met emission casually and want to understand it properly.

```
Lab 1 → Lab 2 → Lab 3 → Lab 4 → Lab 5  →  [optional: Lab 6 take-home]
```

Each numbered lab is **30 minutes**. The full assessed series is 2.5 hours of practical work plus reflection time. Lab 6 is 60–90 minutes take-home, Lab S is 45 minutes whenever convenient.

---

## What you'll have built by the end

By the time you reach the end of Lab 6, your project will contain:

- A bouncing energy orb in an antechamber, glowing on each impact.
- A rigged character that walks under your control through a flickering, decal-marked corridor.
- A chamber at the far end with a static silhouette waiting in the dark.
- A character whose head turns to follow a target you move with the mouse — even as their body keeps walking.

The whole environment runs on layered systems you've built piece by piece: keyframed clips, state machines, scripted input, procedural decoration, and constraint-based rigging. Once you've built it, you have the conceptual toolkit to tackle any character animation problem in Year 2 and beyond.

---

## Repository structure

```
unity-animation-labs/
├── README.md                       (this file)
└── Labs/
   ├── Lab01_BouncingBall.md
   ├── Lab02_AnimatingACharacter.md
   ├── Lab03_ProceduralAnimation.md
   ├── Lab04_InputAndCharacterControl.md
   ├── Lab05_DecoratorsMaterialsAndDecals.md
   └── Lab06_HeadTracking.md
```

Each starter folder is a scene in a single Unity 6.4 LTS project. Open after downloading the starter code in the [repo](https://github.com/nmcguinness/Unity-Intro). The first open will take a minute or two as Unity imports assets and recompiles scripts.

---

## Tips for getting the most out of these labs

- **Read the lab handout fully before starting.** The Core Ideas at the top will save you time when you hit the practical steps.
- **Try the Tinker Tasks.** Each lab has a "Tinker Tasks" section near the end with one-minute experiments designed to deepen understanding. They're consistently the highest-value-per-minute thing in each lab.
- **Don't skip the Reflective Questions.** They look optional but they're where the conceptual learning consolidates. Even if you don't write answers down, think them through.
- **The Stretch Tasks are deliberately not walked through.** They're where the lab hands off to your own curiosity. If a lab clicked for you, the stretch task is your reward — and the time you spend on it usually outweighs the in-lab time pedagogically.
- **Pitfalls tables exist for a reason.** If something goes wrong, check the lab's Pitfalls table first. The likelihood that your bug is one of the listed ones is genuinely high.
- **Save often.** `Ctrl/Cmd + S` between every lab step.

---

## Getting help

- **Repository/setup issues**: raise a GitHub Issue with a screenshot of any error message.
- **Blender asset problems** (rig type, scale, missing clips): speak to your Blender lecturer first — these aren't Unity issues.
- **Hardware/performance issues**: the labs are designed to run on the standard college lab machines. If you're working from home and hitting performance problems, Unity 6.4 LTS recommends 8GB+ RAM and a GPU with at least 2GB VRAM.

---

## Licensing

- DOTween used under its [free licence](http://dotween.demigiant.com/) (Demigiant).
- Unity Animation Rigging is part of the Unity Package Manager (Unity Companion License).

These materials are provided for educational use within the module. Treat the lab handouts and starter scenes as you would any other course material — use them, modify them for your own learning, but don't republish them outside the module without permission.
