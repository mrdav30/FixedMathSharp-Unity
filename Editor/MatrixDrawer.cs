#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    /// <summary>
    /// A read-only matrix drawer for Fixed3x3 and Fixed4x4 values.
    /// </summary>
    [CustomPropertyDrawer(typeof(Fixed3x3))]
    [CustomPropertyDrawer(typeof(Fixed4x4))]
    public class MatrixDrawer : PropertyDrawer
    {
        private static readonly string[][] Fixed3x3FieldNames =
        {
            new[] { "m00", "m01", "m02" },
            new[] { "m10", "m11", "m12" },
            new[] { "m20", "m21", "m22" }
        };

        private static readonly string[][] Fixed4x4FieldNames =
        {
            new[] { "m00", "m01", "m02", "m03" },
            new[] { "m10", "m11", "m12", "m13" },
            new[] { "m20", "m21", "m22", "m23" },
            new[] { "m30", "m31", "m32", "m33" }
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            int rowCount = IsFixed4x4(property) ? 4 : 3;
            return EditorGUIUtility.singleLineHeight +
                   rowCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                string[][] fieldNames = IsFixed4x4(property) ? Fixed4x4FieldNames : Fixed3x3FieldNames;
                Rect rowRect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUIUtility.singleLineHeight);

                for (int row = 0; row < fieldNames.Length; row++)
                {
                    Rect indentedRowRect = EditorGUI.IndentedRect(rowRect);
                    FMSEditorUtility.DrawReadOnlyMatrixRow(indentedRowRect, $"R{row}", GetRowValues(property, fieldNames[row]));
                    rowRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            EditorGUI.EndProperty();
        }

        private bool IsFixed4x4(SerializedProperty property)
        {
            return fieldInfo != null
                ? fieldInfo.FieldType == typeof(Fixed4x4)
                : property.type == nameof(Fixed4x4);
        }

        private static Fixed64[] GetRowValues(SerializedProperty property, string[] rowFieldNames)
        {
            Fixed64[] values = new Fixed64[rowFieldNames.Length];
            for (int i = 0; i < rowFieldNames.Length; i++)
            {
                values[i] = FMSEditorUtility.GetFixed64Value(property.FindPropertyRelative(rowFieldNames[i]));
            }

            return values;
        }
    }
}
#endif
