using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;

public sealed class FixedMathSharpSampleImportSmokeTests
{
    private const string ConfigAssetPath = "FixedMathSharpSampleImportSmokeConfig.json";
    private const string ImportedPathKey = "FixedMathSharp.SampleImportSmoke.ImportedPath";

    [Test]
    public void DemoSceneSampleWasImported()
    {
        var config = LoadConfig();
        Assert.That(config.expectedSampleAsmdef, Is.Not.Empty, $"Config must define expectedSampleAsmdef: {ConfigAssetPath}");

        var importedPath = SessionState.GetString(ImportedPathKey, string.Empty);
        Assert.That(importedPath, Is.Not.Empty, "Sample import did not record an imported path.");

        var importedAsmdefPath = Path.Combine(importedPath, config.expectedSampleAsmdef);
        Assert.That(File.Exists(importedAsmdefPath), Is.True, $"Imported sample asmdef was not found: {importedAsmdefPath}");
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
