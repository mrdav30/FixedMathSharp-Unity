# Unity Frustum and AnimationCurve Bridges Design

**Status:** Approved

## Summary

Add focused runtime adapters between Unity camera/frustum data and
`FixedBoundFrustum`, plus exact and sampled adapters between Unity
`AnimationCurve` and `FixedCurve`.

The adapters remain boundary code. They convert Unity floating-point state into
fixed data but do not move camera control, animation playback, or simulation
policy into this package. Once converted, consumers use FixedMathSharp for
deterministic evaluation and geometry queries.

## Goals

- Extract perspective, orthographic, physical, oblique, and custom-projection
  Unity frustums into `FixedBoundFrustum` without reconstructing camera math.
- Preserve the opposing Unity and FixedMathSharp frustum-plane conventions
  explicitly and test them in both directions.
- Provide convenient allocating frustum APIs and caller-buffer overloads for
  repeated use.
- Convert representable Unity curves to `FixedCurve` without silently losing
  tangent, weighting, stepping, or wrap semantics.
- Provide an explicitly approximate bake path for curves that cannot be
  represented structurally.
- Convert all valid `FixedCurveMode` values back to equivalent Unity curves.
- Keep the Standard and Lean packages behaviorally and byte-for-byte aligned
  for shared runtime code.

## Non-Goals

- Camera controllers, camera authoring components, culling systems, or render
  pipeline integration.
- Applying an arbitrary `FixedBoundFrustum` back onto a Unity `Camera`.
- Unity editor drawers or serialized authoring assets for `FixedCurve`.
- Implicit per-frame baking or Unity `AnimationCurve.Evaluate` calls inside
  deterministic simulation.
- Extending core FixedMathSharp types or changing their serialization layout.
- Reproducing weighted Unity curves exactly in `FixedCurve`; the core type has
  no tangent weights.
- Sample-scene changes.

## Constraints

- Edit shared runtime code under `Build/Base` first, then use the canonical
  Unity package sync.
- Support Unity 2022.3 and newer; validate with the host project's Unity
  6000.5.0f1 editor.
- Runtime APIs must compile in both the Standard and Lean packages without new
  package dependencies.
- Every public method receives concise XML documentation. Non-obvious buffer,
  failure, tangent, and approximation behavior must be explicit.
- Float evaluation and conversion happen only at the Unity boundary. The
  resulting `FixedBoundFrustum` and `FixedCurve` use deterministic fixed-point
  operations.

## Alternatives Considered

### Semantic boundary adapters (selected)

Use `GeometryUtility.CalculateFrustumPlanes` for Unity frustums, map plane
orientation and order explicitly, structurally convert only representable
curves, and expose a separate sampling API for approximation.

This handles custom Unity projections, preserves exact curve contracts where
possible, and makes every lossy operation opt-in.

### Direct matrix and key copying

Convert Unity view-projection matrices directly to `Fixed4x4` and copy Unity
key tangents without scaling. This is smaller but incorrect: Unity projection
conventions can vary, the two frustum types use opposing plane orientations,
and Unity tangents are value-per-time slopes while `FixedCurve` cubic tangents
operate over normalized segment time.

### Always reconstruct or sample

Rebuild frustums from camera properties and sample every curve. This loses
custom camera projection behavior and needlessly approximates curves that have
an exact Hermite representation.

## Public API

### Frustum bridge

Create `Build/Base/Runtime/Extensions/FixedFrustum.Extensions.cs` with a
`FixedFrustumUnityExtensions` class in `FixedMathSharp.Geometry`:

```csharp
public static FixedBoundFrustum ToFixedBoundFrustum(this Camera camera);

public static FixedBoundFrustum ToFixedBoundFrustum(
    this Camera camera,
    Plane[] planeBuffer);

public static FixedBoundFrustum ToFixedBoundFrustum(
    this Matrix4x4 worldToProjectionMatrix);

public static FixedBoundFrustum ToFixedBoundFrustum(
    this Matrix4x4 worldToProjectionMatrix,
    Plane[] planeBuffer);

public static Plane[] ToUnityFrustumPlanes(this FixedBoundFrustum frustum);

public static void CopyToUnityFrustumPlanes(
    this FixedBoundFrustum frustum,
    Plane[] planeBuffer);
```

The allocating overloads allocate one six-plane array and delegate to their
buffer counterparts. Buffer overloads require a non-null array whose length is
exactly `FixedBoundFrustum.PlaneCount`; they throw `ArgumentNullException` or
`ArgumentException` before calling Unity when the contract is violated.

The camera overload throws `ArgumentNullException` for a null camera. It does
not mutate the camera.

The matrix overload passes the Unity matrix directly to
`GeometryUtility.CalculateFrustumPlanes`. It does not route through
`Matrix4x4.ToFixed4x4()` or the core matrix constructor because the purpose is
to preserve Unity's projection-space interpretation.

### AnimationCurve bridge

Create `Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs` with a
`FixedCurveUnityExtensions` class in `FixedMathSharp`:

