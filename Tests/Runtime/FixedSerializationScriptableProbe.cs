using System.Collections.Generic;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedSerializationScriptableProbe : ScriptableObject
    {
        [SerializeField] private Fixed64 fixedValue;
        [SerializeField] private Vector2d vector2Value;
        [SerializeField] private Vector3d vector3Value;
        [SerializeField] private FixedSerializationNestedProbe nestedValue;
        [SerializeField] private List<FixedSerializationListProbe> listValues = new();

        public void SetSnapshot(FixedSerializationProbeSnapshot snapshot)
        {
            fixedValue = snapshot.FixedValue;
            vector2Value = snapshot.Vector2Value;
            vector3Value = snapshot.Vector3Value;
            nestedValue = snapshot.NestedValue;
            listValues = new List<FixedSerializationListProbe> { snapshot.ListValue };
        }

        public FixedSerializationProbeSnapshot GetSnapshot()
        {
            return new FixedSerializationProbeSnapshot
            {
                FixedValue = fixedValue,
                Vector2Value = vector2Value,
                Vector3Value = vector3Value,
                NestedValue = nestedValue,
                ListValue = listValues[0]
            };
        }
    }
}
