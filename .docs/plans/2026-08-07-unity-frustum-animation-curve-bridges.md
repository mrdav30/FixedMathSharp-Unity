# Unity Frustum and AnimationCurve Bridges Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add explicit Unity camera/frustum and `AnimationCurve` adapters for `FixedBoundFrustum` and `FixedCurve`, including exact rejection rules, opt-in sampling, reverse conversion, documentation, and Standard/Lean package coverage.

**Architecture:** Keep both adapters as thin runtime extension classes under the canonical `Build/Base/Runtime/Extensions` tree. Frustums delegate Unity projection interpretation to `GeometryUtility`, then reverse plane orientation and reorder planes at the boundary; curves use strict structural conversion when semantics fit `FixedCurve`, with a separately named sampled bake path for everything else. Each canonical edit is synchronized into both UPM variants before its tests pass.

**Tech Stack:** C#; Unity 2022.3+ APIs (`Camera`, `Matrix4x4`, `Plane`, `GeometryUtility`, `AnimationCurve`, `Keyframe`); FixedMathSharp v7 (`Fixed64`, `FixedCurve`, `FixedCurveKey`, `FixedCurveMode`, `FixedBoundFrustum`, `FixedPlane`); NUnit EditMode tests; Unity 6000.5.0f1 batch mode.

## Global Constraints

- Edit shared runtime code under `Build/Base` first, then run `FixedMathSharpPackageSync.SyncPackagesBatchMode`; do not hand-edit synchronized package copies.
- Support Unity 2022.3 and newer; validate with `C:\Program Files\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe`.
- Compile and test both `com.mrdav30.fixedmathsharp` and `com.mrdav30.fixedmathsharp.lean` without adding package dependencies.
- Keep floating-point evaluation at the Unity boundary; returned fixed objects must use only FixedMathSharp deterministic operations.
- Give every public method concise XML documentation, including non-obvious parameters, return values, rejection rules, and exceptions.
- Preserve the existing general `Plane` conversions. Frustum-specific sign reversal belongs only in `FixedFrustumUnityExtensions`.
- Do not add camera controllers, culling systems, editor drawers, serialized curve assets, sample changes, core DLL changes, manifest changes, or serializer changes.
- Track Unity-generated `.meta` files for every new canonical, package, and test asset, with repository-wide unique GUIDs.

The frustum and curve work are independently reviewable tasks, but remain in one plan because they share the same approved boundary-adapter release, synchronization flow, final documentation, and Standard/Lean verification.

---

### Task 1: Add the Unity frustum bridge

**Files:**
- Create: `Tests/EditMode/FixedFrustumUnityExtensionsTests.cs`
- Create: `Tests/EditMode/FixedFrustumUnityExtensionsTests.cs.meta` (Unity-generated)
- Create: `Build/Base/Runtime/Extensions/FixedFrustum.Extensions.cs`
- Create: `Build/Base/Runtime/Extensions/FixedFrustum.Extensions.cs.meta` (Unity-generated)
- Create through sync: `com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedFrustum.Extensions.cs`
- Create through sync: `com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedFrustum.Extensions.cs.meta` (Unity-generated)
- Create through sync: `com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedFrustum.Extensions.cs`
- Create through sync: `com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedFrustum.Extensions.cs.meta` (Unity-generated)

**Interfaces:**
- Consumes: Unity `GeometryUtility.CalculateFrustumPlanes(Camera, Plane[])`, `GeometryUtility.CalculateFrustumPlanes(Matrix4x4, Plane[])`, existing `Plane.ToFixedPlane()`, existing `FixedPlane.ToPlane()`, and `FixedBoundFrustum`'s near/far/left/right/top/bottom constructor and properties.
- Produces: the six approved `ToFixedBoundFrustum`, `ToUnityFrustumPlanes`, and `CopyToUnityFrustumPlanes` overloads in `FixedMathSharp.Geometry.FixedFrustumUnityExtensions`.

- [ ] **Step 1: Write the failing frustum tests**

Create the test class with perspective, orthographic, matrix, order/sign, reverse-containment, buffer, and null coverage. Use explicit object cleanup so failed assertions do not leak cameras into later tests:

