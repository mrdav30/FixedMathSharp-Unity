//=======================================================================
// Fixed3x3.Extensions.cs
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
    /// Provides raw basis conversion and explicit rotation-only Unity interop for Fixed3x3.
    /// </summary>
    public static class Fixed3x3UnityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 ToMatrix4x4(this Fixed3x3 matrix)
        {
            Matrix4x4 unityMatrix = Matrix4x4.identity;
            unityMatrix.m00 = (float)matrix.M11;
            unityMatrix.m01 = (float)matrix.M21;
            unityMatrix.m02 = (float)matrix.M31;
            unityMatrix.m10 = (float)matrix.M12;
            unityMatrix.m11 = (float)matrix.M22;
            unityMatrix.m12 = (float)matrix.M32;
            unityMatrix.m20 = (float)matrix.M13;
            unityMatrix.m21 = (float)matrix.M23;
            unityMatrix.m22 = (float)matrix.M33;
            return unityMatrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3Basis(this Matrix4x4 matrix)
        {
            return new Fixed3x3(
                (Fixed64)matrix.m00, (Fixed64)matrix.m10, (Fixed64)matrix.m20,
                (Fixed64)matrix.m01, (Fixed64)matrix.m11, (Fixed64)matrix.m21,
                (Fixed64)matrix.m02, (Fixed64)matrix.m12, (Fixed64)matrix.m22);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3Rotation(this Matrix4x4 matrix)
        {
            return matrix.rotation.ToFixedQuaternion().ToMatrix3x3();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3WorldRotation(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return transform.rotation.ToFixedQuaternion().ToMatrix3x3();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3x3 ToFixed3x3LocalRotation(this Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return transform.localRotation.ToFixedQuaternion().ToMatrix3x3();
        }

        public static Transform CreateTransformRotationLocal(
            this Fixed3x3 matrix,
            string name = "Fixed3x3 Rotation Transform",
            Transform parent = null!)
        {
            Quaternion rotation = ExtractUnityRotation(matrix);
            Transform transform = CreateTransform(name, parent);
            transform.localRotation = rotation;
            return transform;
        }

        public static Transform CreateTransformRotationWorld(
            this Fixed3x3 matrix,
            string name = "Fixed3x3 Rotation Transform",
            Transform parent = null!)
        {
            Quaternion rotation = ExtractUnityRotation(matrix);
            Transform transform = CreateTransform(name, parent);
            transform.rotation = rotation;
            return transform;
        }

        public static void ApplyRotationToTransformLocal(this Fixed3x3 matrix, Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            transform.localRotation = ExtractUnityRotation(matrix);
        }

        public static void ApplyRotationToTransformWorld(this Fixed3x3 matrix, Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            transform.rotation = ExtractUnityRotation(matrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quaternion ExtractUnityRotation(Fixed3x3 matrix)
        {
            return FixedQuaternion.FromMatrix(Fixed3x3.GetNormalized(matrix)).ToQuaternion();
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
