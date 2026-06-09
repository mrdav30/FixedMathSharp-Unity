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
    [CustomPropertyDrawer(typeof(Vector2))]
    [CustomPropertyDrawer(typeof(Vector2d))]
    [CustomPropertyDrawer(typeof(Vector3))]
    [CustomPropertyDrawer(typeof(Vector3d))]
    public class VectorDrawer : PropertyDrawer
    {
        /// <summary>
        /// Called when the GUI is drawn.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int fieldCount = GetFieldCount(property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            EditorGUIUtility.labelWidth = 14f;
            float fieldWidth = contentPosition.width / fieldCount;
            bool hideLabels = contentPosition.width < 185;
            contentPosition.width /= fieldCount + 0.5f;

            using var indent = new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel);
            if (IsUnityVector(property))
                DrawUnityVector(contentPosition, property, fieldCount, fieldWidth, hideLabels);
            else
                DrawFixedVector(contentPosition, property, fieldCount, fieldWidth, hideLabels);
        }

        private int GetFieldCount(SerializedProperty property)
        {
            Type type = fieldInfo?.FieldType;
            if (type == typeof(Vector2) || type == typeof(Vector2d))
                return 2;

            return property.type == nameof(Vector2) || property.type == nameof(Vector2d) ? 2 : 3;
        }

        private bool IsUnityVector(SerializedProperty property)
        {
            Type type = fieldInfo?.FieldType;
            return type == typeof(Vector2)
                || type == typeof(Vector3)
                || property.type == nameof(Vector2)
                || property.type == nameof(Vector3);
        }

        private static void DrawUnityVector(Rect contentPosition, SerializedProperty property, int fieldCount, float fieldWidth, bool hideLabels)
        {
            string[] fieldNames = fieldCount == 2
                ? new[] { "x", "y" }
                : new[] { "x", "y", "z" };

            for (int i = 0; i < fieldNames.Length; i++)
            {
                SerializedProperty component = property.FindPropertyRelative(fieldNames[i]);
                if (component == null)
                    break;

                EditorGUI.BeginProperty(contentPosition, GUIContent.none, component);
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(
                    contentPosition,
                    hideLabels ? GUIContent.none : new GUIContent(component.displayName),
                    component.floatValue);
                if (EditorGUI.EndChangeCheck())
                    component.floatValue = newValue;
                EditorGUI.EndProperty();

                contentPosition.x += fieldWidth;
            }
        }

        private static void DrawFixedVector(Rect contentPosition, SerializedProperty property, int fieldCount, float fieldWidth, bool hideLabels)
        {
            object propertyValue = property.GetFixedPropertyValue();
            if (fieldCount == 2 && propertyValue is Vector2d vector2d)
            {
                EditorGUI.BeginChangeCheck();
                Fixed64 x = DrawFixedComponent(contentPosition, "X", vector2d.X, hideLabels);
                contentPosition.x += fieldWidth;
                Fixed64 y = DrawFixedComponent(contentPosition, "Y", vector2d.Y, hideLabels);

                if (EditorGUI.EndChangeCheck())
                    property.SetFixedPropertyValue(new Vector2d(x, y));
            }
            else if (fieldCount == 3 && propertyValue is Vector3d vector3d)
            {
                EditorGUI.BeginChangeCheck();
                Fixed64 x = DrawFixedComponent(contentPosition, "X", vector3d.X, hideLabels);
                contentPosition.x += fieldWidth;
                Fixed64 y = DrawFixedComponent(contentPosition, "Y", vector3d.Y, hideLabels);
                contentPosition.x += fieldWidth;
                Fixed64 z = DrawFixedComponent(contentPosition, "Z", vector3d.Z, hideLabels);

                if (EditorGUI.EndChangeCheck())
                    property.SetFixedPropertyValue(new Vector3d(x, y, z));
            }
        }

        private static Fixed64 DrawFixedComponent(Rect position, string label, Fixed64 value, bool hideLabels)
        {
            return FMSEditorUtility.FixedNumberField(
                position,
                hideLabels ? GUIContent.none : new GUIContent(label),
                value.m_rawValue);
        }
    }
}
#endif
