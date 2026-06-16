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

}
