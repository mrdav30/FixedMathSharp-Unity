Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptUnderTest = Join-Path $PSScriptRoot "update-unity-package-versions.ps1"
$TestRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fixedmathsharp-versioning-tests-" + [System.Guid]::NewGuid().ToString("N"))

function Assert-Equal {
    param(
        [object]$Expected,
        [object]$Actual,
        [string]$Message
    )

    if ($Expected -ne $Actual) {
        throw "$Message Expected '$Expected', got '$Actual'."
    }
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Expected,
        [string]$Message
    )

    if (-not $Text.Contains($Expected)) {
        throw "$Message Expected text to contain '$Expected'. Actual text: $Text"
    }
}

function New-TestFile {
    param(
        [string]$Path,
        [string]$Content
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    Set-Content -Path $Path -Value $Content -Encoding UTF8
}

function New-TestRepo {
    $repoRoot = Join-Path $TestRoot ([System.Guid]::NewGuid().ToString("N"))

    New-TestFile -Path (Join-Path $repoRoot ".assets/unity-package-versions.json") -Content @'
{
  "packageRoot": ".",
  "packageVersion": "4.0.6",
  "dependencyVersions": {
    "FixedMathSharpUnity": "v4.0.0"
  }
}
'@

    New-TestFile -Path (Join-Path $repoRoot "com.mrdav30.fixedmathsharp/package.json") -Content @'
{
    "name": "com.mrdav30.fixedmathsharp",
    "version": "4.0.5",
    "displayName": "fixedmathsharp"
}
'@

    return $repoRoot
}

function Invoke-Updater {
    param(
        [string]$RepoRoot,
        [switch]$Apply,
        [switch]$ValidateOnly
    )

    $configPath = Join-Path $RepoRoot ".assets/unity-package-versions.json"

    if ($Apply) {
        $output = & $ScriptUnderTest -ConfigPath $configPath -Apply 2>&1 | Out-String
    }
    elseif ($ValidateOnly) {
        $output = & $ScriptUnderTest -ConfigPath $configPath -ValidateOnly 2>&1 | Out-String
    }
    else {
        $output = & $ScriptUnderTest -ConfigPath $configPath 2>&1 | Out-String
    }

    return $output.Trim()
}

function Get-PackageVersion {
    param([string]$Path)

    $manifest = Get-Content -Raw -Path $Path | ConvertFrom-Json
    return $manifest.version
}

function Test-DryRunReportsChangesWithoutMutatingFiles {
    $repoRoot = New-TestRepo
    $output = Invoke-Updater -RepoRoot $repoRoot

    Assert-Contains -Text $output -Expected "DRY-RUN" -Message "Dry-run output should identify the mode."
    Assert-Contains -Text $output -Expected "com.mrdav30.fixedmathsharp/package.json version 4.0.5 -> 4.0.6" -Message "Dry-run should report package version drift."

    Assert-Equal -Expected "4.0.5" -Actual (Get-PackageVersion (Join-Path $repoRoot "com.mrdav30.fixedmathsharp/package.json")) -Message "Dry-run must not mutate package.json."
}

function Test-ApplyUpdatesPackageAndInstallerVersions {
    $repoRoot = New-TestRepo
    $output = Invoke-Updater -RepoRoot $repoRoot -Apply

    Assert-Contains -Text $output -Expected "APPLY" -Message "Apply output should identify the mode."
    Assert-Equal -Expected "4.0.6" -Actual (Get-PackageVersion (Join-Path $repoRoot "com.mrdav30.fixedmathsharp/package.json")) -Message "Apply should update package.json."
    Assert-Equal -Expected "4.0.6" -Actual (Get-PackageVersion (Join-Path $repoRoot "com.mrdav30.fixedmathsharp.lean/package.json")) -Message "Apply should update every configured package.json."
}

function Test-ValidateOnlyFailsWhenFilesDrift {
    $repoRoot = New-TestRepo

    try {
        Invoke-Updater -RepoRoot $repoRoot -ValidateOnly | Out-Null
    }
    catch {
        Assert-Contains -Text $_.Exception.Message -Expected "Validation failed" -Message "ValidateOnly should explain drift failures."
        return
    }

    throw "ValidateOnly should fail when files do not match the config."
}

function Test-ValidateOnlyPassesAfterApply {
    $repoRoot = New-TestRepo
    Invoke-Updater -RepoRoot $repoRoot -Apply | Out-Null
    $output = Invoke-Updater -RepoRoot $repoRoot -ValidateOnly

    Assert-Contains -Text $output -Expected "VALIDATE" -Message "ValidateOnly output should identify the mode."
    Assert-Contains -Text $output -Expected "All configured Unity package versions are in sync." -Message "ValidateOnly should report success after apply."
}

try {
    New-Item -ItemType Directory -Path $TestRoot -Force | Out-Null

    $tests = @(
        "Test-DryRunReportsChangesWithoutMutatingFiles",
        "Test-ApplyUpdatesPackageAndInstallerVersions",
        "Test-ValidateOnlyFailsWhenFilesDrift",
        "Test-ValidateOnlyPassesAfterApply"
    )

    foreach ($test in $tests) {
        & $test
        Write-Host "PASS $test"
    }
}
finally {
    if (Test-Path $TestRoot) {
        Remove-Item -Path $TestRoot -Recurse -Force
    }
}
