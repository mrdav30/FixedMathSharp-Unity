#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FixedMathSharp.Build.Editor
{
    /// <summary>
    /// Exports the FixedMathSharp package folders to .unitypackage archives.
    /// </summary>
    public static class FixedMathSharpUnityPackageExporter
    {
        private const string OutputPathArgument = "-FixedMathSharpUnityPackageOutputPath";

        private static readonly PackageDefinition[] Packages =
        {
            new PackageDefinition(
                "Assets/Packages/com.mrdav30.fixedmathsharp",
                "Assets/Packages/com.mrdav30.fixedmathsharp/package.json"),
            new PackageDefinition(
                "Assets/Packages/com.mrdav30.fixedmathsharp.lean",
                "Assets/Packages/com.mrdav30.fixedmathsharp.lean/package.json")
        };

        [MenuItem("Tools/FixedMathSharp/Export Unity Packages")]
        public static void ExportUnityPackagesMenu()
        {
            var defaultOutputDirectory = GetDefaultOutputDirectory();
            var selectedDirectory = EditorUtility.SaveFolderPanel(
                "Export FixedMathSharp Unity Packages",
                defaultOutputDirectory,
                string.Empty);

            if (string.IsNullOrWhiteSpace(selectedDirectory))
                return;

            ExportUnityPackages(selectedDirectory);
        }

        public static void ExportUnityPackagesBatchMode()
        {
            var exitCode = 0;

            try
            {
                ExportUnityPackages(GetBatchModeOutputDirectory());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                exitCode = 1;
            }

            EditorApplication.Exit(exitCode);
        }

        public static void ExportUnityPackages(string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new InvalidOperationException("An output directory is required to export .unitypackage files.");

            FixedMathSharpPackageSync.SyncPackages();

            var resolvedOutputDirectory = Path.GetFullPath(outputDirectory);
            Directory.CreateDirectory(resolvedOutputDirectory);

            foreach (var package in Packages)
            {
                ValidatePackage(package);

                var manifest = LoadManifest(package.ManifestAssetPath);
                var outputPath = Path.Combine(
                    resolvedOutputDirectory,
                    $"{manifest.name}-{manifest.version}.unitypackage");

                AssetDatabase.ExportPackage(
                    new[] { package.RootAssetPath },
                    outputPath,
                    ExportPackageOptions.Recurse);

                Debug.Log($"Exported {manifest.name} to {outputPath}");
            }
        }

        private static void ValidatePackage(PackageDefinition package)
        {
            if (!Directory.Exists(ToAbsolutePath(package.RootAssetPath)))
                throw new DirectoryNotFoundException($"Package folder not found: {package.RootAssetPath}");

            if (!File.Exists(ToAbsolutePath(package.ManifestAssetPath)))
                throw new FileNotFoundException($"Package manifest not found: {package.ManifestAssetPath}");
        }

        private static PackageManifest LoadManifest(string manifestAssetPath)
        {
            var manifestPath = ToAbsolutePath(manifestAssetPath);
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonUtility.FromJson<PackageManifest>(manifestJson);

            if (manifest == null ||
                string.IsNullOrWhiteSpace(manifest.name) ||
                string.IsNullOrWhiteSpace(manifest.version))
            {
                throw new InvalidOperationException($"Unable to parse package manifest: {manifestAssetPath}");
            }

            return manifest;
        }

        private static string GetBatchModeOutputDirectory()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], OutputPathArgument, StringComparison.Ordinal))
                    return ResolveOutputDirectory(args[i + 1]);
            }

            return GetDefaultOutputDirectory();
        }

        private static string GetDefaultOutputDirectory()
        {
            return Path.Combine(GetProjectRoot(), "UnityPackageExports~");
        }

        private static string ResolveOutputDirectory(string path)
        {
            if (Path.IsPathRooted(path))
                return path;

            return Path.GetFullPath(Path.Combine(GetProjectRoot(), path));
        }

        private static string ToAbsolutePath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) &&
                !string.Equals(assetPath, "Assets", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Expected an Assets-relative path, got {assetPath}");
            }

            return Path.GetFullPath(Path.Combine(GetProjectRoot(), assetPath));
        }

        private static string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private struct PackageDefinition
        {
            public string RootAssetPath { get; }
            public string ManifestAssetPath { get; }

            public PackageDefinition(string rootAssetPath, string manifestAssetPath)
            {
                RootAssetPath = rootAssetPath;
                ManifestAssetPath = manifestAssetPath;
            }
        }

        [Serializable]
        private sealed class PackageManifest
        {
            public string name;
            public string version;
        }
    }
}
#endif
