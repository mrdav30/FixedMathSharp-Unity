using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public class VectorUnityExtensionsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void DirectConversions_PreserveComponents()
        {
            Vector2 unity2 = new(1.25f, -2.5f);
            Vector3 unity3 = new(1.25f, -2.5f, 3.75f);
            Vector4 unity4 = new(1.25f, -2.5f, 3.75f, -4.5f);

            AssertVectorEqual(unity2.ToVector2d().ToVector2(), unity2);
            AssertVectorEqual(unity3.ToVector3d().ToVector3(), unity3);
            AssertVectorEqual(unity4.ToVector4d().ToVector4(), unity4);
        }

        [Test]
        public void ToVector3dXY_MapsUnityVector2ToXYPlane()
        {
            Vector3d result = new Vector2(1.25f, 2.5f).ToVector3dXY(3.75f);

            AssertVectorEqual(result, Vector3d.FromDouble(1.25, 2.5, 3.75));
        }

        [Test]
        public void ToVector3dXZ_MapsUnityVector2ToXZPlane()
        {
            Vector3d result = new Vector2(1.25f, 3.75f).ToVector3dXZ(2.5f);

            AssertVectorEqual(result, Vector3d.FromDouble(1.25, 2.5, 3.75));
        }

        [Test]
        public void ToVector2dXY_UsesUnityXAndYComponents()
        {
            Vector3 source = new(1.25f, 2.5f, 3.75f);

            Vector2d result = source.ToVector2dXY();

            Assert.That((float)result.X, Is.EqualTo(source.x).Within(Tolerance));
            Assert.That((float)result.Y, Is.EqualTo(source.y).Within(Tolerance));
        }

        [Test]
        public void ToVector2dXZ_UsesUnityXAndZComponents()
        {
            Vector3 source = new(1.25f, 2.5f, 3.75f);

            Vector2d result = source.ToVector2dXZ();

            Assert.That((float)result.X, Is.EqualTo(source.x).Within(Tolerance));
            Assert.That((float)result.Y, Is.EqualTo(source.z).Within(Tolerance));
        }

        [Test]
        public void ToVector3XY_MapsVector2dToUnityXYPlane()
        {
            Vector2d source = Vector2d.FromDouble(1.25, 2.5);

            Vector3 result = source.ToVector3XY(3.75f);

            Assert.That(result.x, Is.EqualTo(1.25f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(2.5f).Within(Tolerance));
            Assert.That(result.z, Is.EqualTo(3.75f).Within(Tolerance));
        }

        [Test]
        public void ToVector3XZ_MapsVector2dToUnityXZPlane()
        {
            Vector2d source = Vector2d.FromDouble(1.25, 3.75);

            Vector3 result = source.ToVector3XZ(2.5f);

            Assert.That(result.x, Is.EqualTo(1.25f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(2.5f).Within(Tolerance));
            Assert.That(result.z, Is.EqualTo(3.75f).Within(Tolerance));
        }

        [Test]
        public void ToVector2XY_ProjectsFixedVectorOntoUnityXYPlane()
        {
            Vector2 result = Vector3d.FromDouble(1.25, 2.5, 3.75).ToVector2XY();

            AssertVectorEqual(result, new Vector2(1.25f, 2.5f));
        }

        [Test]
        public void ToVector2XZ_ProjectsFixedVectorOntoUnityXZPlane()
        {
            Vector2 result = Vector3d.FromDouble(1.25, 2.5, 3.75).ToVector2XZ();

            AssertVectorEqual(result, new Vector2(1.25f, 3.75f));
        }

        [Test]
        public void ToVector3XZ_ProjectsFixedVectorOntoUnityXZPlane()
        {
            Vector3 result = Vector3d.FromDouble(1.25, 9, 3.75).ToVector3XZ(2.5f);

            AssertVectorEqual(result, new Vector3(1.25f, 2.5f, 3.75f));
        }

        private static void AssertVectorEqual(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }

        private static void AssertVectorEqual(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
        }

        private static void AssertVectorEqual(Vector4 actual, Vector4 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
            Assert.That(actual.w, Is.EqualTo(expected.w).Within(Tolerance));
        }

        private static void AssertVectorEqual(Vector3d actual, Vector3d expected)
        {
            Assert.That(actual.X.m_rawValue, Is.EqualTo(expected.X.m_rawValue));
            Assert.That(actual.Y.m_rawValue, Is.EqualTo(expected.Y.m_rawValue));
            Assert.That(actual.Z.m_rawValue, Is.EqualTo(expected.Z.m_rawValue));
        }
    }
}
