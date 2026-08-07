//=======================================================================
// FixedGeometry.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using System;
using System.Runtime.CompilerServices;

using UnityPlane = UnityEngine.Plane;
using UnityRay = UnityEngine.Ray;
using UnityRay2D = UnityEngine.Ray2D;
using UnityRect = UnityEngine.Rect;

namespace FixedMathSharp.Geometry
{
    /// <summary>
    /// Provides explicit conversions between Unity and FixedMathSharp geometry primitives.
    /// </summary>
    public static class FixedGeometryUnityExtensions
    {
        /// <summary>
        /// Converts a Unity Ray to a FixedRay, normalizing any nonzero direction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedRay ToFixedRay(this UnityRay ray)
        {
            Vector3d direction = ray.direction.ToVector3d();
            if (direction != Vector3d.Zero)
                direction.NormalizeInPlace();

            return new FixedRay(ray.origin.ToVector3d(), direction);
        }

        /// <summary>
        /// Tries to convert a FixedRay whose direction is zero or normalized to a Unity Ray.
        /// </summary>
        /// <param name="fixedRay">The ray to convert.</param>
        /// <param name="ray">The converted ray, or the default value when conversion fails.</param>
        /// <returns><see langword="true" /> when the direction is zero or normalized; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToRay(this FixedRay fixedRay, out UnityRay ray)
        {
            if (fixedRay.Direction != Vector3d.Zero && !fixedRay.Direction.IsNormalized())
            {
                ray = default;
                return false;
            }

            ray = default;
            ray.origin = fixedRay.Position.ToVector3();
            ray.direction = fixedRay.Direction.ToVector3();
            return true;
        }

        /// <summary>
        /// Converts a FixedRay with a zero or normalized direction to a Unity Ray.
        /// </summary>
        /// <exception cref="InvalidOperationException">The direction is nonzero and not normalized.</exception>
        public static UnityRay ToRay(this FixedRay fixedRay)
        {
            if (fixedRay.TryToRay(out UnityRay ray))
                return ray;

            throw new InvalidOperationException("A Unity Ray requires a zero or normalized direction.");
        }

        /// <summary>
        /// Converts a Unity Ray2D to a FixedRay2d, normalizing any nonzero direction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedRay2d ToFixedRay2d(this UnityRay2D ray)
        {
            Vector2d direction = ray.direction.ToVector2d();
            if (direction != Vector2d.Zero)
                direction.NormalizeInPlace();

            return new FixedRay2d(ray.origin.ToVector2d(), direction);
        }

        /// <summary>
        /// Tries to convert a FixedRay2d whose direction is zero or normalized to a Unity Ray2D.
        /// </summary>
        /// <param name="fixedRay">The ray to convert.</param>
        /// <param name="ray">The converted ray, or the default value when conversion fails.</param>
        /// <returns><see langword="true" /> when the direction is zero or normalized; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToRay2D(this FixedRay2d fixedRay, out UnityRay2D ray)
        {
            if (fixedRay.Direction != Vector2d.Zero && !fixedRay.Direction.IsNormalized())
            {
                ray = default;
                return false;
            }

            ray = default;
            ray.origin = fixedRay.Position.ToVector2();
            ray.direction = fixedRay.Direction.ToVector2();
            return true;
        }

        /// <summary>
        /// Converts a FixedRay2d with a zero or normalized direction to a Unity Ray2D.
        /// </summary>
        /// <exception cref="InvalidOperationException">The direction is nonzero and not normalized.</exception>
        public static UnityRay2D ToRay2D(this FixedRay2d fixedRay)
        {
            if (fixedRay.TryToRay2D(out UnityRay2D ray))
                return ray;

            throw new InvalidOperationException("A Unity Ray2D requires a zero or normalized direction.");
        }

        /// <summary>
        /// Converts a Unity Plane to a FixedPlane by copying its normal and distance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPlane ToFixedPlane(this UnityPlane plane) =>
            new FixedPlane(plane.normal.ToVector3d(), (Fixed64)plane.distance);

        /// <summary>
        /// Converts a FixedPlane to a Unity Plane by copying its normal and distance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityPlane ToPlane(this FixedPlane fixedPlane)
        {
            UnityPlane plane = default;
            plane.normal = fixedPlane.Normal.ToVector3();
            plane.distance = (float)fixedPlane.D;
            return plane;
        }

        /// <summary>
        /// Converts a Unity Rect to a FixedMathSharp bound area using its minimum and maximum endpoints.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedBoundArea ToFixedBoundArea(this UnityRect rect) =>
            FixedBoundArea.FromMinMax(rect.min.ToVector2d(), rect.max.ToVector2d());

        /// <summary>
        /// Converts a FixedMathSharp bound area to a Unity Rect using its minimum and maximum endpoints.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityRect ToRect(this FixedBoundArea area) =>
            UnityRect.MinMaxRect(
                (float)area.Min.X,
                (float)area.Min.Y,
                (float)area.Max.X,
                (float)area.Max.Y);
    }
}
