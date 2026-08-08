//=======================================================================
// FixedCurve.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using System;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides explicit conversions between Unity animation curves and deterministic fixed curves.
    /// </summary>
    public static class FixedCurveUnityExtensions
    {
        /// <summary>
        /// Attempts to convert a Unity animation curve to an exactly equivalent deterministic fixed curve.
        /// </summary>
        /// <param name="curve">The Unity curve to convert. It must use <see cref="WrapMode.ClampForever" /> before and after its keys.</param>
        /// <param name="fixedCurve">The exactly equivalent fixed curve on success; otherwise a zero-valued constant curve.</param>
        /// <returns>
        /// <see langword="true" /> when <paramref name="curve" /> is unweighted, uses one global step or Hermite interpolation mode,
        /// and has finite values, strictly increasing times, and duration-scaled tangents representable as <see cref="Fixed64" />;
        /// otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve" /> is <see langword="null" />.</exception>
        /// <remarks>
        /// A successful conversion preserves clamped wrapping exactly and scales Hermite tangents by the adjacent segment duration.
        /// Curves with weighted keys, mixed step and Hermite segments, non-finite values, unrepresentable scaled tangents, or times that
        /// collapse or overflow in fixed-point representation fail safely without returning partial data.
        /// </remarks>
        public static bool TryToFixedCurve(this AnimationCurve curve, out FixedCurve fixedCurve)
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));

            fixedCurve = CreateConstant(Fixed64.Zero);
            if (curve.preWrapMode != WrapMode.ClampForever || curve.postWrapMode != WrapMode.ClampForever)
                return false;

            Keyframe[] unityKeys = curve.keys;
            var fixedKeys = new FixedCurveKey[unityKeys.Length];

            try
            {
                for (int i = 0; i < unityKeys.Length; i++)
                {
                    if (unityKeys[i].weightedMode != WeightedMode.None)
                        return false;

                    fixedKeys[i] = FixedCurveKey.FromDouble(unityKeys[i].time, unityKeys[i].value);
                    if (i > 0 && (unityKeys[i].time <= unityKeys[i - 1].time || fixedKeys[i].Time <= fixedKeys[i - 1].Time))
                        return false;
                }

                bool allStep = unityKeys.Length > 1;
                bool allHermite = true;
                for (int i = 0; i < unityKeys.Length - 1; i++)
                {
                    bool step = float.IsPositiveInfinity(unityKeys[i].outTangent)
                        && float.IsPositiveInfinity(unityKeys[i + 1].inTangent);
                    bool hermite = IsFinite(unityKeys[i].outTangent)
                        && IsFinite(unityKeys[i + 1].inTangent);
                    if (!step && !hermite)
                        return false;

                    allStep &= step;
                    allHermite &= hermite;
                }

                if (!allStep && !allHermite)
                    return false;

                FixedCurveMode mode = allStep ? FixedCurveMode.Step : FixedCurveMode.Cubic;
                if (mode == FixedCurveMode.Cubic)
                {
                    for (int i = 0; i < unityKeys.Length - 1; i++)
                    {
                        double duration = (double)unityKeys[i + 1].time - unityKeys[i].time;
                        fixedKeys[i].OutTangent = Fixed64.FromDouble(unityKeys[i].outTangent * duration);
                        fixedKeys[i + 1].InTangent = Fixed64.FromDouble(unityKeys[i + 1].inTangent * duration);
                    }
                }

                fixedCurve = unityKeys.Length == 0
                    ? CreateConstant(Fixed64.Zero)
                    : new FixedCurve(mode, fixedKeys);
                return true;
            }
            catch (Exception exception) when (exception is ArgumentOutOfRangeException || exception is OverflowException)
            {
                fixedCurve = CreateConstant(Fixed64.Zero);
                return false;
            }
        }

        /// <summary>
        /// Converts a Unity animation curve to an exactly equivalent deterministic fixed curve.
        /// </summary>
        /// <param name="curve">The Unity curve to convert.</param>
        /// <returns>The exactly equivalent fixed curve.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="curve" /> cannot be represented exactly; use <c>BakeToFixedCurve</c> for an explicit sampled approximation.
        /// </exception>
        public static FixedCurve ToFixedCurve(this AnimationCurve curve)
        {
            if (curve.TryToFixedCurve(out FixedCurve fixedCurve))
                return fixedCurve;

            throw new InvalidOperationException(
                "The Unity AnimationCurve cannot be represented exactly as a FixedCurve. Use BakeToFixedCurve for explicit sampled approximation.");
        }

        /// <summary>
        /// Samples a Unity animation curve at inclusive, evenly spaced endpoints and converts the samples to a linear fixed curve.
        /// </summary>
        /// <param name="curve">The Unity curve to sample at the authoring boundary.</param>
        /// <param name="sampleCount">The number of samples, including both authored endpoints. Must be at least two.</param>
        /// <returns>A linear fixed curve approximating <paramref name="curve" /> at the requested samples.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sampleCount" /> is less than two, or a sampled time or value is not finite.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The authored time span is not finite and positive, or the requested sample times are not distinct in the fixed-point domain.
        /// </exception>
        /// <exception cref="OverflowException">A sampled time or value is outside the representable fixed-point range.</exception>
        /// <remarks>
        /// This is an explicit approximation for Unity authoring data and preserves neither tangents nor wrapping modes. Empty and one-key
        /// curves become constant fixed curves with zero and the authored key value respectively.
        /// </remarks>
        public static FixedCurve BakeToFixedCurve(this AnimationCurve curve, int sampleCount)
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve));
            if (sampleCount < 2)
                throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount, "At least two samples are required.");

            Keyframe[] unityKeys = curve.keys;
            if (unityKeys.Length == 0)
                return CreateConstant(Fixed64.Zero);
            if (unityKeys.Length == 1)
                return CreateConstant(Fixed64.FromDouble(unityKeys[0].value));

            double start = unityKeys[0].time;
            double end = unityKeys[^1].time;
            double span = end - start;
            if (double.IsNaN(span) || double.IsInfinity(span) || span <= 0d)
                throw new InvalidOperationException("The curve must have a finite, positive authored time span.");

            var samples = new FixedCurveKey[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                double interpolated = i == sampleCount - 1
                    ? end
                    : start + span * i / (sampleCount - 1);
                float sampleTime = (float)interpolated;
                samples[i] = FixedCurveKey.FromDouble(sampleTime, curve.Evaluate(sampleTime));
                if (i > 0 && samples[i].Time <= samples[i - 1].Time)
                    throw new InvalidOperationException("The requested sample times are not distinct in the fixed-point domain.");
            }

            return new FixedCurve(FixedCurveMode.Linear, samples);
        }

        /// <summary>
        /// Converts a fixed curve to a clamped, unweighted Unity animation curve.
        /// </summary>
        /// <param name="curve">The deterministic fixed curve to convert.</param>
        /// <returns>A Unity curve with <see cref="WeightedMode.None" /> keys and <see cref="WrapMode.ClampForever" /> wrapping.</returns>
        /// <exception cref="InvalidOperationException">
        /// The fixed curve has no storage, an unsupported mode, or keys/tangents that cannot remain finite and strictly ordered after
        /// conversion to Unity floats.
        /// </exception>
        /// <remarks>
        /// Linear, step, smooth, and cubic modes map to Unity linear slopes, positive-infinity step tangents, zero tangents, and
        /// duration-scaled cubic tangents respectively. Endpoint-only tangents remain zero. An empty fixed curve maps to a clamped
        /// constant-one Unity curve.
        /// </remarks>
        public static AnimationCurve ToAnimationCurve(this FixedCurve curve)
        {
            FixedCurveKey[] fixedKeys = curve.Keyframes;
            if (fixedKeys is null)
                throw new InvalidOperationException("A default FixedCurve has no keyframe storage.");
            if (curve.Mode != FixedCurveMode.Linear
                && curve.Mode != FixedCurveMode.Step
                && curve.Mode != FixedCurveMode.Smooth
                && curve.Mode != FixedCurveMode.Cubic)
                throw new InvalidOperationException($"Unsupported FixedCurveMode value: {curve.Mode}.");
            if (fixedKeys.Length == 0)
                return CreateClampedUnityCurve(new Keyframe(0f, 1f));

            var unityKeys = new Keyframe[fixedKeys.Length];
            for (int i = 0; i < fixedKeys.Length; i++)
            {
                float time = (float)fixedKeys[i].Time;
                float value = (float)fixedKeys[i].Value;
                if (!IsFinite(time) || !IsFinite(value) || (i > 0 && time <= unityKeys[i - 1].time))
                    throw new InvalidOperationException("Fixed curve keys must remain finite and strictly increasing after Unity float conversion.");

                unityKeys[i] = new Keyframe(time, value) { weightedMode = WeightedMode.None };
            }

            for (int i = 0; i < unityKeys.Length - 1; i++)
            {
                Keyframe left = unityKeys[i];
                Keyframe right = unityKeys[i + 1];
                float duration = right.time - left.time;

                switch (curve.Mode)
                {
                    case FixedCurveMode.Linear:
                        float slope = (right.value - left.value) / duration;
                        left.outTangent = slope;
                        right.inTangent = slope;
                        break;
                    case FixedCurveMode.Step:
                        left.outTangent = float.PositiveInfinity;
                        right.inTangent = float.PositiveInfinity;
                        break;
                    case FixedCurveMode.Smooth:
                        left.outTangent = 0f;
                        right.inTangent = 0f;
                        break;
                    case FixedCurveMode.Cubic:
                        left.outTangent = (float)((double)fixedKeys[i].OutTangent / duration);
                        right.inTangent = (float)((double)fixedKeys[i + 1].InTangent / duration);
                        break;
                }

                if (curve.Mode != FixedCurveMode.Step
                    && (!IsFinite(left.outTangent) || !IsFinite(right.inTangent)))
                {
                    throw new InvalidOperationException("Fixed curve tangents are not representable as finite Unity slopes.");
                }

                unityKeys[i] = left;
                unityKeys[i + 1] = right;
            }

            return CreateClampedUnityCurve(unityKeys);
        }

        private static FixedCurve CreateConstant(Fixed64 value) =>
            new(FixedCurveMode.Linear, new FixedCurveKey(Fixed64.Zero, value));

        private static AnimationCurve CreateClampedUnityCurve(params Keyframe[] keys)
        {
            var curve = new AnimationCurve(keys)
            {
                preWrapMode = WrapMode.ClampForever,
                postWrapMode = WrapMode.ClampForever
            };
            return curve;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
