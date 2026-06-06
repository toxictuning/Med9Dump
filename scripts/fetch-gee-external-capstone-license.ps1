# Usage: run from repository root (PowerShell)
param()

$Package = "Gee.External.Capstone"
$RepoRoot = (Get-Location).Path
$TempDir = Join-Path $RepoRoot "tmp_fetch_license"
$LicDir = Join-Path $RepoRoot "licenses"
New-Item -ItemType Directory -Force -Path $TempDir | Out-Null
New-Item -ItemType Directory -Force -Path $LicDir | Out-Null

Write-Host "Querying NuGet for package $Package..."
$indexUrl = "https://api.nuget.org/v3-flatcontainer/$($Package.ToLower())/index.json"
try {
    $index = Invoke-RestMethod -Uri $indexUrl -UseBasicParsing -ErrorAction Stop
} catch {
    Write-Error "Failed to query NuGet API: $_"
    exit 1
}

$version = $index.versions[-1]
Write-Host "Latest version: $version"

$nupkgUrl = "https://api.nuget.org/v3-flatcontainer/$($Package.ToLower())/$version/$($Package).$version.nupkg"
$nupkgPath = Join-Path $TempDir "$Package.$version.nupkg"

Write-Host "Downloading package..."
Invoke-WebRequest -Uri $nupkgUrl -OutFile $nupkgPath -UseBasicParsing

$extractDir = Join-Path $TempDir "pkg"
if (Test-Path $extractDir) { Remove-Item $extractDir -Recurse -Force }
Expand-Archive -LiteralPath $nupkgPath -DestinationPath $extractDir

# Try to find a license file inside the package
$licenseCandidates = Get-ChildItem -Path $extractDir -Recurse -File |
    Where-Object { $_.Name -match '^(LICENSE|license|COPYING|LICENCE|LICENSE.txt|license.txt|LICENSE.md|license.md)$' }

$destFile = Join-Path $LicDir "Gee.External.Capstone.txt"

if ($licenseCandidates -and $licenseCandidates.Count -gt 0) {
    Write-Host "Found license file inside package: $($licenseCandidates[0].FullName). Copying to $destFile"
    Get-Content $licenseCandidates[0].FullName -Raw | Out-File -Encoding utf8 $destFile
} else {
    # Inspect nuspec for license expression or licenseUrl
    $nuspec = Get-ChildItem -Path $extractDir -Filter "*.nuspec" -Recurse -File | Select-Object -First 1
    if (-not $nuspec) {
        Write-Warning "No .nuspec found. Manual check required."
        "`nPlaceholder: Could not locate license inside the package. Please verify the license manually." | Out-File -Encoding utf8 $destFile
    } else {
        $nuspecXml = [xml](Get-Content $nuspec.FullName -Raw)
        $ns = $nuspecXml.nuspec.package.metadata
        $licenseNode = $ns.license
        $licenseUrlNode = $ns.licenseUrl
        if ($licenseNode -and $licenseNode.'#text') {
            $expr = $licenseNode.'#text'
            $type = $licenseNode.type
            $outText = "License expression found in .nuspec: `nType: $type`nExpression: $expr`n`nPlease map expression to full license text (e.g., MIT, BSD-3-Clause)."
            $outText | Out-File -Encoding utf8 $destFile
            Write-Host "Wrote license expression to $destFile"
        } elseif ($licenseUrlNode -and $licenseUrlNode -ne '') {
            $url = $licenseUrlNode.Trim()
            Write-Host "Found licenseUrl in .nuspec: $url"
            try {
                $licText = Invoke-WebRequest -Uri $url -UseBasicParsing -ErrorAction Stop
                $licText.Content | Out-File -Encoding utf8 $destFile
                Write-Host "Downloaded license from $url -> $destFile"
            } catch {
                Write-Warning "Failed to download licenseUrl. Writing placeholder to $destFile"
                "Placeholder: licenseUrl found ($url) but download failed. Please fetch manually." | Out-File -Encoding utf8 $destFile
            }
        } else {
            Write-Warning "No license info in .nuspec. Please verify upstream project license and add it to $destFile"
            "Placeholder: no license file or license metadata found in package. Please verify upstream license and replace this file." | Out-File -Encoding utf8 $destFile
        }
    }
}

# Cleanup
Remove-Item -Path $TempDir -Recurse -Force

Write-Host "Done. Check $destFile and update if necessary."