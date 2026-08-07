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
        public void ToFixed4x4_PreservesOneWayPointSemantics()
        {
            Matrix4x4 source = Matrix4x4.TRS(
                new Vector3(1.25f, -2.5f, 3.75f),
                Quaternion.Euler(20f, 90f, -15f),
                new Vector3(2f, 3f, 4f));
            Vector3 point = new(0.5f, -1.25f, 2.75f);

            Vector3d transformed = Fixed4x4.TransformPoint(source.ToFixed4x4(), point.ToVector3d());

            AssertVectorEqual(transformed.ToVector3(), source.MultiplyPoint3x4(point));
        }

        [Test]
        public void ToMatrix4x4_PreservesOneWayPointSemantics()
        {
            Fixed4x4 source = Fixed4x4.CreateTransform(
                Vector3d.FromDouble(1.25, -2.5, 3.75),
                Quaternion.Euler(20f, 90f, -15f).ToFixedQuaternion(),
                Vector3d.FromDouble(2, 3, 4));
            Vector3d point = Vector3d.FromDouble(0.5, -1.25, 2.75);

            Vector3 transformed = source.ToMatrix4x4().MultiplyPoint3x4(point.ToVector3());

            AssertVectorEqual(transformed, Fixed4x4.TransformPoint(source, point).ToVector3());
        }

        [Test]
        public void ToFixed4x4WorldMatrix_PreservesHierarchyShear()
        {
            GameObject parent = null;
            GameObject child = null;

            try
            {
                parent = new GameObject("FixedMathSharp Sheared Parent");
                parent.transform.SetPositionAndRotation(
                    new Vector3(1f, 2f, 3f),
                    Quaternion.Euler(10f, 25f, -5f));
                parent.transform.localScale = new Vector3(2f, 1f, 0.5f);

                child = new GameObject("FixedMathSharp Sheared Child");
                child.transform.SetParent(parent.transform, false);
                child.transform.SetLocalPositionAndRotation(
                    new Vector3(0.5f, -1f, 2f),
                    Quaternion.Euler(-15f, 40f, 20f));
                child.transform.localScale = new Vector3(1.5f, 0.75f, 2.25f);

                Vector3 point = new(0.25f, -0.5f, 1.75f);
                Vector3d transformed = Fixed4x4.TransformPoint(
                    child.transform.ToFixed4x4WorldMatrix(),
                    point.ToVector3d());

                AssertVectorEqual(transformed.ToVector3(), child.transform.localToWorldMatrix.MultiplyPoint3x4(point));
            }
            finally
            {
                DestroyImmediate(child);
                DestroyImmediate(parent);
            }
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
                roundTripped = fixedMatrix.CreateTransformLocal("FixedMathSharp Local Round Trip");

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
                roundTripped = fixedMatrix.CreateTransformWorld("FixedMathSharp World Round Trip", targetParent.transform);

                AssertWorldTransformEqual(roundTripped, source.transform);
            }
            finally
            {
                DestroyImmediate(roundTripped);
                DestroyImmediate(targetParent);
                DestroyImmediate(source);
            }
        }

        [Test]
        public void TryApplyToTransformLocal_AcceptsReflectedTrs()
        {
            GameObject target = null;

            try
            {
                target = new GameObject("FixedMathSharp Reflected Target");
                Fixed4x4 source = Fixed4x4.CreateTransform(
                    Vector3d.FromDouble(1.25, -2.5, 3.75),
                    Quaternion.Euler(20f, 35f, -15f).ToFixedQuaternion(),
                    Vector3d.FromDouble(-2, 3, 4));

                Assert.That(source.TryApplyToTransformLocal(target.transform), Is.True);

                AssertMatrixSemanticsEqual(target.transform.ToFixed4x4LocalMatrix(), source);
            }
            finally
            {
                DestroyImmediate(target);
            }
        }

        [Test]
        public void TryApplyToTransformLocal_RejectsInvalidMatricesAtomically()
        {
            GameObject target = null;

            try
            {
                target = new GameObject("FixedMathSharp Atomic Local Target");
                target.transform.localPosition = new Vector3(1f, 2f, 3f);
                target.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);
                target.transform.localScale = new Vector3(4f, 5f, 6f);

                Fixed4x4 shear = Fixed4x4.Identity;
                shear.M12 = Fixed64.FromDouble(0.25);
                Fixed4x4 singular = Fixed4x4.CreateScale(Fixed64.Zero, Fixed64.One, Fixed64.One);
                Fixed4x4 perspective = Fixed4x4.Identity;
                perspective.M14 = Fixed64.FromDouble(0.25);

                foreach ((string name, Fixed4x4 matrix) in new[]
                {
                    ("shear", shear),
                    ("singular", singular),
                    ("perspective", perspective)
                })
                {
                    Assert.That(matrix.TryApplyToTransformLocal(target.transform), Is.False, name);
                    AssertLocalTransformEqual(
                        target.transform,
                        new Vector3(1f, 2f, 3f),
                        Quaternion.Euler(10f, 20f, 30f),
                        new Vector3(4f, 5f, 6f));
                }

                Assert.Throws<System.InvalidOperationException>(() => shear.ApplyToTransformLocal(target.transform));
                AssertLocalTransformEqual(
                    target.transform,
                    new Vector3(1f, 2f, 3f),
                    Quaternion.Euler(10f, 20f, 30f),
                    new Vector3(4f, 5f, 6f));
            }
            finally
            {
                DestroyImmediate(target);
            }
        }

        [Test]
        public void TryApplyToTransformWorld_ComputesRepresentableParentLocalTrs()
        {
            GameObject parent = null;
            GameObject target = null;

            try
            {
                parent = new GameObject("FixedMathSharp World Parent");
                parent.transform.SetPositionAndRotation(new Vector3(3f, -2f, 1f), Quaternion.identity);
                parent.transform.localScale = new Vector3(2f, 1f, 0.5f);

                target = new GameObject("FixedMathSharp World Target");
                target.transform.SetParent(parent.transform, false);

                Fixed4x4 local = Fixed4x4.CreateTransform(
                    Vector3d.FromDouble(1.25, -2.5, 3.75),
                    Quaternion.Euler(0f, 90f, 0f).ToFixedQuaternion(),
                    Vector3d.FromDouble(1.5, 0.75, 2.25));
                Assert.That(
                    Fixed4x4.TryMultiply(local, parent.transform.ToFixed4x4WorldMatrix(), out Fixed4x4 world),
                    Is.True);

                Assert.That(world.TryApplyToTransformWorld(target.transform), Is.True);

                AssertMatrixSemanticsEqual(target.transform.ToFixed4x4LocalMatrix(), local);
            }
            finally
            {
                DestroyImmediate(target);
                DestroyImmediate(parent);
            }
        }

        [Test]
        public void TryApplyToTransformWorld_RejectsSingularParentAtomically()
        {
            GameObject parent = null;
            GameObject target = null;

            try
            {
                parent = new GameObject("FixedMathSharp Singular Parent");
                parent.transform.localScale = new Vector3(1f, 0f, 1f);
                target = new GameObject("FixedMathSharp Atomic World Target");
                target.transform.SetParent(parent.transform, false);
                target.transform.localPosition = new Vector3(1f, 2f, 3f);
                target.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);
                target.transform.localScale = new Vector3(4f, 5f, 6f);

                Assert.That(Fixed4x4.Identity.TryApplyToTransformWorld(target.transform), Is.False);
                AssertLocalTransformEqual(
                    target.transform,
                    new Vector3(1f, 2f, 3f),
                    Quaternion.Euler(10f, 20f, 30f),
                    new Vector3(4f, 5f, 6f));
                Assert.Throws<System.InvalidOperationException>(
                    () => Fixed4x4.Identity.ApplyToTransformWorld(target.transform));
            }
            finally
            {
                DestroyImmediate(target);
                DestroyImmediate(parent);
            }
        }

        [Test]
        public void TryApplyToTransformWorld_RejectsParentOutsideFixedDomainAtomically()
        {
            GameObject parent = null;
            GameObject target = null;

            try
            {
                parent = new GameObject("FixedMathSharp Out Of Domain Parent");
                parent.transform.position = new Vector3(3_000_000_000f, 0f, 0f);
                target = new GameObject("FixedMathSharp Atomic Domain Target");
                target.transform.SetParent(parent.transform, false);
                target.transform.localPosition = new Vector3(1f, 2f, 3f);
                target.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);
                target.transform.localScale = new Vector3(4f, 5f, 6f);

                bool applied = true;
                Assert.DoesNotThrow(
                    () => applied = Fixed4x4.Identity.TryApplyToTransformWorld(target.transform));
                Assert.That(applied, Is.False);
                AssertLocalTransformEqual(
                    target.transform,
                    new Vector3(1f, 2f, 3f),
                    Quaternion.Euler(10f, 20f, 30f),
                    new Vector3(4f, 5f, 6f));
            }
            finally
            {
                DestroyImmediate(target);
                DestroyImmediate(parent);
            }
        }

        [Test]
        public void CreateTransformLocal_RejectsBeforeAllocatingGameObject()
        {
            const string Name = "FixedMathSharp Invalid Matrix Must Not Allocate";
            Fixed4x4 shear = Fixed4x4.Identity;
            shear.M12 = Fixed64.FromDouble(0.25);

            Assert.That(GameObject.Find(Name), Is.Null);
            Assert.Throws<System.InvalidOperationException>(() => shear.CreateTransformLocal(Name));
            Assert.That(GameObject.Find(Name), Is.Null);
        }

        private static void AssertLocalTransformEqual(Transform actual, Transform expected)
        {
            AssertVectorEqual(actual.localPosition, expected.localPosition);
            AssertQuaternionBasisEqual(actual.localRotation, expected.localRotation);
            AssertVectorEqual(actual.localScale, expected.localScale);
        }

        private static void AssertLocalTransformEqual(
            Transform actual,
            Vector3 expectedPosition,
            Quaternion expectedRotation,
            Vector3 expectedScale)
        {
            AssertVectorEqual(actual.localPosition, expectedPosition);
            AssertQuaternionBasisEqual(actual.localRotation, expectedRotation);
            AssertVectorEqual(actual.localScale, expectedScale);
        }

        private static void AssertMatrixSemanticsEqual(Fixed4x4 actual, Fixed4x4 expected)
        {
            foreach (Vector3d point in new[]
            {
                Vector3d.Zero,
                Vector3d.Right,
                Vector3d.Up,
                Vector3d.Forward
            })
            {
                AssertVectorEqual(
                    Fixed4x4.TransformPoint(actual, point).ToVector3(),
                    Fixed4x4.TransformPoint(expected, point).ToVector3());
            }
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
