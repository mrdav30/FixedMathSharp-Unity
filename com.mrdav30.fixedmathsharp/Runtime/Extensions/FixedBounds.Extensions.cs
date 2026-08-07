//=======================================================================
// FixedBounds.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using System.Runtime.CompilerServices;

using UnityBounds = UnityEngine.Bounds;
using UnityVector3 = UnityEngine.Vector3;

namespace FixedMathSharp.Geometry
{
    /// <summary>
    /// Provides endpoint-preserving conversions between Unity Bounds and FixedMathSharp bounds.
    /// </summary>
    public static class FixedBoundsUnityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityBounds ToBounds(this FixedBoundBox box)
        {
            UnityBounds bounds = default;
            bounds.SetMinMax(box.Min.ToVector3(), box.Max.ToVector3());
            return bounds;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundBox ToFixedBoundBox(this UnityBounds bounds)
        {
            return FixedBoundBox.FromMinMax(bounds.min.ToVector3d(), bounds.max.ToVector3d());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityBounds ToBoundsXZ(this FixedBoundArea area, float y = 0f)
        {
            UnityBounds bounds = default;
            bounds.SetMinMax(
                new UnityVector3((float)area.Min.X, y, (float)area.Min.Y),
                new UnityVector3((float)area.Max.X, y, (float)area.Max.Y));
            return bounds;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundArea ToFixedBoundAreaXZ(this UnityBounds bounds)
        {
            return FixedBoundArea.FromMinMax(bounds.min.ToVector2dXZ(), bounds.max.ToVector2dXZ());
        }
    }
}
