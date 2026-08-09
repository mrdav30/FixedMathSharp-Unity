# FixedMathSharp for Unity (Lean)

Burst-oriented FixedMathSharp Unity package without the MemoryPack
serializer/runtime.

[API Documentation](https://mrdav30.github.io/FixedMathSharp-Unity/) ·
[Changelog](CHANGELOG.md)

The package includes `Chronicler.MemoryPackShim.dll` only to provide the
MemoryPack attribute shapes referenced by the Lean core DLL. The shim is not a
serializer and exposes no MemoryPack runtime serialization path.

## Install

Add this Git URL in Unity Package Manager:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean`

Use the
[Standard package](https://github.com/mrdav30/FixedMathSharp-Unity/tree/develop/com.mrdav30.fixedmathsharp)
when you want the built-in MemoryPack serializer/runtime integration.

## Migrating to v7

Start with the
[FixedMathSharp v6-to-v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x).

Unity companion renames:

| v6                                          | v7                                           |
| ------------------------------------------- | -------------------------------------------- |
| unqualified 2D/3D projection methods        | `*XY` or `*XZ` methods                       |
| `ToBoundingBox()` / `ToBoundingArea()`      | `ToFixedBoundBox()` / `ToFixedBoundAreaXZ()` |
| `FixedBoundArea.ToBounds()`                 | `ToBoundsXZ(y)`                              |
| `Fixed4x4.ToTransformLocal/World()`         | `CreateTransformLocal/World()`               |
| `Fixed3x3.ToTransformRotationLocal/World()` | `CreateTransformRotationLocal/World()`       |
| 3x3 rotation-scale transform helpers        | `FixedTransform` or strict `Fixed4x4` APIs   |

Use atomic `TryToRay*` and `TryApplyToTransform*` methods when rejection is
expected. Throwing wrappers reject semantic loss. Matrix conversion transposes
between Unity column-vector and FixedMathSharp row-vector conventions.

## Included

- Deterministic fixed-point math via `Fixed64`
- Unity math, geometry, bounds, frustum, curve, matrix, and transform adapters
- Fixed-point inspector drawers and a sample scene
- Attribute-only Chronicler MemoryPack compatibility shim

```csharp
Plane[] planeBuffer = new Plane[FixedBoundFrustum.PlaneCount];
FixedBoundFrustum frustum = camera.ToFixedBoundFrustum(planeBuffer);

if (!animationCurve.TryToFixedCurve(out FixedCurve curve))
    curve = animationCurve.BakeToFixedCurve(65);
```

`BakeToFixedCurve` is an explicit floating-point approximation. Bake once at an
authoring/loading boundary and distribute or persist the fixed keys; lockstep
peers should not bake independently.
