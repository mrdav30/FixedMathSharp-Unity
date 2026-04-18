# FixedMathSharp-Unity

Unity package host for FixedMathSharp.

This repository contains two installable Unity Package Manager variants:

- `com.mrdav30.fixedmathsharp`
- `com.mrdav30.fixedmathsharp.nomemorypack`

Both packages provide the same core deterministic fixed-point math library and Unity
interop helpers. The difference is whether the package includes `MemoryPack`.

## Which Package Should I Use?

### `com.mrdav30.fixedmathsharp`

Standard package with `MemoryPack` support enabled.

Install:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp`

Use it when:

- You want the default package.
- You want the built-in `MemoryPack` serialization path.
- Burst AOT compatibility is not your main concern.

### `com.mrdav30.fixedmathsharp.nomemorypack`

Package without `MemoryPack`.

Install:

`https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.nomemorypack`

Use it when:

- You use Unity Burst AOT.
- You want the safer choice for Burst-oriented builds.
- You want to avoid the `MemoryPack` dependency.
- You plan to use your own serialization stack.

## Why Two Variants?

If you use Unity Burst AOT, prefer the `NoMemoryPack` build. `MemoryPack`'s Unity
support is centered on IL2CPP via its .NET source-generator path, so the
no-MemoryPack variant is the safer choice for Burst AOT scenarios.

## What Both Packages Include

- Deterministic fixed-point math via `Fixed64`
- Core math, trigonometry, vectors, quaternions, and matrices
- Bounds and geometry helpers
- Unity transform, matrix, and bounds interop
- Editor helpers and sample content

## Notes

- This repository is currently intended for Git URL installation rather than an
  official package registry.
- Each package has its own `README.md` with package-specific installation guidance.
- The underlying parent library lives here:
  [FixedMathSharp](https://github.com/mrdav30/FixedMathSharp)
