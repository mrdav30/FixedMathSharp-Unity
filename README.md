<p align="center">
  <img src=".assets/icon_128x128.png" alt="FixedMathSharp icon" width="96" height="96">
</p>

# FixedMathSharp for Unity

[![Build](https://github.com/mrdav30/FixedMathSharp-Unity/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mrdav30/FixedMathSharp-Unity/actions/workflows/build-and-test.yml)
[![Unity 2022.3+](https://img.shields.io/badge/Unity-2022.3%2B-222c37?logo=unity)](https://unity.com/releases/editor/whats-new/2022.3.0)
[![API](https://img.shields.io/badge/docs-API-f4511e)](https://mrdav30.github.io/FixedMathSharp-Unity/)
[![Discord](https://img.shields.io/badge/discord-join%20community-5865F2?logo=discord&logoColor=white)](https://discord.gg/mhwK2QFNBA)

Bring [FixedMathSharp](https://github.com/mrdav30/FixedMathSharp)'s deterministic
Q32.32 math into Unity without letting engine types leak into your simulation.
Convert at the boundary, run the same fixed-point logic on every peer, then
convert back for rendering.

## Why use it?

- Deterministic scalars, vectors, rotations, matrices, curves, bounds, and
  geometry for lockstep games and reproducible simulations
- Explicit adapters for Unity vectors, rays, planes, rectangles, bounds,
  matrices, transforms, frustums, and animation curves
- Inspector drawers and a ready-to-import sample for authoring fixed-point data
- Standard and Lean packages with the same public math and Unity adapter surface

## Choose one package

> [!IMPORTANT]
> Install **exactly one** variant. Both packages contain the `FixedMathSharp`
> core assembly and cannot coexist in the same Unity project.

| Package | Choose it when | Serialization dependency |
| --- | --- | --- |
| **Standard** (`com.mrdav30.fixedmathsharp`) | You want the bundled MemoryPack integration. | MemoryPack runtime included |
| **Lean** (`com.mrdav30.fixedmathsharp.lean`) | You do not need MemoryPack runtime and want the smaller dependency surface. | Attribute-shape shim only; it does not serialize |

Lean removes the MemoryPack runtime, which can make it easier to use from
Burst-oriented projects. The adapters still cross Unity managed APIs and are
not blanket-certified as Burst-compatible; keep conversions at well-defined
engine boundaries and Burst-compile your deterministic jobs separately.

## Install

Requires Unity **2022.3 or newer** and Git available to the Unity Editor. In
**Window → Package Manager**, choose **Install package from git URL…** and paste
one pinned URL:

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp#v7.0.0
```

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean#v7.0.0
```

Pinning the tag keeps installs reproducible. Remove `#v7.0.0` only when you
intentionally want the repository's current default branch. The Git URLs are
the canonical way to obtain matching Standard and Lean v7 packages; check the
[OpenUPM listing](https://openupm.com/packages/com.mrdav30.fixedmathsharp/)
before relying on a registry version.

If your code is in a custom asmdef, reference `FixedMathSharp.Runtime` for
Standard or `FixedMathSharp.Lean.Runtime` for Lean.

## Your first deterministic boundary

```csharp
using FixedMathSharp;
using FixedMathSharp.Geometry;
using UnityEngine;

Vector3 unityPosition = new(10f, 0f, 4f);
Vector3d fixedPosition = unityPosition.ToVector3d();

Fixed64 tick = Fixed64.FromDouble(1.0 / 60.0);
fixedPosition += Vector3d.FromDouble(6.0, 0.0, 2.0) * tick;

Vector3 renderPosition = fixedPosition.ToVector3();

Bounds unityBounds = new(unityPosition, new Vector3(2f, 2f, 2f));
FixedBoundBox simulationBounds = unityBounds.ToFixedBoundBox();
```

Unity values are floating point. Convert inputs once, keep simulation state in
FixedMathSharp types, and convert results back only for presentation.

## Unity adapter highlights

| Area | Examples |
| --- | --- |
| Math | `Vector2/3/4`, `Quaternion`, `Matrix4x4` |
| World queries | `Ray`, `Ray2D`, `Plane`, `Rect`, `Bounds` |
| Scene interop | `Transform`, `Camera`, `FixedTransform` |
| Authoring | Fixed-point property drawers and angle attributes |
| Curves | Exact conversion when representable, explicit sampled baking otherwise |

Matrix conversion deliberately transposes between Unity's column-vector model
and FixedMathSharp's row-vector model. `BakeToFixedCurve` is deliberately an
approximation: bake once while authoring or loading, then persist or distribute
the fixed keys rather than baking independently on lockstep peers.

## Try the sample

Select the installed package in Package Manager, open **Samples**, and import
**Demo Scene**. The sample demonstrates fixed bounds and transform interop and
is copied into your project's `Assets/Samples` folder.

## Learn more

- [Getting started](https://mrdav30.github.io/FixedMathSharp-Unity/guides/getting-started.html)
- [Unity adapter API](https://mrdav30.github.io/FixedMathSharp-Unity/api/FixedMathSharp.html)
- [Serialization choices](https://mrdav30.github.io/FixedMathSharp-Unity/guides/serialization.html)
- [FixedMathSharp core guides](https://github.com/mrdav30/FixedMathSharp/wiki)
- [FixedMathSharp core API](https://mrdav30.github.io/FixedMathSharp/)
- [v6 → v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x)
- [Contributing](CONTRIBUTING.md)

FixedMathSharp is MIT licensed. Questions and ideas are welcome on
[GitHub](https://github.com/mrdav30/FixedMathSharp-Unity/issues) or
[Discord](https://discord.gg/mhwK2QFNBA).
