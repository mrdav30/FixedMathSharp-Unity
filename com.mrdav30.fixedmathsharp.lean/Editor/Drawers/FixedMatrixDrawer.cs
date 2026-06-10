//=======================================================================
// FixedMatrixDrawer.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

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
    public class FixedMatrixDrawer : PropertyDrawer
    {
        private static readonly string[][] Fixed3x3FieldNames =
        {
            new[] { "M11", "M12", "M13" },
            new[] { "M21", "M22", "M23" },
            new[] { "M31", "M32", "M33" }
        };

        private static readonly string[][] Fixed4x4FieldNames =
        {
            new[] { "M11", "M12", "M13", "M14" },
            new[] { "M21", "M22", "M23", "M24" },
            new[] { "M31", "M32", "M33", "M34" },
            new[] { "M41", "M42", "M43", "M44" }
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
