[CmdletBinding()]
param(
    [string]$RepoPath = "C:\Programmer\DirectoryService\DirectoryService",
    [string]$OutputDirectory = "C:\Programmer\DirectoryService\DirectoryService\artifacts",
    [string]$BaseRef = "HEAD",
    [int]$MaxFileSizeMB = 5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Keep Git path/content output readable when the script is run from Windows PowerShell 5.1 / PS2EXE.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$OutputEncoding = $utf8NoBom
try {
    [Console]::OutputEncoding = $utf8NoBom
}
catch {
    # Some non-console hosts do not expose Console.OutputEncoding.
}

function Convert-GitOutputToStrings {
    param(
        [Parameter(ValueFromPipeline = $true)]
        $InputObject
    )

    process {
        if ($null -eq $InputObject) {
            return
        }

        if ($InputObject -is [System.Management.Automation.ErrorRecord]) {
            [string]$InputObject.Exception.Message
            return
        }

        [string]$InputObject
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [switch]$AllowNonZeroExit
    )

    $stderrPath = [System.IO.Path]::GetTempFileName()
    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    try {
        $gitArguments = @("-c", "core.quotepath=false") + $Arguments
        $rawOutput = & git -C $script:RepoRoot @gitArguments 2> $stderrPath
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldPreference
    }

    $output = @($rawOutput | Convert-GitOutputToStrings)
    $stderr = @()

    if (Test-Path -LiteralPath $stderrPath) {
        $stderr = @(Get-Content -LiteralPath $stderrPath -ErrorAction SilentlyContinue)
        Remove-Item -LiteralPath $stderrPath -Force -ErrorAction SilentlyContinue
    }

    if (-not $AllowNonZeroExit -and $exitCode -ne 0) {
        $details = @($output + $stderr) -join "`n"
        throw "git $($Arguments -join ' ') завершился с кодом $exitCode.`n$details"
    }

    [pscustomobject]@{
        ExitCode = $exitCode
        Output = $output
        ErrorOutput = $stderr
    }
}

function Get-NormalizedGitLines {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    @(
        (Invoke-Git -Arguments $Arguments).Output |
            ForEach-Object { [string]$_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Get-PathExclusionReason {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $normalized = $RelativePath.Replace("\", "/")
    $fileName = [IO.Path]::GetFileName($normalized)

    if ($normalized -match '(^|/)(\.git|\.vs|bin|obj|node_modules|dist|coverage|TestResults|\.idea|artifacts)(/|$)') {
        return "generated/build/tooling path"
    }

    if (
        $fileName -match '^\.env($|\.)' -or
        $fileName -match '^appsettings\.(Production|Local)\.json$' -or
        $fileName -match '^secrets\.json$' -or
        $fileName -match '^launchSettings\.json$'
    ) {
        return "potentially sensitive configuration"
    }

    if ($fileName -match '\.(pfx|p12|pem|key|cer|crt)$') {
        return "certificate/key material"
    }

    if ($fileName -match '\.(zip|7z|rar|dll|exe|pdb|db|sqlite|sqlite3|log)$') {
        return "binary/archive/runtime artifact"
    }

    return $null
}

function Get-BaseBlobInfo {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $spec = "$script:BaseCommit`:$RelativePath"
    $oidResult = Invoke-Git -Arguments @("rev-parse", "--verify", $spec) -AllowNonZeroExit

    if ($oidResult.ExitCode -ne 0 -or $oidResult.Output.Count -eq 0) {
        return $null
    }

    $oid = ([string]$oidResult.Output[0]).Trim()
    if ([string]::IsNullOrWhiteSpace($oid)) {
        return $null
    }

    $sizeResult = Invoke-Git -Arguments @("cat-file", "-s", $oid)
    [long]$size = 0

    if ($sizeResult.Output.Count -eq 0 -or -not [long]::TryParse(([string]$sizeResult.Output[0]).Trim(), [ref]$size)) {
        throw "Не удалось определить размер blob для $RelativePath."
    }

    [pscustomobject]@{
        Oid = $oid
        Size = $size
    }
}

function Export-GitBlob {
    param(
        [Parameter(Mandatory = $true)][string]$Oid,
        [Parameter(Mandatory = $true)][string]$DestinationPath
    )

    $parent = Split-Path -Parent $DestinationPath
    if ($parent) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = "git"
    $startInfo.Arguments = "-C `"$script:RepoRoot`" cat-file blob $Oid"
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo

    if (-not $process.Start()) {
        throw "Не удалось запустить git cat-file."
    }

    $fileStream = [System.IO.File]::Open(
        $DestinationPath,
        [System.IO.FileMode]::Create,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None
    )

    try {
        $process.StandardOutput.BaseStream.CopyTo($fileStream)
    }
    finally {
        $fileStream.Dispose()
    }

    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        Remove-Item -LiteralPath $DestinationPath -Force -ErrorAction SilentlyContinue
        throw "git cat-file завершился с кодом $($process.ExitCode): $stderr"
    }
}

function Copy-CurrentFile {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$DestinationRoot
    )

    $source = Join-Path $script:RepoRoot $RelativePath
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        return
    }

    $destination = Join-Path $DestinationRoot $RelativePath
    $parent = Split-Path -Parent $destination

    if ($parent) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    Copy-Item -LiteralPath $source -Destination $destination -Force
}

function Add-TrackedChanges {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Changes
    )

    $lines = Get-NormalizedGitLines -Arguments @(
        "diff",
        $BaseRef,
        "--name-status",
        "--find-renames",
        "--find-copies"
    )

    foreach ($line in $lines) {
        $parts = $line -split "`t"
        if ($parts.Count -lt 2) {
            continue
        }

        $rawStatus = $parts[0]
        $code = $rawStatus.Substring(0, 1)

        if (($code -eq "R" -or $code -eq "C") -and $parts.Count -ge 3) {
            $Changes.Add([pscustomobject]@{
                Status = $rawStatus
                Code = $code
                OldPath = $parts[1]
                NewPath = $parts[2]
                IsUntracked = $false
                ExclusionReason = $null
                BaseBlob = $null
            }) | Out-Null
        }
        else {
            $path = $parts[1]
            $Changes.Add([pscustomobject]@{
                Status = $rawStatus
                Code = $code
                OldPath = $(if ($code -eq "A") { $null } else { $path })
                NewPath = $(if ($code -eq "D") { $null } else { $path })
                IsUntracked = $false
                ExclusionReason = $null
                BaseBlob = $null
            }) | Out-Null
        }
    }
}

function Add-UntrackedChanges {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Changes
    )

    $untracked = Get-NormalizedGitLines -Arguments @(
        "ls-files",
        "--others",
        "--exclude-standard"
    )

    foreach ($path in $untracked) {
        $Changes.Add([pscustomobject]@{
            Status = "??"
            Code = "?"
            OldPath = $null
            NewPath = $path
            IsUntracked = $true
            ExclusionReason = $null
            BaseBlob = $null
        }) | Out-Null
    }
}

function Get-ChangeExclusionReason {
    param([Parameter(Mandatory = $true)]$Change)

    $paths = @()
    if ($Change.OldPath) { $paths += [string]$Change.OldPath }
    if ($Change.NewPath) { $paths += [string]$Change.NewPath }

    foreach ($path in $paths) {
        $reason = Get-PathExclusionReason -RelativePath $path
        if ($reason) {
            return $reason
        }
    }

    $maxBytes = [long]$MaxFileSizeMB * 1MB

    if ($Change.OldPath) {
        $baseBlob = Get-BaseBlobInfo -RelativePath $Change.OldPath
        $Change.BaseBlob = $baseBlob

        if ($null -ne $baseBlob -and $baseBlob.Size -gt $maxBytes) {
            return "base file is larger than $MaxFileSizeMB MB"
        }
    }

    if ($Change.NewPath) {
        $currentPath = Join-Path $script:RepoRoot $Change.NewPath
        if (Test-Path -LiteralPath $currentPath -PathType Leaf) {
            $currentItem = Get-Item -LiteralPath $currentPath

            if ($currentItem.Length -gt $maxBytes) {
                return "current file is larger than $MaxFileSizeMB MB"
            }
        }
    }

    return $null
}

function Write-ChangesFile {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Changes
    )

    $target = Join-Path $script:WorkDir "changes.txt"

    @(
        "# Status: M=modified, A=added, D=deleted, R=renamed, C=copied, ??=untracked"
        "# '[content excluded]' means the path is listed, but its contents are omitted from patch/before/current."
        ""
    ) | Set-Content -LiteralPath $target -Encoding UTF8

    foreach ($change in $Changes) {
        if ($change.Code -eq "R" -or $change.Code -eq "C") {
            $line = "$($change.Status)`t$($change.OldPath) -> $($change.NewPath)"
        }
        elseif ($change.IsUntracked) {
            $line = "??`t$($change.NewPath)"
        }
        elseif ($change.Code -eq "D") {
            $line = "$($change.Status)`t$($change.OldPath)"
        }
        else {
            $line = "$($change.Status)`t$($change.NewPath)"
        }

        if ($change.ExclusionReason) {
            $line += " [content excluded: $($change.ExclusionReason)]"
        }

        Add-Content -LiteralPath $target -Value $line -Encoding UTF8
    }
}

function Write-CanonicalPatch {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Changes
    )

    $target = Join-Path $script:WorkDir "changes.patch"
    New-Item -ItemType File -Path $target -Force | Out-Null

    foreach ($change in $Changes) {
        if ($change.IsUntracked -or $change.ExclusionReason) {
            continue
        }

        $paths = @()
        if ($change.OldPath) { $paths += [string]$change.OldPath }
        if ($change.NewPath -and $change.NewPath -ne $change.OldPath) { $paths += [string]$change.NewPath }

        if ($paths.Count -eq 0) {
            continue
        }

        $args = @(
            "diff",
            $BaseRef,
            "--no-ext-diff",
            "--find-renames",
            "--find-copies",
            "--full-index",
            "--"
        ) + $paths

        $result = Invoke-Git -Arguments $args

        if ($result.Output.Count -gt 0) {
            Add-Content -LiteralPath $target -Value $result.Output -Encoding UTF8
        }
    }
}

function Export-BeforeAndCurrent {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Changes
    )

    foreach ($change in $Changes) {
        if ($change.ExclusionReason) {
            continue
        }

        if ($change.OldPath) {
            $baseBlob = $change.BaseBlob
            if ($null -eq $baseBlob) {
                $baseBlob = Get-BaseBlobInfo -RelativePath $change.OldPath
            }

            if ($null -ne $baseBlob) {
                $destination = Join-Path $script:BeforeDir $change.OldPath
                Export-GitBlob -Oid $baseBlob.Oid -DestinationPath $destination
            }
        }

        if ($change.NewPath) {
            Copy-CurrentFile -RelativePath $change.NewPath -DestinationRoot $script:CurrentDir
        }
    }
}

