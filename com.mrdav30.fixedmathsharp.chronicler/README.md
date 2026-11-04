# FixedMathSharp.Chronicler for Unity — Standard

Optional deterministic hashing and exact simulation-time conversions for the
Standard FixedMathSharp math base, with Chronicler recording and restore APIs.
Requires Unity 2022.3 or newer.

[Integration guide](https://mrdav30.github.io/FixedMathSharp-Unity/guides/chronicler.html) ·
[Changelog](CHANGELOG.md)

## Required pairing

Install the matching math base first:

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp#v8.0.0
```

Then install this companion:

```text
https://github.com/mrdav30/FixedMathSharp-Unity.git?path=/com.mrdav30.fixedmathsharp.chronicler#v8.0.0
```

The companion does not install its base automatically. Keep both packages at
the same version and do not mix Standard and Lean variants. To switch, remove
the companion and base, then install the other matching pair.

## Included

- `FixedMathSharp.Chronicler` v8 for deterministic hash writing and exact time conversions
- `Chronicler` v1.0.0 for recording, restore, hashes, and wide simulation time
- .NET Standard 2.1 plugins with supporting documentation and bridge symbols

The math base owns `FixedMathSharp.dll`, diagnostics, JSON dependencies, and
MemoryPack or its Lean attribute shim. This companion does not duplicate those
assemblies. Standard uses the base's MemoryPack runtime; Lean supplies no
MemoryPack runtime serialization.

There is no additional Unity runtime asmdef. Plugins are normally referenced
automatically; custom asmdefs with **Override References** enabled must include
`FixedMathSharp.dll`, `Chronicler.dll`, and `FixedMathSharp.Chronicler.dll`.

## Hash fixed-point state

```csharp
using Chronicler.Hashing;
using FixedMathSharp;
using FixedMathSharp.Chronicler;

var writer = new ChronicleHashWriter();
writer.WriteVector3d(Vector3d.FromDouble(1.25, -2.0, 8.5));
ChronicleHash hash = writer.ToHash();
```

Hash fields in a stable order. The host owns snapshot capture, coordinated
restore, and execution order. See the integration guide for wide-time examples
and the upstream API references. These packages do not include separate Unity
samples or claim blanket IL2CPP/Burst compatibility.
