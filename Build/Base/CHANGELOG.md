# Changelog

All notable changes to the FixedMathSharp Unity packages are documented here.

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
[FixedMathSharp v6-to-v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x)
and this package's README for detailed migration guidance.
