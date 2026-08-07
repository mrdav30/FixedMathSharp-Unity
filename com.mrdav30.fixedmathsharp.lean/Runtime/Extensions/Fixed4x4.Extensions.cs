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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ToMatrix4x4(this Fixed4x4 matrix)
        {
            Matrix4x4 unityMatrix = new();

            unityMatrix.m00 = (float)matrix.M11;
            unityMatrix.m01 = (float)matrix.M21;
            unityMatrix.m02 = (float)matrix.M31;
            unityMatrix.m03 = (float)matrix.M41;

            unityMatrix.m10 = (float)matrix.M12;
            unityMatrix.m11 = (float)matrix.M22;
            unityMatrix.m12 = (float)matrix.M32;
            unityMatrix.m13 = (float)matrix.M42;

            unityMatrix.m20 = (float)matrix.M13;
            unityMatrix.m21 = (float)matrix.M23;
            unityMatrix.m22 = (float)matrix.M33;
            unityMatrix.m23 = (float)matrix.M43;

            unityMatrix.m30 = (float)matrix.M14;
            unityMatrix.m31 = (float)matrix.M24;
            unityMatrix.m32 = (float)matrix.M34;
            unityMatrix.m33 = (float)matrix.M44;

            return unityMatrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4(this Matrix4x4 matrix)
        {
            return new Fixed4x4(
                (Fixed64)matrix.m00, (Fixed64)matrix.m10, (Fixed64)matrix.m20, (Fixed64)matrix.m30,
                (Fixed64)matrix.m01, (Fixed64)matrix.m11, (Fixed64)matrix.m21, (Fixed64)matrix.m31,
                (Fixed64)matrix.m02, (Fixed64)matrix.m12, (Fixed64)matrix.m22, (Fixed64)matrix.m32,
                (Fixed64)matrix.m03, (Fixed64)matrix.m13, (Fixed64)matrix.m23, (Fixed64)matrix.m33);
        }

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

        public static bool TryApplyToTransformLocal(this Fixed4x4 matrix, Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            if (!TryGetLocalUnityComponents(matrix, null, false, out Vector3 position, out Quaternion rotation, out Vector3 scale))
                return false;

            ApplyLocalComponents(transform, position, rotation, scale);
            return true;
        }

        public static void ApplyToTransformLocal(this Fixed4x4 matrix, Transform transform)
        {
            if (!matrix.TryApplyToTransformLocal(transform))
                throw new InvalidOperationException("The Fixed4x4 is not a strict local TRS matrix.");
        }

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

        public static void ApplyToTransformWorld(this Fixed4x4 matrix, Transform transform)
        {
            if (!matrix.TryApplyToTransformWorld(transform))
            {
                throw new InvalidOperationException(
                    "The Fixed4x4 cannot be represented by a Unity Transform in the target world hierarchy.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed4x4 ToFixed4x4WorldMatrix(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return transform.localToWorldMatrix.ToFixed4x4();
        }

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
