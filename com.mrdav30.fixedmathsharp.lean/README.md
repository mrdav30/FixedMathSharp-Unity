# FixedMathSharp for Unity — Lean

Deterministic Q32.32 math, Unity adapters, and editor tooling without the
MemoryPack serializer/runtime dependency. Requires Unity 2022.3+.

[Getting started](https://mrdav30.github.io/FixedMathSharp-Unity/guides/getting-started.html) ·
[API reference](https://mrdav30.github.io/FixedMathSharp-Unity/) ·
[Serialization](https://mrdav30.github.io/FixedMathSharp-Unity/guides/serialization.html) ·
[Changelog](CHANGELOG.md)

> [!IMPORTANT]
> Install either Lean or Standard, never both. They contain the same
> `FixedMathSharp` core assembly and will conflict if installed together.

Lean includes `Chronicler.MemoryPackShim` instead of `MemoryPack.Core`. The shim
only supplies attribute shapes referenced by the Lean core DLL; it is not a
serializer and exposes no MemoryPack runtime serialization path.

## Install

In **Window → Package Manager**, choose **Install package from git URL…**:

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean#v7.0.0
```

Code in a custom asmdef should reference `FixedMathSharp.Lean.Runtime`.

Choose [Standard](https://github.com/mrdav30/FixedMathSharp-Unity/tree/v7.0.0/com.mrdav30.fixedmathsharp)
instead when you want the bundled MemoryPack integration. To switch, remove
Lean, let Unity finish compiling, install Standard, and change custom asmdef
references to `FixedMathSharp.Runtime`.

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

- FixedMathSharp v7 core library without the MemoryPack runtime
- Attribute-only `Chronicler.MemoryPackShim` compatibility assembly
- Vector, quaternion, matrix, ray, plane, bounds, frustum, curve, and transform
  adapters
- Fixed-point inspector drawers and authoring attributes
- **Demo Scene** sample for bounds and transform interop

Import the sample from this package's **Samples** tab in Package Manager. Unity
copies it into the project's `Assets/Samples` folder.

### About Burst

Lean's smaller dependency surface is useful in Burst-oriented projects, but the
package's Unity adapters still call managed Unity APIs and are not blanket-
certified as Burst-compatible. Convert at engine boundaries and Burst-compile
the deterministic jobs you control.

`TryToFixedCurve` accepts only exactly representable Unity curves.
`BakeToFixedCurve` samples an approximation; bake once during authoring or
loading, then persist the fixed keys. Matrix adapters transpose between Unity's
column-vector convention and FixedMathSharp's row-vector convention.

Upgrading from v6? Start with the
[core v6 → v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x).
