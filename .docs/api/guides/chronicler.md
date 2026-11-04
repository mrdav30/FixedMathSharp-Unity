---
title: Chronicler integration
description: Install the matching optional companion for deterministic hashes, exact time conversions, and Chronicler recording and restore APIs.
---

# Chronicler integration

The optional companions connect FixedMathSharp values to Chronicler. Each
contains `FixedMathSharp.Chronicler` v8 and `Chronicler` v1.0.0 for .NET Standard
2.1. Hash-writing extensions and exact time conversions live in the bridge;
recording and restore APIs live in Chronicler.

## Install a matching pair

Install one math base first, then its companion at the same package version.
The companion does not automatically install its base. Keep Standard and Lean
paired; installing both variants creates duplicate assemblies.

| Math base | Companion | Serialization |
| --- | --- | --- |
| `com.mrdav30.fixedmathsharp` | `com.mrdav30.fixedmathsharp.chronicler` | MemoryPack runtime supplied by the base |
| `com.mrdav30.fixedmathsharp.lean` | `com.mrdav30.fixedmathsharp.chronicler.lean` | Attribute shim supplied by the base; no MemoryPack runtime |

After [installing your math base](getting-started.md#2-install-from-git), add
the matching Git URL through Package Manager:

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.chronicler#v8.0.0
```

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.chronicler.lean#v8.0.0
```

The math base owns `FixedMathSharp.dll`, diagnostics, JSON dependencies, and
MemoryPack or its Lean shim. The companion supplies only the Chronicler and
bridge assemblies, with supporting documentation and symbols. Do not import
the complete upstream release archive alongside these packages; it also
contains the math assembly.

These APIs are in precompiled plugins, so there is no additional Unity runtime
asmdef to reference. Custom asmdefs normally reference plugins automatically.
If **Override References** is enabled, explicitly reference `FixedMathSharp.dll`,
`Chronicler.dll`, and `FixedMathSharp.Chronicler.dll`. Keep the base's runtime
asmdef reference when you also use its Unity adapter methods.

To switch variants, remove the companion and math base, install the other base,
then install its matching companion. Existing custom asmdef references to Unity
adapters must be updated for the selected base.

## Hash fixed-point state

```csharp
using Chronicler.Hashing;
using FixedMathSharp;
using FixedMathSharp.Chronicler;

var writer = new ChronicleHashWriter();
writer.WriteVector3d(Vector3d.FromDouble(1.25, -2.0, 8.5));
writer.WriteQuaternion(FixedQuaternion.Identity);
ChronicleHash hash = writer.ToHash();
```

Write authoritative fields in an explicit, stable order. `WriteTransform`
hashes local position, local rotation, and local scale; parent identity and
derived world values are excluded. Hash those separately if your state contract
requires them. A state hash detects differences; the host still owns snapshot
capture, coordinated restore, and deterministic execution order.

## Convert time exactly

```csharp
using Chronicler.Timing;
using FixedMathSharp;
using FixedMathSharp.Chronicler;

var start = new ChronicleTimestamp(3_155_760_000L, 0);
var end = start + FixedChronicleTime.FromFixed64(Fixed64.FromRaw(0x40000000));
Fixed64 elapsed = FixedChronicleTime.ToFixed64(end - start); // Exactly 0.25 seconds.
```

Subtract wide timestamps before narrowing their difference. `TryToFixed64`
returns false and zero when a duration cannot fit; `ToFixed64` throws rather
than saturating. `GetFrameCountForDuration` counts complete represented steps:
one second contains 29 steps at a rounded 1/30-second tick, because that tick
is slightly longer than the exact fraction.

See the [upstream companion guide](https://github.com/mrdav30/FixedMathSharp/blob/main/src/FixedMathSharp.Chronicler/README.md)
and [Chronicler documentation](https://mrdav30.github.io/Chronicler/) for the
underlying APIs. Unity EditMode validation does not establish IL2CPP or Burst
compatibility; validate the runtime and backend used by your application.
