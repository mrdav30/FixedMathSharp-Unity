using System.Collections.Generic;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedSerializationPrefabProbe : MonoBehaviour
    {
        [SerializeField] private Fixed64 fixedValue;
        [SerializeField] private Vector2d vector2Value;
        [SerializeField] private Vector3d vector3Value;
        [SerializeField] private Vector4d vector4Value;
        [SerializeField] private FixedSerializationNestedProbe nestedValue;
        [SerializeField] private List<FixedSerializationListProbe> listValues = new();

        public void SetValues(
            Fixed64 fixedValue,
            Vector2d vector2Value,
            Vector3d vector3Value,
            Vector4d vector4Value,
            Fixed64 nestedValue,
            Fixed64 listValue,
            Vector3d listPosition)
        {
            this.fixedValue = fixedValue;
            this.vector2Value = vector2Value;
            this.vector3Value = vector3Value;
            this.vector4Value = vector4Value;
            this.nestedValue = new FixedSerializationNestedProbe { Value = nestedValue };
            listValues = new List<FixedSerializationListProbe>
            {
                new FixedSerializationListProbe
                {
                    Value = listValue,
                    Position = listPosition
                }
            };
        }

        public Fixed64 FixedValue => fixedValue;
        public Vector2d Vector2Value => vector2Value;
        public Vector3d Vector3Value => vector3Value;
        public Vector4d Vector4Value => vector4Value;
        public Fixed64 NestedValue => nestedValue.Value;
        public int ListCount => listValues.Count;
        public Fixed64 ListValue => listValues.Count > 0 ? listValues[0].Value : default;
        public Vector3d ListPosition => listValues.Count > 0 ? listValues[0].Position : default;
    }
}
