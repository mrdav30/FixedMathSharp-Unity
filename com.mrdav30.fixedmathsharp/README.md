# FixedMathSharp for Unity

Standard Unity package for FixedMathSharp with `MemoryPack` support enabled.

Use this variant when you want the Unity integration plus the built-in `MemoryPack`
serialization path. If you are targeting Unity Burst AOT and want the safer option,
use the `NoMemoryPack` package instead.

## Install

Add this package from Git URL in Unity Package Manager:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`

## When To Use This Package

- You want the standard FixedMathSharp-Unity package.
- You want the package's `MemoryPack`-based serialization support.
- You are not optimizing specifically for Burst AOT compatibility.

## Choose The Other Variant If

- You are using Unity Burst AOT.
- You want to avoid the `MemoryPack` dependency entirely.
- You plan to use your own serialization layer.

## Included

- Deterministic fixed-point math via `Fixed64`
- Vectors, quaternions, matrices, and bounds
- Unity transform, matrix, and bounds interop helpers
- Editor debugging helpers
- Sample demo scene
- `MemoryPack` support

## Related Packages

- Repo overview and variant selection:
  [FixedMathSharp-Unity](https://github.com/mrdav30/FixedMathSharp-Unity)
- No-MemoryPack variant:
  `https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.nomemorypack`
