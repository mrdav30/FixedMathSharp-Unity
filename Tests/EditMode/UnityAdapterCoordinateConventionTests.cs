using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public class UnityAdapterCoordinateConventionTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void Vector3Directions_DirectlyMapToCanonicalFixedMathSharpDirections()
        {
            AssertVectorEqual(Vector3.forward.ToVector3d(), Vector3d.Forward);
            AssertVectorEqual(Vector3d.Forward.ToVector3(), Vector3.forward);

            AssertVectorEqual(Vector3.up.ToVector3d(), Vector3d.Up);
            AssertVectorEqual(Vector3d.Up.ToVector3(), Vector3.up);

            AssertVectorEqual(Vector3.right.ToVector3d(), Vector3d.Right);
            AssertVectorEqual(Vector3d.Right.ToVector3(), Vector3.right);
        }

        [Test]
        public void QuaternionLookRotationForward_RoundTripsCanonicalBasis()
        {
            Quaternion source = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            FixedQuaternion fixedQuaternion = source.ToFixedQuaternion();
            Quaternion roundTripped = fixedQuaternion.ToQuaternion();

            AssertVectorEqual(fixedQuaternion * Vector3d.Forward, Vector3d.Forward);
            AssertVectorEqual(fixedQuaternion * Vector3d.Up, Vector3d.Up);
            AssertVectorEqual(fixedQuaternion * Vector3d.Right, Vector3d.Right);

            AssertVectorEqual(roundTripped * Vector3.forward, Vector3.forward);
            AssertVectorEqual(roundTripped * Vector3.up, Vector3.up);
            AssertVectorEqual(roundTripped * Vector3.right, Vector3.right);
        }

        [Test]
        public void QuaternionLookRotationRight_PreservesUnityFacingBasis()
        {
            Quaternion source = Quaternion.LookRotation(Vector3.right, Vector3.up);
            FixedQuaternion fixedQuaternion = source.ToFixedQuaternion();
            Quaternion roundTripped = fixedQuaternion.ToQuaternion();

            AssertVectorEqual(fixedQuaternion * Vector3d.Forward, source * Vector3.forward);
            AssertVectorEqual(fixedQuaternion * Vector3d.Up, source * Vector3.up);
            AssertVectorEqual(fixedQuaternion * Vector3d.Right, source * Vector3.right);

            AssertVectorEqual(roundTripped * Vector3.forward, source * Vector3.forward);
            AssertVectorEqual(roundTripped * Vector3.up, source * Vector3.up);
            AssertVectorEqual(roundTripped * Vector3.right, source * Vector3.right);
        }

        private static void AssertVectorEqual(Vector3d actual, Vector3d expected)
        {
            Assert.That((float)actual.X, Is.EqualTo((float)expected.X).Within(Tolerance));
            Assert.That((float)actual.Y, Is.EqualTo((float)expected.Y).Within(Tolerance));
            Assert.That((float)actual.Z, Is.EqualTo((float)expected.Z).Within(Tolerance));
        }

        private static void AssertVectorEqual(Vector3d actual, Vector3 expected)
        {
            Assert.That((float)actual.X, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That((float)actual.Y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That((float)actual.Z, Is.EqualTo(expected.z).Within(Tolerance));
        }

        private static void AssertVectorEqual(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
        }
    }
}
