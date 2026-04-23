using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for converting Unity Matrix4x4 values into FixedMathSharp matrix types.
    /// </summary>
    public static class Matrix4x4UnityExtensions
    {
        /// <summary>
        /// Converts a Unity Matrix4x4 into a FixedMathSharp Fixed4x4.
        /// </summary>
        /// <remarks>
        /// This performs a semantic matrix conversion rather than a raw field copy. Unity stores translation in
        /// <c>m03/m13/m23</c>, while FixedMathSharp stores translation in <c>m30/m31/m32</c>. This method remaps
        /// those elements so transformed points behave consistently across both matrix types.
        /// </remarks>
        /// <param name="matrix">The Unity Matrix4x4 to convert.</param>
        /// <returns>A Fixed4x4 with equivalent transform semantics.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4(this Matrix4x4 matrix)
        {
            return new Fixed4x4(
                (Fixed64)matrix.m00, (Fixed64)matrix.m01, (Fixed64)matrix.m02, (Fixed64)matrix.m30,
                (Fixed64)matrix.m10, (Fixed64)matrix.m11, (Fixed64)matrix.m12, (Fixed64)matrix.m31,
                (Fixed64)matrix.m20, (Fixed64)matrix.m21, (Fixed64)matrix.m22, (Fixed64)matrix.m32,
                (Fixed64)matrix.m03, (Fixed64)matrix.m13, (Fixed64)matrix.m23, (Fixed64)matrix.m33
            );
        }

        /// <summary>
        /// Converts a Unity Matrix4x4 into a FixedMathSharp Fixed3x3 by copying the upper-left 3x3 region as-is.
        /// </summary>
        /// <remarks>
        /// Use this when the source matrix should preserve both rotation and scale in the resulting Fixed3x3.
        /// Translation and perspective terms are ignored.
        /// </remarks>
        /// <param name="matrix">The Unity Matrix4x4 to convert.</param>
        /// <returns>A Fixed3x3 containing the upper-left 3x3 region of the source matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3RotationScaleMatrix(this Matrix4x4 matrix)
        {
            return new Fixed3x3(
                (Fixed64)matrix.m00, (Fixed64)matrix.m01, (Fixed64)matrix.m02,
                (Fixed64)matrix.m10, (Fixed64)matrix.m11, (Fixed64)matrix.m12,
                (Fixed64)matrix.m20, (Fixed64)matrix.m21, (Fixed64)matrix.m22
            );
        }

        /// <summary>
        /// Converts a Unity Matrix4x4 into a FixedMathSharp Fixed3x3 rotation matrix.
        /// </summary>
        /// <remarks>
        /// This first copies the upper-left 3x3 region, then normalizes it so the result represents rotation only.
        /// Use <see cref="ToFixed3x3RotationScaleMatrix(UnityEngine.Matrix4x4)"/> when scale should be preserved.
        /// </remarks>
        /// <param name="matrix">The Unity Matrix4x4 to convert.</param>
        /// <returns>A normalized Fixed3x3 rotation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3RotationMatrix(this Matrix4x4 matrix)
        {
            Fixed3x3 rotationScaleMatrix = matrix.ToFixed3x3RotationScaleMatrix();
            return rotationScaleMatrix.Normalize();
        }
    }
}
