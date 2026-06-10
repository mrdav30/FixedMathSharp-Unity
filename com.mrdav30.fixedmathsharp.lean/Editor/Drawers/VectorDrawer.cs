#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    /// <summary>
    /// A custom property drawer for vectors type structures.
    /// </summary>
    /// <see cref="PropertyDrawer" />
    [CustomPropertyDrawer(typeof(Vector2d))]
    [CustomPropertyDrawer(typeof(Vector3d))]
    public class VectorDrawer : PropertyDrawer
    {
        private static readonly GUIContent XLabel = new("X");
        private static readonly GUIContent YLabel = new("Y");
        private static readonly GUIContent ZLabel = new("Z");

        /// <summary>
        /// Called when the GUI is drawn.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            try
            {
                int fieldCount = GetFieldCount(property);
                Rect contentPosition = EditorGUI.PrefixLabel(position, label);

                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 14f;
                try
                {
                    float fieldWidth = contentPosition.width / fieldCount;
                    bool hideLabels = contentPosition.width < 185;
                    contentPosition.width /= fieldCount + 0.5f;

                    using var indent = new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel);
                    DrawFixedVector(contentPosition, property, fieldCount, fieldWidth, hideLabels);
                }
                finally
                {
                    EditorGUIUtility.labelWidth = previousLabelWidth;
                }
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        private int GetFieldCount(SerializedProperty property)
        {
            Type type = fieldInfo?.FieldType;
            if (type == typeof(Vector2d))
                return 2;

            return property.type == nameof(Vector2d) ? 2 : 3;
        }

        private static void DrawFixedVector(Rect contentPosition, SerializedProperty property, int fieldCount, float fieldWidth, bool hideLabels)
        {
            object propertyValue = property.GetFixedPropertyValue();
            if (fieldCount == 2 && propertyValue is Vector2d vector2d)
            {
                EditorGUI.BeginChangeCheck();
                Fixed64 x = DrawFixedComponent(contentPosition, XLabel, vector2d.X, hideLabels);
                contentPosition.x += fieldWidth;
                Fixed64 y = DrawFixedComponent(contentPosition, YLabel, vector2d.Y, hideLabels);

                if (EditorGUI.EndChangeCheck())
                    property.SetFixedPropertyValue(new Vector2d(x, y));
            }
            else if (fieldCount == 3 && propertyValue is Vector3d vector3d)
            {
                EditorGUI.BeginChangeCheck();
                Fixed64 x = DrawFixedComponent(contentPosition, XLabel, vector3d.X, hideLabels);
                contentPosition.x += fieldWidth;
                Fixed64 y = DrawFixedComponent(contentPosition, YLabel, vector3d.Y, hideLabels);
                contentPosition.x += fieldWidth;
                Fixed64 z = DrawFixedComponent(contentPosition, ZLabel, vector3d.Z, hideLabels);

                if (EditorGUI.EndChangeCheck())
                    property.SetFixedPropertyValue(new Vector3d(x, y, z));
            }
        }

        private static Fixed64 DrawFixedComponent(Rect position, GUIContent label, Fixed64 value, bool hideLabels)
        {
            return FMSEditorUtility.FixedNumberField(
                position,
                hideLabels ? GUIContent.none : label,
                value.m_rawValue);
        }
    }
}
#endif
