<#
.SYNOPSIS
Synchronizes Unity package manifest and dependency versions from config.

.DESCRIPTION
Reads unity-package-versions.json and compares the configured package version
and dependency versions against the package manifests and installer scripts.
By default the script runs in dry-run mode and reports pending changes. Pass
-Apply to update files or -ValidateOnly to fail when configured versions are out
of sync.

.PARAMETER ConfigPath
Path to the JSON version configuration file. Defaults to
.assets/unity-package-versions.json.

.PARAMETER Apply
Writes any required package manifest and installer dependency version changes.

.PARAMETER ValidateOnly
Reports version drift as errors and exits with a failure instead of writing
changes.
#>
[CmdletBinding()]
param(
    [string]$ConfigPath = (Join-Path (Split-Path -Parent $PSScriptRoot) "unity-package-versions.json"),
    [switch]$Apply,
    [switch]$ValidateOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Apply -and $ValidateOnly) {
    throw "Use either -Apply or -ValidateOnly, not both."
}

$Mode = if ($ValidateOnly) { "VALIDATE" } elseif ($Apply) { "APPLY" } else { "DRY-RUN" }

function Resolve-ConfiguredPath {
    param(
        [string]$BasePath,
        [string]$Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $Path))
}

function Get-DisplayPath {
    param(
        [string]$Path,
        [string]$BasePath
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullBase = [System.IO.Path]::GetFullPath($BasePath).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    ) + [System.IO.Path]::DirectorySeparatorChar

    if ($fullPath.StartsWith($fullBase, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($fullBase.Length).Replace("\", "/")
    }

    return $fullPath.Replace("\", "/")
}

function Get-RequiredString {
    param(
        [object]$Object,
        [string]$PropertyName,
        [string]$Context
    )

    $property = $Object.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or $null -eq $property.Value -or [string]::IsNullOrWhiteSpace([string]$property.Value)) {
        throw "$Context must define '$PropertyName'."
    }

    return [string]$property.Value
}

function Get-OptionalBoolean {
    param(
        [object]$Object,
        [string]$PropertyName,
        [bool]$DefaultValue
    )

    $property = $Object.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $DefaultValue
    }

    return [bool]$property.Value
}

function Read-JsonFile {
    param([string]$Path)

    try {
        return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    }
    catch {
        throw "Failed to parse JSON file '$Path': $($_.Exception.Message)"
    }
}

function Set-PackageManifestVersion {
    param(
        [string]$ManifestPath,
        [string]$DesiredVersion,
        [string]$DisplayPath,
        [System.Collections.Generic.List[string]]$ChangeMessages,
        [System.Collections.Generic.List[string]]$ValidationIssues
    )

    if (-not (Test-Path -LiteralPath $ManifestPath)) {
        throw "Configured package manifest does not exist: $DisplayPath"
    }

    $content = Get-Content -Raw -LiteralPath $ManifestPath
    $manifest = $content | ConvertFrom-Json
    $currentVersion = [string]$manifest.version

    if ([string]::IsNullOrWhiteSpace($currentVersion)) {
        throw "Package manifest '$DisplayPath' must define a non-empty version."
    }

    if ($currentVersion -eq $DesiredVersion) {
        return
    }

    $message = "$DisplayPath version $currentVersion -> $DesiredVersion"
    $ChangeMessages.Add($message)

    if ($ValidateOnly) {
        $ValidationIssues.Add($message)
        return
    }

    Write-Output "$Mode $message"

    if (-not $Apply) {
        return
    }

    $pattern = '("version"\s*:\s*")[^"]*(")'
    $newContent = [regex]::Replace(
        $content,
        $pattern,
        { param($match) $match.Groups[1].Value + $DesiredVersion + $match.Groups[2].Value },
        1
    )

    if ($newContent -eq $content) {
        throw "Could not update package manifest version in '$DisplayPath'."
    }

    Set-Content -LiteralPath $ManifestPath -Value $newContent -Encoding UTF8 -NoNewline
}

function Update-InstallerDependencies {
    param(
        [string]$InstallerPath,
        [object[]]$Dependencies,
        [hashtable]$DependencyVersions,
        [string]$DisplayPath,
        [System.Collections.Generic.List[string]]$ChangeMessages,
        [System.Collections.Generic.List[string]]$ValidationIssues
    )

    if (-not (Test-Path -LiteralPath $InstallerPath)) {
        throw "Configured dependency installer does not exist: $DisplayPath"
    }

    $content = Get-Content -Raw -LiteralPath $InstallerPath
    $updatedContent = $content
    $hasChanges = $false

    foreach ($dependency in $Dependencies) {
        $name = Get-RequiredString -Object $dependency -PropertyName "name" -Context "Dependency in '$DisplayPath'"
        $gitUrl = Get-RequiredString -Object $dependency -PropertyName "gitUrl" -Context "Dependency '$name' in '$DisplayPath'"
        $versionKey = Get-RequiredString -Object $dependency -PropertyName "versionKey" -Context "Dependency '$name' in '$DisplayPath'"

        if (-not $DependencyVersions.ContainsKey($versionKey)) {
            throw "Dependency '$name' in '$DisplayPath' uses unknown versionKey '$versionKey'."
        }

        $desiredVersion = [string]$DependencyVersions[$versionKey]
        if ([string]::IsNullOrWhiteSpace($desiredVersion)) {
            throw "dependencyVersions.$versionKey must be non-empty."
        }

        $pattern = '(?ms)(?<prefix>new\s*\(\s*"' +
            [regex]::Escape($name) +
            '"\s*,\s*")(?<gitUrl>[^"]+)(?<middle>"\s*,\s*")(?<version>[^"]+)(?<suffix>")'

        $match = [regex]::Match($updatedContent, $pattern)
        if (-not $match.Success) {
            throw "Could not find dependency '$name' in '$DisplayPath'."
        }

        $currentGitUrl = $match.Groups["gitUrl"].Value
        $currentVersion = $match.Groups["version"].Value
        $dependencyChanged = $false

        if ($currentGitUrl -ne $gitUrl) {
            $message = "$DisplayPath $name dependency gitUrl $currentGitUrl -> $gitUrl"
            $ChangeMessages.Add($message)

            if ($ValidateOnly) {
                $ValidationIssues.Add($message)
            }
            else {
                Write-Output "$Mode $message"
            }

            $dependencyChanged = $true
        }

        if ($currentVersion -ne $desiredVersion) {
            $message = "$DisplayPath $name dependency version $currentVersion -> $desiredVersion"
            $ChangeMessages.Add($message)

            if ($ValidateOnly) {
                $ValidationIssues.Add($message)
            }
            else {
                Write-Output "$Mode $message"
            }

            $dependencyChanged = $true
        }

        if ($dependencyChanged) {
            $hasChanges = $true

            if ($Apply) {
                $replacement = $match.Groups["prefix"].Value +
                    $gitUrl +
                    $match.Groups["middle"].Value +
                    $desiredVersion +
                    $match.Groups["suffix"].Value

                $updatedContent = $updatedContent.Remove($match.Index, $match.Length).Insert($match.Index, $replacement)
            }
        }
    }

    if ($Apply -and $hasChanges) {
        Set-Content -LiteralPath $InstallerPath -Value $updatedContent -Encoding UTF8 -NoNewline
    }
}

$resolvedConfig = Resolve-Path -LiteralPath $ConfigPath
$configFile = $resolvedConfig.ProviderPath
$configDirectory = Split-Path -Parent $configFile
$repoRoot = Split-Path -Parent $configDirectory
$config = Read-JsonFile -Path $configFile

$packageRootValue = Get-RequiredString -Object $config -PropertyName "packageRoot" -Context "Version config"
$packageRoot = Resolve-ConfiguredPath -BasePath $repoRoot -Path $packageRootValue
$packageVersion = Get-RequiredString -Object $config -PropertyName "packageVersion" -Context "Version config"

if ($packageVersion.StartsWith("v", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "packageVersion should not include a leading 'v'. Unity package manifests expect values like '4.0.6'."
}

$dependencyVersions = @{}
$dependencyVersionProperty = $config.PSObject.Properties["dependencyVersions"]
if ($null -ne $dependencyVersionProperty -and $null -ne $dependencyVersionProperty.Value) {
    foreach ($property in $dependencyVersionProperty.Value.PSObject.Properties) {
        $dependencyVersions[$property.Name] = [string]$property.Value
    }
}

$packagesProperty = $config.PSObject.Properties["packages"]
if ($null -eq $packagesProperty -or $null -eq $packagesProperty.Value -or @($packagesProperty.Value).Count -eq 0) {
    throw "Version config must define at least one package."
}

$changeMessages = [System.Collections.Generic.List[string]]::new()
$validationIssues = [System.Collections.Generic.List[string]]::new()

Write-Output "$Mode Unity package version sync using $(Get-DisplayPath -Path $configFile -BasePath $repoRoot)"

foreach ($package in @($packagesProperty.Value)) {
    $packagePath = Get-RequiredString -Object $package -PropertyName "path" -Context "Package entry"
    $packageDirectory = Resolve-ConfiguredPath -BasePath $packageRoot -Path $packagePath
    $manifestPath = Join-Path $packageDirectory "package.json"
    $manifestDisplayPath = Get-DisplayPath -Path $manifestPath -BasePath $repoRoot

    if (Get-OptionalBoolean -Object $package -PropertyName "updatePackageVersion" -DefaultValue $true) {
        Set-PackageManifestVersion `
            -ManifestPath $manifestPath `
            -DesiredVersion $packageVersion `
            -DisplayPath $manifestDisplayPath `
            -ChangeMessages $changeMessages `
            -ValidationIssues $validationIssues
    }

    $installerProperty = $package.PSObject.Properties["installer"]
    if ($null -eq $installerProperty -or [string]::IsNullOrWhiteSpace([string]$installerProperty.Value)) {
        continue
    }

    $dependenciesProperty = $package.PSObject.Properties["dependencies"]
    if ($null -eq $dependenciesProperty -or $null -eq $dependenciesProperty.Value -or @($dependenciesProperty.Value).Count -eq 0) {
        throw "Package '$packagePath' defines an installer but no dependencies."
    }

    $installerPath = Resolve-ConfiguredPath -BasePath $packageDirectory -Path ([string]$installerProperty.Value)
    $installerDisplayPath = Get-DisplayPath -Path $installerPath -BasePath $repoRoot

    Update-InstallerDependencies `
        -InstallerPath $installerPath `
        -Dependencies @($dependenciesProperty.Value) `
        -DependencyVersions $dependencyVersions `
        -DisplayPath $installerDisplayPath `
        -ChangeMessages $changeMessages `
        -ValidationIssues $validationIssues
}

if ($ValidateOnly -and $validationIssues.Count -gt 0) {
    foreach ($issue in $validationIssues) {
        Write-Error "VALIDATE $issue" -ErrorAction Continue
    }

    throw "Validation failed with $($validationIssues.Count) issue(s)."
}

if ($changeMessages.Count -eq 0) {
    Write-Output "All configured Unity package versions are in sync."
}
