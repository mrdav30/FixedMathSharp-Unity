# Changelog

Notable changes to the FixedMathSharp Unity packages are documented here from
version 7 onward.

## [8.0.0] - 2026-11-04

### Added

- Optional Standard and Lean Chronicler companion packages for deterministic
  hash writing and exact simulation-time conversions. Each companion includes
  Chronicler v1.0.0 and requires its matching math base package.

### Changed

- Raised the minimum Unity editor from 2022.3 to Unity 6.3 LTS (6000.3) for
  all four packages. CI now validates the minimum supported LTS line.
- Upgraded both bundled .NET Standard 2.1 core builds to FixedMathSharp v8.0.0,
  including its finite-contact, overlap, sweep, and transform corrections.
- Bundled `Chronicler.Diagnostics` v1.0.0 in both variants and updated Lean's
  attribute-only `Chronicler.MemoryPackShim` to v1.0.0.
- Kept the Unity adapter API and Standard/Lean serialization choices unchanged.

See the [Unity package migration guide](https://github.com/mrdav30/FixedMathSharp-Unity/blob/main/MIGRATION.md)
for upgrade guidance.

## [7.0.0] - 2026-08-09

### Added

- Unity frustum conversion for cameras, projection matrices, and caller-owned
  plane buffers.
- Exact and explicitly sampled conversion between Unity `AnimationCurve` and
  FixedMathSharp `FixedCurve`.
- Explicit XY and XZ vector, bounds, and geometry projection helpers.
- Atomic `Try*` APIs for conversions and transform application that can reject
  semantic loss.

### Changed

- Upgraded the bundled FixedMathSharp core library from v6 to v7.
- Renamed Unity adapters around fixed bounds, matrices, transforms, and 2D/3D
  projections for clearer semantics.
- Matrix conversion now transposes between Unity column-vector and
  FixedMathSharp row-vector conventions.
- Throwing conversion and transform APIs now reject non-unit rays, shear,
  perspective, singular matrices, and unrepresentable hierarchy conversions.
- Kept the Standard and Lean package APIs aligned; the Lean package continues to
  omit the MemoryPack serializer/runtime.

See the
[FixedMathSharp core migration guide](https://github.com/mrdav30/FixedMathSharp/blob/main/docs/MIGRATION.md)
and this package's README for detailed migration guidance.
