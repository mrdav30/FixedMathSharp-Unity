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
        /// <summary>
        /// Converts a FixedMathSharp 3D bound box to Unity Bounds using its minimum and maximum endpoints.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityBounds ToBounds(this FixedBoundBox box)
        {
            UnityBounds bounds = default;
            bounds.SetMinMax(box.Min.ToVector3(), box.Max.ToVector3());
            return bounds;
        }

        /// <summary>
        /// Converts Unity Bounds to a FixedMathSharp 3D bound box using its minimum and maximum endpoints.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundBox ToFixedBoundBox(this UnityBounds bounds) =>
            FixedBoundBox.FromMinMax(bounds.min.ToVector3d(), bounds.max.ToVector3d());

        /// <summary>
        /// Converts a 2D bound area to zero-height Unity Bounds on the XZ plane.
        /// </summary>
        /// <param name="area">The area whose X and Y components map to Unity X and Z.</param>
        /// <param name="y">The Unity Y coordinate for both bounds endpoints.</param>
        /// <returns>Unity Bounds spanning the area on the XZ plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityBounds ToBoundsXZ(this FixedBoundArea area, float y = 0f)
        {
            UnityBounds bounds = default;
            bounds.SetMinMax(
                new UnityVector3((float)area.Min.X, y, (float)area.Min.Y),
                new UnityVector3((float)area.Max.X, y, (float)area.Max.Y));
            return bounds;
        }

        /// <summary>
        /// Projects the XZ endpoints of Unity Bounds into a FixedMathSharp 2D bound area.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundArea ToFixedBoundAreaXZ(this UnityBounds bounds) =>
            FixedBoundArea.FromMinMax(bounds.min.ToVector2dXZ(), bounds.max.ToVector2dXZ());
    }
}
