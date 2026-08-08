//=======================================================================
// FixedFrustum.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using System;

using UnityCamera = UnityEngine.Camera;
using UnityGeometryUtility = UnityEngine.GeometryUtility;
using UnityMatrix4x4 = UnityEngine.Matrix4x4;
using UnityPlane = UnityEngine.Plane;

namespace FixedMathSharp.Geometry
{
    /// <summary>
    /// Provides explicit conversions between Unity frustum data and FixedMathSharp frustums.
    /// </summary>
    public static class FixedFrustumUnityExtensions
    {
        private const int LeftIndex = 0;
        private const int RightIndex = 1;
        private const int BottomIndex = 2;
        private const int TopIndex = 3;
        private const int NearIndex = 4;
        private const int FarIndex = 5;

        /// <summary>
        /// Converts a Unity camera's frustum to a FixedMathSharp bound frustum.
        /// </summary>
        /// <param name="camera">The camera that supplies the frustum.</param>
        /// <returns>The converted bound frustum.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="camera" /> is <see langword="null" />.</exception>
        public static FixedBoundFrustum ToFixedBoundFrustum(this UnityCamera camera)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            return camera.ToFixedBoundFrustum(new UnityPlane[FixedBoundFrustum.PlaneCount]);
        }

        /// <summary>
        /// Converts a Unity camera's frustum to a FixedMathSharp bound frustum using the supplied six-plane buffer.
        /// </summary>
        /// <param name="camera">The camera that supplies the frustum.</param>
        /// <param name="planeBuffer">The six-element Unity plane buffer, ordered left, right, bottom, top, near, far.</param>
        /// <returns>The converted bound frustum.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="camera" /> or <paramref name="planeBuffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="planeBuffer" /> does not contain exactly six elements.</exception>
        public static FixedBoundFrustum ToFixedBoundFrustum(this UnityCamera camera, UnityPlane[] planeBuffer)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            ValidateBuffer(planeBuffer);
            UnityGeometryUtility.CalculateFrustumPlanes(camera, planeBuffer);
            return CreateFixedFrustum(planeBuffer);
        }

        /// <summary>
        /// Converts a Unity world-to-projection matrix to a FixedMathSharp bound frustum.
        /// </summary>
        /// <param name="worldToProjectionMatrix">The Unity world-to-projection matrix.</param>
        /// <returns>The converted bound frustum.</returns>
        public static FixedBoundFrustum ToFixedBoundFrustum(this UnityMatrix4x4 worldToProjectionMatrix) =>
            worldToProjectionMatrix.ToFixedBoundFrustum(new UnityPlane[FixedBoundFrustum.PlaneCount]);

        /// <summary>
        /// Converts a Unity world-to-projection matrix to a FixedMathSharp bound frustum using the supplied six-plane buffer.
        /// </summary>
        /// <param name="worldToProjectionMatrix">The Unity world-to-projection matrix.</param>
        /// <param name="planeBuffer">The six-element Unity plane buffer, ordered left, right, bottom, top, near, far.</param>
        /// <returns>The converted bound frustum.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="planeBuffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="planeBuffer" /> does not contain exactly six elements.</exception>
        public static FixedBoundFrustum ToFixedBoundFrustum(
            this UnityMatrix4x4 worldToProjectionMatrix,
            UnityPlane[] planeBuffer)
        {
            ValidateBuffer(planeBuffer);
            UnityGeometryUtility.CalculateFrustumPlanes(worldToProjectionMatrix, planeBuffer);
            return CreateFixedFrustum(planeBuffer);
        }

        /// <summary>
        /// Converts a FixedMathSharp bound frustum to a newly allocated Unity plane array.
        /// </summary>
        /// <param name="frustum">The FixedMathSharp frustum to convert.</param>
        /// <returns>A new six-element Unity plane array ordered left, right, bottom, top, near, far.</returns>
        public static UnityPlane[] ToUnityFrustumPlanes(this FixedBoundFrustum frustum)
        {
            var planes = new UnityPlane[FixedBoundFrustum.PlaneCount];
            frustum.CopyToUnityFrustumPlanes(planes);
            return planes;
        }

        /// <summary>
        /// Converts a FixedMathSharp bound frustum to Unity planes in the supplied six-plane buffer.
        /// </summary>
        /// <param name="frustum">The FixedMathSharp frustum to convert.</param>
        /// <param name="planeBuffer">The six-element destination buffer, ordered left, right, bottom, top, near, far.</param>
        /// <exception cref="ArgumentNullException"><paramref name="planeBuffer" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="planeBuffer" /> does not contain exactly six elements.</exception>
        public static void CopyToUnityFrustumPlanes(this FixedBoundFrustum frustum, UnityPlane[] planeBuffer)
        {
            ValidateBuffer(planeBuffer);
            planeBuffer[LeftIndex] = frustum.Left.ToPlane().flipped;
            planeBuffer[RightIndex] = frustum.Right.ToPlane().flipped;
            planeBuffer[BottomIndex] = frustum.Bottom.ToPlane().flipped;
            planeBuffer[TopIndex] = frustum.Top.ToPlane().flipped;
            planeBuffer[NearIndex] = frustum.Near.ToPlane().flipped;
            planeBuffer[FarIndex] = frustum.Far.ToPlane().flipped;
        }

        private static FixedBoundFrustum CreateFixedFrustum(UnityPlane[] planes) =>
            new(
                planes[NearIndex].flipped.ToFixedPlane(),
                planes[FarIndex].flipped.ToFixedPlane(),
                planes[LeftIndex].flipped.ToFixedPlane(),
                planes[RightIndex].flipped.ToFixedPlane(),
                planes[TopIndex].flipped.ToFixedPlane(),
                planes[BottomIndex].flipped.ToFixedPlane());

        private static void ValidateBuffer(UnityPlane[] planeBuffer)
        {
            if (planeBuffer is null)
                throw new ArgumentNullException(nameof(planeBuffer));
            if (planeBuffer.Length != FixedBoundFrustum.PlaneCount)
                throw new ArgumentException($"The plane buffer must contain exactly {FixedBoundFrustum.PlaneCount} elements.", nameof(planeBuffer));
        }
    }
}
