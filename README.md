FixedMathSharp-Unity
==============

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
- **Unity Integration** – Compatible with **Unity’s Job System** & **Burst Compiler**.

---

## 🚀 Installation

### **Via Unity Package Manager (UPM)**
1. Open **Unity Package Manager** (`Window > Package Manager`).
2. Click **Add package from git URL...**.
3. Enter:

https://github.com/mrdav30/FixedMathSharp-Unity.git

### **Manual Import**
1. Download the latest `FixedMathSharp.unitypackage` from the [Releases](https://github.com/mrdav30/FixedMathSharp-Unity/releases).
2. In Unity, go to **Assets > Import Package > Custom Package...**.
3. Select and import `FixedMathSharp.unitypackage`.

---

## 📖 Usage Examples

### Basic Arithmetic with `Fixed64`:
```csharp
Fixed64 a = new Fixed64(1.5);
Fixed64 b = new Fixed64(2.5);
Fixed64 result = a + b;
Console.WriteLine(result); // Output: 4.0
```

### Vector Operations:
```csharp
Vector3d v1 = new Vector3d(1, 2, 3);
Vector3d v2 = new Vector3d(4, 5, 6);
Fixed64 dotProduct = Vector3d.Dot(v1, v2);
Console.WriteLine(dotProduct); // Output: 32
```

### Quaternion Rotation:
```csharp
FixedQuaternion rotation = FixedQuaternion.FromAxisAngle(Vector3d.Up, FixedMath.PiOver2); // 90 degrees around Y-axis
Vector3d point = new Vector3d(1, 0, 0);
Vector3d rotatedPoint = rotation.Rotate(point);
Console.WriteLine(rotatedPoint); // Output: (0, 0, -1)
```

### Matrix Transformations:
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

### Trigonometry Example:
```csharp
Fixed64 angle = FixedMath.PiOver4; // 45 degrees
Fixed64 sinValue = FixedTrigonometry.Sin(angle);
Console.WriteLine(sinValue); // Output: ~0.707
```

---

## 🛠️ Compatibility

- **.NET Framework:** 4.7.2+
- **Unity3D Version:** 2022.3+
- **Platforms:** Windows, Linux, macOS, WebGL, Mobile

---

## 📄 License

This project is licensed under the MIT License - see the `LICENSE` file
for details.

---

## 👥 Contributors

- **mrdav30** - Lead Developer
- Contributions are welcome! Feel free to submit pull requests or report issues.

---

## 📧 Contact

For questions or support, reach out to **mrdav30** via GitHub or open an issue in the repository.

---