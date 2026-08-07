//=======================================================================
// FixedNumberAngleAttribute.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Displays a Fixed64 sine value as an angle in degrees with optional display scaling and a maximum angle.
    /// </summary>
    public class FixedNumberAngleAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the scale applied while displaying and editing the angle.
        /// </summary>
        public double Timescale { get; private set; }

        /// <summary>
        /// Gets the maximum allowable value for the angle in degrees. 
        /// A value of -1 indicates no maximum limit.
        /// </summary>
        public double Max { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedNumberAngleAttribute"/> class.
        /// </summary>
        /// <param name="timescale">The scale applied while displaying and editing the angle.</param>
        /// <param name="max">The maximum allowable value for the angle in degrees. Default is -1 (no limit).</param>
        public FixedNumberAngleAttribute(double timescale = 1d, double max = -1d)
        {
            Timescale = timescale;
            Max = max;
        }
    }
}
