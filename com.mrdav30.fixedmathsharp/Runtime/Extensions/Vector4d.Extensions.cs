//=======================================================================
// Vector4d.Extensions.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Provides direct component conversions between Unity Vector4 and FixedMathSharp Vector4d.
    /// </summary>
    public static partial class Vector4dExtensions
    {
        public static Vector4d ToVector4d(this Vector4 vec)
        {
            return Vector4d.FromDouble(vec.x, vec.y, vec.z, vec.w);
        }

        public static Vector4 ToVector4(this Vector4d vec)
        {
            return new Vector4((float)vec.X, (float)vec.Y, (float)vec.Z, (float)vec.W);
        }
    }
}
