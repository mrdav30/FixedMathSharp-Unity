//=======================================================================
// FixedBounds.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using FixedMathSharp.Bounds;
using System.Runtime.CompilerServices;

using UnityBounds = UnityEngine.Bounds;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for converting between Unity Bounds and FixedMathSharp bounds types.
    /// </summary>
    public static class FixedBoundsUnityExtensions
    {
        /// <summary>
        /// Converts a FixedMathSharp BoundingBox into Unity Bounds.
        /// </summary>
        /// <param name="boundingBox">The BoundingBox to convert.</param>
        /// <returns>A Unity Bounds with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityBounds ToBounds(this FixedBoundBox boundingBox) =>
            new(boundingBox.Center.ToVector3(), boundingBox.Proportions.ToVector3());

        /// <summary>
        /// Converts a FixedMathSharp BoundingArea into Unity Bounds.
        /// </summary>
        /// <param name="boundingArea">The BoundingArea to convert.</param>
        /// <returns>A Unity Bounds with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityBounds ToBounds(this FixedBoundArea boundingArea) =>
            new(boundingArea.Center.ToVector3(), boundingArea.Size.ToVector3());

        /// <summary>
        /// Converts Unity Bounds into a FixedMathSharp BoundingBox.
        /// </summary>
        /// <param name="bounds">The Unity Bounds to convert.</param>
        /// <returns>A FixedBoundBox with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundBox ToBoundingBox(this UnityBounds bounds) =>
            FixedBoundBox.FromMinMax(bounds.min.ToVector3d(), bounds.max.ToVector3d());

        /// <summary>
        /// Converts Unity Bounds into a FixedMathSharp BoundingArea.
        /// </summary>
        /// <param name="bounds">The Unity Bounds to convert.</param>
        /// <returns>A FixedBoundArea with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundArea ToBoundingArea(this UnityBounds bounds) =>
            FixedBoundArea.FromMinMax(bounds.min.ToVector2d(), bounds.max.ToVector2d());
    }
}
