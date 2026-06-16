using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedSerializationPersistenceTests
    {
        private const string TestAssetFolder = "Assets/Packages/Tests/EditMode/GeneratedSerializationPersistenceTests";

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
            NestedValue = new FixedSerializationNestedProbe
            {
                Value = Fixed64.FromRaw(0x66666666)
            },
            ListValue = new FixedSerializationListProbe
            {
                Value = Fixed64.FromRaw(0x77777777),
                Position = new Vector3d(
                    Fixed64.FromRaw(0x11112222),
                    Fixed64.FromRaw(0x33334444),
                    Fixed64.FromRaw(0x55556666))
            }
        };

        [SetUp]
        public void SetUp()
        {
            DeleteTestAssets();
            AssetDatabase.CreateFolder("Assets/Packages/Tests/EditMode", "GeneratedSerializationPersistenceTests");
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
            asset.SetSnapshot(ExpectedSnapshot);

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            Resources.UnloadAsset(asset);
            asset = null;
            EditorUtility.UnloadUnusedAssetsImmediate();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

            FixedSerializationScriptableProbe reloaded = AssetDatabase.LoadAssetAtPath<FixedSerializationScriptableProbe>(assetPath);

            Assert.That(reloaded, Is.Not.Null);
            AssertSnapshotEqual(reloaded.GetSnapshot(), ExpectedSnapshot);
        }

        [Test]
        public void ScriptableObjectEditSaveReload_PreservesFixedMathSharpValues()
        {
            string assetPath = $"{TestAssetFolder}/FixedSerializationProbe.asset";
            FixedSerializationScriptableProbe asset = ScriptableObject.CreateInstance<FixedSerializationScriptableProbe>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            asset.SetSnapshot(ExpectedSnapshot);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Resources.UnloadAsset(asset);
            asset = null;
            EditorUtility.UnloadUnusedAssetsImmediate();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

            FixedSerializationScriptableProbe reloaded = AssetDatabase.LoadAssetAtPath<FixedSerializationScriptableProbe>(assetPath);

            Assert.That(reloaded, Is.Not.Null);
            AssertSnapshotEqual(reloaded.GetSnapshot(), ExpectedSnapshot);
        }

        [Test]
        public void PrefabSaveReload_PreservesFixedMathSharpValues()
        {
            string prefabPath = $"{TestAssetFolder}/FixedSerializationProbe.prefab";
            GameObject source = new("Fixed Serialization Probe");
            source.AddComponent<FixedSerializationPrefabProbe>().SetSnapshot(ExpectedSnapshot);

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
            AssertSnapshotEqual(reloaded.GetSnapshot(), ExpectedSnapshot);
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
                contents.GetComponent<FixedSerializationPrefabProbe>().SetSnapshot(ExpectedSnapshot);
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
            AssertSnapshotEqual(reloaded.GetSnapshot(), ExpectedSnapshot);
        }

        private static void AssertSnapshotEqual(
            FixedSerializationProbeSnapshot actual,
            FixedSerializationProbeSnapshot expected)
        {
            List<string> failures = new();

            CollectFixedFailure(actual.FixedValue, expected.FixedValue, "Fixed64 field", failures);
            CollectVector2Failure(actual.Vector2Value, expected.Vector2Value, "Vector2d field", failures);
            CollectVector3Failure(actual.Vector3Value, expected.Vector3Value, "Vector3d field", failures);
            CollectFixedFailure(actual.NestedValue.Value, expected.NestedValue.Value, "nested Fixed64 field", failures);
            CollectFixedFailure(actual.ListValue.Value, expected.ListValue.Value, "list Fixed64 field", failures);
            CollectVector3Failure(actual.ListValue.Position, expected.ListValue.Position, "list Vector3d field", failures);

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
