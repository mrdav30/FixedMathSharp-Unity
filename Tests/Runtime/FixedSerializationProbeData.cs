using System;

namespace FixedMathSharp.Unity.Tests
{
    [Serializable]
    public struct FixedSerializationNestedProbe
    {
        public Fixed64 Value;
    }

    [Serializable]
    public struct FixedSerializationListProbe
    {
        public Fixed64 Value;
        public Vector3d Position;
    }

    public struct FixedSerializationProbeSnapshot
    {
        public Fixed64 FixedValue;
        public Vector2d Vector2Value;
        public Vector3d Vector3Value;
        public FixedSerializationNestedProbe NestedValue;
        public FixedSerializationListProbe ListValue;
    }
}