```csharp
public static bool TryToFixedCurve(
    this AnimationCurve curve,
    out FixedCurve fixedCurve);

public static FixedCurve ToFixedCurve(this AnimationCurve curve);

public static FixedCurve BakeToFixedCurve(
    this AnimationCurve curve,
    int sampleCount);

public static AnimationCurve ToAnimationCurve(this FixedCurve curve);
```

`TryToFixedCurve` returns `false` for a valid Unity object whose behavior cannot
be represented exactly by `FixedCurve`. A null receiver remains a programming
error and throws `ArgumentNullException`. On failure, the out value is a safe
constant-zero curve rather than `default(FixedCurve)`, whose key array is null.
Expected fixed conversion failures, including non-finite or out-of-Q32.32
times, values, and scaled tangents, are representability failures and return
`false`; they do not escape from the `Try` API.

`ToFixedCurve` delegates to the `Try` method and throws
`InvalidOperationException` with guidance to use `BakeToFixedCurve` when the
curve is not structurally representable.

`BakeToFixedCurve` is intentionally not a `Try` API. Invalid arguments or
non-finite/unrepresentable sampled values throw at the boundary so bad authored
data is not silently accepted.

## Frustum Semantics

Unity returns planes in this order:

1. left
2. right
3. down
4. up
5. near
6. far

Unity's calculated planes face inward for `GeometryUtility` containment tests.
`FixedBoundFrustum` stores outward-facing planes and considers a positive plane
distance outside. Every frustum conversion therefore negates both the normal
and distance component.

Unity-to-fixed construction reorders and negates the planes before calling:

```csharp
new FixedBoundFrustum(
    near,
    far,
    left,
    right,
    top,
    bottom);
```

Fixed-to-Unity conversion performs the inverse operation and writes the Unity
order. The existing component-preserving `Plane.ToFixedPlane()` and
`FixedPlane.ToPlane()` methods remain unchanged; frustum orientation reversal
is private to the frustum adapter because it is a containment convention, not
a general plane conversion rule.

Degenerate planes, invalid projection matrices, or fixed-domain conversion
failures propagate the specific exception from Unity or FixedMathSharp. The
adapter does not return a partially initialized frustum.

## Exact Curve Conversion

### Unity to FixedMathSharp

An `AnimationCurve` is exactly representable only when all of these conditions
hold:

- Both `preWrapMode` and `postWrapMode` are exactly `WrapMode.ClampForever`.
  Looping, ping-pong, one-shot, and ambient/default wrap behavior are rejected.
- Every key uses `WeightedMode.None`.
- Key times and values are finite and representable as `Fixed64`.
- Key times are strictly increasing both as authored Unity floats and after
  Q32.32 conversion. Distinct float times that collapse to the same fixed time
  are rejected.
- A stepped segment has both the left key's outgoing tangent and the right
  key's incoming tangent set to positive infinity. Every segment must either
  meet that rule, or every segment must have finite Hermite tangents. Mixed
  stepped/Hermite curves and partial/non-positive infinity markers are
  rejected because `FixedCurveMode` is global.
- Duration-scaled tangents remain representable as `Fixed64`.

A wholly stepped Unity curve becomes `FixedCurveMode.Step`. Its fixed tangents
are zero because that mode ignores them.

Every other representable Unity curve becomes `FixedCurveMode.Cubic`. Linear,
smooth, and freely authored unweighted Unity curves are all cubic Hermite
segments and do not need lossy mode inference.

Unity tangents are slopes in value-per-time units. `FixedCurve` passes its
tangents to a Hermite polynomial whose segment parameter runs from zero to one.
Conversion therefore scales tangents by the adjacent segment duration:

```text
fixed out tangent[i] = unity out tangent[i] * (time[i + 1] - time[i])
fixed in tangent[i]  = unity in tangent[i]  * (time[i] - time[i - 1])
```

The first incoming and last outgoing tangent are unused and become zero.
Products are calculated in `double` from the authored Unity floats, then
converted once with `Fixed64.FromDouble`; no saturating fixed multiplication is
used during representability checks.

An empty Unity curve evaluates as zero, while an empty constructed
`FixedCurve` evaluates as one. Exact conversion therefore represents an empty
Unity curve as a one-key constant-zero `FixedCurve`. A one-key Unity curve
becomes a one-key constant fixed curve.

### FixedMathSharp to Unity

`ToAnimationCurve` supports every defined `FixedCurveMode`:

- `Linear`: adjacent endpoint slopes are assigned to each segment.
- `Step`: the relevant Unity tangents use positive infinity.
- `Smooth`: incoming and outgoing slopes are zero.
- `Cubic`: normalized fixed tangents are divided by the emitted Unity segment
  duration.

All emitted keys use `WeightedMode.None`, and both output wrap modes are set to
`WrapMode.ClampForever`.

Key times are converted to `float` before tangent slopes are created. They must
remain finite and strictly increasing after float narrowing; otherwise the
method throws `InvalidOperationException` rather than allowing Unity to merge
or reorder keys silently.

The method also throws `InvalidOperationException` for an undefined
`FixedCurveMode` or a default `FixedCurve` whose `Keyframes` property is null.
An explicitly constructed zero-key `FixedCurve`, whose evaluation result is
one, becomes a one-key constant-one Unity curve.

