//=======================================================================
// FMSEditorUtility.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    public static class FMSEditorUtility
    {
        private const float MatrixRowLabelWidth = 24f;
        private const float MatrixCellSpacing = 2f;

        #region EditorGUI

        public static void DoubleField(Rect position, GUIContent label, ref Fixed64 value, double scale = 1d)
        {
            value = (Fixed64)(EditorGUI.DoubleField(position, label, (double)value * scale) / scale);
        }

        public static void DoubleField(Rect position, string label, ref Fixed64 value, double scale = 1d)
        {
            value = (Fixed64)(EditorGUI.DoubleField(position, label, (double)value * scale) / scale);
        }

        public static Fixed64 FixedNumberField(Rect position, Fixed64 value, Fixed64 max)
        {
            Fixed64 result = (Fixed64)EditorGUI.DoubleField(position, Math.Round((double)value, 2, MidpointRounding.AwayFromZero));
            return max == Fixed64.Zero || result <= max ? result : max;
        }

        public static Fixed64 FixedNumberField(Rect position, Fixed64 value)
        {
            Fixed64 result = (Fixed64)EditorGUI.DoubleField(position, Math.Round((double)value, 2, MidpointRounding.AwayFromZero));
            return result;
        }

        public static Fixed64 FixedNumberField(Rect position, long value)
        {
            return FixedNumberField(position, GUIContent.none, value);
        }

        public static Fixed64 FixedNumberField(Rect position, GUIContent label, long value)
        {
            return (Fixed64)EditorGUI.DoubleField(position, label, Math.Round(Fixed64.ToDouble(value), 4, MidpointRounding.AwayFromZero));
        }

        public static void Vector3dField(Rect position, GUIContent label, ref Vector3d vector)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 14f;

            float fieldWidth = position.width / 3f;
            Rect fieldRect = new Rect(position.x, position.y, position.width / 3.5f, position.height);

            EditorGUI.BeginChangeCheck();
            Fixed64 x = (Fixed64)EditorGUI.DoubleField(fieldRect, "X", (double)vector.X);
            fieldRect.x += fieldWidth;
            Fixed64 y = (Fixed64)EditorGUI.DoubleField(fieldRect, "Y", (double)vector.Y);
            fieldRect.x += fieldWidth;
            Fixed64 z = (Fixed64)EditorGUI.DoubleField(fieldRect, "Z", (double)vector.Z);

            if (EditorGUI.EndChangeCheck())
            {
                vector = new Vector3d(x, y, z);
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        public static Fixed64 GetFixed64Value(SerializedProperty property)
        {
            return TryGetFixed64Value(property, out Fixed64 value, out _)
                ? value
                : Fixed64.Zero;
        }

        internal static bool TryGetFixed64Value(SerializedProperty property, out Fixed64 value, out SerializedProperty rawValue)
        {
            rawValue = property?.FindPropertyRelative("m_rawValue");
            if (rawValue != null && rawValue.propertyType == SerializedPropertyType.Integer)
            {
                value = Fixed64.FromRaw(rawValue.longValue);
                return true;
            }

            if (property != null && property.GetFixedPropertyValue() is Fixed64 reflectedValue)
            {
                value = reflectedValue;
                return true;
            }

            value = Fixed64.Zero;
            return false;
        }

        internal static void SetFixed64Value(SerializedProperty property, SerializedProperty rawValue, Fixed64 value)
        {
            if (rawValue != null && rawValue.propertyType == SerializedPropertyType.Integer)
            {
                rawValue.longValue = value.m_rawValue;
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                property.SetFixedPropertyValue(value);
            }
        }

        public static void DrawReadOnlyMatrixRow(Rect position, string rowLabel, params Fixed64[] values)
        {
            Rect rowLabelRect = new Rect(position.x, position.y, MatrixRowLabelWidth, position.height);
            EditorGUI.LabelField(rowLabelRect, rowLabel);

            float totalSpacing = MatrixCellSpacing * (values.Length - 1);
            float cellWidth = (position.width - MatrixRowLabelWidth - totalSpacing) / values.Length;
            Rect cellRect = new Rect(rowLabelRect.xMax, position.y, cellWidth, position.height);

            using var disabled = new EditorGUI.DisabledScope(true);
            for (int i = 0; i < values.Length; i++)
            {
                EditorGUI.TextField(cellRect, values[i].ToString("0.###"));
                cellRect.x += cellWidth + MatrixCellSpacing;
            }
        }

        #endregion

        #region EditorGUILayout

        public static void FixedNumberField(string Label, ref Fixed64 fixedNumber)
        {
            fixedNumber = (Fixed64)EditorGUILayout.DoubleField(Label, (double)fixedNumber);
        }

        public static void FixedNumberField(string label, ref SerializedProperty property)
        {
            if (!TryGetFixed64Value(property, out Fixed64 currentValue, out SerializedProperty rawValue))
                return;

            EditorGUI.BeginChangeCheck();
            double newValue = EditorGUILayout.DoubleField(label, (double)currentValue);
            if (!EditorGUI.EndChangeCheck())
                return;

            Fixed64 newFixedValue = Fixed64.FromDouble(newValue);
            SetFixed64Value(property, rawValue, newFixedValue);
        }

        public static void FixedNumberField(string label, ref SerializedProperty property, float min, float max)
        {
            if (!TryGetFixed64Value(property, out Fixed64 currentValue, out SerializedProperty rawValue))
                return;

            EditorGUILayout.LabelField(label);
            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUILayout.Slider((float)currentValue, min, max);
            if (!EditorGUI.EndChangeCheck())
                return;

            Fixed64 newFixedValue = Fixed64.FromDouble(newValue);
            SetFixed64Value(property, rawValue, newFixedValue);
        }

        public static void Vector2dField(string Label, ref Vector2d vector)
        {
            vector = EditorGUILayout.Vector2Field(Label, vector.ToVector2()).ToVector2d();
        }

        public static void Vector3dField(string Label, ref Vector3d vector)
        {
            vector = EditorGUILayout.Vector3Field(Label, vector.ToVector3()).ToVector3d();
        }

        #endregion
    }
}
#endif
