param(
    [string]$WorkspaceRoot = "d:\Solucao TaskList",
    [string]$BaseUrl = "",
    [string]$ApiKey = "",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

function Normalize-BaseUrl {
    param([string]$Url)

    if ([string]::IsNullOrWhiteSpace($Url)) {
        throw "BaseUrl nao informada."
    }

    $trimmed = $Url.Trim()
    if (-not $trimmed.EndsWith("/")) {
        $trimmed = "$trimmed/"
    }

    return $trimmed
}

function Set-JsonProperty {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name,
        $Value
    )

    if ($Object.PSObject.Properties.Name -contains $Name) {
        $Object.$Name = $Value
    }
    else {
        $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value
    }
}

$normalizedBaseUrl = Normalize-BaseUrl -Url $BaseUrl
$appsettingsPath = Join-Path $WorkspaceRoot "CineTrack.Mobile\appsettings.json"

if (-not (Test-Path $appsettingsPath)) {
    throw "appsettings do mobile nao encontrado: $appsettingsPath"
}

$json = Get-Content -LiteralPath $appsettingsPath -Raw | ConvertFrom-Json
if ($null -eq $json.CineTrackApi) {
    $json | Add-Member -NotePropertyName "CineTrackApi" -NotePropertyValue ([pscustomobject]@{})
}

Set-JsonProperty -Object $json.CineTrackApi -Name "BaseUrl" -Value $normalizedBaseUrl
Set-JsonProperty -Object $json.CineTrackApi -Name "ApiKey" -Value $ApiKey
Set-JsonProperty -Object $json.CineTrackApi -Name "TimeoutSeconds" -Value 30

$json | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appsettingsPath -Encoding UTF8
Write-Host "[OK] TrackList Mobile configurado para API direta em $normalizedBaseUrl" -ForegroundColor Green

if (-not $SkipBuild) {
    Push-Location $WorkspaceRoot
    try {
        dotnet build .\CineTrack.sln
    }
    finally {
        Pop-Location
    }
}
