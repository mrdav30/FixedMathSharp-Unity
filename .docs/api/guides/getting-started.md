---
title: Getting started
description: Install one FixedMathSharp Unity package, connect your asmdefs, and create a deterministic Unity boundary.
---

# Getting started

FixedMathSharp for Unity is an adapter layer around the engine-agnostic
[FixedMathSharp core library](https://github.com/mrdav30/FixedMathSharp). Unity
types stay at the edges of your application; deterministic state stays in
`Fixed64`, fixed vectors, matrices, transforms, curves, and geometry types.

## 1. Choose exactly one package

Both variants require Unity **2022.3 or newer** and expose the same math and
Unity adapter APIs. Repository CI currently validates Unity **6000.5.0f1**.

| Variant | Package ID | Use it when |
| --- | --- | --- |
| Standard | `com.mrdav30.fixedmathsharp` | You want the bundled MemoryPack serializer/runtime. |
| Lean | `com.mrdav30.fixedmathsharp.lean` | You do not need MemoryPack runtime and prefer the smaller dependency surface. |

> [!WARNING]
> Do not install both. Each package ships the `FixedMathSharp` core assembly, so
> installing both creates duplicate assemblies and types.

Lean includes `Chronicler.MemoryPackShim` instead of `MemoryPack.Core`. The shim
provides only the MemoryPack attribute shapes referenced by the Lean core DLL;
it does not serialize anything. See [Serialization](serialization.md) for the
full package matrix.

## 2. Install from Git

Make sure Git is installed and available to the Unity Editor. Open **Window →
Package Manager**, select the **+** menu, choose **Install package from git
URL…**, and paste one URL:

### Standard

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp#v7.0.0
```

### Lean

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean#v7.0.0
```

The `#v7.0.0` revision makes the install reproducible. Update it deliberately
when you adopt a newer release.

If your scripts live in a custom assembly definition, add the matching
reference:

- Standard: `FixedMathSharp.Runtime`
- Lean: `FixedMathSharp.Lean.Runtime`

To switch variants, remove the installed package, allow Unity to finish
compiling, install the other variant, then update any custom asmdef references.

## 3. Create a deterministic boundary

Convert Unity input once, perform simulation work with fixed-point values, and
convert back when Unity needs a rendering or scene value.

```csharp
using FixedMathSharp;
using FixedMathSharp.Geometry;
using UnityEngine;

Vector3 unityPosition = new(10f, 0f, 4f);
Vector3d fixedPosition = unityPosition.ToVector3d();

Fixed64 tick = Fixed64.FromDouble(1.0 / 60.0);
Vector3d velocity = Vector3d.FromDouble(6.0, 0.0, 2.0);
fixedPosition += velocity * tick;

Vector3 renderPosition = fixedPosition.ToVector3();

Bounds unityBounds = new(unityPosition, new Vector3(2f, 2f, 2f));
FixedBoundBox simulationBounds = unityBounds.ToFixedBoundBox();
```

This makes the nondeterministic boundary visible: the conversion from Unity
`float` values occurs before the deterministic update, and conversion back to
`float` occurs afterward.

## 4. Pick the adapter that owns the conversion

The API is organized by the FixedMathSharp type being adapted.

| Need | Start here |
| --- | --- |
| Unity and fixed vectors | <xref:FixedMathSharp.Vector2dExtensions>, <xref:FixedMathSharp.Vector3dExtensions>, <xref:FixedMathSharp.Vector4dExtensions> |
| Quaternions and matrices | <xref:FixedMathSharp.FixedQuaternionExtensions>, <xref:FixedMathSharp.Fixed3x3UnityExtensions>, <xref:FixedMathSharp.Fixed4x4UnityExtensions> |
| Bounds, rays, planes, and rectangles | <xref:FixedMathSharp.Geometry.FixedBoundsUnityExtensions>, <xref:FixedMathSharp.Geometry.FixedGeometryUnityExtensions> |
| Cameras and frustums | <xref:FixedMathSharp.Geometry.FixedFrustumUnityExtensions> |
| Unity transforms | <xref:FixedMathSharp.FixedTransformUnityExtensions> |
| Animation curves | <xref:FixedMathSharp.FixedCurveUnityExtensions> |

Unity and FixedMathSharp both use `+X` right, `+Y` up, and `+Z` forward. A
`Vector3` conversion is therefore a direct component mapping. Two-dimensional
projections are named explicitly with `XY` or `XZ`; choose the plane your game
actually uses.

### Matrices and transforms

Unity uses column vectors while FixedMathSharp transform math uses row vectors.
Matrix conversions transpose deliberately to preserve the represented
transformation. Avoid copying matrix elements by hand.

Conversions that may lose meaning expose atomic `Try*` methods. Prefer
`TryToRay*` and `TryApplyToTransform*` when rejection is expected; throwing
wrappers are useful when semantic loss is a programming error. Rejections can
include non-unit ray directions, shear, perspective, singular matrices, or
hierarchy conversions that cannot be represented faithfully.

### Curves

`TryToFixedCurve` succeeds only when a Unity `AnimationCurve` has an exactly
representable fixed equivalent. It rejects weighted keys, incompatible wrapping
or interpolation, non-finite values, and keys that collapse in fixed-point.

Use `BakeToFixedCurve(sampleCount)` when an approximation is intentional. Bake
once at an authoring or loading boundary, then persist or distribute the fixed
keys. Lockstep peers should not independently sample Unity curves.

## 5. Author fixed-point values in the Inspector

Both packages include property drawers for supported FixedMathSharp values and
two angle-oriented authoring attributes:

```csharp
using FixedMathSharp;
using UnityEngine;

public sealed class SteeringSettings : MonoBehaviour
{
    [SerializeField, VectorRotation]
    private Vector2d heading = Vector2d.Right;

    [SerializeField, FixedNumberAngle(max: 90d)]
    private Fixed64 steeringSine = Fixed64.Zero;
}
```

`VectorRotation` edits a unit direction, so changing it does not preserve the
vector's previous magnitude. `FixedNumberAngle` displays an angle but stores its
sine in the `Fixed64` field; the stored value is not a degree count.

## 6. Import the sample

Select the installed package in Package Manager, open its **Samples** tab, and
import **Demo Scene**. Unity copies the sample into the project's
`Assets/Samples` folder. The scene demonstrates:

- converting Unity `Bounds` to fixed bounds and back;
- applying fixed transform state at the Unity boundary; and
- the asmdef reference required by the selected package.

## Burst-oriented projects

Lean removes the MemoryPack runtime dependency, but that alone does not make
every adapter Burst-compatible. Unity objects, property drawers,
`AnimationCurve`, `Transform`, and other managed engine APIs remain boundary
code. Keep Burst jobs focused on the fixed data and operations they can compile,
and validate those jobs with your project's Burst version and settings.

## Next steps

- Browse the [Unity adapter API](xref:FixedMathSharp).
- Choose a [serialization strategy](serialization.md).
- Read the [FixedMathSharp wiki](https://github.com/mrdav30/FixedMathSharp/wiki)
  for Q32.32 behavior, rounding, overflow, coordinate conventions, and core
  algorithms.
- Review the [v6 → v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x)
  when upgrading an existing project.
