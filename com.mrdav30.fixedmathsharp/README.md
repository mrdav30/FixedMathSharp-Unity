# FixedMathSharp for Unity — Standard

Deterministic Q32.32 math, Unity adapters, editor tooling, and the MemoryPack
serializer/runtime in one Unity 2022.3+ package.

[Getting started](https://mrdav30.github.io/FixedMathSharp-Unity/guides/getting-started.html) ·
[API reference](https://mrdav30.github.io/FixedMathSharp-Unity/) ·
[Serialization](https://mrdav30.github.io/FixedMathSharp-Unity/guides/serialization.html) ·
[Changelog](CHANGELOG.md)

> [!IMPORTANT]
> Install either Standard or Lean, never both. They contain the same
> `FixedMathSharp` core assembly and will conflict if installed together.

## Install

In **Window → Package Manager**, choose **Install package from git URL…**:

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp#v7.0.0
```

Code in a custom asmdef should reference `FixedMathSharp.Runtime`.

Choose [Lean](https://github.com/mrdav30/FixedMathSharp-Unity/tree/v7.0.0/com.mrdav30.fixedmathsharp.lean)
instead when you do not want the MemoryPack runtime dependency. To switch,
remove Standard, let Unity finish compiling, install Lean, and change custom
asmdef references to `FixedMathSharp.Lean.Runtime`.

## Quick start

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

Convert Unity inputs at the boundary, keep deterministic state in
FixedMathSharp types, and convert back for rendering.

## Included

- FixedMathSharp v7 core library with MemoryPack serializer/runtime support
- Vector, quaternion, matrix, ray, plane, bounds, frustum, curve, and transform
  adapters
- Fixed-point inspector drawers and authoring attributes
- **Demo Scene** sample for bounds and transform interop

Import the sample from this package's **Samples** tab in Package Manager. Unity
copies it into the project's `Assets/Samples` folder.

### Curves and matrices are explicit boundaries

`TryToFixedCurve` accepts only exactly representable Unity curves.
`BakeToFixedCurve` samples an approximation; bake once during authoring or
loading, then persist the fixed keys. Matrix adapters transpose between Unity's
column-vector convention and FixedMathSharp's row-vector convention.

Upgrading from v6? Start with the
[core v6 → v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x).