## Sampled Curve Baking

`BakeToFixedCurve(sampleCount)` samples uniformly over the inclusive range from
the first Unity key time to the last and returns a `FixedCurveMode.Linear`
curve containing those samples.

- `sampleCount` is the total number of generated keys, including both
  endpoints, and must be at least two.
- An empty Unity curve becomes a one-key constant-zero fixed curve.
- A one-key Unity curve becomes a one-key constant fixed curve.
- Multiple keys with no positive authored time span are rejected.
- Weighted tangents, mixed interpolation, and wrap modes do not block baking.
  Only the authored first-to-last key range is sampled; out-of-range wrap
  behavior is intentionally not reproduced.
- Non-finite or out-of-Q32.32 sampled times or values throw.

Baking calls Unity's floating-point evaluator and is not itself a deterministic
simulation operation. Consumers should bake once during authoring, loading, or
another controlled boundary and then distribute or persist the resulting fixed
keys. Peers should not independently bake Unity curves during a lockstep run.

## Error Handling

- Null reference arguments throw `ArgumentNullException`.
- Wrong frustum buffer sizes throw `ArgumentException` before mutation.
- Exact curve representability failures return `false` from
  `TryToFixedCurve` and throw `InvalidOperationException` from `ToFixedCurve`.
- Numeric conversion uses the existing `Fixed64.FromDouble` contract:
  non-finite values are rejected and finite out-of-range values overflow.
- Baking and reverse conversion never silently clamp, saturate, merge, or drop
  keys.
- Frustum conversion never returns partial plane/corner data.

## Files and Ownership

Create or modify only focused package files:

- Create `Build/Base/Runtime/Extensions/FixedFrustum.Extensions.cs`.
- Create `Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs`.
- Create `Tests/EditMode/FixedFrustumUnityExtensionsTests.cs`.
- Create `Tests/EditMode/FixedCurveUnityExtensionsTests.cs`.
- Update the root and both package READMEs with a concise included-feature note
  and short usage examples.
- Sync the two new runtime files into both package variants through the Unity
  package synchronizer.
- Track Unity-generated `.meta` files for canonical, package, and test assets;
  verify every GUID is unique.

No sample scripts, editor drawers, core DLLs, package manifests, or serializer
surfaces change.

## Testing

### Frustum tests

- Perspective camera conversion classifies known inside and outside points.
- Orthographic camera conversion classifies known inside and outside points.
- A custom world-to-projection matrix produces the same fixed planes through
  the camera and matrix overloads.
- Fixed planes match Unity planes after the documented order and sign mapping.
- Fixed-to-Unity planes work with `GeometryUtility.TestPlanesAABB` and match
  core `FixedBoundFrustum` containment for representative bounds.
- Allocating and caller-buffer overloads produce equivalent frustums.
- Buffer overloads reject null and non-six-element arrays before mutation.
- Camera overloads reject null.

### Curve tests

- Unequal-duration unweighted Hermite segments compare with Unity evaluation
  at endpoints and interior normalized times, proving tangent scaling.
- Linear and smooth Unity curves convert exactly through cubic Hermite data.
- A wholly stepped curve converts to `FixedCurveMode.Step`.
- Weighted, mixed step/Hermite, non-clamped, non-finite, and unrepresentable
  curves, including times that collapse after Q32.32 conversion, fail
  atomically through `TryToFixedCurve`; the throwing wrapper points callers to
  baking.
- Empty and one-key curves preserve constant evaluation semantics.
- Baking emits the requested number of uniformly spaced linear keys, includes
  both endpoints, and matches Unity at every sampled time.
- Baking validates sample count and rejects non-finite/unrepresentable samples.
- `Linear`, `Step`, `Smooth`, and `Cubic` fixed curves convert to Unity and
  match fixed evaluation across unequal-duration segments.
- Reverse conversion rejects null key storage, undefined modes, duplicate
  times, and fixed times that collapse to the same Unity float.

### Package verification

- Run the full EditMode suite against Standard and Lean isolated package
  projects.
- Run package sync twice; the second pass must copy zero files.
- Compare `Build/Base/Runtime/Extensions` with both package copies, excluding
  `.meta` files.
- Verify new `.meta` files exist and all repository GUIDs remain unique.
- Run `git diff --check`.

## Documentation

The public XML comments are the primary API reference. README examples should
show:

```csharp
FixedBoundFrustum frustum = camera.ToFixedBoundFrustum(planeBuffer);

if (!animationCurve.TryToFixedCurve(out FixedCurve curve))
    curve = animationCurve.BakeToFixedCurve(65);
```

The README must state that baking is an explicit approximation and should not
be performed independently by lockstep peers.

## References

- Unity `GeometryUtility.CalculateFrustumPlanes`:
  <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GeometryUtility.CalculateFrustumPlanes.html>
- Unity `WeightedMode`:
  <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WeightedMode.html>
- FixedMathSharp v7 source types: `FixedBoundFrustum`, `FixedPlane`,
  `FixedCurve`, `FixedCurveKey`, and `FixedCurveMode`.
