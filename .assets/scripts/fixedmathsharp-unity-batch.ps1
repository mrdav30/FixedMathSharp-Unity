[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-FixedMathSharpPackagesRoot {
    $assetsRoot = Split-Path -Parent $PSScriptRoot
    return [System.IO.Path]::GetFullPath((Split-Path -Parent $assetsRoot))
}

function Resolve-FixedMathSharpPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$BasePath
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $Path))
}

function Resolve-FixedMathSharpUnityProjectPath {
    param([string]$ProjectPath)

    $packagesRoot = Get-FixedMathSharpPackagesRoot

    if (-not [string]::IsNullOrWhiteSpace($ProjectPath)) {
        $resolvedProjectPath = Resolve-FixedMathSharpPath -Path $ProjectPath -BasePath $packagesRoot
    }
    else {
        $resolvedProjectPath = [System.IO.Path]::GetFullPath((Join-Path $packagesRoot "../.."))
    }

    $assetsPath = Join-Path $resolvedProjectPath "Assets"
    $projectSettingsPath = Join-Path $resolvedProjectPath "ProjectSettings"

    if (-not (Test-Path -LiteralPath $assetsPath -PathType Container)) {
        throw "Unity project Assets folder was not found: $assetsPath"
    }

    if (-not (Test-Path -LiteralPath $projectSettingsPath -PathType Container)) {
        throw "Unity project ProjectSettings folder was not found: $projectSettingsPath"
    }

    return $resolvedProjectPath
}

function Resolve-FixedMathSharpUnityEditorPath {
    param([string]$UnityPath)

    if (-not [string]::IsNullOrWhiteSpace($UnityPath)) {
        return $UnityPath
    }

    $environmentPath = [Environment]::GetEnvironmentVariable("UNITY_EDITOR")
    if (-not [string]::IsNullOrWhiteSpace($environmentPath)) {
        return $environmentPath
    }

    $unityCommand = Get-Command "Unity" -ErrorAction SilentlyContinue
    if ($null -ne $unityCommand) {
        return $unityCommand.Source
    }

    $unityExeCommand = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
    if ($null -ne $unityExeCommand) {
        return $unityExeCommand.Source
    }

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        $programFiles = [Environment]::GetEnvironmentVariable("ProgramFiles")
        if ([string]::IsNullOrWhiteSpace($programFiles)) {
            throw "Could not find Unity. Pass -UnityPath or set the UNITY_EDITOR environment variable."
        }

        $hubRoot = Join-Path $programFiles "Unity/Hub/Editor"
        if (Test-Path -LiteralPath $hubRoot -PathType Container) {
            $editors = Get-ChildItem -LiteralPath $hubRoot -Directory |
                Sort-Object -Property @{ Expression = { Get-FixedMathSharpUnityVersionSortKey -VersionName $_.Name } } -Descending

            foreach ($editor in $editors) {
                $candidate = Join-Path $editor.FullName "Editor/Unity.exe"
                if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                    return $candidate
                }
            }
        }
    }

    throw "Could not find Unity. Pass -UnityPath or set the UNITY_EDITOR environment variable."
}