```csharp
using System;
using FixedMathSharp.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedFrustumUnityExtensionsTests
    {
        private const float Tolerance = 0.001f;

        [TestCase(false)]
        [TestCase(true)]
        public void CameraConversion_ClassifiesKnownPoints(bool orthographic)
        {
            GameObject owner = new("FixedMathSharp Frustum Test");
            try
            {
                Camera camera = owner.AddComponent<Camera>();
                camera.orthographic = orthographic;
                camera.orthographicSize = 5f;
                camera.fieldOfView = 60f;
                camera.aspect = 1f;
                camera.nearClipPlane = 0.3f;
                camera.farClipPlane = 20f;

                FixedBoundFrustum frustum = camera.ToFixedBoundFrustum();

                Assert.That(frustum.Contains(Vector3d.FromDouble(0, 0, 2)), Is.EqualTo(FixedEnclosureType.Contains));
                Assert.That(frustum.Contains(Vector3d.FromDouble(100, 0, 2)), Is.EqualTo(FixedEnclosureType.Disjoint));
                Assert.That(frustum.Contains(Vector3d.FromDouble(0, 0, 25)), Is.EqualTo(FixedEnclosureType.Disjoint));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void CameraAndMatrixOverloads_UseTheSameWorldToProjectionMatrix()
        {
            GameObject owner = new("FixedMathSharp Custom Projection Test");
            try
            {
                Camera camera = owner.AddComponent<Camera>();
                camera.transform.SetPositionAndRotation(new Vector3(2f, -1f, 3f), Quaternion.Euler(10f, 25f, 0f));
                camera.projectionMatrix = Matrix4x4.Perspective(47f, 1.7f, 0.25f, 75f);
                Matrix4x4 worldToProjection = camera.projectionMatrix * camera.worldToCameraMatrix;

                Assert.That(camera.ToFixedBoundFrustum(), Is.EqualTo(worldToProjection.ToFixedBoundFrustum()));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void PlaneMapping_ReordersAndNegatesUnityPlanes()
        {
            Matrix4x4 matrix = Matrix4x4.Perspective(60f, 1.25f, 0.3f, 50f);
            Plane[] unityPlanes = GeometryUtility.CalculateFrustumPlanes(matrix);
            FixedBoundFrustum fixedFrustum = matrix.ToFixedBoundFrustum();

            AssertInvertedPlane(fixedFrustum.Left, unityPlanes[0]);
            AssertInvertedPlane(fixedFrustum.Right, unityPlanes[1]);
            AssertInvertedPlane(fixedFrustum.Bottom, unityPlanes[2]);
            AssertInvertedPlane(fixedFrustum.Top, unityPlanes[3]);
            AssertInvertedPlane(fixedFrustum.Near, unityPlanes[4]);
            AssertInvertedPlane(fixedFrustum.Far, unityPlanes[5]);
        }

        [Test]
        public void FixedToUnityPlanes_MatchUnityBoundsContainment()
        {
            GameObject owner = new("FixedMathSharp Reverse Frustum Test");
            try
            {
                Camera camera = owner.AddComponent<Camera>();
                camera.fieldOfView = 60f;
                camera.aspect = 1f;
                camera.nearClipPlane = 0.3f;
                camera.farClipPlane = 20f;
                FixedBoundFrustum frustum = camera.ToFixedBoundFrustum();
                Plane[] planes = frustum.ToUnityFrustumPlanes();

                Assert.That(GeometryUtility.TestPlanesAABB(planes, new Bounds(new Vector3(0f, 0f, 2f), Vector3.one)), Is.True);
                Assert.That(GeometryUtility.TestPlanesAABB(planes, new Bounds(new Vector3(100f, 0f, 2f), Vector3.one)), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void BufferOverloads_MatchAllocatingOverloads()
        {
            Matrix4x4 matrix = Matrix4x4.Perspective(50f, 1.5f, 0.2f, 30f);
            Plane[] extractionBuffer = new Plane[FixedBoundFrustum.PlaneCount];
            FixedBoundFrustum frustum = matrix.ToFixedBoundFrustum(extractionBuffer);
            Plane[] outputBuffer = new Plane[FixedBoundFrustum.PlaneCount];

            frustum.CopyToUnityFrustumPlanes(outputBuffer);

            Assert.That(frustum, Is.EqualTo(matrix.ToFixedBoundFrustum()));
            AssertPlanesEqual(outputBuffer, frustum.ToUnityFrustumPlanes());
        }

        [Test]
        public void BufferOverloads_RejectInvalidArraysBeforeMutation()
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            Plane sentinel = new(Vector3.up, 17f);
            Plane[] shortBuffer = { sentinel };

            Assert.Throws<ArgumentNullException>(() => matrix.ToFixedBoundFrustum(null));
            Assert.Throws<ArgumentException>(() => matrix.ToFixedBoundFrustum(shortBuffer));
            Assert.That(shortBuffer[0].normal, Is.EqualTo(sentinel.normal));
            Assert.That(shortBuffer[0].distance, Is.EqualTo(sentinel.distance));
            Assert.Throws<ArgumentNullException>(() => default(FixedBoundFrustum).CopyToUnityFrustumPlanes(null));
            Assert.Throws<ArgumentException>(() => default(FixedBoundFrustum).CopyToUnityFrustumPlanes(shortBuffer));
        }

        [Test]
        public void CameraOverloads_RejectNull()
        {
            Camera camera = null;
            Assert.Throws<ArgumentNullException>(() => camera.ToFixedBoundFrustum());
            Assert.Throws<ArgumentNullException>(() => camera.ToFixedBoundFrustum(new Plane[FixedBoundFrustum.PlaneCount]));
        }

        private static void AssertInvertedPlane(FixedPlane actual, Plane expected)
        {
            Assert.That((float)actual.Normal.X, Is.EqualTo(-expected.normal.x).Within(Tolerance));
            Assert.That((float)actual.Normal.Y, Is.EqualTo(-expected.normal.y).Within(Tolerance));
            Assert.That((float)actual.Normal.Z, Is.EqualTo(-expected.normal.z).Within(Tolerance));
            Assert.That((float)actual.D, Is.EqualTo(-expected.distance).Within(Tolerance));
        }

        private static void AssertPlanesEqual(Plane[] actual, Plane[] expected)
        {
            Assert.That(actual, Has.Length.EqualTo(expected.Length));
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.That(actual[i].normal.x, Is.EqualTo(expected[i].normal.x).Within(Tolerance));
                Assert.That(actual[i].normal.y, Is.EqualTo(expected[i].normal.y).Within(Tolerance));
                Assert.That(actual[i].normal.z, Is.EqualTo(expected[i].normal.z).Within(Tolerance));
                Assert.That(actual[i].distance, Is.EqualTo(expected[i].distance).Within(Tolerance));
            }
        }
    }
}
```

- [ ] **Step 2: Run the targeted test and verify the bridge is absent**

Run without `-quit`, because this repository's TestRunner XML is reliable only when Unity exits after the test run:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe'
$project = 'F:\gamedevrepos\FixedMathSharp-Unity'
$result = 'F:\gamedevrepos\FixedMathSharp-Unity\TestResults\frustum-red.xml'
$log = 'F:\gamedevrepos\FixedMathSharp-Unity\Logs\frustum-red.log'
$args = @('-batchmode', '-projectPath', $project, '-runTests', '-testPlatform', 'editmode', '-testFilter', 'FixedMathSharp.Unity.Tests.FixedFrustumUnityExtensionsTests', '-testResults', $result, '-logFile', $log)
$process = Start-Process -FilePath $unity -ArgumentList $args -WindowStyle Hidden -Wait -PassThru
Get-Content -LiteralPath $log | Select-String 'CS1061|ToFixedBoundFrustum'
```

Expected: compilation/test failure naming the missing `ToFixedBoundFrustum` extension.

- [ ] **Step 3: Implement the minimal frustum adapter in `Build/Base`**

Create `FixedFrustum.Extensions.cs` with the repository license header, `using System;`, Unity aliases, and this structure:

```csharp
using System;

using UnityCamera = UnityEngine.Camera;
using UnityGeometryUtility = UnityEngine.GeometryUtility;
using UnityMatrix4x4 = UnityEngine.Matrix4x4;
using UnityPlane = UnityEngine.Plane;

