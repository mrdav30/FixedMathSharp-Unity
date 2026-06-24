#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
        private const string AuthoringSamplesRelativePath = "Samples";
        private const string DistributableSamplesRelativePath = "Samples~";

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
                PrepareDistributableSamples(package);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                var manifest = LoadManifest(package.ManifestAssetPath);
                var outputPath = Path.Combine(
                    resolvedOutputDirectory,
                    $"{manifest.name}-{manifest.version}.unitypackage");

                AssetDatabase.ExportPackage(
                    GetExportAssetPaths(package.RootAssetPath),
                    outputPath,
                    ExportPackageOptions.Recurse);

                Debug.Log($"Exported {manifest.name} to {outputPath}");
            }
        }

        private static void PrepareDistributableSamples(PackageDefinition package)
        {
            var authoringSamplesPath = ToAbsolutePath(CombineAssetPath(package.RootAssetPath, AuthoringSamplesRelativePath));
            if (!Directory.Exists(authoringSamplesPath))
                throw new DirectoryNotFoundException($"Authoring samples folder not found: {CombineAssetPath(package.RootAssetPath, AuthoringSamplesRelativePath)}");

            var distributableSamplesPath = ToAbsolutePath(CombineAssetPath(package.RootAssetPath, DistributableSamplesRelativePath));
            if (Directory.Exists(distributableSamplesPath))
                Directory.Delete(distributableSamplesPath, true);

            DeleteMetaFileIfPresent(distributableSamplesPath);
            CopyDirectory(authoringSamplesPath, distributableSamplesPath);
            DeleteMetaFileIfPresent(distributableSamplesPath);

            Debug.Log($"Prepared distributable samples: {CombineAssetPath(package.RootAssetPath, DistributableSamplesRelativePath)}");
        }

        private static string[] GetExportAssetPaths(string rootAssetPath)
        {
            var rootPath = ToAbsolutePath(rootAssetPath);
            var assetPaths = new List<string>();

            foreach (var path in Directory.EnumerateFileSystemEntries(rootPath))
            {
                var name = Path.GetFileName(path);
                if (string.Equals(name, AuthoringSamplesRelativePath, StringComparison.Ordinal) ||
                    string.Equals(name, AuthoringSamplesRelativePath + ".meta", StringComparison.Ordinal) ||
                    string.Equals(name, ".gitignore", StringComparison.Ordinal) ||
                    name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                assetPaths.Add(ToAssetPath(path));
            }

            if (assetPaths.Count == 0)
                throw new InvalidOperationException($"No exportable package assets found under {rootAssetPath}.");

            return assetPaths.ToArray();
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

        private static string ToAssetPath(string absolutePath)
        {
            var projectRoot = GetProjectRoot().TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(absolutePath);

            if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Expected path under Unity project root, got {absolutePath}");

            return fullPath.Substring(projectRoot.Length).Replace('\\', '/');
        }

        private static string CombineAssetPath(string left, string right)
        {
            return $"{left.TrimEnd('/')}/{right.TrimStart('/')}";
        }

        private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);

            foreach (var directoryPath in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDirectory, directoryPath);
                Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
            }

            foreach (var filePath in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDirectory, filePath);
                var destinationPath = Path.Combine(destinationDirectory, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? destinationDirectory);
                File.Copy(filePath, destinationPath, true);
            }
        }

        private static void DeleteMetaFileIfPresent(string assetPath)
        {
            var metaPath = assetPath + ".meta";
            if (File.Exists(metaPath))
                File.Delete(metaPath);
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
