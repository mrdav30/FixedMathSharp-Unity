using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedTransformUnityExtensionsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void LocalRoundTrip_PreservesSignedZeroScaleAndExplicitParent()
        {
            GameObject source = null;
            GameObject target = null;

            try
            {
                source = new GameObject("FixedTransform Local Source");
                source.transform.localPosition = new Vector3(1.25f, -2.5f, 3.75f);
                source.transform.localRotation = Quaternion.Euler(20f, 35f, -15f);
                source.transform.localScale = new Vector3(-2f, 0f, 4f);
                FixedTransform fixedParent = new(Vector3d.Zero, FixedQuaternion.Identity, Vector3d.One);

                FixedTransform snapshot = source.transform.ToFixedTransformLocal(fixedParent);
                target = new GameObject("FixedTransform Local Target");
                snapshot.ApplyToTransformLocal(target.transform);

                Assert.That(snapshot.Parent, Is.SameAs(fixedParent));
                AssertVectorEqual(target.transform.localPosition, source.transform.localPosition);
                AssertQuaternionBasisEqual(target.transform.localRotation, source.transform.localRotation);
                AssertVectorEqual(target.transform.localScale, source.transform.localScale);
            }
            finally
            {
                DestroyImmediate(target);
                DestroyImmediate(source);
            }
        }

        [Test]
        public void TryApplyToTransformWorld_ComposesFixedParentIntoMatchingUnityHierarchy()
        {
            GameObject unityParent = null;
            GameObject target = null;

            try
            {
                FixedTransform fixedParent = new(
                    Vector3d.FromDouble(3, -2, 1),
                    FixedQuaternion.Identity,
                    Vector3d.FromDouble(2, 1, 0.5));
                FixedTransform fixedChild = new(
                    Vector3d.FromDouble(1.25, -2.5, 3.75),
                    Quaternion.Euler(10f, 20f, 30f).ToFixedQuaternion(),
                    Vector3d.FromDouble(1.5, 0.75, 2.25),
                    fixedParent);

                unityParent = new GameObject("FixedTransform World Parent");
                fixedParent.ApplyToTransformLocal(unityParent.transform);
                target = new GameObject("FixedTransform World Target");
                target.transform.SetParent(unityParent.transform, false);

                Assert.That(fixedChild.TryApplyToTransformWorld(target.transform), Is.True);

                AssertVectorEqual(target.transform.localPosition, fixedChild.LocalPosition.ToVector3());
                AssertQuaternionBasisEqual(target.transform.localRotation, fixedChild.LocalRotation.ToQuaternion());
                AssertVectorEqual(target.transform.localScale, fixedChild.LocalScale.ToVector3());
            }
            finally
            {
                DestroyImmediate(target);
                DestroyImmediate(unityParent);
            }
        }

        [Test]
        public void TryApplyToTransformWorld_RejectsSingularUnityParentAtomically()
        {
            GameObject unityParent = null;
            GameObject target = null;

            try
            {
                FixedTransform snapshot = new(
                    Vector3d.FromDouble(1.25, -2.5, 3.75),
                    Quaternion.Euler(10f, 20f, 30f).ToFixedQuaternion(),
                    Vector3d.One);
                unityParent = new GameObject("FixedTransform Singular Parent");
                unityParent.transform.localScale = new Vector3(1f, 0f, 1f);
                target = new GameObject("FixedTransform Atomic Target");
                target.transform.SetParent(unityParent.transform, false);
                target.transform.localPosition = new Vector3(1f, 2f, 3f);
                target.transform.localRotation = Quaternion.Euler(20f, 30f, 40f);
                target.transform.localScale = new Vector3(4f, 5f, 6f);

                Assert.That(snapshot.TryApplyToTransformWorld(target.transform), Is.False);
                AssertVectorEqual(target.transform.localPosition, new Vector3(1f, 2f, 3f));
                AssertQuaternionBasisEqual(target.transform.localRotation, Quaternion.Euler(20f, 30f, 40f));
                AssertVectorEqual(target.transform.localScale, new Vector3(4f, 5f, 6f));
                Assert.Throws<System.InvalidOperationException>(() => snapshot.ApplyToTransformWorld(target.transform));
            }
            finally
            {
                DestroyImmediate(target);
                DestroyImmediate(unityParent);
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

        private static void DestroyImmediate(GameObject gameObject)
        {
            if (gameObject != null)
                Object.DestroyImmediate(gameObject);
        }
    }
}
