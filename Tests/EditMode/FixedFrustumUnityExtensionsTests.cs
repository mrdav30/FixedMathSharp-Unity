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
        public void MatrixConversion_ReversesBoundaryDistanceBeforeFixedConversion()
        {
            const float Boundary = 2_147_483_648f;
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.m03 = Boundary / 2f;
            matrix.m33 = Boundary / 2f;
            Plane[] unityPlanes = GeometryUtility.CalculateFrustumPlanes(matrix);

            Assert.That(unityPlanes[0].distance, Is.EqualTo(Boundary));

            FixedBoundFrustum fixedFrustum = matrix.ToFixedBoundFrustum();

            Assert.That(fixedFrustum.Left.D, Is.EqualTo(Fixed64.MinValue));
            Assert.That(fixedFrustum.ToUnityFrustumPlanes()[0].distance, Is.EqualTo(Boundary));
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
