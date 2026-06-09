# FixedMathSharp-Unity Agent Instructions

## Purpose

FixedMathSharp-Unity is the Unity Package Manager host for the
engine-agnostic FixedMathSharp core library. It ships two installable package
variants:

- `com.mrdav30.fixedmathsharp` - standard package with `MemoryPack` support.
- `com.mrdav30.fixedmathsharp.lean` - no-MemoryPack package, preferred for
  Burst AOT-oriented Unity projects.

The actual Git repository root is `Assets/Packages`, not the outer Unity
project root. The outer project exists so Unity can compile, sync, export, and
validate the packages.

The core deterministic math library is maintained in the separate
`FixedMathSharp` repository:

- Git URL: `https://github.com/mrdav30/FixedMathSharp.git`

If the current workspace also contains a local checkout of that core repo, use
its `AGENTS.md` and source files as the source of truth for `Fixed64`, Q32.32
math, coordinate conventions, serialization semantics, performance
expectations, and deterministic behavior. Do not assume a fixed host-specific
path; discover the checkout from the current workspace, git remotes, or user
context.

## Current Package Facts

- Package manifests currently declare version `5.0.0`.
- Package manifests require Unity `2022.3` or newer.
- This Unity host project currently uses Unity `6000.3.9f1` according to
  `../../ProjectSettings/ProjectVersion.txt`.
- Both packages include the precompiled FixedMathSharp plugin assets under
  `Plugins/`.
- The standard package carries the MemoryPack-related dependencies; the Lean
  package intentionally omits MemoryPack.

## Source Of Truth

When shared Unity-managed code changes, edit `Build/Base` first.

`Build/Base` is synchronized into both package variants by
`Build/Editor/FixedMathSharpPackageSync.cs`. Do not hand-edit synced copies in
`com.mrdav30.fixedmathsharp` or `com.mrdav30.fixedmathsharp.lean` unless you
are intentionally changing package-specific files outside the sync set.

The sync-managed paths are:

- `COPYRIGHT`
- `LICENSE`
- `NOTICE`
- `Editor/Utility`
- `Editor/Drawers`
- `Runtime/Attributes`
- `Runtime/Extensions`
- `Samples/FixedMathSharpDemo/Scripts`

Package-specific files live directly in each package and are not copied from
`Build/Base`, including:

- `package.json`
- package `README.md`
- asmdef files
- `Plugins/` DLL, PDB, XML, and dependency assets
- sample scene assets

Keep these aligned whenever package shape, install guidance, package variants,
or exported assets change:

- Root `README.md`
- `.agents/AGENTS.md`
- `Build/Base`
- `Build/Editor/FixedMathSharpPackageSync.cs`
- `Build/Editor/FixedMathSharpUnityPackageExporter.cs`
- `com.mrdav30.fixedmathsharp/**`
- `com.mrdav30.fixedmathsharp.lean/**`

## Repository Map

| Path | Purpose |
| --- | --- |
| `Build/Base` | Shared managed Unity code copied into both package variants. |
| `Build/Editor` | Unity editor tooling for syncing and exporting packages. |
| `com.mrdav30.fixedmathsharp` | Standard UPM package with MemoryPack support. |
| `com.mrdav30.fixedmathsharp.lean` | Lean UPM package without MemoryPack. |
| `Build/Base/Runtime/Extensions` | Shared Unity adapter helpers for transforms, matrices, bounds, vectors, and quaternions. |
| `Build/Base/Runtime/Attributes` | Shared Unity-facing attributes such as fixed-number angle and vector rotation attributes. |
| `Build/Base/Editor/Drawers` | Shared Unity inspector drawers for FixedMathSharp types and Unity-facing attributes. |
| `Build/Base/Editor/Utility` | Shared editor helpers, including serialized-property reflection utilities. |
| `Build/Base/Samples/FixedMathSharpDemo` | Shared demo scripts copied into both package variants. |

Ignore generated Unity output when reasoning about package source:

- `../../Library/`
- `../../Temp/`
- `../../Obj/`
- `UnityPackageExports~`

## FixedMathSharp Core Rules Still Apply

Unity integration code is an adapter layer. It should not weaken the core
library's deterministic design.

- Prefer optimized, low time-complexity code. No band-aid solutions.
- Keep deterministic runtime paths in fixed-point land. Convert Unity
  `float`/`double` values at adapter boundaries into `Fixed64`, `Vector2d`,
  `Vector3d`, `FixedQuaternion`, or matrix/bounds types.
- Do not introduce ambient time, ambient randomness, hidden Unity state, or
  platform-specific floating-point shortcuts into deterministic data paths.