namespace FixedMathSharp.Geometry
{
    /// <summary>
    /// Provides explicit conversions between Unity frustum data and FixedMathSharp frustums.
    /// </summary>
    public static class FixedFrustumUnityExtensions
    {
        private const int LeftIndex = 0;
        private const int RightIndex = 1;
        private const int BottomIndex = 2;
        private const int TopIndex = 3;
        private const int NearIndex = 4;
        private const int FarIndex = 5;

        public static FixedBoundFrustum ToFixedBoundFrustum(this UnityCamera camera)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            return camera.ToFixedBoundFrustum(new UnityPlane[FixedBoundFrustum.PlaneCount]);
        }

        public static FixedBoundFrustum ToFixedBoundFrustum(this UnityCamera camera, UnityPlane[] planeBuffer)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            ValidateBuffer(planeBuffer);
            UnityGeometryUtility.CalculateFrustumPlanes(camera, planeBuffer);
            return CreateFixedFrustum(planeBuffer);
        }

        public static FixedBoundFrustum ToFixedBoundFrustum(this UnityMatrix4x4 worldToProjectionMatrix) =>
            worldToProjectionMatrix.ToFixedBoundFrustum(new UnityPlane[FixedBoundFrustum.PlaneCount]);

        public static FixedBoundFrustum ToFixedBoundFrustum(
            this UnityMatrix4x4 worldToProjectionMatrix,
            UnityPlane[] planeBuffer)
        {
            ValidateBuffer(planeBuffer);
            UnityGeometryUtility.CalculateFrustumPlanes(worldToProjectionMatrix, planeBuffer);
            return CreateFixedFrustum(planeBuffer);
        }

        public static UnityPlane[] ToUnityFrustumPlanes(this FixedBoundFrustum frustum)
        {
            var planes = new UnityPlane[FixedBoundFrustum.PlaneCount];
            frustum.CopyToUnityFrustumPlanes(planes);
            return planes;
        }

        public static void CopyToUnityFrustumPlanes(this FixedBoundFrustum frustum, UnityPlane[] planeBuffer)
        {
            ValidateBuffer(planeBuffer);
            planeBuffer[LeftIndex] = Reverse(frustum.Left).ToPlane();
            planeBuffer[RightIndex] = Reverse(frustum.Right).ToPlane();
            planeBuffer[BottomIndex] = Reverse(frustum.Bottom).ToPlane();
            planeBuffer[TopIndex] = Reverse(frustum.Top).ToPlane();
            planeBuffer[NearIndex] = Reverse(frustum.Near).ToPlane();
            planeBuffer[FarIndex] = Reverse(frustum.Far).ToPlane();
        }

        private static FixedBoundFrustum CreateFixedFrustum(UnityPlane[] planes) =>
            new(
                Reverse(planes[NearIndex].ToFixedPlane()),
                Reverse(planes[FarIndex].ToFixedPlane()),
                Reverse(planes[LeftIndex].ToFixedPlane()),
                Reverse(planes[RightIndex].ToFixedPlane()),
                Reverse(planes[TopIndex].ToFixedPlane()),
                Reverse(planes[BottomIndex].ToFixedPlane()));

        private static FixedPlane Reverse(FixedPlane plane) => new(-plane.Normal, -plane.D);

        private static void ValidateBuffer(UnityPlane[] planeBuffer)
        {
            if (planeBuffer is null)
                throw new ArgumentNullException(nameof(planeBuffer));
            if (planeBuffer.Length != FixedBoundFrustum.PlaneCount)
                throw new ArgumentException($"The plane buffer must contain exactly {FixedBoundFrustum.PlaneCount} elements.", nameof(planeBuffer));
        }
    }
}
```

Add concise `<summary>` to every public method. Add `<param>`, `<returns>`, and `<exception>` where the buffer order, allocation, null rule, or exact-six requirement is not obvious. Do not add public constants or a second plane-conversion abstraction.

- [ ] **Step 4: Sync both packages and run the targeted test green**

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe'
$project = 'F:\gamedevrepos\FixedMathSharp-Unity'
$syncLog = 'F:\gamedevrepos\FixedMathSharp-Unity\Logs\frustum-sync.log'
$syncArgs = @('-batchmode', '-quit', '-projectPath', $project, '-executeMethod', 'FixedMathSharp.Build.Editor.FixedMathSharpPackageSync.SyncPackagesBatchMode', '-logFile', $syncLog)
$sync = Start-Process -FilePath $unity -ArgumentList $syncArgs -WindowStyle Hidden -Wait -PassThru
if ($sync.ExitCode -ne 0) { throw "Package sync failed: $($sync.ExitCode)" }

$result = 'F:\gamedevrepos\FixedMathSharp-Unity\TestResults\frustum-green.xml'
$testLog = 'F:\gamedevrepos\FixedMathSharp-Unity\Logs\frustum-green.log'
$testArgs = @('-batchmode', '-projectPath', $project, '-runTests', '-testPlatform', 'editmode', '-testFilter', 'FixedMathSharp.Unity.Tests.FixedFrustumUnityExtensionsTests', '-testResults', $result, '-logFile', $testLog)
$tests = Start-Process -FilePath $unity -ArgumentList $testArgs -WindowStyle Hidden -Wait -PassThru
if ($tests.ExitCode -ne 0) { throw "Frustum tests failed: $($tests.ExitCode)" }
```

Expected: all `FixedFrustumUnityExtensionsTests` pass; Unity creates the four new `.meta` files with unique GUIDs.

- [ ] **Step 5: Commit the independently working frustum bridge**

```powershell
git add -- Build/Base/Runtime/Extensions/FixedFrustum.Extensions.cs Build/Base/Runtime/Extensions/FixedFrustum.Extensions.cs.meta Tests/EditMode/FixedFrustumUnityExtensionsTests.cs Tests/EditMode/FixedFrustumUnityExtensionsTests.cs.meta com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedFrustum.Extensions.cs com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedFrustum.Extensions.cs.meta com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedFrustum.Extensions.cs com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedFrustum.Extensions.cs.meta
git commit -m "feat: add Unity frustum bridge"
```

---

### Task 2: Add exact Unity-to-fixed curve conversion

**Files:**
- Create: `Tests/EditMode/FixedCurveUnityExtensionsTests.cs`
- Create: `Tests/EditMode/FixedCurveUnityExtensionsTests.cs.meta` (Unity-generated)
- Create: `Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs`
- Create: `Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs.meta` (Unity-generated)
- Create through sync: both package copies and their Unity-generated `.meta` files

