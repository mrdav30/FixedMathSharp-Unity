using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for converting between Unity Bounds and FixedMathSharp bounds types.
    /// </summary>
    public static class BoundsUnityExtensions
    {
        /// <summary>
        /// Converts a FixedMathSharp BoundingBox into Unity Bounds.
        /// </summary>
        /// <param name="boundingBox">The BoundingBox to convert.</param>
        /// <returns>A Unity Bounds with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds ToBounds(this BoundingBox boundingBox)
        {
            Bounds bounds = new Bounds();
            bounds.SetMinMax(boundingBox.Min.ToVector3(), boundingBox.Max.ToVector3());
            return bounds;
        }

        /// <summary>
        /// Converts a FixedMathSharp BoundingArea into Unity Bounds.
        /// </summary>
        /// <param name="boundingArea">The BoundingArea to convert.</param>
        /// <returns>A Unity Bounds with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds ToBounds(this BoundingArea boundingArea)
        {
            Bounds bounds = new Bounds();
            bounds.SetMinMax(boundingArea.Min.ToVector3(), boundingArea.Max.ToVector3());
            return bounds;
        }

        /// <summary>
        /// Converts Unity Bounds into a FixedMathSharp BoundingBox.
        /// </summary>
        /// <param name="bounds">The Unity Bounds to convert.</param>
        /// <returns>A BoundingBox with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundingBox ToBoundingBox(this Bounds bounds)
        {
            BoundingBox boundingBox = default;
            boundingBox.SetMinMax(bounds.min.ToVector3d(), bounds.max.ToVector3d());
            return boundingBox;
        }

        /// <summary>
        /// Converts Unity Bounds into a FixedMathSharp BoundingArea.
        /// </summary>
        /// <param name="bounds">The Unity Bounds to convert.</param>
        /// <returns>A BoundingArea with equivalent minimum and maximum corners.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundingArea ToBoundingArea(this Bounds bounds)
        {
            return new BoundingArea(bounds.min.ToVector3d(), bounds.max.ToVector3d());
        }
    }
}
