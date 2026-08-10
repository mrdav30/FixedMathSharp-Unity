---
title: Serialization
description: Choose between the Standard and Lean packages and persist FixedMathSharp values safely in Unity.
---

# Serialization

Serialization is the only behavioral difference between the Standard and Lean
package variants. Their FixedMathSharp and Unity adapter APIs otherwise match.

## Package matrix

| Capability | Standard | Lean |
| --- | --- | --- |
| Unity field/YAML serialization for tested authoring types | Yes | Yes |
| MemoryPack attributes present | Yes | Shape-compatible attributes only |
| MemoryPack runtime serialization | Yes | No |
| Custom serialization stacks | Yes | Yes |
| Runtime dependency | `MemoryPack.Core` | `Chronicler.MemoryPackShim` |

Install exactly one variant. The packages contain the same core assembly and
cannot coexist in a Unity project.

## Unity field serialization

Unity persistence does not depend on the MemoryPack choice. The package's
EditMode coverage verifies non-zero values across:

- serialized `Fixed64`, `Vector2d`, `Vector3d`, and `Vector4d` fields;
- nested serializable structs and list elements;
- ScriptableObject save/reload; and
- prefab save/reload, including edits to already-created assets.

The core `Fixed64.m_rawValue` field is intentionally public and mutable so
field-based serializers can persist the exact Q32.32 payload without adding a
Unity dependency to the core library.

Treat direct access to `m_rawValue` as serializer or bootstrap-level code.
Normal gameplay code should construct values through `Fixed64.FromRaw`,
`Fixed64.FromDouble`, casts, constants, and operators.

```csharp
using FixedMathSharp;
using UnityEngine;

public sealed class MovementSettings : ScriptableObject
{
    [SerializeField]
    private Fixed64 speed = Fixed64.FromDouble(4.5);

    [SerializeField]
    private Vector3d spawn = Vector3d.FromDouble(0.0, 1.0, 0.0);
}
```

Use `[SerializeField]` and normal Unity field-serialization rules. Properties,
unsupported containers, and other exclusions remain subject to Unity's own
[serialization rules](https://docs.unity3d.com/Manual/script-serialization-rules.html).

## Standard: MemoryPack included

The Standard package includes the MemoryPack runtime used by the bundled core
assembly. Use it when your project already uses MemoryPack or you want the
library's built-in binary serialization path.

```csharp
using FixedMathSharp;
using MemoryPack;

Vector3d source = Vector3d.FromDouble(1.25, -2.0, 8.5);
byte[] payload = MemoryPackSerializer.Serialize(source);
Vector3d restored = MemoryPackSerializer.Deserialize<Vector3d>(payload);
```

Keep network or replay envelopes explicitly versioned. A serializer can retain
the raw value exactly, but it does not by itself define message ordering,
compatibility policy, or deterministic application order.

## Lean: bring your own serializer

Lean removes `MemoryPack.Core` and includes `Chronicler.MemoryPackShim` only so
the Lean core DLL can resolve its MemoryPack attribute shapes. The shim is not a
serializer. Calling MemoryPack runtime APIs is not supported by the Lean
package.

For a custom binary format, encode the fixed raw payload and rebuild it
explicitly:

```csharp
Fixed64 value = Fixed64.FromDouble(12.75);
long raw = value.m_rawValue;

// Write and read `raw` with your chosen deterministic format.
Fixed64 restored = Fixed64.FromRaw(raw);
```

For vectors, quaternions, matrices, and compound values, serialize fields in an
explicit, stable order. Define byte order and schema version in the format
rather than relying on reflection or runtime member order.

## Inspector angle attributes

The editor includes two convenience attributes whose displayed value differs
from the stored shape:

- `[VectorRotation]` displays a `Vector2d` as an angle and writes a unit
  direction. Editing it does not preserve the vector's old magnitude.
- `[FixedNumberAngle]` displays an angle in degrees but stores its sine in the
  `Fixed64` field. Serialize the fixed sine value, not the displayed degrees.

These rules matter when authoring assets outside Unity or comparing YAML with
runtime data.

## Deterministic persistence checklist

- Serialize exact fixed raw values, not display strings or converted floats.
- Specify schema version, field order, and byte order for custom formats.
- Keep collection ordering stable before writing network or replay data.
- Test non-zero, negative, minimum/maximum, nested, and collection values.
- Validate old payloads before changing a released layout.

For core representation and overflow behavior, see the
[FixedMathSharp wiki](https://github.com/mrdav30/FixedMathSharp/wiki).
