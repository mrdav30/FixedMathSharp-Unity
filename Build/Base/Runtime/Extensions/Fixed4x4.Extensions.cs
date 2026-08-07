//=======================================================================
// Fixed4x4.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides semantic conversions between FixedMathSharp row-vector matrices and Unity
    /// column-vector matrices, plus strict Unity Transform application.
    /// </summary>
    public static class Fixed4x4UnityExtensions
    {
        /// <summary>
        /// Converts a FixedMathSharp row-vector matrix to a Unity column-vector matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ToMatrix4x4(this Fixed4x4 matrix)
        {
            Matrix4x4 unityMatrix = new()
            {
                m00 = (float)matrix.M11,
                m01 = (float)matrix.M21,
                m02 = (float)matrix.M31,
                m03 = (float)matrix.M41,

                m10 = (float)matrix.M12,
                m11 = (float)matrix.M22,
                m12 = (float)matrix.M32,
                m13 = (float)matrix.M42,

                m20 = (float)matrix.M13,
                m21 = (float)matrix.M23,
                m22 = (float)matrix.M33,
                m23 = (float)matrix.M43,

                m30 = (float)matrix.M14,
                m31 = (float)matrix.M24,
                m32 = (float)matrix.M34,
                m33 = (float)matrix.M44
            };

            return unityMatrix;
        }

        /// <summary>
        /// Converts a Unity column-vector matrix to a FixedMathSharp row-vector matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4(this Matrix4x4 matrix) =>
            new Fixed4x4(
                (Fixed64)matrix.m00, (Fixed64)matrix.m10, (Fixed64)matrix.m20, (Fixed64)matrix.m30,
                (Fixed64)matrix.m01, (Fixed64)matrix.m11, (Fixed64)matrix.m21, (Fixed64)matrix.m31,
                (Fixed64)matrix.m02, (Fixed64)matrix.m12, (Fixed64)matrix.m22, (Fixed64)matrix.m32,
                (Fixed64)matrix.m03, (Fixed64)matrix.m13, (Fixed64)matrix.m23, (Fixed64)matrix.m33);

        /// <summary>
        /// Creates a Unity transform from a strict local TRS matrix.
        /// </summary>
        /// <param name="matrix">The local TRS matrix to decompose.</param>
        /// <param name="name">The new GameObject's name.</param>
        /// <param name="parent">The optional parent transform.</param>
        /// <returns>The created transform.</returns>
        /// <exception cref="InvalidOperationException">The matrix is not a strict local TRS matrix.</exception>
        public static Transform CreateTransformLocal(
            this Fixed4x4 matrix,
            string name = "Fixed4x4 Transform",
            Transform parent = null!)
        {
            if (!TryGetLocalUnityComponents(matrix, parent, false, out Vector3 position, out Quaternion rotation, out Vector3 scale))
                throw new InvalidOperationException("The Fixed4x4 is not a strict local TRS matrix.");

            Transform transform = CreateTransform(name, parent);
            ApplyLocalComponents(transform, position, rotation, scale);
            return transform;
        }

        /// <summary>
        /// Creates a Unity transform from a world matrix relative to an optional parent.
        /// </summary>
        /// <param name="matrix">The desired world matrix.</param>
        /// <param name="name">The new GameObject's name.</param>
        /// <param name="parent">The optional parent transform.</param>
        /// <returns>The created transform.</returns>
        /// <exception cref="InvalidOperationException">The matrix cannot be represented in the requested hierarchy.</exception>
        public static Transform CreateTransformWorld(
            this Fixed4x4 matrix,
            string name = "Fixed4x4 Transform",
            Transform parent = null!)
        {
            if (!TryGetLocalUnityComponents(matrix, parent, true, out Vector3 position, out Quaternion rotation, out Vector3 scale))
                throw new InvalidOperationException("The Fixed4x4 cannot be represented by a Unity Transform in the requested world hierarchy.");

            Transform transform = CreateTransform(name, parent);
            ApplyLocalComponents(transform, position, rotation, scale);
            return transform;
        }

        /// <summary>
        /// Tries to apply a strict local TRS matrix to a Unity transform.
        /// </summary>
        /// <returns><see langword="true" /> when the matrix can be decomposed as strict TRS; otherwise, <see langword="false" />.</returns>
        public static bool TryApplyToTransformLocal(this Fixed4x4 matrix, Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            if (!TryGetLocalUnityComponents(matrix, null, false, out Vector3 position, out Quaternion rotation, out Vector3 scale))
                return false;

            ApplyLocalComponents(transform, position, rotation, scale);
            return true;
        }

        /// <summary>
        /// Applies a strict local TRS matrix to a Unity transform.
        /// </summary>
        /// <exception cref="InvalidOperationException">The matrix is not a strict local TRS matrix.</exception>
        public static void ApplyToTransformLocal(this Fixed4x4 matrix, Transform transform)
        {
            if (!matrix.TryApplyToTransformLocal(transform))
                throw new InvalidOperationException("The Fixed4x4 is not a strict local TRS matrix.");
        }

        /// <summary>
        /// Tries to apply a world matrix within the Unity transform's current hierarchy.
        /// </summary>
        /// <returns><see langword="true" /> when the world matrix is representable by the target transform; otherwise, <see langword="false" />.</returns>
        public static bool TryApplyToTransformWorld(this Fixed4x4 matrix, Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            if (!TryGetLocalUnityComponents(
                matrix,
                transform.parent,
                true,
                out Vector3 position,
                out Quaternion rotation,
                out Vector3 scale))
            {
                return false;
            }

            ApplyLocalComponents(transform, position, rotation, scale);
            return true;
        }

        /// <summary>
        /// Applies a world matrix within the Unity transform's current hierarchy.
        /// </summary>
        /// <exception cref="InvalidOperationException">The matrix cannot be represented by the target transform.</exception>
        public static void ApplyToTransformWorld(this Fixed4x4 matrix, Transform transform)
        {
            if (!matrix.TryApplyToTransformWorld(transform))
            {
                throw new InvalidOperationException(
                    "The Fixed4x4 cannot be represented by a Unity Transform in the target world hierarchy.");
            }
        }

        /// <summary>
        /// Converts a Unity transform's local-to-world matrix to FixedMathSharp convention.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4WorldMatrix(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return transform.localToWorldMatrix.ToFixed4x4();
        }

        /// <summary>
        /// Creates a FixedMathSharp matrix from a Unity transform's local TRS components.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4LocalMatrix(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return Fixed4x4.CreateTransform(
                transform.localPosition.ToVector3d(),
                transform.localRotation.ToFixedQuaternion(),
                transform.localScale.ToVector3d());
        }

        private static bool TryGetLocalUnityComponents(
            Fixed4x4 matrix,
            Transform parent,
            bool matrixIsWorld,
            out Vector3 position,
            out Quaternion rotation,
            out Vector3 scale)
        {
            Fixed4x4 localMatrix = matrix;
            Fixed4x4 parentWorld = Fixed4x4.Identity;

            if (matrixIsWorld && parent != null)
            {
                try
                {
                    parentWorld = parent.localToWorldMatrix.ToFixed4x4();
                }
                catch (Exception exception) when (
                    exception is OverflowException || exception is ArgumentOutOfRangeException)
                {
                    return Fail(out position, out rotation, out scale);
                }

                if (!TryGetVerifiedInverse(parentWorld, out Fixed4x4 parentInverse)
                    || !Fixed4x4.TryMultiply(matrix, parentInverse, out localMatrix))
                {
                    return Fail(out position, out rotation, out scale);
                }
            }

            if (!Fixed4x4.Decompose(
                localMatrix,
                out Vector3d fixedPosition,
                out FixedQuaternion fixedRotation,
                out Vector3d fixedScale))
            {
                return Fail(out position, out rotation, out scale);
            }

            if (matrixIsWorld && parent != null)
            {
                Fixed4x4 recomposedLocal = Fixed4x4.CreateTransform(fixedPosition, fixedRotation, fixedScale);
                if (!Fixed4x4.TryMultiply(recomposedLocal, parentWorld, out Fixed4x4 recomposedWorld)
                    || !recomposedWorld.FuzzyEqualAbsolute(matrix, FixedMath.CanonicalSinCosErrorBound))
                {
                    return Fail(out position, out rotation, out scale);
                }
            }

            position = fixedPosition.ToVector3();
            rotation = fixedRotation.ToQuaternion();
            scale = fixedScale.ToVector3();
            return true;
        }

        private static bool TryGetVerifiedInverse(Fixed4x4 matrix, out Fixed4x4 inverse)
        {
            if (Fixed4x4.Invert(matrix, out Fixed4x4 candidate)
                && Fixed4x4.TryMultiply(matrix, candidate, out Fixed4x4 forwardRoundTrip)
                && Fixed4x4.TryMultiply(candidate, matrix, out Fixed4x4 reverseRoundTrip)
                && forwardRoundTrip.FuzzyEqualAbsolute(Fixed4x4.Identity, FixedMath.CanonicalSinCosErrorBound)
                && reverseRoundTrip.FuzzyEqualAbsolute(Fixed4x4.Identity, FixedMath.CanonicalSinCosErrorBound))
            {
                inverse = candidate;
                return true;
            }

            inverse = Fixed4x4.Identity;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Fail(out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = default;
            rotation = Quaternion.identity;
            scale = Vector3.one;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyLocalComponents(
            Transform transform,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale)
        {
            transform.SetLocalPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Transform CreateTransform(string name, Transform parent)
        {
            GameObject gameObject = new(name);
            Transform transform = gameObject.transform;
            if (parent != null)
                transform.SetParent(parent, false);

            return transform;
        }
    }
}
