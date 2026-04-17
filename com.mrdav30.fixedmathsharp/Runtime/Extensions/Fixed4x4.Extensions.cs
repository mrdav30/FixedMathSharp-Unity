using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for applying FixedMathSharp 4x4 matrices to Unity transforms.
    /// </summary>
    public static class Fixed4x4UnityExtensions
    {
        /// <summary>
        /// Converts a FixedMathSharp Fixed4x4 into a Unity Matrix4x4.
        /// </summary>
        /// <remarks>
        /// This performs a semantic matrix conversion rather than a raw field copy. FixedMathSharp stores
        /// translation in <c>m30/m31/m32</c>, while Unity stores translation in <c>m03/m13/m23</c>. This method
        /// remaps those elements so transformed points behave consistently across both matrix types.
        /// </remarks>
        /// <param name="matrix">The Fixed4x4 to convert.</param>
        /// <returns>A Unity Matrix4x4 with equivalent transform semantics.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ToMatrix4x4(this Fixed4x4 matrix)
        {
            Matrix4x4 unityMatrix = new Matrix4x4();

            unityMatrix.m00 = (float)matrix.m00;
            unityMatrix.m01 = (float)matrix.m01;
            unityMatrix.m02 = (float)matrix.m02;
            unityMatrix.m03 = (float)matrix.m30;

            unityMatrix.m10 = (float)matrix.m10;
            unityMatrix.m11 = (float)matrix.m11;
            unityMatrix.m12 = (float)matrix.m12;
            unityMatrix.m13 = (float)matrix.m31;

            unityMatrix.m20 = (float)matrix.m20;
            unityMatrix.m21 = (float)matrix.m21;
            unityMatrix.m22 = (float)matrix.m22;
            unityMatrix.m23 = (float)matrix.m32;

            unityMatrix.m30 = (float)matrix.m03;
            unityMatrix.m31 = (float)matrix.m13;
            unityMatrix.m32 = (float)matrix.m23;
            unityMatrix.m33 = (float)matrix.m33;

            return unityMatrix;
        }

        /// <summary>
        /// Creates a Unity Transform from a local-space Fixed4x4 matrix.
        /// </summary>
        /// <param name="matrix">The local-space matrix to apply.</param>
        /// <param name="name">The name to use for the created GameObject.</param>
        /// <param name="parent">An optional parent for the created Transform.</param>
        /// <returns>A new Unity Transform initialized from the matrix.</returns>
        public static Transform ToTransformLocal(this Fixed4x4 matrix, string name = "Fixed4x4 Transform", Transform parent = null!)
        {
            Transform transform = CreateTransform(name, parent);
            matrix.ApplyToTransformLocal(transform);
            return transform;
        }

        /// <summary>
        /// Creates a Unity Transform from a world-space Fixed4x4 matrix.
        /// </summary>
        /// <param name="matrix">The world-space matrix to apply.</param>
        /// <param name="name">The name to use for the created GameObject.</param>
        /// <param name="parent">An optional parent for the created Transform.</param>
        /// <returns>A new Unity Transform initialized from the matrix.</returns>
        public static Transform ToTransformWorld(this Fixed4x4 matrix, string name = "Fixed4x4 Transform", Transform parent = null!)
        {
            Transform transform = CreateTransform(name, parent);
            matrix.ApplyToTransformWorld(transform);
            return transform;
        }

        /// <summary>
        /// Applies a local-space Fixed4x4 transform matrix to a Unity Transform.
        /// </summary>
        /// <param name="matrix">The local-space matrix to apply.</param>
        /// <param name="transform">The target Unity Transform.</param>
        public static void ApplyToTransformLocal(this Fixed4x4 matrix, Transform transform)
        {
            DecomposeToUnityTransform(matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale);

            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        /// <summary>
        /// Applies a world-space Fixed4x4 transform matrix to a Unity Transform.
        /// </summary>
        /// <param name="matrix">The world-space matrix to apply.</param>
        /// <param name="transform">The target Unity Transform.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the target has a parent whose world matrix is not invertible, which prevents
        /// a stable conversion from world-space transform data into Unity local-space values.
        /// </exception>
        public static void ApplyToTransformWorld(this Fixed4x4 matrix, Transform transform)
        {
            if (transform.parent == null)
            {
                DecomposeToUnityTransform(matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale);

                transform.position = position;
                transform.rotation = rotation;
                transform.localScale = scale;
                return;
            }

            Fixed4x4 parentWorldMatrix = transform.parent.ToFixed4x4WorldMatrix();
            if (!Fixed4x4.Invert(parentWorldMatrix, out Fixed4x4 parentWorldInverse))
                throw new InvalidOperationException("Cannot apply a world-space Fixed4x4 to a parented Transform when the parent's world matrix is not invertible.");

            Fixed4x4 localMatrix = matrix * parentWorldInverse;
            localMatrix.ApplyToTransformLocal(transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DecomposeToUnityTransform(Fixed4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            Fixed4x4.Decompose(matrix, out Vector3d fixedScale, out FixedQuaternion fixedRotation, out Vector3d fixedTranslation);

            position = fixedTranslation.ToVector3();
            rotation = fixedRotation.ToQuaternion();
            scale = fixedScale.ToVector3();
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
