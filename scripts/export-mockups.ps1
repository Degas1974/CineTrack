param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"
$mockupsDir = Join-Path $RepoRoot "mockups"
$pngDir = Join-Path $mockupsDir "png"
$htmlPath = Join-Path $mockupsDir "studio.html"
$runnerPath = Join-Path $PSScriptRoot "export-mockups.mjs"

if (-not (Test-Path $htmlPath)) {
    throw "studio.html not found at $htmlPath"
}

New-Item -ItemType Directory -Force -Path $pngDir | Out-Null

Push-Location $RepoRoot
try {
    $playwrightPkg = Join-Path $RepoRoot "node_modules\playwright"
    if (-not (Test-Path $playwrightPkg)) {
        Write-Host "Installing playwright (npm)..."
        npm install --no-save playwright 2>&1 | Write-Host
    }

    Write-Host "Ensuring Chromium for Playwright..."
    npx playwright install chromium 2>&1 | Write-Host

    Write-Host "Exporting mockup PNGs..."
    node $runnerPath
}
finally {
    Pop-Location
}

$expected = @(
    "home-default","home-loading","home-empty","home-error",
    "search-empty","search-loading","search-results","search-no-results",
    "detail-series","detail-series-seasons","detail-film","detail-loading","detail-not-found",
    "stats-default","stats-loading",
    "sync-pending","sync-synced","sync-offline","sync-syncing","sync-loading","sync-resolved","sync-diagnostic"
)

$legacyPatterns = @(
    "cinetrack-screens.png",
    "cinetrack-all-screens.png",
    "cinetrack-screens.html",
    "cinetrack-mockups.zip"
)
foreach ($legacy in $legacyPatterns) {
    $legacyPath = Join-Path $mockupsDir $legacy
    if (Test-Path $legacyPath) {
        Remove-Item -Force $legacyPath
        Write-Host "Removed legacy $legacy"
    }
}
$legacyDir = Join-Path $mockupsDir "cinetrack-mockups"
if (Test-Path $legacyDir) {
    Remove-Item -Recurse -Force $legacyDir
    Write-Host "Removed legacy cinetrack-mockups/"
}

$expectedSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$expected)
foreach ($file in Get-ChildItem -Path $pngDir -Filter "*.png" -File) {
    if (-not $expectedSet.Contains($file.BaseName)) {
        Remove-Item -Force $file.FullName
        Write-Host "Removed extra png/$($file.Name)"
    }
}
foreach ($file in Get-ChildItem -Path $mockupsDir -Filter "*.png" -File) {
    if (-not $expectedSet.Contains($file.BaseName)) {
        Remove-Item -Force $file.FullName
        Write-Host "Removed extra $($file.Name)"
    }
}

$missing = @()
foreach ($name in $expected) {
    $file = Join-Path $pngDir "$name.png"
    if (-not (Test-Path $file)) { $missing += $name }
}

if ($missing.Count -gt 0) {
    throw "Missing PNG exports: $($missing -join ', ')"
}

Add-Type -AssemblyName System.Drawing
foreach ($name in $expected) {
    $file = Join-Path $pngDir "$name.png"
    $img = [System.Drawing.Image]::FromFile($file)
    try {
        $w = $img.Width
        $h = $img.Height
        if ($w -ne 1290 -or $h -ne 2796) {
            throw "$name.png has dimensions ${w}x${h}, expected 1290x2796"
        }
        Write-Host "OK png/$name.png ${w}x${h}"
    }
    finally {
        $img.Dispose()
    }
}

Write-Host "Copying PNGs to mockups root..."
foreach ($name in $expected) {
    $src = Join-Path $pngDir "$name.png"
    $dest = Join-Path $mockupsDir "$name.png"
    Copy-Item -Path $src -Destination $dest -Force
}

foreach ($name in $expected) {
    $file = Join-Path $mockupsDir "$name.png"
    if (-not (Test-Path $file)) {
        throw "Missing copy in mockups root: $name.png"
    }
    $img = [System.Drawing.Image]::FromFile($file)
    try {
        $w = $img.Width
        $h = $img.Height
        if ($w -ne 1290 -or $h -ne 2796) {
            throw "$name.png (mockups root) has dimensions ${w}x${h}, expected 1290x2796"
        }
        Write-Host "OK $name.png ${w}x${h}"
    }
    finally {
        $img.Dispose()
    }
}

Write-Host "All 22 mockup PNGs exported to png/, copied to mockups root, and verified (44 files at 1290x2796)."