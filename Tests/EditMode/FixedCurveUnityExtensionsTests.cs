using System;
using NUnit.Framework;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedCurveUnityExtensionsTests
    {
        [Test]
        public void TryToFixedCurve_UnequalDurationHermiteSegmentsPreserveEvaluation()
        {
            AnimationCurve source = CreateClamped(
                new Keyframe(0f, 1f, 0f, 2f),
                new Keyframe(2f, 3f, -0.5f, 1.25f),
                new Keyframe(5f, -2f, -1f, 0f));

            Assert.That(source.TryToFixedCurve(out FixedCurve result), Is.True);
            Assert.That(result.Mode, Is.EqualTo(FixedCurveMode.Cubic));
            Assert.That((double)result.Keyframes[0].OutTangent, Is.EqualTo(4d).Within(1e-8));
            Assert.That((double)result.Keyframes[1].InTangent, Is.EqualTo(-1d).Within(1e-8));
            Assert.That((double)result.Keyframes[1].OutTangent, Is.EqualTo(3.75d).Within(1e-8));
            Assert.That((double)result.Keyframes[2].InTangent, Is.EqualTo(-3d).Within(1e-8));

            foreach (float time in new[] { 0f, 0.5f, 1.5f, 2f, 3f, 4.5f, 5f })
                Assert.That((float)result.Evaluate(Fixed64.FromDouble(time)), Is.EqualTo(source.Evaluate(time)).Within(0.001f));
        }

        [Test]
        public void TryToFixedCurve_PureStepCurveUsesGlobalStepMode()
        {
            Keyframe first = new(0f, 1f) { outTangent = float.PositiveInfinity };
            Keyframe second = new(2f, 3f) { inTangent = float.PositiveInfinity, outTangent = float.PositiveInfinity };
            Keyframe third = new(5f, -2f) { inTangent = float.PositiveInfinity };

            Assert.That(CreateClamped(first, second, third).TryToFixedCurve(out FixedCurve result), Is.True);
            Assert.That(result.Mode, Is.EqualTo(FixedCurveMode.Step));
            foreach (FixedCurveKey key in result.Keyframes)
            {
                Assert.That(key.InTangent, Is.EqualTo(Fixed64.Zero));
                Assert.That(key.OutTangent, Is.EqualTo(Fixed64.Zero));
            }
        }

        [Test]
        public void TryToFixedCurve_RejectsSemanticLossAtomically()
        {
            AnimationCurve weighted = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            Keyframe weightedKey = weighted.keys[0];
            weightedKey.weightedMode = WeightedMode.Out;
            weighted.MoveKey(0, weightedKey);

            AnimationCurve mixed = CreateClamped(
                new Keyframe(0f, 0f, 0f, float.PositiveInfinity),
                new Keyframe(1f, 1f, float.PositiveInfinity, 0f),
                new Keyframe(2f, 2f, 0f, 0f));

            AnimationCurve nonClamped = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            nonClamped.preWrapMode = WrapMode.Loop;
            nonClamped.postWrapMode = WrapMode.ClampForever;

            AnimationCurve collapsedTimes = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1e-12f, 1f));
            AnimationCurve outOfRange = CreateClamped(new Keyframe(0f, 0f), new Keyframe(3_000_000_000f, 1f));
            AnimationCurve nonFinite = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            Keyframe nonFiniteKey = nonFinite.keys[0];
            nonFiniteKey.outTangent = float.NaN;
            nonFinite.MoveKey(0, nonFiniteKey);
            Assert.That(float.IsNaN(nonFinite.keys[0].outTangent), Is.True);

            foreach (AnimationCurve source in new[] { weighted, mixed, nonClamped, collapsedTimes, outOfRange, nonFinite })
            {
                Assert.That(source.TryToFixedCurve(out FixedCurve result), Is.False);
                Assert.That(result.Keyframes, Is.Not.Null);
                Assert.That(result.Evaluate(Fixed64.Zero), Is.EqualTo(Fixed64.Zero));
            }
        }

        [Test]
        public void ToFixedCurve_WhenNotRepresentable_PointsToBakeApi()
        {
            AnimationCurve source = CreateClamped(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            Keyframe key = source.keys[0];
            key.weightedMode = WeightedMode.Out;
            source.MoveKey(0, key);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => source.ToFixedCurve());
            Assert.That(exception.Message, Does.Contain("BakeToFixedCurve"));
        }

        [Test]
        public void ExactConversion_PreservesEmptyAndOneKeyConstants()
        {
            AnimationCurve empty = CreateClamped();
            AnimationCurve oneKey = CreateClamped(new Keyframe(7f, -3f));

            Assert.That(empty.ToFixedCurve().Evaluate(Fixed64.FromDouble(100)), Is.EqualTo(Fixed64.Zero));
            Assert.That(oneKey.ToFixedCurve().Evaluate(Fixed64.FromDouble(-100)), Is.EqualTo(Fixed64.FromDouble(-3)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TryToFixedCurve_FiniteLinearAndSmoothKeysRemainExactCubic(bool smooth)
        {
            float tangent = smooth ? 0f : 1f;
            AnimationCurve source = CreateClamped(
                new Keyframe(0f, 0f, 0f, tangent),
                new Keyframe(2f, 2f, tangent, tangent),
                new Keyframe(5f, 5f, tangent, 0f));

            Assert.That(source.TryToFixedCurve(out FixedCurve result), Is.True);
            Assert.That(result.Mode, Is.EqualTo(FixedCurveMode.Cubic));
            foreach (float time in new[] { 0f, 0.5f, 1.5f, 2f, 3f, 4.5f, 5f })
                Assert.That((float)result.Evaluate(Fixed64.FromDouble(time)), Is.EqualTo(source.Evaluate(time)).Within(0.001f));
        }

        [Test]
        public void ExactConversion_RejectsNullReceiver()
        {
            AnimationCurve source = null;
            Assert.Throws<ArgumentNullException>(() => source.TryToFixedCurve(out _));
            Assert.Throws<ArgumentNullException>(() => source.ToFixedCurve());
        }

        private static AnimationCurve CreateClamped(params Keyframe[] keys)
        {
            var curve = new AnimationCurve(keys)
            {
                preWrapMode = WrapMode.ClampForever,
                postWrapMode = WrapMode.ClampForever
            };
            return curve;
        }
    }
}
