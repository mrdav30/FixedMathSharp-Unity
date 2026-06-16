using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedSerializationPersistenceTests
    {
        private const string TestAssetFolder = "Assets/GeneratedSerializationPersistenceTests";

        private static readonly FixedSerializationProbeSnapshot ExpectedSnapshot = new()
        {
            FixedValue = Fixed64.FromRaw(0x12345678),
            Vector2Value = new Vector2d(
                Fixed64.FromRaw(0x11111111),
                Fixed64.FromRaw(0x22222222)),
            Vector3Value = new Vector3d(
                Fixed64.FromRaw(0x33333333),
                Fixed64.FromRaw(0x44444444),
                Fixed64.FromRaw(0x55555555)),
            NestedValue = Fixed64.FromRaw(0x66666666),
            ListCount = 1,
            ListValue = Fixed64.FromRaw(0x77777777),
            ListPosition = new Vector3d(
                Fixed64.FromRaw(0x11112222),
                Fixed64.FromRaw(0x33334444),
                Fixed64.FromRaw(0x55556666))
        };

        [SetUp]
        public void SetUp()
        {
            DeleteTestAssets();
            AssetDatabase.CreateFolder("Assets", "GeneratedSerializationPersistenceTests");
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestAssets();
        }

        [Test]
        public void ScriptableObjectSaveReload_PreservesFixedMathSharpValues()
        {
            string assetPath = $"{TestAssetFolder}/FixedSerializationProbe.asset";
            FixedSerializationScriptableProbe asset = ScriptableObject.CreateInstance<FixedSerializationScriptableProbe>();
            SetSnapshot(asset, ExpectedSnapshot);

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            Resources.UnloadAsset(asset);
            asset = null;
            EditorUtility.UnloadUnusedAssetsImmediate();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

            FixedSerializationScriptableProbe reloaded = AssetDatabase.LoadAssetAtPath<FixedSerializationScriptableProbe>(assetPath);

            Assert.That(reloaded, Is.Not.Null);
            AssertSnapshotEqual(GetSnapshot(reloaded), ExpectedSnapshot);
        }

        [Test]
        public void ScriptableObjectEditSaveReload_PreservesFixedMathSharpValues()
        {
            string assetPath = $"{TestAssetFolder}/FixedSerializationProbe.asset";
            FixedSerializationScriptableProbe asset = ScriptableObject.CreateInstance<FixedSerializationScriptableProbe>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            SetSnapshot(asset, ExpectedSnapshot);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Resources.UnloadAsset(asset);
            asset = null;
            EditorUtility.UnloadUnusedAssetsImmediate();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

            FixedSerializationScriptableProbe reloaded = AssetDatabase.LoadAssetAtPath<FixedSerializationScriptableProbe>(assetPath);

            Assert.That(reloaded, Is.Not.Null);
            AssertSnapshotEqual(GetSnapshot(reloaded), ExpectedSnapshot);
        }

        [Test]
        public void PrefabSaveReload_PreservesFixedMathSharpValues()
        {
            string prefabPath = $"{TestAssetFolder}/FixedSerializationProbe.prefab";
            GameObject source = new("Fixed Serialization Probe");
            SetSnapshot(source.AddComponent<FixedSerializationPrefabProbe>(), ExpectedSnapshot);

            try
            {
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
                Assert.That(prefab, Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceSynchronousImport);

            GameObject reloadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            FixedSerializationPrefabProbe reloaded = reloadedPrefab.GetComponent<FixedSerializationPrefabProbe>();

            Assert.That(reloaded, Is.Not.Null);
            AssertSnapshotEqual(GetSnapshot(reloaded), ExpectedSnapshot);
        }

        [Test]
        public void PrefabEditSaveReload_PreservesFixedMathSharpValues()
        {
            string prefabPath = $"{TestAssetFolder}/FixedSerializationProbe.prefab";
            GameObject source = new("Fixed Serialization Probe");
            source.AddComponent<FixedSerializationPrefabProbe>();

            try
            {
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
                Assert.That(prefab, Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                SetSnapshot(contents.GetComponent<FixedSerializationPrefabProbe>(), ExpectedSnapshot);
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceSynchronousImport);

            GameObject reloadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            FixedSerializationPrefabProbe reloaded = reloadedPrefab.GetComponent<FixedSerializationPrefabProbe>();

            Assert.That(reloaded, Is.Not.Null);
            AssertSnapshotEqual(GetSnapshot(reloaded), ExpectedSnapshot);
        }

        private struct FixedSerializationProbeSnapshot
        {
            public Fixed64 FixedValue;
            public Vector2d Vector2Value;
            public Vector3d Vector3Value;
            public Fixed64 NestedValue;
            public int ListCount;
            public Fixed64 ListValue;
            public Vector3d ListPosition;
        }

        private static void SetSnapshot(
            FixedSerializationScriptableProbe probe,
            FixedSerializationProbeSnapshot snapshot)
        {
            probe.SetValues(
                snapshot.FixedValue,
                snapshot.Vector2Value,
                snapshot.Vector3Value,
                snapshot.NestedValue,
                snapshot.ListValue,
                snapshot.ListPosition);
        }

        private static void SetSnapshot(
            FixedSerializationPrefabProbe probe,
            FixedSerializationProbeSnapshot snapshot)
        {
            probe.SetValues(
                snapshot.FixedValue,
                snapshot.Vector2Value,
                snapshot.Vector3Value,
                snapshot.NestedValue,
                snapshot.ListValue,
                snapshot.ListPosition);
        }

        private static FixedSerializationProbeSnapshot GetSnapshot(FixedSerializationScriptableProbe probe)
        {
            return new FixedSerializationProbeSnapshot
            {
                FixedValue = probe.FixedValue,
                Vector2Value = probe.Vector2Value,
                Vector3Value = probe.Vector3Value,
                NestedValue = probe.NestedValue,
                ListCount = probe.ListCount,
                ListValue = probe.ListValue,
                ListPosition = probe.ListPosition
            };
        }

        private static FixedSerializationProbeSnapshot GetSnapshot(FixedSerializationPrefabProbe probe)
        {
            return new FixedSerializationProbeSnapshot
            {
                FixedValue = probe.FixedValue,
                Vector2Value = probe.Vector2Value,
                Vector3Value = probe.Vector3Value,
                NestedValue = probe.NestedValue,
                ListCount = probe.ListCount,
                ListValue = probe.ListValue,
                ListPosition = probe.ListPosition
            };
        }

        private static void AssertSnapshotEqual(
            FixedSerializationProbeSnapshot actual,
            FixedSerializationProbeSnapshot expected)
        {
            List<string> failures = new();

            CollectFixedFailure(actual.FixedValue, expected.FixedValue, "Fixed64 field", failures);
            CollectVector2Failure(actual.Vector2Value, expected.Vector2Value, "Vector2d field", failures);
            CollectVector3Failure(actual.Vector3Value, expected.Vector3Value, "Vector3d field", failures);
            CollectFixedFailure(actual.NestedValue, expected.NestedValue, "nested Fixed64 field", failures);
            if (actual.ListCount != expected.ListCount)
                failures.Add($"list count: expected {expected.ListCount}, got {actual.ListCount}");
            CollectFixedFailure(actual.ListValue, expected.ListValue, "list Fixed64 field", failures);
            CollectVector3Failure(actual.ListPosition, expected.ListPosition, "list Vector3d field", failures);

            if (failures.Count > 0)
                Assert.Fail(string.Join(Environment.NewLine, failures));
        }

        private static void CollectVector2Failure(
            Vector2d actual,
            Vector2d expected,
            string context,
            List<string> failures)
        {
            CollectFixedFailure(actual.X, expected.X, $"{context}.X", failures);
            CollectFixedFailure(actual.Y, expected.Y, $"{context}.Y", failures);
        }

        private static void CollectVector3Failure(
            Vector3d actual,
            Vector3d expected,
            string context,
            List<string> failures)
        {
            CollectFixedFailure(actual.X, expected.X, $"{context}.X", failures);
            CollectFixedFailure(actual.Y, expected.Y, $"{context}.Y", failures);
            CollectFixedFailure(actual.Z, expected.Z, $"{context}.Z", failures);
        }

        private static void CollectFixedFailure(Fixed64 actual, Fixed64 expected, string context, List<string> failures)
        {
            if (actual.m_rawValue != expected.m_rawValue)
                failures.Add($"{context}: expected raw {expected.m_rawValue}, got {actual.m_rawValue}");
        }

        private static void DeleteTestAssets()
        {
            if (AssetDatabase.IsValidFolder(TestAssetFolder))
                AssetDatabase.DeleteAsset(TestAssetFolder);
        }
    }
}
