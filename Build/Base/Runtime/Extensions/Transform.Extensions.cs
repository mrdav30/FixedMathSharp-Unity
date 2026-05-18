using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for converting Unity transforms into FixedMathSharp matrix types.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Converts a Unity Transform into a world-space Fixed4x4 transform matrix.
        /// </summary>
        /// <param name="transform">The Unity Transform to convert.</param>
        /// <returns>A Fixed4x4 representing the transform's world position, rotation, and scale.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4WorldMatrix(this Transform transform)
        {
            return Fixed4x4.CreateTransform(
                transform.position.ToVector3d(),
                transform.rotation.ToFixedQuaternion(),
                transform.lossyScale.ToVector3d());
        }

        /// <summary>
        /// Converts a Unity Transform into a local-space Fixed4x4 transform matrix.
        /// </summary>
        /// <param name="transform">The Unity Transform to convert.</param>
        /// <returns>A Fixed4x4 representing the transform's local position, rotation, and scale.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4LocalMatrix(this Transform transform)
        {
            return Fixed4x4.CreateTransform(
                transform.localPosition.ToVector3d(),
                transform.localRotation.ToFixedQuaternion(),
                transform.localScale.ToVector3d());
        }

        /// <summary>
        /// Converts a Unity Transform into a world-space Fixed3x3 rotation matrix.
        /// </summary>
        /// <param name="transform">The Unity Transform to convert.</param>
        /// <returns>A Fixed3x3 representing the transform's world rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3WorldRotationMatrix(this Transform transform)
        {
            return transform.rotation.ToFixedQuaternion().ToMatrix3x3();
        }

        /// <summary>
        /// Converts a Unity Transform into a local-space Fixed3x3 rotation matrix.
        /// </summary>
        /// <param name="transform">The Unity Transform to convert.</param>
        /// <returns>A Fixed3x3 representing the transform's local rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3LocalRotationMatrix(this Transform transform)
        {
            return transform.localRotation.ToFixedQuaternion().ToMatrix3x3();
        }

        /// <summary>
        /// Converts a Unity Transform into a world-space Fixed3x3 rotation-scale matrix.
        /// </summary>
        /// <param name="transform">The Unity Transform to convert.</param>
        /// <returns>
        /// A Fixed3x3 representing the transform's world rotation with lossy scale applied to each basis row.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3WorldRotationScaleMatrix(this Transform transform)
        {
            return CreateRotationScaleMatrix(
                transform.rotation.ToFixedQuaternion(),
                transform.lossyScale.ToVector3d());
        }

        /// <summary>
        /// Converts a Unity Transform into a local-space Fixed3x3 rotation-scale matrix.
        /// </summary>
        /// <param name="transform">The Unity Transform to convert.</param>
        /// <returns>
        /// A Fixed3x3 representing the transform's local rotation with local scale applied to each basis row.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3LocalRotationScaleMatrix(this Transform transform)
        {
            return CreateRotationScaleMatrix(
                transform.localRotation.ToFixedQuaternion(),
                transform.localScale.ToVector3d());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed3x3 CreateRotationScaleMatrix(FixedQuaternion rotation, Vector3d scale)
        {
            Fixed3x3 matrix = rotation.ToMatrix3x3();

            matrix.m00 *= scale.x;
            matrix.m01 *= scale.x;
            matrix.m02 *= scale.x;

            matrix.m10 *= scale.y;
            matrix.m11 *= scale.y;
            matrix.m12 *= scale.y;

            matrix.m20 *= scale.z;
            matrix.m21 *= scale.z;
            matrix.m22 *= scale.z;

            return matrix;
        }
    }
}
