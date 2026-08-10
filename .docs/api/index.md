---
title: FixedMathSharp for Unity
description: Unity adapters, editor tooling, package guidance, and API reference for deterministic FixedMathSharp simulations.
---

<div class="fms-hero">
  <p class="fms-kicker">DETERMINISTIC MATH AT THE UNITY BOUNDARY</p>
  <h1>Author in Unity. Simulate in fixed point.</h1>
  <p>FixedMathSharp for Unity connects engine types and workflows to a
  deterministic Q32.32 math core—without making Unity the source of truth for
  your simulation.</p>
  <div class="fms-actions">
    <a href="guides/getting-started.md">Get started</a>
    <a href="xref:FixedMathSharp">Browse the adapter API</a>
  </div>
</div>

## Choose your package

Install exactly one variant; both contain the same `FixedMathSharp` core
assembly.

<div class="fms-card-grid">
  <div class="fms-card">
    <h3><a href="https://github.com/mrdav30/FixedMathSharp-Unity/tree/v7.0.0/com.mrdav30.fixedmathsharp">Standard</a></h3>
    <p>Unity adapters, editor tooling, samples, and the bundled MemoryPack
    serializer/runtime.</p>
  </div>
  <div class="fms-card">
    <h3><a href="https://github.com/mrdav30/FixedMathSharp-Unity/tree/v7.0.0/com.mrdav30.fixedmathsharp.lean">Lean</a></h3>
    <p>The same adapter surface without MemoryPack runtime, for projects that
    bring their own serialization stack.</p>
  </div>
  <div class="fms-card">
    <h3><a href="https://mrdav30.github.io/FixedMathSharp/">Core library</a></h3>
    <p>Explore the engine-agnostic scalars, vectors, rotations, matrices,
    curves, transforms, random streams, and geometry underneath the adapters.</p>
  </div>
</div>

## Work at explicit boundaries

<div class="fms-card-grid">
  <div class="fms-card">
    <h3><a href="xref:FixedMathSharp.Vector3dExtensions">Math interop</a></h3>
    <p>Convert vectors, quaternions, matrices, and transforms while preserving
    documented coordinate and multiplication conventions.</p>
  </div>
  <div class="fms-card">
    <h3><a href="xref:FixedMathSharp.Geometry.FixedGeometryUnityExtensions">World interop</a></h3>
    <p>Bridge rays, planes, rectangles, bounds, cameras, and frustums with
    explicit XY and XZ projections.</p>
  </div>
  <div class="fms-card">
    <h3><a href="xref:FixedMathSharp.FixedCurveUnityExtensions">Authoring interop</a></h3>
    <p>Use fixed-point inspector drawers and choose exact or deliberately baked
    animation-curve conversion.</p>
  </div>
</div>

Unity values are converted at adapter boundaries. Keep deterministic state in
FixedMathSharp types after conversion, and convert back only when the engine
needs a presentation value.

## Resources

- [Getting started](guides/getting-started.md)
- [Serialization choices](guides/serialization.md)
- [Unity adapter source and issues](https://github.com/mrdav30/FixedMathSharp-Unity)
- [FixedMathSharp wiki](https://github.com/mrdav30/FixedMathSharp/wiki)
- [v6 → v7 migration guide](https://github.com/mrdav30/FixedMathSharp/blob/v7.0.0/docs/MIGRATION.md#migrating-from-v6x-to-v7x)

The API reference is generated from the canonical adapter source under
`Build/Base/Runtime`. The core API is linked through the FixedMathSharp site.
