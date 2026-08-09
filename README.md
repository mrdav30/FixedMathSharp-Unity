# FixedMathSharp-Unity

[![Build](https://github.com/mrdav30/FixedMathSharp-Unity/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mrdav30/FixedMathSharp-Unity/actions/workflows/build-and-test.yml)
[![Discord](https://img.shields.io/badge/discord-join%20community-5865F2?logo=discord&logoColor=white)](https://discord.gg/mhwK2QFNBA)

[API Documentation](https://mrdav30.github.io/FixedMathSharp-Unity/)

Unity package host for
[FixedMathSharp](https://github.com/mrdav30/FixedMathSharp). Both variants
contain the same deterministic math library and Unity math, geometry, bounds,
and transform adapters.

## Choose a Package

### Standard

`com.mrdav30.fixedmathsharp` includes the MemoryPack serializer/runtime
integration.

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`

### Lean

`com.mrdav30.fixedmathsharp.lean` omits the MemoryPack serializer/runtime and is
the safer default for Burst AOT. It includes only
`Chronicler.MemoryPackShim.dll`, an attribute-shape compatibility assembly
required by the Lean core DLL; it does not serialize data.

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean`

## Included

- Deterministic `Fixed64` math, vectors, quaternions, matrices, bounds,
  frustums, curves, and geometry
- Unity vector, ray, plane, rectangle, bounds, matrix, `AnimationCurve`, and
  `FixedTransform` interop
- Fixed-point inspector drawers and a sample scene

```csharp
Plane[] planeBuffer = new Plane[FixedBoundFrustum.PlaneCount];
FixedBoundFrustum frustum = camera.ToFixedBoundFrustum(planeBuffer);

if (!animationCurve.TryToFixedCurve(out FixedCurve curve))
    curve = animationCurve.BakeToFixedCurve(65);
```

`BakeToFixedCurve` is an explicit floating-point approximation. Bake once at an
authoring/loading boundary and distribute or persist the fixed keys; lockstep
peers should not bake independently.

These packages are installed by Git URL; they are not currently published to a
Unity registry.
