using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public class VectorUnityExtensionsTests
    {
        private const float Tolerance = 0.0001f;

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
    }
}