function Get-CountByCode {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Changes,
        [Parameter(Mandatory = $true)]
        [string]$Code
    )

    @($Changes | Where-Object { $_.Code -eq $Code }).Count
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    throw "Git не найден в PATH."
}

if (-not (Test-Path -LiteralPath $RepoPath -PathType Container)) {
    throw "Путь к репозиторию не существует: $RepoPath"
}

$resolvedRepo = (Resolve-Path -LiteralPath $RepoPath).Path

$probeRaw = & git -C $resolvedRepo rev-parse --show-toplevel 2>$null
$probeExit = $LASTEXITCODE

if ($probeExit -ne 0 -or $null -eq $probeRaw) {
    throw "Путь не является Git-репозиторием: $resolvedRepo"
}

$script:RepoRoot = ([string]($probeRaw | Select-Object -First 1)).Trim()
$script:BaseCommit = ((Invoke-Git -Arguments @("rev-parse", "--verify", "$BaseRef^{commit}")).Output | Select-Object -First 1).Trim()

$branch = ((Invoke-Git -Arguments @("branch", "--show-current")).Output | Select-Object -First 1)
if ([string]::IsNullOrWhiteSpace($branch)) {
    $branch = "(detached HEAD)"
}

$head = ((Invoke-Git -Arguments @("rev-parse", "HEAD")).Output | Select-Object -First 1).Trim()

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$name = "git-review-$timestamp"
$script:WorkDir = Join-Path $env:TEMP $name
$script:BeforeDir = Join-Path $script:WorkDir "before"
$script:CurrentDir = Join-Path $script:WorkDir "current"
$zipPath = Join-Path $OutputDirectory "$name.zip"

