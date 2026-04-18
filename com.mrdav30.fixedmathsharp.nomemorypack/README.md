# FixedMathSharp for Unity (NoMemoryPack)

Unity package for FixedMathSharp without the `MemoryPack` dependency.

This is the recommended variant for Unity Burst AOT scenarios. `MemoryPack`'s Unity
support is centered on IL2CPP through its .NET source-generator path, so the
no-MemoryPack build is the safer choice when Burst AOT compatibility is the priority.

## Install

Add this package from Git URL in Unity Package Manager:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.nomemorypack`

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

## Related Packages

- Repo overview and variant selection:
  [FixedMathSharp-Unity](https://github.com/mrdav30/FixedMathSharp-Unity)
- Standard variant:
  `https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`
