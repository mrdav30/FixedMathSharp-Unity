using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class Fixed3x3UnityExtensionsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ToFixed3x3Basis_PreservesOneWayVectorSemantics()
        {
            Matrix4x4 source = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(20f, 90f, -15f),
                new Vector3(2f, 3f, 4f));
            Vector3 vector = new(0.5f, -1.25f, 2.75f);

            Vector3d transformed = source.ToFixed3x3Basis() * vector.ToVector3d();

            AssertVectorEqual(transformed.ToVector3(), source.MultiplyVector(vector));
        }

        [Test]
        public void ToMatrix4x4_PreservesOneWayVectorSemantics()
        {
            Fixed3x3 source = Quaternion.Euler(20f, 90f, -15f).ToFixedQuaternion().ToMatrix3x3();
            Vector3d vector = Vector3d.FromDouble(0.5, -1.25, 2.75);

            Vector3 transformed = source.ToMatrix4x4().MultiplyVector(vector.ToVector3());

            AssertVectorEqual(transformed, (source * vector).ToVector3());
        }

        [Test]
        public void ToFixed3x3Rotation_RemovesScale()
        {
            Quaternion rotation = Quaternion.Euler(20f, 90f, -15f);
            Matrix4x4 source = Matrix4x4.TRS(Vector3.zero, rotation, new Vector3(2f, 3f, 4f));

            Vector3d transformed = source.ToFixed3x3Rotation() * Vector3d.Forward;

            AssertVectorEqual(transformed.ToVector3(), rotation * Vector3.forward);
        }

        [Test]
        public void TransformRotationCapture_UsesExplicitLocalAndWorldRotations()
        {
            GameObject parent = null;
            GameObject child = null;

            try
            {
                parent = new GameObject("Fixed3x3 Rotation Parent");
                parent.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
                child = new GameObject("Fixed3x3 Rotation Child");
                child.transform.SetParent(parent.transform, false);
                child.transform.localRotation = Quaternion.Euler(20f, 30f, -10f);

                AssertVectorEqual(
                    (child.transform.ToFixed3x3LocalRotation() * Vector3d.Forward).ToVector3(),
                    child.transform.localRotation * Vector3.forward);
                AssertVectorEqual(
                    (child.transform.ToFixed3x3WorldRotation() * Vector3d.Forward).ToVector3(),
                    child.transform.rotation * Vector3.forward);
            }
            finally
            {
                DestroyImmediate(child);
                DestroyImmediate(parent);
            }
        }

        [Test]
        public void CreateTransformRotationLocalAndWorld_UseExplicitSpaces()
        {
            GameObject parent = null;
            Transform local = null;
            Transform world = null;

            try
            {
                parent = new GameObject("Fixed3x3 Creation Parent");
                parent.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
                Quaternion expected = Quaternion.Euler(20f, 30f, -10f);
                Fixed3x3 rotation = expected.ToFixedQuaternion().ToMatrix3x3();

                local = rotation.CreateTransformRotationLocal("Fixed3x3 Local", parent.transform);
                world = rotation.CreateTransformRotationWorld("Fixed3x3 World", parent.transform);

                AssertQuaternionBasisEqual(local.localRotation, expected);
                AssertQuaternionBasisEqual(world.rotation, expected);
            }
            finally
            {
                DestroyImmediate(local);
                DestroyImmediate(world);
                DestroyImmediate(parent);
            }
        }

        private static void AssertQuaternionBasisEqual(Quaternion actual, Quaternion expected)
        {
            AssertVectorEqual(actual * Vector3.forward, expected * Vector3.forward);
            AssertVectorEqual(actual * Vector3.up, expected * Vector3.up);
            AssertVectorEqual(actual * Vector3.right, expected * Vector3.right);
        }

        private static void AssertVectorEqual(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
        }

        private static void DestroyImmediate(Transform transform)
        {
            if (transform != null)
                Object.DestroyImmediate(transform.gameObject);
        }

        private static void DestroyImmediate(GameObject gameObject)
        {
            if (gameObject != null)
                Object.DestroyImmediate(gameObject);
        }
    }
}