- Treat Unity coordinate and transform conventions as boundary concerns.
  FixedMathSharp core currently uses `+X` right, `+Y` up, `+Z` forward and
  row-vector transform semantics.
- Do not add engine assumptions back into the core DLL or docs. This repo may
  provide Unity conversions, but core FixedMathSharp remains engine-agnostic.
- Preserve serialization layout and package compatibility unless the task is
  explicitly a breaking package update.

## Unity Adapter And Editor Guidance

- Runtime extension methods should remain thin conversion/adaptation layers
  between Unity types and FixedMathSharp types.
- Editor drawers must be robust against FixedMathSharp value-type changes.
  Avoid relying on Unity exposing nested `Fixed64.m_rawValue` as an editable
  `SerializedProperty`; use the shared reflection helpers when whole-value
  writes are required.
- Be careful with readonly or immutable structs. Setting a nested field on a
  boxed struct does not update the owning serialized object unless the whole
  path is written back.
- Keep editor-only code under `Editor/` and runtime package code under
  `Runtime/`. Editor asmdefs must include only the `Editor` platform.
- Do not make the Lean package depend on MemoryPack or MemoryPack-specific
  runtime APIs.
- Keep both package variants behaviorally aligned except for the intentional
  MemoryPack dependency difference.

## Sync And Verification Commands

Run package sync through Unity after editing `Build/Base`:

```bash
UNITY_PROJECT_ROOT="$(cd ../.. && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-Unity}"

"$UNITY_EDITOR" \
  -batchmode -quit \
  -projectPath "$UNITY_PROJECT_ROOT" \
  -executeMethod FixedMathSharp.Build.Editor.FixedMathSharpPackageSync.SyncPackagesBatchMode \
  -logFile -
```

Run the same command again when the first pass copied package files and you
need evidence that the generated package assemblies compile with the final
synced contents. A clean final sync should report `Copied 0 files`.

To export `.unitypackage` archives:

```bash
UNITY_PROJECT_ROOT="$(cd ../.. && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-Unity}"

"$UNITY_EDITOR" \
  -batchmode -quit \
  -projectPath "$UNITY_PROJECT_ROOT" \
  -executeMethod FixedMathSharp.Build.Editor.FixedMathSharpUnityPackageExporter.ExportUnityPackagesBatchMode \
  -FixedMathSharpUnityPackageOutputPath UnityPackageExports~ \
  -logFile -
```

Set `UNITY_EDITOR` to the local Unity executable when Unity is not available as
`Unity` on `PATH`.

The exporter calls the sync step before exporting.

Useful lightweight checks:

```bash
git diff --check

for f in Editor/Utility Editor/Drawers Runtime/Attributes Runtime/Extensions Samples/FixedMathSharpDemo/Scripts; do
  diff -qr -x '*.meta' "Build/Base/$f" "com.mrdav30.fixedmathsharp/$f"
  diff -qr -x '*.meta' "Build/Base/$f" "com.mrdav30.fixedmathsharp.lean/$f"
done
```

There is no broad .NET solution in this Unity package repo. Unity batch
compilation is the main compile verification for package/editor changes.

## Updating The Core DLLs

When bumping the FixedMathSharp core library:

- Start from the sibling core repo and verify the intended version or commit
  there.
- Update both package variants' `Plugins/FixedMathSharp.dll`,
  `Plugins/FixedMathSharp.pdb`, and `Plugins/FixedMathSharp.xml` as
  appropriate.
- Keep standard-package dependency DLLs aligned with the MemoryPack-enabled
  build.
- Keep the Lean package free of MemoryPack dependencies.
- Update both `package.json` files, package READMEs, and the root README when
  the public version, install guidance, dependencies, or variant behavior
  changes.
- Re-run Unity batch sync/compile after plugin or package metadata changes.

## Documentation Guidance

- Keep the root README focused on package selection and Git URL installation.
- Keep package READMEs package-specific: standard explains MemoryPack support,
  Lean explains the no-MemoryPack/Burst AOT rationale.
- Do not duplicate long core FixedMathSharp API documentation here. Link to or
  align with the core repo instead.
- If a Unity package behavior depends on a core FixedMathSharp convention,
  mention the adapter boundary clearly rather than reframing it as a Unity
  rule.

## Agent Editing Guidance

- Make focused edits and preserve unrelated dirty files.
- For shared managed code, patch `Build/Base` first, then sync.
- For package-specific metadata/assets, edit each package intentionally.
- Read the corresponding generated package copy after sync if a Unity import or
  compiler error points there.
- Prefer explicit validation evidence over assumptions, especially around
  editor drawers, serialization, Burst/Lean differences, and plugin updates.
