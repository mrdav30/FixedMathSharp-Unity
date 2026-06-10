//=======================================================================
// FixedQuaternionDrawer.cs
//=======================================================================
// MIT License, Copyright (c) 2024–present David Oravsky (mrdav30)
// See LICENSE file in the project root for full license information.
//=======================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    [CustomPropertyDrawer(typeof(FixedQuaternion)), CanEditMultipleObjects]
    public class FixedQuaternionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            // Get the FixedQuaternion instance from the serialized property
            if (property.GetFixedPropertyValue() is FixedQuaternion quaternion)
            {
                // Convert the quaternion to euler angles (in degrees)
                Vector3d eulerAngles = quaternion.EulerAngles;

                using var indent = new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel);
                // Display euler angles in the inspector and get the new values after editing
                EditorGUI.BeginChangeCheck();
                FMSEditorUtility.Vector3dField(contentPosition, GUIContent.none, ref eulerAngles);

                // Convert the edited euler angles back to a quaternion (in radians) and set the quaternion value
                FixedQuaternion newQuaternion = FixedQuaternion.FromEulerAnglesInDegrees(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);

                if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {
                    property.SetFixedPropertyValue(newQuaternion);
                }
            }
            else
            {
                Debug.LogWarning("Property value is null or not a FixedQuaternion.");
            }      

            EditorGUI.EndProperty();
        }
    }
}
#endif
