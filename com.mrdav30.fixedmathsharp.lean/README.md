# FixedMathSharp for Unity (Lean Variant)

Unity package for FixedMathSharp without the `MemoryPack` dependency.

This is the recommended variant for Unity Burst AOT scenarios. `MemoryPack`'s Unity
support is centered on IL2CPP through its .NET source-generator path, so the
`Lean` build is the safer choice when Burst AOT compatibility is the priority.

## Install

Add this package from Git URL in Unity Package Manager:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.lean`

## When To Use This Package

- You are using Unity Burst AOT.
- You want the safest default option for Burst-oriented projects.
- You do not want `MemoryPack` included.
- You prefer to bring your own serialization solution.

## Choose The Other Variant If

- You want the standard package with built-in `MemoryPack` support.
- Your project depends on the package's `MemoryPack` serialization path.

## Included

- Deterministic fixed-point math via `Fixed64`
- Vectors, quaternions, matrices, and bounds
- Unity transform, matrix, and bounds interop helpers
- Editor debugging helpers
- Sample demo scene
- No `MemoryPack` dependency

## Coordinate Conventions

FixedMathSharp's canonical 3D basis is `+X` right, `+Y` up, and `+Z` forward.
Unity uses the same semantic basis for `Vector3.right`, `Vector3.up`, and
`Vector3.forward`, so vector and quaternion adapters use direct component mappings.

Matrix and `Transform` helpers are semantic conversions. Unity's `Matrix4x4` and
`Transform` APIs use Unity's storage and application rules, while FixedMathSharp
uses row-vector matrices with translation in `M41`/`M42`/`M43`.

## Related Packages

- Repo overview and variant selection:
  [FixedMathSharp-Unity](https://github.com/mrdav30/FixedMathSharp-Unity)
- Standard variant:
  `https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`
