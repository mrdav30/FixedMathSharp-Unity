//=======================================================================
// Vector3d.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides extension methods for converting between Unity Vector types and FixedMathSharp Vector3d types.
    /// </summary>
    /// <remarks>
    /// Unity and FixedMathSharp both use <c>+X</c> as right, <c>+Y</c> as up, and <c>+Z</c> as
    /// forward for 3D direction naming. These helpers therefore use direct component mappings for
    /// Unity <see cref="Vector3"/> values instead of flipping axes.
    /// </remarks>
    public static partial class Vector3dExtensions
    {
        /// <summary>
        /// Converts a Unity Vector2 to a FixedMathSharp Vector3d on the XY plane.
        /// </summary>
        /// <remarks>
        /// The supplied <paramref name="z"/> fills FixedMathSharp <c>Z</c>.
        /// </remarks>
        /// <param name="vec">The Unity Vector2 to convert.</param>
        /// <param name="z">The z-value for the Vector3d's z-component.</param>
        /// <returns>A FixedMathSharp Vector3d with the given components.</returns>
        public static Vector3d ToVector3dXY(this Vector2 vec, float z = 0f) =>
            Vector3d.FromDouble(vec.x, vec.y, z);

        /// <summary>
        /// Converts a Unity Vector2 to a FixedMathSharp Vector3d on the XZ plane.
        /// </summary>
        /// <param name="vec">The Unity Vector2 to convert.</param>
        /// <param name="y">The y-value for the Vector3d's y-component.</param>
        /// <returns>A FixedMathSharp Vector3d with the given components.</returns>
        public static Vector3d ToVector3dXZ(this Vector2 vec, float y = 0f) =>
            Vector3d.FromDouble(vec.x, y, vec.y);

        /// <summary>
        /// Converts a Unity Vector3 to a FixedMathSharp Vector3d.
        /// </summary>
        /// <remarks>
        /// This is a direct component mapping. Unity <see cref="Vector3.forward"/> maps to
        /// FixedMathSharp <see cref="Vector3d.Forward"/> because both use <c>+Z</c> as semantic forward.
        /// </remarks>
        /// <param name="vec">The Unity Vector3 to convert.</param>
        /// <returns>A FixedMathSharp Vector3d with the corresponding components from the Unity Vector3.</returns>
        public static Vector3d ToVector3d(this Vector3 vec) =>
            Vector3d.FromDouble(vec.x, vec.y, vec.z);

        /// <summary>
        /// Converts a FixedMathSharp Vector3d to a Unity Vector2 using the x and y components.
        /// </summary>
        /// <remarks>
        /// This projects onto Unity's XY plane and does not perform a 3D forward-axis conversion.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector3d to convert.</param>
        /// <returns>A Unity Vector2 projected onto the XY plane.</returns>
        public static Vector2 ToVector2XY(this Vector3d vec) =>
            new Vector2((float)vec.X, (float)vec.Y);

        /// <summary>
        /// Converts a FixedMathSharp Vector3d to a Unity Vector2 using the x and z components.
        /// </summary>
        /// <remarks>
        /// Unity and FixedMathSharp both use <c>+X</c> right and <c>+Z</c> forward in 3D, so this
        /// projection preserves ground-plane direction semantics directly.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector3d to convert.</param>
        /// <returns>A Unity Vector2 projected onto the XZ plane.</returns>
        public static Vector2 ToVector2XZ(this Vector3d vec) =>
            new Vector2((float)vec.X, (float)vec.Z);

        /// <summary>
        /// Converts a FixedMathSharp Vector3d to a Unity Vector3.
        /// </summary>
        /// <remarks>
        /// This is a direct component mapping. FixedMathSharp <see cref="Vector3d.Forward"/> maps to
        /// Unity <see cref="Vector3.forward"/> because both use <c>+Z</c> as semantic forward.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector3d to convert.</param>
        /// <returns>A Unity Vector3 with the corresponding components from the FixedMathSharp Vector3d.</returns>
        public static Vector3 ToVector3(this Vector3d vec) =>
            new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);

        /// <summary>
        /// Projects a FixedMathSharp Vector3d onto Unity's XZ plane with an explicit y-component.
        /// </summary>
        public static Vector3 ToVector3XZ(this Vector3d vec, float y = 0f) =>
            new Vector3((float)vec.X, y, (float)vec.Z);

        /// <summary>
        /// Converts a FixedMathSharp Vector3d to a Unity Vector4, assigning a specified w-value for the Vector4's w-component.
        /// </summary>
        /// <remarks>
        /// This is a direct component mapping for homogeneous/vector data and does not perform an
        /// axis conversion.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector3d to convert.</param>
        /// <param name="w">The w-value for the resulting Vector4's w-component.</param>
        /// <returns>A Unity Vector4 with the x, y, and z components from the FixedMathSharp Vector3d and the specified w-component.</returns>
        public static Vector4 ToVector4(this Vector3d vec, float w = 0f) =>
            new Vector4((float)vec.X, (float)vec.Y, (float)vec.Z, w);
    }
}
