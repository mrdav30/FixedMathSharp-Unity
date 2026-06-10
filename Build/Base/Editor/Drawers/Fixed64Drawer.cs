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
            if (!FMSEditorUtility.TryGetFixed64Value(property, out Fixed64 currentValue, out SerializedProperty rawValue))
                return;

            EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            try
            {
                EditorGUI.BeginChangeCheck();
                Fixed64 newVal = FMSEditorUtility.FixedNumberField(
                                    contentPosition,
                                    GUIContent.none,
                                    currentValue.m_rawValue);

                if (EditorGUI.EndChangeCheck())
                    FMSEditorUtility.SetFixed64Value(property, rawValue, newVal);
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }
    }
}
#endif
