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

            unityMatrix.m00 = (float)matrix.m00;
            unityMatrix.m01 = (float)matrix.m01;
            unityMatrix.m02 = (float)matrix.m02;

            unityMatrix.m10 = (float)matrix.m10;
            unityMatrix.m11 = (float)matrix.m11;
            unityMatrix.m12 = (float)matrix.m12;

            unityMatrix.m20 = (float)matrix.m20;
            unityMatrix.m21 = (float)matrix.m21;
            unityMatrix.m22 = (float)matrix.m22;

            return unityMatrix;
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
            Fixed3x3 normalizedMatrix = matrix.Normalize();
            return FixedQuaternion.FromMatrix(normalizedMatrix).ToQuaternion();
        }

        private static void DecomposeRotationScale(Fixed3x3 matrix, out FixedQuaternion rotation, out Vector3d scale)
        {
            scale = Fixed3x3.ExtractScale(matrix);

            Fixed64 scaleX = scale.x == Fixed64.Zero ? Fixed64.One : scale.x;
            Fixed64 scaleY = scale.y == Fixed64.Zero ? Fixed64.One : scale.y;
            Fixed64 scaleZ = scale.z == Fixed64.Zero ? Fixed64.One : scale.z;

            Fixed3x3 normalizedMatrix = new Fixed3x3(
                matrix.m00 / scaleX, matrix.m01 / scaleX, matrix.m02 / scaleX,
                matrix.m10 / scaleY, matrix.m11 / scaleY, matrix.m12 / scaleY,
                matrix.m20 / scaleZ, matrix.m21 / scaleZ, matrix.m22 / scaleZ
            );

            Fixed64 determinant = normalizedMatrix.GetDeterminant();
            if (determinant < Fixed64.Zero)
            {
                scale.x = -scale.x;
                normalizedMatrix.m00 = -normalizedMatrix.m00;
                normalizedMatrix.m01 = -normalizedMatrix.m01;
                normalizedMatrix.m02 = -normalizedMatrix.m02;
            }

            rotation = FixedQuaternion.FromMatrix(normalizedMatrix.Normalize());
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