**Interfaces:**
- Consumes: `AnimationCurve.keys`, `preWrapMode`, `postWrapMode`, `Keyframe.weightedMode`, `Fixed64.FromDouble`, and the `FixedCurve(FixedCurveMode, params FixedCurveKey[])` constructor.
- Produces: `TryToFixedCurve(this AnimationCurve, out FixedCurve)` and `ToFixedCurve(this AnimationCurve)` in `FixedMathSharp.FixedCurveUnityExtensions`.

- [ ] **Step 1: Write failing exact-conversion tests**

Create the test class and a `CreateClamped(params Keyframe[] keys)` helper that sets both wrap modes to `WrapMode.ClampForever`. Cover the actual semantic boundaries:

```csharp
using System;
using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
public sealed class FixedCurveUnityExtensionsTests
{
[Test]
public void TryToFixedCurve_UnequalDurationHermiteSegmentsPreserveEvaluation()
{
    AnimationCurve source = CreateClamped(
        new Keyframe(0f, 1f, 0f, 2f),
        new Keyframe(2f, 3f, -0.5f, 1.25f),
        new Keyframe(5f, -2f, -1f, 0f));

    Assert.That(source.TryToFixedCurve(out FixedCurve result), Is.True);
    Assert.That(result.Mode, Is.EqualTo(FixedCurveMode.Cubic));
    Assert.That((double)result.Keyframes[0].OutTangent, Is.EqualTo(4d).Within(1e-8));
    Assert.That((double)result.Keyframes[1].InTangent, Is.EqualTo(-1d).Within(1e-8));
    Assert.That((double)result.Keyframes[1].OutTangent, Is.EqualTo(3.75d).Within(1e-8));
    Assert.That((double)result.Keyframes[2].InTangent, Is.EqualTo(-3d).Within(1e-8));

    foreach (float time in new[] { 0f, 0.5f, 1.5f, 2f, 3f, 4.5f, 5f })
        Assert.That((float)result.Evaluate(Fixed64.FromDouble(time)), Is.EqualTo(source.Evaluate(time)).Within(0.001f));
}

[Test]
public void TryToFixedCurve_PureStepCurveUsesGlobalStepMode()
{
    Keyframe first = new(0f, 1f) { outTangent = float.PositiveInfinity };
    Keyframe second = new(2f, 3f) { inTangent = float.PositiveInfinity, outTangent = float.PositiveInfinity };
    Keyframe third = new(5f, -2f) { inTangent = float.PositiveInfinity };

    Assert.That(CreateClamped(first, second, third).TryToFixedCurve(out FixedCurve result), Is.True);
    Assert.That(result.Mode, Is.EqualTo(FixedCurveMode.Step));
    foreach (FixedCurveKey key in result.Keyframes)
    {
        Assert.That(key.InTangent, Is.EqualTo(Fixed64.Zero));
        Assert.That(key.OutTangent, Is.EqualTo(Fixed64.Zero));
    }
}

[Test]
public void TryToFixedCurve_RejectsSemanticLossAtomically()
{
    AnimationCurve weighted = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    Keyframe weightedKey = weighted.keys[0];
    weightedKey.weightedMode = WeightedMode.Out;
    weighted.MoveKey(0, weightedKey);

    AnimationCurve mixed = CreateClamped(
        new Keyframe(0f, 0f, 0f, float.PositiveInfinity),
        new Keyframe(1f, 1f, float.PositiveInfinity, 0f),
        new Keyframe(2f, 2f, 0f, 0f));

    AnimationCurve nonClamped = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    nonClamped.preWrapMode = WrapMode.Loop;
    nonClamped.postWrapMode = WrapMode.ClampForever;

    AnimationCurve collapsedTimes = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1e-12f, 1f));
    AnimationCurve outOfRange = CreateClamped(new Keyframe(0f, 0f), new Keyframe(3_000_000_000f, 1f));
    AnimationCurve nonFinite = CreateClamped(new Keyframe(0f, float.NaN), new Keyframe(1f, 1f));

    foreach (AnimationCurve source in new[] { weighted, mixed, nonClamped, collapsedTimes, outOfRange, nonFinite })
    {
        Assert.That(source.TryToFixedCurve(out FixedCurve result), Is.False);
        Assert.That(result.Keyframes, Is.Not.Null);
        Assert.That(result.Evaluate(Fixed64.Zero), Is.EqualTo(Fixed64.Zero));
    }
}

[Test]
public void ToFixedCurve_WhenNotRepresentable_PointsToBakeApi()
{
    AnimationCurve source = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    Keyframe key = source.keys[0];
    key.weightedMode = WeightedMode.Out;
    source.MoveKey(0, key);

    InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => source.ToFixedCurve());
    Assert.That(exception.Message, Does.Contain("BakeToFixedCurve"));
}

[Test]
public void ExactConversion_PreservesEmptyAndOneKeyConstants()
{
    AnimationCurve empty = CreateClamped();
    AnimationCurve oneKey = CreateClamped(new Keyframe(7f, -3f));

    Assert.That(empty.ToFixedCurve().Evaluate(Fixed64.FromDouble(100)), Is.EqualTo(Fixed64.Zero));
    Assert.That(oneKey.ToFixedCurve().Evaluate(Fixed64.FromDouble(-100)), Is.EqualTo(Fixed64.FromDouble(-3)));
}

[TestCase(false)]
[TestCase(true)]
public void TryToFixedCurve_FiniteLinearAndSmoothKeysRemainExactCubic(bool smooth)
{
    float tangent = smooth ? 0f : 1f;
    AnimationCurve source = CreateClamped(
        new Keyframe(0f, 0f, 0f, tangent),
        new Keyframe(2f, 2f, tangent, tangent),
        new Keyframe(5f, 5f, tangent, 0f));

    Assert.That(source.TryToFixedCurve(out FixedCurve result), Is.True);
    Assert.That(result.Mode, Is.EqualTo(FixedCurveMode.Cubic));
    foreach (float time in new[] { 0f, 0.5f, 1.5f, 2f, 3f, 4.5f, 5f })
        Assert.That((float)result.Evaluate(Fixed64.FromDouble(time)), Is.EqualTo(source.Evaluate(time)).Within(0.001f));
}

[Test]
public void ExactConversion_RejectsNullReceiver()
{
    AnimationCurve source = null;
    Assert.Throws<ArgumentNullException>(() => source.TryToFixedCurve(out _));
    Assert.Throws<ArgumentNullException>(() => source.ToFixedCurve());
}

private static AnimationCurve CreateClamped(params Keyframe[] keys)
{
    var curve = new AnimationCurve(keys)
    {
        preWrapMode = WrapMode.ClampForever,
        postWrapMode = WrapMode.ClampForever
    };
    return curve;
}
}
}
```

