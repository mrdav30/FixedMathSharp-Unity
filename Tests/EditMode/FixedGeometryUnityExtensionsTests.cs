using System;
using FixedMathSharp.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedGeometryUnityExtensionsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void UnityRayRoundTrip_PreservesNonAxisDirection()
        {
            Ray source = new(new Vector3(1.25f, -2.5f, 3.75f), new Vector3(1f, 2f, 3f));

            FixedRay fixedRay = source.ToFixedRay();

            Assert.That(fixedRay.Direction.IsNormalized(), Is.True);
            Assert.That(fixedRay.TryToRay(out Ray roundTrip), Is.True);
            AssertVectorEqual(roundTrip.origin, source.origin);
            AssertVectorEqual(roundTrip.direction, source.direction);
        }

        [Test]
        public void FixedRayToUnity_RejectsNonUnitDirectionWithoutChangingParameterization()
        {
            FixedRay source = new(Vector3d.FromDouble(1, 2, 3), Vector3d.FromDouble(2, 0, 0));

            Assert.That(source.TryToRay(out Ray result), Is.False);
            AssertVectorEqual(result.origin, Vector3.zero);
            AssertVectorEqual(result.direction, Vector3.zero);
            Assert.Throws<InvalidOperationException>(() => source.ToRay());
        }

        [Test]
        public void FixedRayToUnity_AcceptsZeroDirection()
        {
            FixedRay source = new(Vector3d.FromDouble(1, 2, 3), Vector3d.Zero);

            Assert.That(source.TryToRay(out Ray result), Is.True);
            AssertVectorEqual(result.origin, new Vector3(1, 2, 3));
            AssertVectorEqual(result.direction, Vector3.zero);
        }

        [Test]
        public void UnityRay2DRoundTrip_PreservesNonAxisDirection()
        {
            Ray2D source = new(new Vector2(1.25f, -2.5f), new Vector2(1f, 2f));

            FixedRay2d fixedRay = source.ToFixedRay2d();

            Assert.That(fixedRay.Direction.IsNormalized(), Is.True);
            Assert.That(fixedRay.TryToRay2D(out Ray2D roundTrip), Is.True);
            AssertVectorEqual(roundTrip.origin, source.origin);
            AssertVectorEqual(roundTrip.direction, source.direction);
        }

        [Test]
        public void FixedRay2dToUnity_RejectsNonUnitDirection()
        {
            FixedRay2d source = new(Vector2d.FromDouble(1, 2), Vector2d.FromDouble(2, 0));

            Assert.That(source.TryToRay2D(out Ray2D result), Is.False);
            AssertVectorEqual(result.origin, Vector2.zero);
            AssertVectorEqual(result.direction, Vector2.zero);
            Assert.Throws<InvalidOperationException>(() => source.ToRay2D());
        }

        [Test]
        public void PlaneConversions_PreserveUnnormalizedEquationComponents()
        {
            Plane unityPlane = default;
            unityPlane.normal = new Vector3(2f, -3f, 4f);
            unityPlane.distance = -5.5f;

            FixedPlane fixedPlane = unityPlane.ToFixedPlane();
            Plane roundTrip = fixedPlane.ToPlane();

            AssertVectorEqual(roundTrip.normal, unityPlane.normal);
            Assert.That(roundTrip.distance, Is.EqualTo(unityPlane.distance).Within(Tolerance));
        }

        [Test]
        public void RectConversions_PreserveMinAndMaxEndpoints()
        {
            Rect source = Rect.MinMaxRect(-10.5f, -20.25f, 30.75f, 40.5f);

            Rect roundTrip = source.ToFixedBoundArea().ToRect();

            AssertVectorEqual(roundTrip.min, source.min);
            AssertVectorEqual(roundTrip.max, source.max);
        }

        [Test]
        public void BoundsConversions_PreserveMinAndMaxEndpoints()
        {
            Bounds source = default;
            source.SetMinMax(new Vector3(-10.5f, -20.25f, -30.75f), new Vector3(40.5f, 50.25f, 60.75f));

            Bounds roundTrip = source.ToFixedBoundBox().ToBounds();

            AssertVectorEqual(roundTrip.min, source.min);
            AssertVectorEqual(roundTrip.max, source.max);
        }

        [Test]
        public void BoundsAreaXZConversions_PreserveGroundPlaneEndpoints()
        {
            Bounds source = default;
            source.SetMinMax(new Vector3(-10.5f, -20.25f, -30.75f), new Vector3(40.5f, 50.25f, 60.75f));

            Bounds roundTrip = source.ToFixedBoundAreaXZ().ToBoundsXZ(7.5f);

            AssertVectorEqual(roundTrip.min, new Vector3(source.min.x, 7.5f, source.min.z));
            AssertVectorEqual(roundTrip.max, new Vector3(source.max.x, 7.5f, source.max.z));
        }

        [Test]
        public void FullDomainFixedBoundsToUnity_UsesEndpointsWithoutSpanOverflow()
        {
            FixedBoundBox source = FixedBoundBox.FromMinMax(
                new Vector3d(Fixed64.MinValue, Fixed64.MinValue, Fixed64.MinValue),
                new Vector3d(Fixed64.MaxValue, Fixed64.MaxValue, Fixed64.MaxValue));

            Bounds result = source.ToBounds();

            AssertVectorEqual(result.min, new Vector3((float)Fixed64.MinValue, (float)Fixed64.MinValue, (float)Fixed64.MinValue));
            AssertVectorEqual(result.max, new Vector3((float)Fixed64.MaxValue, (float)Fixed64.MaxValue, (float)Fixed64.MaxValue));
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
    }
}
