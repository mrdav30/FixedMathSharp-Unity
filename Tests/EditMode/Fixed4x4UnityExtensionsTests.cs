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
    }
}
