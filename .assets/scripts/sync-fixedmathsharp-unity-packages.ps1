<#
.SYNOPSIS
Runs the FixedMathSharp package sync Unity editor method in batch mode.

.DESCRIPTION
Launches Unity without opening the editor UI and executes
FixedMathSharpPackageSync.SyncPackagesBatchMode. By default the script finds the
Unity project two directories above this package repository and uses UNITY_EDITOR
or Unity on PATH. Pass -UnityPath or -ProjectPath to override those defaults.

.PARAMETER UnityPath
Path to the Unity executable. When omitted, UNITY_EDITOR is used first, then
Unity/Unity.exe on PATH, then the newest Unity Hub editor on Windows.

.PARAMETER ProjectPath
Path to the Unity project root. Relative paths are resolved from the package
repository root.

.PARAMETER LogFile
Unity log destination. Defaults to '-' so Unity writes to stdout.

.PARAMETER WhatIf
Prints the Unity command without launching Unity.
#>
[CmdletBinding()]
param(
    [string]$UnityPath,
    [string]$ProjectPath,
    [string]$LogFile = "-",
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "fixedmathsharp-unity-batch.ps1")

Invoke-FixedMathSharpUnityBatchMethod `
    -ExecuteMethod "FixedMathSharp.Build.Editor.FixedMathSharpPackageSync.SyncPackagesBatchMode" `
    -UnityPath $UnityPath `
    -ProjectPath $ProjectPath `
    -LogFile $LogFile `
    -WhatIf:$WhatIf
