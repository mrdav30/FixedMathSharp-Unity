//=======================================================================
// VectorRotationAttribute.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Displays a Vector2d direction as an angle in degrees with optional display scaling.
    /// </summary>
    public class VectorRotationAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the scale applied while displaying and editing the angle.
        /// </summary>
        public double Timescale { get; private set; }

        /// <summary>
        /// Initializes a vector rotation attribute with an optional display scale.
        /// </summary>
        /// <param name="timescale">The scale applied while displaying and editing the angle.</param>
        public VectorRotationAttribute(double timescale = 1d)
        {
            Timescale = timescale;
        }
    }
}
