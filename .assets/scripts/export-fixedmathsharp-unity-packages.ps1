<#
.SYNOPSIS
Exports the FixedMathSharp Unity package variants in batch mode.

.DESCRIPTION
Launches Unity without opening the editor UI and executes
FixedMathSharpUnityPackageExporter.ExportUnityPackagesBatchMode. The exporter
runs package sync before writing .unitypackage archives.

.PARAMETER UnityPath
Path to the Unity executable. When omitted, UNITY_EDITOR is used first, then
Unity/Unity.exe on PATH, then the newest Unity Hub editor on Windows.

.PARAMETER ProjectPath
Path to the Unity project root. Relative paths are resolved from the package
repository root.

.PARAMETER OutputPath
Output directory passed to the Unity exporter. Relative paths are resolved by the
exporter from the Unity project root. Defaults to UnityPackageExports~.

.PARAMETER LogFile
Unity log destination. Defaults to '-' so Unity writes to stdout.

.PARAMETER WhatIf
Prints the Unity command without launching Unity.
#>
[CmdletBinding()]
param(
    [string]$UnityPath,
    [string]$ProjectPath,
    [string]$OutputPath = "UnityPackageExports~",
    [string]$LogFile = "-",
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "fixedmathsharp-unity-batch.ps1")

$additionalArguments = @(
    "-FixedMathSharpUnityPackageOutputPath",
    $OutputPath
)

Invoke-FixedMathSharpUnityBatchMethod `
    -ExecuteMethod "FixedMathSharp.Build.Editor.FixedMathSharpUnityPackageExporter.ExportUnityPackagesBatchMode" `
    -UnityPath $UnityPath `
    -ProjectPath $ProjectPath `
    -LogFile $LogFile `
    -AdditionalArguments $additionalArguments `
    -PathArgumentNames @("-FixedMathSharpUnityPackageOutputPath") `
    -WhatIf:$WhatIf