function Get-FixedMathSharpUnityVersionSortKey {
    param([string]$VersionName)

    if ($VersionName -match '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?<channel>[abfp])(?<build>\d+)') {
        $channelRank = switch ($Matches.channel) {
            "a" { 0 }
            "b" { 1 }
            "f" { 2 }
            "p" { 3 }
            default { 0 }
        }

        return "{0:D6}.{1:D6}.{2:D6}.{3:D2}.{4:D6}" -f `
            [int]$Matches.major,
            [int]$Matches.minor,
            [int]$Matches.patch,
            $channelRank,
            [int]$Matches.build
    }

    return "000000.000000.000000.00.000000.$VersionName"
}

function Format-FixedMathSharpCommandArgument {
    param([string]$Argument)

    if ($Argument -notmatch '\s|["]') {
        return $Argument
    }

    return '"' + $Argument.Replace('"', '\"') + '"'
}

function Test-FixedMathSharpWindowsUnityFromUnix {
    param([string]$UnityPath)

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        return $false
    }

    return $UnityPath.EndsWith(".exe", [System.StringComparison]::OrdinalIgnoreCase)
}

function Convert-FixedMathSharpPathForUnity {
    param(
        [string]$Path,
        [string]$UnityPath
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or $Path -eq "-") {
        return $Path
    }

    if (-not [System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    if (-not (Test-FixedMathSharpWindowsUnityFromUnix -UnityPath $UnityPath)) {
        return $Path
    }

    $wslPathCommand = Get-Command "wslpath" -ErrorAction SilentlyContinue
    if ($null -eq $wslPathCommand) {
        throw "Cannot convert '$Path' for Windows Unity because wslpath was not found."
    }

    return (& $wslPathCommand.Source -w $Path).Trim()
}

function Invoke-FixedMathSharpProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [string[]]$Arguments = @()
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FilePath
    $startInfo.UseShellExecute = $false

    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo

    try {
        if (-not $process.Start()) {
            throw "Failed to start process: $FilePath"
        }

        $process.WaitForExit()
        return $process.ExitCode
    }
    finally {
        $process.Dispose()
    }
}

function Invoke-FixedMathSharpUnityBatchMethod {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExecuteMethod,

        [string]$UnityPath,

        [string]$ProjectPath,

        [string]$LogFile = "-",

        [string[]]$AdditionalArguments = @(),

        [string[]]$PathArgumentNames = @(),

        [switch]$WhatIf
    )

    $resolvedUnityPath = Resolve-FixedMathSharpUnityEditorPath -UnityPath $UnityPath
    $resolvedProjectPath = Resolve-FixedMathSharpUnityProjectPath -ProjectPath $ProjectPath
    $unityProjectPath = Convert-FixedMathSharpPathForUnity -Path $resolvedProjectPath -UnityPath $resolvedUnityPath
    $unityLogFile = Convert-FixedMathSharpPathForUnity -Path $LogFile -UnityPath $resolvedUnityPath
    $unityAdditionalArguments = @($AdditionalArguments)

    for ($i = 0; $i -lt $unityAdditionalArguments.Count - 1; $i++) {
        if ($PathArgumentNames -contains $unityAdditionalArguments[$i]) {
            $unityAdditionalArguments[$i + 1] = Convert-FixedMathSharpPathForUnity `
                -Path $unityAdditionalArguments[$i + 1] `
                -UnityPath $resolvedUnityPath
        }
    }

    $arguments = @(
        "-batchmode",
        "-quit",
        "-projectPath",
        $unityProjectPath,
        "-executeMethod",
        $ExecuteMethod,
        "-logFile",
        $unityLogFile
    ) + $unityAdditionalArguments

    Write-Output "Unity: $resolvedUnityPath"
    Write-Output "Project: $resolvedProjectPath"
    Write-Output "ExecuteMethod: $ExecuteMethod"

    if ($unityProjectPath -ne $resolvedProjectPath) {
        Write-Output "Unity project argument: $unityProjectPath"
    }

    if ($unityAdditionalArguments.Count -gt 0) {
        Write-Output "Additional arguments: $($unityAdditionalArguments -join ' ')"
    }

    if ($WhatIf) {
        Write-Output "WhatIf: skipping Unity launch."
        $displayArguments = @($arguments | ForEach-Object { Format-FixedMathSharpCommandArgument -Argument $_ })
        Write-Output "Command: $(Format-FixedMathSharpCommandArgument -Argument $resolvedUnityPath) $($displayArguments -join ' ')"
        return
    }

    $exitCode = Invoke-FixedMathSharpProcess -FilePath $resolvedUnityPath -Arguments $arguments

    if ($exitCode -ne 0) {
        throw "Unity batch method '$ExecuteMethod' failed with exit code $exitCode."
    }
}
