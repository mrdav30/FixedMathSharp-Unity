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
            if (rawValue == null)
                return;

            EditorGUI.BeginProperty(contentPosition, label, property);
            {
                EditorGUI.BeginChangeCheck();

                Fixed64 newVal = FMSEditorUtility.FixedNumberField(
                                    contentPosition,
                                    GUIContent.none,
                                    rawValue.longValue);

                if (EditorGUI.EndChangeCheck())
                {
                    rawValue.longValue = newVal.m_rawValue;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif