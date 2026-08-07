//=======================================================================
// VectorRotationDrawer.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    [CustomPropertyDrawer(typeof(VectorRotationAttribute))]
    public class VectorRotationDrawer : PropertyDrawer
    {
        private static readonly GUIContent AngleLabel = new("Angle");
        private static readonly GUIContent UnsupportedLabel = new("Use with Vector2d");

        /// <summary>
        /// Gets the single-line height used by the vector rotation field.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Draws a Vector2d direction as an editable angle in degrees.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            try
            {
                if (property.GetFixedPropertyValue() is Vector2d vector)
                {
                    VectorRotationAttribute at = (VectorRotationAttribute)attribute;
                    Fixed64 angleInRadians = FixedMath.Atan2(vector.Y, vector.X);
                    Fixed64 angleInDegrees = FixedMath.RadToDeg(angleInRadians);

                    EditorGUI.BeginChangeCheck();
                    FMSEditorUtility.DoubleField(position, AngleLabel, ref angleInDegrees, at.Timescale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Fixed64 newAngleInRadians = FixedMath.DegToRad(angleInDegrees);
                        Fixed64 cos = FixedMath.Cos(newAngleInRadians);
                        Fixed64 sin = FixedMath.Sin(newAngleInRadians);

                        property.SetFixedPropertyValue(new Vector2d(cos, sin));
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label, UnsupportedLabel);
                }
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }
    }
}
#endif
