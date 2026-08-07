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

        public static void ApplyToTransformLocal(this FixedTransform fixedTransform, Transform target)
        {
            Validate(fixedTransform, target);
            ApplyLocalComponents(fixedTransform, target);
        }

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
