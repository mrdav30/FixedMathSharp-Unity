using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public sealed class FixedMathSharpSampleImportSmokeTests
{
    private const string ConfigAssetPath = "FixedMathSharpSampleImportSmokeConfig.json";
    private const string ImportedPathKey = "FixedMathSharp.SampleImportSmoke.ImportedPath";

    [Test]
    public void DemoSceneSampleWasImportedAndOpens()
    {
        var config = LoadConfig();
        Assert.That(config.expectedSampleAsmdef, Is.Not.Empty, $"Config must define expectedSampleAsmdef: {ConfigAssetPath}");

        var importedPath = SessionState.GetString(ImportedPathKey, string.Empty);
        Assert.That(importedPath, Is.Not.Empty, "Sample import did not record an imported path.");

        var importedAsmdefPath = Path.Combine(importedPath, config.expectedSampleAsmdef);
        Assert.That(File.Exists(importedAsmdefPath), Is.True, $"Imported sample asmdef was not found: {importedAsmdefPath}");

        var importedScenes = Directory.GetFiles(importedPath, "*.unity", SearchOption.AllDirectories);
        Assert.That(importedScenes, Has.Length.EqualTo(1), $"Expected one imported sample scene under: {importedPath}");

        var importedScenePath = importedScenes[0];
        AssertSerializedFieldLine(importedScenePath, "worldRotationMatrix");
        AssertSerializedFieldLine(importedScenePath, "localFixedAsUnityMatrix");
        AssertSerializedFieldLine(importedScenePath, "worldFixedAsUnityMatrix");

        var assetsPath = Path.GetFullPath(UnityEngine.Application.dataPath);
        var fullScenePath = Path.GetFullPath(importedScenePath);
        Assert.That(
            fullScenePath.StartsWith(assetsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase),
            Is.True,
            $"Imported scene is outside the Unity project: {importedScenePath}");
        var sceneAssetPath = "Assets/" + fullScenePath[(assetsPath.Length + 1)..].Replace('\\', '/');

        Scene scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Single);
        Assert.That(scene.IsValid(), Is.True, $"Imported sample scene did not open: {sceneAssetPath}");
        Assert.That(scene.rootCount, Is.GreaterThan(0), $"Imported sample scene has no roots: {sceneAssetPath}");
    }

    private static void AssertSerializedFieldLine(string scenePath, string fieldName)
    {
        var prefix = $"  {fieldName}:";
        foreach (var line in File.ReadLines(scenePath))
        {
            if (line.StartsWith(prefix, StringComparison.Ordinal))
                return;
        }

        Assert.Fail($"Imported sample scene is missing serialized field '{fieldName}': {scenePath}");
    }

    private static SmokeConfig LoadConfig()
    {
        var configPath = Path.Combine(UnityEngine.Application.dataPath, ConfigAssetPath);
        Assert.That(File.Exists(configPath), Is.True, $"Sample import smoke config was not found: {configPath}");

        var config = UnityEngine.JsonUtility.FromJson<SmokeConfig>(File.ReadAllText(configPath));
        Assert.That(config, Is.Not.Null, $"Unable to parse sample import smoke config: {configPath}");
        return config;
    }

    [Serializable]
    private sealed class SmokeConfig
    {
        public string expectedSampleAsmdef;
    }
}
