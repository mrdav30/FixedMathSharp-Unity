using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for applying FixedMathSharp 4x4 matrices to Unity transforms.
    /// </summary>
    /// <remarks>
    /// Unity and FixedMathSharp share the canonical 3D basis of <c>+X</c> right, <c>+Y</c> up,
    /// and <c>+Z</c> forward. Matrix and transform helpers still perform semantic conversions
    /// because FixedMathSharp uses row-vector transform matrices with translation in
    /// <c>M41/M42/M43</c>, while Unity exposes translation through <c>m03/m13/m23</c> and
    /// <see cref="Transform"/> properties.
    /// </remarks>
    public static class Fixed4x4UnityExtensions
    {
        /// <summary>
        /// Converts a FixedMathSharp Fixed4x4 into a Unity Matrix4x4.
        /// </summary>
        /// <remarks>
        /// This performs a semantic matrix conversion rather than a raw field copy. FixedMathSharp stores
        /// translation in <c>M41/M42/M43</c>, while Unity stores translation in <c>m03/m13/m23</c>. This method
        /// remaps those elements so transformed points behave consistently across both matrix types while
        /// preserving the shared <c>+Z</c> forward basis.
        /// </remarks>
        /// <param name="matrix">The Fixed4x4 to convert.</param>
        /// <returns>A Unity Matrix4x4 with equivalent transform semantics.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ToMatrix4x4(this Fixed4x4 matrix)
        {
            Matrix4x4 unityMatrix = new Matrix4x4();

            unityMatrix.m00 = (float)matrix.M11;
            unityMatrix.m01 = (float)matrix.M12;
            unityMatrix.m02 = (float)matrix.M13;
            unityMatrix.m03 = (float)matrix.M41;

            unityMatrix.m10 = (float)matrix.M21;
            unityMatrix.m11 = (float)matrix.M22;
            unityMatrix.m12 = (float)matrix.M23;
            unityMatrix.m13 = (float)matrix.M42;

            unityMatrix.m20 = (float)matrix.M31;
            unityMatrix.m21 = (float)matrix.M32;
            unityMatrix.m22 = (float)matrix.M33;
            unityMatrix.m23 = (float)matrix.M43;

            unityMatrix.m30 = (float)matrix.M14;
            unityMatrix.m31 = (float)matrix.M24;
            unityMatrix.m32 = (float)matrix.M34;
            unityMatrix.m33 = (float)matrix.M44;

            return unityMatrix;
        }

        /// <summary>
        /// Converts a Unity Matrix4x4 into a FixedMathSharp Fixed4x4.
        /// </summary>
        /// <remarks>
        /// This performs a semantic matrix conversion rather than a raw field copy. Unity stores translation in
        /// <c>m03/m13/m23</c>, while FixedMathSharp stores translation in <c>M41/M42/M43</c>. This method remaps
        /// those elements so transformed points behave consistently across both matrix types while preserving
        /// the shared <c>+Z</c> forward basis.
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
        /// Creates a Unity Transform from a local-space Fixed4x4 matrix.
        /// </summary>
        /// <remarks>
        /// Decomposes the FixedMathSharp row-vector matrix into Unity local position, rotation, and scale
        /// values. The resulting transform preserves the canonical <c>+Z</c> forward basis.
        /// </remarks>
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
        /// <remarks>
        /// Decomposes the FixedMathSharp row-vector matrix into Unity world-space transform semantics.
        /// Parent-space conversion is handled explicitly when a parent is supplied.
        /// </remarks>
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
        /// <remarks>
        /// This is a semantic transform application: the FixedMathSharp matrix is decomposed into Unity
        /// local properties rather than copied into a Unity matrix field-by-field.
        /// </remarks>
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
        /// <remarks>
        /// This preserves world-space right/up/forward behavior in Unity. For parented transforms, the
        /// FixedMathSharp world matrix is converted through the parent's inverse world matrix before
        /// applying local Unity properties.
        /// </remarks>
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

        /// <summary>
        /// Converts a Unity Transform into a world-space Fixed4x4 transform matrix.
        /// </summary>
        /// <remarks>
        /// Uses Unity's world-space position, rotation, and lossy scale to build a FixedMathSharp
        /// row-vector transform matrix in the shared <c>+Z</c> forward basis.
        /// </remarks>
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
        /// <remarks>
        /// Uses Unity's local position, rotation, and scale to build a FixedMathSharp row-vector transform
        /// matrix in the shared <c>+Z</c> forward basis.
        /// </remarks>
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DecomposeToUnityTransform(Fixed4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            Fixed4x4.Decompose(matrix, out Vector3d fixedTranslation, out FixedQuaternion fixedRotation, out Vector3d fixedScale);

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
