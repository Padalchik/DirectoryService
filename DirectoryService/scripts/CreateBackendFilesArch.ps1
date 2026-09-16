param(
    [string]$SourceRoot = "C:\Programmer\DirectoryService\DirectoryService\src",
    [string]$TestsRoot = "C:\Programmer\DirectoryService\DirectoryService\Tests",
    [string]$OutputDirectory = "C:\Programmer\DirectoryService\DirectoryService\artifacts",
    [string]$ArchiveName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-NormalizedFullPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    return [System.IO.Path]::GetFullPath($Path)
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$BasePath,
        [Parameter(Mandatory = $true)][string]$TargetPath
    )

    $normalizedBase = Get-NormalizedFullPath $BasePath
    $normalizedTarget = Get-NormalizedFullPath $TargetPath

    if ($normalizedTarget.Equals($normalizedBase, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ""
    }

    if (-not $normalizedBase.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $normalizedBase += [System.IO.Path]::DirectorySeparatorChar
    }

    if (-not $normalizedTarget.StartsWith($normalizedBase, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path '$TargetPath' is outside '$BasePath'."
    }

    return $normalizedTarget.Substring($normalizedBase.Length).Replace("\", "/")
}

function Test-ExcludedBackendPath {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = $RelativePath.Replace("\", "/")
    $excludedDirectories = @(
        "bin/",
        "obj/",
        "TestResults/",
        ".vs/",
        ".idea/",
        "artifacts/"
    )

    foreach ($directory in $excludedDirectories) {
        if ($path.StartsWith($directory, [System.StringComparison]::OrdinalIgnoreCase) -or
            $path.IndexOf("/$directory", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return $true
        }
    }

    $fileName = [System.IO.Path]::GetFileName($RelativePath)

    if ($fileName -in @(".DS_Store", "Thumbs.db")) {
        return $true
    }

    if ($fileName.EndsWith(".log", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    $excludedFileNames = @(
        ".env",
        "secrets.json",
        "launchSettings.json",
        "appsettings.Development.json",
        "appsettings.Local.json"
    )

    foreach ($excludedFileName in $excludedFileNames) {
        if ($fileName.Equals($excludedFileName, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    if ($fileName.StartsWith(".env.", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    return $false
}

function Get-ProjectRoots {
    param([Parameter(Mandatory = $true)][string]$RootPath)

    $normalizedRoot = Get-NormalizedFullPath $RootPath

    if (-not (Test-Path -LiteralPath $normalizedRoot -PathType Container)) {
        return @()
    }

    $projectFiles = Get-ChildItem -LiteralPath $normalizedRoot -Filter "*.csproj" -File -Recurse -Force |
        Where-Object {
            $relativePath = Get-RelativePath -BasePath $normalizedRoot -TargetPath $_.FullName
            -not (Test-ExcludedBackendPath -RelativePath $relativePath)
        }

    return @(
        $projectFiles |
            ForEach-Object { $_.Directory.FullName } |
            Sort-Object -Unique
    )
}

function Copy-ProjectFiles {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectRoot,
        [Parameter(Mandatory = $true)][string]$ProjectsBaseRoot,
        [Parameter(Mandatory = $true)][string]$DestinationRoot
    )

    $normalizedProjectRoot = Get-NormalizedFullPath $ProjectRoot
    $normalizedProjectsBaseRoot = Get-NormalizedFullPath $ProjectsBaseRoot
    $relativeProjectPath = Get-RelativePath -BasePath $normalizedProjectsBaseRoot -TargetPath $normalizedProjectRoot

    if ([string]::IsNullOrWhiteSpace($relativeProjectPath)) {
        $relativeProjectPath = Split-Path -Leaf $normalizedProjectRoot
    }

    $projectTargetRoot = Join-Path $DestinationRoot ($relativeProjectPath.Replace("/", [System.IO.Path]::DirectorySeparatorChar))
    $copiedCount = 0

    $projectFiles = Get-ChildItem -LiteralPath $normalizedProjectRoot -File -Recurse -Force |
        Where-Object {
            $relativePath = Get-RelativePath -BasePath $normalizedProjectRoot -TargetPath $_.FullName
            -not (Test-ExcludedBackendPath -RelativePath $relativePath)
        }

    foreach ($file in $projectFiles) {
        $relativePath = Get-RelativePath -BasePath $normalizedProjectRoot -TargetPath $file.FullName
        $targetPath = Join-Path $projectTargetRoot ($relativePath.Replace("/", [System.IO.Path]::DirectorySeparatorChar))
        $targetDirectory = Split-Path -Parent $targetPath

        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $targetPath -Force
        $copiedCount++
    }

    return $copiedCount
}

$sourceRootNormalized = Get-NormalizedFullPath $SourceRoot
if (-not (Test-Path -LiteralPath $sourceRootNormalized -PathType Container)) {
    throw "Source root does not exist: $sourceRootNormalized"
}

$outputRoot = Get-NormalizedFullPath $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

if ([string]::IsNullOrWhiteSpace($ArchiveName)) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $ArchiveName = "backend-files-$timestamp.zip"
}

if (-not $ArchiveName.EndsWith(".zip", [System.StringComparison]::OrdinalIgnoreCase)) {
    $ArchiveName = "$ArchiveName.zip"
}

$archivePath = Get-NormalizedFullPath (Join-Path $outputRoot $ArchiveName)
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("directoryservice-backend-export-" + [System.Guid]::NewGuid().ToString("N"))
$normalizedTempRoot = Get-NormalizedFullPath $tempRoot
$systemTempRoot = Get-NormalizedFullPath ([System.IO.Path]::GetTempPath())

if (-not $normalizedTempRoot.StartsWith($systemTempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Temporary export path is outside the system temp directory: $normalizedTempRoot"
}

New-Item -ItemType Directory -Path $normalizedTempRoot -Force | Out-Null

try {
    $backendProjectRoots = @(Get-ProjectRoots -RootPath $sourceRootNormalized)

    if ($backendProjectRoots.Count -eq 0) {
        throw "No .csproj projects were found under source root: $sourceRootNormalized"
    }

    $backendFileCount = 0
    foreach ($projectRoot in $backendProjectRoots) {
        $backendFileCount += Copy-ProjectFiles `
            -ProjectRoot $projectRoot `
            -ProjectsBaseRoot $sourceRootNormalized `
            -DestinationRoot $normalizedTempRoot
    }

    $testFileCount = 0
    $testProjectCount = 0
    $testsRootNormalized = Get-NormalizedFullPath $TestsRoot

    if (Test-Path -LiteralPath $testsRootNormalized -PathType Container) {
        $testProjectRoots = @(Get-ProjectRoots -RootPath $testsRootNormalized)
        $testProjectCount = $testProjectRoots.Count

        if ($testProjectCount -gt 0) {
            $testsTargetRoot = Join-Path $normalizedTempRoot "Tests"

            foreach ($projectRoot in $testProjectRoots) {
                $testFileCount += Copy-ProjectFiles `
                    -ProjectRoot $projectRoot `
                    -ProjectsBaseRoot $testsRootNormalized `
                    -DestinationRoot $testsTargetRoot
            }
        }
    }

    if ($backendFileCount -eq 0) {
        throw "No backend files were found for export."
    }

    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }

    Compress-Archive `
        -Path (Join-Path $normalizedTempRoot "*") `
        -DestinationPath $archivePath `
        -CompressionLevel Optimal

    Write-Host "Backend files archive created:"
    Write-Host $archivePath
    Write-Host "Backend projects included: $($backendProjectRoots.Count)"
    Write-Host "Backend files included: $backendFileCount"

    if ($testProjectCount -gt 0) {
        Write-Host "Test projects included: $testProjectCount"
        Write-Host "Test files included: $testFileCount"
    }
    else {
        Write-Host "Test projects included: 0 (Tests root absent or no .csproj projects found)"
    }
}
finally {
    if ((Test-Path -LiteralPath $normalizedTempRoot) -and
        $normalizedTempRoot.StartsWith($systemTempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        Remove-Item -LiteralPath $normalizedTempRoot -Recurse -Force
    }
}