- [ ] **Step 2: Run the curve tests red**

Use the Task 1 TestRunner command with `-testFilter FixedMathSharp.Unity.Tests.FixedCurveUnityExtensionsTests`, result `curve-exact-red.xml`, and log `curve-exact-red.log`.

Expected: compilation/test failure naming missing `TryToFixedCurve`/`ToFixedCurve` methods.

- [ ] **Step 3: Implement strict conversion and its throwing wrapper**

Create `FixedCurve.Extensions.cs` under `Build/Base` with the license header and this algorithm:

```csharp
using System;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides explicit conversions between Unity animation curves and deterministic fixed curves.
    /// </summary>
    public static class FixedCurveUnityExtensions
    {
        public static bool TryToFixedCurve(this AnimationCurve curve, out FixedCurve fixedCurve)
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));

            fixedCurve = CreateConstant(Fixed64.Zero);
            if (curve.preWrapMode != WrapMode.ClampForever || curve.postWrapMode != WrapMode.ClampForever)
                return false;

            Keyframe[] unityKeys = curve.keys;
            var fixedKeys = new FixedCurveKey[unityKeys.Length];

            try
            {
                for (int i = 0; i < unityKeys.Length; i++)
                {
                    if (unityKeys[i].weightedMode != WeightedMode.None)
                        return false;

                    fixedKeys[i] = FixedCurveKey.FromDouble(unityKeys[i].time, unityKeys[i].value);
                    if (i > 0 && (unityKeys[i].time <= unityKeys[i - 1].time || fixedKeys[i].Time <= fixedKeys[i - 1].Time))
                        return false;
                }

                bool allStep = unityKeys.Length > 1;
                bool allHermite = true;
                for (int i = 0; i < unityKeys.Length - 1; i++)
                {
                    bool step = float.IsPositiveInfinity(unityKeys[i].outTangent)
                        && float.IsPositiveInfinity(unityKeys[i + 1].inTangent);
                    bool hermite = IsFinite(unityKeys[i].outTangent)
                        && IsFinite(unityKeys[i + 1].inTangent);
                    if (!step && !hermite)
                        return false;

                    allStep &= step;
                    allHermite &= hermite;
                }

                if (!allStep && !allHermite)
                    return false;

                FixedCurveMode mode = allStep ? FixedCurveMode.Step : FixedCurveMode.Cubic;
                if (mode == FixedCurveMode.Cubic)
                {
                    for (int i = 0; i < unityKeys.Length - 1; i++)
                    {
                        double duration = (double)unityKeys[i + 1].time - unityKeys[i].time;
                        fixedKeys[i].OutTangent = Fixed64.FromDouble(unityKeys[i].outTangent * duration);
                        fixedKeys[i + 1].InTangent = Fixed64.FromDouble(unityKeys[i + 1].inTangent * duration);
                    }
                }

                fixedCurve = unityKeys.Length == 0
                    ? CreateConstant(Fixed64.Zero)
                    : new FixedCurve(mode, fixedKeys);
                return true;
            }
            catch (Exception exception) when (exception is ArgumentOutOfRangeException || exception is OverflowException)
            {
                fixedCurve = CreateConstant(Fixed64.Zero);
                return false;
            }
        }

        public static FixedCurve ToFixedCurve(this AnimationCurve curve)
        {
            if (curve.TryToFixedCurve(out FixedCurve fixedCurve))
                return fixedCurve;

            throw new InvalidOperationException(
                "The Unity AnimationCurve cannot be represented exactly as a FixedCurve. Use BakeToFixedCurve for explicit sampled approximation.");
        }

        private static FixedCurve CreateConstant(Fixed64 value) =>
            new(FixedCurveMode.Linear, new FixedCurveKey(Fixed64.Zero, value));

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
```

Add complete XML documentation. `TryToFixedCurve` must document exact-clamp, unweighted, global step/Hermite, fixed-time, and safe-failure behavior; `ToFixedCurve` must document its `InvalidOperationException` and direct users to baking. Keep `CreateConstant` private so no extra API is introduced.

- [ ] **Step 4: Sync and run exact-conversion tests green**

Run the Task 1 sync command, then the targeted curve test command using `curve-exact-green.xml` and `curve-exact-green.log`.

Expected: all current `FixedCurveUnityExtensionsTests` pass and Unity generates the canonical, Standard, Lean, and test `.meta` files.

- [ ] **Step 5: Commit exact curve conversion**

```powershell
git add -- Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs.meta Tests/EditMode/FixedCurveUnityExtensionsTests.cs Tests/EditMode/FixedCurveUnityExtensionsTests.cs.meta com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedCurve.Extensions.cs com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedCurve.Extensions.cs.meta com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedCurve.Extensions.cs com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedCurve.Extensions.cs.meta
git commit -m "feat: add exact Unity curve conversion"
```

---

### Task 3: Add sampled baking and fixed-to-Unity conversion

**Files:**
- Modify: `Tests/EditMode/FixedCurveUnityExtensionsTests.cs`
- Modify: `Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs`
- Modify through sync: both package copies of `FixedCurve.Extensions.cs`

**Interfaces:**
- Consumes: Task 2's `CreateConstant` and `IsFinite` helpers; Unity `AnimationCurve.Evaluate`; all four `FixedCurveMode` values.
- Produces: `BakeToFixedCurve(this AnimationCurve, int sampleCount)` and `ToAnimationCurve(this FixedCurve)`.

- [ ] **Step 1: Add failing bake and reverse-conversion tests**

Append these concrete cases to `FixedCurveUnityExtensionsTests`:

