using UnityEngine;

namespace FixedMathSharp
{
    public static partial class Vector2dExtensions
    {
        /// <summary>
        /// Converts a Unity Vector2 to a FixedMathSharp Vector2d.
        /// </summary>
        /// <remarks>
        /// This is a direct component mapping for Unity 2D values. In FixedMathSharp 2D plane math,
        /// <c>+X</c> is right and <c>+Y</c> is forward/up within the 2D plane.
        /// </remarks>
        /// <param name="vec2">The Unity Vector2 to convert.</param>
        /// <returns>A FixedMathSharp Vector2d.</returns>
        public static Vector2d ToVector2d(this Vector2 vec2)
        {
            return Vector2d.FromDouble(vec2.x, vec2.y);
        }

        /// <summary>
        /// Converts a Unity Vector3 to a FixedMathSharp Vector2d on Unity's XZ plane.
        /// </summary>
        /// <remarks>
        /// Unity and FixedMathSharp both use <c>+X</c> right and <c>+Z</c> forward in 3D, so this
        /// method maps Unity <c>x</c> to FixedMathSharp <c>X</c> and Unity <c>z</c> to FixedMathSharp
        /// <c>Y</c> for XZ-plane gameplay math.
        /// </remarks>
        /// <param name="vec">The Unity Vector3 to convert.</param>
        /// <returns>A FixedMathSharp Vector2d.</returns>
        public static Vector2d ToVector2d(this Vector3 vec)
        {
            return vec.ToVector2dXZ();
        }

        /// <summary>
        /// Converts a Unity Vector3 to a FixedMathSharp Vector2d using the x and y components.
        /// </summary>
        /// <remarks>
        /// This is a direct component projection onto the Unity XY plane. Use
        /// <see cref="ToVector2dXZ(Vector3)"/> when the 2D vector represents Unity ground-plane
        /// movement where <c>+Z</c> is forward.
        /// </remarks>
        /// <param name="vec">The Unity Vector3 to convert.</param>
        /// <returns>A FixedMathSharp Vector2d projected onto the XY plane.</returns>
        public static Vector2d ToVector2dXY(this Vector3 vec)
        {
            return Vector2d.FromDouble(vec.x, vec.y);
        }

        /// <summary>
        /// Converts a Unity Vector3 to a FixedMathSharp Vector2d using the x and z components.
        /// </summary>
        /// <remarks>
        /// Unity and FixedMathSharp both use <c>+X</c> right and <c>+Z</c> forward in 3D. This helper
        /// keeps that convention explicit when reducing Unity XZ-plane directions or positions to
        /// FixedMathSharp 2D plane math.
        /// </remarks>
        /// <param name="vec">The Unity Vector3 to convert.</param>
        /// <returns>A FixedMathSharp Vector2d projected onto the XZ plane.</returns>
        public static Vector2d ToVector2dXZ(this Vector3 vec)
        {
            return Vector2d.FromDouble(vec.x, vec.z);
        }

        /// <summary>
        /// Converts a FixedMathSharp Vector2d to a Unity Vector2.
        /// </summary>
        /// <remarks>
        /// This is a direct component mapping for 2D values and does not perform any 3D axis
        /// conversion.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector2d to convert.</param>
        /// <returns>A Unity Vector2.</returns>
        public static Vector2 ToVector2(this Vector2d vec)
        {
            return new Vector2((float)vec.X, (float)vec.Y);
        }

        /// <summary>
        /// Converts a FixedMathSharp Vector2d to a Unity Vector3 on Unity's XZ plane, with the specified y-value.
        /// </summary>
        /// <remarks>
        /// The FixedMathSharp <c>Y</c> component is mapped to Unity <c>z</c>, preserving Unity and
        /// FixedMathSharp's shared <c>+Z</c> forward convention for 3D ground-plane values.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector2d to convert.</param>
        /// <param name="y">The y-value to use in the resulting Vector3.</param>
        /// <returns>A Unity Vector3.</returns>
        public static Vector3 ToVector3(this Vector2d vec, float y = 0f)
        {
            return vec.ToVector3XZ(y);
        }

        /// <summary>
        /// Converts a FixedMathSharp Vector2d to a Unity Vector3 on the XY plane, with the specified z-value.
        /// </summary>
        /// <remarks>
        /// This is a direct component projection onto Unity's XY plane. Use
        /// <see cref="ToVector3XZ(Vector2d, float)"/> when the 2D vector represents 3D ground-plane
        /// values using <c>+Z</c> as forward.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector2d to convert.</param>
        /// <param name="z">The z-value to use in the resulting Vector3.</param>
        /// <returns>A Unity Vector3 with x/y populated from the Vector2d.</returns>
        public static Vector3 ToVector3XY(this Vector2d vec, float z = 0f)
        {
            return new Vector3((float)vec.X, (float)vec.Y, z);
        }

        /// <summary>
        /// Converts a FixedMathSharp Vector2d to a Unity Vector3 on the XZ plane, with the specified y-value.
        /// </summary>
        /// <remarks>
        /// The FixedMathSharp <c>Y</c> component is mapped to Unity <c>z</c>, preserving Unity and
        /// FixedMathSharp's shared <c>+Z</c> forward convention for 3D ground-plane values.
        /// </remarks>
        /// <param name="vec">The FixedMathSharp Vector2d to convert.</param>
        /// <param name="y">The y-value to use in the resulting Vector3.</param>
        /// <returns>A Unity Vector3 with x/z populated from the Vector2d.</returns>
        public static Vector3 ToVector3XZ(this Vector2d vec, float y = 0f)
        {
            return new Vector3((float)vec.X, y, (float)vec.Y);
        }
    }
}
