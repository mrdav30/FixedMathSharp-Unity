# FixedMathSharp for Unity Documentation

FixedMathSharp for Unity provides explicit adapters between Unity's
floating-point math types and the deterministic types in FixedMathSharp.

- [Browse the Unity adapter API](xref:FixedMathSharp.FixedCurveUnityExtensions)
- [Read the FixedMathSharp core API](https://mrdav30.github.io/FixedMathSharp/)
- [Review the v6-to-v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x)
- [Install the Standard or Lean Unity package](https://github.com/mrdav30/FixedMathSharp-Unity#choose-a-package)

Unity values are converted at adapter boundaries. Keep deterministic simulation
state in FixedMathSharp types after conversion.

`BakeToFixedCurve` is an explicit floating-point approximation. Bake once during
authoring or loading and distribute or persist the resulting fixed keys;
lockstep peers should not bake independently.