```csharp
[Test]
public void BakeToFixedCurve_SamplesInclusiveRangeIntoLinearKeys()
{
    Keyframe first = new(-2f, 1f, 0f, 3f, 0.4f, 0.2f) { weightedMode = WeightedMode.Both };
    Keyframe last = new(3f, -4f, -1f, 0f, 0.3f, 0.5f) { weightedMode = WeightedMode.Both };
    AnimationCurve source = new(first, last);
    source.preWrapMode = WrapMode.PingPong;
    source.postWrapMode = WrapMode.Loop;

    FixedCurve baked = source.BakeToFixedCurve(5);

    Assert.That(baked.Mode, Is.EqualTo(FixedCurveMode.Linear));
    Assert.That(baked.Keyframes, Has.Length.EqualTo(5));
    Assert.That((float)baked.Keyframes[0].Time, Is.EqualTo(-2f));
    Assert.That((float)baked.Keyframes[4].Time, Is.EqualTo(3f));
    foreach (FixedCurveKey key in baked.Keyframes)
        Assert.That((float)key.Value, Is.EqualTo(source.Evaluate((float)key.Time)).Within(0.0001f));
}

[Test]
public void BakeToFixedCurve_ValidatesInputsAndConstantCases()
{
    AnimationCurve empty = new();
    AnimationCurve oneKey = new(new Keyframe(7f, -3f));

    Assert.Throws<ArgumentOutOfRangeException>(() => empty.BakeToFixedCurve(1));
    Assert.That(empty.BakeToFixedCurve(2).Evaluate(Fixed64.Zero), Is.EqualTo(Fixed64.Zero));
    Assert.That(oneKey.BakeToFixedCurve(2).Evaluate(Fixed64.Zero), Is.EqualTo(Fixed64.FromDouble(-3)));
    AnimationCurve source = null;
    Assert.Throws<ArgumentNullException>(() => source.BakeToFixedCurve(2));
}

[Test]
public void BakeToFixedCurve_RejectsCollapsedTimesAndOutOfRangeSamples()
{
    AnimationCurve collapsed = new(new Keyframe(0f, 0f), new Keyframe(1e-12f, 1f));
    AnimationCurve outOfRange = new(new Keyframe(0f, 3_000_000_000f), new Keyframe(1f, 3_000_000_000f));

    Assert.Throws<InvalidOperationException>(() => collapsed.BakeToFixedCurve(2));
    Assert.Throws<OverflowException>(() => outOfRange.BakeToFixedCurve(2));
}

[TestCase(FixedCurveMode.Linear)]
[TestCase(FixedCurveMode.Step)]
[TestCase(FixedCurveMode.Smooth)]
[TestCase(FixedCurveMode.Cubic)]
public void ToAnimationCurve_PreservesEvaluationAcrossUnequalDurations(FixedCurveMode mode)
{
    FixedCurve source = new(
        mode,
        new FixedCurveKey(Fixed64.FromDouble(0), Fixed64.FromDouble(1), Fixed64.Zero, Fixed64.FromDouble(4)),
        new FixedCurveKey(Fixed64.FromDouble(2), Fixed64.FromDouble(3), Fixed64.FromDouble(-1), Fixed64.FromDouble(3.75)),
        new FixedCurveKey(Fixed64.FromDouble(5), Fixed64.FromDouble(-2), Fixed64.FromDouble(-3), Fixed64.Zero));

    AnimationCurve result = source.ToAnimationCurve();

    Assert.That(result.preWrapMode, Is.EqualTo(WrapMode.ClampForever));
    Assert.That(result.postWrapMode, Is.EqualTo(WrapMode.ClampForever));
    foreach (Keyframe key in result.keys)
        Assert.That(key.weightedMode, Is.EqualTo(WeightedMode.None));

    foreach (float time in new[] { 0f, 0.5f, 1.5f, 2f, 3f, 4.5f, 5f })
        Assert.That(result.Evaluate(time), Is.EqualTo((float)source.Evaluate(Fixed64.FromDouble(time))).Within(0.001f));

    Keyframe[] keys = result.keys;
    if (mode == FixedCurveMode.Step)
    {
        Assert.That(keys[0].outTangent, Is.EqualTo(float.PositiveInfinity));
        Assert.That(keys[1].inTangent, Is.EqualTo(float.PositiveInfinity));
    }
    else if (mode == FixedCurveMode.Cubic)
    {
        Assert.That(keys[0].outTangent, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(keys[1].inTangent, Is.EqualTo(-0.5f).Within(0.0001f));
        Assert.That(keys[1].outTangent, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(keys[2].inTangent, Is.EqualTo(-1f).Within(0.0001f));
    }
}

[Test]
public void ToAnimationCurve_MapsEmptyFixedCurveToConstantOne()
{
    AnimationCurve result = new FixedCurve(FixedCurveMode.Linear, Array.Empty<FixedCurveKey>()).ToAnimationCurve();
    Assert.That(result.Evaluate(-100f), Is.EqualTo(1f));
    Assert.That(result.Evaluate(100f), Is.EqualTo(1f));
}

[Test]
public void ToAnimationCurve_RejectsInvalidStorageModeAndTimes()
{
    FixedCurve defaultCurve = default;
    FixedCurve undefinedMode = new((FixedCurveMode)255, new FixedCurveKey(0, 0), new FixedCurveKey(1, 1));
    FixedCurve duplicateTimes = new(FixedCurveMode.Linear, new FixedCurveKey(0, 0), new FixedCurveKey(0, 1));
    FixedCurve floatCollapsedTimes = new(
        FixedCurveMode.Linear,
        FixedCurveKey.FromDouble(16_777_216, 0),
        FixedCurveKey.FromDouble(16_777_217, 1));

    Assert.Throws<InvalidOperationException>(() => defaultCurve.ToAnimationCurve());
    Assert.Throws<InvalidOperationException>(() => undefinedMode.ToAnimationCurve());
    Assert.Throws<InvalidOperationException>(() => duplicateTimes.ToAnimationCurve());
    Assert.Throws<InvalidOperationException>(() => floatCollapsedTimes.ToAnimationCurve());
}
```

- [ ] **Step 2: Run the expanded curve suite red**

Run the targeted curve command with `curve-full-red.xml` and `curve-full-red.log`.

Expected: failure naming missing `BakeToFixedCurve` and `ToAnimationCurve`.

- [ ] **Step 3: Implement explicit baking**

Add this public method and a private fixed-key ordering check to the canonical class:

