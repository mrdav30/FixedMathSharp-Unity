# FixedMathSharp-Unity Hardening Plan

## Goals

- Improve day-to-day Unity ergonomics without weakening the core library's deterministic intent.
- Prefer explicit APIs over ambiguous convenience wrappers.
- Keep runtime interop lightweight and predictable.
- Focus on editor/debugging value before broader Unity-specific integrations.

## Current Baseline

Completed:

- `Vector2d`, `Vector3d`, and `FixedQuaternion` Unity conversion extensions.
- Explicit `Transform` conversion helpers for:
  - `Transform -> Fixed4x4` world/local
  - `Transform -> Fixed3x3` world/local rotation-only
  - `Transform -> Fixed3x3` world/local rotation-scale

Current principle:

- `Fixed4x4` conversions should be explicit about world vs local.
- `Fixed3x3` conversions should be explicit about rotation-only vs rotation-scale.
- Avoid ambiguous APIs like bare `ToFixed3x3()` when the source data contains scale.

## Priority Roadmap

### 1. Reverse Matrix -> Transform Helpers

Priority: Highest
Status: Completed

Why this matters:

- This is the most natural companion to the new `Transform -> Fixed*` helpers.
- It unlocks real workflows instead of one-way conversion only.
- It will also make sample/demo code much easier to write and verify.

Planned API direction:

- `fixedMatrix.ApplyToTransformWorld(transform)`
- `fixedMatrix.ApplyToTransformLocal(transform)`
- `fixedMatrix.ToTransformWorld(...)`
- `fixedMatrix.ToTransformLocal(...)`
- `fixedMatrix3x3.ApplyRotationToTransformWorld(transform)`
- `fixedMatrix3x3.ApplyRotationToTransformLocal(transform)`
- `fixedMatrix3x3.ApplyRotationScaleToTransformWorld(transform)`
- `fixedMatrix3x3.ApplyRotationScaleToTransformLocal(transform)`
- `fixedMatrix3x3.ToTransformRotationWorld(...)`
- `fixedMatrix3x3.ToTransformRotationLocal(...)`
- `fixedMatrix3x3.ToTransformRotationScaleWorld(...)`
- `fixedMatrix3x3.ToTransformRotationScaleLocal(...)`

Implementation notes:

- `Fixed4x4` should decompose into translation, rotation, and scale before applying to the Unity `Transform`.
- `Fixed3x3` should never imply translation.
- Rotation-only `Fixed3x3` helpers should only touch rotation.
- Rotation-scale `Fixed3x3` helpers should touch rotation and scale, but remain explicit in naming.

Risks / cautions:

- `Transform.lossyScale` is approximate in Unity, especially through skewed parent hierarchies.
- Applying world-space scale can be tricky when the transform has a rotated or non-uniformly scaled parent.
- If world-space matrix decomposition behaves poorly in parented hierarchies, we may want to prefer local-space application APIs first.

Definition of done:

- Local-space reverse helpers are solid and predictable.
- World-space variants are included if they are behaviorally trustworthy.
- XML docs clearly call out any `lossyScale` limitations.

Implementation notes captured:

- `Fixed4x4` world-space application is handled by converting the target world matrix into a parent-relative local matrix when the transform is parented.
- `Fixed3x3` world-space application follows the same pattern using parent-relative inverse rotation or rotation-scale matrices.
- World-space application throws when the parent matrix is not invertible, rather than silently applying incorrect results.
- Factory-style helpers now exist for callers that want a brand-new Unity `Transform` instead of mutating an existing one.

### 2. Fixed4x4 / Fixed3x3 <-> Unity Matrix Interop

Priority: High

Why this matters:

- Matrix interop complements the transform helpers and gives us a debugging bridge into Unity tooling.
- It provides a lower-level route when callers already have `Matrix4x4` data.

Planned API direction:

- `fixed4x4.ToMatrix4x4()`
- `matrix4x4.ToFixed4x4()`
- `fixed3x3.ToMatrix4x4()` or `ToRotationMatrix4x4()`
- `matrix4x4.ToFixed3x3RotationMatrix()`
- `matrix4x4.ToFixed3x3RotationScaleMatrix()`

Implementation notes:

- Prefer explicit names when scale is involved.
- `Fixed3x3` interop should be careful not to silently treat a general 4x4 as a pure rotation matrix.
- If we expose a `Fixed3x3 -> Matrix4x4` conversion, it should clearly define whether it fills only the upper-left 3x3 and leaves translation as zero.

Definition of done:

- Common round-trip conversions are available and documented.
- Conversion semantics are clear for rotation-only vs rotation-scale cases.

### 3. Bounds Interop

Priority: High

Why this matters:

- This is one of the most practical missing bridges for Unity users working with scenes, gameplay zones, or gizmos.
- `BoundingBox` maps especially well to Unity's `Bounds`.

Planned API direction:

- `BoundingBox.ToBounds()`
- `Bounds.ToBoundingBox()`
- Optional helper methods for center/extents conversions where useful

Open question:

- Unity has no built-in `BoundingSphere`, so sphere interop may need to stay as helper methods rather than a direct type conversion.

Definition of done:

- `BoundingBox <-> Bounds` is easy and obvious.
- No confusing or partial `BoundingSphere` API is introduced just for symmetry.

### 4. Matrix Inspector / Debug Tooling

Priority: Medium

Why this matters:

- Debuggability is one of the biggest wins for a Unity integration package.
- Matrix types are hard to inspect without dedicated UI.

Planned scope:

- Read-only drawer for `Fixed3x3`
- Read-only drawer for `Fixed4x4`
- Optional formatting helpers for cleaner display

Implementation notes:

- Start read-only unless there is a very strong reason to make matrix fields editable.
- Read-only inspectors reduce the chance of accidental invalid states while still helping users debug.

Definition of done:

- Matrices render clearly in the inspector.
- Display format is compact and useful for debugging.

### 5. Demo Script / Sample Scene

Priority: Medium

Why this matters:

- A tiny sample does more for onboarding than a long README section.
- It gives us a manual verification loop outside of ad hoc testing.

Suggested sample:

- A `MonoBehaviour` that mirrors a `Transform` into fixed-space and logs/displays:
  - local/world `Fixed4x4`
  - local/world `Fixed3x3`
  - reconstructed Unity values from reverse helpers

Nice-to-have:

- Gizmo drawing for axes or bounds
- A simple “round-trip” validation component

Definition of done:

- New users can import the package, attach one component, and immediately understand the Unity interop story.

## Out Of Scope For Now

- Rigidbody or Unity physics wrappers
- Broad gameplay helpers unrelated to math interop
- Large editor frameworks beyond targeted inspectors and debug tooling

Reasoning:

- The package is strongest when it stays focused on deterministic math interop and debugging support.

## Recommended Execution Order

1. Reverse matrix -> transform helpers
2. Matrix4x4 / Fixed4x4 / Fixed3x3 interop
3. Bounds interop
4. Matrix drawers
5. Demo script / sample scene

## Validation Checklist Per Task

- Build/compile the runtime/editor assembly cleanly.
- Open in Unity and confirm the new APIs appear where expected.
- Compare converted values against Unity equivalents using simple known transforms.
- Test parented transforms for any world/local-space helper that touches scale.
- Document any Unity-specific caveats instead of hiding them.
