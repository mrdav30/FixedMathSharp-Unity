#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    /// <summary>
    /// A custom property drawer for Fixed64 structures.
    /// </summary>
    /// <see cref="PropertyDrawer" />
    [CustomPropertyDrawer(typeof(Fixed64))]
    public class Fixed64Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            SerializedProperty rawValue = property.FindPropertyRelative("m_rawValue");
            bool hasSerializedRawValue = rawValue != null && rawValue.propertyType == SerializedPropertyType.Integer;
            bool hasReflectedValue = property.GetFixedPropertyValue() is Fixed64;
            if (!hasSerializedRawValue && !hasReflectedValue)
                return;

            EditorGUI.BeginProperty(contentPosition, label, property);
            {
                EditorGUI.BeginChangeCheck();
                Fixed64 currentValue = hasSerializedRawValue
                    ? Fixed64.FromRaw(rawValue.longValue)
                    : (Fixed64)property.GetFixedPropertyValue();

                Fixed64 newVal = FMSEditorUtility.FixedNumberField(
                                    contentPosition,
                                    GUIContent.none,
                                    currentValue.m_rawValue);

                if (EditorGUI.EndChangeCheck())
                {
                    if (hasSerializedRawValue)
                    {
                        rawValue.longValue = newVal.m_rawValue;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        property.SetFixedPropertyValue(newVal);
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif
