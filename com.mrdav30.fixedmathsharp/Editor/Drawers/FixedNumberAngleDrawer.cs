#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    [CustomPropertyDrawer(typeof(FixedNumberAngleAttribute))]
    public class FixedNumberAngleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            try
            {
                FixedNumberAngleAttribute angleAttribute = (FixedNumberAngleAttribute)attribute;
                if (!(property.GetFixedPropertyValue() is Fixed64 value))
                    return;

                value = ClampUnit(value);

                Fixed64 angle = FixedMath.RoundToPrecision(FixedMath.RadToDeg(FixedMath.Asin(value)), 2);
                Fixed64 max = angleAttribute.Max > 0d
                    ? Fixed64.FromDouble(angleAttribute.Max)
                    : Fixed64.Zero;

                EditorGUI.BeginChangeCheck();
                FMSEditorUtility.DoubleField(position, label, ref angle, angleAttribute.Timescale);
                if (max > Fixed64.Zero && angle > max)
                    angle = max;

                if (EditorGUI.EndChangeCheck())
                    property.SetFixedPropertyValue(FixedMath.Sin(FixedMath.DegToRad(angle)));
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        private static Fixed64 ClampUnit(Fixed64 value)
        {
            if (value < Fixed64.Zero - Fixed64.One)
                return Fixed64.Zero - Fixed64.One;

            return value > Fixed64.One ? Fixed64.One : value;
        }
    }
}
#endif