```csharp
public static FixedCurve BakeToFixedCurve(this AnimationCurve curve, int sampleCount)
{
    if (curve is null)
        throw new ArgumentNullException(nameof(curve));
    if (sampleCount < 2)
        throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount, "At least two samples are required.");

    Keyframe[] unityKeys = curve.keys;
    if (unityKeys.Length == 0)
        return CreateConstant(Fixed64.Zero);
    if (unityKeys.Length == 1)
        return CreateConstant(Fixed64.FromDouble(unityKeys[0].value));

    double start = unityKeys[0].time;
    double end = unityKeys[^1].time;
    double span = end - start;
    if (double.IsNaN(span) || double.IsInfinity(span) || span <= 0d)
        throw new InvalidOperationException("The curve must have a finite, positive authored time span.");

    var samples = new FixedCurveKey[sampleCount];
    for (int i = 0; i < sampleCount; i++)
    {
        double interpolated = i == sampleCount - 1
            ? end
            : start + span * i / (sampleCount - 1);
        float sampleTime = (float)interpolated;
        samples[i] = FixedCurveKey.FromDouble(sampleTime, curve.Evaluate(sampleTime));
        if (i > 0 && samples[i].Time <= samples[i - 1].Time)
            throw new InvalidOperationException("The requested sample times are not distinct in the fixed-point domain.");
    }

    return new FixedCurve(FixedCurveMode.Linear, samples);
}
```

Document `sampleCount`, inclusive endpoints, approximation, authoring-boundary use, and every thrown exception.

- [ ] **Step 4: Implement fixed-to-Unity conversion**

Use a two-pass conversion: validate/narrow times and values first, then assign per-segment tangents. This prevents Unity from silently reordering or merging invalid keys:

```csharp
public static AnimationCurve ToAnimationCurve(this FixedCurve curve)
{
    FixedCurveKey[] fixedKeys = curve.Keyframes;
    if (fixedKeys is null)
        throw new InvalidOperationException("A default FixedCurve has no keyframe storage.");
    if (curve.Mode != FixedCurveMode.Linear
        && curve.Mode != FixedCurveMode.Step
        && curve.Mode != FixedCurveMode.Smooth
        && curve.Mode != FixedCurveMode.Cubic)
        throw new InvalidOperationException($"Unsupported FixedCurveMode value: {curve.Mode}.");
    if (fixedKeys.Length == 0)
        return CreateClampedUnityCurve(new Keyframe(0f, 1f));

    var unityKeys = new Keyframe[fixedKeys.Length];
    for (int i = 0; i < fixedKeys.Length; i++)
    {
        float time = (float)fixedKeys[i].Time;
        float value = (float)fixedKeys[i].Value;
        if (!IsFinite(time) || !IsFinite(value) || (i > 0 && time <= unityKeys[i - 1].time))
            throw new InvalidOperationException("Fixed curve keys must remain finite and strictly increasing after Unity float conversion.");

        unityKeys[i] = new Keyframe(time, value) { weightedMode = WeightedMode.None };
    }

    for (int i = 0; i < unityKeys.Length - 1; i++)
    {
        Keyframe left = unityKeys[i];
        Keyframe right = unityKeys[i + 1];
        float duration = right.time - left.time;

        switch (curve.Mode)
        {
            case FixedCurveMode.Linear:
                float slope = (right.value - left.value) / duration;
                left.outTangent = slope;
                right.inTangent = slope;
                break;
            case FixedCurveMode.Step:
                left.outTangent = float.PositiveInfinity;
                right.inTangent = float.PositiveInfinity;
                break;
            case FixedCurveMode.Smooth:
                left.outTangent = 0f;
                right.inTangent = 0f;
                break;
            case FixedCurveMode.Cubic:
                left.outTangent = (float)((double)fixedKeys[i].OutTangent / duration);
                right.inTangent = (float)((double)fixedKeys[i + 1].InTangent / duration);
                break;
        }

        if (curve.Mode != FixedCurveMode.Step
            && (!IsFinite(left.outTangent) || !IsFinite(right.inTangent)))
        {
            throw new InvalidOperationException("Fixed curve tangents are not representable as finite Unity slopes.");
        }

        unityKeys[i] = left;
        unityKeys[i + 1] = right;
    }

    return CreateClampedUnityCurve(unityKeys);
}

private static AnimationCurve CreateClampedUnityCurve(params Keyframe[] keys)
{
    var curve = new AnimationCurve(keys)
    {
        preWrapMode = WrapMode.ClampForever,
        postWrapMode = WrapMode.ClampForever
    };
    return curve;
}
```

Keep endpoint-only tangents at zero; only segment-relevant outgoing/incoming tangents are assigned. Add XML documentation covering all modes, `WeightedMode.None`, clamped output, the empty-fixed constant-one rule, and `InvalidOperationException` cases.

- [ ] **Step 5: Sync and run the complete curve tests green**

Run package sync, then the targeted curve suite with `curve-full-green.xml` and `curve-full-green.log`.

Expected: all exact, bake, and reverse-conversion tests pass.

- [ ] **Step 6: Commit the complete curve bridge**

```powershell
git add -- Build/Base/Runtime/Extensions/FixedCurve.Extensions.cs Tests/EditMode/FixedCurveUnityExtensionsTests.cs com.mrdav30.fixedmathsharp/Runtime/Extensions/FixedCurve.Extensions.cs com.mrdav30.fixedmathsharp.lean/Runtime/Extensions/FixedCurve.Extensions.cs
git commit -m "feat: add Unity curve baking and export"
```

---

### Task 4: Document, synchronize, and verify both package variants

**Files:**
- Modify: `README.md`
- Modify: `com.mrdav30.fixedmathsharp/README.md`
- Modify: `com.mrdav30.fixedmathsharp.lean/README.md`
- Verify only: all new runtime/test `.meta` files and synchronized runtime files

**Interfaces:**
- Consumes: all APIs from Tasks 1-3.
- Produces: concise user guidance and final evidence that Standard and Lean ship identical adapter source.

- [ ] **Step 1: Add concise README guidance**

Add frustums and curves to each `Included` list, then add this short example (with the existing package-specific introduction left intact):

```csharp
Plane[] planeBuffer = new Plane[FixedBoundFrustum.PlaneCount];
FixedBoundFrustum frustum = camera.ToFixedBoundFrustum(planeBuffer);

if (!animationCurve.TryToFixedCurve(out FixedCurve curve))
    curve = animationCurve.BakeToFixedCurve(65);
```

