# FixedMathSharp for Unity Migration Guide

This guide covers upgrades to the Unity packages. For changes to the
engine-agnostic math, geometry, and optional Chronicler companion APIs, see the
[FixedMathSharp core migration guide](https://github.com/mrdav30/FixedMathSharp/blob/main/docs/MIGRATION.md).

Update the installed Standard or Lean variant and rebuild dependent
assemblies. Install exactly one variant; both contain the `FixedMathSharp`
core assembly.

Chronicler integration is optional. Pair
`com.mrdav30.fixedmathsharp.chronicler` with the Standard math base, or
`com.mrdav30.fixedmathsharp.chronicler.lean` with Lean, at the same package
version. Install the base first. When switching variants, remove the companion
and base before installing the other pair. See the
[Chronicler integration guide](https://mrdav30.github.io/FixedMathSharp-Unity/guides/chronicler.html).

## Migrating from v7.x to v8.x

Both variants bundle the .NET Standard 2.1 build of FixedMathSharp v8.0.0 and
`Chronicler.Diagnostics` v1.0.0. Lean also bundles the attribute-only
`Chronicler.MemoryPackShim` v1.0.0. These dependencies are included as plugins;
no separate Chronicler installation is needed. Standard retains the MemoryPack
serializer/runtime; Lean continues to omit it.

The Unity adapter API is unchanged. Re-run geometry, transform, and
deterministic replay tests: corrected core results can change simulation
outcomes and replay hashes.

For formatting, use concrete `TryFormat` methods or `IFormattable`; the bundled
.NET Standard 2.1 core does not implement `ISpanFormattable`.

## Upgrading from earlier versions

Review the
[Unity package changelog](https://github.com/mrdav30/FixedMathSharp-Unity/blob/main/Build/Base/CHANGELOG.md)
for adapter changes and follow the matching sections in the core migration
guide linked above.
