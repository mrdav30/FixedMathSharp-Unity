# FixedMathSharp-Unity

![FixedMathSharp Icon](https://raw.githubusercontent.com/mrdav30/fixedmathsharp/main/icon.png)

A **deterministic fixed-point math** library for **Unity** and **.NET**, designed for **lockstep simulation**, **multiplayer determinism**, and **floating-point-free precision**.

This package is a Unity-specific implementation of the [FixedMathSharp](https://github.com/mrdav30/FixedMathSharp) library

---

## 🛠️ Key Features

- **Fully Deterministic** – Eliminates floating-point errors for lockstep simulations.
- **Fixed-Point Arithmetic** – Implements `Fixed64` with high precision.
- **Math & Trigonometry** – Optimized `FixedMath` and `FixedTrigonometry` utilities.
- **Vector & Matrix Support** – Includes `Vector2d`, `Vector3d`, `FixedQuaternion`, and `Fixed4x4`.
- **Bounding Volume Utilities** – Use `BoundingBox`, `BoundingSphere`, and `BoundingArea` for collision checks.
- **Unity Conversion Helpers** – Convert between Unity `Transform`, `Matrix4x4`, `Bounds`, and FixedMathSharp matrix and bounds types.
- **Round-Trip Transform Interop** – Capture Unity transform data into `Fixed4x4`/`Fixed3x3`, then apply it back onto preview transforms for validation or tooling.
- **Editor Debugging Support** – Includes inspector support for viewing `Fixed3x3` and `Fixed4x4` data more easily in Unity.
- **Unity Integration** – Compatible with **Unity’s Job System** & **Burst Compiler**.
- **Full Serialization Support:** Out-of-the-box round-trip serialization via `MemoryPack` across all serializable structs.

---

## 🚀 Installation

### **Via Unity Package Manager (UPM)**

1. Open **Unity Package Manager** (`Window > Package Manager`).
2. Click **Add package from git URL...**.
3. Enter:

<https://github.com/mrdav30/FixedMathSharp-Unity.git>

### **Manual Import**

1. Download the latest `FixedMathSharp.unitypackage` from the [Releases](https://github.com/mrdav30/FixedMathSharp-Unity/releases).
2. In Unity, go to **Assets > Import Package > Custom Package...**.
3. Select and import `FixedMathSharp.unitypackage`.

---

## 📖 Usage Examples

### Basic Arithmetic with `Fixed64`

```csharp
Fixed64 a = new Fixed64(1.5);
Fixed64 b = new Fixed64(2.5);
Fixed64 result = a + b;
Console.WriteLine(result); // Output: 4.0
```

### Vector Operations

```csharp
Vector3d v1 = new Vector3d(1, 2, 3);
Vector3d v2 = new Vector3d(4, 5, 6);
Fixed64 dotProduct = Vector3d.Dot(v1, v2);
Console.WriteLine(dotProduct); // Output: 32
```

### Quaternion Rotation

```csharp
FixedQuaternion rotation = FixedQuaternion.FromAxisAngle(Vector3d.Up, FixedMath.PiOver2); // 90 degrees around Y-axis
Vector3d point = new Vector3d(1, 0, 0);
Vector3d rotatedPoint = rotation.Rotate(point);
Console.WriteLine(rotatedPoint); // Output: (0, 0, -1)
```

### Matrix Transformations

```csharp
Fixed4x4 matrix = Fixed4x4.Identity;
Vector3d position = new Vector3d(1, 2, 3);
matrix.SetTransform(position, Vector3d.One, FixedQuaternion.Identity);
Console.WriteLine(matrix);
```

### Bounding Shapes and Intersection

```csharp
BoundingBox box = new BoundingBox(new Vector3d(0, 0, 0), new Vector3d(5, 5, 5));
BoundingSphere sphere = new BoundingSphere(new Vector3d(3, 3, 3), new Fixed64(1));
bool intersects = box.Intersects(sphere);
Console.WriteLine(intersects); // Output: True
```

### Trigonometry Example

```csharp
Fixed64 angle = FixedMath.PiOver4; // 45 degrees
Fixed64 sinValue = FixedTrigonometry.Sin(angle);
Console.WriteLine(sinValue); // Output: ~0.707
```

### Unity Transform Interop

```csharp
Transform source = transform;

Fixed4x4 worldMatrix = source.ToFixed4x4WorldMatrix();
Fixed3x3 worldRotation = source.ToFixed3x3WorldRotationMatrix();
Fixed3x3 worldRotationScale = source.ToFixed3x3WorldRotationScaleMatrix();
```

```csharp
worldMatrix.ApplyToTransformWorld(targetTransform);
worldRotation.ApplyRotationToTransformWorld(rotationOnlyTarget);
worldRotationScale.ApplyRotationScaleToTransformWorld(rotationScaleTarget);
```

### Unity Matrix Interop

```csharp
Matrix4x4 unityMatrix = transform.localToWorldMatrix;

Fixed4x4 fixedMatrix = unityMatrix.ToFixed4x4();
Fixed3x3 fixedRotation = unityMatrix.ToFixed3x3RotationMatrix();
Fixed3x3 fixedRotationScale = unityMatrix.ToFixed3x3RotationScaleMatrix();

Matrix4x4 roundTripMatrix = fixedMatrix.ToMatrix4x4();
```

### Unity Bounds Interop

```csharp
Bounds unityBounds = renderer.bounds;

BoundingBox fixedBoundingBox = unityBounds.ToBoundingBox();
BoundingArea fixedBoundingArea = unityBounds.ToBoundingArea();

Bounds boxRoundTrip = fixedBoundingBox.ToBounds();
Bounds areaRoundTrip = fixedBoundingArea.ToBounds();
```

---

## 🎬 Example Scene

The package includes a ready-to-use demo scene and demo components under `Examples/`:

- `Examples/DemoScene.unity`
- `Examples/FixedTransformInteropDemo.cs`
- `Examples/FixedBoundsInteropDemo.cs`

`DemoScene.unity` is intended to visually validate the new Unity interop helpers:

- `Transform -> Fixed4x4`
- `Transform -> Fixed3x3`
- `Fixed4x4/Fixed3x3 -> Transform`
- `Bounds <-> BoundingBox`
- `Bounds <-> BoundingArea`

The transform demo captures local and world matrix data from a source `Transform`, exposes the fixed values in the inspector, and applies round-tripped results onto preview targets. The bounds demo captures Unity `Bounds`, converts them into FixedMathSharp bounds types, and draws round-tripped gizmos for quick visual comparison.

---

## 🔄 Unity Interop Overview

The current Unity-facing helpers include:

- `Transform` to `Fixed4x4`
  - `ToFixed4x4LocalMatrix()`
  - `ToFixed4x4WorldMatrix()`
- `Transform` to `Fixed3x3`
  - `ToFixed3x3LocalRotationMatrix()`
  - `ToFixed3x3WorldRotationMatrix()`
  - `ToFixed3x3LocalRotationScaleMatrix()`
  - `ToFixed3x3WorldRotationScaleMatrix()`
- `Fixed4x4` to Unity
  - `ToMatrix4x4()`
  - `ApplyToTransformLocal()`
  - `ApplyToTransformWorld()`
  - `ToTransformLocal()`
  - `ToTransformWorld()`
- `Fixed3x3` to Unity
  - `ToMatrix4x4()`
  - `ApplyRotationToTransformLocal()`
  - `ApplyRotationToTransformWorld()`
  - `ApplyRotationScaleToTransformLocal()`
  - `ApplyRotationScaleToTransformWorld()`
  - `ToTransformRotationLocal()`
  - `ToTransformRotationWorld()`
  - `ToTransformRotationScaleLocal()`
  - `ToTransformRotationScaleWorld()`
- `Matrix4x4` to FixedMathSharp
  - `ToFixed4x4()`
  - `ToFixed3x3RotationMatrix()`
  - `ToFixed3x3RotationScaleMatrix()`
- `Bounds` interop
  - `ToBoundingBox()`
  - `ToBoundingArea()`
  - `BoundingBox.ToBounds()`
  - `BoundingArea.ToBounds()`

These helpers are designed to preserve FixedMathSharp’s deterministic math model while still fitting naturally into Unity workflows, editor tooling, and visual debugging.

---

## 🛠️ Compatibility

- **.NET Standard** 2.1
- **Unity3D Version:** 2022.3+
- **Platforms:** Windows, Linux, macOS, WebGL, Mobile

---

## 📄 License

This project is licensed under the MIT License.

See the following files for details:

- LICENSE – standard MIT license
- NOTICE – additional terms regarding project branding and redistribution
- COPYRIGHT – authorship information

---

## 👥 Contributors

- **mrdav30** - Lead Developer
- Contributions are welcome! Feel free to submit pull requests or report issues.

---

## 💬 Community & Support

For questions, discussions, or general support, join the official Discord community:

👉 **[Join the Discord Server](https://discord.gg/mhwK2QFNBA)**

For bug reports or feature requests, please open an issue in this repository.

We welcome feedback, contributors, and community discussion across all projects.

---
