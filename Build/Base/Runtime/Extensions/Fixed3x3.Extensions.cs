using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for applying FixedMathSharp 3x3 matrices to Unity transforms.
    /// </summary>
    public static class Fixed3x3UnityExtensions
    {
        /// <summary>
        /// Converts a FixedMathSharp Fixed3x3 into a Unity Matrix4x4 by copying it into the upper-left 3x3 region.
        /// </summary>
        /// <remarks>
        /// Translation is set to zero and the homogeneous bottom-right element is set to one. The upper-left 3x3 is
        /// preserved as-is, so this works for both rotation-only and rotation-scale matrices.
        /// </remarks>
        /// <param name="matrix">The Fixed3x3 to convert.</param>
        /// <returns>A Unity Matrix4x4 containing the Fixed3x3 in its upper-left region.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ToMatrix4x4(this Fixed3x3 matrix)
        {
            Matrix4x4 unityMatrix = Matrix4x4.identity;

            unityMatrix.m00 = (float)matrix.M11;
            unityMatrix.m01 = (float)matrix.M12;
            unityMatrix.m02 = (float)matrix.M13;

            unityMatrix.m10 = (float)matrix.M21;
            unityMatrix.m11 = (float)matrix.M22;
            unityMatrix.m12 = (float)matrix.M23;

            unityMatrix.m20 = (float)matrix.M31;
            unityMatrix.m21 = (float)matrix.M32;
            unityMatrix.m22 = (float)matrix.M33;

            return unityMatrix;
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
            return rotationScaleMatrix.NormalizeInPlace();
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

            matrix.M11 *= scale.X;
            matrix.M12 *= scale.X;
            matrix.M13 *= scale.X;

            matrix.M21 *= scale.Y;
            matrix.M22 *= scale.Y;
            matrix.M23 *= scale.Y;

            matrix.M31 *= scale.Z;
            matrix.M32 *= scale.Z;
            matrix.M33 *= scale.Z;

            return matrix;
        }

        /// <summary>
        /// Creates a Unity Transform from a local-space Fixed3x3 rotation matrix.
        /// </summary>
        /// <param name="matrix">The local-space rotation matrix to apply.</param>
        /// <param name="name">The name to use for the created GameObject.</param>
        /// <param name="parent">An optional parent for the created Transform.</param>
        /// <returns>A new Unity Transform initialized from the matrix's rotation.</returns>
        public static Transform ToTransformRotationLocal(this Fixed3x3 matrix, string name = "Fixed3x3 Rotation Transform", Transform parent = null!)
        {
            Transform transform = CreateTransform(name, parent);
            matrix.ApplyRotationToTransformLocal(transform);
            return transform;
        }

        /// <summary>
        /// Creates a Unity Transform from a world-space Fixed3x3 rotation matrix.
        /// </summary>
        /// <param name="matrix">The world-space rotation matrix to apply.</param>
        /// <param name="name">The name to use for the created GameObject.</param>
        /// <param name="parent">An optional parent for the created Transform.</param>
        /// <returns>A new Unity Transform initialized from the matrix's rotation.</returns>
        public static Transform ToTransformRotationWorld(this Fixed3x3 matrix, string name = "Fixed3x3 Rotation Transform", Transform parent = null!)
        {
            Transform transform = CreateTransform(name, parent);
            matrix.ApplyRotationToTransformWorld(transform);
            return transform;
        }

        /// <summary>
        /// Creates a Unity Transform from a local-space Fixed3x3 rotation-scale matrix.
        /// </summary>
        /// <param name="matrix">The local-space rotation-scale matrix to apply.</param>
        /// <param name="name">The name to use for the created GameObject.</param>
        /// <param name="parent">An optional parent for the created Transform.</param>
        /// <returns>A new Unity Transform initialized from the matrix's rotation and scale.</returns>
        public static Transform ToTransformRotationScaleLocal(this Fixed3x3 matrix, string name = "Fixed3x3 RotationScale Transform", Transform parent = null!)
        {
            Transform transform = CreateTransform(name, parent);
            matrix.ApplyRotationScaleToTransformLocal(transform);
            return transform;
        }

        /// <summary>
        /// Creates a Unity Transform from a world-space Fixed3x3 rotation-scale matrix.
        /// </summary>
        /// <param name="matrix">The world-space rotation-scale matrix to apply.</param>
        /// <param name="name">The name to use for the created GameObject.</param>
        /// <param name="parent">An optional parent for the created Transform.</param>
        /// <returns>A new Unity Transform initialized from the matrix's rotation and scale.</returns>
        public static Transform ToTransformRotationScaleWorld(this Fixed3x3 matrix, string name = "Fixed3x3 RotationScale Transform", Transform parent = null!)
        {
            Transform transform = CreateTransform(name, parent);
            matrix.ApplyRotationScaleToTransformWorld(transform);
            return transform;
        }

        /// <summary>
        /// Applies a local-space Fixed3x3 rotation matrix to a Unity Transform.
        /// </summary>
        /// <param name="matrix">The local-space rotation matrix to apply.</param>
        /// <param name="transform">The target Unity Transform.</param>
        public static void ApplyRotationToTransformLocal(this Fixed3x3 matrix, Transform transform)
        {
            transform.localRotation = ExtractUnityRotation(matrix);
        }

        /// <summary>
        /// Applies a world-space Fixed3x3 rotation matrix to a Unity Transform.
        /// </summary>
        /// <param name="matrix">The world-space rotation matrix to apply.</param>
        /// <param name="transform">The target Unity Transform.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the target has a parent whose world rotation matrix is not invertible.
        /// </exception>
        public static void ApplyRotationToTransformWorld(this Fixed3x3 matrix, Transform transform)
        {
            if (transform.parent == null)
            {
                transform.rotation = ExtractUnityRotation(matrix);
                return;
            }

            Fixed3x3 parentWorldRotationMatrix = transform.parent.ToFixed3x3WorldRotationMatrix();
            if (!Fixed3x3.Invert(parentWorldRotationMatrix, out Fixed3x3? parentWorldRotationInverse) || !parentWorldRotationInverse.HasValue)
                throw new InvalidOperationException("Cannot apply a world-space Fixed3x3 rotation to a parented Transform when the parent's world rotation matrix is not invertible.");

            Fixed3x3 localRotationMatrix = matrix * parentWorldRotationInverse.Value;
            localRotationMatrix.ApplyRotationToTransformLocal(transform);
        }

        /// <summary>
        /// Applies a local-space Fixed3x3 rotation-scale matrix to a Unity Transform.
        /// </summary>
        /// <param name="matrix">The local-space rotation-scale matrix to apply.</param>
        /// <param name="transform">The target Unity Transform.</param>
        public static void ApplyRotationScaleToTransformLocal(this Fixed3x3 matrix, Transform transform)
        {
            DecomposeRotationScale(matrix, out FixedQuaternion rotation, out Vector3d scale);

            transform.localRotation = rotation.ToQuaternion();
            transform.localScale = scale.ToVector3();
        }

        /// <summary>
        /// Applies a world-space Fixed3x3 rotation-scale matrix to a Unity Transform.
        /// </summary>
        /// <param name="matrix">The world-space rotation-scale matrix to apply.</param>
        /// <param name="transform">The target Unity Transform.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the target has a parent whose world rotation-scale matrix is not invertible.
        /// </exception>
        public static void ApplyRotationScaleToTransformWorld(this Fixed3x3 matrix, Transform transform)
        {
            if (transform.parent == null)
            {
                DecomposeRotationScale(matrix, out FixedQuaternion rotation, out Vector3d scale);

                transform.rotation = rotation.ToQuaternion();
                transform.localScale = scale.ToVector3();
                return;
            }

            Fixed3x3 parentWorldRotationScaleMatrix = transform.parent.ToFixed3x3WorldRotationScaleMatrix();
            if (!Fixed3x3.Invert(parentWorldRotationScaleMatrix, out Fixed3x3? parentWorldRotationScaleInverse) || !parentWorldRotationScaleInverse.HasValue)
                throw new InvalidOperationException("Cannot apply a world-space Fixed3x3 rotation-scale matrix to a parented Transform when the parent's world rotation-scale matrix is not invertible.");

            Fixed3x3 localRotationScaleMatrix = matrix * parentWorldRotationScaleInverse.Value;
            localRotationScaleMatrix.ApplyRotationScaleToTransformLocal(transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quaternion ExtractUnityRotation(Fixed3x3 matrix)
        {
            Fixed3x3 normalizedMatrix = Fixed3x3.GetNormalized(matrix);
            return FixedQuaternion.FromMatrix(normalizedMatrix).ToQuaternion();
        }

        private static void DecomposeRotationScale(Fixed3x3 matrix, out FixedQuaternion rotation, out Vector3d scale)
        {
            scale = Fixed3x3.ExtractScale(matrix);

            Fixed64 scaleX = scale.X == Fixed64.Zero ? Fixed64.One : scale.X;
            Fixed64 scaleY = scale.Y == Fixed64.Zero ? Fixed64.One : scale.Y;
            Fixed64 scaleZ = scale.Z == Fixed64.Zero ? Fixed64.One : scale.Z;

            Fixed3x3 normalizedMatrix = new Fixed3x3(
                matrix.M11 / scaleX, matrix.M12 / scaleX, matrix.M13 / scaleX,
                matrix.M21 / scaleY, matrix.M22 / scaleY, matrix.M23 / scaleY,
                matrix.M31 / scaleZ, matrix.M32 / scaleZ, matrix.M33 / scaleZ
            );

            Fixed64 determinant = normalizedMatrix.GetDeterminant();
            if (determinant < Fixed64.Zero)
            {
                scale.X = -scale.X;
                normalizedMatrix.M11 = -normalizedMatrix.M11;
                normalizedMatrix.M12 = -normalizedMatrix.M12;
                normalizedMatrix.M13 = -normalizedMatrix.M13;
            }

            rotation = FixedQuaternion.FromMatrix(Fixed3x3.GetNormalized(normalizedMatrix));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Transform CreateTransform(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name);
            Transform transform = gameObject.transform;

            if (parent != null)
                transform.SetParent(parent, false);

            return transform;
        }
    }
}