Immediately below it, state: “`BakeToFixedCurve` is an explicit floating-point approximation. Bake once at an authoring/loading boundary and distribute or persist the fixed keys; lockstep peers should not bake independently.” Keep the root README focused on package selection and keep Standard/Lean serializer wording unchanged.

- [ ] **Step 2: Run package sync twice and require an idempotent second pass**

Run the Task 1 sync command twice with separate logs. The first may copy changed canonical files. The second must contain:

```text
FixedMathSharp package sync complete. Copied 0 files, deleted 0 files, removed 0 directories across 2 packages.
```

- [ ] **Step 3: Compare canonical and package runtime extensions byte-for-byte**

```powershell
$roots = @('com.mrdav30.fixedmathsharp', 'com.mrdav30.fixedmathsharp.lean')
$canonical = 'Build\Base\Runtime\Extensions'
foreach ($package in $roots)
{
    $destination = Join-Path $package 'Runtime\Extensions'
    $sourceNames = Get-ChildItem -LiteralPath $canonical -File -Filter '*.cs' | Select-Object -ExpandProperty Name
    $destinationNames = Get-ChildItem -LiteralPath $destination -File -Filter '*.cs' | Select-Object -ExpandProperty Name
    if (Compare-Object $sourceNames $destinationNames)
    {
        throw "Runtime extension file set differs: $package"
    }

    foreach ($source in Get-ChildItem -LiteralPath $canonical -File -Filter '*.cs')
    {
        $copy = Join-Path $package "Runtime\Extensions\$($source.Name)"
        if (-not (Test-Path -LiteralPath $copy)) { throw "Missing synced file: $copy" }
        if ((Get-FileHash -LiteralPath $source.FullName).Hash -ne (Get-FileHash -LiteralPath $copy).Hash)
        {
            throw "Synced file differs: $copy"
        }
    }
}
```

Expected: no output and no exception.

- [ ] **Step 4: Verify `.meta` coverage and GUID uniqueness**

Require `.meta` partners for both new canonical files, both new tests, and all four package copies. Then scan every tracked/untracked repository `.meta` file:

```powershell
$guidOwners = Get-ChildItem -Recurse -File -Filter '*.meta' | ForEach-Object {
    $match = Select-String -LiteralPath $_.FullName -Pattern '^guid: ([0-9a-f]+)$'
    if ($match) { [pscustomobject]@{ Guid = $match.Matches[0].Groups[1].Value; Path = $_.FullName } }
}
$duplicates = $guidOwners | Group-Object Guid | Where-Object Count -gt 1
if ($duplicates) { $duplicates | Format-List; throw 'Duplicate Unity GUIDs found.' }
```

Expected: no missing partners and no duplicate GUID group.

- [ ] **Step 5: Prepare isolated Standard and Lean EditMode projects**

Use the same CI preparation script as `.github/workflows/build-and-test.yml`:

```powershell
$bash = 'C:\Program Files\Git\bin\bash.exe'
& $bash '.assets/ci/scripts/prepare-unity-editmode-test-project.sh' --project-path '.ci/unity-standard' --package-name 'com.mrdav30.fixedmathsharp' --package-path 'com.mrdav30.fixedmathsharp' --runtime-assembly 'FixedMathSharp.Runtime'
if ($LASTEXITCODE -ne 0) { throw 'Standard test project preparation failed.' }
& $bash '.assets/ci/scripts/prepare-unity-editmode-test-project.sh' --project-path '.ci/unity-lean' --package-name 'com.mrdav30.fixedmathsharp.lean' --package-path 'com.mrdav30.fixedmathsharp.lean' --runtime-assembly 'FixedMathSharp.Lean.Runtime'
if ($LASTEXITCODE -ne 0) { throw 'Lean test project preparation failed.' }
```

- [ ] **Step 6: Run the full EditMode suite against both isolated packages**

For each project, run Unity without `-quit` and require exit code zero:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe'
$repo = 'F:\gamedevrepos\FixedMathSharp-Unity\Assets\Packages'
$outer = 'F:\gamedevrepos\FixedMathSharp-Unity'
foreach ($variant in @('standard', 'lean'))
{
    $project = Join-Path $repo ".ci\unity-$variant"
    $result = Join-Path $outer "TestResults\fixedmathsharp-$variant-editmode.xml"
    $log = Join-Path $outer "Logs\fixedmathsharp-$variant-editmode.log"
    $args = @('-batchmode', '-projectPath', $project, '-runTests', '-testPlatform', 'editmode', '-assemblyNames', 'FixedMathSharp.Unity.Tests.EditMode', '-runSynchronously', '-testResults', $result, '-logFile', $log)
    $process = Start-Process -FilePath $unity -ArgumentList $args -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "$variant EditMode tests failed: $($process.ExitCode)" }
}
```

Expected: both XML files report zero failures. After results are captured, resolve `.ci/unity-standard` and `.ci/unity-lean`, verify both paths remain under this repository's `.ci` directory, then remove only those generated test projects so they do not pollute `git status`.

```powershell
$ciRoot = [IO.Path]::GetFullPath((Join-Path $repo '.ci'))
foreach ($variant in @('standard', 'lean'))
{
    $generatedProject = [IO.Path]::GetFullPath((Join-Path $ciRoot "unity-$variant"))
    if (-not $generatedProject.StartsWith($ciRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))
    {
        throw "Refusing to remove path outside .ci: $generatedProject"
    }
    Remove-Item -LiteralPath $generatedProject -Recurse -Force
}
```

- [ ] **Step 7: Run final repository checks**

```powershell
git diff --check
git status --short
```

Inspect the final diff for only the approved adapter, test, `.meta`, README, design clarification, and plan files. Confirm no sample, core plugin, asmdef, manifest, serializer, or generated `.ci` content changed.

- [ ] **Step 8: Commit documentation and final synchronization state**

```powershell
git add -- README.md com.mrdav30.fixedmathsharp/README.md com.mrdav30.fixedmathsharp.lean/README.md .docs/specs/2026-08-07-unity-frustum-animation-curve-bridges-design.md .docs/plans/2026-08-07-unity-frustum-animation-curve-bridges.md
git commit -m "docs: document Unity frustum and curve bridges"
```

If implementation is intentionally being left unstaged for user review, skip all commit commands in this plan and instead suggest: `feat: add Unity frustum and AnimationCurve bridges`.
