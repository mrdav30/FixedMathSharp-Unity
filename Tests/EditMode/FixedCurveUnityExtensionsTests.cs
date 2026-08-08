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

        [Test]
        public void BakeToFixedCurve_SamplesInclusiveRangeIntoLinearKeys()
        {
            Keyframe first = new(-2f, 1f, 0f, 3f, 0.4f, 0.2f) { weightedMode = WeightedMode.Both };
            Keyframe last = new(3f, -4f, -1f, 0f, 0.3f, 0.5f) { weightedMode = WeightedMode.Both };
            AnimationCurve source = new(first, last);
            source.preWrapMode = WrapMode.PingPong;
            source.postWrapMode = WrapMode.Loop;

            FixedCurve baked = source.BakeToFixedCurve(5);

            Assert.That(baked.Mode, Is.EqualTo(FixedCurveMode.Linear));
            Assert.That(baked.Keyframes, Has.Length.EqualTo(5));
            Assert.That((float)baked.Keyframes[0].Time, Is.EqualTo(-2f));
            Assert.That((float)baked.Keyframes[4].Time, Is.EqualTo(3f));
            foreach (FixedCurveKey key in baked.Keyframes)
                Assert.That((float)key.Value, Is.EqualTo(source.Evaluate((float)key.Time)).Within(0.0001f));
        }

        [Test]
        public void BakeToFixedCurve_ValidatesInputsAndConstantCases()
        {
            AnimationCurve empty = new();
            AnimationCurve oneKey = new(new Keyframe(7f, -3f));

            Assert.Throws<ArgumentOutOfRangeException>(() => empty.BakeToFixedCurve(1));
            Assert.That(empty.BakeToFixedCurve(2).Evaluate(Fixed64.Zero), Is.EqualTo(Fixed64.Zero));
            Assert.That(oneKey.BakeToFixedCurve(2).Evaluate(Fixed64.Zero), Is.EqualTo(Fixed64.FromDouble(-3)));
            AnimationCurve source = null;
            Assert.Throws<ArgumentNullException>(() => source.BakeToFixedCurve(2));
        }

        [Test]
        public void BakeToFixedCurve_RejectsCollapsedTimesAndOutOfRangeSamples()
        {
            AnimationCurve collapsed = new(new Keyframe(0f, 0f), new Keyframe(1e-12f, 1f));
            AnimationCurve outOfRange = new(new Keyframe(0f, 3_000_000_000f), new Keyframe(1f, 3_000_000_000f));

            Assert.Throws<InvalidOperationException>(() => collapsed.BakeToFixedCurve(2));
            Assert.Throws<OverflowException>(() => outOfRange.BakeToFixedCurve(2));
        }

        [TestCase(FixedCurveMode.Linear)]
        [TestCase(FixedCurveMode.Step)]
        [TestCase(FixedCurveMode.Smooth)]
        [TestCase(FixedCurveMode.Cubic)]
        public void ToAnimationCurve_PreservesEvaluationAcrossUnequalDurations(FixedCurveMode mode)
        {
            FixedCurve source = new(
                mode,
                new FixedCurveKey(Fixed64.FromDouble(0), Fixed64.FromDouble(1), Fixed64.Zero, Fixed64.FromDouble(4)),
                new FixedCurveKey(Fixed64.FromDouble(2), Fixed64.FromDouble(3), Fixed64.FromDouble(-1), Fixed64.FromDouble(3.75)),
                new FixedCurveKey(Fixed64.FromDouble(5), Fixed64.FromDouble(-2), Fixed64.FromDouble(-3), Fixed64.Zero));

            AnimationCurve result = source.ToAnimationCurve();

            Assert.That(result.preWrapMode, Is.EqualTo(WrapMode.ClampForever));
            Assert.That(result.postWrapMode, Is.EqualTo(WrapMode.ClampForever));
            foreach (Keyframe key in result.keys)
                Assert.That(key.weightedMode, Is.EqualTo(WeightedMode.None));

            foreach (float time in new[] { 0f, 0.5f, 1.5f, 2f, 3f, 4.5f, 5f })
                Assert.That(result.Evaluate(time), Is.EqualTo((float)source.Evaluate(Fixed64.FromDouble(time))).Within(0.001f));

            Keyframe[] keys = result.keys;
            if (mode == FixedCurveMode.Step)
            {
                Assert.That(keys[0].outTangent, Is.EqualTo(float.PositiveInfinity));
                Assert.That(keys[1].inTangent, Is.EqualTo(float.PositiveInfinity));
            }
            else if (mode == FixedCurveMode.Cubic)
            {
                Assert.That(keys[0].outTangent, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(keys[1].inTangent, Is.EqualTo(-0.5f).Within(0.0001f));
                Assert.That(keys[1].outTangent, Is.EqualTo(1.25f).Within(0.0001f));
                Assert.That(keys[2].inTangent, Is.EqualTo(-1f).Within(0.0001f));
            }
        }

        [Test]
        public void ToAnimationCurve_MapsEmptyFixedCurveToConstantOne()
        {
            AnimationCurve result = new FixedCurve(FixedCurveMode.Linear, Array.Empty<FixedCurveKey>()).ToAnimationCurve();
            Assert.That(result.Evaluate(-100f), Is.EqualTo(1f));
            Assert.That(result.Evaluate(100f), Is.EqualTo(1f));
        }

        [Test]
        public void ToAnimationCurve_RejectsInvalidStorageModeAndTimes()
        {
            FixedCurve defaultCurve = default;
            FixedCurve undefinedMode = new((FixedCurveMode)255, new FixedCurveKey(0, 0), new FixedCurveKey(1, 1));
            FixedCurve duplicateTimes = new(FixedCurveMode.Linear, new FixedCurveKey(0, 0), new FixedCurveKey(0, 1));
            FixedCurve floatCollapsedTimes = new(
                FixedCurveMode.Linear,
                FixedCurveKey.FromDouble(16_777_216, 0),
                FixedCurveKey.FromDouble(16_777_217, 1));

            Assert.Throws<InvalidOperationException>(() => defaultCurve.ToAnimationCurve());
            Assert.Throws<InvalidOperationException>(() => undefinedMode.ToAnimationCurve());
            Assert.Throws<InvalidOperationException>(() => duplicateTimes.ToAnimationCurve());
            Assert.Throws<InvalidOperationException>(() => floatCollapsedTimes.ToAnimationCurve());
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