if (Test-Path -LiteralPath $script:WorkDir) {
    Remove-Item -LiteralPath $script:WorkDir -Recurse -Force
}

try {
    New-Item -ItemType Directory -Path $script:WorkDir -Force | Out-Null
    New-Item -ItemType Directory -Path $script:BeforeDir -Force | Out-Null
    New-Item -ItemType Directory -Path $script:CurrentDir -Force | Out-Null

    $changes = [System.Collections.Generic.List[object]]::new()
    Add-TrackedChanges -Changes $changes
    Add-UntrackedChanges -Changes $changes

    foreach ($change in $changes) {
        $change.ExclusionReason = Get-ChangeExclusionReason -Change $change
    }

    Write-ChangesFile -Changes $changes
    Write-CanonicalPatch -Changes $changes
    Export-BeforeAndCurrent -Changes $changes

    $diffCheck = Invoke-Git -Arguments @("diff", $BaseRef, "--check") -AllowNonZeroExit
    $diffCheckStatus = if ($diffCheck.ExitCode -eq 0) { "PASS" } else { "FAIL" }

    $excluded = @($changes | Where-Object { -not [string]::IsNullOrWhiteSpace($_.ExclusionReason) })

    if ($excluded.Count -gt 0) {
        $excludedPath = Join-Path $script:WorkDir "excluded.txt"

        @(
            "# These changed paths are intentionally omitted from changes.patch, before/ and current/."
            "# The status/path remains visible in changes.txt."
            ""
        ) | Set-Content -LiteralPath $excludedPath -Encoding UTF8

        foreach ($change in $excluded) {
            if ($change.Code -eq "R" -or $change.Code -eq "C") {
                $display = "$($change.OldPath) -> $($change.NewPath)"
            }
            elseif ($change.Code -eq "D") {
                $display = $change.OldPath
            }
            else {
                $display = $change.NewPath
            }

            Add-Content -LiteralPath $excludedPath `
                -Value "$($change.Status)`t$display`t$($change.ExclusionReason)" `
                -Encoding UTF8
        }
    }

    $modifiedCount = Get-CountByCode -Changes $changes -Code "M"
    $addedCount = Get-CountByCode -Changes $changes -Code "A"
    $deletedCount = Get-CountByCode -Changes $changes -Code "D"
    $renamedCount = Get-CountByCode -Changes $changes -Code "R"
    $copiedCount = Get-CountByCode -Changes $changes -Code "C"
    $untrackedCount = @($changes | Where-Object { $_.IsUntracked }).Count
    $otherTrackedCount = @(
        $changes |
            Where-Object {
                -not $_.IsUntracked -and
                $_.Code -notin @("M", "A", "D", "R", "C")
            }
    ).Count

    @"
# Git review

Repository: $script:RepoRoot
Branch: $branch
BaseRef: $BaseRef
Resolved base commit: $script:BaseCommit
HEAD: $head
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")

## Change summary

- Modified: $modifiedCount
- Added (tracked): $addedCount
- Deleted: $deletedCount
- Renamed: $renamedCount
- Copied: $copiedCount
- Untracked: $untrackedCount
- Other tracked statuses: $otherTrackedCount
- Content-excluded paths: $($excluded.Count)
- git diff $BaseRef --check: **$diffCheckStatus**

## Archive contract

- changes.txt is the canonical file/status index.
- changes.patch is the canonical tracked-file diff against BaseRef.
- before/ contains complete versions from the resolved base commit.
- current/ contains complete current working-tree versions.
- Added/untracked files exist only in current/.
- Deleted files exist only in before/.
- Renames use the old path in before/ and the new path in current/.
- Untracked files are listed in changes.txt and copied to current/; they are not part of changes.patch.
- Sensitive/generated/oversized paths are listed but their contents are omitted. See excluded.txt when present.
- Binary files are never emitted as binary Git patches; Git records only its normal binary-difference marker.
- git diff --check raw output is intentionally not archived because it may echo changed line content.

This archive is intended to be self-contained for reviewing what changed, how it changed, and the complete before/after context without duplicating multiple Git status/stat formats.
"@ | Set-Content -LiteralPath (Join-Path $script:WorkDir "REVIEW.md") -Encoding UTF8

    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        $script:WorkDir,
        $zipPath,
        [System.IO.Compression.CompressionLevel]::Optimal,
        $false
    )

    Write-Host ""
    Write-Host "Git review archive created:" -ForegroundColor Green
    Write-Host $zipPath
    Write-Host ""
    Write-Host "Base:" -ForegroundColor Cyan
    Write-Host "$BaseRef -> $script:BaseCommit"
    Write-Host ""
    Write-Host "Changes: $($changes.Count)"
    Write-Host "Excluded contents: $($excluded.Count)"
    Write-Host "git diff --check: $diffCheckStatus"
}
finally {
    if (Test-Path -LiteralPath $script:WorkDir) {
        Remove-Item -LiteralPath $script:WorkDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
