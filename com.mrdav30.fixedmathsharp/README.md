# FixedMathSharp for Unity

Standard FixedMathSharp Unity package with the MemoryPack serializer/runtime integration enabled.

## Install

Add this Git URL in Unity Package Manager:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`

Use the [Lean package](https://github.com/mrdav30/FixedMathSharp-Unity/tree/develop/com.mrdav30.fixedmathsharp.lean) instead when Burst AOT is the priority or you provide your own serializer.

## Migrating to v7

Start with the [FixedMathSharp v6-to-v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x).

Unity companion renames:

| v6 | v7 |
| --- | --- |
| unqualified 2D/3D projection methods | `*XY` or `*XZ` methods |
| `ToBoundingBox()` / `ToBoundingArea()` | `ToFixedBoundBox()` / `ToFixedBoundAreaXZ()` |
| `FixedBoundArea.ToBounds()` | `ToBoundsXZ(y)` |
| `Fixed4x4.ToTransformLocal/World()` | `CreateTransformLocal/World()` |
| `Fixed3x3.ToTransformRotationLocal/World()` | `CreateTransformRotationLocal/World()` |
| 3x3 rotation-scale transform helpers | `FixedTransform` or strict `Fixed4x4` APIs |

Use atomic `TryToRay*` and `TryApplyToTransform*` methods when rejection is expected. Throwing wrappers reject semantic loss. Matrix conversion transposes between Unity column-vector and FixedMathSharp row-vector conventions.

## Included

- Deterministic fixed-point math via `Fixed64`
- Unity math, geometry, bounds, matrix, and transform adapters
- Fixed-point inspector drawers and a sample scene
- MemoryPack serializer/runtime support
