#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Editor
{
    // Union type representing either a property name or array element index.  The element
    // index is valid only if propertyName is null.
    internal struct FixedPropertyPathComponent
    {
        public string propertyName;
        public int elementIndex;
    }

    /// <summary>
    /// Provide simple value get/set methods for SerializedProperty.
    /// </summary>
    internal static class FixedSerializedPropertyExtensions
    {
        private static readonly Regex pathComponentRegex = new(@"([^.\[\]]+)|\[(\d+)\]", RegexOptions.Compiled);

        internal static object GetFixedPropertyValue(this SerializedProperty property)
        {
            return GetTargetObjectOfProperty(property, out _);
        }

        internal static void SetFixedPropertyValue(this SerializedProperty property, object value)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");
            SetValueNoRecord(property, value);
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void SetValueNoRecord(this SerializedProperty property, object value)
        {
            List<FixedPropertyPathComponent> components = ParsePropertyPath(property.propertyPath);
            if (components.Count == 0)
            {
                Debug.LogError($"Property path is empty, unable to set {property.name}.");
                return;
            }

            SetPathValue(property.serializedObject.targetObject, components, 0, value);
        }

        private static List<FixedPropertyPathComponent> ParsePropertyPath(string propertyPath)
        {
            string normalizedPath = propertyPath.Replace(".Array.data[", "[");
            List<FixedPropertyPathComponent> components = new();

            foreach (Match match in pathComponentRegex.Matches(normalizedPath))
            {
                if (match.Groups[1].Success)
                {
                    components.Add(new FixedPropertyPathComponent { propertyName = match.Groups[1].Value });
                }
                else if (match.Groups[2].Success)
                {
                    components.Add(new FixedPropertyPathComponent { elementIndex = int.Parse(match.Groups[2].Value) });
                }
            }

            return components;
        }

        private static object GetPathComponentValue(object container, FixedPropertyPathComponent component)
        {
            if (container == null) return null;

            if (component.propertyName == null)
                return ((IList)container)[component.elementIndex];
            else
                return GetMemberValue(container, component.propertyName);
        }

        private static void SetPathComponentValue(object container, FixedPropertyPathComponent component, object value)
        {
            if (container == null) return;

            if (component.propertyName == null)
                ((IList)container)[component.elementIndex] = value;
            else
                SetMemberValue(container, component.propertyName, value);
        }

        private static object SetPathValue(object container, List<FixedPropertyPathComponent> components, int componentIndex, object value)
        {
            if (container == null)
            {
                Debug.LogError($"Container is null, unable to set path component {componentIndex}.");
                return null;
            }

            FixedPropertyPathComponent component = components[componentIndex];
            if (componentIndex == components.Count - 1)
            {
                SetPathComponentValue(container, component, value);
                return container;
            }

            object child = GetPathComponentValue(container, component);
            object updatedChild = SetPathValue(child, components, componentIndex + 1, value);
            SetPathComponentValue(container, component, updatedChild);
            return container;
        }

        private static object GetMemberValue(object container, string name)
        {
            if (container == null) return null;
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member is FieldInfo field)
                    return field.GetValue(container);
                else if (member is PropertyInfo property)
                    return property.GetValue(container);
            }
            return null;
        }

        private static void SetMemberValue(object container, string name, object value)
        {
            if (container == null) return;

            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    field.SetValue(container, value);
                    return;
                }
                else if (member is PropertyInfo property)
                {
                    property.SetValue(container, value);
                    return;
                }
            }
            Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }

        private static object GetTargetObjectOfProperty(SerializedProperty prop, out FixedPropertyPathComponent lastComponent, bool stopBeforeLast = false)
        {
            object obj = prop.serializedObject.targetObject;
            List<FixedPropertyPathComponent> components = ParsePropertyPath(prop.propertyPath);
            int componentCount = stopBeforeLast ? components.Count - 1 : components.Count;

            lastComponent = new FixedPropertyPathComponent();
            for (int i = 0; i < componentCount; i++)
            {
                FixedPropertyPathComponent component = components[i];
                obj = GetPathComponentValue(obj, component);
                lastComponent = component;
            }
            return obj;
        }
    }
}
#endif
