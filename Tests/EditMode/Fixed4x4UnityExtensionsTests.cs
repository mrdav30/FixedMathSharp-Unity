using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public class Fixed4x4UnityExtensionsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ToMatrix4x4_RoundTripsUnityTranslationSemantics()
        {
            Vector3 position = new(1.25f, -2.5f, 3.75f);
            Matrix4x4 source = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);

            Matrix4x4 roundTripped = source.ToFixed4x4().ToMatrix4x4();

            Assert.That(roundTripped.m03, Is.EqualTo(source.m03).Within(Tolerance));
            Assert.That(roundTripped.m13, Is.EqualTo(source.m13).Within(Tolerance));
            Assert.That(roundTripped.m23, Is.EqualTo(source.m23).Within(Tolerance));
            Assert.That(roundTripped.m30, Is.EqualTo(source.m30).Within(Tolerance));
            Assert.That(roundTripped.m31, Is.EqualTo(source.m31).Within(Tolerance));
            Assert.That(roundTripped.m32, Is.EqualTo(source.m32).Within(Tolerance));
        }

        [Test]
        public void Matrix4x4Trs_RoundTripsUnityDirectionSemantics()
        {
            Vector3 position = new(1.25f, -2.5f, 3.75f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Vector3 scale = new(2f, 3f, 4f);
            Matrix4x4 source = Matrix4x4.TRS(position, rotation, scale);

            Matrix4x4 roundTripped = source.ToFixed4x4().ToMatrix4x4();

            AssertVectorEqual(roundTripped.MultiplyVector(Vector3.forward), source.MultiplyVector(Vector3.forward));
            AssertVectorEqual(roundTripped.MultiplyVector(Vector3.up), source.MultiplyVector(Vector3.up));
            AssertVectorEqual(roundTripped.MultiplyVector(Vector3.right), source.MultiplyVector(Vector3.right));
            AssertVectorEqual(roundTripped.MultiplyPoint3x4(Vector3.zero), source.MultiplyPoint3x4(Vector3.zero));
        }

        [Test]
        public void TransformLocalMatrix_ToTransformLocal_RoundTripsLocalTransform()
        {
            GameObject source = null;
            Transform roundTripped = null;

            try
            {
                source = new GameObject("FixedMathSharp Local Source");
                source.transform.localPosition = new Vector3(1.25f, -2.5f, 3.75f);
                source.transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                source.transform.localScale = new Vector3(1.5f, 0.75f, 2.25f);

                Fixed4x4 fixedMatrix = source.transform.ToFixed4x4LocalMatrix();
                roundTripped = fixedMatrix.ToTransformLocal("FixedMathSharp Local Round Trip");

                AssertLocalTransformEqual(roundTripped, source.transform);
            }
            finally
            {
                DestroyImmediate(roundTripped);
                DestroyImmediate(source);
            }
        }

        [Test]
        public void TransformWorldMatrix_ToTransformWorld_RoundTripsWorldTransform()
        {
            GameObject source = null;
            GameObject targetParent = null;
            Transform roundTripped = null;

            try
            {
                source = new GameObject("FixedMathSharp World Source");
                source.transform.position = new Vector3(1.25f, -2.5f, 3.75f);
                source.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                source.transform.localScale = new Vector3(1.5f, 0.75f, 2.25f);

                targetParent = new GameObject("FixedMathSharp World Target Parent");
                targetParent.transform.position = new Vector3(-4f, 1.5f, 2f);
                targetParent.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                targetParent.transform.localScale = Vector3.one;

                Fixed4x4 fixedMatrix = source.transform.ToFixed4x4WorldMatrix();
                roundTripped = fixedMatrix.ToTransformWorld("FixedMathSharp World Round Trip", targetParent.transform);

                AssertWorldTransformEqual(roundTripped, source.transform);
            }
            finally
            {
                DestroyImmediate(roundTripped);
                DestroyImmediate(targetParent);
                DestroyImmediate(source);
            }
        }

        private static void AssertLocalTransformEqual(Transform actual, Transform expected)
        {
            AssertVectorEqual(actual.localPosition, expected.localPosition);
            AssertQuaternionBasisEqual(actual.localRotation, expected.localRotation);
            AssertVectorEqual(actual.localScale, expected.localScale);
        }

        private static void AssertWorldTransformEqual(Transform actual, Transform expected)
        {
            AssertVectorEqual(actual.position, expected.position);
            AssertQuaternionBasisEqual(actual.rotation, expected.rotation);
            AssertVectorEqual(actual.lossyScale, expected.lossyScale);
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
