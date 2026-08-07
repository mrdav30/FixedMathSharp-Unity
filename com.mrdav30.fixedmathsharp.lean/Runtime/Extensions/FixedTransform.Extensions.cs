//=======================================================================
// FixedTransform.Extensions.cs
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
    /// Provides component-based interop between Unity Transform and FixedMathSharp FixedTransform.
    /// </summary>
    public static class FixedTransformUnityExtensions
    {
        /// <summary>
        /// Creates a FixedTransform from a Unity transform's local TRS components and an optional fixed parent.
        /// </summary>
        /// <param name="transform">The Unity transform to convert.</param>
        /// <param name="parent">The optional parent in the FixedTransform hierarchy.</param>
        /// <returns>The converted local transform.</returns>
        public static FixedTransform ToFixedTransformLocal(
            this Transform transform,
            FixedTransform parent = null!)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new FixedTransform(
                transform.localPosition.ToVector3d(),
                transform.localRotation.ToFixedQuaternion(),
                transform.localScale.ToVector3d(),
                parent);
        }

        /// <summary>
        /// Applies a FixedTransform's local TRS components to a Unity transform.
        /// </summary>
        public static void ApplyToTransformLocal(this FixedTransform fixedTransform, Transform target)
        {
            Validate(fixedTransform, target);
            ApplyLocalComponents(fixedTransform, target);
        }

        /// <summary>
        /// Tries to compose and apply a FixedTransform in the target Unity hierarchy.
        /// </summary>
        /// <returns><see langword="true" /> when the world transform is representable by the target; otherwise, <see langword="false" />.</returns>
        public static bool TryApplyToTransformWorld(this FixedTransform fixedTransform, Transform target)
        {
            Validate(fixedTransform, target);

            if (fixedTransform.Parent == null && target.parent == null)
            {
                ApplyLocalComponents(fixedTransform, target);
                return true;
            }

            return fixedTransform.TryGetLocalToWorldMatrix(out Fixed4x4 world)
                && world.TryApplyToTransformWorld(target);
        }

        /// <summary>
        /// Composes and applies a FixedTransform in the target Unity hierarchy.
        /// </summary>
        /// <exception cref="InvalidOperationException">The world transform is not representable by the target.</exception>
        public static void ApplyToTransformWorld(this FixedTransform fixedTransform, Transform target)
        {
            if (!fixedTransform.TryApplyToTransformWorld(target))
            {
                throw new InvalidOperationException(
                    "The FixedTransform cannot be composed into a representable Unity world transform.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyLocalComponents(FixedTransform fixedTransform, Transform target)
        {
            Vector3 position = fixedTransform.LocalPosition.ToVector3();
            Quaternion rotation = fixedTransform.LocalRotation.ToQuaternion();
            Vector3 scale = fixedTransform.LocalScale.ToVector3();

            target.SetLocalPositionAndRotation(position, rotation);
            target.localScale = scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Validate(FixedTransform fixedTransform, Transform target)
        {
            if (fixedTransform == null)
                throw new ArgumentNullException(nameof(fixedTransform));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
        }
    }
}
