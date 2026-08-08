# FixedMathSharp-Unity

[![Build](https://github.com/mrdav30/FixedMathSharp-Unity/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mrdav30/FixedMathSharp-Unity/actions/workflows/build-and-test.yml)

Unity package host for [FixedMathSharp](https://github.com/mrdav30/FixedMathSharp). Both variants contain the same deterministic math library and Unity math, geometry, bounds, and transform adapters.

## Choose a Package

### Standard

`com.mrdav30.fixedmathsharp` includes the MemoryPack serializer/runtime integration.

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`

### Lean

`com.mrdav30.fixedmathsharp.lean` omits the MemoryPack serializer/runtime and is the safer default for Burst AOT. It includes only `Chronicler.MemoryPackShim.dll`, an attribute-shape compatibility assembly required by the Lean core DLL; it does not serialize data.

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean`

## Migrating to v7

Read the [FixedMathSharp v6-to-v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x) for core changes.

The Unity companion also makes projection, allocation, and failure semantics explicit:

| v6 Unity API | v7 Unity API |
| --- | --- |
| `Vector3.ToVector2d()` | `ToVector2dXY()` or `ToVector2dXZ()` |
| `Vector2.ToVector3d(y)` | `ToVector3dXY(z)` or `ToVector3dXZ(y)` |
| `Vector2d.ToVector3(y)` | `ToVector3XY(z)` or `ToVector3XZ(y)` |
| `Vector3d.ToVector2()` | `ToVector2XY()` or `ToVector2XZ()` |
| `Bounds.ToBoundingBox()` / `ToBoundingArea()` | `ToFixedBoundBox()` / `ToFixedBoundAreaXZ()` |
| `FixedBoundArea.ToBounds()` | `ToBoundsXZ(y)` |
| `Fixed4x4.ToTransformLocal/World()` | `CreateTransformLocal/World()` |
| `Fixed3x3.ToTransformRotationLocal/World()` | `CreateTransformRotationLocal/World()` |
| 3x3 rotation-scale transform helpers | `FixedTransform` components or strict `Fixed4x4` application |

Fixed-to-Unity rays and matrix application now provide atomic `Try*` APIs. Their throwing wrappers reject non-unit ray directions, shear, perspective, singular matrices, or unrepresentable hierarchy conversions instead of silently normalizing or partially mutating a `Transform`.

Unity uses column-vector matrices while FixedMathSharp uses row-vector matrices. v7 transposes matrix elements during conversion so one-way point and direction transforms have matching semantics.

## Included

- Deterministic `Fixed64` math, vectors, quaternions, matrices, bounds, frustums, curves, and geometry
- Unity vector, ray, plane, rectangle, bounds, matrix, `AnimationCurve`, and `FixedTransform` interop
- Fixed-point inspector drawers and a sample scene

```csharp
Plane[] planeBuffer = new Plane[FixedBoundFrustum.PlaneCount];
FixedBoundFrustum frustum = camera.ToFixedBoundFrustum(planeBuffer);

if (!animationCurve.TryToFixedCurve(out FixedCurve curve))
    curve = animationCurve.BakeToFixedCurve(65);
```

`BakeToFixedCurve` is an explicit floating-point approximation. Bake once at an authoring/loading boundary and distribute or persist the fixed keys; lockstep peers should not bake independently.

These packages are installed by Git URL; they are not currently published to a Unity registry.
