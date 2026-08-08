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
        /// and has finite, strictly increasing times representable as <see cref="Fixed64" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve" /> is <see langword="null" />.</exception>
        /// <remarks>
        /// A successful conversion preserves clamped wrapping exactly. Curves with weighted keys, mixed step and Hermite segments,
        /// non-finite values, or times that collapse or overflow in fixed-point representation fail safely without returning partial data.
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

        private static FixedCurve CreateConstant(Fixed64 value) =>
            new(FixedCurveMode.Linear, new FixedCurveKey(Fixed64.Zero, value));

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
